using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Infrastructure;

namespace Allard.Configinator.Core.Model
{
    public class RealmVariableContainer
    {
        private readonly IRealm realm;
        private readonly Dictionary<string, RealmVariable> variables = new();

        public RealmVariableContainer(IRealm realm, IConfigStore configStore)
        {
            this.realm = realm.EnsureValue(nameof(realm));
        }

        public void AddVariable(RealmVariable variable)
        {
            ValidateVariable(variable);
            variables.Add(variable.Name, variable);
        }

        private void ApplyVariable(RealmVariable variable)
        {
            var source = realm.GetConfigurationSection(variable.SectionId);
            
        }

        private void ValidateVariable(RealmVariable variable)
        {
            // make sure variable doesn't already exist
            var cs = realm.GetConfigurationSection(variable.SectionId);
            if (variables.ContainsKey(variable.Name))
            {
                throw ModelExceptions.RealmVariableAlreadyExists(variable.Name);
            }

            // make sure source path is valid.
            if (string.IsNullOrWhiteSpace(variable.ConfigPath) || variable.ConfigPath
                .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 0)
            {
                throw new InvalidOperationException("Invalid Configuration Path: " + variable.ConfigPath);
            }

            // make sure source path exists
            if (!cs.PathExists(variable.ConfigPath))
            {
                throw new InvalidOperationException("Variable Source Path doesn't exist: " + variable.ConfigPath);
            }

            // make sure all assignment paths exist
            foreach (var target in variable.Assignments)
            {
                var targetSection = realm.GetConfigurationSection(target.ConfigurationSectionId);
                if (!targetSection.PathExists(target.ConfigPath))
                {
                    throw new InvalidOperationException("Variable Target Path doesn't exist: " + variable.ConfigPath);
                }
            }
        }
    }
}