using connectBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace connectBase.Helper
{
    public class AppSettings
    {
        // Secret for our JSON-WEB-TOKEN
        // We should change this to an external source (azure key vault, ...)
        private string _secret = "we-should-change-this";

        // We need this user for creating our scheme and validation files on startup
        // We should change this to a real service user so we don´t block other users connection
        private readonly User _serviceUser = new User("simon", "MbcoMTH2020", MANDANT);
        
        // Initial mandant setup 
        public static readonly string MANDANT = "1";

        // Configuration data for our scheduler to ensure user can only perform action after action
        // How often should we retry after waiting 
        private readonly int retryCount = 3;
        // How long do we wait after one retry
        private readonly int waitUntilRetry = 1000;

        public int getRetryCount()
        {
            return this.retryCount;
        }

        public int getWaitUntilRetry()
        {
            return this.waitUntilRetry;
        }

        public User getServiceUser()
        {
            return _serviceUser;
        }

        public string getSecret()
        {
            return _secret;
        }

        public void setSecret(string value)
        {
            _secret = value;
        }
    }
}
