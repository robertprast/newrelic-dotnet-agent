// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System;
using Mono.Cecil;
using NewRelic.Agent.MultiverseScanner.Models;

namespace NewRelic.Agent.MultiverseScanner
{
    // https://github.com/jbevain/cecil/wiki/HOWTO

    public class AssemblyAnalyzer
    {
        public AssemblyAnalysis RunAssemblyAnalysis(string filePath, string nugetDataDirectory)
        {
            var assemblyModel = GetAssemblyModel(filePath, nugetDataDirectory);
            var assemblyAnalysis = new AssemblyAnalysis(assemblyModel);
            return assemblyAnalysis;
        }

        public AssemblyModel GetAssemblyModel(string filePath, string nugetDataDirectory)
        {
            try
            {
                var moduleDefinition = ModuleDefinition.ReadModule(filePath);
                var assemblyModel = AssemblyModel.GetAssemblyModel(moduleDefinition, nugetDataDirectory);
                return assemblyModel;
            }
            catch(System.BadImageFormatException badImageFormatException)
            {
                Console.WriteLine($"Warning: Mono.Cecil could not read the assembly!");
                Console.WriteLine(badImageFormatException.Message);
                Console.WriteLine();
                return AssemblyModel.EmptyAssemblyModel;
            }
        }
    }
}
