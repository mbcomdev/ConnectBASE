using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace connectBase.Services
{
    public interface ISchedulerService
    {
        public bool IsLocked();
        public void Interlock();
        public void Release();
    }

    public class SchedulerService : ISchedulerService
    {

        public static bool LOCKED = false;
        private ILogger<SchedulerService> Logger { get; }
        private readonly int retryCount;
        private readonly int waitUntilRetry;

        public SchedulerService(ILogger<SchedulerService> logger, IConfiguration configuration)
        {
            Logger = logger;
            retryCount = int.Parse(configuration["retry_count"]);                    
            waitUntilRetry = int.Parse(configuration["wait_until_retry"]);
        }

        public bool IsLocked()
        {
            if (!SchedulerService.LOCKED)
            {
                SchedulerService.LOCKED = true;
                return SchedulerService.LOCKED;
            }
            else
            {
                try
                {
                    Retry();
                    SchedulerService.LOCKED = true;
                    return SchedulerService.LOCKED;
                }
                catch (Scheduler.UserIsLockedException e)
                {
                    throw e;
                }
            }
        }

        public void Interlock()
        {
            SchedulerService.LOCKED = true;
        }

        public void Release()
        {
            SchedulerService.LOCKED = false;
        }

        private void Retry()
        {
            int tryCount = this.retryCount;
            if (tryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(tryCount));

            while (true)
            {
                try
                {
                    CheckLockState();
                    break; // success!
                }
                catch (Scheduler.UserIsLockedException e)
                {
                    if (--tryCount == 0)
                        throw e;
                    Thread.Sleep(waitUntilRetry);
                }
            }
        }

        private void CheckLockState()
        {
            if (SchedulerService.LOCKED == true)
            {
                throw new Scheduler.UserIsLockedException("User is currently performing an action. Please wait until last action is finished.");
            }
        }
    }
}
