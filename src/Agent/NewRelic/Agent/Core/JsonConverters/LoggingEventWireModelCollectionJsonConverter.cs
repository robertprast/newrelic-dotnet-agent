// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System;
using NewRelic.Agent.Core.WireModels;
using NewRelic.Core;
using Newtonsoft.Json;

namespace NewRelic.Agent.Core.JsonConverters
{
    public class LoggingEventWireModelCollectionJsonConverter : JsonConverter<LoggingEventWireModelCollection>
    {
        public override LoggingEventWireModelCollection ReadJson(JsonReader reader, Type objectType, LoggingEventWireModelCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter jsonWriter, LoggingEventWireModelCollection value, JsonSerializer serializer)
        {
            WriteJsonImpl(jsonWriter, value, serializer);
        }

        public static void WriteJsonImpl(JsonWriter jsonWriter, LoggingEventWireModelCollection value, JsonSerializer serializer)
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("common");
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("attributes");
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("entity.name");
            jsonWriter.WriteValue(value.CommonProperties.Attributes.EntityName);
            jsonWriter.WritePropertyName("entity.type");
            jsonWriter.WriteValue(value.CommonProperties.Attributes.EntityType);
            jsonWriter.WritePropertyName("entity.guid");
            jsonWriter.WriteValue(value.CommonProperties.Attributes.EntityGuid);
            jsonWriter.WritePropertyName("hostname");
            jsonWriter.WriteValue(value.CommonProperties.Attributes.Hostname);
            jsonWriter.WritePropertyName("plugin.type");
            jsonWriter.WriteValue(value.CommonProperties.Attributes.PluginType);
            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

            jsonWriter.WritePropertyName("logs");
            jsonWriter.WriteStartArray();
            for (int i = 0; i < value.LoggingEvents.Count; i++)
            {
                var logEvent = value.LoggingEvents[i];
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("timestamp");
                jsonWriter.WriteValue(logEvent.TimeStamp);
                jsonWriter.WritePropertyName("message");
                jsonWriter.WriteValue(logEvent.Message);
                jsonWriter.WritePropertyName("level");
                jsonWriter.WriteValue(logEvent.Level);
                jsonWriter.WritePropertyName("attributes");
                jsonWriter.WriteStartObject();
                if (logEvent.Attributes.Count > 0)
                {
                    // using foreach here since it is fast enough and simpler than other methods
                    foreach (var pair in logEvent.Attributes)
                    {
                        jsonWriter.WritePropertyName(pair.Key);
                        jsonWriter.WriteValue(pair.Value);
                    }
                }

                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }
    }
}
