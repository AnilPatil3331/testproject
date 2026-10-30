using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class ClsPatient
    {
        public int PatientFid { get; set; }
        public string PatientFid_Encrypted { get; set; }
        public string FirstName { get; set; }
        public string LName { get; set; }
        public int ClinicFid { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }      
        public string BloodGroup { get; set; }

        [Required(ErrorMessage = "Please select Gender")]
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
        public string PatientProfile { get; set; }

        public string Age { get; set; }
        public string HealthIssue { get; set; }
        public string PatientType { get; set; }
        public string PatientReference { get; set; }

        public int Weight { get; set; }
        public string Palse { get; set; }
        public string BloodPressure { get; set; }
        public string SpO2 { get; set; }
        public string UHIDNO { get; set; }
        public string Origin { get; set; }
    }
}