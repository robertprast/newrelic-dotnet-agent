// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Agent.Configuration;
using NewRelic.Agent.Core.AgentHealth;
using NewRelic.Agent.Core.DataTransport;
using NewRelic.Agent.Core.Events;
using NewRelic.Agent.Core.Time;
using NewRelic.Agent.Core.WireModels;
using NewRelic.SystemInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NewRelic.Agent.Core.Aggregators
{
    public interface ILoggingEventAggregator
    {
        void Collect(LoggingEventWireModel loggingEventWireModel);
    }

    /// <summary>
    /// An service for collecting and managing logging events.
    /// </summary>
    public class LoggingEventAggregator : AbstractAggregator<LoggingEventWireModel>, ILoggingEventAggregator
    {
        private const String EntityType = "SERVICE";
        private const String PluginType = "nr-dotnet-agent";

        private readonly IAgentHealthReporter _agentHealthReporter;
        private readonly IConfigurationService _configurationService;

        private uint _loggingEventCollectionMaximum;
        private ConcurrentBag<LoggingEventWireModel> _loggingEventWireModels = new ConcurrentBag<LoggingEventWireModel>();

        public LoggingEventAggregator(IDataTransportService dataTransportService, IScheduler scheduler, IProcessStatic processStatic, IAgentHealthReporter agentHealthReporter,
            IConfigurationService configurationService)
            : base(dataTransportService, scheduler, processStatic)
        {
            _agentHealthReporter = agentHealthReporter;
            _configurationService = configurationService;
            GetAndResetCollection();
        }

        protected override TimeSpan HarvestCycle => TimeSpan.FromSeconds(5);

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override bool IsEnabled => _configuration.LoggingEventCollectorEnabled;

        public override void Collect(LoggingEventWireModel loggingEventWireModel)
        {
            _agentHealthReporter.ReportLoggingEventCollected();
            AddToCollection(loggingEventWireModel);
        }

        protected override void Harvest()
        {
            ConcurrentBag<LoggingEventWireModel> loggingEventWireModels;
            loggingEventWireModels = GetAndResetCollection();
            if (loggingEventWireModels.Count <= 0)
            {
                return;
            }

            // matches metadata so that utilization and this match
            var hostname = !string.IsNullOrEmpty(_configurationService.Configuration.UtilizationFullHostName)
                ? _configurationService.Configuration.UtilizationFullHostName
                : _configurationService.Configuration.UtilizationHostName;

            var loggingEvents = loggingEventWireModels.ToArray();
            var modelsCollection = new LoggingEventWireModelCollection(
                _configurationService.Configuration.ApplicationNames.ElementAt(0),
                EntityType,
                _configurationService.Configuration.EntityGuid,
                hostname,
                PluginType,
                loggingEvents); ;

            var responseStatus = DataTransportService.Send(modelsCollection);

            HandleResponse(responseStatus, loggingEventWireModels);
        }

        protected override void OnConfigurationUpdated(ConfigurationUpdateSource configurationUpdateSource)
        {
            // It is *CRITICAL* that this method never do anything more complicated than clearing data and starting and ending subscriptions.
            // If this method ends up trying to send data synchronously (even indirectly via the EventBus or RequestBus) then the user's application will deadlock (!!!).

            GetAndResetCollection();
        }

        private ConcurrentBag<LoggingEventWireModel> GetAndResetCollection()
        {
            // this assumes that we want to collect the full amount each 5 second harvest cycle.
            // If instead we want to call that for a 60 second internal with 5 second harvest, we will need to divide the value.
            _loggingEventCollectionMaximum = _configuration.LoggingEventsMaximumPerPeriod;
            return Interlocked.Exchange(ref _loggingEventWireModels, new ConcurrentBag<LoggingEventWireModel>());
        }

        private void AddToCollection(LoggingEventWireModel loggingEventWireModel)
        {
            if (_loggingEventWireModels.Count >= _loggingEventCollectionMaximum)
            {
                return;
            }

            _loggingEventWireModels.Add(loggingEventWireModel);
        }

        private void Retain(IEnumerable<LoggingEventWireModel> loggingEventWireModels)
        {
            _agentHealthReporter.ReportLoggingEventsRecollected(loggingEventWireModels.Count());

            // It is possible, but unlikely, to lose incoming log events here due to a race condition
            var savedloggingEventWireModels = GetAndResetCollection();

            // It is possible that newer, incoming log events will be added to our collection before we add the retained and saved ones.
            foreach (var model in loggingEventWireModels)
            {
                if (model != null)
                {
                    AddToCollection(model);
                }
            }

            foreach (var model in savedloggingEventWireModels)
            {
                if (model != null)
                {
                    AddToCollection(model);
                }
            }
        }

        private void HandleResponse(DataTransportResponseStatus responseStatus, ConcurrentBag<LoggingEventWireModel> loggingEventWireModels)
        {
            switch (responseStatus)
            {
                case DataTransportResponseStatus.RequestSuccessful:
                    _agentHealthReporter.ReportLoggingEventsSent(loggingEventWireModels.Count);
                    break;
                case DataTransportResponseStatus.Retain:
                    Retain(loggingEventWireModels);
                    break;
                case DataTransportResponseStatus.ReduceSizeIfPossibleOtherwiseDiscard:
                case DataTransportResponseStatus.Discard:
                default:
                    break;
            }
        }
    }
}
