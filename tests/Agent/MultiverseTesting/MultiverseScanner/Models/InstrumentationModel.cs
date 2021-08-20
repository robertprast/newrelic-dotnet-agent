// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using NewRelic.Agent.MultiverseScanner.ExtensionSerialization;
using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Agent.MultiverseScanner.Models
{
    public class InstrumentationModel
    {
        public List<Match> Matches { get; }

        private List<string> _assemblies;

        public List<string> UniqueAssemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    _assemblies = new List<string>();
                    foreach (var match in Matches)
                    {
                        if (!_assemblies.Contains(match.AssemblyName))
                        {
                            _assemblies.Add(match.AssemblyName);
                        }
                    }
                }
                return _assemblies;
            }
        }

        public InstrumentationModel()
        {
            Matches = new List<Match>();
        }

        public static InstrumentationModel CreateInstrumentationModel(Extension extension)
        {
            var model = new InstrumentationModel();
            model.Matches.AddRange(from tracerFactory in extension.Instrumentation.TracerFactories
                                   from match in tracerFactory.Matches
                                   select match);
            FixGenericTokensInExactMethodMatcher(model);
            return model;
        }

        // This fixes how the XML files (profiler) indicate generics.  The XML uses "!!0" to represent the generic, while the code uses "T".
        // Since the code and therefore Cecil expect "T" we need to replace the token.
        private static void FixGenericTokensInExactMethodMatcher(InstrumentationModel instrumentationModel)
        {
            foreach (var match in instrumentationModel.Matches)
            {
                foreach (var exactMethodMatcher in match.ExactMethodMatchers)
                {
                    if (string.IsNullOrWhiteSpace(exactMethodMatcher?.Parameters))
                    {
                        continue;
                    }

                    exactMethodMatcher.Parameters = exactMethodMatcher.Parameters.Replace("!!0","T");
                }
            }
        }
    }
}
