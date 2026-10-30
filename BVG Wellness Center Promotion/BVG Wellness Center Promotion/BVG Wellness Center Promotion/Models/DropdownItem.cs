using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Models
{
    public class DropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ServiceGroupID { get; set; }
    }
}