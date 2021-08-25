// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0


using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace NewRelic.Agent.MultiverseScanner.Models
{
    public class AssemblyModel
    {
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
            BuildClassModels(moduleDefinition.Types);
            return this;
        }

        private void BuildClassModels(Mono.Collections.Generic.Collection<TypeDefinition> typeDefinitions)
        {
            var splitAssemblyName = AssemblyName.Split('.');
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.IsClass
                    && (typeDefinition.FullName.StartsWith(AssemblyName) || typeDefinition.FullName.StartsWith(splitAssemblyName[0]))
                    )
                {
                    //Build nested classes/methods.
                    BuildClassModels(typeDefinition.NestedTypes);

                    // Cecil uses a / to indicated a nested type while the profiler/XML uses +
                    var correctedClassName = typeDefinition.FullName.Replace('/', '+');
                    var classModel = new ClassModel(correctedClassName, GetAccessLevel(typeDefinition));
                    BuildMethodModels(classModel, typeDefinition);
                    ClassModels.Add(classModel.Name, classModel);
                }
            }
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
            if (string.IsNullOrWhiteSpace(baseAssemblyName))
            {
                return new List<MethodDefinition>();
            }

            if (!_baseModuleDefinitions.TryGetValue(baseAssemblyName, out var moduleDefinition))
            {
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
