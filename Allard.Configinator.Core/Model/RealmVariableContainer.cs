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
            ValidateNewVariable(variable);
            variables.Add(variable.Name, variable);
        }
        
        private void ValidateNewVariable(RealmVariable variable)
        {
            // schema type must exit
            realm.Organization.GetSchemaType(variable.SchemaTypeId);
            
            // variable can't already exist
            if (variables.ContainsKey(variable.Name))
            {
                throw ModelExceptions.RealmVariableAlreadyExists(variable.Name);
            }

            // make sure all assignment paths exist
            foreach (var assignment in variable.Assignments)
            {
                var targetSection = realm.GetConfigurationSection(assignment.SectionId);
                if (!targetSection.PathExists(assignment.ConfigPath))
                {
                    throw new InvalidOperationException("Variable Target Path doesn't exist: SectionId=" + assignment.SectionId + ", " + assignment.ConfigPath);
                }
            }
        }
    }
}