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
    public class AddDoctorController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Admin/AddDoctor
        public ActionResult AddDoctor(string User_Encrypted)
        {
            try
            {
                if(Session["UserID"]==null)
                {
                    return RedirectToAction("Login", "Home",new { area=""});
                }
                UserMaster user = new UserMaster();
                if (User_Encrypted!=null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                   
                    param1.Add("@p_IsDoctor", true);
                    param1.Add("@p_UserId_Encrypted", User_Encrypted);
                    user = BVGWellnessCenterDapparORM.ReturnList<UserMaster>("sp_List_UserList", param1).FirstOrDefault();
                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(user.ProfileImage);
                    ViewBag.ProfileImageFil = base64Image;
                    ViewBag.PraviousProfileImageFil = user.ProfileImage;
                    ViewBag.Status = user.Status;

                    List<DropdownItem> specialization = new List<DropdownItem>
                    {
                        new DropdownItem { Id = 1, Name = "Allergy and Immunology" },
                        new DropdownItem { Id = 2, Name = "Dermatology" },
                        new DropdownItem { Id = 3, Name = "Family Medicine" },
                        new DropdownItem { Id = 4, Name = "Neurology" }
                        
                    };
                    ViewBag.specialization = specialization;
                }
                else
                {
                    ViewBag.ProfileImageFil = null;
                    ViewBag.PraviousProfileImageFil = null;

                    List<DropdownItem> specialization = new List<DropdownItem>
                    {
                        new DropdownItem { Id = 1, Name = "Allergy and Immunology" },
                        new DropdownItem { Id = 2, Name = "Dermatology" },
                        new DropdownItem { Id = 3, Name = "Family Medicine" },
                        new DropdownItem { Id = 4, Name = "Neurology" }

                    };
                    ViewBag.specialization = specialization;
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query","select Fid as Id, ClinicName as Name from Mas_Clinic");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if(GetClinic.Count()==0)
                {
                    ViewBag.ClinicName = "";
                }else
                {
                    ViewBag.ClinicName = GetClinic;
                }


                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Query", "select DepartmentID as Id, DepartmentName as Name from MAS_Department");
                var Department = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param2);
                ViewBag.Department = Department;
                return View(user);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home",new { area=""});
            }

        }

        #region SaveDoctor
        [HttpPost]
        public ActionResult SaveDoctor(UserMaster user,HttpPostedFileBase file, HttpPostedFileBase Signature, string PreviousProfileImage)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                int DegreeYear = user.Degree_Year;
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                //string DefaultLoginPass = "@123#";
                //string DefaultLoginId = user.FirstName;
                // Navin Code (As lihava)
             
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(user.UserId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_UserID", user.UserId);
                param.Add("@p_UserId_Encrypted", user.UserId_Encrypted);
                //param.Add("@p_LoginId", DefaultLoginId);
                //param.Add("@p_LoginPassword", DefaultLoginPass);
                param.Add("@p_LoginId", user.LoginId); // Model madhun direct value ghyaychi
                param.Add("@p_LoginPassword", user.LoginPassword); // Model madhun direct value ghyaychi
                param.Add("@p_FirstName", user.FirstName);
                param.Add("@p_LName", user.LName);
                param.Add("@p_Email", user.Email);
                param.Add("@p_ClinicFid", user.ClinicFid);
                param.Add("@p_ContactNumber", user.ContactNumber);
                param.Add("@p_DateOfBirth", user.DateOfBirth);
                param.Add("@DepartmentID", user.DepartmentID);
                param.Add("@p_SpecializationFid", user.SpecializationFid);
                param.Add("@p_ExperienceInYear", user.ExperienceInYear);
                param.Add("@p_Gender", user.Gender);
                //string FilePath = @"c:\\" + BVGWellnessCenter + "\\" + DoctorImage + "\\" + user.ContactNumber + "";

                // ================= PROFILE IMAGE LOGIC =================
                // ================= PROFILE IMAGE UPDATE LOGIC =================
                string FilePath = @"C:\BVGWellnessCenter\DoctorImage\" + user.ContactNumber + "\\";

                // 'File' ऑब्जेक्ट तुमच्या कोडनुसार (System.Web.HttpContext.Current.Request.Files["file"])
                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    string fullImagePath = FilePath + Path.GetFileName(File.FileName);
                    File.SaveAs(fullImagePath);

                    param.Add("@p_ProfileImage", fullImagePath); // नवीन फाईलचा पाथ
                }
                else if (!string.IsNullOrEmpty(PreviousProfileImage))
                {
                    // जर नवीन फोटो निवडला नाही, तर हिडन फील्ड मधून आलेला जुनाच पाथ डेटाबेसला परत पाठवा
                    param.Add("@p_ProfileImage", PreviousProfileImage.Replace("\\\\", "\\"));
                }
                else
                {
                    param.Add("@p_ProfileImage", null);
                }
                string SignatureFilePath = @"C:\BVGWellnessCenter\DoctorSignature\\" + user.ContactNumber + "\\";
                if (Signature != null && Signature.ContentLength > 0)
                {
                    if (!Directory.Exists(SignatureFilePath))
                    {
                        Directory.CreateDirectory(SignatureFilePath);
                    }
                    Signature.SaveAs(SignatureFilePath + Path.GetFileName((Signature.FileName)));
                    param.Add("@p_DoctorSignature", Convert.ToString(SignatureFilePath + Path.GetFileName(Signature.FileName)));
                }              
                param.Add("@p_Address", user.Address);
                param.Add("@p_Country", user.Country);
                param.Add("@p_CityName", user.CityName);
                param.Add("@p_Postalcode", user.Postalcode);
                param.Add("@p_Degree", user.Degree);
                param.Add("@p_University", user.University);
                param.Add("@p_Degree_Year", DegreeYear);
                param.Add("@p_IsDoctor", true);
                param.Add("@p_IsReceptionist", false);
                param.Add("@p_IsAdmin", false);
                param.Add("@p_Status", user.Status);
                param.Add("@p_DoctorRefrenceNo", user.DoctorRefrenceNo);
                param.Add("@p_IsPaymentMandatory", user.IsPaymentMandatory);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_UserMaster", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                
                return RedirectToAction("AddDoctor", "AddDoctor", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region UpdateClinicStatus
        public ActionResult ChangeDoctorStatus(string UserId_Encrypted, string Status)
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

                return RedirectToAction("DoctorList", "AddDoctor", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
        #region DoctorList
        public ActionResult DoctorList()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home",new { area=""});
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_IsDoctor", true);
                var GetDoctorData = BVGWellnessCenterDapparORM.DynamicList("sp_List_UserList",param);
                ViewBag.DoctorData = GetDoctorData;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteDoctor
        public ActionResult DeleteDoctor(string UserId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home",new { area=""});
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_UserId_Encrypted", UserId_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_UserMaster", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("DoctorList", "AddDoctor", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}