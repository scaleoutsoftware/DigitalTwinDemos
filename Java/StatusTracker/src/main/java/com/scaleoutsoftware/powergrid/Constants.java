package com.scaleoutsoftware.powergrid;

public class Constants {
    public static String NODE_TYPE_INFRASTRUCTURE   = "infrastructure";
    public static String NODE_TYPE_CONTROLLER       = "controller";

    /* Power grid node status */
    public static String MESSAGE_TYPE_INIT          = "init";       // initialization message
    public static String MESSAGE_TYPE_STATUS        = "status";     // status message
    public static String NODE_CONDITION_OFFLINE     = "offline";    // node was powered off
    public static String NODE_CONDITION_NORMAL      = "normal";     // base condition
    public static String NODE_CONDITION_MINOR       = "minor";      // previously abnormal
    public static String NODE_CONDITION_MODERATE    = "moderate";   // previously suspect attack
    public static String NODE_CONDITION_SEVERE      = "severe";     // previously attack

    /* Power Grid node location */
    public static String NODE_REGION_NW = "NW";
    public static String NODE_REGION_SW = "SW";
    public static String NODE_REGION_MN = "MN";
    public static String NODE_REGION_MS = "MS";
    public static String NODE_REGION_NE = "NE";
    public static String NODE_REGION_SE = "SE";

    /* Alert Levels for Controller and Infrastructure power grid nodes. */
    // controller and infrastructure normal alert levels
    public static int CONTROLLER_NORMAL_ALERTLEVEL          = 0;
    public static int INFRASTRUCTURE_NORMAL_ALERTLEVEL      = 0;

    // controller and infrastructure minor alert levels
    public static int CONTROLLER_MINOR_ALERT_LEVEL          = 2;
    public static int INFRASTRUCTURE_MINOR_ALERTLEVEL       = 1;

    // controller and infrastructure moderate alert levels
    public static int CONTROLLER_MODERATE_ALERTLEVEL        = 8;
    public static int INFRASTRUCTURE_MODERATE_ALERTLEVEL    = 4;

    // controller and infrastructure severe alert levels
    public static int CONTROLLER_SEVERE_ALERTLEVEL          = 20;
    public static int INFRASTRUCTURE_SEVERE_ALERTLEVEL      = 10;

    /* Prevent power grid state object memory growth */
    public static int MAX_INCIDENT_LIST_SIZE        = 15;
    public static int INCIDENT_LIST_SUBLIST_FROM    = 10;
    public static int INCIDENT_LIST_SUBLIST_TO      = MAX_INCIDENT_LIST_SIZE;

}
