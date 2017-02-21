﻿// **************************************************************************************
// **																				   **
// **		(C) FOOSBOT - Final Software Engineering Project, 2015 - 2016			   **
// **		(C) Authors: M.Toubian, M.Shimon, E.Kleinman, O.Sasson, J.Gleyzer          **
// **			Advisors: Mr.Resh Amit & Dr.Hoffner Yigal							   **
// **		The information and source code here belongs to Foosbot project			   **
// **		and may not be reproduced or used without authors explicit permission.	   **
// **																				   **
// **************************************************************************************

using EasyLog;
using Foosbot.Common.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Foosbot
{
    /// <summary>
    /// Configuration Multithreaded Singleton
    /// Using this, configuration attributes
    /// can be easily accessed from any place in solution.
    /// </summary>
    public sealed partial class Configuration
    {
        /// <summary>
        /// Configuration File Path Const
        /// </summary>
        private const string CONFIG_FILE_PATH = "..//config.ini";

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static volatile Configuration _attributes;

        /// <summary>
        /// Synchronization Token for Multithreading
        /// </summary>
        private static object syncToken = new Object();

        /// <summary>
        /// Attributes stored localy for fast access.
        /// Dictionary contains key and value.
        /// </summary>
        private Dictionary<string, string> _local;

        /// <summary>
        /// Get the number of attributes in configuration file
        /// </summary>
        public int Count
        {
            get
            {
                if (_local == null) 
                    return 0;
                return _local.Count;
            }
        }

        /// <summary>
        /// Singleton Constructor
        /// </summary>
        private Configuration() 
        {
            Load();
        }

        /// <summary>
        /// Singleton Instance Properry
        /// </summary>
        public static Configuration Attributes
        {
            get
            {
                if (_attributes == null || _attributes.Count < 1)
                {
                    lock(syncToken)
                    {
                        if (_attributes == null || _attributes.Count < 1)
                            _attributes = new Configuration();
                    }
                }
                return _attributes;
            }
        }
        

        /// <summary>
        /// Loading configuration file
        /// Used only once in constructor
        /// </summary>
        private void Load()
        {
            _local = new Dictionary<string, string>();
            try
            {
                using (StreamReader file = new StreamReader(CONFIG_FILE_PATH, Encoding.ASCII))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        //ignore comments
                        if (!line.StartsWith("#") && !String.IsNullOrEmpty(line))
                        {
                            //replace whitespaces and split to key and value
                            string[] split = line.Replace(" ", "").Split('=', '#');
                            if (split.Length >= 2)
                                _local.Add(split[0], split[1]);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.Print(String.Format("Failed to load configuration file: [{0}] Reason: {1}",
                    CONFIG_FILE_PATH, e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Is key exist in attributes
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <returns>[True] if exist, [False] - otherwise</returns>
        public bool IsKeyExist(string key)
        {
            return _local.ContainsKey(key);
        }

        /// <summary>
        /// Get string value for provided key
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <exception cref="ConfigurationException">In case key does not exist.</exception>
        /// <returns>Value for given key</returns>
        public string GetValue(string key)
        {
            if (!_local.ContainsKey(key))
                throw new ConfigurationException(String.Format("Key [{0}] not exist in configuration attributes.", key));
            return _local[key];
        }

        /// <summary>
        /// Get generic value for provided key
        /// </summary>
        /// <typeparam name="T">Type of value to return. 
        /// Supported types are: string, int, double and bool</typeparam>
        /// <param name="key">Key for value</param>
        /// <returns>Value for given key</returns>
        /// <exception cref="NotSupportedException">In case requested type T is not supported.</exception>
        public T GetValue<T>(string key)
        {
            string value = GetValue(key);
            Type requested = typeof(T);

            if (requested == typeof(string))
                return (T)Convert.ChangeType(value, requested);
            else if (typeof(T) == typeof(int))
                return (T)Convert.ChangeType(Convert.ToInt32(value),requested);
            else if (typeof(T) == typeof(bool))
                return (T)Convert.ChangeType(Convert.ToBoolean(value), requested);
            else if (typeof(T) == typeof(double))
                return (T)Convert.ChangeType(Convert.ToDouble(value), requested);
            else
                throw new NotSupportedException(String.Format("Requested type [{0}] is not supported", requested));
        }
    }
}
