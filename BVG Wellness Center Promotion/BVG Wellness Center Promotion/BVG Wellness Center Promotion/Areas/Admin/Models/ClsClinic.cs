using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class ClsClinic
    {
        public int Fid { get; set; }
        public string Fid_Encrypted { get; set; }
        public int ClinicAdminFid { get; set; }
        public string ClinicAdminFid_Encrypted { get; set; }
        public string Specialities { get; set; }
        public string ClinicName { get; set; }
        public string ClinicEmail { get; set; }
        public string ClinicContact { get; set; }
        public string Status { get; set; }
        public string ClinicAddress { get; set; }
        public string CityName { get; set; }
        public string Country { get; set; }
        public string postalcode { get; set; }
        public string ClinicAdminFName { get; set; }
        public string ClinicAdminLName { get; set; }
        public string ClinicAdminEmail { get; set; }
        public string ClinicAdminContact { get; set; }
        public DateTime ClinicAdminDOB { get; set; } = DateTime.Now;
        public string Gender { get; set; }
        public string ClinicImageFile { get; set; }
        public string ClinicAdmin_Image { get; set; }
    }
}