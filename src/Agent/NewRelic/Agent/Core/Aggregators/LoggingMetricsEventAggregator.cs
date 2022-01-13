// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Agent.Core.AgentHealth;
using NewRelic.Agent.Core.DataTransport;
using NewRelic.Agent.Core.Events;
using NewRelic.Agent.Core.Time;
using NewRelic.Agent.Core.WireModels;
using NewRelic.Collections;
using NewRelic.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NewRelic.Agent.Core.Aggregators
{
    // LoggingMetrics

    public interface ILoggingMetricsEventAggregator
    {
        void Collect(LoggingMetricsEventWireModel loggingMetricsEventWireModel);
    }

    /// <summary>
    /// An service for collecting and managing logging metrics events.
    /// </summary>
    public class LoggingMetricsEventAggregator : AbstractAggregator<LoggingMetricsEventWireModel>, ILoggingMetricsEventAggregator
    {
        private readonly IAgentHealthReporter _agentHealthReporter;
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        private uint _loggingMetricsEvenCollectionMaximum;
        private ICollection<LoggingMetricsEventWireModel> _errorTraceWireModels = new ConcurrentList<LoggingMetricsEventWireModel>();

        public LoggingMetricsEventAggregator(IDataTransportService dataTransportService, IScheduler scheduler, IProcessStatic processStatic, IAgentHealthReporter agentHealthReporter)
            : base(dataTransportService, scheduler, processStatic)
        {
            _agentHealthReporter = agentHealthReporter;
            GetAndResetCollection();
        }

        protected override TimeSpan HarvestCycle => TimeSpan.FromSeconds(5);

        public override void Dispose()
        {
            base.Dispose();
            _readerWriterLock.Dispose();
        }

        protected override bool IsEnabled => _configuration.LoggingMetricsEventCollectorEnabled;

        public override void Collect(LoggingMetricsEventWireModel loggingMetricsEventWireModel)
        {
            _agentHealthReporter.ReportErrorTraceCollected();

            _readerWriterLock.EnterReadLock();
            try
            {
                AddToCollection(loggingMetricsEventWireModel);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        protected override void Harvest()
        {
            ICollection<LoggingMetricsEventWireModel> loggingMetricsEventWireModels;

            _readerWriterLock.EnterWriteLock();
            try
            {
                loggingMetricsEventWireModels = GetAndResetCollection();
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            if (loggingMetricsEventWireModels.Count <= 0)
                return;

            var responseStatus = DataTransportService.Send(loggingMetricsEventWireModels);

            HandleResponse(responseStatus, loggingMetricsEventWireModels);
        }

        protected override void OnConfigurationUpdated(ConfigurationUpdateSource configurationUpdateSource)
        {
            // It is *CRITICAL* that this method never do anything more complicated than clearing data and starting and ending subscriptions.
            // If this method ends up trying to send data synchronously (even indirectly via the EventBus or RequestBus) then the user's application will deadlock (!!!).

            GetAndResetCollection();
        }

        private ICollection<LoggingMetricsEventWireModel> GetAndResetCollection()
        {
            _loggingMetricsEvenCollectionMaximum = _configuration.ErrorsMaximumPerPeriod;
            return Interlocked.Exchange(ref _errorTraceWireModels, new ConcurrentList<LoggingMetricsEventWireModel>());
        }

        private void AddToCollection(LoggingMetricsEventWireModel loggingMetricsEventWireModel)
        {
            if (_errorTraceWireModels.Count >= _loggingMetricsEvenCollectionMaximum)
                return;

            _errorTraceWireModels.Add(loggingMetricsEventWireModel);
        }

        private void Retain(IEnumerable<LoggingMetricsEventWireModel> loggingMetricsEventWireModels)
        {
            loggingMetricsEventWireModels = loggingMetricsEventWireModels.ToList();
            _agentHealthReporter.ReportErrorTracesRecollected(loggingMetricsEventWireModels.Count());

            // It is possible, but unlikely, to lose incoming log events here due to a race condition
            var savedLoggingMetricsEventWireModels = GetAndResetCollection();

            // It is possible that newer, incoming log events will be added to our collection before we add the retained and saved ones.
            foreach (var model in loggingMetricsEventWireModels)
            {
                if (model != null)
                {
                    AddToCollection(model);
                }
            }

            foreach (var model in savedLoggingMetricsEventWireModels)
            {
                if (model != null)
                {
                    AddToCollection(model);
                }
            }
        }

        private void HandleResponse(DataTransportResponseStatus responseStatus, ICollection<LoggingMetricsEventWireModel> loggingMetricsEventWireModels)
        {
            switch (responseStatus)
            {
                case DataTransportResponseStatus.RequestSuccessful:
                    //_agentHealthReporter.ReportLoggingMetricsEventsSent(loggingMetricsEventWireModels.Count);
                    break;
                case DataTransportResponseStatus.Retain:
                    Retain(loggingMetricsEventWireModels);
                    break;
                case DataTransportResponseStatus.ReduceSizeIfPossibleOtherwiseDiscard:
                case DataTransportResponseStatus.Discard:
                default:
                    break;
            }
        }
    }
}
