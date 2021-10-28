// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using NewRelic.Agent.IntegrationTestHelpers;
using NewRelic.Agent.IntegrationTests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace NewRelic.Agent.UnboundedIntegrationTests.MongoDB
{
    public abstract class CosmosDBTests<T> : NewRelicIntegrationTest<T>
        where T : RemoteServiceFixtures.CosmosDBApplicationFixture
    {
        private readonly RemoteServiceFixtures.CosmosDBApplicationFixture _fixture;

        public CosmosDBTests(T fixture, ITestOutputHelper output)  : base(fixture)
        {
            _fixture = fixture;
            _fixture.TestLogger = output;
            _fixture.Actions
            (
                exerciseApplication: () =>
                {
                    _fixture.SampleAction();
                }
            );

            _fixture.Initialize();
        }

        // TODO: modify to make sense for CosmosDB
        [Fact]
        public void CheckForDatastoreInstanceMetrics()
        {
            var serverHost = CommonUtils.NormalizeHostname(MongoDbConfiguration.MongoDb26Server);
            var m = _fixture.AgentLog.GetMetricByName($"Datastore/instance/MongoDB/{serverHost}/{MongoDbConfiguration.MongoDb26Port}");
            Assert.NotNull(m);
        }

        // TODO: modify to make sense for CosmosDB
        [Fact]
        public void CreateCollection()
        {
            var m = _fixture.AgentLog.GetMetricByName("Datastore/statement/MongoDB/createTestCollection/CreateCollection");

            Assert.NotNull(m);
        }
    }

    [NetFrameworkTest]
    public class CosmosDBFrameworkTests : CosmosDBTests<RemoteServiceFixtures.CosmosDBFrameworkApplicationFixture>
    {
        public CosmosDBFrameworkTests(RemoteServiceFixtures.CosmosDBFrameworkApplicationFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }
    }

    [NetCoreTest]
    public class CosmosDBCoreTests : CosmosDBTests<RemoteServiceFixtures.CosmosDBCoreApplicationFixture>
    {
        public CosmosDBCoreTests(RemoteServiceFixtures.CosmosDBCoreApplicationFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }
    }
}
