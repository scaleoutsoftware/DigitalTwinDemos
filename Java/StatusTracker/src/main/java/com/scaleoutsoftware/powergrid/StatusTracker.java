/**
 * © Copyright 2019 by ScaleOut Software, Inc.
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

import com.scaleoutsoftware.digitaltwin.core.DigitalTwinBase;

import java.util.LinkedList;
import java.util.List;
import java.util.Objects;

/**
 * StatusTracker class used to represent Infrastructure and Controller power grid nodes.
 */
public class StatusTracker extends DigitalTwinBase {
    // State variables
    public String node_type;
    public String node_condition;
    public String region;
    public double longitude;
    public double latitude;

    // Derived state variables
    public int alert_level; // alert level visible in the demo
    public int minorIncidentCount;
    public int moderateIncidentCount;
    public int falseIncidentCount;
    public int severeIncidentCount;
    public int totalIncidents;
    public int totalResolvedIncidents;
    public boolean experiencingIncident;

    // Dynamic incident report list
    public List<IncidentReport> incidentList;

    /**
     * Default constructor.
     */
    public StatusTracker() {
        node_condition = "";
        node_type = "";
        region = "";
        alert_level = 0;
        minorIncidentCount = 0;
        moderateIncidentCount = 0;
        falseIncidentCount = 0;
        severeIncidentCount = 0;
        longitude = 0.0;
        latitude = 0.0;
        incidentList = new LinkedList<>();
    }

    /**
     * Sets the power grid node's type
     * @param statusTrackerType the power grid node's type ("controller" or "infrastructure")
     */
    public void setStatusTrackerType(String statusTrackerType) {
        node_type = statusTrackerType;
    }

    /**
     * Sets the power grid node's condition.
     * @param statusTrackerCondition the condition to set
     */
    public void setStatusTrackerCondition(String statusTrackerCondition) {
        node_condition = statusTrackerCondition;
    }

    /**
     * Sets the region of this power grid node
     * @param r the region
     * @param lon the longitude of this node's region
     * @param lat the latitude of this node's region
     */
    public void setRegion(String r, double lon, double lat) {
        region = r;
        longitude = lon;
        latitude = lat;
    }

    /**
     * Returns true if this node is experiencing a moderate event, otherwise false..
     * @return true if node_condition is equal to moderate, otherwise false
     */
    public boolean experiencingModerateEvent() {
        return node_condition.compareTo(Constants.NODE_CONDITION_MODERATE) == 0;
    }

    /**
     * Returns true if this node is experiencing a severe event, otherwise false..
     * @return true if node_condition is equal to severe, otherwise false
     */
    public boolean experiencingSevereEvent() {
        return node_condition.compareTo(Constants.NODE_CONDITION_SEVERE) == 0;
    }

    /**
     * Returns true if this node is experiencing a minor event, otherwise false.
     * @return true if the node_condition is equal to minor, otherwise false
     */
    public boolean experiencingMinorEvent() {
        return node_condition.compareTo(Constants.NODE_CONDITION_MINOR) == 0;
    }

    /**
     * Increments the false alarm count and resolves incident.
     */
    public void incrementFalseAlarmCount() {
        experiencingIncident = false;
        falseIncidentCount++;
    }

    /**
     * Increments the server event count and marks this node as "experiencing incident".
     */
    public void incrementSevereEventCount() {
        experiencingIncident = true;
        severeIncidentCount++;
    }

    /**
     * Increments the moderate event count and marks this node as "experiencing incident".
     */
    public void incrementModerateEventCount() {
        experiencingIncident = true;
        moderateIncidentCount++;
    }

    /**
     * Increments the minor event count and marks this node as "experiencing incident".
     */
    public void incrementMinorEventCount() {
        experiencingIncident = true;
        minorIncidentCount++;
    }

    /**
     * Returns the severe incident count.
     * @return the sever incident count
     */
    public int getSevereIncidentCount() {
        return severeIncidentCount;
    }

    /**
     * Returns the false incident count.
     * @return the false incident count
     */
    public int getFalseIncidentCount() {
        return falseIncidentCount;
    }

    /**
     * Returns moderate incident count.
     * @return the moderate incident count
     */
    public int getModerateIncidentCount() {
        return moderateIncidentCount;
    }

    /**
     * Set the alert level relative to this status trackers node type
     * @param infrastructureLevel the alert level for an infrastructure node
     * @param controllerLevel the alert level for a controller node
     */
    public void setAlertLevel(int infrastructureLevel, int controllerLevel) {
        if(node_type.compareTo(Constants.NODE_TYPE_INFRASTRUCTURE) == 0) {
            alert_level = infrastructureLevel;
        } else if (node_type.compareTo(Constants.NODE_TYPE_CONTROLLER) == 0) {
            alert_level = controllerLevel;
        }
    }

    /**
     * Increment resolved incident count
     */
    public void incrementResolvedIncidents() {
        experiencingIncident = false;
        totalResolvedIncidents++;
    }

    /**
     * Increment total incident count
     */
    public void incrementTotalIncidents() {
        totalIncidents++;
    }

    /**
     * Adds a msg to the incident list.
     * @param msg the message to add
     */
    public void addToIncidentList(StatusTrackerMessage msg) {
        if(incidentList == null) {
            incidentList = new LinkedList<>();
        }
        incidentList.add(new IncidentReport(System.currentTimeMillis(), msg.getNodeCondition()));
        if(incidentList.size() == Constants.MAX_INCIDENT_LIST_SIZE) {
            incidentList = incidentList.subList(Constants.INCIDENT_LIST_SUBLIST_FROM, Constants.INCIDENT_LIST_SUBLIST_TO);
        }
    }

    @Override
    public void init(String model, String id) {
        // optionally load from cloud service or database.
        super.init(model, id);
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        StatusTracker that = (StatusTracker) o;
        return alert_level == that.alert_level &&
                minorIncidentCount == that.minorIncidentCount &&
                moderateIncidentCount == that.moderateIncidentCount &&
                falseIncidentCount == that.falseIncidentCount &&
                severeIncidentCount == that.severeIncidentCount &&
                totalIncidents == that.totalIncidents &&
                totalResolvedIncidents == that.totalResolvedIncidents &&
                experiencingIncident == that.experiencingIncident &&
                Double.compare(that.longitude, longitude) == 0 &&
                Double.compare(that.latitude, latitude) == 0 &&
                node_type.equals(that.node_type) &&
                node_condition.equals(that.node_condition) &&
                region.equals(that.region) &&
                incidentList.equals(that.incidentList);
    }

    @Override
    public int hashCode() {
        return Objects.hash(node_type, node_condition, region, alert_level, minorIncidentCount, moderateIncidentCount, falseIncidentCount, severeIncidentCount, totalIncidents, totalResolvedIncidents, experiencingIncident, longitude, latitude, incidentList);
    }
}
