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
	/// Defines the incident type for wind turbine operations and 
	/// incident's details. Sample illustrates the logic for high 
	/// temperature and low RPM speed tracking.
	/// </summary>
	public class Incident
	{
		/// <summary>
		/// Type of the incident.
		/// </summary>
		public IncidentType IncidentType { get; set; }

		/// <summary>
		/// Indicates in what phase the incident tracking currently is.
		/// </summary>
		public IncidentPhase IncidentPhase { get; set; }

		/// <summary>
		/// Date and time when incident is registered.
		/// </summary>
		public DateTime Timestamp { get; set; }

		/// <summary>
		/// Absolute value of tracking metric depending on the type of incident:
		///  - For <see cref="IncidentType.HighTemperature"/>: engine's temperature.
		///  - For <see cref="IncidentType.LowRPM"/>: turbine's RPM value.
		///  - For <see cref="IncidentType.HighRPM"/>: turbine's RPM value.
		/// </summary>
		public int MetricValue { get; set; }
	}
}
