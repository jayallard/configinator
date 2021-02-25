The TYPES folder contains the schemas that we want to test.
Schemas contains 0 or more types, each of which contains 0 or more properties.
Each property is either a primitive, or refers to another type.
If it refers to another type, then it inherits all of its properties.
It is fully recursive of unlimited depth... it is also vulnerable to
circular references. That will be dealt with eventually.

The types are loaded and resolved to build up the entire schema.

The files in ExpectedResolution define what the resolved schemas are to look like.

The test loads all of the types, then compares them to the yaml in the ExpectedResolution folder. 
The tests pass when the resolved schema matches whats in the ExpectedResoluton folder.

IE:

---
types:
    "blah":
        "properties":
            "name": "string"
            "complex": "ns/another-type"

When that resolves, it will pull in the properties for another-type.
Suppose that another-type contains two name properties.
The expected result will look like:

---
types:
    "blah":
        "properties":
            "name": "string"
            "complex":
                "type": "ns/another-type"
                "properties":
                    "first-name": "string"
                    "last-name": "string"

IMPORTANT NOTE
--------------
The files in the ExpectedResolution folder are loaded into DTOs, as-is.

You can't use OPTIONAL: [] and SECRETS:[] as you do in a schema. Those things are processed by the resolver, which is what we're testing.

You need to explicitly define everything. Nothing is going to analyze your intent. The test is an object-by-object, property-by-property comparison.

GOOD:
"properties":
    "my-property":
        "type": "string"
        "is-optional": true
    
BAD:
"properties":
    "my-property":
        "type": "string"
        "is-optional": true
    "optional": ["my-property"]
    
Schemas support that. The resolver handles it.
The files in ExpectedResolution are not schemas. They are yam files used for the purposes of validating schema resolution.
They just happen to use the same DTOs, as a matter of convenience.





