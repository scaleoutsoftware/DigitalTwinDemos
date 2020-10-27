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
package com.scaleoutsoftware.demos;

import com.google.gson.Gson;
import com.scaleoutsoftware.digitaltwin.core.SendingResult;
import com.scaleoutsoftware.digitaltwin.mock.MockEndpoint;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironment;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironmentBuilder;
import com.scaleoutsoftware.digitaltwin.mock.MockEnvironmentException;
import org.junit.Assert;
import org.junit.Test;

import java.util.List;

public class TestProcessor {
    @Test
    public void test15MinutePpmLimit() throws MockEnvironmentException {
        MockEnvironment environment = new MockEnvironmentBuilder()
                .addDigitalTwin(
                        "NaturalGasMeter",
                        new NaturalGasSensorMessageProcessor(),
                        NaturalGasSensor.class,
                        NaturalGasSensorMessage.class)
                .build();

        Gson gson = new Gson();
        long now = System.currentTimeMillis();
        long in15Minutes = now + (15 * 60000) + 100;
        NaturalGasSensorMessage highPpmMsg = new NaturalGasSensorMessage(51, now);
        String serializedMsg = gson.toJson(highPpmMsg);
        SendingResult result = MockEndpoint.send(
                "NaturalGasMeter",
                "23",
                serializedMsg
                );
        Assert.assertEquals(result, SendingResult.Handled);
        NaturalGasSensorMessage highPpmMsgIn15Minutes = new NaturalGasSensorMessage(51, in15Minutes);
        serializedMsg = gson.toJson(highPpmMsgIn15Minutes);
        result = MockEndpoint.send(
                "NaturalGasMeter",
                "23",
                serializedMsg
        );
        Assert.assertEquals(result, SendingResult.Handled);

        List<String> recievedMessages = MockEndpoint.receive("NaturalGasMeter", "23");
        Assert.assertNotNull(recievedMessages);
        Assert.assertEquals(1,recievedMessages.size());
        for(String msg : recievedMessages) {
            NaturalGasAlert actual = gson.fromJson(msg, NaturalGasAlert.class);
            Assert.assertEquals("Warning: dangerous air quality.", actual.getAlertMessage());
        }
        environment.shutdown();
    }

    @Test
    public void test200ppmSpike() throws MockEnvironmentException {
        MockEnvironment environment = new MockEnvironmentBuilder()
                .addDigitalTwin(
                        "NaturalGasMeter",
                        new NaturalGasSensorMessageProcessor(),
                        NaturalGasSensor.class,
                        NaturalGasSensorMessage.class)
                .build();

        Gson gson = new Gson();
        NaturalGasSensorMessage highPpmMsg = new NaturalGasSensorMessage(200, System.currentTimeMillis());
        String serializedMsg = gson.toJson(highPpmMsg);
        SendingResult result = MockEndpoint.send(
                "NaturalGasMeter",
                "23",
                serializedMsg
        );
        Assert.assertEquals(result, SendingResult.Handled);

        List<String> recievedMessages = MockEndpoint.receive("NaturalGasMeter", "23");
        Assert.assertNotNull(recievedMessages);
        Assert.assertTrue(recievedMessages.size() > 0);
        for(String msg : recievedMessages) {
            NaturalGasAlert actual = gson.fromJson(msg, NaturalGasAlert.class);
            Assert.assertEquals("Warning: dangerous air quality.", actual.getAlertMessage());
        }

    }


}
