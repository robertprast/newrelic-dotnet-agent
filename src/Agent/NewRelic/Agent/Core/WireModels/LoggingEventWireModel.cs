// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Agent.Core.JsonConverters;
using Newtonsoft.Json;

namespace NewRelic.Agent.Core.WireModels
{
    /*
[{
   "common": {
     "attributes": {
       "logtype": "accesslogs",
       "service": "login-service",
       "hostname": "login.example.com"
     }
   },
   "logs": [{
       "timestamp": <TIMESTAMP_IN_UNIX_EPOCH>,
       "message": "User 'xyz' logged in"
     },{
       "timestamp": <TIMESTAMP_IN_UNIX_EPOCH>,
       "message": "User 'xyz' logged out",
       "attributes": {
         "auditId": 123
       }
     }]
}]
     */

    [JsonConverter(typeof(JsonArrayConverter))]
    public class LoggingEventWireModel

    {
        /// <summary>
        /// The UTC timestamp indicating when the message was logged. 
        /// </summary>
        [JsonProperty("timestamp")]
        [DateTimeSerializesAsUnixTimeMilliseconds]
        public DateTime TimeStamp { get; }

        /// <summary>
        /// The log message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }

        /// <summary>
        /// Metadata from GetLinkingMetadata and instrumentation.
        /// </summary>
        [JsonProperty("attributes")]
        public IDictionary<string, string> Attributes { get; }

        public LoggingEventWireModel(DateTime timestamp, string message, IDictionary<string, string> attributes)
        {
            TimeStamp = timestamp;
            Message = message;
            Attributes = attributes;
        }
    }
}
