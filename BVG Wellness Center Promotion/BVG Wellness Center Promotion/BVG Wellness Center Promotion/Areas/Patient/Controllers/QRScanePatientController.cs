using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using System.Data;
using System.IO;
using System.Net;
using QRCoder;
using System.Drawing;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class QRScanePatientController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/QRScanePatient
        public ActionResult AddPatientQRScane()
        {
            try
            {
                //BVGWellnessCenterDapparORM.SetConnection();
                //if (PatientContact != null)
                //{
                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@p_PatientContact", PatientContact);
                //    param1.Add("@p_ClinicFid", 1);
                //    var Getpatient = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param1).FirstOrDefault();
                //    if (Getpatient != null)
                //    {
                //        ViewBag.PatientContact = Getpatient.ContactNumber;
                //        ViewBag.Message1 = "Already Registered";
                //        ViewBag.Icon1 = "warning";
                //    }
                //    else
                //    {
                //        ViewBag.PatientNumber = PatientContact;
                //        ViewBag.Message1 = null;
                //        ViewBag.Icon1 = null;
                //    }
                //}
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
                return View();
            }
            catch (Exception)
            {

                throw;
            }

        }

        #region CheckPatientExistOrnot
        public ActionResult PatientExist(string PatientContact)
        {
            try
            {
                //BVGWellnessCenterDapparORM.SetConnection();
                if (PatientContact != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_PatientContact", PatientContact);
                    param1.Add("@p_ClinicFid", 1);
                    var Getpatient = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param1).FirstOrDefault();
                    if (Getpatient != null)
                    {
                        ViewBag.PatientContact = Getpatient.ContactNumber;
                        ViewBag.Message1 = "Already Registered";
                        ViewBag.Icon1 = "warning";                       
                    }                   
                }
                return Json(new { Message = ViewBag.Message1, Icon = ViewBag.Icon1, PatientNumber = PatientContact },JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region SaveAddPatientQRScane
        [HttpPost]
        public ActionResult SaveAddPatientQRScane(ClsPatient patient)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();

                //BVGWellnessCenterDapparORM.SetConnection();
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(patient.PatientFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_PatientFid", patient.PatientFid);
                param.Add("@p_PatientFid_Encrypted", patient.PatientFid_Encrypted);
                param.Add("@p_ClinicFid", patient.ClinicFid);
                param.Add("@p_FirstName", patient.FirstName);
                param.Add("@p_LName", patient.LName);
                param.Add("@p_Email", patient.Email);
                param.Add("@p_ContactNumber", patient.ContactNumber);
                param.Add("@p_DateOfBirth", patient.DateOfBirth);
                param.Add("@p_BloodGroup", patient.BloodGroup);
                param.Add("@p_Gender", patient.Gender);
                param.Add("@p_Address", patient.Address);
                param.Add("@p_Country", patient.Country);
                param.Add("@p_CityName", patient.CityName);
                param.Add("@p_PostalCode", patient.PostalCode);
                param.Add("@p_Status", "Active");
                param.Add("@p_Age", patient.Age);
                param.Add("@p_HealthIssue", patient.HealthIssue);
                param.Add("@p_PatientType", "SelfRegistration");
                param.Add("@p_PatientReference", patient.PatientReference);
                param.Add("@p_Weight", patient.Weight);
                param.Add("@p_Palse", patient.Palse);
                param.Add("@p_BloodPressure", patient.BloodPressure);
                param.Add("@p_SpO2", patient.SpO2);

                string FilePath = @"C:\BVGWellnessCenter\PatientImage\\" + patient.ContactNumber + "\\";
                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@P_PatientProfile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Patient", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");                          

                return RedirectToAction("AddPatientQRScane", "QRScanePatient", new { area = "Patient" });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

     

        #region GetQRCodeImage
        public ActionResult GetQRCodeImage()
        {
            try
            {
                //BVGWellnessCenterDapparORM.SetConnection();
                DynamicParameters paramQR = new DynamicParameters();
                paramQR.Add("@p_Query", "select QRCodeImage from QRCode where Deactivate=0 and ClinicFid=1");
                var GetQRCodeImage = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramQR).FirstOrDefault();
                if (GetQRCodeImage != null)
                {
                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(GetQRCodeImage.QRCodeImage);
                    ViewBag.GetQRCodeImage = base64Image;
                }
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

    }
}