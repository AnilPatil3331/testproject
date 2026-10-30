using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using System.Net;
using System.Data;
using System.IO;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class AddPatientController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/AddPatient

        #region AddPatient
        public ActionResult AddPatient(string PatientFid_Encrypted,string PatientContact)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                ClsPatient patient = new ClsPatient();
                if(PatientFid_Encrypted!=null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_PatientFid_Encrypted", PatientFid_Encrypted);
                    param1.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));
                    patient = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param1).FirstOrDefault();
                    if(patient!=null)
                    {
                        ViewBag.Update = "Update  patient";
                        string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(patient.PatientProfile);
                        ViewBag.ClinicPatientImageFil = base64Image;
                        ViewBag.PraviousClinicPatientImageFil = patient.PatientProfile;
                        ViewBag.DateOfBirth = patient.DateOfBirth;
                    }                  
                }
                else if(PatientContact!=null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_PatientContact", PatientContact);
                    param1.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));
                    patient = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param1).FirstOrDefault();       
                    if(patient!=null)
                    {
                        if (patient.PatientFid > 0)
                        {
                            ViewBag.Update = "Update  patient";
                            ViewBag.FromPatientContact = true;
                            ViewBag.FromContactChangeButtonName = true;
                        }
                        else
                        {
                            ViewBag.Update = "Add patient";
                        }
                        string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(patient.PatientProfile);
                        ViewBag.ClinicPatientImageFil = base64Image;
                        ViewBag.PraviousClinicPatientImageFil = patient.PatientProfile;
                        ViewBag.DateOfBirth = patient.DateOfBirth;
                    }
                    else
                    {
                        ViewBag.Message = "Patient not find";
                        ViewBag.Icon = "error";
                        ViewBag.PatientNumber = PatientContact;
                        ViewBag.Update = "Add patient";
                    }            
                }
                else
                {
                    ViewBag.Update = "Add patient";
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
                return View(patient);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }           
        }
        #endregion


        #region SavePatient
        [HttpPost]
        public ActionResult SavePatient(ClsPatient patient, HttpPostedFileBase file,string PreviousPatientImage,string FromPatientContact)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

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
                param.Add("@p_PatientType", patient.PatientType);
                param.Add("@p_PatientReference", patient.PatientReference);
                param.Add("@p_Weight", patient.Weight);
                param.Add("@p_Palse", patient.Palse);
                param.Add("@p_BloodPressure", patient.BloodPressure);
                param.Add("@p_SpO2", patient.SpO2);
                param.Add("@p_Origin", "Ayurmitra");

                string FilePath = @"C:\BVGWellnessCenter\PatientImage\\" + patient.ContactNumber + "\\";
                if (File!=null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@P_PatientProfile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                else if (PreviousPatientImage != null && PreviousPatientImage != "")
                {
                    param.Add("@P_PatientProfile", PreviousPatientImage);
                }
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Patient", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var messge = TempData["Message"];
                if (FromPatientContact != null && FromPatientContact== "true")
                {
                    TempData["FromPatinetContact"] = true;
                    return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                }
                return RedirectToAction("PatientList", "AddPatient", new { area = "Patient" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region ImportPatients
        [HttpPost]
        public ActionResult ImportPatients(List<ClsPatient> patients)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                if (patients == null || !patients.Any())
                {
                    return Json(new { success = false, message = "No patient data found in the file." });
                }

                int clinicFid = Convert.ToInt32(Session["ClinicFid"]);
                string loginMachinId = Dns.GetHostName();
                string loginmachinIp = "127.0.0.1";
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(loginMachinId);
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            loginmachinIp = ip.ToString();
                            break;
                        }
                    }
                }
                catch { }

                int importCount = 0;
                int updateCount = 0;
                int errorCount = 0;
                List<string> errors = new List<string>();

                for (int i = 0; i < patients.Count; i++)
                {
                    var p = patients[i];
                    int rowNum = i + 2; // Row 1 is header
                    try
                    {
                        if (string.IsNullOrWhiteSpace(p.FirstName))
                        {
                            errorCount++;
                            errors.Add($"Row {rowNum}: First Name is required.");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(p.ContactNumber))
                        {
                            errorCount++;
                            errors.Add($"Row {rowNum}: Contact Number is required.");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(p.Gender))
                        {
                            p.Gender = "Male";
                        }

                        // Check if patient already exists by contact number in this clinic
                        DynamicParameters existParam = new DynamicParameters();
                        existParam.Add("@p_PatientContact", p.ContactNumber);
                        existParam.Add("@p_ClinicFid", clinicFid);
                        var existingPatientList = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", existParam);

                        ClsPatient existingPatient = null;
                        if (existingPatientList != null && existingPatientList.Any())
                        {
                            // Match by FirstName & LName (case-insensitive)
                            existingPatient = existingPatientList.FirstOrDefault(x =>
                                string.Equals(x.FirstName?.Trim(), p.FirstName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(x.LName?.Trim() ?? "", p.LName?.Trim() ?? "", StringComparison.OrdinalIgnoreCase));

                            // Fallback to match by FirstName only
                            if (existingPatient == null)
                            {
                                existingPatient = existingPatientList.FirstOrDefault(x =>
                                    string.Equals(x.FirstName?.Trim(), p.FirstName?.Trim(), StringComparison.OrdinalIgnoreCase));
                            }
                        }

                        DynamicParameters param = new DynamicParameters();
                        if (existingPatient != null)
                        {
                            param.Add("@p_ProcessType", "Update");
                            param.Add("@p_PatientFid", existingPatient.PatientFid);
                            param.Add("@p_PatientFid_Encrypted", existingPatient.PatientFid_Encrypted);
                            updateCount++;
                        }
                        else
                        {
                            param.Add("@p_ProcessType", "Save");
                            param.Add("@p_PatientFid", 0);
                            param.Add("@p_PatientFid_Encrypted", "");
                            importCount++;
                        }

                        param.Add("@p_MacId", loginMachinId);
                        param.Add("@p_MacIp", loginmachinIp);
                        param.Add("@p_ClinicFid", clinicFid);
                        param.Add("@p_FirstName", p.FirstName);
                        param.Add("@p_LName", p.LName ?? "");
                        param.Add("@p_Email", p.Email ?? "");
                        param.Add("@p_ContactNumber", p.ContactNumber);
                        param.Add("@p_DateOfBirth", p.DateOfBirth);
                        param.Add("@p_BloodGroup", p.BloodGroup ?? "");
                        param.Add("@p_Gender", p.Gender);
                        param.Add("@p_Address", p.Address ?? "");
                        param.Add("@p_Country", p.Country ?? "");
                        param.Add("@p_CityName", p.CityName ?? "");
                        param.Add("@p_PostalCode", p.PostalCode ?? "");
                        param.Add("@p_Status", "Active");
                        param.Add("@p_Age", p.Age ?? "");
                        param.Add("@p_HealthIssue", p.HealthIssue ?? "");
                        param.Add("@p_PatientType", p.PatientType ?? "WalkIn");
                        param.Add("@p_PatientReference", p.PatientReference ?? "");
                        param.Add("@p_Weight", p.Weight);
                        param.Add("@p_Palse", p.Palse ?? "");
                        param.Add("@p_BloodPressure", p.BloodPressure ?? "");
                        param.Add("@p_SpO2", p.SpO2 ?? "");
                        param.Add("@P_PatientProfile", p.PatientProfile ?? "");
                        param.Add("@p_Origin", "Ayurmitra");
                        param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                        BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Patient", param);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Row {rowNum} ({p.FirstName ?? "Unknown"}): {ex.Message}");
                    }
                }

                string summary = $"Import complete: {importCount} added, {updateCount} updated.";
                if (errorCount > 0)
                {
                    summary += $" {errorCount} rows failed.";
                }

                return Json(new { success = true, message = summary, errors = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System Error: " + ex.Message });
            }
        }
        #endregion


        #region UpdatePatientStatus
        public ActionResult ChangePatientStatus(string PatientFid_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update Patient set Status='" + Status + "' where  Deactivate=0 and PatientFid_Encrypted='" + PatientFid_Encrypted + "'";
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

                return RedirectToAction("PatientList", "AddPatient", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region PatientList
        public ActionResult PatientList()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));
                var GetPatientDetail = BVGWellnessCenterDapparORM.DynamicList("sp_List_Patient", param);
                ViewBag.PatientDetails = GetPatientDetail;
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region PatientList
        public ActionResult ListingPage()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));
                var GetPatientDetail = BVGWellnessCenterDapparORM.DynamicList("sp_List_Patient", param);
                ViewBag.PatientDetails = GetPatientDetail;
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region DeletePatient
        public ActionResult DeletePatient(string PatientFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_PatientFid_Encrypted", PatientFid_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Patient", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("PatientList", "AddPatient", new { area = "Patient" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #endregion

        
        public ActionResult PatientDetails(string PatientFid_Encrypted, int? PatientFid, string FromPatientList)
        {

            DynamicParameters param1 = new DynamicParameters();
            param1.Add("@p_PatientFid_Encrypted", PatientFid_Encrypted);
            param1.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));
             var  Result = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param1).FirstOrDefault();
            ViewBag.PatientDetails = Result;

            return Json(Result, JsonRequestBehavior.AllowGet);
        }


        #region Check Existing Contact
        [HttpPost]
        public ActionResult CheckPatientExists(string ContactNumber)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_PatientContact", ContactNumber);
                param.Add("@p_ClinicFid", Convert.ToInt32(Session["ClinicFid"]));

                // Fetch list of patients with this contact number
                var patientList = BVGWellnessCenterDapparORM.ReturnList<ClsPatient>("sp_List_Patient", param);

                // FIX 1: Added parentheses to Count() (or you could use patientList.Any())
                if (patientList != null && patientList.Count() > 0)
                {
                    // Extract names of the existing patients
                    var names = patientList.Select(p => p.FirstName + " " + p.LName).ToList();

                    // FIX 2: Added parentheses to Count() here as well
                    return Json(new { exists = true, count = patientList.Count(), patientNames = names });
                }

                return Json(new { exists = false });
            }
            catch (Exception ex)
            {
                return Json(new { exists = false, error = ex.Message });
            }
        }
        #endregion
      

    }
}