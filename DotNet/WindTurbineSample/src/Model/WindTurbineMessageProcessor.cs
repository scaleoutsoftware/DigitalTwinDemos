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
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using Scaleout.Streaming.DigitalTwin.Core;
using Scaleout.Streaming.TimeWindowing.Linq;

namespace Scaleout.Streaming.DigitalTwin.Samples.WindTurbine
{
	/// <summary>
	/// User defined <see cref="MessageProcessor"/> class which inherits to
	/// ScaleOut <see cref="ScaleOut.Streaming.DigitalTwin.Model.MessageProcessor{TDigitalTwin, TMessage}"/> 
	/// abstract base class. ScaleOut digital twin processing engine calls the ProcessMessages method when new messages 
	/// come to a digital twin object which is registered together with this message processor class.
	/// </summary>
	public class WindTurbineMessageProcessor : MessageProcessor<WindTurbineDigitalTwin, DeviceTelemetry>
	{
		static Dictionary<WindTurbineModel, TimeSpan> _preMaintenancePeriod;
		static bool LogAlerts = false;
	
		/// <summary>
		/// Static constructor.
		/// </summary>
		static WindTurbineMessageProcessor()
		{
			_preMaintenancePeriod = new Dictionary<WindTurbineModel, TimeSpan>();
			// Populate the dictionary with time period for each model
			_preMaintenancePeriod.Add(WindTurbineModel.ModelA, TimeSpan.FromDays(90));
			_preMaintenancePeriod.Add(WindTurbineModel.ModelB, TimeSpan.FromDays(120));
			_preMaintenancePeriod.Add(WindTurbineModel.ModelC, TimeSpan.FromDays(150));
		}

		/// <summary>
		/// This method is called by a message processor engine to pass new messages
		/// to the specified instance of a digital twin object.
		/// </summary>
		/// <param name="context">The <see cref="ProcessingContext"/> for current operation.</param>
		/// <param name="dt">The target digital twin object.</param>
		/// <param name="newMessages">New messages for processing.</param>
		/// <returns><see cref="ProcessingResult.DoUpdate"/> when digital twin object needs to be updated
		/// and <see cref="ProcessingResult.NoUpdate"/> when no updates are needed.</returns>
		public override ProcessingResult ProcessMessages(ProcessingContext				context,
														 WindTurbineDigitalTwin			dt, 
														 IEnumerable<DeviceTelemetry>	newMessages)
		{
			var result = ProcessingResult.DoUpdate; // Forcing to always update digital twin object

			bool truncateMsgList = false;
			if (dt.MessageList.Count >= WindTurbineDigitalTwin.MaxNumberOfMessagesToKeep)
				truncateMsgList = true;

			// Determine if we are in the pre-maintenance period for our wind turbine model:
			var preMaintTimePeriod = _preMaintenancePeriod[dt.TurbineModel];
			bool isInPreMaintenancePeriod = ((dt.NextMaintDate - DateTime.UtcNow) < preMaintTimePeriod) ? true : false;

			// 
			// Process incoming messages
			//
			foreach (var msg in newMessages)
			{
				// Send the test alert to the data source
				if (msg.Latitude == 100)
				{
					var alert = new Alert();

					alert.IncidentType = IncidentType.HighRPM;
					alert.Timestamp = DateTime.UtcNow;
					alert.Duration = TimeSpan.FromMinutes(11);
					alert.IsInPreMaintenancePeriod = false;
					alert.NumberWarningMessagesReceived = 11;
					alert.DigitalTwinId = dt.Id;

					// Send message back to device (it could be a command to shut down the turbine)
					context.SendToDataSource(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(alert)));
				}

				if (msg.Latitude == 101) // Log the test trace message
					context.LogMessage(severity: LogSeverity.Informational, message: $"Test message logged at {DateTime.Now.ToString("o")}");

				var res1 = TrackForHighTemperature(context, dt, msg, isInPreMaintenancePeriod);

				var res2 = TrackForLowRPM(context, dt, msg, isInPreMaintenancePeriod);

				if (res1 == ProcessingResult.DoUpdate || res2 == ProcessingResult.DoUpdate)
					result = ProcessingResult.DoUpdate;

				// Store incoming messages so we can calculate some aggregates below
				dt.MessageList.AddTimeOrdered(msg, element => element.Timestamp);
			}

