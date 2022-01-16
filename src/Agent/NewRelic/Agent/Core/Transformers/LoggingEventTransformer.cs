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
using NewRelic.Core;
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
        void Transform(DateTime timestamp, string logLevel, string logMessage, IDictionary<string, string> linkingMetadata);
    }

    class LoggingEventTransformer : ILoggingMetricsEventTransformer
    {
        private readonly ILoggingEventAggregator _loggingEventAggregator;
        private readonly IMetricNameService _metricNameService;
        private readonly IConfigurationService _configurationService;
        private readonly IAgentTimerService _agentTimerService;
        private readonly IAdaptiveSampler _adaptiveSampler;

        public LoggingEventTransformer(ILoggingEventAggregator loggingEventAggregator, IMetricNameService metricNameService, IConfigurationService configurationService, IAgentTimerService agentTimerService, IAdaptiveSampler adaptiveSampler)
        {
            _loggingEventAggregator = loggingEventAggregator;
            _metricNameService = metricNameService;
            _configurationService = configurationService;
            _agentTimerService = agentTimerService;
            _adaptiveSampler = adaptiveSampler;
        }

        public void Transform(DateTime timestamp, string logLevel, string logMessage, IDictionary<string, string> linkingMetadata)
        {
            if (string.IsNullOrWhiteSpace(logLevel) || string.IsNullOrWhiteSpace(logMessage))
            {
                return;
            }

            // make a logging event, build event with metadata eventually
            var wiremodel = new LoggingEventWireModel(timestamp.ToUnixTimeMilliseconds(), logMessage, logLevel, linkingMetadata);

            _loggingEventAggregator.Collect(wiremodel);
        }
    }
}
