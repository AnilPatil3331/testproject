using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class MastersController : Controller
    {
        // GET: Admin/Masters
        public ActionResult Index()
        {
            return View();
        }


        #region suchna 
        public ActionResult SuchnaMaster(int ? id )
        {
            try
            {
                if (Session["UserID"] == null)
                {

                    return RedirectToAction("Login", "Home", new { area = "" });

                }


                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "SELECT FormID, FormName FROM MAS_Form WHERE IsActive=1");
                ViewBag.Forms = BVGWellnessCenterDapparORM.ReturnList<Master>("sp_ExecuteQuery", param).ToList();

                DynamicParameters paramDoctor = new DynamicParameters();
                paramDoctor.Add("@p_Query", "select MS.Fid as id, MS.FormID, MS.InstructionName, MS.Description, MF.FormName from MAS_SuchanaMaster MS inner join  MAS_Form MF on MS.FormID = MF.FormID   where MS.IsActive = 1");
                var SuchnaList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramDoctor);
                ViewBag.SuchnaList = SuchnaList;

                if (id.HasValue && id > 0)
                {
                    DynamicParameters paramEdit = new DynamicParameters();
                    paramEdit.Add("@p_Query", $@"
                SELECT 
                    Fid,
                    FormID,
                    InstructionName,
                    Description,
                    IsActive
                FROM MAS_SuchanaMaster
                WHERE Fid = {id}
            ");

                    var editData = BVGWellnessCenterDapparORM
                                        .ReturnList<Master>("sp_ExecuteQuery", paramEdit)
                                        .FirstOrDefault();

                    if (editData != null)
                    {
                        ViewBag.Update = true;   // 👉 view मध्ये form open करण्यासाठी
                        return View(editData);   // 👉 model bind
                    }
                }
                return View();
            }

            catch (Exception Ex)
            {
                return View();
            }
          
        }


        [HttpPost]
        public ActionResult SaveAddSuchana(Master model)
        {
            try
            {
                if (Session["UserID"] == null)
                {

                    return RedirectToAction("Login", "Home", new { area = "" });

                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@Action", model.Fid == 0 || model.Fid == null ? "INSERT" : "UPDATE");
                param.Add("@Fid", model.Fid);
                param.Add("@Mip", model.Mip);
                param.Add("@Mid", model.Mid);
                param.Add("@CreatedBy",1);
                param.Add("@UpdatedBy", 1);
                param.Add("@FormID", model.FormID);
                param.Add("@InstructionName", model.InstructionName);
                param.Add("@Description", model.Description);
                param.Add("@IsActive", model.isActive);

                var result = BVGWellnessCenterDapparORM.ReturnList<Master>("SP_MAS_SuchanaMaster_CRUD", param).FirstOrDefault();

                TempData["Message"] = result != null ? result.Description : "Operation completed successfully.";

                return RedirectToAction("SuchnaMaster");  // Redirect to list page
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public JsonResult DeleteSuchna(int id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session Expired" },
                                JsonRequestBehavior.AllowGet);
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@Action", "DELETE");
                param.Add("@Fid", id);
                param.Add("@UpdatedBy", Session["UserID"].ToString());

                var result = BVGWellnessCenterDapparORM .ReturnList<Master>("SP_MAS_SuchanaMaster_CRUD", param);

                return Json(new { success = true, message = "Deleted Successfully" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion suchna 

        #region RoomType 
        [HttpGet]
        public ActionResult RoomTypeMaster(int? RoomID)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home", new { area = "" });

            DynamicParameters param = new DynamicParameters();
            param.Add("@p_Query", "SELECT * FROM MAS_RoomType WHERE IsActive = 1");
            ViewBag.RoomTypeList = BVGWellnessCenterDapparORM
                                        .ReturnList<dynamic>("sp_ExecuteQuery", param);

            Master model = new Master();

            if (RoomID != null)
            {
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query",
                    $"SELECT * FROM MAS_RoomType WHERE IsActive = 1 AND RoomID = {RoomID}");

                model = BVGWellnessCenterDapparORM
                            .ReturnList<Master>("sp_ExecuteQuery", param1)
                            .FirstOrDefault();

                ViewBag.Update = true;
            }

            return View(model);
        }


        [HttpPost]
        public ActionResult SaveRoomType(Master model)
        {
            try
            {
                if (Session["UserID"] == null)
                {

                    return RedirectToAction("Login", "Home", new { area = "" });

                }

                DynamicParameters param = new DynamicParameters();
             
                param.Add("@RoomID", model.RoomID);
                param.Add("@RoomName", model.RoomName);
                param.Add("@RoomOccupancy", model.RoomOccupancy);
                param.Add("@Rate", model.Rate);
                param.Add("@IsActive", true);
                param.Add("@Message", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

                var result = BVGWellnessCenterDapparORM.ReturnList<Master>("Sp_RoomType", param).FirstOrDefault();

                TempData["Message"] = result != null ? result.Description : "Operation completed successfully.";

                return RedirectToAction("RoomTypeMaster");  // Redirect to list page
//return View();
            }
            catch(Exception Ex)
            {
                return View();
            }
        }

        #endregion Roomtype
        [HttpGet]
        public ActionResult DeleteRoomType(int RoomID)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("LoginPage", "Home", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@RoomID", RoomID);
                param.Add("@RoomName", "");
                param.Add("@RoomOccupancy", 0);
                param.Add("@Rate", 0);
                param.Add("@IsActive", false);
                param.Add("@Message", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

                var result = BVGWellnessCenterDapparORM.ReturnList<Master>("Sp_RoomType", param).FirstOrDefault();

                TempData["Message"] = "Room deleted successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("RoomTypeMaster", "Masters", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #region Pathya

        [HttpGet]
        public ActionResult PathyaMaster(int? id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction(
                        "Login",
                        "Home",
                        new { area = "" });
                }

                Pathya model = new Pathya();
                // Get All Pathya List
                DynamicParameters param =
                    new DynamicParameters();

                param.Add(
        "@p_Query",
        @"SELECT 
        Id,
        PathyaName,
        PathyaInstruction
      FROM tbl_PathyaMaster
      ORDER BY Id ASC"
    );

                ViewBag.PathyaList =
                    BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>(
                        "sp_ExecuteQuery",
                        param
                    );

                // Edit
                if (id.HasValue && id > 0)
                {
                    DynamicParameters editParam =
                        new DynamicParameters();

                    editParam.Add(
                        "@p_Query",
                        $@"SELECT 
                        Id,
                        PathyaName,
                        PathyaInstruction
                   FROM tbl_PathyaMaster
                   WHERE Id = {id}"
                    );

                    var editData =
                        BVGWellnessCenterDapparORM
                        .ReturnList<Pathya>(
                            "sp_ExecuteQuery",
                            editParam
                        )
                        .FirstOrDefault();

                    if (editData != null)
                    {
                        ViewBag.Update = true;
                        return View(editData);
                    }
                }

                return View(model);
            }
            catch (Exception)
            {
                return View();
            }
        }


        // SAVE / UPDATE
        [HttpPost]
        public ActionResult SaveAddPathya(Pathya model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction(
                        "Login",
                        "Home",
                        new { area = "" });
                }

                DynamicParameters param =
                    new DynamicParameters();

                param.Add(
                    "@ProcessType",
                    model.Id > 0
                    ? "UPDATE"
                    : "INSERT"
                );

                param.Add("@Id", model.Id);
                param.Add("@PathyaName", model.PathyaName);
                param.Add("@PathyaInstruction",
                          model.PathyaInstruction);

                BVGWellnessCenterDapparORM
                .ReturnList<dynamic>(
                    "sp_PathyaMaster",
                    param
                );

                TempData["Message"] =
                    model.Id > 0
                    ? "Pathya Updated Successfully"
                    : "Pathya Saved Successfully";

                TempData["Icon"] = "success";

                return RedirectToAction(
                    "PathyaMaster"
                );
            }
            catch (Exception)
            {
                TempData["Message"] =
                    "Something went wrong";

                TempData["Icon"] =
                    "error";

                return RedirectToAction(
                    "PathyaMaster"
                );
            }
        }


        // DELETE
        [HttpGet]
        public JsonResult DeletePathya(int id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message =
                            "Session Expired"
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                DynamicParameters param =
                    new DynamicParameters();

                param.Add(
                    "@ProcessType",
                    "DELETE"
                );

                param.Add("@Id", id);

                BVGWellnessCenterDapparORM
                .ReturnList<dynamic>(
                    "sp_PathyaMaster",
                    param
                );

                return Json(
                    new
                    {
                        success = true,
                        message =
                        "Deleted Successfully"
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                        "Delete Failed"
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #endregion

        #region Dispensing Formula Master

        [HttpGet]
        public ActionResult DispensingFormulaMaster(int? id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DispensingFormula model = new DispensingFormula() { IsActive = true };

                // Get All Formula List with Department
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "SELECT FormulaID, FormulaName, IsActive, Department FROM MAS_Formula ORDER BY FormulaID ASC");

                ViewBag.FormulaList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);

                // Edit
                if (id.HasValue && id > 0)
                {
                    DynamicParameters editParam = new DynamicParameters();
                    editParam.Add("@p_Query", $"SELECT FormulaID, FormulaName, IsActive, Department FROM MAS_Formula WHERE FormulaID = {id}");

                    var editData = BVGWellnessCenterDapparORM.ReturnList<DispensingFormula>("sp_ExecuteQuery", editParam).FirstOrDefault();

                    if (editData != null)
                    {
                        ViewBag.Update = true;
                        return View("~/Areas/Patient/Views/DispensingMasters/DispensingFormulaMaster.cshtml", editData);
                    }
                }

                return View("~/Areas/Patient/Views/DispensingMasters/DispensingFormulaMaster.cshtml", model);
            }
            catch (Exception)
            {
                return View("~/Areas/Patient/Views/DispensingMasters/DispensingFormulaMaster.cshtml");
            }
        }

        [HttpPost]
        public ActionResult SaveAddDispensingFormula(DispensingFormula model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@ProcessType", model.FormulaID > 0 ? "UPDATE" : "INSERT");
                param.Add("@FormulaID", model.FormulaID);
                param.Add("@FormulaName", model.FormulaName);
                param.Add("@IsActive", model.IsActive);
                param.Add("@Department", model.Department); // Controller मधून SP ला डेटा पाठवण्यासाठी

                BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_MAS_Formula_CRUD", param);

                TempData["Message"] = model.FormulaID > 0 ? "Formula Updated Successfully" : "Formula Saved Successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("DispensingFormulaMaster");
            }
            catch (Exception)
            {
                TempData["Message"] = "Something went wrong";
                TempData["Icon"] = "error";

                return RedirectToAction("DispensingFormulaMaster");
            }
        }



        // DELETE
        [HttpGet]
        public JsonResult DeleteDispensingFormula(int id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session Expired" }, JsonRequestBehavior.AllowGet);
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@ProcessType", "DELETE");
                param.Add("@FormulaID", id);

                BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_MAS_Formula_CRUD", param);

                return Json(new { success = true, message = "Deleted Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Delete Failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


    }
}