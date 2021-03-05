using BpNT;
using connectBase.Services.COM;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace connectBase.Services
{
    public interface IGetService
    {
        string GetAll(string table, List<string> returnFields);
        string GetFromIndexList(string table, string index, string indexList, List<string> returnFields);
        string GetIndexList(string table);
        string GetFromNestedTable(string table, string nestedTable, List<string> returnFields);
        string GetRange(string table, string index, Dictionary<string, string> indexFields, List<string> returnFields);
    }
    public class GetService : IGetService
    {
        private COMConnection COMConnection { get; }
        private ILogger<SchemeService> Logger { get; }
        public GetService(COMConnection comConnection, ILogger<SchemeService> logger)
        {
            COMConnection = comConnection;
            Logger = logger;

            Logger.LogInformation("GetService started");
        }

        /// <summary>
        /// Get all datasets for a given table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="returnFields"></param>
        /// <returns></returns>
        public string GetAll(string table, List<string> returnFields)
        {
            Logger.LogInformation("GetAll from table " + table);
            Application bpApp = this.COMConnection.GetApplication();
            AutoDataSet dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var globalList = new List<Dictionary<string, dynamic?>>();
            dataset.First();
            while (!dataset.Eof)
            {
                var localDictionary = new Dictionary<string, dynamic?>();
                foreach (BpNT.AutoField autoField in dataset.Fields)
                {
                    if (autoField.CanAccess)
                    {
                        if (returnFields.Count == 0 || returnFields.Contains(autoField.Name))
                        {

                            try
                            {
                                var fieldValue = Util.Util.ReadPropertyValue(dataset, autoField.Name);
                                localDictionary.Add(autoField.Name, fieldValue);
                            }
                            catch (Exception)
                            {
                                // Do nothing if a field does not have a correct value type
                            }
                        }
                    }
                }
                globalList.Add(localDictionary);
                dataset.Next();
            }
            var serializedData = "[]";
            if (globalList.Count > 0)
            {
                serializedData = System.Text.Json.JsonSerializer.Serialize(globalList, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
            }
            Logger.LogInformation("Get sucessful");
            return serializedData;
        }

        /// <summary>
        /// Get data from range with given table, index and returnFields
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="indexFields"></param>
        /// <param name="returnFields"></param>
        /// <returns></returns>
        public string GetRange(string table, string index, Dictionary<string, string> indexFields, List<string> returnFields)
        {
            Logger.LogInformation("GetRange from table " + table);
            List<Dictionary<string, dynamic?>> result = new List<Dictionary<string, dynamic?>>();
            Application bpApp = this.COMConnection.GetApplication();
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            dataset.Indices[index].Select();
            dataset = Util.Util.SetRange(dataset, indexFields);
            dataset.First();
            while (!dataset.Eof)
            {
                Dictionary<string, dynamic?> resultDictionary = new Dictionary<string, dynamic?>();
                if (returnFields.Count == 0)
                {
                    foreach (BpNT.AutoField autoField in dataset.Fields)
                    {
                        if (autoField.CanAccess)
                        {
                            try
                            {
                                var fieldValue = Util.Util.ReadPropertyValue(dataset, autoField.Name);
                                resultDictionary.Add(autoField.Name, fieldValue);
                            }
                            catch (Exception)
                            {
                                // Do nothing if a field does not have a correct value type
                            }

                        }
                    }
                }
                else
                {
                    foreach (string key in returnFields)
                    {
                        if (dataset.Fields[key].CanAccess)
                        {
                            resultDictionary.Add(key, Util.Util.ReadPropertyValue(dataset, dataset.Fields[key].Name));
                        }
                    }
                }
                result.Add(resultDictionary);
                dataset.Next();
            }
            var serializedData = System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }); ;
            Logger.LogInformation("Get sucessful");
            return serializedData;
        }


        /// <summary>
        /// Returns a list of values from a given table and index list
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="indexList"></param>
        /// <param name="returnFields"></param>
        /// <returns></returns>
        public string GetFromIndexList(string table, string index, string indexList, List<string> returnFields)
        {
            Logger.LogInformation("GetFromIndexList from table " + table);
            Application bpApp = this.COMConnection.GetApplication();
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var globalList = new List<Dictionary<string, dynamic?>>();
            var indexDictionary = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(indexList);
            foreach (KeyValuePair<string, string[]> kvp in indexDictionary)
            {
                foreach (var value in kvp.Value)
                {
                    dataset.Indices[index].Select();
                    dataset.SetKey();
                    dataset.Fields[kvp.Key].AsString = value;

                    if (dataset.GotoKey())
                    {
                        var localDictionary = new Dictionary<string, dynamic?>();
                        foreach (BpNT.AutoField autoField in dataset.Fields)
                        {
                            if (autoField.CanAccess)
                            {
                                if (returnFields.Count == 0 || returnFields.Contains(autoField.Name))
                                {
                                    try
                                    {
                                        var fieldValue = Util.Util.ReadPropertyValue(dataset, autoField.Name);
                                        localDictionary.Add(autoField.Name, fieldValue);
                                    }
                                    catch (Exception)
                                    {
                                        // Do nothing if a field does not have a correct value type
                                    }
                                }
                            }
                        }
                        if (localDictionary.Count > 0)
                        {
                            globalList.Add(localDictionary);
                        }
                    };
                }
            }
            var serializedData = "[]";
            if (globalList.Count > 0)
            {
                serializedData = System.Text.Json.JsonSerializer.Serialize(globalList, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
            }
            Logger.LogInformation("Get sucessful");
            return serializedData;
        }

        /// <summary>
        /// Get all datasets for a given nestedTable
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="nestedTable"></param>
        /// <param name="returnFields"></param>
        /// <returns></returns>
        public string GetFromNestedTable(string table, string nestedTable, List<string> returnFields)
        {
            Logger.LogInformation("GetFromNestedTable from table " + table);
            Application bpApp = this.COMConnection.GetApplication();
            AutoDataSet dataset = bpApp.DataSetInfos[table].CreateDataSet();
            AutoDataSet nestedDataSet = dataset.NestedDataSets[nestedTable];
            var globalList = new List<Dictionary<string, dynamic?>>();
            nestedDataSet.First();
            while (!nestedDataSet.Eof)
            {
                var localDictionary = new Dictionary<string, dynamic?>();
                foreach (BpNT.AutoField autoField in nestedDataSet.Fields)
                {
                    if (autoField.CanAccess)
                    {
                        if (returnFields.Count == 0 || returnFields.Contains(autoField.Name))
                        {
                            try
                            {
                                var fieldValue = Util.Util.ReadPropertyValue(nestedDataSet, autoField.Name);
                                localDictionary.Add(autoField.Name, fieldValue);
                            }
                            catch (Exception)
                            {
                                // Do nothing if a field does not have a correct value type
                            }
                        }
                    }
                }
                globalList.Add(localDictionary);
                nestedDataSet.Next();
            }
            var serializedData = "[]";
            if (globalList.Count > 0)
            {
                serializedData = System.Text.Json.JsonSerializer.Serialize(globalList, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
            }
            Logger.LogInformation("Get sucessful");
            return serializedData;
        }

        /// <summary>
        /// Get a index list from a table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public string GetIndexList(string table)
        {
            Logger.LogInformation("GetIndexList from table " + table);
            Application bpApp = this.COMConnection.GetApplication();
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var indexList = new List<string>();

            foreach (AutoIndex index in dataset.Indices)
            {
                indexList.Add(index.Name);
            }

            var serializedData = "[]";
            if (indexList.Count > 0)
            {
                serializedData = System.Text.Json.JsonSerializer.Serialize(indexList, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
            }
            Logger.LogInformation("Get sucessful");
            return serializedData;
        }
    }
}