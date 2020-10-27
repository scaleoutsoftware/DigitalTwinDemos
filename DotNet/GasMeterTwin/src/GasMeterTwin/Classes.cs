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
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using Scaleout.Streaming.DigitalTwin.Core;

namespace GasMeterTwin
{
    /// <summary>
    /// Real-Time digital twin model class
    /// </summary>
    public class GasSensor : DigitalTwinBase
    {
        #region Constant values
        public const int MaxAllowedPPM = 50;
        public const int MaxAllowedMinutes = 15;
        public const int SpikeAlertPPM = 200;
        #endregion

        #region State properties
        public int LastPPMReading { get; set; }
        public DateTime LastPPMTime { get; set; }

        public bool LimitExceeded { get; set; }
        public bool AlarmSounded { get; set; }
        public DateTime LimitStartTime { get; set; }

        public int NumEvents { get; set; }
        #endregion
    }

    /// <summary>
    /// Message class that represents gas sensor telemetry.
    /// </summary>
    public class Message
    {
        public int PPMReading { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// MessageProcessing class with the overridden ProcessMessages method.
    /// </summary>
    public class GasSensorTwinMessageProcessor : MessageProcessor<GasSensor, Message>
    {
        public override ProcessingResult ProcessMessages(ProcessingContext context,
                                                         GasSensor dt,
                                                         IEnumerable<Message> newMessages)
        {
            // 
            // Process incoming messages
            //
            foreach (var msg in newMessages)
            {
                dt.LastPPMReading = msg.PPMReading;
                dt.LastPPMTime = msg.Timestamp;

                if (msg.PPMReading > GasSensor.MaxAllowedPPM)
                {
                    if (!dt.LimitExceeded)
                    {
                        dt.LimitExceeded = true;
                        dt.LimitStartTime = dt.LastPPMTime;
                        dt.NumEvents++;
                    }
                    else if ((dt.LastPPMTime - dt.LimitStartTime) > TimeSpan.FromMinutes(GasSensor.MaxAllowedMinutes) ||
                             dt.LastPPMReading >= GasSensor.SpikeAlertPPM)
                    {
                        dt.AlarmSounded = true; // notify some personal

                        // BONUS CODE:
                        var action = new ActionCommand();
                        action.Description = "Shutdown the incoming gas pipe";
                        action.Code = 100;
                        // Send the shutdown gas pipe command back to the device
                        context.SendToDataSource(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(action)));
                    }
                }
            }

            return ProcessingResult.DoUpdate;
        }
    }

    /// <summary>
    /// Example of action command message 
    /// that could be sent back to a data source.
    /// </summary>
    public class ActionCommand
    {
        public string Description { get; set; }
        public int Code { get; set; }
    }
}
