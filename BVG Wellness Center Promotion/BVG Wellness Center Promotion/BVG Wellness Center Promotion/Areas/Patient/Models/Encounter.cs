using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class Encounter
    {
        public int fid { get; set; }
        public DateTime EncounterDate { get; set; }
        public int ClinicFid { get; set; }
        public int DoctorFid { get; set; }
        public int PatientFid { get; set; }
        public string Description { get; set; }
    }
}