			// Use ScaleOut Time Windowing Library to calculate averages for the last 24 hours:
			// - average number of processed messages; 
			// - average PM speed per hour;
			// - average temperature per hour.
			var stats = CalculateAveNumbersPerHour(dt.MessageList);

			dt.AverageProcessedMessagesPerHour = stats.aveMsgNumber;
			dt.AverageRPM = stats.aveRPM;
			dt.AverageTemperaturePerHour = stats.aveTemperature;

			if (truncateMsgList)
			{
				int msgCountToRemove = (WindTurbineDigitalTwin.MaxNumberOfMessagesToKeep - 1000 > 0) ? 1000 : 100;
				dt.MessageList.RemoveRange(0, msgCountToRemove);
			}

			return result;
		}

		/// <summary>
		/// Helper method which implements high temperature tracking algorithm.
		/// </summary>
		/// <param name="context">The <see cref="ProcessingContext"/> for current operation.</param>
		/// <param name="dt">The target digital twin object.</param>
		/// <param name="message">The currently processing message.</param>
		/// <param name="isInPreMaintenancePeriod">Indicates whether device is in pre-maintenance period.</param>
		/// <returns><see cref="ProcessingResult.DoUpdate"/> when digital twin object needs to be updated
		/// and <see cref="ProcessingResult.NoUpdate"/> when no updates are needed.</returns>
		private ProcessingResult TrackForHighTemperature(ProcessingContext context, WindTurbineDigitalTwin dt, DeviceTelemetry msg, bool isInPreMaintenancePeriod)
		{
			ProcessingResult result = ProcessingResult.NoUpdate;

			// If this is a high temp indication, record and analyze it
			if (msg.Temp > WindTurbineDigitalTwin.MaxAllowedTemperature)
			{
				dt.NumberMsgsWithOverTemp++;

				if (!dt.TrackingHighTemperature) // first time the message indicates the temp is out of norm
				{
					dt.TrackingHighTemperature	= true;
					dt.HighTempStartTime		= DateTime.UtcNow;

					// Add an incident notification to the incident list:
					dt.IncidentList.Add(new Incident() { IncidentPhase = IncidentPhase.Started, IncidentType = IncidentType.HighTemperature,
					 									 Timestamp = dt.HighTempStartTime, MetricValue = msg.Temp });
				}

				TimeSpan duration = DateTime.UtcNow - dt.HighTempStartTime;

				if (duration > dt.MaxTimeHighTemperatureAllowed ||
					  (isInPreMaintenancePeriod && duration > dt.MaxTimeHighTemperatureAllowedPreMaint))
				{
					// See how long it's been since the last alert
					var lastAlertTime = DateTime.MinValue;
					var lastAlert = dt.IncidentList.LastOrDefault();
					if (lastAlert != null)
						lastAlertTime = lastAlert.Timestamp;

					if (dt.HighTempStartTime == lastAlert?.Timestamp || DateTime.UtcNow.Subtract(lastAlertTime).CompareTo(WindTurbineDigitalTwin.MaxAlertFrequency) > 0) 
					{
						// If we have exceeded the max allowed time for high temperature condition, send an alert:
						var alert = new Alert();

						alert.IncidentType = IncidentType.HighTemperature;
						alert.Timestamp = DateTime.UtcNow;
						alert.Duration = duration;
						alert.IsInPreMaintenancePeriod = isInPreMaintenancePeriod;
						alert.NumberWarningMessagesReceived = dt.NumberMsgsWithOverTemp;
						alert.DigitalTwinId = dt.Id;

						// Send message back to device (it could be a command to shut down the turbine)
						context.SendToDataSource(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(alert)));
						if (LogAlerts)
							WriteAlertToFile(context, alert, activeIncident: true);

						// Add an incident notification to the incident list:
						dt.IncidentList.Add(new Incident() { IncidentPhase = IncidentPhase.AlertSent, IncidentType = IncidentType.HighTemperature,
															 Timestamp = DateTime.UtcNow, MetricValue = msg.Temp });
                    }
				}

				result = ProcessingResult.DoUpdate;
			}
			else
			{
				// If we were tracking high temperature events, close it now:
				if (dt.TrackingHighTemperature)
				{
                    Alert alert = new Alert();
                    alert.IncidentType = IncidentType.HighTemperature;
                    alert.Timestamp = DateTime.UtcNow;
                    alert.IsInPreMaintenancePeriod = isInPreMaintenancePeriod;
                    alert.NumberWarningMessagesReceived = dt.NumberMsgsWithOverTemp;
					alert.DigitalTwinId = dt.Id;

					dt.TrackingHighTemperature = false;
					dt.NumberMsgsWithOverTemp = 0;

					// Add an incident notification to the incident list:
					dt.IncidentList.Add(new Incident() { IncidentPhase = IncidentPhase.Resolved, IncidentType = IncidentType.HighTemperature,
														 Timestamp = DateTime.UtcNow, MetricValue = msg.Temp });
					if (LogAlerts)
						WriteAlertToFile(context, alert, activeIncident: false);

					result = ProcessingResult.DoUpdate;
				}
			}

