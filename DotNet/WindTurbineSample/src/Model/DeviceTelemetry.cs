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
	/// The test class that models a WindTurbine telemetry message.
	/// </summary>
	public class DeviceTelemetry
	{
		/// <summary>Device location's latitude.</summary>
		public decimal Latitude { get; set; }

		/// <summary>Device location's longitude.</summary>
		public decimal Longitude { get; set; }

		/// <summary>Device status.</summary>
		public DeviceStatus Status { get; set; }

		/// <summary>Turbine's RPM speed.</summary>
		public int RPMSpeed { get; set; }

		/// <summary>Turbine's engine temperature.</summary>
		public int Temp { get; set; }
        /// <summary>Used to add size to the messages being sent to the device</summary>
        public byte[] AdditionalPayload { get; set; }

		/// <summary>Timestamp of when the message was originated by device.</summary>
		public DateTime Timestamp { get; set; }

		/// <summary>String representation of the <see cref="DeviceTelemetry"/> class instance.</summary>
		/// <returns>String representation of the class instance.</returns>
		public override string ToString()
		{
			return $"{Timestamp.ToString("s")} - lat:{Latitude}, long:{Longitude}, RPM: {RPMSpeed}, Temp: {Temp}";
		}
	}
}
