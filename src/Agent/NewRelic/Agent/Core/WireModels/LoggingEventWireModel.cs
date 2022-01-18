// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace NewRelic.Agent.Core.WireModels
{
    public class LoggingEventWireModel

    {
        /// <summary>
        /// The UTC timestamp in unix milliseconds. 
        /// </summary>
        public long TimeStamp { get; }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The log level.
        /// </summary>
        public string Level { get; }

        /// <summary>
        /// The span id of the segment.
        /// </summary>
        public string SpanId { get; }

        /// <summary>
        /// The traced if of the transaction.
        /// </summary>
        public string TraceId { get; }

        public LoggingEventWireModel(long unixTimestampMS, string message, string level, string spanId, string traceId)
        {
            TimeStamp = unixTimestampMS;
            Message = message;
            Level = level;
            SpanId = spanId;
            TraceId = traceId;
        }
    }
}
