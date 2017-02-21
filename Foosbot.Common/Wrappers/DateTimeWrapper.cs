﻿using Foosbot.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foosbot.Common.Wrappers
{
    /// <summary>
    /// Wrapper for DateTime
    /// </summary>
    public class DateTimeWrapper:ITime
    {
        /// <summary>
        /// Stopwatch Instance
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start">Starts the stopwatch, if true [false] by default</param>
        public DateTimeWrapper(bool start = false)
        {
            if (start)
            {
                Start();
            }
        }

        /// <summary>
        /// Start stopwatch
        /// </summary>
        public void Start()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stop stopwatch
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Gets Elapsed Time from the stopwatch
        /// </summary>
        /// <returns>Time passed since started the stopwatch TimeSpan</returns>
        public TimeSpan Elapsed
        {
            get
            {
                return _stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Current DateTime
        /// </summary>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

    }
}
