using BVG_Wellness_Center_Promotion.Areas.IPD.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Controllers
{
    public class IPD_RoomController : Controller
    {
        private bool IsSessionValid() => Session["UserID"] != null;
        private int GetUserId() => Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

        // =============================================
        // ROOM MASTER — List + Create/Edit
        // =============================================
        public ActionResult RoomMaster()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomModel>("sp_IPD_RoomMaster_List", null).ToList();
            LoadRoomDropdowns();
            return View(list);
        }

        public ActionResult RoomCreate(int id = 0)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var model = new IPD_RoomModel();
            if (id > 0)
            {
                var param = new DynamicParameters();
                param.Add("@RoomId", id);
                model = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomModel>("sp_IPD_RoomMaster_List", param).FirstOrDefault() ?? model;

                // Load mapped charges for this room
                var cparam = new DynamicParameters();
                cparam.Add("@RoomId", id);
                model.MappedCharges = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMapping_GetByRoom", cparam).ToList();
            }
            LoadRoomDropdowns();
            return View(model);
        }

        [HttpPost]
        public JsonResult RoomMaster_Save(IPD_RoomModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.RoomId > 0 ? "Update" : "Save");
                param.Add("@RoomId", model.RoomId);
                param.Add("@RoomNumber", model.RoomNumber);
                param.Add("@FloorId", model.FloorId);
                param.Add("@WardId", model.WardId);
                param.Add("@CategoryId", model.CategoryId);
                param.Add("@RoomChargePerDay", model.RoomChargePerDay);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomMaster_SUD", param);
                bool ok = param.Get<string>("@p_Icon") == "success";
                return Json(new { success = ok, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult RoomMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@RoomId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // AJAX — Get rooms (filtered by floor/ward/category for booking form)
        public JsonResult GetRooms(int? floorId = null, int? wardId = null, int? categoryId = null)
        {
            var param = new DynamicParameters();
            param.Add("@FloorId", floorId);
            param.Add("@WardId", wardId);
            param.Add("@CategoryId", categoryId);
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomModel>("sp_IPD_RoomMaster_List", param)
                       .Select(r => new { r.RoomId, r.RoomNumber, r.RoomChargePerDay, r.FloorName, r.WardName, r.CategoryName }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // AJAX — Get charges for a room (for booking form charge section)
        public JsonResult GetRoomCharges(int roomId)
        {
            var param = new DynamicParameters();
            param.Add("@RoomId", roomId);
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMapping_GetByRoom", param).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // AJAX — Save charge mapping for a room
        [HttpPost]
        public JsonResult SaveChargeMapping(int roomId, int chargeId, decimal chargeAmount, bool isCompulsory)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@RoomId", roomId);
                param.Add("@ChargeId", chargeId);
                param.Add("@ChargeAmount", chargeAmount);
                param.Add("@IsCompulsory", isCompulsory);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomChargesMapping_Save", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // AJAX — Delete charge from room mapping
        [HttpPost]
        public JsonResult DeleteChargeMapping(int mappingId)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@MappingId", mappingId);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_RoomChargesMapping_Delete", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        private void LoadRoomDropdowns()
        {
            ViewBag.Floors = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", null).ToList();
            ViewBag.Wards = BVGWellnessCenterDapparORM.ReturnList<IPD_WardModel>("sp_IPD_WardMaster_List", null).ToList();
            ViewBag.Categories = BVGWellnessCenterDapparORM.ReturnList<IPD_CategoryModel>("sp_IPD_CategoryMaster_List", null).ToList();
            ViewBag.AllCharges = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMaster_List", null).ToList();
            var setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault() ?? new IPD_HospitalSettingModel();
            ViewBag.Settings = setting;
            ViewBag.UseFloor = setting.UseFloor;
            ViewBag.UseWard = setting.UseWard;
            ViewBag.UseCategory = setting.UseCategory;
        }

        // =============================================
        // BED MASTER
        // =============================================
        public ActionResult BedMaster(int? roomId = null)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@RoomId", roomId);
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_BedModel>("sp_IPD_BedMaster_List", param).ToList();
            ViewBag.Rooms = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomModel>("sp_IPD_RoomMaster_List", null)
                           .Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = r.RoomNumber }).ToList();
            ViewBag.SelectedRoomId = roomId;
            return View(list);
        }

        [HttpPost]
        public JsonResult BedMaster_Save(IPD_BedModel model)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", model.BedId > 0 ? "Update" : "Save");
                param.Add("@BedId", model.BedId);
                param.Add("@RoomId", model.RoomId);
                param.Add("@BedNumber", model.BedNumber);
                param.Add("@BedStatus", model.BedStatus);
                param.Add("@IsActive", model.IsActive);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_BedMaster_SUD", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult BedMaster_Delete(int id)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "Delete"); param.Add("@BedId", id);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_BedMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult BedMaster_UpdateStatus(int bedId, string status)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@Action", "UpdateStatus");
                param.Add("@BedId", bedId);
                param.Add("@BedStatus", status);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_BedMaster_SUD", param);
                return Json(new { success = true, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // AJAX — Available beds for a room (booking form)
        public JsonResult GetAvailableBeds(int roomId)
        {
            var param = new DynamicParameters();
            param.Add("@RoomId", roomId);
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_BedModel>("sp_IPD_GetAvailableBeds", param)
                       .Select(b => new { b.BedId, b.BedNumber, b.BedStatus }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
