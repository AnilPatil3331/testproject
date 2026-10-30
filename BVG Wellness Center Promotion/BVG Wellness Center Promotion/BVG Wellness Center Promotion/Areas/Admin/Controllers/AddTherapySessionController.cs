using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class AddTherapySessionController : Controller
    {
        string LoginMachinId = Dns.GetHostName();



        #region TherapySessionList
        public ActionResult TherapySessionList(string SessionFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                TherapySessions session = new TherapySessions();

                // ================= EDIT MODE =================
                if (!string.IsNullOrEmpty(SessionFid_Encrypted))
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_SessionFid_Encrypted", SessionFid_Encrypted);

                    session = BVGWellnessCenterDapparORM
                        .ReturnList<TherapySessions>("[sp_List_Mas_TherapySessions]", param2)
                        .FirstOrDefault();

                    if (session != null)
                    {
                        // 🔴 CRITICAL: ensure SessionFid exists for UPDATE
                        // (SP should return SessionFid)
                        session.SessionFid = session.SessionFid;
                        session.SessionFid_Encrypted = SessionFid_Encrypted;

                        // Week days binding
                        session.SelectedDays = string.IsNullOrWhiteSpace(session.WeekDays)
                            ? new List<string>()
                            : session.WeekDays.Split(',').ToList();

                        ViewBag.selectDay = session.SelectedDays;
                        ViewBag.SelectDaysCount = session.SelectedDays.Count == 7 ? 7 : (int?)null;

                        ViewBag.Update = "Update Therapy Session";
                        ViewBag.CheckUpdate = "update";
                    }
                }
                else
                {
                    ViewBag.Update = "Add Therapy Session";
                }

                // ================= DROPDOWNS =================
                DynamicParameters paramClinic = new DynamicParameters();
                paramClinic.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic where Deactivate=0");
                ViewBag.ClinicName = BVGWellnessCenterDapparORM
                    .ReturnList<DropdownItem>("sp_ExecuteQuery", paramClinic);

                DynamicParameters paramRoom = new DynamicParameters();
                paramRoom.Add("@p_Query", "select ServiceRoomID as Id, RoomName as Name from MAS_ServiceRoomType where IsActive=1");
                ViewBag.GetRoom = BVGWellnessCenterDapparORM
                    .ReturnList<DropdownItem>("sp_ExecuteQuery", paramRoom);

                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query",
                    "select ServiceFid as Id, ServiceName as Name from Mas_Service where Status='Active' and Deactivate=0");
                ViewBag.Getdata = BVGWellnessCenterDapparORM
                    .ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);

                // ================= LIST =================
                ViewBag.TherapySessionList =
                    BVGWellnessCenterDapparORM.DynamicList("[sp_List_Mas_TherapySessions]");

                return View(session);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion



        #region SaveTherapySession
        [HttpPost]
        public ActionResult SaveTherapySession(TherapySessions session, string check_all_days)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                string macIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();

                // ================= WEEK DAYS FIX =================
                if (string.IsNullOrEmpty(session.WeekDays) && session.SelectedDays != null)
                {
                    session.WeekDays = string.Join(",", session.SelectedDays);
                }

                if (check_all_days == "All" && !string.IsNullOrEmpty(session.WeekDays))
                {
                    session.WeekDays = string.Join(",",
                        session.WeekDays.Split(',')
                        .Where(x => x != "All")
                        .Select(x => x.Trim()));
                }

                DynamicParameters param = new DynamicParameters();

                // 🔴 CRITICAL FIX: UPDATE detection
                param.Add("@p_ProcessType",
                    session.SessionFid > 0 ? "Update" : "Save");

                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", macIp);

                param.Add("@p_SessionFid", session.SessionFid);
                param.Add("@p_SessionFid_Encrypted", session.SessionFid_Encrypted);

                param.Add("@p_ClinicFid", session.ClinicFid);
                param.Add("@p_ServiceRoomID", session.ServiceRoomID);
                param.Add("@p_ServiceFid", session.ServiceFid);

                param.Add("@p_TimeSlot", session.TimeSlot);
                param.Add("@p_Occupancy", session.Occupancy);

                param.Add("@p_MorningSession_From", session.MorningSession_From);
                param.Add("@p_MorningSession_To", session.MorningSession_To);
                param.Add("@p_EveningSession_From", session.EveningSession_From);
                param.Add("@p_EveningSession_To", session.EveningSession_To);

                param.Add("@p_WeekDays", session.WeekDays);

                param.Add("@p_Message", dbType: DbType.String,
                    direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String,
                    direction: ParameterDirection.Output, size: 50);

                BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_TherapySessions", param);

                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("TherapySessionList", "AddTherapySession", new { area = "Admin" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    

         #region GetDoctor
        public ActionResult GetDoctor(int? ClinicFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                //param.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + ClinicFid + "' ");
                param.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + ClinicFid + "' and IsDoctor=1 and Deactivate=0 ");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);

                return Json(new { data }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        
        #region DeleteSession
        public ActionResult DeleteSession(string SessionFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");

                param.Add("@p_SessionFid_Encrypted", SessionFid_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_TherapySessions", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("TherapySessionList", "AddTherapySession", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}


