using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
