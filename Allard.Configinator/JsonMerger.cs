using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;

namespace Allard.Configinator
{
    /// <summary>
    /// Merge json documents into one.
    /// </summary>
    public class JsonMerger
    {
        // given a list of documents
        //      a
        //      b
        //      c
        //
        // merge a into b, then b in to c
        //

        private readonly List<JToken> toMerge = new();

        public JsonMerger(params JToken[] toMerge)
        {
            this.toMerge = toMerge.ToList();
        }

        public JsonMerger(IEnumerable<JToken> toMerge)
        {
            this.toMerge = toMerge.ToList();
        }

        public JToken Merge()
        {
            if (toMerge.Count == 0)
            {
                return null;
            }

            if (toMerge.Count == 1)
            {
                return toMerge[0];
            }

            var source = toMerge[0];
            for (var i = 1; i < toMerge.Count; i++)
            {
                var target = toMerge[i];
                Merge(source, target);
                source = target;
            }

            return toMerge.Last();
        }

        private void Merge(JToken source, JToken target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);

            // if the source is null,
            // then delete if from the target.
            if (source.Type == JTokenType.Null)
            {
                target.Parent?.Remove();
                return;
            }

            if (source.Type != target.Type)
            {
                throw new Exception("different types");
            }

            if (source is JValue value)
            {
                ((JValue) target).Value = value.Value;
                return;
            }

            if (source is JObject obj)
            {
                MergeObject(obj, (JObject) target);
                return;
            }

            if (source is JProperty prop)
            {
                Merge(prop.Value, ((JProperty) target).Value);
                return;
            }

            throw new Exception("unhandled type: " + source.Type);
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