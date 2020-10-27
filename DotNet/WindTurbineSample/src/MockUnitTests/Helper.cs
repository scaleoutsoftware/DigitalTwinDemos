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

using Newtonsoft.Json;
using NLog.Extensions.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scaleout.Streaming.DigitalTwin.Core;
using ScaleOut.Streaming.DigitalTwin.Mock;
using Scaleout.Streaming.DigitalTwin.Samples.WindTurbine;

namespace ScaleOut.Streaming.DigitalTwin.Samples.WindTubine.MockEnvironment
{
    public class Helper
    {
		internal enum ActionTrigger
		{
			NoAction = 1,
			TriggerSendingMsgToDatasource = 2,
			TriggerLoggingMsg = 3
		}

		internal static ILoggerFactory CreateLoggerFactoryForNLog()
		{
			ILoggerFactory loggerFactory = new LoggerFactory();
			var options = new NLogProviderOptions() { CaptureMessageTemplates = true, CaptureMessageProperties = true };
			return loggerFactory.AddNLog(options);
		}

		internal static void CreateAndSendTestMessageToTwin(string instanceId, ActionTrigger trigger)
		{
			var telemetryMsg = Toolbox.CreateDeviceMessage(false, TimeSpan.FromSeconds(1), MessageType.Normal);

			switch (trigger)
			{
				case ActionTrigger.TriggerSendingMsgToDatasource:
					telemetryMsg.Latitude = 100; // signal to model to send message back
					break;

				case ActionTrigger.TriggerLoggingMsg:
					telemetryMsg.Latitude = 101; // signal to model to log the message into the logging object
					break;
			}

			var jsonMsg = JsonConvert.SerializeObject(telemetryMsg);

			SendingResult result = MockEndpoint.Send(digitalTwinModel: "windturbine", instanceId, jsonMsg);
			if (result == SendingResult.NotHandled)
				Assert.Fail($"The following message was not processed successfully: {jsonMsg}. See the ..\\logs\\{DateTime.Now.ToString("yyyy-MM-dd")}-mock-unittests.log log file for more details.");
		}

	}
}
