using BpNT;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using connectBase.Entities;
using Newtonsoft.Json;
using System.Linq;

namespace connectBase.Services
{
    public interface ISchemeService
    {
        void CreateDatabaseScheme(User user);
        string GetDatabaseScheme(string table);
        string GetAllTableNames();
        void CleanupDirectories(string arg);
        string GetCurrentBuildingStatus();
    }
    public class SchemeService : ISchemeService
    {
        private COMConnection _COMConnection { get; }
        private readonly ILogger<SchemeService> _logger;
        private readonly string schemePath = @"./SchemeFiles/";
        private readonly string validationPath = @"./ValidationFiles/";
        private readonly string validationFileExtension = ".json";
        private readonly string schemeFileExtension = ".json";
        private static string statusProgress = "In Progress";
        private static string statusDate = "";
        public SchemeService(COMConnection comConnection, ILogger<SchemeService> logger)
        {
            //Initialize logger
            _logger = logger;
            //Get COMConnection Service
            this._COMConnection = comConnection;
        }

        /**
         * Get a database scheme from file
         */
        public string GetDatabaseScheme(string table)
        {
            return File.ReadAllText(schemePath + table + schemeFileExtension);
        }

        /**
         * Get all file names from directory
         */
        public string GetAllTableNames()
        {
            IEnumerable<string> fileArray = Directory.GetFiles(schemePath, "*", SearchOption.AllDirectories).Select(f => ((Path.GetFileName(f)).Split("."))[0]);
            return JsonConvert.SerializeObject(fileArray);
        }

