// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System.Net;
using Xunit;
using NewRelic.Agent.IntegrationTestHelpers.RemoteServiceFixtures;
using System;

namespace NewRelic.Agent.UnboundedIntegrationTests.RemoteServiceFixtures
{
    public class CosmosDBApplicationFixture : RemoteApplicationFixture
    {
        private readonly string _baseUrl;
        private const string CosmosDBApiPath = "/api/CosmosDB";
        private readonly string _deleteDatabaseUrl;

        public CosmosDBApplicationFixture(bool isCore) : base(isCore ? new RemoteService("CosmosDBCoreApplication", "CosmosDBCoreApplication.exe", "net5", ApplicationType.Unbounded, isCoreApp: true, publishApp: true) :
            new RemoteWebApplication("CosmosDBApplication", ApplicationType.Unbounded))
        {
            _baseUrl = $"http://localhost:{Port}";
            _deleteDatabaseUrl = $"{_baseUrl}{CosmosDBApiPath}/DropDatabase?dbName=";
        }

        //TODO: modify this to make sense for CosmosDB (this is from the mongo fixture)
        public void SampleAction()
        {
            var guid = Guid.NewGuid();
            var dbParam = $"?dbName={guid}";
            var address = $"{_baseUrl}{CosmosDBApiPath}/DoSomething/{dbParam}";

            using (var webClient = new WebClient())
            {
                var responseBody = webClient.DownloadString(address);
                var deleteDatabaseResponse = webClient.DownloadString($"{_deleteDatabaseUrl}{guid}");
                Assert.NotNull(responseBody);
            }
        }
    }

    public class CosmosDBFrameworkApplicationFixture : CosmosDBApplicationFixture
    {
        public CosmosDBFrameworkApplicationFixture() : base(false)
        {
        }
    }

    public class CosmosDBCoreApplicationFixture : CosmosDBApplicationFixture
    {
        public CosmosDBCoreApplicationFixture() : base(true)
        {
        }
    }
}
