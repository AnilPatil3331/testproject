using BVG_Wellness_Center_Promotion.Areas.IPD.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Controllers
{
    public class IPD_MastersController : Controller
    {
        #region SESSION CHECK
        private bool IsSessionValid()
        {
            return Session["UserID"] != null;
        }
        private int GetUserId()
        {
            return Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
        }
        #endregion

        // =============================================
        // FLOOR MASTER
        // =============================================
        #region FLOOR MASTER
        public ActionResult FloorMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", param).ToList();
            return View(list);
        }

        [HttpPost]
        public JsonResult FloorMaster_Save(IPD_FloorModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.FloorId > 0 ? "Update" : "Save");
                param.Add("@FloorId", model.FloorId);
                param.Add("@FloorName", model.FloorName);
                param.Add("@FloorNumber", model.FloorNumber);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_FloorMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult FloorMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@FloorId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_FloorMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetFloors()
        {
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", null)
                       .Select(f => new { f.FloorId, f.FloorName }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // =============================================
        // WARD MASTER
        // =============================================
        #region WARD MASTER
        public ActionResult WardMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_WardModel>("sp_IPD_WardMaster_List", null).ToList();
            var setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault() ?? new IPD_HospitalSettingModel();
            ViewBag.Settings = setting;
            if (setting.EnableHierarchicalMapping)
            {
                ViewBag.Floors = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", null).Where(f => f.IsActive).ToList();
            }
            return View(list);
        }

        [HttpPost]
        public JsonResult WardMaster_Save(IPD_WardModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.WardId > 0 ? "Update" : "Save");
                param.Add("@WardId", model.WardId);
                param.Add("@WardName", model.WardName);
                param.Add("@WardType", model.WardType);
                param.Add("@IsActive", model.IsActive);
                param.Add("@FloorId", model.FloorId);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_WardMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult WardMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@WardId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_WardMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetWards()
        {
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_WardModel>("sp_IPD_WardMaster_List", null)
                       .Select(w => new { w.WardId, w.WardName, w.WardType }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // =============================================
        // CATEGORY MASTER
        // =============================================
        #region CATEGORY MASTER
        public ActionResult CategoryMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_CategoryModel>("sp_IPD_CategoryMaster_List", null).ToList();
            var setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault() ?? new IPD_HospitalSettingModel();
            ViewBag.Settings = setting;
            if (setting.EnableHierarchicalMapping)
            {
                ViewBag.Floors = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", null).Where(f => f.IsActive).ToList();
                ViewBag.Wards = BVGWellnessCenterDapparORM.ReturnList<IPD_WardModel>("sp_IPD_WardMaster_List", null).Where(w => w.IsActive).ToList();
            }
            return View(list);
        }

        [HttpPost]
        public JsonResult CategoryMaster_Save(IPD_CategoryModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.CategoryId > 0 ? "Update" : "Save");
                param.Add("@CategoryId", model.CategoryId);
                param.Add("@CategoryName", model.CategoryName);
                param.Add("@FoodOption", model.FoodOption);
                param.Add("@FloorId", model.FloorId);
                param.Add("@WardId", model.WardId);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_CategoryMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult CategoryMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@CategoryId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_CategoryMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetCategories()
        {
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_CategoryModel>("sp_IPD_CategoryMaster_List", null)
                       .Select(c => new { c.CategoryId, c.CategoryName, c.FoodOption }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // =============================================
        // ROOM CHARGES MASTER
        // =============================================
        #region ROOM CHARGES MASTER
        public ActionResult RoomChargesMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMaster_List", null).ToList();
            return View(list);
        }

        [HttpPost]
        public JsonResult RoomChargesMaster_Save(IPD_RoomChargesModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.ChargeId > 0 ? "Update" : "Save");
                param.Add("@ChargeId", model.ChargeId);
                param.Add("@ChargeName", model.ChargeName);
                param.Add("@ChargeType", model.ChargeType);
                param.Add("@DefaultAmount", model.DefaultAmount);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomChargesMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult RoomChargesMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@ChargeId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomChargesMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetAllCharges()
        {
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMaster_List", null).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // =============================================
        // FOOD PLAN MASTER
        // =============================================
        #region FOOD PLAN MASTER
        public ActionResult FoodPlanMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_FoodPlanModel>("sp_IPD_FoodPlanMaster_List", null).ToList();
            return View(list);
        }

        [HttpPost]
        public JsonResult FoodPlanMaster_Save(IPD_FoodPlanModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.FoodPlanId > 0 ? "Update" : "Save");
                param.Add("@FoodPlanId", model.FoodPlanId);
                param.Add("@PlanName", model.PlanName);
                param.Add("@DailyCharge", model.DailyCharge);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_FoodPlanMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult FoodPlanMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@FoodPlanId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_FoodPlanMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetFoodPlans()
        {
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_FoodPlanModel>("sp_IPD_FoodPlanMaster_List", null)
                       .Select(f => new { f.FoodPlanId, f.PlanName, f.DailyCharge }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // =============================================
        // HOSPITAL SETTINGS
        // =============================================
        #region HOSPITAL SETTINGS
        public ActionResult HospitalSetting()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault()
                          ?? new IPD_HospitalSettingModel();
            return View(setting);
        }

        [HttpPost]
        public JsonResult HospitalSetting_Save(IPD_HospitalSettingModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@UseFloor", model.UseFloor);
                param.Add("@UseWard", model.UseWard);
                param.Add("@UseCategory", model.UseCategory);
                param.Add("@FoodModuleEnabled", model.FoodModuleEnabled);
                param.Add("@FoodMandatory", model.FoodMandatory);
                param.Add("@AdvanceRequired", model.AdvanceRequired);
                param.Add("@BedTransferAllowed", model.BedTransferAllowed);
                param.Add("@PackageBillingEnabled", model.PackageBillingEnabled);
                param.Add("@AttendantAllowed", model.AttendantAllowed);
                param.Add("@AutoReleaseBedOnDischarge", model.AutoReleaseBedOnDischarge);
                param.Add("@GSTEnabled", model.GSTEnabled);
                param.Add("@GSTRate", model.GSTRate);
                param.Add("@EnableHierarchicalMapping", model.EnableHierarchicalMapping);
                param.Add("@UpdatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_HospitalSetting_Save", param);
                // Save to session for fast access
                Session["IPD_Settings"] = model;
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GetHospitalSettings()
        {
            var setting = Session["IPD_Settings"] as IPD_HospitalSettingModel;
            if (setting == null)
            {
                setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault()
                          ?? new IPD_HospitalSettingModel();
                Session["IPD_Settings"] = setting;
            }
            return Json(setting, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
