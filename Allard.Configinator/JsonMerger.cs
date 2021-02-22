using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator
{
    /// <summary>
    ///     Merge json documents into one.
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
            this.toMerge = toMerge?.ToList() ?? throw new ArgumentNullException(nameof(toMerge));
        }

        public JsonMerger(IEnumerable<JToken> toMerge)
        {
            this.toMerge = toMerge?.ToList() ?? throw new ArgumentNullException(nameof(toMerge));
        }

        public JToken Merge()
        {
            switch (toMerge.Count)
            {
                // if no documents, then nothing to do.
                case 0:
                    return null;
                
                // if only one document, then nothing to merge,
                // so do nothing and return the 1 doc.
                case 1:
                    return toMerge[0];
            }

            // iterate the input docs.
            // merge x into x+1
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
            while (true)
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

                switch (source)
                {
                    case JValue value:
                        ((JValue) target).Value = value.Value;
                        return;
                    case JObject obj:
                        MergeObject(obj, (JObject) target);
                        return;
                    case JProperty prop:
                        source = prop.Value;
                        target = ((JProperty) target).Value;
                        continue;
                    default:
                        throw new Exception("unhandled type: " + source.Type);
                }

                break;
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