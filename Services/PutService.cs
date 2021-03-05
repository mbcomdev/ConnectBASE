using BpNT;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace connectBase.Services.COM
{
    public interface IPutService
    {
        /// <summary>
        /// function to modify a top level table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="newValues"></param>
        /// <returns></returns>
        public string PutTopLevelTable(string table, string index, Dictionary<string, object> newValues);
    };
    public class PutService : IPutService
    {
        private COMConnection COMConnection { get; }
        private ILogger<SchemeService> Logger { get; }
        public PutService(COMConnection comConnection, ILogger<SchemeService> logger)
        {
            COMConnection = comConnection;
            Logger = logger;
            Logger.LogInformation("PutService started");
        }

        #region funktions called from Controller 

        /// <summary>
        /// function to modify a top level table
        /// </summary>
        /// <param name="user"></param>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <param name="newValues"></param>
        /// <returns></returns>
        public string PutTopLevelTable(string table, string index, Dictionary<string, object> newValues)
        {
            Logger.LogInformation("PutTopLevelTable " + table);
            Application bpApp = this.COMConnection.GetApplication();
            BpNT.AutoDataSet dataset = bpApp.DataSetInfos[table].CreateDataSet();
            List<string> returnFields = new List<string>(newValues.Keys);
            Util.Util.GoToSpecificRowOfTable(dataset, index, newValues, table);
            var ret = Util.Util.UpdateSpecifiedRow(dataset, newValues, returnFields);
            Logger.LogInformation("Put successful");
            return ret;
        }

        #endregion
    }
}