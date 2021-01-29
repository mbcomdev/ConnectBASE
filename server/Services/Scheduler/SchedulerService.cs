using connectBase.Entities;
using connectBase.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace connectBase.Services
{
    public interface ISchedulerService
    {
        public bool IsLocked(User user);
        public void Interlock(User user);
        public void Release(User user);
    }

    public class SchedulerService: ISchedulerService
    {

        public static Dictionary<string, bool> LOCKED = new Dictionary<string, bool>();
        private ILogger<SchedulerService> Logger { get; }
        private readonly AppSettings _settings = new AppSettings();
        private readonly int retryCount;
        private readonly int waitUntilRetry;

        public SchedulerService(ILogger<SchedulerService> logger)
        {
            Logger = logger;
            retryCount = _settings.getRetryCount();
            waitUntilRetry = _settings.getWaitUntilRetry();
        }

        public bool IsLocked(User user)
        {
            if (SchedulerService.LOCKED.ContainsKey(GetKey(user)))
            {
                if (!SchedulerService.LOCKED[GetKey(user)])
                {
                    SchedulerService.LOCKED[GetKey(user)] = true;
                    return SchedulerService.LOCKED[GetKey(user)];
                } else
                {
                    try
                    {
                        Retry(user);
                        SchedulerService.LOCKED[GetKey(user)] = true;
                        return SchedulerService.LOCKED[GetKey(user)];
                    } catch (Scheduler.UserIsLockedException e)
                    {
                        throw e;
                    }
                }
            } else
            {
                throw new Scheduler.SchedulerException("LOCKED Dictionary does not contain user key.");
            }
        }

        public void Interlock(User user) {
            SchedulerService.LOCKED[GetKey(user)] = true;
        }

        public void Release(User user)
        {
            SchedulerService.LOCKED[GetKey(user)] = false;
        }

        public string GetKey(User user)
        {
            return user.Username + user.Mandant;
        }

        private void Retry(User user)
        {
            int tryCount = this.retryCount;
            if (tryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(tryCount));

            while (true)
            {
                try
                {
                    CheckLockState(user);
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

        private void CheckLockState(User user)
        {
            if (SchedulerService.LOCKED[GetKey(user)] == true)
            {
                throw new Scheduler.UserIsLockedException("User is currently performing an action. Please wait until last action is finished.");
            }
        }
    }
}