			return result;
		}

		/// <summary>
		/// Helper method which implements a tracking algorithm to detect and act on low RPM signaled by turbine.
		/// </summary>
		/// <param name="context">The <see cref="ProcessingContext"/> for current operation.</param>
		/// <param name="dt">The target digital twin object.</param>
		/// <param name="message">The currently processing message.</param>
		/// <param name="isInPreMaintenancePeriod">Indicates whether device is in pre-maintenance period.</param>
		/// <returns><see cref="ProcessingResult.DoUpdate"/> when digital twin object needs to be updated
		/// and <see cref="ProcessingResult.NoUpdate"/> when no updates are needed.</returns>
		private ProcessingResult TrackForLowRPM(ProcessingContext context, WindTurbineDigitalTwin dt, DeviceTelemetry msg, bool isInPreMaintenancePeriod)
		{
			ProcessingResult result = ProcessingResult.NoUpdate;

			// If RPM is below allowed threshold, record and analyze it
			if (msg.RPMSpeed < WindTurbineDigitalTwin.MinAllowedRPM)
			{
				dt.NumberMsgsWithLowRPM++;

				if (!dt.TrackingLowRPM) // first time the message indicates that RPM is out of norm
				{
					dt.TrackingLowRPM = true;
					dt.LowRPMStartTime = DateTime.UtcNow;

					// Add an incident notification to the incident list:
					dt.IncidentList.Add(new Incident() { IncidentPhase = IncidentPhase.Started, IncidentType = IncidentType.LowRPM,
														 Timestamp = dt.LowRPMStartTime, MetricValue = msg.RPMSpeed});
				}

				TimeSpan duration = DateTime.UtcNow - dt.LowRPMStartTime;

				if (duration > dt.MaxTimeLowRPMAllowed ||
					  (isInPreMaintenancePeriod && duration > dt.MaxTimeLowRPMAllowedPreMaint))
				{
                    // See how long it's been since the last alert
                    var lastAlertTime = DateTime.MinValue;
                    var lastAlert = dt.IncidentList.LastOrDefault();
                    if (lastAlert != null)
                        lastAlertTime = lastAlert.Timestamp;
					if (dt.LowRPMStartTime == lastAlert?.Timestamp || DateTime.UtcNow.Subtract(lastAlertTime).CompareTo(WindTurbineDigitalTwin.MaxAlertFrequency) > 0)
					{
						// If we have exceeded the max allowed time for low RPM condition, send an alert:
						var alert = new Alert();

						alert.IncidentType = IncidentType.LowRPM;
						alert.Duration = duration;
						alert.IsInPreMaintenancePeriod = isInPreMaintenancePeriod;
						alert.NumberWarningMessagesReceived = dt.NumberMsgsWithLowRPM;
						alert.Timestamp = DateTime.UtcNow;
						alert.DigitalTwinId = dt.Id;

						// Send message back to device (it could be a command to shut down the turbine)
						context.SendToDataSource(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(alert)));
						if (LogAlerts)
							WriteAlertToFile(context, alert, activeIncident: true);

						// Add an incident notification to the incident list:
						dt.IncidentList.Add(new Incident()
						{
							IncidentPhase = IncidentPhase.AlertSent,
							IncidentType = IncidentType.LowRPM,
							Timestamp = DateTime.UtcNow,
							MetricValue = msg.RPMSpeed
						});
					}
				}

				result = ProcessingResult.DoUpdate;
			}
			else
			{
				// If we were tracking low RPM events, close it now:
				if (dt.TrackingLowRPM)
				{
					var alert = new Alert();

					alert.IncidentType = IncidentType.LowRPM;
					alert.Timestamp = DateTime.UtcNow;
					alert.IsInPreMaintenancePeriod = isInPreMaintenancePeriod;
					alert.NumberWarningMessagesReceived = dt.NumberMsgsWithLowRPM;
					alert.DigitalTwinId = dt.Id;

					dt.TrackingLowRPM = false;
					dt.NumberMsgsWithLowRPM = 0;

					// Add an incident notification to the incident list:
					dt.IncidentList.Add(new Incident() { IncidentPhase = IncidentPhase.Resolved, IncidentType = IncidentType.LowRPM,
														 Timestamp = DateTime.UtcNow, MetricValue = msg.RPMSpeed });
					if (LogAlerts)
						WriteAlertToFile(context, alert, activeIncident: false);

					result = ProcessingResult.DoUpdate;
				}
			}

			return result;
		}


		/// <summary>
		/// Helper method that calculates average number of processed messages as well as 
		/// average RPM speed and wind turbine engine's temperature per hour in the last 24 hours.
		/// </summary>
		/// <param name="messages">Ordered list of all messages.</param>
		/// <returns>Calculated average numbers of processed messages, RPM speed and temperature per hour.</returns>
		private (double aveMsgNumber, double aveRPM, double aveTemperature) CalculateAveNumbersPerHour(IEnumerable<DeviceTelemetry> messages)
		{
			if (messages.Count() == 0)
				return (0d, 0d, 0d);

			var endTime		= DateTime.UtcNow;
			var startTime	= endTime.Subtract(TimeSpan.FromHours(24)); // last 24 hours
			
			// Transform list of messages into enumerable collection of 1 hour windows
			var tumblingWindows = messages.ToTumblingWindows(elem => elem.Timestamp, startTime, endTime, TimeSpan.FromHours(1));

			// Filter out empty time windows and call ToList() -- without ToList() we'd recalculate the time windows every time we
			// enumerate (or call Average(), as we're doing below):
			var occupiedWindows = tumblingWindows.Where(win => win.Count > 0).ToList();

			double? aveMsgNumberPerHour = null, aveRPMSpeedPerHour = null, aveTemperaturePerHour = null;
			if (occupiedWindows.Count() > 0)
			{
				// Calculate average number of processed messages per hour
				aveMsgNumberPerHour = occupiedWindows.Average(win => win.Count);
				// Calculate average RPM speed per hour
				aveRPMSpeedPerHour = occupiedWindows.Average(msgs => msgs.Select(y => (long)y.RPMSpeed).Sum() / (msgs.Count == 0 ? 1 : msgs.Count));
				// Calculate average temperature per hour
				aveTemperaturePerHour = occupiedWindows.Average(msgs => msgs.Select(y => (long)y.Temp).Sum() / (msgs.Count == 0 ? 1 : msgs.Count));
			}

			return (aveMsgNumberPerHour		!= null ? (double) aveMsgNumberPerHour	: 0d,
					aveRPMSpeedPerHour      != null ? (double) aveRPMSpeedPerHour	: 0d,
					aveTemperaturePerHour	!= null ? (double) aveTemperaturePerHour: 0d);
		}

		void WriteAlertToFile(ProcessingContext context, Alert alert, bool activeIncident)
		{
			string message = string.Empty;
			string logDirectory = @"C:\Temp";
			if (activeIncident)
				message = $"[{DateTime.Now.ToString("G")}:] Alert: Instance {alert.DigitalTwinId} reported {alert.IncidentType} condition. Premaint: {alert.IsInPreMaintenancePeriod}. Prior Warning Count: {alert.NumberWarningMessagesReceived}";
			else
				message = $"[{DateTime.Now.ToString("G")}:] Alert: Instance {alert.DigitalTwinId}, Exited {alert.IncidentType} state. Premaint: {alert.IsInPreMaintenancePeriod}. Resume Normal Operation";

			// Create the target directory if it does not exist
			if (!Directory.Exists(logDirectory))
				Directory.CreateDirectory(logDirectory);

			using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(logDirectory, "DigitalTwin.Samples.WindTurbine.Alerts.txt"), true))
			{
				file.WriteLine(message);
			}
			// Log this alert to the log object
			if (context != null)
				context.LogMessage(LogSeverity.Informational, message);
		}
	}
}
