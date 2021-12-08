import { Injectable, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';


@Injectable({
  providedIn: 'root'
})

export class HelpService {
    title:string;
    helpData = [
        {
            title: "Performance",
            panel: true,
            data: [
                {
                    name: "Throughput",
                    htmlText: "<p>Throughput is a measure of efficiency. It is meant to represent the maximum number of vehicles served on all phases at an intersection.</p><p></p>It is calculated as the highest 15-minute volume in a day at an intersection, converted to an hourly volume. Volumes come from high-resolution event logs from the controller, which are stored in the ATSPM database. All detectors used for volume counts are used in the throughput calculation for an intersection. It includes Tuesdays, Wednesdays and Thursdays only.<p></p>Detectors used for volume counts are selected based on a hierarchy, as there may be more than one detector in a given lane. For each lane, the detector with the highest count priority is selected for the count-based metrics. The priority scale is as follows:<ul><li>Exit</li><li>Advanced Count</li><li>Lane-by-lane Count</li></ul>"
                },
                {
                    name: "Arrivals on Green",
                    htmlText: "<p>Arrivals on Green (AOG) is a measure of coordination. A high percentage of arrivals on green would be the result of good offsets and should be correlated with fewer stops and less delay.</p><p>AOG is calculated as the total number of vehicles arriving on green light divided by the total number of arrivals. It is based on primary street through-phases, limited to peak periods (6am-10am, 3pm-7pm) on Tuesdays, Wednesdays and Thursdays.</p><p>The calculation uses detector data from Advance Count or Exit detectors, as configured in ATSPM. For advance detectors, the time of arrival at the intersection is adjusted for the setback distance and speed limit, both of which are configured in ATSPM.</p>"
                },
                {
                    name: "Progression Ratio",
                    htmlText: '<p>The percent of vehicles arriving during green is correlated very strongly with the amount of green time given to each phase. For phases with a high proportion of green time per cycle, there will naturally tend to be more arrivals on green, regardless of the arrival pattern.</p><p>Progression Ratio addresses this fact by controlling for the amount of green time per cycle on each phase. It is calculated as the arrival on green percentage divided by the percentage of green time (g/C) for that phase. It can be considered the quality of progression.</p><p>The Highway Capacity Manual (HCM) gives a range of values progression ratio and their interpretation (HCM Exhibit 15-4). A value less than one is poor progression. A value of 1 is equivalent to random arrivals. A value greater than one is desirable.</p>'
                },
                {
                    name: "Queue Spillback Rate",
                    htmlText: "<p>Queue Spillback Rate is an experimental measure of effectiveness. It is a measure of unmet demand in a cycle as measured by setback detectors. When vehicle dwell times on setback detectors exceed a threshold above what is typical for setback detectors under freely flowing conditions, that is interpreted as standing or slowed traffic over that detector, meaning the queue has reached the setback detector.</p><p>Specifically, under freely flowing conditions, the time between subsequent detector on and off events is typically around 0.1 seconds for setback detectors. When the 95th percentile detector occupancy duration increases above 3 seconds in a cycle, it will be assumed there is standing traffic on the setback detector and a spillback event will be flagged for that phase in that cycle.</p><p>If any lane on a phase registers high dwell time over a setback detector, that phase is considered to be spilled back for that cycle. The spillback rate for an intersection is the number of phases with a spillback condition (which could be more than one per cycle) divided by the number of phases multiplied by the number of cycles.</p>"
                },
                {
                    name: "Split Failures",
                    htmlText: "<p>Split failure is another measure of unmet demand. It identifies cycles where a phase has unserved demand. A phase is flagged for split failure when the average occupancy of the stop bar detectors on the phase are greater than 80% during the green phase and greater than 80% during the first five seconds of the red phase, which means there was demand at the stop bar both before and after the green interval. The intersection is flagged as a split failure on that cycle if at least one phase meets the criteria for split failure during that phase.</p><p>This metrics only uses Stop Bar Presence detection, and is only run for side street and left turn phases, i.e., all phases other than main street through phases.</p>"
                },
                {
                    name: "Travel Time Index",
                    htmlText: '<p>Travel Time Index (TTI) is a measure of delay on the corridor. It is the ratio of travel time to free flow travel time.</p><p>Hourly travel time data comes from HERE as queried from RITIS. Free flow travel times are based on the "reference speed" value from HERE for each segment. Travel time and free flow travel time are calculated for each corridor by summing over all segments in the corridor for every hour in the month.</p><p>An hourly Travel Time Index is then calculated for each corridor as the average travel time for that hour of the day divided by the free flow travel time for the corridor, i.e., each of the 24 hours of the day has its own measure for the month. The TTI for each hour is the average for each hour over the Tuesdays, Wednesdays and Thursdays in the month, divided by the free flow travel time for the corridor. The TTI for the day is the calculated as the average hourly travel time weighted by the hourly volume for the corridor, which is the hourly volume averaged over all signals in the corridor. This gives more weight to peak periods than off-peak periods).</p>'
                },
                {
                    name: "Planning Time Index",
                    htmlText: "<p>The Planning Time Index (PTI) calculation uses the same data as the Travel Time Index. However, instead of taking the average travel time for each hour in the month, it takes the 90th percentile of the day over the Tuesdays, Wednesdays and Thursdays for each hour. These 90th percentile travel times are then averaged over the day, weighted by the average hourly volume from the main street through phases (from ATSPM) to get a PTI for the month (this gives more weight to peak periods than off-peak periods).</p>"
                },
                {
                    name: "Daily Volume",
                    htmlText: "<p>Volume is a measure of demand on a corridor. Total volume on main street through phases are summed over each Tuesday, Wednesday and Thursday, and then averaged over all days in the month.</p>"
                },
                {
                    name: "Pedestrians",
                    htmlText: "<p>Pedestrian activity is the total number of pedestrian pushbutton events recorded by hour and by day. It is calculated over Tuesdays, Wednesdays and Thursdays.</p>"
                }
            ]
        },
        {
            title: "Volume & Equipment",
            panel: true,
            data: [
                {
                    name: "Detector Uptime",
                    htmlText: "<p>Detector Uptime is a measure of state-of-good-repair, which may be correlated to other performance measures since failed detectors may negatively affect performance.</p><p>Based on hourly volumes by detector, detector is evaluated according to three criteria:</p><ul><li>Volume too high</li><li>Volume erratic (too much change from one hour to the next)</li><li>Volume flatlined (no change in volume between successive time periods.</li></ul><p>Each detector is evaluated over each day. A detector is considered if failed for the day if any of the following conditions apply:</p><ul><li>There is a streak of at least 5 hours where the volume does not change, disregarding the hours before 5am.</li><li>At least 5 hours in the day have a volume exceeding 2000 vehicles</li><li>The mean absolute deviation (average magnitude difference between successive hours) is greater than 500.</li></ul>"
                },
                {
                    name: "Pedestrian Pushbutton Uptime",
                    htmlText: "<p>Pedestrian Pushbutton Uptime is the percentage of pedestrian inputs likely to be operational. It is based on the historical distribution of the daily number of pedestrian actuations. Currently, when the number of consecutive days without an input yields a probability of failure based on the historical distribution for that input, it is flagged as failed. This measure is still experimental and the distributions and thresholds are a work in progress.</p><p>In the past, this measure was based on manual testing of pedestrian push buttons and was self-reported by the engineers responsible for the corridor. While labor-intensive, This had some benefits over the current automated approach. The first is that multiple push buttons are often physically wired into the same detector input, making it impossible from the controller inputs to determine whether both push buttons are working, or just one. The second is due to the relatively infrequent calls, we have to rely upon a probabilistic approach to determining whether an input is failed because for some push buttons, there is so little demand it can be difficult to say with certainty from the data whether that the push button is indeed failed.</p>"
                },
                {
                    name: "CCTV Uptime",
                    htmlText: '<p>Through December 2017, CCTV Uptime was reported by Corridor Managers in their monthly reports. In January, 2018, it came from TSOS based on a manual check of each camera during the month and reporting whether it returns an image and whether it can be controlled (pan-tilt-zoom).</p><p>As of February 2018, CCTV Uptime is calculated based on the image returned from the Georgia 511 website. If there is a live image, it will considered working as of its "last modified" timestamp from the website.</p>'
                },
                {
                    name: "Communications Uptime",
                    htmlText: "<p>This is calculated from gaps in the ATSPM high resolution data. Any gaps in subsequent events greater than 15 minutes are considered to be due to communication loss. The sum of these gaps converted to a percent is the daily communication uptime for that controller. If comms are lost for all intersections, it is considered a system failure and that time is excluded from the uptime calculation.</p>"
                },
                {
                    name: "Events Reported, Resolved, Outstanding",
                    htmlText: "<p>Activity measures come from TEAMS reports from data as entered by corridor managers. TEAMS is GDOT's ticketing and tracking system for signal equipment and incident-related activity.</p><p>All events reported in the month are counted, as are the number of events resolved in the month. The number of outstanding events is the cumulative sum of reported events less the cumulative sum of resolved events.</p>"
                },
                {
                    name: "RTOP Activity Logs",
                    htmlText: "<p>These are the monthly reports produced monthly by RTOP Corridor Managers detailing key activities, maintenance tasks and action items for the month.</p>"
                }]
        },
        {
            title: "Misc",
            panel: false,
            data: [
                {
                    name: "Dashboard",
                    htmlText: "The home page of the SigOps Metrics website. This shows a high level overview of performance, volume, equipment, and TEAMS metrics at a filtered system and signal level."
                },
                {
                    name: "Watchdog",
                    htmlText: "The Watchdog analyzes hi-res ATSPM data and 3rd party data sources for anomalies, which are displayed here and ordered by frequency."
                },
                {
                    name: "TEAMSTasks",
                    htmlText: "Displays reported, resolved, and outstanding tasks from the TEAMS ticketing system. Tasks can be broken up by source, type, and subtype to lock in on where issues may be occurring in the field."
                },
                {
                    name: "Reports",
                    htmlText: "This is an interactive PowerBI report that allows users to control and export their data visualizations. These reports can also be emailed on a schedule. Please contact the SigOps Metrics administration team (via the Contact Us window) to be included on these emailed reports."
                },
                {
                    name: "Maintenance",
                    htmlText: "Maintenance"
                },
                {
                    name: "SignalInfo",
                    htmlText: "This is a read-only view of all the signals within the SigOps Metrics system. To correct any erroneous data, please contact the SigOps Metrics administration team (via the Contact Us window)."
                },
                {
                    name: "Help",
                    htmlText: "An all-encompassing repository of help documentation within the SigOps Metrics system."
                },
                {
                    name: "HealthMetrics",
                    htmlText: "Health Metrics were developed to provide a consistent means to calculate and compare the relative health of signalized intersections throughout the state. The Health Metrics score for an individual intersection is based on 15 different performance metrics within three main categories: operations, maintenance, and safety."
                    + "Metrics within each category are weighted differently depending on the characteristics of the intersection (i.e. volume, coordination, presence of pedestrians, etc.). The goal behind developing Health Metrics was to provide GDOT with a means to identify signalized intersections or groups of signalized intersection that may need additional attention or resources (retiming, technology upgrades, infrastructure improvements, etc.)."
                }]
        }]
        patchNotes = [
        'v1.0.2',
        'Added patch notes.',
        'Column name in Signal Info will still show up when filtering.',
        'Renamed Help to About.',
        'Map toolbox is now hidden.',
        'Adjusted % to be inline with other measurement units on dashboard.',
        'Added chips to TEAMS tasks.',
        'v1.0.1',
        '-Added ability to deselect filters one at a time.',
        '-Corrected issue with region mismatch between chip and filter.',
        '-Pressing the clear button on the filter no longer automatically applies it.',
        '-When an invalid filter is applied, existing charts/maps should show blank.',
        '-Disabled filter on pages where it is not used.',
        'v1.0.0',
        '-Site launch.'
    ];
    constructor(private titleService:Title) { }


    getHelpData(): any {
        var data;
        this.title = this.titleService.getTitle().replace("SigOpsMetrics - ","");
        if (this.title.includes("-")) {
            this.title = this.title.substring(this.title.indexOf("-")+2, this.title.length);
        }
        switch(this.title) {
            case "Progression Rate":
                this.title = "Progression Ratio";
                break;
            case "Spillback Rate":
                this.title = "Queue Spillback Rate";
                break;
            case "Peak Period Split Failures":
            case "Off-Peak Split Failures":
                this.title = "Split Failures";
                break;
            case "Vehicle Detector Uptime":
                this.title = "Detector Uptime";
                break;
            case "Daily Traffic Volumes":
                this.title = "Daily Volume";
                break;
            case "Daily Pedestrian Pushbutton Activity":
                this.title = "Pedestrians";
                break;
            case "Communication Uptime":
                this.title = "Communications Uptime";
                break;
            case "Region Status":
            case "Maintenance":
            case "Maintenance Trend":
            case "Operations":
            case "Operation Trend":
            case "Safety":
            case "Safety Trend":
                this.title = "HealthMetrics";
                break;
            default:
                break;
        }

        for (let index = 0; index < this.helpData.length; index++) {
            let item = this.helpData[index].data.filter(y => y.name === this.title);

            if(item.length > 0){
                var firstP = item[0].htmlText.indexOf("</p>");
                if (firstP != -1) {
                    data = item[0].htmlText.substr(0,firstP) + "<p><a href=" + window.location.origin + "/about>Learn more...</a></p>";
                } else {
                    data = item[0].htmlText + "</p>";
                }
                break;
            }
        }
        return data;
    }

    getAllHelpData(): any [] {
        return this.helpData;
    }

    getPatchNotes(): any[] {
        return this.patchNotes;
    }
}

