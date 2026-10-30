using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class RolePermissionsController : Controller
    {
        // GET: Admin/RolePermissions
        public ActionResult Index(int? roleId)
        {
            var model = new RolePermissionsVM();
            
            // Get all active roles for the dropdown
            DynamicParameters paramRoles = new DynamicParameters();
            paramRoles.Add("@p_Query", "SELECT RoleId AS ID, RoleName AS Name FROM MAS_Role WHERE IsActive = 1");
            var roles = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramRoles).ToList();
            ViewBag.Roles = new SelectList(roles, "ID", "Name", roleId);

            model.RoleId = roleId ?? 0;
            model.Menus = new List<RoleMenuModel>();

            if (model.RoleId > 0)
            {
                // Get all active menus and check if they are mapped to this role
                DynamicParameters paramMenus = new DynamicParameters();
                paramMenus.Add("@p_Query", $@"
                    SELECT 
                        m.MenuId, 
                        m.MenuName, 
                        m.MenuUrl, 
                        m.MenuIcon, 
                        m.ParentMenuId,
                        CAST(CASE WHEN rm.MapId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned
                    FROM MAS_Menu m
                    LEFT JOIN Map_RoleMenu rm ON m.MenuId = rm.MenuId AND rm.RoleId = {model.RoleId}
                    WHERE m.IsActive = 1
                    ORDER BY ISNULL(m.ParentMenuId, 0), m.DisplayOrder
                ");
                
                model.Menus = BVGWellnessCenterDapparORM.ReturnList<RoleMenuModel>("sp_ExecuteQuery", paramMenus).ToList();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SavePermissions(int roleId, List<int> selectedMenus)
        {
            if (roleId <= 0)
            {
                TempData["Error"] = "Please select a valid Role.";
                return RedirectToAction("Index");
            }

            try
            {
                // 1. Delete existing mapping for this role
                DynamicParameters paramDelete = new DynamicParameters();
                paramDelete.Add("@p_Query", $"DELETE FROM Map_RoleMenu WHERE RoleId = {roleId}");
                BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramDelete);

                // 2. Insert new mappings
                if (selectedMenus != null && selectedMenus.Any())
                {
                    foreach (var menuId in selectedMenus)
                    {
                        DynamicParameters paramInsert = new DynamicParameters();
                        paramInsert.Add("@p_Query", $"INSERT INTO Map_RoleMenu (RoleId, MenuId, IsActive) VALUES ({roleId}, {menuId}, 1)");
                        BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramInsert);
                    }
                }

                TempData["Success"] = "Permissions saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while saving permissions: " + ex.Message;
            }

            return RedirectToAction("Index", new { roleId = roleId });
        }
    }
}
