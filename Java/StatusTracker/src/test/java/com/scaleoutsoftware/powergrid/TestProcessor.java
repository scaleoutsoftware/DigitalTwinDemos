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

import com.google.gson.Gson;
import com.scaleoutsoftware.digitaltwin.core.DigitalTwinBase;
import com.scaleoutsoftware.digitaltwin.core.SendingResult;
import com.scaleoutsoftware.digitaltwin.mock.MockEndpoint;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironment;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironmentBuilder;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironmentException;
import org.junit.Assert;
import org.junit.Test;

import java.util.HashMap;

public class TestProcessor {
    @Test
    public void testInitMessage() {
        try {
            MockEnvironment environment = new MockEnvironmentBuilder()
                    .addDigitalTwin(
                            "StatusTracker",
                            new StatusTrackerMessageProcessor(),
                            StatusTracker.class,
                            StatusTrackerMessage.class)
                    .build();
            Assert.assertNotNull(environment);

            Gson gson = new Gson();
            StatusTrackerMessage initMessage = new StatusTrackerMessage(
                    Constants.MESSAGE_TYPE_INIT,
                    "23",
                    Constants.NODE_REGION_NW,
                    Constants.NODE_CONDITION_NORMAL,
                    Constants.NODE_TYPE_CONTROLLER,
                    47.5404,
                    122.6362
            );
            String serializedMsg = gson.toJson(initMessage);
            SendingResult result = MockEndpoint.send(
                    "StatusTracker",
                    "23",
                    serializedMsg);
            Assert.assertEquals(result, SendingResult.Handled);

            HashMap<String, DigitalTwinBase> instances = environment.getInstances("StatusTracker");
            Assert.assertNotNull(instances);

            DigitalTwinBase actual = instances.get("23");
            Assert.assertNotNull(actual);

            StatusTracker expected = new StatusTracker();
            expected.setStatusTrackerType(initMessage.getNodeType());
            expected.setStatusTrackerCondition(initMessage.getNodeCondition());
            expected.setRegion(initMessage.getRegion(), initMessage.getLongitude(), initMessage.getLatitude());

            Assert.assertEquals(expected, actual);
            environment.shutdown();
        } catch (MockEnvironmentException e) {
            Assert.fail();
        }
    }

    @Test
    public void generateModelSchema() throws Exception {
        MockEnvironment environment = new MockEnvironmentBuilder()
                .addDigitalTwin(
                        "StatusTracker",
                        new StatusTrackerMessageProcessor(),
                        StatusTracker.class,
                        StatusTrackerMessage.class)
                .build();
        environment.generateModelSchema("StatusTracker", "C:\\Users\\brandonr\\Desktop\\");
        environment.shutdown();
    }
}