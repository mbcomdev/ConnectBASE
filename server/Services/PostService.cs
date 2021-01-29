using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BpNT;
using connectBase.Entities;

namespace connectBase.Services
{
    public interface IPostService
    {
        void Post(User user, string table, Dictionary<string, object> data);

        void Post(User user, string table, string nestedTable, Dictionary<string, object> data);
    }

    public class PostService : IPostService
    {
        private COMConnection COMConnection { get; }
        private ILogger<SchemeService> Logger { get; }

        public PostService(COMConnection comConnection, ILogger<SchemeService> logger)
        {
            Logger = logger;
            COMConnection = comConnection;
            Logger.LogInformation("PostService started");
        }

        Application bpApp;

        /// <summary>
        /// inserts a dataset into the specified table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="data">should be validated before calling this function</param>
        /// <returns></returns>
        public void Post(User user, string table, Dictionary<string, object> data)
        {
            Logger.LogInformation("Post into table " + table);
            Application bpApp = COMConnection.GetUserApplication(user);
            var dataset = bpApp.DataSetInfos[table].CreateDataSet();

            dataset.Append();
            foreach (var item in data)
            {
                dataset.Fields[item.Key].Value = item.Value;
            }
            dataset.Post();
            Logger.LogInformation("Post sucessful");
        }

        /// <summary>
        /// inserts a dataset into the specified nested table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="nestedTable"></param>
        /// <param name="data"></param>
        public void Post(User user, string table, string nestedTable, Dictionary<string, object> data)
        {
            Logger.LogInformation("Post into nested table " + table);
            Application bpApp = COMConnection.GetUserApplication(user);

            var dataset = bpApp.DataSetInfos[table].CreateDataSet();
            var nestedDataset = dataset.NestedDataSets[nestedTable];

            dataset.Edit();
            nestedDataset.Append();
            foreach (var item in data)
            {
                nestedDataset.Fields[item.Key].Value = item.Value;
            }
            nestedDataset.Post();
            nestedDataset.PostNestedDataSet();
            dataset.Post();
            Logger.LogInformation("Post sucessful");
        }
    }
}
