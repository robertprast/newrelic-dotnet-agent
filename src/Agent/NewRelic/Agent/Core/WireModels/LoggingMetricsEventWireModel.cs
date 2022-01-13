// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NewRelic.Agent.Core.Attributes;
using NewRelic.Agent.Core.JsonConverters;
using NewRelic.Collections;
using Newtonsoft.Json;

namespace NewRelic.Agent.Core.WireModels
{
    // {"timestamp":1642022903048,
    // "thread.name":"1",
    // "message":"Starting TestMethod",
    // "log.level":"INFO",
    // "logger.name":"NewRelic.LogEnrichers.Log4Net.Examples.Program",
    // "Message.Properties.StandardLogFileName":".\\StandardLogs\\Log4NetExample.log",
    // "Message.Properties.log4net:UserName":"AzureAD\\JacobAffinito",
    // "Message.Properties.log4net:Identity":"",
    // "Message.Properties.NewRelicLogFileName":".\\NewRelicLogs\\Log4NetExample.json",
    // "Message.Properties.log4net:HostName":"NRFP104H2"}

    [JsonConverter(typeof(JsonArrayConverter))]
    public class LoggingMetricsEventWireModel
    {
        /// <summary>
        /// The UTC timestamp indicating when the message was logged. 
        /// </summary>
        [JsonProperty("timestamp")]
        [DateTimeSerializesAsUnixTimeMilliseconds]
        public virtual DateTime TimeStamp { get; }

        /// <summary>
        /// The log message.
        /// </summary>
        [JsonProperty("message")]
        public virtual string Message { get; }

        /// <summary>
        /// The log level name of the message.
        /// </summary>
        [JsonProperty("log.level")]
        public virtual string LogLevel { get; }

        public LoggingMetricsEventWireModel(DateTime timestamp, string message, string logLevel)
        {
            TimeStamp = timestamp;
            Message = message;
            LogLevel = logLevel;
        }
    }
}
