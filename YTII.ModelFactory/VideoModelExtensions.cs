#region file_header

// QuickVideoInfo - YTII.ModelFactory - VideoModelExtensions.cs
// 
// Licensed to the Apache Software Foundation (ASF) under one or more contributor license agreements.  
// See the NOTICE file distributed with this work for additional information regarding copyright ownership.  
// The ASF licenses this file to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;

namespace YTII.ModelFactory
{
    internal static class VideoModelExtensions
    {
        /// <summary>
        ///     Returns large numbers in a nicer, short-hand format (e.g., 1.6M vs. 1615435, 5.6K vs. 5605)
        /// </summary>
        /// <param name="num">Number to pretty-ize</param>
        /// <returns>A pretty number as a string</returns>
        internal static string PrettyNumberString(this int? num)
        {
            var number = num ?? 0;

            if (number > 1000000)
                return Math.Floor(number / 1000000D) + "." + Math.Floor(number % 1000000D / 100000D) + "M";

            if (number > 1000)
                return Math.Floor(number / 1000D) + "." + Math.Floor(number % 1000D / 100D) + "K";

            return number.ToString();
        }

        /// <summary>
        ///     Converts a <see cref="TimeSpan" /> to a nicely formatted string by trimming excess leading zeros
        /// </summary>
        /// <param name="duration"><see cref="TimeSpan" /> to prettify</param>
        /// <returns>A prettier string</returns>
        internal static string PrettyTimeSpanString(this TimeSpan? duration)
        {
            var videoDurationString = (duration ?? TimeSpan.FromMinutes(0)).ToString().TrimStart('0', ':');
            switch (videoDurationString.Length)
            {
                case 0:
                    return @"0:00";
                case 1:
                    return @"0:0" + videoDurationString;
                case 2:
                    return @"0:" + videoDurationString;
                case 3:
                    return videoDurationString;
            }
            return videoDurationString;
        }
    }
}