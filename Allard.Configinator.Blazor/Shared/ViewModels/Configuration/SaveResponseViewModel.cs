using System.Collections.Generic;
using Allard.Configinator.Core.Infrastructure;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Configuration
{
    public class SaveResponseViewModel
    {
        public ConfigurationId ConfigurationId { get; set; }
        public List<FailureMessage> Failures { get; set; }

        public bool Success { get; set; }
    }

    public class FailureMessage
    {
        public string Code { get; set; }
        public string ObjectPath { get; set; }
        public string Message { get; set; }
    }
}