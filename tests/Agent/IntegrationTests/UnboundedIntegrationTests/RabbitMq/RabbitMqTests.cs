// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System.Collections.Generic;
using System.Linq;
using NewRelic.Agent.IntegrationTestHelpers;
using NewRelic.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;

namespace NewRelic.Agent.UnboundedIntegrationTests.RabbitMq
{
    [NetFrameworkTest]
    public abstract class RabbitMqTestsBase<TFixture> : NewRelicIntegrationTest<TFixture>
        where TFixture : RemoteServiceFixtures.RabbitMqBasicMvcFixture
    {
        private readonly RemoteServiceFixtures.RabbitMqBasicMvcFixture _fixture;

        private string _sendReceiveQueue;
        private string _purgeQueue;

        // Scope properties
        public string ScopeSendReceive { get; set; } = "WebTransaction/MVC/RabbitMQController/RabbitMQ_SendReceive";
        public string ScopeQueuePurge { get; set; } = "WebTransaction/MVC/RabbitMQController/RabbitMQ_QueuePurge";
        public string ScopeSendReceiveTempQueue { get; set; } = "WebTransaction/MVC/RabbitMQController/RabbitMQ_SendReceiveTempQueue";
        public string ScopeSendReceiveTopic { get; set; } = "WebTransaction/MVC/RabbitMQController/RabbitMQ_SendReceiveTopic";

        protected RabbitMqTestsBase(TFixture fixture, ITestOutputHelper output)  : base(fixture)
        {
            _fixture = fixture;
            _fixture.TestLogger = output;
            _fixture.Actions
            (
                setupConfiguration: () =>
                {
                    var configModifier = new NewRelicConfigModifier(fixture.DestinationNewRelicConfigFilePath);
                    configModifier.ForceTransactionTraces();
                },
                exerciseApplication: () =>
                {
                    _sendReceiveQueue = _fixture.GetMessageQueue_RabbitMQ_SendReceive("Test Message");
                    _fixture.GetMessageQueue_RabbitMQ_SendReceiveTempQueue("Test Message");
                    _purgeQueue = _fixture.GetMessageQueue_RabbitMQ_Purge();
                    _fixture.GetMessageQueue_RabbitMQ_SendReceiveTopic("SendReceiveTopic.Topic", "Test Message");
                }
            );
            _fixture.Initialize();
        }

        [Fact]
        public void Test()
        {
            var metrics = _fixture.AgentLog.GetMetrics().ToList();

            var expectedMetrics = new List<Assertions.ExpectedMetric>
            {
                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Produce/Named/{_sendReceiveQueue}", callCount = 1},
                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Produce/Named/{_sendReceiveQueue}", callCount = 1, metricScope = ScopeSendReceive},

                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Consume/Named/{_sendReceiveQueue}", callCount = 1},
                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Consume/Named/{_sendReceiveQueue}", callCount = 1, metricScope = ScopeSendReceive},

                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Produce/Named/{_purgeQueue}", callCount = 1 },
                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Produce/Named/{_purgeQueue}", callCount = 1, metricScope = ScopeQueuePurge },

                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Purge/Named/{_purgeQueue}", callCount = 1 },
                new Assertions.ExpectedMetric { metricName = $"MessageBroker/RabbitMQ/Queue/Purge/Named/{_purgeQueue}", callCount = 1, metricScope = ScopeQueuePurge },

                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Queue/Produce/Temp", callCount = 1 },
                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Queue/Produce/Temp", callCount = 1, metricScope = ScopeSendReceiveTempQueue},

                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Topic/Produce/Named/SendReceiveTopic.Topic", callCount = 1 },
                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Topic/Produce/Named/SendReceiveTopic.Topic", callCount = 1, metricScope = ScopeSendReceiveTopic },

                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Queue/Consume/Temp", callCount = 2 },
                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Queue/Consume/Temp", callCount = 1, metricScope = ScopeSendReceiveTempQueue},
                new Assertions.ExpectedMetric { metricName = @"MessageBroker/RabbitMQ/Queue/Consume/Temp", callCount = 1, metricScope = ScopeSendReceiveTopic },
            };

            var sendReceiveTransactionEvent = _fixture.AgentLog.TryGetTransactionEvent(ScopeSendReceive);
            var sendReceiveTempQueueTransactionEvent = _fixture.AgentLog.TryGetTransactionEvent(ScopeSendReceiveTempQueue);
            var queuePurgeTransactionEvent = _fixture.AgentLog.TryGetTransactionEvent(ScopeQueuePurge);
            var sendReceiveTopicTransactionEvent = _fixture.AgentLog.TryGetTransactionEvent(ScopeSendReceiveTopic);

            var expectedTransactionTraceSegments = new List<string>
            {
                $"MessageBroker/RabbitMQ/Queue/Consume/Named/{_sendReceiveQueue}"
            };

            var transactionSample = _fixture.AgentLog.TryGetTransactionSample(ScopeSendReceive);


            Assertions.MetricsExist(expectedMetrics, metrics);

            NrAssert.Multiple(
                () => Assert.NotNull(sendReceiveTransactionEvent),
                () => Assert.NotNull(sendReceiveTempQueueTransactionEvent),
                () => Assert.NotNull(queuePurgeTransactionEvent),
                () => Assert.NotNull(sendReceiveTopicTransactionEvent),
                () => Assert.NotNull(transactionSample),
                () => Assertions.TransactionTraceSegmentsExist(expectedTransactionTraceSegments, transactionSample)
            );

        }
    }

    public class RabbitMqTests : RabbitMqTestsBase<RemoteServiceFixtures.RabbitMqBasicMvcFixture>
    {
        public RabbitMqTests(RemoteServiceFixtures.RabbitMqBasicMvcFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }
    }

    public class RabbitMqLegacyTests : RabbitMqTestsBase<RemoteServiceFixtures.RabbitMqLegacyBasicMvcFixture>
    {
        public RabbitMqLegacyTests(RemoteServiceFixtures.RabbitMqLegacyBasicMvcFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }
    }

    public class RabbitMqCoreTests : RabbitMqTestsBase<RemoteServiceFixtures.RabbitMqCoreBasicMvcFixture>
    {
        public RabbitMqCoreTests(RemoteServiceFixtures.RabbitMqCoreBasicMvcFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            base.ScopeSendReceive = "WebTransaction/MVC/RabbitMQ/RabbitMQ_SendReceive/{queueName}/{message}";
            base.ScopeQueuePurge = "WebTransaction/MVC/RabbitMQ/RabbitMQ_QueuePurge/{queueName}";
            base.ScopeSendReceiveTempQueue = "WebTransaction/MVC/RabbitMQ/RabbitMQ_SendReceiveTempQueue/{message}";
            base.ScopeSendReceiveTopic = "WebTransaction/MVC/RabbitMQ/RabbitMQ_SendReceiveTopic/{exchangeName}/{topicName}/{message}";
            
        }
    }

}
