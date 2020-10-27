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
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using Scaleout.Streaming.DigitalTwin.Samples.WindTurbine;

namespace Scaleout.Streaming.DigitalTwin.Samples.Client
{
	public class SimulatedAzureDevice
	{
		public async Task Run()
		{
			if (!Int32.TryParse(ConfigurationManager.AppSettings["NumberOfRandomMessagesInBatch"], out int numberOfRandomMsg))
				numberOfRandomMsg = 100;
			if (!Int32.TryParse(ConfigurationManager.AppSettings["NumberOfDevicesToUse"], out int numberOfDevices))
				numberOfDevices = 1;

			DeviceClient[] _device = new DeviceClient[numberOfDevices];

			for(int nIdx = 0; nIdx < numberOfDevices; nIdx++)
			{
				_device[nIdx] = DeviceClient.CreateFromConnectionString(ConfigurationManager.AppSettings[$"Device{nIdx+1}ConnectionString"]);
				await _device[nIdx].OpenAsync();
				var receiveEventsTask = ReceiveEventsFromAzure(_device[nIdx]);
			}

			Console.WriteLine($"{_device.Length} devices are connected to the cloud.");

			Console.WriteLine("Press a key to perform an action:");
			Console.WriteLine("\tq: exit the program");
			Console.WriteLine("\tr: send a random message to the cloud and digital twin");
			Console.WriteLine("\tl: send a low RPM message to the cloud and digital twin");
			Console.WriteLine("\th: send a high temp message to the cloud and digital twin");

			var random = new Random();
			var quitRequested = false;
			while (!quitRequested)
			{
				Console.Write("Action? ");
				var input = Console.ReadKey().KeyChar;
				Console.WriteLine();

				if (input == 'q')
				{
					quitRequested = true;
					break;
				}
				else
				{
					MessageType msgType = MessageType.Normal;
					int msgToSend = 1;

					switch(input)
					{
						case 'r':
							msgType = MessageType.RandomValues;
							msgToSend = numberOfRandomMsg;
							break;
						case 'h':
							msgType = MessageType.HighTemperature;
							break;
						case 'l':
							msgType = MessageType.LowRPM;
							break;
					}

					List<Task> tasks = new List<Task>();
					int numMsg = msgToSend;

					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					while (msgToSend-- > 0)
					{
						 tasks.Add(Task.Factory.StartNew(() =>
						 {
							 var deviceTelemetry = Toolbox.CreateDeviceMessage(false, TimeSpan.FromSeconds(1), msgType);
							 var jsonTelemetry = JsonConvert.SerializeObject(deviceTelemetry);
							 var message = new Message(Encoding.UTF8.GetBytes(jsonTelemetry));
							 message.Properties.Add("DigitalTwinModel", WindTurbineDigitalTwin.DigitalTwinModelType);
							 _device[random.Next(0, _device.Length)].SendEventAsync(message);
							 Console.WriteLine($"Message sent to the cloud (RPM: {deviceTelemetry.RPMSpeed}, Temp: {deviceTelemetry.Temp})");
						 }));
					}
					await Task.WhenAll(tasks);
					stopwatch.Stop();

					if (numMsg > 1) // batch mode
						Console.WriteLine($"{numMsg} messages were sent to the cloud in { stopwatch.ElapsedMilliseconds} ms.");
				}
			}
		}

		private static async Task ReceiveEventsFromAzure(DeviceClient device)
		{
			while (true)
			{
				try
				{
					var message = await device.ReceiveAsync();

					if (message == null) // timeout occurred on the ReceiveAsync call
						continue;

					var messageBody = message.GetBytes();
					var payload = Encoding.UTF8.GetString(messageBody);
					var alerts = JsonConvert.DeserializeObject(payload, typeof(List<Alert>)) as List<Alert>;
					foreach(var msg in alerts)
						Console.WriteLine($"Received message from the digital twin {msg.DigitalTwinId}:\n\t'{msg.ToString()}'");

					await device.CompleteAsync(message);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Unexpected error while processing the message from a digital twin:\n\t'{ex.Message}'");
				}
			}
		}
	}
}
