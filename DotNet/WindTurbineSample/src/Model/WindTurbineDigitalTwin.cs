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
using System.Collections.Generic;

using Scaleout.Streaming.DigitalTwin.Core;

namespace Scaleout.Streaming.DigitalTwin.Samples.WindTurbine
{
	/// <summary>
	/// Digital twin object that derives from ScaleOut 
	/// <see cref="DigitalTwinBase"/> abstract base class. Users can
	/// derive other classes from this one (e.g. to support twin hierarchy).
	/// </summary>
	public class WindTurbineDigitalTwin : DigitalTwinBase
	{
		// The upper cap for the number of messages we store for each DT instance
		public static int MaxNumberOfMessagesToKeep = 2000;
		public const string DigitalTwinModelType = "windturbine";

		public const int MaxAllowedTemperature = 100; // in Celsius
		public TimeSpan MaxTimeHighTemperatureAllowed = TimeSpan.FromSeconds(20);
		public TimeSpan MaxTimeHighTemperatureAllowedPreMaint = TimeSpan.FromSeconds(10);

		public const int MinAllowedRPM = 60;
		public TimeSpan MaxTimeLowRPMAllowed = TimeSpan.FromSeconds(20);
		public TimeSpan MaxTimeLowRPMAllowedPreMaint = TimeSpan.FromSeconds(10);

		public static readonly TimeSpan MaxAlertFrequency = TimeSpan.FromSeconds(30);
		static Random _rand = new Random();

		/// <summary>Type of the wind turbine.</summary>
		public WindTurbineModel TurbineModel { get; set; } = WindTurbineModel.ModelA;

		/// <summary>
		/// When being set to true, indicates that the current wind turbine's temperature is above allowed
		/// threshold (specified above).
		/// </summary>
		public bool TrackingHighTemperature { get; set; }

		/// <summary>When being set to true, indicates that the current wind turbine's RPM is below normal.</summary>
		public bool TrackingLowRPM { get; set; }

		/// <summary>Starting time for tracking a high temperature condition.</summary>
		public DateTime HighTempStartTime { get; set; }

		/// <summary>Starting time for tracking a low RPM condition.</summary>
		public DateTime LowRPMStartTime { get; set; }

		/// <summary>Number of received messages in a row with turbine engine's temperature above normal.</summary>
		public int NumberMsgsWithOverTemp { get; set;}

		/// <summary>Number of received messages in a row with turbine's RPM speed below normal.</summary>
		public int NumberMsgsWithLowRPM { get; set; }

		/// <summary>Next maintenance date for this wind turbine (set it here for 1 month ahead).</summary>
		public DateTime NextMaintDate { get; set; } = new DateTime().AddMonths(1);

		/// <summary>The list of processed messages by this digital twin.</summary>
		public List<DeviceTelemetry> MessageList { get; } = new List<DeviceTelemetry>();

		/// <summary>List of all captured incidents for this DT instance.</summary>
		public List<Incident> IncidentList { get; } = new List<Incident>();

		/// <summary>State where each wind turbine is located.</summary>
		public string State { get; set; }

		///-----------------------------------------------------------------------------
		/// Below are some aggregates we calculate using ScaleOut Time Windowing library 
		/// and the list of processed telemetry messages:
		///-----------------------------------------------------------------------------

		/// <summary>Average number of processed messages per hour by this digital twin instance.</summary>
		public double AverageProcessedMessagesPerHour { get; set; }

		/// <summary>Average RPM speed per hour.</summary>
		public double AverageRPM { get; set; }

		/// <summary>Average engine temperature per hour.</summary>
		public double AverageTemperaturePerHour { get; set; }

		/// <summary>
		/// Illustrates how a digital twin specific initialization logic 
		/// can be added here, which will be called once at the object creation time.
		/// </summary>
		/// <param name="id">Digital twin identifier.</param>
		/// <param name="model">Digital twin model type.</param>
		public override void Init(string id, string model)
		{
			base.Init(id, model);
			int rand;
			lock (_rand)
			{
				rand = _rand.Next(0, 3);
			}

			switch (rand)
			{
				case 0:
					State = "WA";
					break;
				case 1:
					State = "OR";
					break;
				case 2:
					State = "CA";
					break;
				default:
					State = "TX";
					break;
			}
		}
	}
}
