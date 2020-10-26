package com.scaleoutsoftware.powergrid;

import java.io.Serializable;

public class IncidentReport implements Serializable {
    public long timestamp;
    public String incidentType;

    public IncidentReport(long ts, String type) {
        timestamp = ts;
        incidentType = type;
    }
}
