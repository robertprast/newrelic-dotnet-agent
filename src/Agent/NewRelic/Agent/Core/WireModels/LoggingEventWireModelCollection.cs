// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Agent.Core.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

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
    public class LoggingEventWireModelCollection
    {
        [JsonProperty("common")]
        public LoggingEventsCommonProperties CommonProperties { get; }

        [JsonProperty("logs")]
        public IList<LoggingEventWireModel> LoggingEvents { get; }

        public LoggingEventWireModelCollection(string logtype, string service, string hostname, IList<LoggingEventWireModel> loggingEvents)
        {
            CommonProperties = new LoggingEventsCommonProperties(logtype, service, hostname);
            LoggingEvents = loggingEvents;
        }
    }

    [JsonConverter(typeof(JsonArrayConverter))]
    public class LoggingEventsCommonProperties
    {
        [JsonProperty("attributes")]
        public LoggingEventsCommonAttributes Attributes { get; }

        public LoggingEventsCommonProperties(string logtype, string service, string hostname)
        {
            Attributes = new LoggingEventsCommonAttributes(logtype, service, hostname);
        }
    }

    [JsonConverter(typeof(JsonArrayConverter))]
    public class LoggingEventsCommonAttributes
    {
        [JsonProperty("logtype")]
        public string Logtype { get; }

        [JsonProperty("service")]
        public string Service { get; }

        [JsonProperty("hostname")]
        public string Hostname { get; }

        public LoggingEventsCommonAttributes(string logtype, string service, string hostname)
        {
            Logtype = logtype;
            Service = service;
            Hostname = hostname;
        }
    }
}