        /**
         * If command line arguments -cleanupDirectories is set to true, validation and scheme
         * file folders will be deleted at startup
         */
        public void CleanupDirectories(string arg)
        {
            try
            {
                if (Convert.ToBoolean(arg))
                {
                    if (Directory.Exists(schemePath))
                    {
                        Directory.Delete(schemePath, true);
                        _logger.LogInformation("Successfully deleted directory " + schemePath + " with all files and subfolders");
                    }
                    if (Directory.Exists(validationPath))
                    {
                        Directory.Delete(validationPath, true);
                        _logger.LogInformation("Successfully deleted directory " + validationPath + " with all files and subfolders");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Given value for command line argument '-cleanupDirectories' was not a valid boolean\n\r" + e.Message);
            }
        }

        /**
         * Generate complete database scheme and validation files
         * For every database a json file with the scheme and a json validation file will be generated
         * If scheme or validation files already exists we will skip this step
         */
        public void CreateDatabaseScheme(User user)
        {
            this._COMConnection.Login(user);
            Application bpApp = this._COMConnection.GetUserApplication(user);
            _logger.LogInformation("Start creating database scheme and validation files.");
            try
            {
                foreach (BpNT.AutoDataSetInfo dataset in bpApp.DataSetInfos)
                {
                    var topLevelDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    var schemeFileName = PolishFileName(dataset.Name + schemeFileExtension);
                    var validationFileName = PolishFileName(dataset.Name + validationFileExtension);
                    var schemeFileExists = false;
                    var validationFileExists = false;

                    if (!Directory.Exists(schemePath))
                    {
                        Directory.CreateDirectory(schemePath);
                    }

                    // Check if scheme file exists
                    if (!File.Exists(schemePath + schemeFileName))
                    {
                        // Start creating schema file
                        _logger.LogInformation("Scheme file: '" + schemeFileName + "' does not exist");
                        FileStream fs = File.Create(schemePath + schemeFileName);
                        fs.Close();
                        _logger.LogInformation("Scheme file '" + schemeFileName + "' created.");
                    }
                    else
                    {
                        schemeFileExists = true;
                        _logger.LogInformation("Scheme file '" + schemeFileName + "' already exists.");
                    }

                    if (!Directory.Exists(validationPath))
                    {
                        Directory.CreateDirectory(validationPath);
                    }

                    // Check if validation file exists
                    if (!File.Exists(validationPath + validationFileName))
                    {
                        // Start creating validation file
                        _logger.LogInformation("Validation file: '" + validationFileName + "' does not exist");
                        FileStream fs = File.Create(validationPath + validationFileName);
                        fs.Close();
                        _logger.LogInformation("Validation file '" + validationFileName + "' created.");
                    }
                    else
                    {
                        validationFileExists = true;
                        _logger.LogInformation("Validation file '" + validationFileName + "' already exists.");
                    }

                    // Skip if both files already exists
                    if (!schemeFileExists || !validationFileExists)
                    {
                        // Get top level database scheme and create validation json file
                        var tableDictionary = GenerateDatabaseDictionary(bpApp, dataset);

                        if (!topLevelDictionary.ContainsKey(dataset.Name))
                        {
                            topLevelDictionary.Add(dataset.Name, tableDictionary);
                            // Serialize Dictionary to JSON
                            var serializedDatabase = System.Text.Json.JsonSerializer.Serialize(topLevelDictionary, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                            // Write serialized data to file
                            _logger.LogInformation("Trying to write into scheme file '" + schemeFileName + "'");
                            File.WriteAllText(schemePath + schemeFileName, serializedDatabase);
                            _logger.LogInformation("Finished writing.");
                        }
                    }

                    //Write nested database scheme and validation files
                    foreach (BpNT.AutoDataSetInfo nestedDataset in dataset.NestedDataSets)
                    {
                        var nestedSchemeFileExists = false;
                        var nestedValidationFileExists = false;

                        //Set nested file name
                        var nestedSchemeFileName = PolishFileName(nestedDataset.Name + schemeFileExtension);
                        var nestedValidationFileName = PolishFileName(nestedDataset.Name + validationFileExtension);

                        // Check if nested scheme file exists
                        if (!File.Exists(schemePath + nestedSchemeFileName))
                        {
                            // Start creating schema file
                            _logger.LogInformation("Scheme file: '" + nestedSchemeFileName + "' does not exist");
                            FileStream fs = File.Create(schemePath + nestedSchemeFileName);
                            fs.Close();
                            _logger.LogInformation("Scheme file '" + nestedSchemeFileName + "' created.");
                        }
                        else
                        {
                            nestedSchemeFileExists = true;
                            _logger.LogInformation("Scheme file '" + nestedSchemeFileName + "' already exists.");
                        }

                        // Check if nested validation file exists
                        if (!File.Exists(validationPath + nestedValidationFileName))
                        {
                            // Start creating validation file
                            _logger.LogInformation("Validation file: '" + nestedValidationFileName + "' does not exist");
                            FileStream fs = File.Create(validationPath + nestedValidationFileName);
                            fs.Close();
                            _logger.LogInformation("Validation file '" + nestedValidationFileName + "' created.");
                        }
                        else
                        {
                            nestedValidationFileExists = true;
                            _logger.LogInformation("Validation file '" + nestedValidationFileName + "' already exists.");
                        }

                        // Skip if both files already exists
                        if (!nestedSchemeFileExists || !nestedValidationFileExists)
                        {
                            var nestedDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                            // Get nested scheme data and create validation files                           
                            var nestedTableDictionary = GenerateDatabaseDictionary(bpApp, dataset, true, nestedDataset);

                            if (!nestedDictionary.ContainsKey(nestedDataset.Name))
                            {
                                nestedDictionary.Add(nestedDataset.Name, nestedTableDictionary);
                                //Write serialized data to file
                                var nestedSerializedDatabase = System.Text.Json.JsonSerializer.Serialize(nestedDictionary, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                                _logger.LogInformation("Trying to write into schema file '" + nestedSchemeFileName + "'");
                                File.WriteAllText(schemePath + nestedSchemeFileName, nestedSerializedDatabase);
                                _logger.LogInformation("Finished writing.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this._COMConnection.Logout(user.Username, user.Mandant);
                _logger.LogError(e.Message);
            }
            _logger.LogInformation("Successfully created scheme and validation files");

            if (File.Exists(schemePath + "configuration"))
            {
                SchemeService.statusDate = File.ReadAllText(schemePath + "configuration");
            }
            else
            {
                File.WriteAllText(schemePath + "configuration", (DateTime.Now).ToString());
            }

            this._COMConnection.Logout(user.Username, user.Mandant);
            SchemeService.statusProgress = "Ok";
            SchemeService.statusDate = File.ReadAllText(schemePath + "configuration");
        }

        /**
         * Get information about the current building status
         */
        public string GetCurrentBuildingStatus()
        {
            var status = new Dictionary<string, string>();
            status.Add("Status", SchemeService.statusProgress);
            status.Add("Builded", SchemeService.statusDate);
            return System.Text.Json.JsonSerializer.Serialize(status);
        }

        /**
         * Some databases contains forbidden characters for creating files
         * ToDo: Refactor (regex?)
         */
        private string PolishFileName(string name)
        {
            return (name.Replace("/", "-")).ToLower();
        }

        /**
         * Get a database field properties
         * Return a Dictionary with all field informations and create json validation file
         */
        private Dictionary<string, Dictionary<string, string>> GenerateDatabaseDictionary(Application bpApp, AutoDataSetInfo dataset, bool isNested = false, AutoDataSetInfo nestedDataset = null)
        {
            //Create AutoDataSet Interface
            BpNT.IAutoDataSet productDataset = bpApp.DataSetInfos[dataset.Name].CreateDataSet();

            //Check if database is nested
            var productDatasetFields = (AutoFields)null;
            if (!isNested)
            {
                productDatasetFields = productDataset.Fields;
            }
            else
            {
                productDatasetFields = productDataset.NestedDataSets[nestedDataset.Name].Fields;
            }

            //Create class
            List<string> lines = new List<string>
            {
                "{",
                "\"$schema\": \"http://json-schema.org/schema#\",",
                "\"type\": \"object\",",
                "\"properties\": {",
            };

            var countFields = productDatasetFields.Count;
            var count = 0;
            var tableDictionary = new Dictionary<string, Dictionary<string, string>>();
            foreach (BpNT.AutoField autoField in productDatasetFields)
            {
                count = count + 1;
                var fieldDictionary = new Dictionary<string, string>();
                if (autoField.CanAccess)
                {
                    string type = "";
                    switch (autoField.FieldInfo.Art)
                    {
                        case BpNT.FeldDefArt.fdaChar:
                            type = "char?";
                            break;
                        case BpNT.FeldDefArt.fdaByte:
                            type = "byte?";
                            break;
                        case BpNT.FeldDefArt.fdaBoolean:
                            type = "bool?";
                            break;
                        case BpNT.FeldDefArt.fdaSmallInt:
                            type = "short?";
                            break;
                        case BpNT.FeldDefArt.fdaAutoInc:
                            type = "int?";
                            break;
                        case BpNT.FeldDefArt.fdaInteger:
                            type = "int?";
                            break;
                        case BpNT.FeldDefArt.fdaSingle:
                            type = "float?";
                            break;
                        case BpNT.FeldDefArt.fdaDouble:
                            type = "double?";
                            break;
                        case BpNT.FeldDefArt.fdaDate:
                        case BpNT.FeldDefArt.fdaTime:
                        case BpNT.FeldDefArt.fdaDateTime:
                            type = "DateTime?";
                            break;
                        case BpNT.FeldDefArt.fdaString:
                            type = "string";
                            break;
                        case BpNT.FeldDefArt.fdaArray:
                            continue; //COM Access?
                        case BpNT.FeldDefArt.fdaBild:
                            type = "string";
                            break;
                        case BpNT.FeldDefArt.fdaBlobTable:
                            continue; //No COM Access
                        case BpNT.FeldDefArt.fdaInfo:
                            type = "string";
                            break;
                        case BpNT.FeldDefArt.fdaBlob:
                            continue;
                        case BpNT.FeldDefArt.fdaStringList:
                            continue; //COM Access?
                        case BpNT.FeldDefArt.fdaBpDataSet:
                        case BpNT.FeldDefArt.fdaTable:
                        case BpNT.FeldDefArt.fdaFilterBits:
                            continue; //No COM Access
                        case BpNT.FeldDefArt.fdaUnicodeString:
                            type = "string";
                            break;
                        case BpNT.FeldDefArt.fdaGUID:
                            continue; //No COM Access
                        default:
                            throw new Exception("No type defined for:" + autoField.FieldInfo.Art.ToString());
                    }

                    fieldDictionary.Add("Feldname", (autoField.FieldInfo.Name));
                    fieldDictionary.Add("Feldinfo", (autoField.FieldInfo.Info));
                    fieldDictionary.Add("Feldart", ((autoField.FieldInfo.Art).ToString()));
                    fieldDictionary.Add("Länge des Feldes", ((autoField.FieldInfo.Size).ToString()));
                    tableDictionary.Add(autoField.FieldInfo.Name, fieldDictionary);

                    // Get scheme type as string
                    var schemeType = GetSchemeType(type);

                    // Add field name
                    lines.Add("\"" + autoField.FieldInfo.Name + "\": {");

                    // If its a date time object add format
                    if (type == "DateTime?")
                    {
                        lines.Add("\"format\": \"date-time\",");
                    }

                    // If its a string, char or byte add min and max length
                    if (type == "string" || type == "char?" || type == "byte?")
                    {
                        lines.Add("\"minLength\": 0,");
                        lines.Add("\"maxLength\": " + (autoField.FieldInfo.Size).ToString() + ",");
                    }

                    // If its a numeric type add minimum and maximum
                    if (type == "short?" || type == "int?")
                    {
                        lines.Add("\"minimum\": 0,");
                        if ((autoField.FieldInfo.Size) != 0)
                        {
                            lines.Add("\"maximum\": " + (GetMaximumFieldNumber(type, autoField.FieldInfo.Size)) + ",");
                        }
                    }

                    // Add type 
                    lines.Add("\"type\": " + schemeType);

                    // Check for last field - last field doesnt need a ','
                    if (count < countFields)
                    {
                        lines.Add("},");
                    }
                    else
                    {
                        lines.Add("}");
                    }
                }
            }

            lines.Add("}");
            lines.Add("}");

            if (isNested)
            {
                _logger.LogInformation("Trying to write into nested validation file '" + nestedDataset.Name + "'");
                if (File.Exists(validationPath + nestedDataset.Name + validationFileExtension))
                {
                    System.IO.File.WriteAllLines(validationPath + nestedDataset.Name + validationFileExtension, lines);
                    _logger.LogInformation("Created nested validation file '" + nestedDataset.Name + "'");
                }
                else
                {
                    _logger.LogInformation("Nested validation file '" + nestedDataset.Name + "' already exists");
                }
            }
            else
            {
                _logger.LogInformation("Trying to write into validation file '" + dataset.Name + "'");
                if (File.Exists(validationPath + dataset.Name + validationFileExtension))
                {
                    System.IO.File.WriteAllLines(validationPath + dataset.Name + validationFileExtension, lines);
                    _logger.LogInformation("Created validation file '" + dataset.Name + "'");
                }
                else
                {
                    _logger.LogInformation("Validation file '" + dataset.Name + "' already exists");
                }
            }
            return tableDictionary;
        }

        private string GetSchemeType(string type)
        {
            var nullable = "\"null\"";
            if (type.Contains("?"))
            {
                var types = type.Split("?");
                return "[\"" + TransferJsonType(types[0]) + "\", " + nullable + "]";
            }
            else
            {
                return "\"" + TransferJsonType(type) + "\"";
            }
        }

        private string GetMaximumFieldNumber(string type, int size)
        {
            var maxLength = "0";
            switch (type)
            {
                case "short?":
                    if (size >= 5)
                    {
                        maxLength = "32767";
                    }
                    else
                    {
                        maxLength = string.Concat(Enumerable.Repeat("9", size));
                    }
                    break;
                case "int?":
                    if (size >= 10)
                    {
                        return "2147483647";
                    }
                    else
                    {
                        return string.Concat(Enumerable.Repeat("9", size));
                    }
                    break;
                default:
                    throw new Exception("No type defined");
            }
            return maxLength;
        }

        private string TransferJsonType(string type)
        {
            var jsonType = "";
            switch (type)
            {
                case "char":
                    jsonType = "string";
                    break;
                case "byte":
                    jsonType = "string";
                    break;
                case "bool":
                    jsonType = "boolean";
                    break;
                case "short":
                    jsonType = "integer";
                    break;
                case "int":
                    jsonType = "integer";
                    break;
                case "float":
                    jsonType = "number";
                    break;
                case "double":
                    jsonType = "number";
                    break;
                case "DateTime":
                    jsonType = "string";
                    break;
                case "string":
                    jsonType = "string";
                    break;
                default:
                    throw new Exception("No type defined");
            }
            return jsonType;
        }

    }
}
