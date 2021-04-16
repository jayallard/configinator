using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public class RealmVariableContainer
    {
        private readonly IRealm realm;
        private readonly Dictionary<string, RealmVariable> variables = new();

        public RealmVariableContainer(IRealm realm)
        {
            this.realm = realm;
        }

        public void AddVariable(RealmVariable variable)
        {
            var id = new SectionId(variable.SectionId);
            var cs = realm.GetConfigurationSection(variable.SectionId);
            if (variables.ContainsKey(variable.Name))
            {
                throw ModelExceptions.RealmVariableAlreadyExists(variable.Name);
            }

            if (string.IsNullOrWhiteSpace(variable.ConfigPath) || variable.ConfigPath
                .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 0)
            {
                throw new InvalidOperationException("Invalid Configuration Path: " + variable.ConfigPath);
            }

            var structure = StructureBuilder.ToStructure(cs);
            if (!structure.Exists(variable.ConfigPath))
            {
                throw new InvalidOperationException("Configuration Path doesn't exist: " + variable.ConfigPath);
            }

            var node = structure.FindNode(variable.ConfigPath);
        }

        private class VariableAdder
        {
            
            private readonly RealmVariableContainer variables;
            private readonly RealmVariable variableToAdd;
            private ConfigurationSection configurationSection;
            public VariableAdder(RealmVariableContainer container, RealmVariable variableToAdd)
            {
                variables = container;
                this.variableToAdd = variableToAdd;
            }

            public void Add()
            {
                GetConfigurationSection();
                ValidateVariable();
            }

            private void GetConfigurationSection()
            {
                configurationSection = variables.realm.GetConfigurationSection(variableToAdd.SectionId);
            }

            private void ValidateVariable()
            {
                // todo: validate paths have at least one segment
                // variable name can't already exist
                if (variables.variables.ContainsKey(variableToAdd.Name))
                {
                    throw ModelExceptions.RealmVariableAlreadyExists(variableToAdd.Name);
                }
                
                var sectionNodes = variableToAdd
                    .Assignments
                    .Select(a => a.ConfigurationSectionId)
                    .Union(new[] {variableToAdd.ConfigPath})
                    .Select(cs => variables.realm.GetConfigurationSection(cs))
                    .Select(cs => StructureBuilder.ToStructure(cs))
                    .ToDictionary(cs => cs.Name);
                
                // the source path needs to exist
                if (!sectionNodes[variableToAdd.ToString()].Exists(variableToAdd.ConfigPath))
                {
                    throw new InvalidOperationException("Source Config Path doesn't exist. Section=" +
                                                        configurationSection.SectionId.Id + ", Setting Path=" +
                                                        variableToAdd.ConfigPath);
                }
                
                // the assignment paths need to exist
                var firstBadPath = variableToAdd.Assignments.FirstOrDefault(assignment =>
                    !sectionNodes[assignment.ConfigurationSectionId].Exists(assignment.ConfigPath));
                if (firstBadPath != null)
                {
                    throw new InvalidOperationException("Target Config Path doesn't exist. Section=" +
                                                        configurationSection.SectionId.Id + ", Setting Path=" +
                                                        firstBadPath.ConfigPath);
                }
            }
        }
    }
}