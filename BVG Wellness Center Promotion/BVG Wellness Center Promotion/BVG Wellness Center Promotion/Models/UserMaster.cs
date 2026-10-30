using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Models
{
    public class UserMaster
    {
        public int UserId { get; set; }
        public string UserId_Encrypted { get; set; }
        public DateTime FDate { get; set; }
        public string LoginId { get; set; }
        public string LoginPassword { get; set; }
        public string FirstName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string SpecializationFid { get; set; }
        public int ExperienceInYear { get; set; }
        public string Gender { get; set; }
        public string ProfileImage { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string CityName { get; set; }
        public string Postalcode { get; set; }
        public string Degree { get; set; }
        public string University { get; set; }
        public int Degree_Year { get; set; }
        public int ClinicFid { get; set; }
        public string Status { get; set; }
        public string DepartmentID { get; set; }
        public bool IsDoctor { get; set; }
        public bool IsReceptionist { get; set; }
        public bool IsAdmin { get; set; }
        public string DoctorSignature { get; set; }
        public string DoctorRefrenceNo { get; set; }
        public bool IsPaymentMandatory { get; set; }
        public string Designation { get; set; }
        public bool IsTherapist { get; set; }
    }
}