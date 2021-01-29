using BpNT;
using connectBase.Entities;
using connectBase.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace connectBase.Services
{
    public class COMConnection
    {
        private readonly string CONNECTION_NAME = "MBCOM IT-Systemhaus GmbH - MBCOM ConnectEGIS!";
        private readonly string CONNECTION_KEY = "GPVV2-8ESVD-4XZ9J-6BG23-K5D68-FZZUE";
        private Dictionary<string, Application> bpApps;
        private Dictionary<string, bool> locked;
        private ILogger<COMConnection> _logger;

        public COMConnection(ILogger<COMConnection> logger)
        {
            //Initialize logger
            _logger = logger;
            //Inititialize Dicitionaries
            InitializeDictionaries();
        }

        private void InitializeDictionaries()
        {
            _logger.LogInformation("COMAktiv Service Started");
            bpApps = new Dictionary<string, Application>();
            locked = new Dictionary<string, bool>();
        }

        public void Login(User user)
        {
            string key = GetKey(user.Username, AppSettings.MANDANT);
            if (bpApps.ContainsKey(key))
            {
                _logger.LogInformation("{0} already logged in to Mandant {1}", user.Username, AppSettings.MANDANT);
                throw new Exception("User " + user.Username + " already logged in to Mandant " + AppSettings.MANDANT);
            }
            else
            {
                try
                {
                    SchedulerService.LOCKED.Add(key, false);
                    locked.Add(key, false);
                    bpApps.Add(key, new Application());
                    bpApps[key].Init(
                                CONNECTION_NAME,
                                CONNECTION_KEY,
                                user.Username,
                                user.Password
                                );
                    bpApps[key].SelectMand(AppSettings.MANDANT);
                    _logger.LogInformation("{0} logged in to Mandant {1}", user.Username, AppSettings.MANDANT);
                }
                catch (COMException exception)
                {
                    _logger.LogError(exception.Message);
                    if (bpApps.ContainsKey(key))
                    {
                        Process.GetProcessById(Convert.ToInt32(bpApps[key].GetAppProcessId())).Kill();
                        bpApps.Remove(key);
                        locked.Remove(key);
                    }
                    throw exception;
                }
            }
        }

        public void SelectMandant(User user)
        {
            string key = GetKey(user.Username, AppSettings.MANDANT);
            bpApps[key].SelectMand(AppSettings.MANDANT);
        }

        public void Logout(string username, string mandant)
        {
            string key = GetKey(username, mandant);
            if (bpApps.ContainsKey(key))
            {
                bpApps[key].DeInit();
                Process.GetProcessById(Convert.ToInt32(bpApps[key].GetAppProcessId())).Kill();
                bpApps.Remove(key);
                locked.Remove(key);

                SchedulerService.LOCKED.Remove(key);
            }
            else
            {
                throw new Exception("There is no user with this name logged in");
            }
        }

        public Application GetUserApplication(User user)
        {
            var key = GetKey(user.Username, user.Mandant);
            return bpApps[key];
        }

        private string GetKey(string username, string mandant)
        {
            return username + mandant;
        }
    }
}