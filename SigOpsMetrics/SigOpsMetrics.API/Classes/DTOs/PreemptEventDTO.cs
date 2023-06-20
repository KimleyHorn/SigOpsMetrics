using System;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class PreemptEventDTO
    {

        public DateTime? InputOn { get; set; }
        public DateTime? InputOff { get; set; }
        public DateTime? EntryStart { get; set; }
        public DateTime? TrackClear { get; set; }
        public DateTime? DwellService { get; set; }
        public DateTime? ExitCall { get; set; }
        public long? SignalID { get; set; }

        public TimeSpan Duration { get; set; }

        public string? PreemptType { get; set; }

        public bool? ExternalCallOn { get; set; }
        public bool? ExternalCallOff { get; set; }

        
        /// <summary>
        /// A default preempt DTO constructor that sets all parameters to their default values.
        /// </summary>
        public PreemptEventDTO()
        {
            InputOn = default;
            InputOff = default;
            EntryStart = default;
            TrackClear = default;
            DwellService = default;
            ExitCall = default;
            SignalID = 0;
            Duration = default;
            PreemptType = "";
        }

        /// <summary>
        /// A preempt DTO constructor that sets all parameters to their respective values.
        /// </summary>
        /// <param name="inputOn">The date and time that the sensor relays the signal for "preempt input on" </param>
        /// <param name="inputOff">The date and time that the sensor relays the signal for "preempt input off"</param>
        /// <param name="entryStart">The date and time that the sensor relays the signal for "preempt entry started "</param>
        /// <param name="trackClear">The date and time that the sensor relays the signal for "track clearance"</param>
        /// <param name="dwellService">The date and time that the sensor relays the signal for "begin dwell service</param>
        /// <param name="exitCall">The date and time that the sensor relays the signal for "begin exit call"</param>
        /// <param name="signalID">The ID of the intersection where the signal originates from</param>
        /// <param name="duration">The length in seconds of the preempt event</param>
        /// <param name="preemptType">The type of preempt</param>
        public PreemptEventDTO(DateTime? inputOn, DateTime? inputOff, DateTime? entryStart, DateTime? trackClear,
            DateTime? dwellService, DateTime? exitCall, long? signalID, TimeSpan duration, string? preemptType = "generic")
        {
            InputOn = inputOn;
            InputOff = inputOff;
            EntryStart = entryStart;
            TrackClear = trackClear;
            DwellService = dwellService;
            ExitCall = exitCall;
            SignalID = signalID;
            Duration = duration;
            PreemptType = preemptType;
        }

    }
}
