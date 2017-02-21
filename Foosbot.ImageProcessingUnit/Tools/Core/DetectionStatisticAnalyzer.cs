﻿// **************************************************************************************
// **																				   **
// **		(C) FOOSBOT - Final Software Engineering Project, 2015 - 2016			   **
// **		(C) Authors: M.Toubian, M.Shimon, E.Kleinman, O.Sasson, J.Gleyzer          **
// **			Advisors: Mr.Resh Amit & Dr.Hoffner Yigal							   **
// **		The information and source code here belongs to Foosbot project			   **
// **		and may not be reproduced or used without authors explicit permission.	   **
// **																				   **
// **************************************************************************************

using Foosbot.Common.Contracts;
using Foosbot.Common.Wrappers;
using Foosbot.ImageProcessingUnit.Tools.Contracts;
using System;
using System.Diagnostics;

namespace Foosbot.ImageProcessingUnit.Tools.Core
{
    /// <summary>
    /// Detection Statistical Analyzer for image processing unit
    /// </summary>
    public class DetectionStatisticAnalyzer : IDetectionAnalyzer
    {
        #region Private Members

        /// <summary>
        /// Current Working Second time stamp
        /// </summary>
        private DateTime _currenWorkingSecond;

        /// <summary>
        /// Total Frames received in last second
        /// </summary>
        private int _totalFramesPerSecond;

        /// <summary>
        /// Total Successful detection for last second
        /// </summary>
        private int _successDetectionFrame;

        /// <summary>
        /// Time spent on ball location detection in past second
        /// </summary>
        private TimeSpan _spentOnDetectionInSecond;

        /// <summary>
        /// Instance of timing object
        /// </summary>
        private ITime _dateTime;

        #endregion Private Members

        /// <summary>
        /// Detection Rate in percent
        /// </summary>
        public double DetectionRate { get; private set; }

        /// <summary>
        /// Average Time taken for detection
        /// </summary>
        public double AverageDetectionTime { get; private set; }

        /// <summary>
        /// Frames per second then detection was successful
        /// </summary>
        public int DetectedFPS { get; private set; }

        /// <summary>
        /// Total frames received per second
        /// </summary>
        public int TotalFPS { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_dateTime">Instance of DateTimeWrapper</param>
        public DetectionStatisticAnalyzer(ITime dateTime = null)
        {
            _dateTime = dateTime ?? new DateTimeWrapper();
            _currenWorkingSecond = DateTime.Now;
        }

        /// <summary>
        /// Steps to perform each detection started
        /// 1. Count frame
        /// 2. Start detection stopwatch
        /// If not same second as in previous frame then update statistics and start from the beginning
        /// </summary>
        public void Begin()
        {
            DateTime now = _dateTime.Now;
            if (_currenWorkingSecond.Second != now.Second)
            {
                DetectionRate = (_totalFramesPerSecond < 1) ? 100 : 100 * _successDetectionFrame / _totalFramesPerSecond;
                AverageDetectionTime = (_totalFramesPerSecond < 1) ? 0 : _spentOnDetectionInSecond.Milliseconds / _totalFramesPerSecond;
                DetectedFPS = _successDetectionFrame;
                TotalFPS = _totalFramesPerSecond;

                Statistics.TryUpdateBasicImageProcessingInfo(String.Format("Detection: Rate {0}% ({1}/{2}) Average T {3}(ms)",
                        DetectionRate, DetectedFPS, TotalFPS, AverageDetectionTime));

                _totalFramesPerSecond = 0;
                _successDetectionFrame = 0;
                _spentOnDetectionInSecond = TimeSpan.FromMilliseconds(0);
                _currenWorkingSecond = now;
            }
            _totalFramesPerSecond++;
            _dateTime.Start();
        }

        /// <summary>
        /// Steps to perform after each detection finished
        /// 1. Count detection if successful
        /// 2. Stop the detection stopwatch
        /// </summary>
        /// <param name="isBallLocationFound">Detection result</param>
        public void Finalize(bool isBallLocationFound)
        {
            _dateTime.Stop();
            //Stopwatch 
            _spentOnDetectionInSecond += _dateTime.Elapsed;
            if (isBallLocationFound)
                _successDetectionFrame++;
        }
    }
}
