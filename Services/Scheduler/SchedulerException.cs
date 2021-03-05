﻿using System;

namespace connectBase.Services.Scheduler
{
    public class SchedulerException : Exception
    {
        public SchedulerException()
        {
        }

        public SchedulerException(string message)
            : base(message)
        {
        }

        public SchedulerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
