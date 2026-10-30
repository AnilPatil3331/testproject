using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Models
{
    public class IPD
    {

     
            // Patient Personal Details
            public string Prefix { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DOB { get; set; }
            public int? Age { get; set; }
            public string ContactNo { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
            public string MaritalStatus { get; set; }

            // Address
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Pin { get; set; }

            // Identity & Emergency
            public string Aadhaar { get; set; }
            public string EmergencyName { get; set; }
            public string EmergencyNo { get; set; }
            public string TPA { get; set; }
            public string RefDoctor { get; set; }

            // Admission Details
            public string AdmissionNo { get; set; }
            public DateTime? AdmitDateTime { get; set; }
            public string AdmissionType { get; set; }

            // Reason / Therapy Multiselect
            public List<int> ReasonList { get; set; }  // Therapies selected

            // Department & Doctors
            public string Department { get; set; }
            public string Vaidya { get; set; }
            public string ResidentDoctor { get; set; }

            // Room Information
            public string RoomType { get; set; }
            public string RoomName { get; set; }
        


    }
}