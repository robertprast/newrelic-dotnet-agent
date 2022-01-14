// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace NewRelic.Agent.Core.WireModels
{
    public class LoggingEventWireModel

    {
        /// <summary>
        /// The UTC timestamp indicating when the message was logged. 
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Metadata from GetLinkingMetadata and instrumentation.
        /// </summary>
        public IDictionary<string, string> Attributes { get; }

        public LoggingEventWireModel(DateTime timestamp, string message, IDictionary<string, string> attributes)
        {
            TimeStamp = timestamp;
            Message = message;
            Attributes = attributes;
        }
    }
}
