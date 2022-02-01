// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using NewRelic.Agent.Api;
using NewRelic.Agent.Api.Experimental;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using NewRelic.Reflection;

namespace NewRelic.Providers.Wrapper.Logging
{
    public class Log4netWrapper : IWrapper
    {
        private static Func<object, DateTime> _getTimestamp;
        private static Func<object, object> _getLogLevel;
        private static Func<object, string> _getRenderedMessage;
        private static Func<object, IDictionary> _getProperties;

        public bool IsTransactionRequired => false;

        private const string WrapperName = "log4net";

        public CanWrapResponse CanWrap(InstrumentedMethodInfo methodInfo)
        {
            return new CanWrapResponse(WrapperName.Equals(methodInfo.RequestedWrapperName));
        }

        public AfterWrappedMethodDelegate BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall, IAgent agent, ITransaction transaction)
        {
            var loggingEvent = instrumentedMethodCall.MethodCall.MethodArguments[0];

            var getLogLvelFunc = _getLogLevel ??= VisibilityBypasser.Instance.GeneratePropertyAccessor<object>(loggingEvent.GetType(), "Level");
            var logLevel = getLogLvelFunc(loggingEvent).ToString(); // Level class has a ToString override we can use.

            var xapi = agent.GetExperimentalApi();
            xapi.IncrementLogLinesCount(logLevel);

            if (!agent.CurrentTransaction.IsValid)
            {
                return Delegates.NoOp;
            }

            // RenderedMessage is get only
            var getRenderedMessageFunc = _getRenderedMessage ??= VisibilityBypasser.Instance.GeneratePropertyAccessor<string>(loggingEvent.GetType(), "RenderedMessage");
            var renderedMessage = getRenderedMessageFunc(loggingEvent);

            if (string.IsNullOrWhiteSpace(renderedMessage))
            {
                return Delegates.NoOp;
            }

            // We can either get this in Local or UTC
            var getTimestampFunc = _getTimestamp ??= VisibilityBypasser.Instance.GeneratePropertyAccessor<DateTime>(loggingEvent.GetType(), "TimeStampUtc");
            var timestamp = getTimestampFunc(loggingEvent);

            ((ITransactionExperimental)agent.CurrentTransaction).RecordLogMessage(timestamp, logLevel, renderedMessage, agent.TraceMetadata.SpanId, agent.TraceMetadata.TraceId);

            if (!agent.Configuration.LogDecoratorEnabled)
            {
                return Delegates.NoOp;
            }

            var getProperties = _getProperties ??= VisibilityBypasser.Instance.GeneratePropertyAccessor<IDictionary>(loggingEvent.GetType(), "Properties");
            var props = getProperties(loggingEvent);
            props["nr.spanid"] = agent.TraceMetadata.SpanId ?? string.Empty;
            props["nr.traceid"] = agent.TraceMetadata.TraceId ?? string.Empty;

            return Delegates.NoOp;
        }
    }
}
