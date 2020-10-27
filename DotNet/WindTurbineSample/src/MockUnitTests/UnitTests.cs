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
using System.Threading.Tasks;

using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scaleout.Streaming.DigitalTwin.Core;
using ScaleOut.Streaming.DigitalTwin.Mock;
using Scaleout.Streaming.DigitalTwin.Common;
using Scaleout.Streaming.DigitalTwin.Samples.WindTurbine;
using Toolbox = Scaleout.Streaming.DigitalTwin.Samples.WindTurbine.Toolbox;

namespace ScaleOut.Streaming.DigitalTwin.Samples.WindTubine.MockEnvironment
{
    [TestClass]
    public class MockUnitTests
    {
		[TestMethod]
		public async Task RegisterDeployAndShutdownDigitalTwin()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine");

			var mockEnvironment = await builder.BuildAsync();

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task TestMethodFromDocs()
		{
			// Optionally, create and configure a logger factory with the logger of your choice
			// for use by the mock environment
			ILoggerFactory loggerFactory = new LoggerFactory();
			loggerFactory.AddNLog();

			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine")
					.SetLoggerFactory(loggerFactory);

			// Load the digital twin model so it is ready to start processing events
			var mockEnvironment = await builder.BuildAsync();

			// Create a new message and serialize it to JSON
			var msg = new DeviceTelemetry()
			{
				Latitude = 47.6M,
				Longitude = -122.34M,
				Timestamp = DateTime.UtcNow
			};
			var jsonMsg = JsonConvert.SerializeObject(msg);

			// By sending the message below, the mock runtime would create a digital twin instance 
			// of the MyDigitalTwin model named "device_123" if it does not already exist, 
			// and process the message
			SendingResult result = MockEndpoint.Send("windturbine", "device_123", jsonMsg);
			if (result == SendingResult.Handled)
				Console.WriteLine("The message was processed by digital twin instance 'device_123'");
			else // SendingResult.NotHandled
				Console.WriteLine("The message was not handled and no digital twin was created.");

			// Retrieve all instances of the "windturbine" model (type of WindTurbineDigitalTwin):
			var modelInstances = mockEnvironment.GetInstances("windturbine");

			// Retrieve an instance from the returned collection:
			modelInstances.TryGetValue("device_123", out DigitalTwinBase instance);

			// Shut down the model
			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task SendOneMessage()
		{
			var instanceId = "windturbine_1";

			var loggerFactory = Helper.CreateLoggerFactoryForNLog();
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine")
					.SetLoggerFactory(loggerFactory);

			var mockEnvironment = await builder.BuildAsync();

			var telemetryMsg = Toolbox.CreateDeviceMessage(false, TimeSpan.FromSeconds(1), MessageType.Normal);
			var jsonMsg = JsonConvert.SerializeObject(telemetryMsg);

			var result = MockEndpoint.Send(digitalTwinModel: "windturbine", instanceId, jsonMsg);
			if (result == SendingResult.NotHandled)
				Assert.Fail($"The following message was not processed successfully: {jsonMsg}. See the ..\\logs\\{DateTime.Now.ToString("yyyy-MM-dd")}-mock-unittests.log log file for more details.");

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task SendMessageTriggeringSendingMessageBackToDatasource()
		{
			var instanceId = "windturbine_1_ds";

			var loggerFactory = Helper.CreateLoggerFactoryForNLog();

			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine")
					.SetLoggerFactory(loggerFactory);

			var mockEnvironment = await builder.BuildAsync();

			var telemetryMsg = Toolbox.CreateDeviceMessage(false, TimeSpan.FromSeconds(1), MessageType.Normal);
			telemetryMsg.Latitude = 100; // signal to model to send message back

			var jsonMsg = JsonConvert.SerializeObject(telemetryMsg);

			var result = MockEndpoint.Send(digitalTwinModel: "windturbine", instanceId, jsonMsg);
			if (result == SendingResult.NotHandled)
				Assert.Fail($"The following message was not processed successfully: {jsonMsg}. See the ..\\logs\\{DateTime.Now.ToString("yyyy-MM-dd")}-mock-unittests.log log file for more details.");

			// Waiting for message delivery to the mock data source collection (it's done asynchronously)
			await Task.Delay(1000);

			var iList = MockEndpoint.Receive(digitalTwinModel: "windturbine", instanceId);

			if (iList == null)
				Assert.Fail($"Unable to read message from the data source mock collection for instance '{instanceId}'");
			else
			{
				var msgList = iList.ToList<string>();
				if (msgList.Count != 1)
					Assert.Fail($"Unexpected number of messages retrieved from the data source mock collection for instance '{instanceId}'");
			}

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task LoggedAndRetrieveAllMessages()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine");

			var mockEnvironment = await builder.BuildAsync();

			var instanceId = "wt_LoggedAndRetrieveAllMessages";
			var numMsg = 10;
			for (int i = 0; i < numMsg; i++)
				Helper.CreateAndSendTestMessageToTwin(instanceId, Helper.ActionTrigger.TriggerLoggingMsg);

			var msgs = mockEnvironment.GetAllLoggedMessages().ToList<LogMessage>();
			if (msgs == null)
				Assert.Fail("The list of logged messages is null");

			if (msgs.Count < 10)
				Assert.Fail($"Unexpected number of retrieved logged messages ('{msgs.Count}'), the expected number of messages should be at least {numMsg}");

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task GetAllInstances()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine");

			var mockEnvironment = await builder.BuildAsync();
			int count = 50;
			for (int i = 0; i < count; i++) // Send 50 different messages to 50 unique twin instances
			{
				var instanceId = $"wt_{i}";
				Helper.CreateAndSendTestMessageToTwin(instanceId, Helper.ActionTrigger.NoAction);
			}

			var instances = mockEnvironment.GetInstances(digitalTwinModel: "windturbine");
			if (instances.Count != count)
				Assert.Fail($"The number of retrieved instances ({instances.Count}) is unexpected (expected number: {count})");

			foreach (var item in instances)
				Console.WriteLine($"Instance Id: {item.Key}, state: {(item.Value as WindTurbineDigitalTwin).State}");

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task GenerateModelSchemaTest()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine");

			var mockEnvironment = await builder.BuildAsync();
			var dtSchemaFileContent = mockEnvironment.GenerateModelSchema(digitalTwinModel: "windturbine");
			try
			{
				// Checking for format correctness
				ModelSchema schema = JsonConvert.DeserializeObject<ModelSchema>(dtSchemaFileContent);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Failed to deserialize produced model's schema file content to the schema class: '{ex.Message}'");
			}

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task CreateModelSchemaFileTest1()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<WindTurbineDigitalTwin, WindTurbineMessageProcessor, DeviceTelemetry>(digitalTwinModel: "windturbine");

			var mockEnvironment = await builder.BuildAsync();
			var pathSchemaFile = mockEnvironment.CreateModelSchemaFile(digitalTwinModel: "windturbine", outputDirectory: null);
			var jsonContent = string.Empty;

			try
			{
				// Making sure the schema file exists
				jsonContent = File.ReadAllText(pathSchemaFile);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Unable to open produced schema file: '{ex.Message}'");
			}
			try
			{
				// Making sure the schema file has correct content:
				ModelSchema schema = JsonConvert.DeserializeObject<ModelSchema>(jsonContent);
			}
			catch (Exception ex)
			{
				Assert.Fail($"Failed to deserialize produced model's schema file content to the corresponding schema class: '{ex.Message}'");
			}
			finally
			{
				if (File.Exists(pathSchemaFile))
					File.Delete(pathSchemaFile);
			}

			await mockEnvironment.ShutdownAsync();
		}
	}
}
