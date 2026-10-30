using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Setting.Models
{
    public class LocationS
    {


        public int? FId { get; set; }
        public string Taluka { get; set; }
        public string Pincode { get; set; }
        public string Area { get; set; }
        public string  PostOffice { get; set; }
    }
}