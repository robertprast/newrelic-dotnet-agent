// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Agent.Configuration;
using NewRelic.Agent.Core.Aggregators;
using NewRelic.Agent.Core.Attributes;
using NewRelic.Agent.Core.Database;
using NewRelic.Agent.Core.DistributedTracing;
using NewRelic.Agent.Core.Errors;
using NewRelic.Agent.Core.Metric;
using NewRelic.Agent.Core.Metrics;
using NewRelic.Agent.Core.Segments;
using NewRelic.Agent.Core.Spans;
using NewRelic.Agent.Core.Transactions;
using NewRelic.Agent.Core.Utilities;
using NewRelic.Agent.Core.WireModels;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using NewRelic.Core.DistributedTracing;
using NewRelic.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static NewRelic.Agent.Core.WireModels.MetricWireModel;

namespace NewRelic.Agent.Core.Transformers
{
    public interface ILoggingMetricsEventTransformer
    {
        void Transform(string logLevel, string logMessage, IDictionary<string, string> linkingMetadata);
    }

    class LoggingMetricsEventTransformer : ILoggingMetricsEventTransformer
    {
        private readonly ILoggingMetricsEventAggregator _loggingMetricsEventAggregator;
        private readonly IMetricNameService _metricNameService;
        private readonly IConfigurationService _configurationService;
        private readonly IAgentTimerService _agentTimerService;
        private readonly IAdaptiveSampler _adaptiveSampler;

        public LoggingMetricsEventTransformer(ILoggingMetricsEventAggregator loggingMetricsEventAggregator, IMetricNameService metricNameService, IConfigurationService configurationService, IAgentTimerService agentTimerService, IAdaptiveSampler adaptiveSampler)
        {
            _loggingMetricsEventAggregator = loggingMetricsEventAggregator;
            _metricNameService = metricNameService;
            _configurationService = configurationService;
            _agentTimerService = agentTimerService;
            _adaptiveSampler = adaptiveSampler;
        }

        public void Transform(string logLevel, string logMessage, IDictionary<string, string> linkingMetadata)
        {
            if (string.IsNullOrWhiteSpace(logLevel) || string.IsNullOrWhiteSpace(logMessage))
            {
                return;
            }

            // make a logging event, build event with metadata eventually
            // Fix timestamp - get from instrumentation!!
            var wiremodel = new LoggingMetricsEventWireModel(DateTime.Now, logLevel, logMessage);

            // call collect
            Log.Debug("LoggingMetricsEventTransformer transformed!");
            // GenerateAndCollect...
        }
    }
}
