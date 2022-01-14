// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Agent.Core.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NewRelic.Agent.Core.WireModels
{
    [JsonConverter(typeof(LoggingEventWireModelCollectionJsonConverter))]
    public class LoggingEventWireModelCollection
    {
        public LoggingEventsCommonProperties CommonProperties { get; }

        public IList<LoggingEventWireModel> LoggingEvents { get; }

        public LoggingEventWireModelCollection(string logtype, string service, string hostname, IList<LoggingEventWireModel> loggingEvents)
        {
            CommonProperties = new LoggingEventsCommonProperties(logtype, service, hostname);
            LoggingEvents = loggingEvents;
        }
    }

    public class LoggingEventsCommonProperties
    {
        public LoggingEventsCommonAttributes Attributes { get; }

        public LoggingEventsCommonProperties(string logtype, string service, string hostname)
        {
            Attributes = new LoggingEventsCommonAttributes(logtype, service, hostname);
        }
    }

    public class LoggingEventsCommonAttributes
    {
        public string Logtype { get; }

        public string Service { get; }

        public string Hostname { get; }

        public LoggingEventsCommonAttributes(string logtype, string service, string hostname)
        {
            Logtype = logtype;
            Service = service;
            Hostname = hostname;
        }
    }
}
