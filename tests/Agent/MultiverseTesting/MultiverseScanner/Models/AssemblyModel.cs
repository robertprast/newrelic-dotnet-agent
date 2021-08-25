// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace NewRelic.Agent.MultiverseScanner.Models
{
    public class AssemblyModel
    {
        private const string SystemLowercase = @"^System.[a-z\d]+";
        private const string System1NestLowercase = @"^System.[A-Z][A-Za-z\d]+.[a-z\d]+";
        private const string System2NestLowercase = @"^System.[A-Z][A-Za-z\d]+.[A-Z][A-Za-z\d]+.[a-z\d]+";
        private const string System3NestLowercase = @"^System.[A-Z][A-Za-z\d]+.[A-Z][A-Za-z\d]+.[A-Z][A-Za-z\d]+.[a-z\d]+";

        private const string ObsLowercase = @"^[a-z\d]+.[a-z\d]+";
        private const string Obs1NestLowercase = @"^[a-z\d]+.[a-z\d]+.[a-z\d]+";
        private const string Obs2NestLowercase = @"^[a-z\d]+.[a-z\d]+.[a-z\d]+.[a-z\d]+";
        private const string Obs3NestLowercase = @"^[a-z\d]+.[a-z\d]+.[a-z\d]+.[a-z\d]+.[a-z\d]+";

        private static string[] _nugetDataDirectories;

        private static Dictionary<string, ModuleDefinition> _baseModuleDefinitions;

        public static AssemblyModel EmptyAssemblyModel = new AssemblyModel();

        public string AssemblyName { get; private set; }

        public Dictionary<string, ClassModel> ClassModels { get; }

        public static AssemblyModel GetAssemblyModel(ModuleDefinition moduleDefinition, string nugetDataDirectory)
        {
            _nugetDataDirectories = Directory.GetDirectories(nugetDataDirectory);
            _baseModuleDefinitions = new Dictionary<string, ModuleDefinition>();

            if (moduleDefinition == null)
            {
                return new AssemblyModel();
            }

            return new AssemblyModel().Build(moduleDefinition);
        }
          
        private AssemblyModel()
        {
            ClassModels = new Dictionary<string, ClassModel>();
        }

        private AssemblyModel Build(ModuleDefinition moduleDefinition)
        {
            if (string.IsNullOrWhiteSpace(moduleDefinition?.Assembly?.Name?.Name))
            {
                return this;
            }

            AssemblyName = moduleDefinition.Assembly.Name.Name;
            foreach (var typeDefinition in moduleDefinition.Types)
            {
                if (!typeDefinition.IsClass
                    || typeDefinition.FullName.StartsWith("<")
                    || typeDefinition.FullName.StartsWith("Dotfuscator")
                    || typeDefinition.FullName.StartsWith("System.Reflection")
                    || typeDefinition.FullName.StartsWith("Microsoft.Win32")
                    || Regex.Match(typeDefinition.FullName, SystemLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, System1NestLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, System2NestLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, System3NestLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, ObsLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, Obs1NestLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, Obs2NestLowercase, RegexOptions.Compiled).Success
                    || Regex.Match(typeDefinition.FullName, Obs3NestLowercase, RegexOptions.Compiled).Success
                    )
                {
                    continue;
                }

                var classModel = new ClassModel(typeDefinition.FullName, GetAccessLevel(typeDefinition));
                BuildMethodModels(classModel, typeDefinition);
                ClassModels.Add(classModel.Name, classModel);
            }

            return this;
        }

        private string GetAccessLevel(TypeDefinition typeDefinition)
        {
            if (typeDefinition.IsPublic)
            {
                return "public";
            }
            else if (typeDefinition.IsNotPublic)
            {
                return "private";
            }


            return "";
        }

        private void BuildMethodModels(ClassModel classModel, TypeDefinition typeDefinition)
        {
            var methods = typeDefinition.Methods.ToList();
            methods.AddRange(GetBaseTypeMethods(typeDefinition.BaseType));
            foreach (var method in methods.Distinct())
            {
                var methodModel = classModel.GetOrCreateMethodModel(method.Name);
                if (method.HasParameters)
                {
                    var parameters = method.Parameters.Select((x) => x.ParameterType.FullName.Replace('<', '[').Replace('>', ']')).ToList();
                    methodModel.ParameterSets.Add(string.Join(",", parameters));
                }
                else
                {
                    // covers a method having no parameters.
                    methodModel.ParameterSets.Add(string.Empty);
                }
            }
        }

        private List<MethodDefinition> GetBaseTypeMethods(TypeReference baseType)
        {
            var baseAssemblyName = baseType?.Scope?.Name;
            if (!_baseModuleDefinitions.TryGetValue(baseAssemblyName, out var moduleDefinition))
            {
                if (string.IsNullOrWhiteSpace(baseAssemblyName))
                {
                    return new List<MethodDefinition>();
                }

                var baseAssemblyDir = _nugetDataDirectories.FirstOrDefault(d => new DirectoryInfo(d).Name.ToLower().StartsWith(baseAssemblyName.ToLower()));
                if (string.IsNullOrWhiteSpace(baseAssemblyDir))
                {
                    return new List<MethodDefinition>();
                }

                var candidaterDlls = Directory.GetFiles(baseAssemblyDir, $"{baseAssemblyName}.dll", SearchOption.AllDirectories);
                if (candidaterDlls.Length < 1)
                {
                    return new List<MethodDefinition>();
                }

                moduleDefinition = ModuleDefinition.ReadModule(candidaterDlls[0]);
                _baseModuleDefinitions.Add(baseAssemblyName, moduleDefinition);
            }

            var typeDef = moduleDefinition.Types.FirstOrDefault(t => t.FullName == baseType.FullName);
            if (typeDef == null)
            {
                return new List<MethodDefinition>();
            }

            return typeDef.Methods.ToList();
        }
    }
}
