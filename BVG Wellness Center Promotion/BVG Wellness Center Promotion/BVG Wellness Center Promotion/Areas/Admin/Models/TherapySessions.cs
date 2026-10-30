using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class TherapySessions
    {
        public int SessionFid { get; set; }
        public string SessionFid_Encrypted { get; set; }
        public int ClinicFid { get; set; }
        public int DoctorFid { get; set; }
        public int TimeSlot { get; set; }
        public int Occupancy { get; set; }
        public TimeSpan? MorningSession_From { get; set; }
        public TimeSpan? MorningSession_To { get; set; }
        public TimeSpan? EveningSession_From { get; set; }
        public TimeSpan? EveningSession_To { get; set; }
        public string WeekDays { get; set; }
        public List<string> SelectedDays { get; set; } = new List<string>();

        public int ServiceRoomID { get; set; }

        public string RoomName { get; set; }

        public int ServiceFid { get; set; }

    }
}