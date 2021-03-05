using BpNT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace connectBase.Services.COM
{
    public class COMConnection : IDisposable
    {
        private readonly string CONNECTION_NAME = "MBCOM IT-Systemhaus GmbH - MBCOM ConnectEGIS!";
        private readonly string CONNECTION_KEY = "GPVV2-8ESVD-4XZ9J-6BG23-K5D68-FZZUE";
        private Application bpApp;
        private ILogger<COMConnection> _logger;
        public static readonly List<string> APIKEYLIST = new List<string>();

        public COMConnection(ILogger<COMConnection> logger, IConfiguration configuration)
        {
            //Initialize logger
            _logger = logger;
            _logger.LogInformation("COMAktiv Service Started");
            bpApp = new Application();
            try
            {
                bpApp.Init(
                            CONNECTION_NAME,
                            CONNECTION_KEY,
                            configuration["bueroplus_username"],
                            configuration["bueroplus_password"]
                            );
                bpApp.SelectMand(configuration["bueroplus_mandant"]);
                _logger.LogInformation("Bueroplus logged in");
                AutoDataSet dataset = bpApp.DataSetInfos["GlobalData"].CreateDataSet().NestedDataSets["ZugDa"];
                dataset.First();
                while (!dataset.Eof) {
                    if (dataset.Fields["Art"].AsInteger == 1 && dataset.Fields["Bez"].AsString.StartsWith("ConnectBASE")) {
                        APIKEYLIST.Add(dataset.Fields["ClientId"].AsString);
                    }
                    dataset.Next();
                }
            }
            catch (COMException exception)
            {
                _logger.LogError(exception.Message);
                if (bpApp != null)
                {
                    Process.GetProcessById(Convert.ToInt32(bpApp.GetAppProcessId())).Kill();
                    bpApp = null;
                }
            }
        }
        public void Dispose()
        {
            if (bpApp != null)
            {
                bpApp.DeInit();
                Process.GetProcessById(Convert.ToInt32(bpApp.GetAppProcessId())).Kill();
            }
        }

        public Application GetApplication()
        {
            return bpApp;
        }
    }
}