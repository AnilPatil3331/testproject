using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class EncounterController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/Encounter
        public ActionResult EncounterList(int? EncounterFid, string FromPatientList, string fromDoctorAppointmentList)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (fromDoctorAppointmentList != null)
                {
                    ViewBag.fromDoctorAppointmentList = true;
                }
                if (FromPatientList != null)
                {
                    ViewBag.ShowAddPatientForm = true;
                }
                ClsPatientEncounter encounter = new ClsPatientEncounter();
                if (EncounterFid != null)
                {
                    DynamicParameters paramUpdate = new DynamicParameters();
                    paramUpdate.Add("@p_EncountrFid", EncounterFid);
                    encounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_GetPatientEncounterDetails", paramUpdate).FirstOrDefault();
                    ViewBag.UpdateEncounter = true;

                    DynamicParameters paramDoctor = new DynamicParameters();
                    paramDoctor.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + encounter.ClinicFid + "' and IsDoctor=1 and Deactivate=0 ");
                    var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramDoctor);
                    ViewBag.GetDoctor = GetDoctor;

                    DynamicParameters ParamPatient = new DynamicParameters();
                    ParamPatient.Add("@p_Query", "select PatientFid as Id, FirstName + ' ' + LName as Name from Patient where Deactivate=0 and ClinicFid='" + encounter.ClinicFid + "'");
                    var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", ParamPatient);
                    ViewBag.GetPatient = GetPatient;

                    ViewBag.GetFileName = Path.GetFileName(encounter.AttachmentFile);
                    ViewBag.PreviousReportFile = encounter.AttachmentFile;
                }
                else
                {
                    ViewBag.UpdateEncounter = null;
                    ViewBag.GetPatient = "";
                    ViewBag.GetDoctor = "";
                }
                DynamicParameters paramList = new DynamicParameters();

                if (Session["UserRole"] != null && Session["UserRole"].ToString() == "IsAdmin")
                {
                    paramList.Add("@p_EncountrFid", null);
                    var result = BVGWellnessCenterDapparORM.DynamicList("sp_GetPatientEncounterDetails", paramList);
                    ViewBag.GetPatientEncounterDetails = result;
                }
                else if (Session["UserRole"] != null && Session["UserRole"].ToString() == "IsDoctor")
                {
                    paramList.Add("@p_EncountrFid", null);
                    paramList.Add("@p_DoctorFid", Session["UserID"]);
                    var result = BVGWellnessCenterDapparORM.DynamicList("sp_GetPatientEncounterDetails", paramList);
                    ViewBag.GetPatientEncounterDetails = result;
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic where Fid='" + Session["ClinicFid"] + "'");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if (GetClinic.Count() == 0)
                {
                    ViewBag.ClinicName = "";
                }
                else
                {
                    ViewBag.ClinicName = GetClinic;
                }
                return View(encounter);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        [HttpPost]
        public ActionResult SaveAddEncounter(ClsPatientEncounter encounter, HttpPostedFileBase file, string PreviousFileName)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                param.Add("@p_ProcessType", string.IsNullOrEmpty(encounter.EncountrFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_EncounterFid", encounter.EncounterFid);
                param.Add("@p_EncountrFid_Encrypted", encounter.EncountrFid_Encrypted);
                param.Add("@p_EncounterDate", encounter.EncounterDate);
                param.Add("@p_ClinicFid", encounter.ClinicFid);
                param.Add("@p_DoctorFid", encounter.DoctorFid);
                param.Add("@p_PatientFid", encounter.PatientFid);
                param.Add("@p_Description", encounter.Description);
                param.Add("@p_Status", "Active");
                string FilePath = @"C:\BVGWellnessCenter\PatientEncounterFile\\" + encounter.EncounterFid + "\\";
                if (File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@p_AttachmentFile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                else if (PreviousFileName != null && PreviousFileName != "")
                {
                    param.Add("@p_AttachmentFile", PreviousFileName);
                }
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientEncounter", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                string query = "SELECT ISNULL(MAX(EncounterFid), 0) FROM PatientEncounter where Deactivate=0";
                var maxEncounterFid = BVGWellnessCenterDapparORM.MaxId(query);
                return RedirectToAction("PatientEncounter", "Encounter", new { EncountrFid = maxEncounterFid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #region PatientEncounter
        public ActionResult PatientEncounter(int? EncountrFid, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                //string paymentStatus = "";
                DynamicParameters paramProp = new DynamicParameters();
                paramProp.Add("@p_Query", "select Fid as Id, ProblemName as Name from Problem where Deactivate=0");
                var GetProblem = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramProp);
                ViewBag.Problem = GetProblem;
                DynamicParameters paramOvser = new DynamicParameters();
                paramOvser.Add("@p_Query", "select Fid as Id, Ovservation as Name from Ovservation where Deactivate=0");
                var GetOvservation = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramOvser);
                ViewBag.Ovservation = GetOvservation;

                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Deactivate=0 and Servicecategory='Medicine'");
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;
                if (EncountrFid != null)
                {
                    DynamicParameters paramFile = new DynamicParameters();
                    paramFile.Add("@p_Query", "select AttachFile,FileType from PatientEncounterFile where Deactivate=0 and EncounterFid='"+ EncountrFid + "'");
                    var GetFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFile);
                    ViewBag.Files = GetFile;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_EncountrFid", EncountrFid);
                    var result = BVGWellnessCenterDapparORM.DynamicList("sp_GetPatientEncounterDetails", param);
                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramDetails = new DynamicParameters();
                    paramDetails.Add("@p_EncountrFid", EncountrFid);
                    var PatientDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", paramDetails).FirstOrDefault();
                    if (PatientDetails != null)
                    {
                        ViewBag.PatientDetails = PatientDetails;
                        ViewBag.SpecialNote = PatientDetails.SpecialNote;
                        ViewBag.TherapySuggested = PatientDetails.TherapySuggested;
                        ViewBag.TestSuggested = PatientDetails.TestSuggested;
                        var DoctorSignature = PatientDetails.DoctorSignature;
                        string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(DoctorSignature);
                        ViewBag.DoctorSignature = base64Image;

                        if (PatientDetails.Advise != null && PatientDetails.Advise != "")
                        {
                            ViewBag.Advise = PatientDetails.Advise;
                        }
                        else
                        {
                            ViewBag.Advise = "All medicine should be taken 30 mins after food with warm water.";
                        }
                    }
                    else
                    {
                        ViewBag.PatientDetails = "";
                        ViewBag.Advise = "All medicine should be taken 30 mins after food with warm water.";
                    }


                    DynamicParameters paramlist = new DynamicParameters();
                    paramlist.Add("@p_Query", "select Fid as Id, MedicineName as Name from Mas_Medicine where Deactivate=0");
                    var GetMedicine = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramlist);
                    ViewBag.Medicine = GetMedicine;

                    DynamicParameters paramProblem = new DynamicParameters();
                    paramProblem.Add("@p_Query", "select Fid as Id, Problem as Name from patientProblem where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var PatientProblem = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramProblem);
                    ViewBag.PatientProblem = PatientProblem;

                    DynamicParameters paramovser = new DynamicParameters();
                    paramovser.Add("@p_Query", "select Fid as Id, Ovservation as Name from PatientOvservation where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var PatientOvser = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramovser);
                    ViewBag.PatientOvservation = PatientOvser;

                    DynamicParameters paramNotes = new DynamicParameters();
                    paramNotes.Add("@p_Query", "select Fid as Id, DoctorNotes as Name from PatientNotes where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var PatientNotes = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramNotes);
                    ViewBag.PatientNotes = PatientNotes;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Fid_Encrypted", null);
                    paramList.Add("@p_EncounterFid", EncountrFid);
                    var GetResult = BVGWellnessCenterDapparORM.DynamicList("sp_List_Prescription", paramList);
                    ViewBag.GetPrescriptionList = GetResult;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", EncountrFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;


                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status,DiscountAmt from finalbill where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.paymentStatus = FinalBill?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill?.DiscountAmt;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from finalbill where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;

                    DynamicParameters paramFinalBill = new DynamicParameters();
                    paramFinalBill.Add("@p_Query", "select TotalAmount,DiscountAmt,PayableAmount, Status from finalbill where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    var GetFinalBillDeatils = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalBill).FirstOrDefault();
                    if (GetFinalBillDeatils != null)
                    {
                        ViewBag.GetFinalBillDeatils = GetFinalBillDeatils;
                    }


                }
                else
                {

                    DynamicParameters paramProblem = new DynamicParameters();
                    paramProblem.Add("@p_Query", "select Fid as Id, Problem as Name from patientProblem where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var PatientProblem = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramProblem);
                    ViewBag.PatientProblem = PatientProblem;

                    DynamicParameters paramovser = new DynamicParameters();
                    paramovser.Add("@p_Query", "select Fid as Id, Ovservation as Name from PatientOvservation where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var PatientOvser = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramovser);
                    ViewBag.PatientOvservation = PatientOvser;

                    DynamicParameters paramNotes = new DynamicParameters();
                    paramNotes.Add("@p_Query", "select Fid as Id, DoctorNotes as Name from PatientNotes where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var PatientNotes = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramNotes);
                    ViewBag.PatientNotes = PatientNotes;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    var result = BVGWellnessCenterDapparORM.DynamicList("sp_List_patientappoinment", param);
                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters Query = new DynamicParameters();
                    Query.Add("@p_Query", "select EncounterStatus from PatientAppoinment where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var getData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Query).FirstOrDefault();
                    if (getData.EncounterStatus == null)
                    {
                        DynamicParameters paramapp = new DynamicParameters();
                        paramapp.Add("@p_Query", "update PatientAppoinment set EncounterStatus='Open' where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                        var updateResult = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramapp);
                    }

                    DynamicParameters paramlist = new DynamicParameters();
                    paramlist.Add("@p_Query", "select Fid as Id, MedicineName as Name from Mas_Medicine where Deactivate=0");
                    var GetMedicine = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramlist);
                    ViewBag.Medicine = GetMedicine;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid); ;
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    //DynamicParameters paramFinal = new DynamicParameters();
                    //paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and EncounterFid='" + EncountrFid + "'");
                    //var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    //ViewBag.paymentStatus = FinalBill?.Status ?? "Unknown";
                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status,DiscountAmt from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.paymentStatus = FinalBill?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill?.DiscountAmt ?? "Unknown";

                    //DynamicParameters paramFinal = new DynamicParameters();
                    //paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    //var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal);

                }

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveEncounterReport
        [HttpPost]
        public ActionResult SaveEncounterReport(HttpPostedFileBase[] casePaper, HttpPostedFileBase[] consentForm, HttpPostedFileBase[] patientReport1, int PatientFid, int EncounterFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                
                string FilePath = @"C:\BVGWellnessCenter\PatientEncounterFile\" + EncounterFid + @"\";
                int Save = 0;
                // Ensure Directory Exists (Only Once)
                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }

                DynamicParameters paramFinal = new DynamicParameters();
                paramFinal.Add("@p_Query", "select AttachFile from patientencounterfile where Deactivate=0 and EncounterFid='" + EncounterFid + "' and PatientFid='" + PatientFid + "'");
                var GetEncounterFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                
                string processType = "Save";
                if (GetEncounterFile != null)
                {
                    if (casePaper != null)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Query", "Update patientencounterfile set Deactivate=1 where Deactivate=0 and EncounterFid='" + EncounterFid + "' and PatientFid='" + PatientFid + "' and FileType='CasePaper' ");
                        var DeactivatePreviousFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param1);
                    }
                    if (consentForm != null)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Query", "Update patientencounterfile set Deactivate=1  where Deactivate=0 and EncounterFid='" + EncounterFid + "' and PatientFid='" + PatientFid + "' and FileType='ConsentForm' ");
                        var DeactivatePreviousFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param1);
                    }
                    if (patientReport1 != null)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Query", "Update patientencounterfile set Deactivate=1 where Deactivate=0 and EncounterFid='" + EncounterFid + "' and PatientFid='" + PatientFid + "' and FileType='PatientReport' ");
                        var DeactivatePreviousFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param1);
                    }
                }

                if (casePaper != null)
                {
                    foreach (var item in casePaper)
                    {
                        if (item != null && item.ContentLength > 0) // Ensure the file is valid
                        {
                            string casePaperPath = Path.Combine(FilePath, Path.GetFileName(item.FileName));
                            item.SaveAs(casePaperPath); 
                            
                            // Initialize fresh parameters for each file
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_ProcessType", processType);
                            param.Add("@p_AttachFile", casePaperPath);
                            param.Add("@p_FileType", "CasePaper");
                            param.Add("@p_EncounterFid", EncounterFid);
                            param.Add("@p_PatientFid", PatientFid);
                            param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            
                            var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientEncounterFile", param);
                            Save = 1;
                        }
                    }
                }

                if (consentForm != null)
                {
                    foreach (var item in consentForm)
                    {
                        if (item != null && item.ContentLength > 0) // Ensure the file is valid
                        {
                            string consentFormPath = Path.Combine(FilePath, Path.GetFileName(item.FileName));
                            item.SaveAs(consentFormPath); 
                            
                            // Initialize fresh parameters for each file
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_ProcessType", processType);
                            param.Add("@p_AttachFile", consentFormPath);
                            param.Add("@p_FileType", "ConsentForm");
                            param.Add("@p_EncounterFid", EncounterFid);
                            param.Add("@p_PatientFid", PatientFid);
                            param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                            var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientEncounterFile", param);
                            Save = 1;
                        }
                    }
                }

                if (patientReport1 != null)
                {
                    foreach (var item in patientReport1)
                    {
                        if (item != null && item.ContentLength > 0) // Ensure the file is valid
                        {
                            string patientReportPath = Path.Combine(FilePath, Path.GetFileName(item.FileName));
                            item.SaveAs(patientReportPath); // Save file to server

                            // Initialize fresh parameters for each file                           
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_ProcessType", processType);
                            param.Add("@p_AttachFile", patientReportPath);
                            param.Add("@p_FileType", "PatientReport");
                            param.Add("@p_EncounterFid", EncounterFid);
                            param.Add("@p_PatientFid", PatientFid);
                            param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            
                            // Execute stored procedure for each file
                            var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientEncounterFile", param);
                            Save = 1;
                        }
                    }
                }

                if (Save != 0)
                {
                    TempData["Message"] = "File Save Successfuly";
                    TempData["Icon"] = "success";
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] });

            }
            catch (Exception ex)
            {

                return Json(new { Message = "Error: " + ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : ""), Icon = "error" });
            }
        }
        #endregion

        #region SaveProbleOvservationDesacription
        public ActionResult SaveProbleOvservationDesacription(string Value, string type)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Save");
                param.Add("@p_Tppe", type);
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                if (type == "Problem")
                {
                    param.Add("@p_ProblemName", Value);
                }
                else if (type == "Observations")
                {
                    param.Add("@p_Ovservation", Value);
                }
                else
                {
                    param.Add("@p_Description", Value);
                }

                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Problem_Ovservation_Notes", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                DynamicParameters paramProp = new DynamicParameters();
                paramProp.Add("@p_Query", "select Fid as Id, ProblemName as Name from Problem where Deactivate=0");
                var GetProblem = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramProp);
                //TempData["Problems"] = GetProblem;
                DynamicParameters paramOvser = new DynamicParameters();
                paramOvser.Add("@p_Query", "select Fid as Id, Ovservation as Name from Ovservation where Deactivate=0");
                var GetOvservation = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramOvser);
                //TempData["Ovservation"] = GetOvservation;

                var data = TempData["Message"];
                return Json(new { GetProblem = GetProblem, GetOvservation = GetOvservation }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveMedicine
        public ActionResult SaveMedicine(string MedicineName)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Save");
                param.Add("@p_MedicineName", MedicineName);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_Medicine", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@p_Query", "select Fid as Id, MedicineName as Name from Mas_Medicine where Deactivate=0");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramlist);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SavePrescription
        [HttpPost]
        public ActionResult SavePrescription(ClsPrescription prescrip, int? PatientFid)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                string query = "SELECT ISNULL(MAX(EncounterFid), 0) FROM PatientEncounter where Deactivate=0";
                var maxEncounterFid = BVGWellnessCenterDapparORM.MaxId(query);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(prescrip.Fid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_Fid", prescrip.Fid);
                param.Add("@p_Fid_Encrypted", prescrip.Fid_Encrypted);
                param.Add("@p_MedicineName", prescrip.MedicineName);
                param.Add("@p_Frequency", prescrip.Frequency);
                param.Add("@p_Duration", prescrip.Duration);
                param.Add("@p_Instruction", prescrip.Instruction);
                param.Add("@p_PatientFid", PatientFid);
                param.Add("@p_EncounterStatus", "Active");
                param.Add("@p_EncounterFid", prescrip.EncounterFid);
                //param.Add("@p_AppointmentFid", prescrip.AppointmentFid);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Prescription", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var Data = TempData["Message"];
                var SuccesMsg = TempData["Message"];
                var Icon = TempData["Icon"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Fid_Encrypted", null);
                paramList.Add("@p_EncounterFid", prescrip.EncounterFid);
                var GetResult = BVGWellnessCenterDapparORM.DynamicList("sp_List_Prescription", paramList);
                var GetPrescriptionList = GetResult;


                return Json(new { GetPrescriptionList = GetPrescriptionList, SuccesMsg = SuccesMsg, Icon = Icon, EncounterFid = prescrip.EncounterFid }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #endregion

        #region SaveEncounterProbOvserDescrip
        public ActionResult SaveEncounterProbOvserDescrip(string Value, string type, int? EncounterFid, int? PatientFid, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                DynamicParameters param = new DynamicParameters();

                var data = "";
                if (type == "Problem")
                {
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_Problem", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_patientProblem", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();

                }
                else if (type == "Observations")
                {
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_Ovservation", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientOvservation", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();
                }
                else
                {
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_DoctorNotes", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientNotes", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region UpdateEncounterProbOvserDescrip
        public ActionResult UpdateEncounterProbOvserDescrip(string Value, string type, int? EncounterFid, int? PatientFid, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                DynamicParameters param = new DynamicParameters();

                var data = "";
                if (type == "Problem")
                {
                    param.Add("@p_ProcessType", "Delete");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_Problem", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_patientProblem", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();
                }
                else if (type == "Observations")
                {
                    param.Add("@p_ProcessType", "Delete");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_Ovservation", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientOvservation", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();
                }
                else
                {
                    param.Add("@p_ProcessType", "Delete");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    param.Add("@p_DoctorNotes", Value);
                    param.Add("@p_PatientFid", PatientFid);
                    param.Add("@p_EncounterFid", EncounterFid);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientNotes", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    data = TempData["Message"].ToString();
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region UpdatePrescription
        public ActionResult UpdatePrescription(string Fid_Encrypted)
        {
            try
            {
                ClsPrescription prescription = new ClsPrescription();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramPrescrip = new DynamicParameters();
                if (Fid_Encrypted != null)
                {
                    paramPrescrip.Add("@p_Fid_Encrypted", Fid_Encrypted);
                    prescription = BVGWellnessCenterDapparORM.ReturnList<ClsPrescription>("sp_List_Prescription", paramPrescrip).FirstOrDefault();
                    ViewBag.prescriptionUpdate = true;

                }
                var MedicineName = prescription.MedicineName;
                var Frequency = prescription.Frequency;
                var Duration = prescription.Duration;
                var Instruction = prescription.Instruction;
                var FidEncrypted = prescription.Fid_Encrypted;
                return Json(new { MedicineName = MedicineName, Frequency = Frequency, Duration = Duration, Instruction = Instruction, FidEncrypted = FidEncrypted }, JsonRequestBehavior.AllowGet);

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
                if (Session["UserRole"] != null && Session["UserRole"].ToString() == "IsAdmin")
                {
                    int UserId = Convert.ToInt32(Session["UserID"]);
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + ClinicFid + "' and IsDoctor=1 and Deactivate=0");
                    var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                    ViewBag.GetDoctor = GetDoctor;
                    DynamicParameters ParamPatient = new DynamicParameters();
                    ParamPatient.Add("@p_ClinicFid", ClinicFid);
                    ParamPatient.Add("@p_DoctorFid", null);
                    var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_CheckInGetPatient", ParamPatient);
                    ViewBag.GetPatient = GetPatient;
                }
                else if (Session["UserRole"] != null && Session["UserRole"].ToString() == "IsDoctor")
                {
                    int UserId = Convert.ToInt32(Session["UserID"]);
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + ClinicFid + "' and IsDoctor=1 and Deactivate=0 and UserId='" + UserId + "'");
                    var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                    ViewBag.GetDoctor = GetDoctor;
                    DynamicParameters ParamPatient = new DynamicParameters();
                    ParamPatient.Add("@p_ClinicFid", ClinicFid);
                    ParamPatient.Add("@p_DoctorFid", UserId);
                    var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_CheckInGetPatient", ParamPatient);
                    ViewBag.GetPatient = GetPatient;
                }

                //var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", ParamPatient);

                return Json(new { GetDoctor = ViewBag.GetDoctor, GetPatient = ViewBag.GetPatient }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeletePrescription
        public ActionResult DeletePrescription(string Fid_Encrypted, int EncounterFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DynamicParameters paramDelete = new DynamicParameters();
                paramDelete.Add("@p_ProcessType", "Delete");
                paramDelete.Add("@p_Fid_Encrypted", Fid_Encrypted);
                paramDelete.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramDelete.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Prescription", paramDelete);
                TempData["Message"] = paramDelete.Get<string>("@p_Message");
                TempData["Icon"] = paramDelete.Get<string>("@p_Icon");
                var SuccesMsg = TempData["Message"];
                var Icon = TempData["Icon"];

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Fid_Encrypted", null);
                paramList.Add("@p_EncounterFid", EncounterFid);
                var GetResult = BVGWellnessCenterDapparORM.DynamicList("sp_List_Prescription", paramList);
                var GetPrescriptionList = GetResult;

                return Json(new { GetPrescriptionList = GetPrescriptionList, SuccesMsg = SuccesMsg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteEncounter
        public ActionResult DeleteEncounter(string EncountrFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramDelete = new DynamicParameters();
                paramDelete.Add("@p_ProcessType", "Delete");
                paramDelete.Add("@p_EncountrFid_Encrypted", EncountrFid_Encrypted);
                paramDelete.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramDelete.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientEncounter", paramDelete);
                TempData["Message"] = paramDelete.Get<string>("@p_Message");
                TempData["Icon"] = paramDelete.Get<string>("@p_Icon");
                var SuccesMsg = TempData["Message"];
                var Icon = TempData["Icon"];
                return RedirectToAction("EncounterList", "Encounter", new { area = "Patient" });
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetServicePrice
        public ActionResult GetServicePrice(int GetServiceId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramprice = new DynamicParameters();
                paramprice.Add("@p_Query", "select Charges  from MAS_Form where FormID = '" + GetServiceId + "'");
                var GetServicePrice = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramprice).FirstOrDefault();

                return Json(GetServicePrice.Charges, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetServicePrice
        public ActionResult GetApppoServicePrice(int GetServiceId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramprice = new DynamicParameters();
                paramprice.Add("@p_Query", "select Charges  from Mas_Service where ServiceFid  = '" + GetServiceId + "'");
                var GetServicePrice = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramprice).FirstOrDefault();

                return Json(GetServicePrice.Charges, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveGenerateBill
        [HttpPost]
        public ActionResult SaveGenerateBill(ClsPrescription Bill, int AppointmentFid)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //var GetBillList="";
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(Bill.Bill_Fid_Encrapted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_Bill_Fid", Bill.Bill_Fid);
                param.Add("@p_Bill_Fid_Encrapted", Bill.Bill_Fid_Encrapted);
                if (Bill.ServiceFid != 0 && Bill.ServiceFid != null)
                {
                    param.Add("@p_ServiceFid", Bill.ServiceFid);
                }
                else if (Bill.Service_Fid != 0 && Bill.Service_Fid != null)
                {
                    param.Add("@p_ServiceFid", Bill.Service_Fid);
                }
                param.Add("@p_Price", Bill.Price);
                param.Add("@p_Quantity", Bill.Quantity);
                param.Add("@p_Total", Bill.Total);
                param.Add("@p_EncounterFid", Bill.EncounterFid);
                param.Add("@p_AppointmentFid", Bill.AppointmentFid);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBill", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.AppointmentFid != 0 && Bill.AppointmentFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", Bill.AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                return Json(new { GetBillList = ViewBag.GenerateBill }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveFinalAmountAndCloseEncounter
        //[HttpPost]
        //public ActionResult SaveAndCloseEncounter(ClsPrescription finalbill, string PaymentStatus)
        //{
        //    try
        //    {
        //        ClsPatientEncounter PEncounter = new ClsPatientEncounter();
        //        string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }
        //        int EncounterFid = 0;
        //        int AppointmentFid = 0;
        //        var FlageId = "1" + finalbill.EncounterFid;
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_ProcessType", "Save");
        //        param.Add("@p_MacId", LoginMachinId);
        //        param.Add("@p_MacIp", LoginmachinIp);
        //        if (finalbill.EncounterFid != 0 && finalbill.EncounterFid != 0!=null)
        //        {
        //            param.Add("@p_EncounterFid", finalbill.EncounterFid);
        //            DynamicParameters paramFinal = new DynamicParameters();
        //            paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and EncounterFid='" + finalbill.EncounterFid + "'");
        //            var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
        //            DynamicParameters paramEncounter = new DynamicParameters();
        //            paramEncounter.Add("@p_Query", "select PatientFid,DoctorFid,EncounterDate from PatientEncounter where Deactivate=0 and EncounterFid='" + finalbill.EncounterFid + "'");
        //            PEncounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_ExecuteQuery", paramEncounter).FirstOrDefault();
        //            if (FinalBill != null)
        //            {
        //                if (FinalBill?.Status == "Unpaid" || FinalBill?.Status == "Paid")
        //                {
        //                    param.Add("@p_AlreadyExist", "Yesh");
        //                    param.Add("@p_PatientFid", PEncounter.PatientFid);
        //                    param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
        //                    param.Add("@p_DoctorFid", PEncounter.DoctorFid);
        //                }                      
        //            }
        //        }
        //        else
        //        {
        //            param.Add("@p_EncounterFid", null);
        //        }
        //        if (finalbill.AppointmentFid != 0 && finalbill.AppointmentFid!=null)
        //        {
        //            param.Add("@p_AppointmentFid", finalbill.AppointmentFid);
        //            DynamicParameters paramFinal = new DynamicParameters();
        //            paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and AppointmentFid='" + finalbill.AppointmentFid + "'");
        //            var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
        //            if (FinalBill != null)
        //            {
        //                if (FinalBill?.Status == "Unpaid" || FinalBill?.Status == "Paid")
        //                {
        //                    param.Add("@p_AlreadyExist", "Yesh");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            param.Add("@p_AppointmentFid", null);
        //        }


        //        param.Add("@p_TotalAmount", finalbill.finalAmount);
        //        param.Add("@p_DiscountAmt", finalbill.Discount);
        //        param.Add("@p_PayableAmount", finalbill.FinalPaybalAmount);
        //        param.Add("@p_Status", PaymentStatus);
        //        param.Add("@p_FlageID", FlageId);
        //        param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBill", param);
        //        TempData["Message"] = param.Get<string>("@p_Message");
        //        if (finalbill.EncounterFid != 0 && finalbill.EncounterFid != null)
        //        {
        //            EncounterFid = Convert.ToInt32(finalbill.EncounterFid);
        //            return Json(new { EncounterFid = EncounterFid }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (finalbill.AppointmentFid != 0 && finalbill.AppointmentFid != null)
        //        {
        //            AppointmentFid = Convert.ToInt32(finalbill.AppointmentFid);
        //            return Json(new { AppointmentFid = AppointmentFid }, JsonRequestBehavior.AllowGet);
        //        }
        //        var data = TempData["Message"];
        //        return Json(data, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        return RedirectToAction("ErrorPage", "Home", new { area = "" });
        //    }
        //}

        [HttpPost]
        public ActionResult SaveAndCloseEncounter(List<ClsPrescription> paymentList)
        {
            try
            {
                ClsPatientEncounter PEncounter = new ClsPatientEncounter();
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                int EncounterFid = 0;
                int? AppointmentFid = paymentList.FirstOrDefault()?.AppointmentFid;

                for (int i = 0; i < paymentList.Count(); i++)
                {
                    EncounterFid = Convert.ToInt32(paymentList[i].EncounterFid);
                    var FlageId = $"{i + 1}{paymentList[i].EncounterFid}";

                    //DynamicParameters paramEncounter = new DynamicParameters();
                    //paramEncounter.Add("@p_Query", "select PatientFid,DoctorFid,EncounterDate from PatientEncounter where Deactivate=0 and EncounterFid='" + paymentList[i].EncounterFid + "'");
                    //PEncounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_ExecuteQuery", paramEncounter).FirstOrDefault();

                    //DynamicParameters paramAppointmentFid = new DynamicParameters();
                    //paramAppointmentFid.Add("@p_Query", "select AppointmentFid from PatientAppoinment where Deactivate=0 and PatientFid='" + PEncounter.PatientFid + "' and AppoinmentDate='" + PEncounter.EncounterDate + "' and DoctorFid='" + PEncounter.DoctorFid + "'");
                    //var GetAppointFid = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppointmentFid).FirstOrDefault();


                    DynamicParameters paramAppointmentFid = new DynamicParameters();
                    paramAppointmentFid.Add("@p_Query", $"select AppointmentFid from PatientAppoinment where Deactivate=0 and AppointmentFid = {AppointmentFid}");
                    var GetAppointFid = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppointmentFid).FirstOrDefault();

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_MacId", LoginMachinId);
                    param.Add("@p_MacIp", LoginmachinIp);
                    if (paymentList[i].EncounterFid != 0 && paymentList[i].EncounterFid != null)
                    {
                        param.Add("@p_EncounterFid", paymentList[i].EncounterFid);
                        DynamicParameters paramFinal = new DynamicParameters();
                        paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and EncounterFid='" + paymentList[i].EncounterFid + "'");
                        var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();

                        DynamicParameters paramFlageId = new DynamicParameters();
                        paramFlageId.Add("@p_Query", "select FlageID from finalbill where Deactivate=0 and EncounterFid='" + paymentList[i].EncounterFid + "' and FlageID='" + FlageId + "'");
                        var GetFlageId = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFlageId).FirstOrDefault();
                        if (GetFlageId != null)
                        {
                            param.Add("@p_FlageID", GetFlageId.FlageID);
                            if (FinalBill != null)
                            {
                                if (FinalBill?.Status == "Unpaid" || FinalBill?.Status == "Paid")
                                {
                                    param.Add("@p_AlreadyExist", "Yesh");
                                    param.Add("@p_PatientFid", PEncounter.PatientFid);
                                    param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
                                    param.Add("@p_DoctorFid", PEncounter.DoctorFid);
                                }
                                else
                                {
                                    param.Add("@p_PatientFid", PEncounter.PatientFid);
                                    param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
                                    param.Add("@p_DoctorFid", PEncounter.DoctorFid);
                                }
                            }
                        }
                        else
                        {
                            param.Add("@p_FlageID", FlageId);
                            param.Add("@p_AlreadyExist", null);
                            param.Add("@p_PatientFid", PEncounter.PatientFid);
                            param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
                            param.Add("@p_DoctorFid", PEncounter.DoctorFid);
                        }

                    }
                    else
                    {
                      param.Add("@p_EncounterFid", null);
                    }
                    param.Add("@p_TotalAmount", paymentList[i].finalAmount);
                    param.Add("@p_DiscountAmt", paymentList[i].Discount);
                    param.Add("@p_PayableAmount", paymentList[i].FinalPaybalAmount);
                    //param.Add("@p_Status", PaymentStatus);
                    param.Add("@p_Status", paymentList[i].PaymentStatus);
                    param.Add("@p_PaymentType", paymentList[i].PaymentType);
                    param.Add("@p_ReferenceNumber", paymentList[i].ReferenceNumber);
                    if (GetAppointFid != null)
                    {
                        param.Add("@p_AppointmentFid", GetAppointFid.AppointmentFid);
                    }
                    else
                    {
                        param.Add("@p_AppointmentFid", null);
                    }

                    param.Add("@p_Amount", paymentList[i].Amount);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBill", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                }
                if (EncounterFid != 0 && EncounterFid != null)
                {
                    return Json(new { EncounterFid = EncounterFid }, JsonRequestBehavior.AllowGet);
                }

                var data = TempData["Message"];
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region UpdateBill
        public ActionResult UpdateBill(string Bill_Fid_Encrapted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                ClsPrescription GetBillList = new ClsPrescription();
                DynamicParameters paramPrescrip = new DynamicParameters();
                if (Bill_Fid_Encrapted != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_Bill_Fid_Encrapted", Bill_Fid_Encrapted);
                    GetBillList = BVGWellnessCenterDapparORM.ReturnList<ClsPrescription>("sp_List_GenerateBill", paramBill).FirstOrDefault();
                    ViewBag.GenerateBillUpdate = true;

                }
                var MedicineName = "";
                if (GetBillList.ServiceName != null && GetBillList.ServiceName != "")
                {
                    MedicineName = GetBillList.ServiceName;
                }
                else if (GetBillList.MedicineName != null && GetBillList.MedicineName != "")
                {
                    MedicineName = GetBillList.MedicineName;
                }

                var Price = GetBillList.Price;
                var Quantity = GetBillList.Quantity;
                var Total = GetBillList.Total;
                var BillFid_Encrapted = GetBillList.Bill_Fid_Encrapted;
                var ServiceFid = GetBillList.ServiceFid;
                return Json(new { MedicineName = MedicineName, Price = Price, Quantity = Quantity, Total = Total, BillFid_Encrapted = BillFid_Encrapted, ServiceFid = ServiceFid }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteBill
        public ActionResult DeleteBill(string Bill_Fid_Encrapted, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DynamicParameters paramDelete = new DynamicParameters();
                paramDelete.Add("@p_ProcessType", "Delete");
                paramDelete.Add("@p_Bill_Fid_Encrapted", Bill_Fid_Encrapted);
                paramDelete.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramDelete.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBill", paramDelete);

                var SuccesMsg = paramDelete.Get<string>("@p_Message");
                var Icon = paramDelete.Get<string>("@p_Icon");

                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", AppointmentFid);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                var GenerateBill = GetBillList;

                return Json(new { GenerateBill = GenerateBill, SuccesMsg = SuccesMsg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        public ActionResult GetServices(string check, string FromPrescriptioncheck, int? EncounterFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (check == "Checked")
                {
                    DynamicParameters paramService = new DynamicParameters();
                    paramService.Add("@p_Query", "select ServiceFid as id, concat(Servicecategory,' ',ServiceName) as Name  from mas_service where ServiceName='Health Check'");
                    var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                    ViewBag.Getdata = data;
                }
                else if (FromPrescriptioncheck == "Checked")
                {
                    DynamicParameters paramFromPres = new DynamicParameters();
                    paramFromPres.Add("@p_EncounterFid", EncounterFid);
                    var Result = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_List_GetMedicineFromPrescription", paramFromPres);
                    ViewBag.Getdata = Result;
                }
                else
                {
                    DynamicParameters paramService = new DynamicParameters();
                    paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Servicecategory='Medicine'");
                    var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                    ViewBag.Getdata = data;
                }

                return Json(new { GetData = ViewBag.Getdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #region GetMedicineDetails
        public ActionResult GetMedicineDetails(string MedicineName)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MedicineName", MedicineName);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetMedicineDetails", param).FirstOrDefault();
                if (result != null)
                {
                    ViewBag.Doses = result.Doses;
                    ViewBag.Duration = result.Duration;
                    ViewBag.Instructions = result.Instructions;
                }
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Deactivate=0 and Servicecategory='Medicine'");
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;
                return Json(new { Doses = ViewBag.Doses, Duration = ViewBag.Duration, Instructions = ViewBag.Instructions, service = ViewBag.Service }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        [HttpPost]
        public ActionResult SendBillEmail(HttpPostedFileBase pdfFile, string EncounterFid)
        {
            try
            {
                int Encounterfid = Convert.ToInt32(EncounterFid);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EncounterFid", Encounterfid);
                var GetPatientEmail = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEmailDeatils", param).FirstOrDefault();

                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetEmailSetting").FirstOrDefault();

                if (result != null)
                {
                    if (pdfFile != null && pdfFile.ContentLength > 0)
                    {
                        // Read the PDF into a memory stream
                        MemoryStream pdfMemoryStream = new MemoryStream();
                        pdfFile.InputStream.CopyTo(pdfMemoryStream);
                        pdfMemoryStream.Position = 0;  // Reset stream position

                        // Prepare email message
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(result.FromEmailId);
                        if (GetPatientEmail != null)
                        {
                            mail.To.Add(GetPatientEmail.Email);
                        }
                        // Replace with recipient email
                        mail.Subject = "Invoice from BVG Wellness";
                        mail.Body = "<p>Please find the attached invoice.</p>";  // Use HTML formatting
                        mail.IsBodyHtml = true;
                        // Attach PDF correctly
                        Attachment pdfAttachment = new Attachment(pdfMemoryStream, "Invoice.pdf", "application/pdf");
                        mail.Attachments.Add(pdfAttachment);

                        // Send email
                        SmtpClient smtp = new SmtpClient(result.SMTPServerName);
                        smtp.Port = result.PortNo; // Change if needed
                        smtp.Credentials = new NetworkCredential(result.FromEmailId, result.SMTPPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                        return Json(new { success = true, message = "Bill details sent successfully!", EncounterFid = Encounterfid });
                    }
                    else
                    {
                        return Json(new { success = false, message = "No file received!", EncounterFid = Encounterfid });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "No file received!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GeneratePdfAndSendEmail(string htmlContent, string EncounterFid)
        {
            try
            {

                // Set base URL for external stylesheets (Change this to your actual URL)
                string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
                htmlContent = "<style>" +
                      "@import url('https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css');" +
                      "</style>" + htmlContent;
                // Convert HTML to PDF with CSS
                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.WebPageWidth = 1024; // Ensures proper rendering
                converter.Options.WebPageHeight = 0;   // Auto height
                //converter.Options.MarginTop = 20;
                //converter.Options.MarginBottom = 20;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;

                // Enable JavaScript for better rendering (Optional)
                converter.Options.JavaScriptEnabled = true;
                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;

                // Convert HTML with base URL to retain styles
                PdfDocument doc = converter.ConvertHtmlString(htmlContent, baseUrl);



               // HtmlToPdf converter = new HtmlToPdf();
                //PdfDocument doc = converter.ConvertHtmlString(htmlContent);
                using (MemoryStream pdfStream = new MemoryStream())
                {
                    doc.Save(pdfStream);
                    doc.Close();

                    pdfStream.Position = 0;

                    // Send email with PDF attachment
                    //SendEmailWithPdf(pdfStream.ToArray(), recipientEmail);
                    byte[] pdfBytes = pdfStream.ToArray();
                    int Encounterfid = Convert.ToInt32(EncounterFid);
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_EncounterFid", 12);
                    var GetPatientEmail = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEmailDeatils", param).FirstOrDefault();

                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetEmailSetting").FirstOrDefault();

                    if (result != null)
                    {
                        // Read the PDF into a memory stream


                        // Prepare email message
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(result.FromEmailId);
                        if (GetPatientEmail != null)
                        {
                            mail.To.Add(GetPatientEmail.Email);
                        }
                        // Replace with recipient email
                        mail.Subject = "Invoice from BVG Wellness";
                        mail.Body = "<p>Please find the attached invoice.</p>";  // Use HTML formatting
                        mail.IsBodyHtml = true;
                        // Attach PDF correctly
                        Attachment attachment = new Attachment(new MemoryStream(pdfBytes), "Document.pdf", "application/pdf");
                        //Attachment pdfAttachment = new Attachment(pdfMemoryStream, "Invoice.pdf", "application/pdf");
                        mail.Attachments.Add(attachment);

                        // Send email
                        SmtpClient smtp = new SmtpClient(result.SMTPServerName);
                        smtp.Port = result.PortNo; // Change if needed
                        smtp.Credentials = new NetworkCredential(result.FromEmailId, result.SMTPPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                        return Json(new { success = true, message = "Priscription sent successfully!", EncounterFid = Encounterfid });

                    }
                    return View();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        #region TestSuggested
        public ActionResult SaveTestSuggested(int? EncounterFid, int? PatientFid, string TestSuggested)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update patientEncounter set TestSuggested='" + TestSuggested + "' where  Deactivate=0 and PatientFid='" + PatientFid + "' and EncounterFid='" + EncounterFid + "'";
                var data1 = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                var data = TestSuggested;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region SaveSpecialNote
        public ActionResult SaveSpecialNote(int? EncounterFid, int? PatientFid, string SpecialNote)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update patientEncounter set SpecialNote='" + SpecialNote + "' where  Deactivate=0 and PatientFid='" + PatientFid + "' and EncounterFid='" + EncounterFid + "'";
                var data1 = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                var data = SpecialNote;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region TestSuggested
        public ActionResult SaveTherapySuggested(int? EncounterFid, int? PatientFid, string Therapy)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update patientEncounter set TherapySuggested='" + Therapy + "' where  Deactivate=0 and PatientFid='" + PatientFid + "' and EncounterFid='" + EncounterFid + "'";
                var data1 = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                var data = Therapy;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region SaveAdvise
        public ActionResult SaveAdvise(int? EncounterFid, int? PatientFid, string Advise)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update patientEncounter set Advise='" + Advise + "' where  Deactivate=0 and PatientFid='" + PatientFid + "' and EncounterFid='" + EncounterFid + "'";
                var data1 = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                var data = Advise;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region showUploadedFile
        public ActionResult ShowUploadedFile(string fileName)
        {
            try
            {
                // Absolute path to the file stored on the C: drive
                //string folderPath = @"C:\BVGWellnessCenter\PatientEncounterFile\";
                string fullPath = fileName;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    return HttpNotFound(); // Return 404 if the file doesn't exist
                }

                // Determine the content type based on the file extension (for example, PDF or Image)
                string contentType = "application/pdf"; // Default for PDF files
                string fileExtension = Path.GetExtension(fileName).ToLower();

                if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    contentType = "image/jpeg"; // Content type for JPEG image
                }
                else if (fileExtension == ".png")
                {
                    contentType = "image/png"; // Content type for PNG image
                }
                else if (fileExtension == ".docx")
                {
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; // Content type for Word document
                }
                // You can add more conditions here for other file types (Excel, Text, etc.)

                // Return the file to the browser
                return File(fullPath, contentType, fileName); // This will either open or download the file
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, show error message, etc.)
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message);
            }
        }

        #endregion
    }
}