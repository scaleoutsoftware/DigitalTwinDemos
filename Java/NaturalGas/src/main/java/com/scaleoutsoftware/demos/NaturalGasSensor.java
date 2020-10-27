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

import com.scaleoutsoftware.digitaltwin.core.DigitalTwinBase;

public class NaturalGasSensor extends DigitalTwinBase {
    // static constants
    public static final int MAX_READING_ALLOWED_PPM = 50;
    public static final int MAX_READING_ALLOWED_LIMIT_TIME_MINS = 15;
    public static final int	MAX_PPM_READING_SPIKE = 200;

    // state variables
    private int		_lastPpmReading;
    private long	_lastPpmTime;
    private boolean	_limitExceeded = false;
    private boolean	_alarmSounded  = false;
    private long	_limitStartTime;
    private int		_numEvents;

    public int getLastPpmReading() {
        return _lastPpmReading;
    }

    public void setLastPpmReading(int lastPpmReading) {
        _lastPpmReading = lastPpmReading;
    }

    public long getLastPpmTime() {
        return _lastPpmTime;
    }

    public void setLastPpmTime(long lastPpmTime) {
        _lastPpmTime = lastPpmTime;
    }

    public boolean isLimitExceeded() {
        return _limitExceeded;
    }

    public void setLimitExceeded(boolean limitExceeded) {
        _limitExceeded = limitExceeded;
    }

    public boolean isAlarmSounded() {
        return _alarmSounded;
    }

    public void setAlarmSounded(boolean alarmSounded) {
        _alarmSounded = alarmSounded;
    }

    public long getLimitStartTime() {
        return _limitStartTime;
    }

    public void setLimitStartTime(long limitStartTime) {
        _limitStartTime = limitStartTime;
    }

    public int getNumEvents() {
        return _numEvents;
    }

    public void incrementNumEvents() {
        _numEvents++;
    }

    @Override
    public void init(String model, String id) {
        super.init(model, id);
    }
}
