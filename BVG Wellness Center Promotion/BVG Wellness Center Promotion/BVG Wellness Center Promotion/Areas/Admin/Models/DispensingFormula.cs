using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class DispensingFormula
    {
        public int FormulaID { get; set; }
        public string FormulaName { get; set; }
        public bool IsActive { get; set; }
        public string Department { get; set; } // नवीन प्रॉपर्टी
    }
}
