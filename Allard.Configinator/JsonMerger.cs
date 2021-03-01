using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator
{
    /// <summary>
    ///     Merge json documents into one.
    ///     Given a list of documents: a, b, c
    /// </summary>
    public class JsonMerger
    {
        private readonly JToken target;
        private readonly List<JToken> overrides;

        public JsonMerger(JToken target, params JToken[] overrides)
        {
            this.target = target.EnsureValue(nameof(target));
            this.overrides = overrides
                .EnsureValue(nameof(overrides))
                .Where(d => d != null)
                .ToList();
        }

        public JsonMerger(JToken target, IEnumerable<JToken> overrides)
        {
            this.target = target ?? new JObject();
            this.overrides = overrides == null
                ? new List<JToken>()
                : overrides.ToList();
        }

        public JToken Merge()
        {
            /*
             get base1
             get base2
             get getTarget
             
             apply b2 to b1
             apply t to b1 
             */

            if (overrides.Count == 0)
            {
                // nothing to do
                return target;
            }

            // iterate the input docs using x.
            // merge x into x+1.
            foreach (var over in overrides)
            {
                Merge(over, target);
            }

            return target;
        }

        private void Merge(JToken overRide, JToken target)
        {
            if (overRide == null)
            {
                return;
            }

            while (true)
            {
                Debug.Assert(overRide != null);
                Debug.Assert(target != null);

                // if the source is null,
                // then delete if from the target.
                if (overRide.Type == JTokenType.Null)
                {
                    target.Parent?.Remove();
                    return;
                }

                if (overRide.Type != target.Type)
                {
                    throw new Exception("different types");
                }

                switch (overRide)
                {
                    case JValue value:
                        ((JValue) target).Value = value.Value;
                        return;
                    case JObject obj:
                        MergeObject(obj, (JObject) target);
                        return;
                    case JProperty prop:
                        overRide = prop.Value;
                        target = ((JProperty) target).Value;
                        continue;
                    default:
                        throw new Exception("unhandled type: " + overRide.Type);
                }
            }
        }

        private void MergeObject(JObject source, JObject target)
        {
            foreach (var sourceProperty in source.Properties())
            {
                var targetProperty = target.Property(sourceProperty.Name);
                if (targetProperty == null)
                {
                    // exists in source but not target, add
                    // it to target in its entirety
                    target[sourceProperty.Name] = sourceProperty.Value;
                    continue;
                }

                Merge(sourceProperty, targetProperty);
            }
        }
    }
}