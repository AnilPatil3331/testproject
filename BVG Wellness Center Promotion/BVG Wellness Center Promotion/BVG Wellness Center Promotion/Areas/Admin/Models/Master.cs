using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class Master
    {

        public string InstructionName { get; set; }
        public int FormID { get; set; }
        public string FormName { get; set; }
        public string Description { get; set; }

        public bool isActive { get; set; }

        public int? Fid { get; set; }
        public string Mip { get; set; }
        public string Mid { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? Rate { get; set; }


        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public int RoomOccupancy { get; set; }
        //public bool IsActive { get; set; }

    }
}