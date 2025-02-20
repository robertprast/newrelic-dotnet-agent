﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore5BasicWebApiApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DefaultController : ControllerBase
    {
        public DefaultController()
        {
        }

        public async Task<string> MakeExternalCallUsingHttpClient(string baseAddress, string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetStringAsync(path);

                if (!string.IsNullOrEmpty(response))
                {
                    return "Worked";
                }

                return "Error";
            }
        }

        public string AwesomeName()
        {
            return "Chuck Norris";
        }
    }
}
