// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Agent.Core.Aggregators;
using NewRelic.Agent.Core.WireModels;
using NewRelic.Core;

namespace NewRelic.Agent.Core.Transformers
{
    public interface ILoggingMetricsEventTransformer
    {
        void Transform(DateTime timestamp, string logLevel, string logMessage, string spanId, string traceId);
    }

    class LoggingEventTransformer : ILoggingMetricsEventTransformer
    {
        private readonly ILoggingEventAggregator _loggingEventAggregator;

        public LoggingEventTransformer(ILoggingEventAggregator loggingEventAggregator)
        {
            _loggingEventAggregator = loggingEventAggregator;
        }

        public void Transform(DateTime timestamp, string logLevel, string logMessage, string spanId, string traceId)
        {
            if (string.IsNullOrWhiteSpace(logLevel) || string.IsNullOrWhiteSpace(logMessage))
            {
                return;
            }

            // make a logging event, build event with metadata eventually
            var wiremodel = new LoggingEventWireModel(timestamp.ToUnixTimeMilliseconds(), logMessage, logLevel, spanId, traceId);

            _loggingEventAggregator.Collect(wiremodel);
        }
    }
}
