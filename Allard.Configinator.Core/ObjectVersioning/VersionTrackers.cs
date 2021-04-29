using System.Collections.Generic;
using System.Linq;
namespace Allard.Configinator.Core.ObjectVersioning
{
    public class VersionTrackers
    {
        private readonly Dictionary<string, VersionTracker> trackers = new();
        private readonly Node objectModel;

        public VersionTrackers(Node objectModel)
        {
            this.objectModel = objectModel.EnsureValue(nameof(objectModel));
        }

        private VersionTracker GetOrCreate(string trackerName)
        {
            if (trackers.TryGetValue(trackerName, out var tracker))
            {
                return tracker;
            }

            tracker = new VersionTracker(objectModel, trackerName);
            trackers[trackerName] = tracker;
            return tracker;
        }

        public VersionTrackers AddVersion(string trackerName, string versionName, Node value)
        {
            GetOrCreate(trackerName).AddVersion(versionName, value);
            return this;
        }

        public VersionTrackers UpdateVersion(string trackerName, string versionName, Node value)
        {
            GetOrCreate(trackerName).UpdateVersion(versionName, value);
            return this;
        }
    }
}