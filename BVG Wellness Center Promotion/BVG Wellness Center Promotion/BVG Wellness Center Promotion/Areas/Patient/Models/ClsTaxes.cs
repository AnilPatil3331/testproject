using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class ClsTaxes
    {
        public int TaxFid { get; set; }
        public string TaxFid_Encrypted { get; set; }
        public string Tax_Name { get; set; }
        public int Tax_Rate { get; set; }
        public List<int> ListClinicFid { get; set; }
        public string ClinicFid { get; set; }
        public List<int> ListDoctorFid { get; set; }
        public string DoctorFid { get; set; }
        public List<string> ListService { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        public string TaxType { get; set; }
    }
}