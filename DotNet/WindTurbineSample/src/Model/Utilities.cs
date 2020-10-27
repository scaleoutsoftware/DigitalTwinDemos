/* 
 * © Copyright 2017-2020 by ScaleOut Software, Inc.
 *
 * LICENSE AND DISCLAIMER
 * ----------------------
 * This material contains sample programming source code ("Sample Code").
 * ScaleOut Software, Inc. (SSI) grants you a nonexclusive license to compile, 
 * link, run, display, reproduce, and prepare derivative works of 
 * this Sample Code.  The Sample Code has not been thoroughly
 * tested under all conditions.  SSI, therefore, does not guarantee
 * or imply its reliability, serviceability, or function. SSI
 * provides no support services for the Sample Code.
 *
 * All Sample Code contained herein is provided to you "AS IS" without
 * any warranties of any kind. THE IMPLIED WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGMENT ARE EXPRESSLY
 * DISCLAIMED.  SOME JURISDICTIONS DO NOT ALLOW THE EXCLUSION OF IMPLIED
 * WARRANTIES, SO THE ABOVE EXCLUSIONS MAY NOT APPLY TO YOU.  IN NO 
 * EVENT WILL SSI BE LIABLE TO ANY PARTY FOR ANY DIRECT, INDIRECT, 
 * SPECIAL OR OTHER CONSEQUENTIAL DAMAGES FOR ANY USE OF THE SAMPLE CODE
 * INCLUDING, WITHOUT LIMITATION, ANY LOST PROFITS, BUSINESS 
 * INTERRUPTION, LOSS OF PROGRAMS OR OTHER DATA ON YOUR INFORMATION
 * HANDLING SYSTEM OR OTHERWISE, EVEN IF WE ARE EXPRESSLY ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGES.
 */
using System;
using System.Linq;
using System.Collections.Generic;

namespace Scaleout.Streaming.DigitalTwin.Samples.WindTurbine
{
	/// <summary>
	/// Specifies what message to generate.
	/// </summary>
	public enum MessageType
	{
		RandomValues,
		Normal,
		HighTemperature,
		LowRPM
	}

	public static class Toolbox
    {
        public static void AddTimeOrdered<T>(this IList<T> source, T item, Func<T, DateTime> timestampSelector)
        {
            DateTime timestamp = timestampSelector(item);
            int insertPosition = source.Count;
            while (insertPosition > 0)
            {
                if (timestamp < timestampSelector(source[insertPosition - 1]))
                    insertPosition--;
                else
                    break;
            }

            source.Insert(insertPosition, item);
        }

		static readonly Random _random = new Random();

		/// <summary>
		/// Creates a new instance of <see cref="DeviceTelemetry"/> class with
		/// random member values.
		/// </summary>
		/// <param name="forMultipleDevices">When used in the loop, this parameter controls
		/// whether the lat/long should be the same (if it is true) or randomly generated for each call.</param>
		/// <param name="timeWindow">The time window interval where the message timestamp should be generated in.</param>
		/// <returns>New instance of <see cref="DeviceTelemetry"/> class.</returns>
		public static DeviceTelemetry CreateDeviceMessage(bool forMultipleDevices, TimeSpan timeWindow, MessageType msgType)
		{
			var maxStatusValueAsInt = Enum.GetValues(typeof(DeviceStatus)).Cast<int>().Max();
			DeviceStatus status = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), _random.Next(0, maxStatusValueAsInt).ToString());

			// Latitude and longitude for Seattle (will be used for generating messages for a single device):
			decimal latSeattle = 47.6M;
			decimal longSeattle = -122.34M;

			var latitude = (forMultipleDevices) ? _random.Next(-100, 100) : latSeattle;
			var longitude = (forMultipleDevices) ? _random.Next(-100, 100) : longSeattle;

			int rpm = 0;
			int temp = 0;

			switch (msgType)
			{
				case MessageType.RandomValues:
					rpm = _random.Next(45, 120);
					temp = _random.Next(70, 120);
					break;
				case MessageType.Normal:
					rpm = _random.Next(WindTurbineDigitalTwin.MinAllowedRPM + 1, 120);
					temp = _random.Next(70, WindTurbineDigitalTwin.MaxAllowedTemperature);
					break;
				case MessageType.HighTemperature:
					rpm = _random.Next(WindTurbineDigitalTwin.MinAllowedRPM + 1, 120);
					temp = _random.Next(WindTurbineDigitalTwin.MaxAllowedTemperature + 1, 120);
					break;
				case MessageType.LowRPM:
					rpm = _random.Next(45, WindTurbineDigitalTwin.MinAllowedRPM - 1);
					temp = _random.Next(70, WindTurbineDigitalTwin.MaxAllowedTemperature);
					break;
			}

			var endTime = DateTime.UtcNow;
			var startTime = endTime.Subtract(timeWindow);
			var created = GetRandomValueFromTimeRange(startTime, endTime);

			return new DeviceTelemetry
			{
				Latitude = latitude,
				Longitude = longitude,
				Status = status,
				RPMSpeed = rpm,
				Temp = temp,
				Timestamp = created
			};
		}

		/// <summary>
		/// Generates a random <see cref="DateTime"/> value from
		/// the specified time interval.
		/// </summary>
		/// <param name="fromDate">The <see cref="DateTime"/> value, where the time interval starts.</param>
		/// <param name="toDate">The <see cref="DateTime"/> value, where the time interval ends.</param>
		/// <returns>Generated value from the specified time interval.</returns>
		public static DateTime GetRandomValueFromTimeRange(DateTime fromDate, DateTime toDate)
		{
			// Get the TimeSpan for the date/time range
			var range = toDate - fromDate;
			// Calculate a random TimeSpan interval within the specified range
			var randTimeSpan = new TimeSpan((long)(range.Ticks * _random.NextDouble()));
			// Return a random date within the range
			return fromDate + randTimeSpan;
		}
	}
}
