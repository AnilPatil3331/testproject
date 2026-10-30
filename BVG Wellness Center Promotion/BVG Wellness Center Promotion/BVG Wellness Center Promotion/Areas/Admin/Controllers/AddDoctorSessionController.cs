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
    public class AddDoctorSessionController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Admin/AddDoctorSession
        #region DoctorSessionList
        public ActionResult DoctorSessionList(string SessionFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //DoctorSessions session = new DoctorSessions();
                //if(SessionFid_Encrypted!=null)
                //{
                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where Deactivate=0 ");
                //    var DoctorName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param1);
                //    ViewBag.GetDoctorName = DoctorName;
                //    DynamicParameters param2 = new DynamicParameters();
                //    param2.Add("@p_SessionFid_Encrypted", SessionFid_Encrypted);
                //    session = BVGWellnessCenterDapparORM.ReturnList<DoctorSessions>("sp_List_mas_DoctorSessions", param2).FirstOrDefault();
                //    ViewBag.Update = "Update Doctor Sessions";
                //    ViewBag.CheckUpdate = "update";
                //    //SelectedDays = session.WeekDays.Split(',').ToList();
                //    session.SelectedDays = string.IsNullOrWhiteSpace(session.WeekDays)? new List<string>(): session.WeekDays.Split(',').ToList();
                //    ViewBag.selectDay = session.SelectedDays;

                //    if (session.SelectedDays.Count==7)
                //    {
                //        ViewBag.SelectDaysCount = session.SelectedDays.Count;
                //    }else
                //    {
                //        ViewBag.SelectDaysCount = null;
                //    }

                //}
                DoctorSessions session = new DoctorSessions();
                if (SessionFid_Encrypted != null)
                {
                    // 1. प्रथम session माहिती लोड करा जेणेकरून आपल्याला ClinicFid मिळेल
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_SessionFid_Encrypted", SessionFid_Encrypted);
                    session = BVGWellnessCenterDapparORM.ReturnList<DoctorSessions>("sp_List_mas_DoctorSessions", param2).FirstOrDefault();

                    // 2. फक्त त्या Clinic मधील आणि Active Doctors ची लिस्ट घ्या
                    DynamicParameters param1 = new DynamicParameters();
                    if (session != null)
                    {
                        param1.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + session.ClinicFid + "' and IsDoctor=1 and Deactivate=0 ");
                    }
                    else
                    {
                        param1.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where IsDoctor=1 and Deactivate=0 ");
                    }

                    var DoctorName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param1);
                    ViewBag.GetDoctorName = DoctorName;

                    ViewBag.Update = "Update Doctor Sessions";
                    ViewBag.CheckUpdate = "update";

                    if (session != null)
                    {
                        session.SelectedDays = string.IsNullOrWhiteSpace(session.WeekDays) ? new List<string>() : session.WeekDays.Split(',').ToList();
                        ViewBag.selectDay = session.SelectedDays;

                        if (session.SelectedDays.Count == 7)
                        {
                            ViewBag.SelectDaysCount = session.SelectedDays.Count;
                        }
                        else
                        {
                            ViewBag.SelectDaysCount = null;
                        }
                    }
                }

                else
                {
                    ViewBag.Update = "Doctor Sessions";
                    ViewBag.GetDoctorName = "";
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic where Deactivate=0");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if (GetClinic.Count() == 0)
                {
                    ViewBag.ClinicName = "";
                }
                else
                {
                    ViewBag.ClinicName = GetClinic;
                }

                var data = BVGWellnessCenterDapparORM.DynamicList("sp_List_mas_DoctorSessions");
                ViewBag.DoctorSessionList = data;


               
               
                return View(session);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion


        #region SaveDoctorSession
        [HttpPost]
        public ActionResult SaveDoctorSession(DoctorSessions session, string check_all_days)
        {
            try
            {
               
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                if (check_all_days == "All")
                {
                    string[] Days = session.WeekDays.Split(',');
                    List<string> UpdateDays = new List<string>();
                    foreach (var item in Days)
                    {
                        if (item != "All")
                        {

                            UpdateDays.Add(item.Trim());
                        }
                    }
                    session.WeekDays = string.Join(",", UpdateDays);
                }


                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(session.SessionFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_SessionFid", session.SessionFid);
                param.Add("@p_SessionFid_Encrypted", session.SessionFid_Encrypted);
                param.Add("@p_ClinicFid", session.ClinicFid);
                param.Add("@p_DoctorFid", session.DoctorFid);
                param.Add("@p_TimeSlot", session.TimeSlot);
                param.Add("@p_MorningSession_From", session.MorningSession_From);
                param.Add("@p_MorningSession_To", session.MorningSession_To);
                param.Add("@p_EveningSession_From", session.EveningSession_From);
                param.Add("@p_EveningSession_To", session.EveningSession_To);
                param.Add("@p_WeekDays", session.WeekDays);
                param.Add("@p_SlotCount", session.SlotCount);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_mas_DoctorSessions", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("DoctorSessionList", "AddDoctorSession", new { area = "Admin" });
            }
            catch (Exception ex)
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
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_mas_DoctorSessions", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("DoctorSessionList", "AddDoctorSession", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}