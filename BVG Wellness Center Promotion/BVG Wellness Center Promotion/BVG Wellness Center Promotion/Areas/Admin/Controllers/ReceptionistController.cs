using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class ReceptionistController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Admin/Receptionist       
        public ActionResult AddReceptionist(string User_EncryptedId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                UserMaster user = new UserMaster();
                if (User_EncryptedId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();

                    param1.Add("@p_IsReceptionist", true);
                    param1.Add("@p_UserId_Encrypted", User_EncryptedId);
                    user = BVGWellnessCenterDapparORM.ReturnList<UserMaster>("sp_List_UserList", param1).FirstOrDefault();
                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(user.ProfileImage);
                    ViewBag.ProfileImageFil = base64Image;
                    ViewBag.PraviousProfileImageFil = user.ProfileImage;
                    ViewBag.Status = user.Status;
                    DynamicParameters paramClinic = new DynamicParameters();
                    paramClinic.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic");
                    var GetClinicName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramClinic);
                    if (GetClinicName.Count() == 0)
                    {
                        ViewBag.ClinicName = "";
                    }
                    else
                    {
                        ViewBag.ClinicName = GetClinicName;
                    }
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if (GetClinic.Count() == 0)
                {
                    ViewBag.ClinicName = "";
                }
                else
                {
                    ViewBag.ClinicName = GetClinic;
                }
                return View(user);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #region SaveReceptionist
        public ActionResult SaveReceptionist(UserMaster user,HttpPostedFileBase file,string PreviousProfileImage)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                int DegreeYear = user.Degree_Year;
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                //string DefaultLoginPass = "@123#";
                //string DefaultLoginId = user.FirstName;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(user.UserId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_UserID", user.UserId);
                param.Add("@p_UserId_Encrypted", user.UserId_Encrypted);

                if (user.Designation != "Receptionist" && user.Designation != "Therapist")
                {
                    user.LoginId = null;
                    user.LoginPassword = null;
                }

                param.Add("@p_LoginId", user.LoginId);
                param.Add("@p_LoginPassword", user.LoginPassword);
                param.Add("@p_FirstName", user.FirstName);
                param.Add("@p_LName", user.LName);
                param.Add("@p_Email", user.Email);
                param.Add("@p_ClinicFid", user.ClinicFid);
                param.Add("@p_ContactNumber", user.ContactNumber);
                param.Add("@p_DateOfBirth", user.DateOfBirth);                
                param.Add("@p_ExperienceInYear", user.ExperienceInYear);
                param.Add("@p_Gender", user.Gender);               

                string FilePath = @"C:\BVGWellnessCenter\ReceptionistImage\\" + user.ContactNumber + "\\";
                if (File!=null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@p_ProfileImage", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                else if (PreviousProfileImage != null && PreviousProfileImage != "")
                {
                    param.Add("@p_ProfileImage", PreviousProfileImage);
                }
                param.Add("@p_Address", user.Address);
                param.Add("@p_Country", user.Country);
                param.Add("@p_CityName", user.CityName);
                param.Add("@p_Postalcode", user.Postalcode);              
                param.Add("@p_IsDoctor", false);
                
                bool isReceptionist = (user.Designation == "Receptionist");
                bool isTherapist = (user.Designation == "Therapist");

                param.Add("@p_IsReceptionist", isReceptionist);
                param.Add("@p_IsAdmin", false);
                param.Add("@p_IsTherapist", isTherapist);
                param.Add("@p_Designation", user.Designation);
                param.Add("@p_Status", user.Status);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_UserMaster", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ReceptionistList", "Receptionist", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
        public ActionResult ReceptionistList()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_IsReceptionist", true);
                var GetDoctorData = BVGWellnessCenterDapparORM.DynamicList("sp_List_UserList", param);
                ViewBag.DoctorData = GetDoctorData;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }

        #region ChangeReceptionnistStatus
        public ActionResult ChangeReceptionnistStatus(string UserId_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update UserMaster set Status='" + Status + "' where  Deactivate=0 and UserId_Encrypted='" + UserId_Encrypted + "'";
                var data = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                if (Status == "Active")
                {
                    TempData["Message"] = "Status changed to Active";
                    TempData["Icon"] = "Success";
                }
                else if (Status == "Inactive")
                {
                    TempData["Message"] = "Status changed to Inactive";
                    TempData["Icon"] = "Success";
                }

                return RedirectToAction("ReceptionistList", "Receptionist", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteDoctor
        public ActionResult Deletereceptionist(string UserId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_UserId_Encrypted", UserId_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_UserMaster", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ReceptionistList", "Receptionist", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}