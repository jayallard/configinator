The test loads all of the schemas from the files in this folder.

Then, it runs tests against them.
The tests make sure that the validation produces the expected results.

The tests are defined in the json files.

Each json file can test a single type.
Multiple tests can be defined for that type.
Each tests consists of:
     - a test value - the json to run through the validator
     - expected failures - the error messages the validator should produce
     
NOTES:
    the tests are is json rather than yaml due to the test-value.
    
    the expected-failures are defined in delimited text rather than individual properties, which would be better json.
    this is to keep it less verbose.
    
    
    