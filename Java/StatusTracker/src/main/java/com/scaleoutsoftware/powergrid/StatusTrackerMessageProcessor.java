/**
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
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT ARE EXPRESSLY
 * DISCLAIMED.  SOME JURISDICTIONS DO NOT ALLOW THE EXCLUSION OF IMPLIED
 * WARRANTIES, SO THE ABOVE EXCLUSIONS MAY NOT APPLY TO YOU.  IN NO
 * EVENT WILL SSI BE LIABLE TO ANY PARTY FOR ANY DIRECT, INDIRECT,
 * SPECIAL OR OTHER CONSEQUENTIAL DAMAGES FOR ANY USE OF THE SAMPLE CODE
 * INCLUDING, WITHOUT LIMITATION, ANY LOST PROFITS, BUSINESS
 * INTERRUPTION, LOSS OF PROGRAMS OR OTHER DATA ON YOUR INFORMATION
 * HANDLING SYSTEM OR OTHERWISE, EVEN IF WE ARE EXPRESSLY ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGES.
 */
package com.scaleoutsoftware.powergrid;

import com.scaleoutsoftware.digitaltwin.core.MessageProcessor;
import com.scaleoutsoftware.digitaltwin.core.ProcessingContext;
import com.scaleoutsoftware.digitaltwin.core.ProcessingResult;

import java.io.PrintWriter;
import java.io.Serializable;
import java.io.StringWriter;
import java.util.logging.Level;

/**
 * The StatusTrackerMessageProcessor class is responsible for implementing the MessageProcessor base class.
 * Once deployed to the RealTime DigitalTwin service, the StatusTrackerMessageProcessor will be used to process messages
 * from the StatusTracker model twin. The StatusTrackerMessageProcessor will use state information from the StatusTracker
 * digital twin and the incoming message for analysis and introspection.
 */
public class StatusTrackerMessageProcessor extends MessageProcessor<StatusTracker, StatusTrackerMessage> implements Serializable {

    /**
     * Analyze messages with state information in the StatusTracker digital twin and update the state object.
     *
     * @param processingContext The processingContext is used for sending a reply to a datasource or a message to a digital twin.
     * @param digitalTwin the state object
     * @param messages Messages from the ModelTwin
     * @return ProcessingResult.Update or ProcessingResult.NoUpdate.
     * @throws Exception
     */
    @Override
    public ProcessingResult processMessages(ProcessingContext processingContext,
                                            StatusTracker digitalTwin,
                                            Iterable<StatusTrackerMessage> messages) throws Exception {
        try {
            // iterate through the incoming messages
            for(StatusTrackerMessage msg : messages) {
                // this is an initialization message so we set our status and return.
                if(msg.initMessage()) {
                    digitalTwin.setStatusTrackerType(msg.getNodeType());
                    digitalTwin.setStatusTrackerCondition(Constants.NODE_CONDITION_NORMAL);
                    digitalTwin.setRegion(msg.getRegion(), msg.getLongitude(), msg.getLatitude());
                    return ProcessingResult.UpdateDigitalTwin;
                }

                /* Run through the Status Tracker rules. */

                // incoming message indicates the status tracker is offline or in normal operation.
                if(msg.offline() || msg.normalOperation()) {
                    // set the state object statistics
                    if(digitalTwin.experiencingModerateEvent() || digitalTwin.experiencingMinorEvent()) {
                        digitalTwin.incrementFalseAlarmCount();
                        digitalTwin.incrementResolvedIncidents();
                    }
                    else if(digitalTwin.experiencingSevereEvent()) {
                        digitalTwin.incrementResolvedIncidents();
                    }

                    // set the status tracker's alert level to normal, and update the state objects condition
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_NORMAL_ALERTLEVEL, Constants.CONTROLLER_NORMAL_ALERTLEVEL);
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a minor incident
                else if(msg.minorIncident()) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_MINOR_ALERTLEVEL, Constants.CONTROLLER_MINOR_ALERT_LEVEL);
                    digitalTwin.incrementMinorEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a severe incident
                else if(msg.severeIncident()) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_SEVERE_ALERTLEVEL, Constants.CONTROLLER_SEVERE_ALERTLEVEL);
                    digitalTwin.incrementSevereEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a moderate incident and this tracker has seen a severe incident
                else if(msg.moderateIncident() && digitalTwin.getSevereIncidentCount() > 0) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_MODERATE_ALERTLEVEL+1, Constants.CONTROLLER_MODERATE_ALERTLEVEL+3);
                    digitalTwin.incrementModerateEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a moderate incident and this tracker has never had a severe incident and
                // this tracker has never seen a false incident
                else if(msg.moderateIncident() &&
                   digitalTwin.getSevereIncidentCount() == 0 &&
                   digitalTwin.getFalseIncidentCount() == 0) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_MODERATE_ALERTLEVEL+2, Constants.CONTROLLER_MODERATE_ALERTLEVEL+4);
                    digitalTwin.incrementModerateEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a moderate incident and this tracker has never had a severe incident while the
                // heuristic of a false incident is greater than 50%
                else if(msg.moderateIncident() &&
                   digitalTwin.getSevereIncidentCount() == 0 &&
                   digitalTwin.getModerateIncidentCount() > 0 &&
                   ((double)(digitalTwin.getFalseIncidentCount()/digitalTwin.getModerateIncidentCount()) >= 0.5)) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_MODERATE_ALERTLEVEL+3, Constants.CONTROLLER_MODERATE_ALERTLEVEL+5);
                    digitalTwin.incrementModerateEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates a moderate incident and this tracker has never had a severe incident while the
                // heuristic of a false incident is less than 50%
                else if(msg.moderateIncident() &&
                   digitalTwin.getSevereIncidentCount() == 0 &&
                   digitalTwin.getModerateIncidentCount() > 0 &&
                   ((double)(digitalTwin.getFalseIncidentCount()/digitalTwin.getModerateIncidentCount()) < 0.5)) {
                    digitalTwin.setAlertLevel(Constants.INFRASTRUCTURE_MODERATE_ALERTLEVEL+4, Constants.CONTROLLER_MODERATE_ALERTLEVEL+6);
                    digitalTwin.incrementModerateEventCount();
                    digitalTwin.setStatusTrackerCondition(msg.getNodeCondition());
                }

                // the message indicates some form of incident -- update total incidents and add message to message list
                if(msg.moderateIncident() || msg.minorIncident() || msg.severeIncident()) {
                    digitalTwin.incrementTotalIncidents();
                    digitalTwin.addToIncidentList(msg);
                }
            }

            return ProcessingResult.UpdateDigitalTwin;
        } catch (Exception e) {
            // Print the full exception to the models log
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            processingContext.logMessage(Level.SEVERE, pw.toString());
            pw.close();
            sw.close();
            throw e;
        }
    }
}
