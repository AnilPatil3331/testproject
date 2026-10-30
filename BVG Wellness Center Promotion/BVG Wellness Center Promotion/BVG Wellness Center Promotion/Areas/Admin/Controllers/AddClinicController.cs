using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
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
    public class AddClinicController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Admin/AddClinic
        public ActionResult AddClinic(string Fid_Encrypted)
        {

            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                ClsClinic clinic = new ClsClinic();
                if (Fid_Encrypted != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Fid_Encrypted", Fid_Encrypted);
                    clinic = BVGWellnessCenterDapparORM.ReturnList<ClsClinic>("sp_List_Mas_Clinic", param).FirstOrDefault();
                    ViewBag.Status = clinic.Status;
                    ViewBag.Gender = clinic.Gender;

                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(clinic.ClinicImageFile);
                    ViewBag.ClinicImageFil = base64Image;
                    ViewBag.PraviousClinicImageFil = clinic.ClinicImageFile;
                    string AdminImagebase64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(clinic.ClinicAdmin_Image);
                    ViewBag.ClinicAdminImage = AdminImagebase64Image;
                    ViewBag.PraviousAdminImage = clinic.ClinicAdmin_Image;
                    List<DropdownItem> specialties = new List<DropdownItem>
                    {
                        new DropdownItem { Id = 1, Name = "Allergy &amp; Immunology" },
                        new DropdownItem { Id = 2, Name = "Dermatology" },
                        new DropdownItem { Id = 3, Name = "Family Medicine" },
                        new DropdownItem { Id = 4, Name = "Neurology" },
                        new DropdownItem { Id = 5, Name = "General Medicine" }
                    };

                    // Assign specialties to ViewBag
                    ViewBag.Specialties = specialties;
                }
                else
                {
                    ViewBag.Gender = null;
                    ViewBag.ClinicImageFile = null;
                    ViewBag.ClinicAdminImage = null;
                    List<DropdownItem> specialties = new List<DropdownItem>
                    {
                         new DropdownItem { Id = 1, Name = "Allergy &amp; Immunology" },
                        new DropdownItem { Id = 2, Name = "Dermatology" },
                        new DropdownItem { Id = 3, Name = "Family Medicine" },
                        new DropdownItem { Id = 4, Name = "Neurology" },
                        new DropdownItem { Id = 5, Name = "General Medicine" }
                    };

                    // Assign specialties to ViewBag
                    ViewBag.Specialties = specialties;
                }
                return View(clinic);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        [HttpPost]
        public ActionResult SaveClinic(ClsClinic clinic, HttpPostedFileBase file, HttpPostedFileBase admin_file, string PreviousClinicImageFil, string PreviousAdminImage)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                HttpPostedFile adminfile = System.Web.HttpContext.Current.Request.Files["admin_file"];
                DynamicParameters paramadmin = new DynamicParameters();
                paramadmin.Add("@p_ProcessType", string.IsNullOrEmpty(clinic.ClinicAdminFid_Encrypted) ? "Save" : "Update");
                paramadmin.Add("@p_MacId", LoginMachinId);
                paramadmin.Add("@p_MacIp", LoginmachinIp);
                paramadmin.Add("@p_ClinicAdminFid", clinic.ClinicAdminFid);
                paramadmin.Add("@p_ClinicAdminFid_Encrypted", clinic.ClinicAdminFid_Encrypted);
                paramadmin.Add("@p_ClinicAdminName", clinic.ClinicAdminFName);
                paramadmin.Add("@p_ClinicAdminLName", clinic.ClinicAdminLName);
                paramadmin.Add("@p_ClinicAdminEmail", clinic.ClinicAdminEmail);
                paramadmin.Add("@p_ClinicAdminContact", clinic.ClinicAdminContact);
                paramadmin.Add("@p_ClinicAdminDOB", clinic.ClinicAdminDOB);
                paramadmin.Add("@p_Gender", clinic.Gender);
                string AdminFilePath = @"C:\BVGWellnessCenter\ClinicAdminImage\\" + clinic.ClinicAdminContact + "\\";
                if (adminfile != null && adminfile.ContentLength > 0)
                {
                    if (!Directory.Exists(AdminFilePath))
                    {
                        Directory.CreateDirectory(AdminFilePath);
                    }
                    adminfile.SaveAs(AdminFilePath + Path.GetFileName((adminfile.FileName)));
                    paramadmin.Add("@p_ClinicAdmin_Image", Convert.ToString(AdminFilePath + Path.GetFileName(adminfile.FileName)));
                }
                else if (PreviousAdminImage != null && PreviousAdminImage != "")
                {
                    paramadmin.Add("@p_ClinicAdmin_Image", PreviousAdminImage);
                }
                paramadmin.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramadmin.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_ClinicAdmin", paramadmin);
                TempData["Message"] = paramadmin.Get<string>("@p_Message");
                TempData["Icon"] = paramadmin.Get<string>("@p_Icon");

                string query = "SELECT ISNULL(MAX(ClinicAdminFid), 0) FROM ClinicAdmin where Deactivate=0";
                var maxId = BVGWellnessCenterDapparORM.MaxId(query);

                if (TempData["Message"] != null && TempData["Message"].ToString() != "")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProcessType", string.IsNullOrEmpty(clinic.Fid_Encrypted) ? "Save" : "Update");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_Fid", clinic.Fid);
                    param.Add("@p_Fid_Encrypted", clinic.Fid_Encrypted);
                    param.Add("@p_ClinicName", clinic.ClinicName);
                    param.Add("@p_ClinicEmail", clinic.ClinicEmail);
                    param.Add("@p_ClinicContact", clinic.ClinicContact);
                    param.Add("@p_Specialities", clinic.Specialities);
                    param.Add("@p_ClinicAddress", clinic.ClinicAddress);
                    param.Add("@p_Country", clinic.Country);
                    param.Add("@p_CityName", clinic.CityName);
                    param.Add("@p_PostalCode", clinic.postalcode);
                    param.Add("@p_Status", clinic.Status);
                    param.Add("@p_AdminFid", maxId);
                    string FilePath = @"C:\BVGWellnessCenter\ClinicImage\\" + clinic.ClinicContact + "\\";
                    if (File != null && File.ContentLength > 0)
                    {
                        if (!Directory.Exists(FilePath))
                        {
                            Directory.CreateDirectory(FilePath);
                        }
                        File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                        param.Add("@p_ClinicImageFile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                    }
                    else if (PreviousClinicImageFil != null && PreviousClinicImageFil != "")
                    {
                        param.Add("@p_ClinicImageFile", PreviousClinicImageFil);
                    }
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result1 = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_Clinic", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }


                return RedirectToAction("AddClinic", "AddClinic", new { area = "Admin" });

            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #region UpdateClinicStatus
        public ActionResult ChangeClinicStatus(string Fid_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update Mas_Clinic set Status='" + Status + "' where  Deactivate=0 and Fid_Encrypted='" + Fid_Encrypted + "'";
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

                return RedirectToAction("ClinicList", "AddClinic", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region ClinicList
        public ActionResult ClinicList()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //BVGLifeScienceDapparORM.SetConnection();
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Fid_Encrypted", null);
                var data = BVGWellnessCenterDapparORM.DynamicList("sp_List_Mas_Clinic", param);
                ViewBag.ClinicList = data;
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region DeleteClinic
        public ActionResult DeleteClinic(string Fid_Encrypted, string ClinicAdminFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramadmin = new DynamicParameters();
                paramadmin.Add("@p_ProcessType", "Delete");
                paramadmin.Add("@p_ClinicAdminFid_Encrypted", ClinicAdminFid_Encrypted);
                paramadmin.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramadmin.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_ClinicAdmin", paramadmin);
                TempData["Message"] = paramadmin.Get<string>("@p_Message");
                TempData["Icon"] = paramadmin.Get<string>("@p_Icon");

                if (TempData["Message"] != null && TempData["Message"].ToString() != "")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProcessType", "Delete");
                    param.Add("@p_Fid_Encrypted", Fid_Encrypted);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result1 = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_Clinic", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }
                return RedirectToAction("ClinicList", "AddClinic", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}