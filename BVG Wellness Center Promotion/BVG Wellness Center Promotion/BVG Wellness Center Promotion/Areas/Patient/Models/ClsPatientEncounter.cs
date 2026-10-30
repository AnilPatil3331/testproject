using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class ClsPatientEncounter
    {
        public int EncounterFid { get; set; }
        public string  EncountrFid_Encrypted { get; set; }
        public DateTime EncounterDate { get; set; } = DateTime.Now;
        public int ClinicFid { get; set; }
        public int DoctorFid { get; set; }
        public int PatientFid { get; set; }
        public string Description { get; set; }
        public string AttachmentFile { get; set; }
    }
}