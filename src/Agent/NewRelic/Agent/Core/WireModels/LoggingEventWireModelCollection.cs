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

        public LoggingEventWireModelCollection(string entityName, string entityType, string entityGuid, string hostname, string pluginType, IList<LoggingEventWireModel> loggingEvents)
        {
            CommonProperties = new LoggingEventsCommonProperties(entityName, entityType, entityGuid, hostname, pluginType);
            LoggingEvents = loggingEvents;
        }
    }

    public class LoggingEventsCommonProperties
    {
        public LoggingEventsCommonAttributes Attributes { get; }

        public LoggingEventsCommonProperties(string entityName, string entityType, string entityGuid, string hostname, string pluginType)
        {
            Attributes = new LoggingEventsCommonAttributes(entityName, entityType, entityGuid, hostname, pluginType);
        }
    }

    public class LoggingEventsCommonAttributes
    {
        public string EntityName { get; }

        public string EntityType { get; }

        public string EntityGuid { get; }

        public string Hostname { get; }

        public string PluginType { get; }

        public LoggingEventsCommonAttributes(string entityName, string entityType, string entityGuid, string hostname, string pluginType)
        {
            EntityName = entityName;
            EntityType = entityType;
            EntityType = entityGuid;
            Hostname = hostname;
            PluginType = pluginType;
        }
    }
}
