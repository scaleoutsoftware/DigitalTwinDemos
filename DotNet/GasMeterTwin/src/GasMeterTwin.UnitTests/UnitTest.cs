/* 
 * © Copyright 2020 by ScaleOut Software, Inc.
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
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scaleout.Streaming.DigitalTwin.Core;
using ScaleOut.Streaming.DigitalTwin.Mock;

namespace GasMeterTwin.UnitTests
{
	[TestClass]
	public class UnitTests
	{
		[TestMethod]
		public async Task SendSpikePPMMessage()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<GasSensor, GasSensorTwinMessageProcessor, Message>(digitalTwinModel: "gasmeter");

			var mockEnvironment = await builder.BuildAsync();

			// Create a new message and serialize it to JSON
			var msg = new Message() { PPMReading = 250, Timestamp = DateTime.Now };
			var jsonMsg = JsonConvert.SerializeObject(msg);

			// By sending the message below, the mock runtime would create a digital twin instance 
			// of the gasmeter model type with Id "gasmeter_123" if it does not already exist, 
			// and process the message
			SendingResult result = MockEndpoint.Send("gasmeter", "gasmeter_123", jsonMsg);
			if (result == SendingResult.Handled)
				Console.WriteLine("The message was processed by digital twin instance 'gasmeter_123'");
			else // SendingResult.NotHandled
				Console.WriteLine("The message was not handled and no digital twin was created.");

			// Retrieve all instances of the "gasmeter" model (type of GasMeterTwin):
			var modelInstances = mockEnvironment.GetInstances("gasmeter");

			// Retrieve an instance from the returned collection:
			modelInstances.TryGetValue("gasmeter_123", out DigitalTwinBase instance);

			await mockEnvironment.ShutdownAsync();
		}

		[TestMethod]
		public async Task SendTwoMessagesWithTimestampsMoreThan15MinutesApart()
		{
			MockEnvironmentBuilder builder =
				new MockEnvironmentBuilder()
					.AddDigitalTwin<GasSensor, GasSensorTwinMessageProcessor, Message>(digitalTwinModel: "gasmeter");

			var mockEnvironment = await builder.BuildAsync();

			// Create a list of two messages
			var msgsBytes = new List<byte[]>() { 
					Encoding.UTF8.GetBytes(
						JsonConvert.SerializeObject(new Message { PPMReading = 55, Timestamp = DateTime.Now })),
					Encoding.UTF8.GetBytes(
						JsonConvert.SerializeObject(new Message { PPMReading = 90, Timestamp = DateTime.Now.AddMinutes(20) }))};

			// Send the list of "toxic" messages to the real-time twin instance to trigger the alert
			SendingResult result = MockEndpoint.Send("gasmeter", "gasmeter_123", msgsBytes);
			if (result == SendingResult.Handled)
				Console.WriteLine("The message was processed by digital twin instance 'gasmeter_123'");
			else // SendingResult.NotHandled
				Console.WriteLine("The message was not handled and no digital twin was created.");

			// The alert condition that was triggered by two "toxic" messages above,
			// should have sent a pipe shutdown command back to the data source.
			// Waiting here for message delivery to the mock data source collection 
			// (since it's done asynchronously)
			await Task.Delay(1000);

			// Now we can retrieve it
			var alertMessages = MockEndpoint.Receive("gasmeter", "gasmeter_123");

			// It should be just one message, we can deserialize it back to expected type:
			var receivedActionCommand = JsonConvert.DeserializeObject<ActionCommand>(alertMessages.FirstOrDefault());

			// The final checks
			Assert.AreEqual(receivedActionCommand.Description, "Shutdown the incoming gas pipe");
			Assert.AreEqual(receivedActionCommand.Code, 100);

			await mockEnvironment.ShutdownAsync();
		}
	}
}
