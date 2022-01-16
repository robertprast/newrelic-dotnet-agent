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
        /// Metadata from GetLinkingMetadata and instrumentation.
        /// </summary>
        public IDictionary<string, string> Attributes { get; }

        public LoggingEventWireModel(long unixTimestampMS, string message, string level, IDictionary<string, string> attributes)
        {
            TimeStamp = unixTimestampMS;
            Message = message;
            Attributes = attributes;
        }
    }
}
