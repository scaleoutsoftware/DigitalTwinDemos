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
using System.Configuration;
using System.Threading.Tasks;

using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;

using Scaleout.AzureIoT.Client;
using Scaleout.Streaming.DigitalTwin.Common;
using Scaleout.Streaming.DigitalTwin.Deployment;
using Scaleout.Streaming.DigitalTwin.Samples.WindTurbine;

namespace Scaleout.Streaming.DigitalTwin.Samples.Client
{
	class Program
	{
        #region Azure IoT Hub parameters
        public static string _eventHubName				= "";
		public static string _eventHubConnectionString	= "";
		public static string _eventHubEventsEndpoint	= "";
		public static string _storageConnectionString	= "";
		public static string _consumerGroupName			= "";
        #endregion

        static async Task Main(string[] args)
		{
			ExecutionEnvironment execEnvironment = null;
			bool clientOnly = false;

			try
			{
				if (args.Length > 0 && args[0].Contains("clientOnly"))
					clientOnly = true;

				if (!clientOnly)
				{
					ILoggerFactory loggerFactory = new LoggerFactory();
					var options = new NLogProviderOptions() { CaptureMessageTemplates = true, CaptureMessageProperties = true };
					loggerFactory.AddNLog(options);

					// Register WindTurbine digital twin
					ExecutionEnvironmentBuilder builder =
						new ExecutionEnvironmentBuilder()
							.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(WindTurbineDigitalTwin.DigitalTwinModelType)
							.SetLoggerFactory(loggerFactory);

					// Load WindTurbine digital twin so it is ready to start processing events from Azure IoT Hub
					execEnvironment = await builder.BuildAsync();

					// Start Azure IoT Hub Connector
					EventListenerManager.StartAzureIoTHubConnector(
													connectorId				: "SampleAzureConnector",
													eventHubName			: _eventHubName,
													eventHubConnectionString: _eventHubConnectionString,
													eventHubEventsEndpoint	: _eventHubEventsEndpoint,
													storageConnectionString	: _storageConnectionString,
																maxBatchSize: 65,
													scaleoutConnectionString: "",
													initialState			: InitialConnectorState.Enabled,
													consumerGroupName		: _consumerGroupName);
				}

				// Create a simulate Azure IoT device
				SimulatedAzureDevice device = new SimulatedAzureDevice();

				// Device is ready to send and receive messages
				await device.Run();

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception occurred: {ex.Message}");
			}
			finally
			{
				if (!clientOnly)
				{
					// Stops message processing and clean up resources
					if (execEnvironment != null)
						await execEnvironment.ShutdownAsync();
					else
						ExecutionEnvironment.DeleteDigitalTwinModel(
														digitalTwinModel: WindTurbineDigitalTwin.DigitalTwinModelType,
														removeInstances: true);
					EventListenerManager.StopAzureIoTHubConnector(connectorId: "SampleAzureConnector");
				}
			}
		}

		/// <summary>
		/// The helper method that checks and reads Azure IoT Hub configuration parameters
		/// from the .config file.
		/// </summary>
		private static void ReadAzureIoTHubParams()
		{
			if (ConfigurationManager.AppSettings["EventHubName"] != null)
				_eventHubName = ConfigurationManager.AppSettings["EventHubName"];

			if (ConfigurationManager.AppSettings["EventHubConnectionString"] != null)
				_eventHubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];

			if (ConfigurationManager.AppSettings["StorageConnectionString"] != null)
				_storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

			if (ConfigurationManager.AppSettings["ConsumerGroupName"] != null)
				_consumerGroupName = ConfigurationManager.AppSettings["ConsumerGroupName"];
		}
	}
}