using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class RolePermissionsVM
    {
        public int RoleId { get; set; }
        public List<RoleMenuModel> Menus { get; set; }
    }

    public class RoleMenuModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public string MenuIcon { get; set; }
        public int? ParentMenuId { get; set; }
        public bool IsAssigned { get; set; }
    }

    public static class MenuHelper
    {
        public static List<RoleMenuModel> GetDynamicMenus(string roleName)
        {
            var param = new Dapper.DynamicParameters();
            param.Add("@p_Query", $@"
                SELECT m.MenuId, m.MenuName, m.MenuUrl, m.MenuIcon, m.ParentMenuId 
                FROM MAS_Menu m
                INNER JOIN Map_RoleMenu rm ON m.MenuId = rm.MenuId
                INNER JOIN MAS_Role r ON rm.RoleId = r.RoleId
                WHERE r.RoleName = '{roleName}' AND m.IsActive = 1 AND r.IsActive = 1 AND rm.IsActive = 1
                ORDER BY ISNULL(m.ParentMenuId, 0), m.DisplayOrder");
                
            return BVG_Wellness_Center_Promotion.Models.BVGWellnessCenterDapparORM.ReturnList<RoleMenuModel>("sp_ExecuteQuery", param).ToList();
        }
    }
}
