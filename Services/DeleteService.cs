using BpNT;
using connectBase.Services.COM;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace connectBase.Services
{
    public interface IDeleteService
    {
        string DeleteDatasetValue(string table, string index, string key);
        void DeleteAll(string table, string index);
    }
    public class DeleteService : IDeleteService
    {
        private COMConnection COMConnection { get; }

        private ILogger<SchemeService> Logger { get; }

        public DeleteService(COMConnection comConnection, ILogger<SchemeService> logger)
        {
            COMConnection = comConnection;
            Logger = logger;
            Logger.LogInformation("DeleteService started");
        }

        public void DeleteAll(string table, string index)
        {
            Logger.LogInformation("DeleteAll from table: " + table);
            Application bpApp = COMConnection.GetApplication();
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var indexDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(index);

            foreach (KeyValuePair<string, string> kvp in indexDictionary)
            {
                dataset.Indices[kvp.Key].Select();
                dataset.SetKey();
                dataset.Fields[kvp.Key].AsString = kvp.Value;

                if (dataset.GotoKey())
                {
                    dataset.Delete();
                }
            }
            Logger.LogInformation("Delete successful");
        }

        public string DeleteDatasetValue(string table, string index, string key)
        {
            Logger.LogInformation("DeleteDatasetValue from table: " + table);
            Application bpApp = COMConnection.GetApplication();
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var globalList = new List<Dictionary<string, dynamic?>>();
            var indexDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(index);

            foreach (KeyValuePair<string, string> kvp in indexDictionary)
            {
                dataset.Indices[kvp.Key].Select();
                dataset.SetKey();
                dataset.Fields[kvp.Key].AsString = kvp.Value;

                if (dataset.GotoKey())
                {
                    dataset.Edit();
                    var localDictionary = new Dictionary<string, dynamic?>();
                    foreach (BpNT.AutoField autofield in dataset.Fields)
                    {
                        if (autofield.Name == key)
                        {
                            if (autofield.CanAccess)
                            {
                                autofield.Clear();
                                dataset.Post();
                                localDictionary.Add(autofield.Name, autofield.Value);
                            }
                            else
                            {
                                throw new Exception("can not access " + autofield.Name);
                            }
                        }
                    }
                    globalList.Add(localDictionary);
                }
            }
            var serializedData = "[]";
            if (globalList.Count > 0)
            {
                serializedData = System.Text.Json.JsonSerializer.Serialize(globalList, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
            }
            Logger.LogInformation("Delete successful");
            return serializedData;
        }
    }
}
