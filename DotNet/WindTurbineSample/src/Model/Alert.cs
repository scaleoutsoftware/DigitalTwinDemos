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

namespace Scaleout.Streaming.DigitalTwin.Samples.WindTurbine
{
	/// <summary>
	/// Defines an alert to be sent out to either device or operator in case of some incident.
	/// </summary>
	public class Alert
	{
		/// <summary>Type of the incident for this alert.</summary>
		public IncidentType IncidentType { get; set; }

		/// <summary>The digital twin instance Id which generated the alert.</summary>
		public string DigitalTwinId { get; set; }

		/// <summary>The timestamp when alert was created.</summary>
		public DateTime Timestamp { get; set; }

		/// <summary>Duration of the incident.</summary>
		public TimeSpan Duration { get; set; }

		/// <summary>Indicates whether a wind turbine is in pre-maintenance period.</summary>
		public bool IsInPreMaintenancePeriod { get; set; }

		/// <summary>The number of messages processed with the tracking metric outside of its normal range.</summary>
		public int NumberWarningMessagesReceived { get; set; }

		/// <summary>
		/// String representation of the <see cref="Alert"/> class instance.
		/// </summary>
		/// <returns>String representation of the class instance.</returns>
		public override string ToString()
		{
			return $"Incident created: {Timestamp.ToString("yyyy-MM-ddTHH:mm:ssK")}, Type: {IncidentType.ToString()}, Duration: {Duration.TotalSeconds:N0} (sec), Prior Warning Count: {NumberWarningMessagesReceived}";
		}
	}
}
