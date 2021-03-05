using System;

namespace connectBase.Services.Scheduler
{
    public class UserIsLockedException : SchedulerException
    {
        public UserIsLockedException()
        {
        }

        public UserIsLockedException(string message)
            : base(message)
        {
        }

        public UserIsLockedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
