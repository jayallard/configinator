using System;

namespace Allard.Configinator.Core.Model
{
    public static class ModelExceptions
    {
        public static Exception ConfigurationSectionDoesntExists(string configurationSectionId)
        {
            return new InvalidOperationException("Configuration Section Doesn't Exist. Section Id=" +
                                                 configurationSectionId);
        }

        public static Exception HabitatDoesntExist(string habitatId)
        {
            return new InvalidOperationException("Habitat doesn't exist. HabitatId= " + habitatId);
        }

        public static Exception RealmVariableAlreadyExists(string variableName)
        {
            return new InvalidOperationException("Realm Variable already exists. Variable Name= " + variableName);
        }
    }
}