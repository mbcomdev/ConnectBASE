using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NJsonSchema.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace connectBase.Services
{
    public interface IValidationService
    {
        public Task Validate(Dictionary<string, object> data, string table);
    }
    public class ValidationService : IValidationService
    {
        private ILogger<ValidationService> Logger { get; }
        private readonly string validationPath = @"./ValidationFiles/";
        private readonly string validationFileExtension = ".json";

        public ValidationService(ILogger<ValidationService> logger)
        {
            //Initialize logger
            Logger = logger;
        }

        public async Task Validate(Dictionary<string, object> data, string table)
        {
            Logger.LogInformation("Validate JSON for table " + table);
            // read file into a string and parse JsonSchema from the string
            var schema = await NJsonSchema.JsonSchema.FromFileAsync(validationPath + table + validationFileExtension);
            var validator = new JsonSchemaValidator();
            var result = validator.Validate(JsonConvert.SerializeObject(data), schema);

            if (result.Count > 0)
            {
                List<string> errorList = new List<string>();
                foreach (var error in result)
                {
                    errorList.Add(error.ToString());
                }
                var serialized = JsonConvert.SerializeObject(errorList);
                throw new Exception(serialized);
            }
        }
    }
}
