//Ha Controller Normal Half Hourse according ahe ata new develope,ent couunt according booking ghaychi ahe tr ha tya adhi ch backup ahe ata count sati new bnvt ahet.
using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
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

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class AppointmentController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/Appointment
        public ActionResult Appointment(string AppointmentFid_Encrypted, int? AppointmentFid, string FromPatientList, int? PatientFid)
        {
            try
            {
                bool isServiceSet = false;
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //if(FromPatientList!=null)
                //{
                //    ViewBag.ShowAddPatientForm = true;
                //    ViewBag.GetPatientData = PatientID;
                //}


                if (AppointmentFid != null)
                {
                    DynamicParameters paramDetails = new DynamicParameters();
                    paramDetails.Add("@p_AppointmentFid", AppointmentFid);
                    var Result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramDetails).FirstOrDefault();
                    ViewBag.PatientDetails = Result;
                }
                Appointment appointment = new Appointment();

                if (FromPatientList != null && PatientFid != null)
                {
                    appointment.PatientFid = PatientFid;   // ✅ Model binding
                    ViewBag.ShowAddPatientForm = true;
                    ViewBag.IsPatientReadonly = true;
                }


                if (AppointmentFid_Encrypted != null)
                {
                    DynamicParameters ParamUpdate = new DynamicParameters();
                    ParamUpdate.Add("@p_AppointmentFid_Encrypted", AppointmentFid_Encrypted);
                    appointment = BVGWellnessCenterDapparORM.ReturnList<Appointment>("sp_List_patientappoinment", ParamUpdate).FirstOrDefault();
                    if (appointment != null)
                    {
                        ViewBag.GetServiceFid = appointment.ServiceFidString.Split(',').ToList();

                    }



                    DynamicParameters paramDoctor = new DynamicParameters();
                    paramDoctor.Add("@p_DoctorId", appointment.DoctorFid);
                    paramDoctor.Add("@p_AppointmentDate", appointment.AppoinmentDate);
                    var GetDoctorSlote = BVGWellnessCenterDapparORM.DynamicList("sp_List_DoctorTimeSlote", paramDoctor);
                    ViewBag.DoctorSlote = GetDoctorSlote;
                    ViewBag.timeSlote = "True";
                    ViewBag.targetStartTime = appointment.SloteStartTime;
                    ViewBag.targetEndTime = appointment.SloteEndTime;
                    // ViewBag.GetServiceDetail = appointment.ServiceDetails.Split(',').ToList();
                    ViewBag.GetServiceDetail = appointment.ServiceDetails?.Split(',')?.ToList() ?? new List<string>();
                    ViewBag.OpenAppointmentForm = "true";
                    ViewBag.GetFileName = Path.GetFileName(appointment.ReportFile);
                    ViewBag.PreviousReportFile = appointment.ReportFile;


                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_Query", "select  ClinicFid , DoctorFid  from patientappoinment where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "'");
                    var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param3).FirstOrDefault();
                    var DoctorFid = data.DoctorFid;
                    var Clinicid = data.ClinicFid;
                    DynamicParameters paramservice = new DynamicParameters();
                    paramservice.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment'  and  Status = 'Active'  and  Deactivate = 0  ");
                    var GetService1 = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);

                    if (appointment.PatientFid == null || appointment.DoctorFid == null)
                    {
                        // 👉 First load (no patient/doctor selected)
                        ViewBag.GetServiceName = GetService1;
                        isServiceSet = true;
                    }
                    else
                    {
                        DateTime? lastVisitDate = null;

                        DynamicParameters lastVisitParam = new DynamicParameters();
                        lastVisitParam.Add("@p_Query", $@"
    SELECT TOP 1 AppoinmentDate 
    FROM PatientAppoinment
    WHERE PatientFid = {appointment.PatientFid}
    AND DoctorFid = {appointment.DoctorFid}
AND AppointmentFid != {appointment.AppointmentFid}   -- 🔥 FIX
    AND Status IN ('Booked','Check In','Check Out')
    ORDER BY AppoinmentDate DESC
");

                        var lastVisit = BVGWellnessCenterDapparORM
                            .ReturnList<dynamic>("sp_ExecuteQuery", lastVisitParam)
                            .FirstOrDefault();

                        if (lastVisit != null)
                        {
                            lastVisitDate = Convert.ToDateTime(lastVisit.AppoinmentDate);
                        }

                        // ================= APPLY LOGIC =================
                        if (GetService1 != null && GetService1.Any())
                        {
                            if (lastVisitDate == null)
                            {
                                // 👉 First Visit → Only Consultation
                                GetService1 = GetService1
                                    .Where(x => x.Name.ToLower().Contains("consultation"))
                                    .ToList();
                            }
                            else
                            {
                                var diffDays = (DateTime.Now - lastVisitDate.Value).TotalDays;

                                // 👉 ≤ 90 days → Only FollowUp (Free)
                                if (diffDays <= 90)
                                {
                                    GetService1 = GetService1
                                        .Where(x =>
                                            x.Name.ToLower().Contains("follow") &&   // 🔥 FIX HERE
                                            !x.Name.ToLower().Contains("3 month") &&
                                            !x.Name.ToLower().Contains("6 month")
                                        )
                                        .ToList();

                                    foreach (var item in GetService1)
                                    {
                                        item.Name += " (Free)";
                                    }
                                }

                                // 👉 3–6 months → Only 3 Month Service
                                else if (diffDays > 90 && diffDays <= 180)
                                {
                                    GetService1 = GetService1
                                        .Where(x => x.Name.ToLower().Contains("3 month"))
                                        .ToList();
                                }

                                // 👉 > 6 months → Only 6 Month Service
                                else
                                {
                                    GetService1 = GetService1
                                        .Where(x => x.Name.ToLower().Contains("6 month"))
                                        .ToList();
                                }
                            }
                            ViewBag.GetServiceName = GetService1;
                            isServiceSet = true;
                        }


                    }
                }


                else
                {
                    ViewBag.GetServiceName = "";
                    ViewBag.timeSlote = null;
                    ViewBag.DoctorSlote = null;
                    ViewBag.GetServiceDetail = null;
                    ViewBag.OpenAppointmentForm = null;
                    ViewBag.GetFileName = null;
                }
                int ClinicFid = Convert.ToInt32(Session["ClinicFid"]);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic where Deactivate=0 and Fid='" + ClinicFid + "'");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if (GetClinic.Count() == 0)
                {
                    ViewBag.ClinicName = "";
                }
                else
                {
                    ViewBag.ClinicName = GetClinic;
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", "select UM.UserId as Id, UM.FirstName + ' ' + UM.LName + case when MD.DepartmentName is null or MD.DepartmentName = '' then '' else ' - (' + MD.DepartmentName + ')' end as Name from UserMaster UM left join MAS_Department MD on MD.DepartmentID = UM.DepartmentID where UM.Deactivate=0 and UM.ClinicFid='" + ClinicFid + "' and UM.IsDoctor=1");

                var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param1);
                if (GetDoctor.Count() != 0)
                {
                    ViewBag.GetDoctorName = GetDoctor;
                }
                else
                {
                    ViewBag.GetDoctorName = "";
                }
                DynamicParameters ParamPatient = new DynamicParameters();
                ParamPatient.Add("@p_Query", "select PatientFid as Id, FirstName + ' ' + LName + '-'+ (ContactNumber) as Name from Patient where Deactivate=0 and Status = 'Active' and    ClinicFid='" + ClinicFid + "'");
                var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", ParamPatient);
                //DynamicParameters ParamPatient = new DynamicParameters();
                //ParamPatient.Add("@p_ClinicFid", ClinicFid);
                //var GetPatient = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_CheckInGetPatient", ParamPatient);
                if (GetPatient.Count() != 0)
                {
                    ViewBag.GetPatientData = GetPatient;
                }
                else
                {
                    ViewBag.GetPatientData = "";
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_AppointmentFid_Encrypted", null);
                var result = BVGWellnessCenterDapparORM.DynamicList("sp_List_patientappoinment", paramList);

                ViewBag.AppoinmentList = result;

                //services
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment'  and  Status = 'Active'  and  Deactivate = 0  ");  // ClinicFid = '" + Session["ClinicFid"] + "'AND Servicecategory IN('Both')
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;
                if (!isServiceSet)
                {
                    ViewBag.GetServiceName = GetService;
                }


                if (AppointmentFid != 0 && AppointmentFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }



                return View(appointment);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        public ActionResult PatientAppointmentDetails(string AppointmentFid_Encrypted, int? AppointmentFid, string FromPatientList)
        {

            DynamicParameters paramDetails = new DynamicParameters();
            paramDetails.Add("@p_AppointmentFid", AppointmentFid);
            var Result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramDetails).FirstOrDefault();
            ViewBag.PatientDetails = Result;

            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        #region SaveAppointment
        [HttpPost]
        public ActionResult SaveAppointment(string startTime, string endTime, Appointment appoinment, HttpPostedFileBase file, string PreviousFileName)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string bookingSource = "Ayurmitra"; // default



                if (Session["Source"] != null && !string.IsNullOrEmpty(Session["Source"].ToString()))
                {
                    bookingSource = Session["Source"].ToString();
                }
                else if (Request.UserAgent != null && Request.UserAgent.ToLower().Contains("android"))
                {
                    bookingSource = "Mobile";
                }
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                string serviceFid = appoinment.ServiceFid;
                // PatientFid वरून MUid (UserId) घेणे
                string userId = "";

                DynamicParameters pUser = new DynamicParameters();
                pUser.Add("@p_Query", "SELECT MUid FROM Patient WHERE PatientFid = '" + appoinment.PatientFid + "'");

                var patientData = BVGWellnessCenterDapparORM
                                    .ReturnList<dynamic>("sp_ExecuteQuery", pUser)
                                    .FirstOrDefault();

                if (patientData != null)
                {
                    userId = Convert.ToString(patientData.MUid);
                }
                // --- नवीन लॉजिक: जुनी सर्व्हिस चेक करण्यासाठी ---
                string oldServiceFid = "";
                string paymentStatus = "";
                if (!string.IsNullOrEmpty(appoinment.AppointmentFid_Encrypted))
                {
                    DynamicParameters pOld = new DynamicParameters();
                    pOld.Add("@p_Query", "SELECT ServiceFid FROM Trn_PatientAppointment WHERE AppointmentFid_Encrypted = '" + appoinment.AppointmentFid_Encrypted + "'");
                    var oldData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pOld).FirstOrDefault();
                    if (oldData != null)
                    {
                        oldServiceFid = Convert.ToString(oldData.ServiceFid);
                        paymentStatus = Convert.ToString(oldData.PaymentStatus);
                    }
                }
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];

                // Check patient already has appointment at same time with any doctor
                DynamicParameters paramCheck = new DynamicParameters();

                paramCheck.Add("@p_Query",
                "SELECT AppointmentFid FROM Trn_PatientAppointment " +
                "WHERE PatientFid='" + appoinment.PatientFid + "' " +
                "AND AppoinmentDate='" + appoinment.AppoinmentDate + "' " +
                "AND SloteStartTime='" + startTime + "' " +
                "AND Status='Booked'");

                var alreadyBooked = BVGWellnessCenterDapparORM
                                    .ReturnList<dynamic>("sp_ExecuteQuery", paramCheck)
                                    .FirstOrDefault();

                if (alreadyBooked != null)
                {
                    TempData["Message"] = "This patient already has appointment at this time.";
                    TempData["Icon"] = "warning";
                    TempData["OpenBillPopup"] = false;

                    return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                }

                // Check doctor already booked for this slot
                DynamicParameters paramDoctorCheck = new DynamicParameters();

                paramDoctorCheck.Add("@p_Query",
                "SELECT AppointmentFid FROM Trn_PatientAppointment " +
                "WHERE DoctorFid='" + appoinment.DoctorFid + "' " +
                "AND AppoinmentDate='" + appoinment.AppoinmentDate + "' " +
                "AND SloteStartTime='" + startTime + "' " +
                "AND Status='Booked'");

                var doctorSlotBooked = BVGWellnessCenterDapparORM
                                        .ReturnList<dynamic>("sp_ExecuteQuery", paramDoctorCheck)
                                        .FirstOrDefault();

                if (doctorSlotBooked != null)
                {
                    TempData["Message"] = "Doctor already has appointment at this time.";
                    TempData["Icon"] = "warning";
                    TempData["OpenBillPopup"] = false;

                    return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(appoinment.AppointmentFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_AppointmentFid_Encrypted", appoinment.AppointmentFid_Encrypted);
                param.Add("@p_ClinicFid", appoinment.ClinicFid);
                param.Add("@p_DoctorFid", appoinment.DoctorFid);
                param.Add("@p_PatientFid", appoinment.PatientFid);
                // जर userId रिकामी असेल तर थेट null पाठवा, DBNull.Value नको
                param.Add("@p_UserId", string.IsNullOrEmpty(userId) ? null : userId);
                param.Add("@p_ServiceFid", serviceFid);
                param.Add("@p_AppoinmentDate", appoinment.AppoinmentDate);
                param.Add("@p_Status", "Booked");
                param.Add("@p_BookingSource", bookingSource);

                string FilePath = @"C:\BVGWellnessCenter\PatientReportFile\\" + appoinment.PatientFid + "\\";

                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }

                    string FullPath = FilePath + Path.GetFileName(File.FileName);
                    File.SaveAs(FullPath);

                    param.Add("@p_ReportFile", FullPath);
                }
                else if (!string.IsNullOrEmpty(PreviousFileName))
                {
                    param.Add("@p_ReportFile", PreviousFileName);
                }

                param.Add("@p_SloteStartTime", startTime);
                param.Add("@p_SloteEndTime", endTime);
                param.Add("@p_ServiceDetails", appoinment.ServiceDetails);
                param.Add("@p_TaxDetails", appoinment.TaxDetails);

                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_AppointmentFid", appoinment.AppointmentFid, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientAppoinment", param);

                string message = param.Get<string>("@p_Message");
                TempData["Message"] = message;
                TempData["Icon"] = param.Get<string>("@p_Icon");

                int? AppointmentFid = param.Get<int>("@p_AppointmentFid");
                bool isServiceChanged = false;

                if (string.IsNullOrEmpty(appoinment.AppointmentFid_Encrypted))
                {
                    // जर नवीन सेव्ह असेल तर बिलिंग व्हायलाच हवे
                    isServiceChanged = true;
                }
                else
                {
                    // अपडेट असेल तर तुलना करा
                    // .Trim() वापरा जेणेकरून रिकाम्या जागांमुळे (spaces) तफावत येणार नाही
                    isServiceChanged = (oldServiceFid.Trim() != serviceFid.Trim());
                }
                // Billing only if appointment saved successfully
                if (AppointmentFid != null && AppointmentFid > 0 && message.Contains("Success"))
                {

                    // १. ईमेलसाठी माहिती गोळा करणारी सुधारित क्वेरी
                    DynamicParameters pInfo = new DynamicParameters();
                    pInfo.Add("@p_Query", $@"
    SELECT P.Email, P.FirstName, 
           (U.FirstName + ' ' + ISNULL(U.LName, '')) as DocName, 
           H.HospitalName 
    FROM Patient P 
    LEFT JOIN UserMaster U ON U.UserId = {appoinment.DoctorFid}
    LEFT JOIN [JYAMDB].[dbo].[HospitalRegistration] H ON H.HospitalId = {appoinment.ClinicFid}
    WHERE P.PatientFid = {appoinment.PatientFid}");

                    var info = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pInfo).FirstOrDefault();

                    // ... [कोडमधील हा भाग शोधा]
                    if (info != null && !string.IsNullOrEmpty(info.Email))
                    {
                        string clinicName = (info.HospitalName != null) ? info.HospitalName.ToString() : "BVG Wellness Center";
                        string pEmail = info.Email.ToString();
                        // सुधारायचा कोड: तारीख फॉरमॅट करा
                        string formattedDate = "";
                        if (appoinment.AppoinmentDate is DateTime)
                        {
                            formattedDate = ((DateTime)appoinment.AppoinmentDate).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            formattedDate = appoinment.AppoinmentDate.ToString();
                        }
                        // ईमेल पाठवा
                        SendAppointmentEmail(pEmail, info.FirstName.ToString(), formattedDate, startTime, info.DocName.ToString(), clinicName);
                    }
                    else
                    {
                        // जर इथे कोड आला, तर याचा अर्थ पेशंटचा ईमेल डेटाबेसमध्ये रिकामा आहे
                        System.Diagnostics.Debug.WriteLine("Patient Email is null or patient info not found.");
                    }
                    bool isNewRecord = string.IsNullOrEmpty(appoinment.AppointmentFid_Encrypted);
                    bool isUnpaid = (paymentStatus.Trim().ToLower() == "unpaid");
                    DynamicParameters paramprice = new DynamicParameters();
                    paramprice.Add("@p_Query", "select Charges from Mas_Service where ServiceFid = '" + serviceFid + "'");

                    var GetServicePrice = BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>("sp_ExecuteQuery", paramprice)
                        .FirstOrDefault();

                    decimal Charges = GetServicePrice.Charges;
                    if (isNewRecord || isUnpaid)
                    {
                        if (Charges > 0)
                        {
                            DynamicParameters paramgetBill = new DynamicParameters();
                            paramgetBill.Add("@p_ProcessType", string.IsNullOrEmpty(appoinment.Bill_Fid_Encrapted) ? "Save" : "Update");
                            paramgetBill.Add("@p_AppointmentFid", AppointmentFid);
                            paramgetBill.Add("@p_Bill_Fid", appoinment.Bill_Fid);
                            paramgetBill.Add("@p_Bill_Fid_Encrapted", appoinment.Bill_Fid_Encrapted);
                            paramgetBill.Add("@p_ServiceFid", serviceFid);
                            paramgetBill.Add("@p_Price", Charges);
                            paramgetBill.Add("@p_Quantity", appoinment.Quantity);
                            paramgetBill.Add("@p_Total", Charges);
                            paramgetBill.Add("@p_EncounterFid", appoinment.EncounterFid);

                            paramgetBill.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            paramgetBill.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                            BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillAppointment", paramgetBill);

                            // Load bill list and open popup for regular paid services
                            DynamicParameters paramBill = new DynamicParameters();
                            paramBill.Add("@p_AppointmentFid", AppointmentFid);

                            var GetBillList = BVGWellnessCenterDapparORM
                                .ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill)
                                .ToList();

                            ViewBag.GenerateBill = GetBillList;
                            TempData["OpenBillPopup"] = true;
                        }
                        else
                        {
                            // Do not make any entries in billing tables for 0 charge appointments
                            TempData["OpenBillPopup"] = false;
                        }
                        // Load bill list
                        //DynamicParameters paramBill = new DynamicParameters();
                        //paramBill.Add("@p_AppointmentFid", AppointmentFid);

                        //var GetBillList = BVGWellnessCenterDapparORM
                        //    .ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill)
                        //    .ToList();

                        //ViewBag.GenerateBill = GetBillList;

                        //TempData["OpenBillPopup"] = true;
                    }
                    else
                    {
                        // जर आधीच 'Paid' असेल तर पॉपअप उघडणार नाही
                        TempData["OpenBillPopup"] = false; 
                    }
                }
                else
                {
                    TempData["OpenBillPopup"] = false;
                }

                return RedirectToAction("Appointment", "Appointment", new { area = "Patient", AppointmentFid = AppointmentFid });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        private void SendAppointmentEmail(string patientEmail, string patientName, string appointmentDate, string startTime, string doctorName, string clinicName)
        {
            try
            {
                // 1. Gmail साठी 'App Password' आवश्यक आहे, तुमचा सामान्य पासवर्ड नाही.
                var fromAddress = new System.Net.Mail.MailAddress("apiprobus@gmail.com", clinicName);
                var toAddress = new System.Net.Mail.MailAddress(patientEmail);
                string fromPassword = "nibd kmnr fbdb pguf";

                var smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
                };

                string emailBody = $@"
                    <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #fdfdfd;'>
                        <div style='text-align: center; border-bottom: 2px solid #5881ed; padding-bottom: 20px; margin-bottom: 20px;'>
                            <h2 style='color: #2c3e50; margin: 0;'>Appointment Confirmation</h2>
                            <p style='color: #7f8c8d; margin: 5px 0 0 0;'>{clinicName}</p>
                        </div>
                        
                        <div style='color: #34495e; line-height: 1.6;'>
                            <p>Dear <b>{patientName}</b>,</p>
                            <p>Your appointment has been successfully booked at <b>{clinicName}</b>. Below are your appointment details:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                                <tr style='background-color: #f2f4f8;'>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left; width: 35%;'>Doctor</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>Dr. {doctorName}</td>
                                </tr>
                                <tr>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>Appointment Date</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{appointmentDate}</td>
                                </tr>
                                <tr style='background-color: #f2f4f8;'>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>Time Slot</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{startTime}</td>
                                </tr>
                            </table>
                            
                            <!-- 15 min warning note box -->
                            <div style='background-color: #fff9e6; border-left: 4px solid #ffc107; padding: 12px; margin: 20px 0; border-radius: 4px;'>
                                <p style='margin: 0; font-size: 14px; color: #856404; font-weight: bold;'>
                                    ⏰ Note:
                                </p>
                                <p style='margin: 5px 0 0 0; font-size: 14px; color: #856404;'>
                                    Please reach the clinic 15 minutes before your scheduled appointment time.
                                </p>
                            </div>
                            
                            <p style='margin-top: 30px;'>If you have any questions or need to reschedule, please contact the clinic reception.</p>
                            <p style='margin-bottom: 0;'>Warm regards,<br/><b>Team {clinicName}</b></p>
                        </div>
                        
                        <div style='margin-top: 30px; padding-top: 15px; border-top: 1px solid #e0e0e0; text-align: center; font-size: 12px; color: #95a5a6;'>
                            This is an automated email confirmation. Please do not reply directly to this message.
                        </div>
                    </div>";


                using (var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                {
                    Subject = "Appointment Confirmation - " + clinicName,
                    Body = emailBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // हे तुम्हाला नक्की सांगेल की एरर कुठे आहे
                throw new Exception("Email Error: " + ex.Message);
            }
        }


        [HttpGet]
        public JsonResult GetDoctorSlots(int DoctorFid, string AppointmentDate)
        {
            DateTime date = Convert.ToDateTime(AppointmentDate);

            DynamicParameters param = new DynamicParameters();

            param.Add("@p_Query",
            "SELECT SloteStartTime FROM Trn_PatientAppointment " +
            "WHERE DoctorFid = '" + DoctorFid + "' " +
            "AND CAST(AppoinmentDate AS DATE) = '" + date.ToString("yyyy-MM-dd") + "' " +
           "AND Status != 'Cancelled' " +
    "AND Deactivate = 0");

            var slots = BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>("sp_ExecuteQuery", param)
                        .ToList();

            return Json(slots, JsonRequestBehavior.AllowGet);
        }

        #region GetTimeSlots
        public ActionResult GetTimeSlots(int DoctorID, int? PatientFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DoctorId", DoctorID);
                var GetDoctorSlote = BVGWellnessCenterDapparORM.DynamicList("sp_List_DoctorTimeSlote", param);

                DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='"+ Session["ClinicFid"] + "' ");               
                paramservice.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment'  and  Status = 'Active'  and  Deactivate = 0"); /*DoctorId='" + data.DoctorFid + "' and*/
                var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);

                // ================= FOLLOW-UP LOGIC START =================

                DateTime? lastVisitDate = null;
                bool isFollowUpFree = false;

                if (PatientFid != null)
                {
                    DynamicParameters lastVisitParam = new DynamicParameters();


                    lastVisitParam.Add("@p_Query", $@"
        SELECT TOP 1 AppoinmentDate 
        FROM PatientAppoinment
        WHERE PatientFid = {PatientFid}
        AND DoctorFid = {DoctorID}

       AND Status IN ('Booked','Check In','Check Out')
        ORDER BY AppoinmentDate DESC
    ");

                    var lastVisit = BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>("sp_ExecuteQuery", lastVisitParam)
                        .FirstOrDefault();

                    if (lastVisit != null)
                    {
                        lastVisitDate = Convert.ToDateTime(lastVisit.AppoinmentDate);
                    }
                }

                // Condition check
                if (GetServiceName != null && GetServiceName.Any())
                {
                    if (lastVisitDate == null)
                    {
                        // 👉 First Visit → Only Consultation
                        GetServiceName = GetServiceName
                            .Where(x => x.Name.ToLower().Contains("consultation"))
                            .ToList();
                    }
                    else
                    {
                        var diffDays = (DateTime.Now - lastVisitDate.Value).TotalDays;

                        // 👉 ≤ 90 days
                        if (diffDays <= 90)
                        {
                            GetServiceName = GetServiceName
                                .Where(x =>
                                {
                                    var name = x.Name.ToLower().Replace("-", "").Replace(" ", "");

                                    return name.Contains("followup") &&
                                           !name.Contains("3month") &&
                                           !name.Contains("6month");
                                })
                                .ToList();

                            // 👉 Only normal follow-up ला Free tag लाव
                            foreach (var item in GetServiceName)
                            {
                                System.Diagnostics.Debug.WriteLine(item.Name);
                                item.Name += " (Free)";

                            }
                        }

                        // 👉 3–6 months
                        else if (diffDays > 90 && diffDays <= 180)
                        {
                            GetServiceName = GetServiceName
                                .Where(x => {
                                    var name = x.Name.ToLower().Replace("-", "").Replace(" ", "");
                                    return name.Contains("3month");
                                })
                                .ToList();
                        }

                        // 👉 > 6 months
                        else
                        {
                            GetServiceName = GetServiceName
                                .Where(x => {
                                    var name = x.Name.ToLower().Replace("-", "").Replace(" ", "");
                                    return name.Contains("6month");
                                })
                                .ToList();
                        }
                    }
                }
                return Json(new { GetDoctorSlote = GetDoctorSlote, GetServiceName = GetServiceName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetServiceChargesTexes
        public ActionResult GetServiceChargesTexes(int serviceFid, int DoctorID)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select  ServiceName + ' - Rs ' + CAST(Charges AS VARCHAR) + ' /-' AS ServiceCharges from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='" + Session["ClinicFid"] + "' and ServiceFid='" + serviceFid + "'");
                //var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);
                paramservice.Add("@p_Query", "select  ServiceName + ' - Rs ' + CAST(Charges AS VARCHAR) + ' /-' AS ServiceCharges from mas_service where ClinicFid='" + Session["ClinicFid"] + "' and ServiceFid='" + serviceFid + "'");
                var data = BVGWellnessCenterDapparORM.DynamicList("sp_ExecuteQuery", paramservice);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteAppointment
        public ActionResult DeleteAppointment(string appointmentFidEncrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_AppointmentFid_Encrypted", appointmentFidEncrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientAppoinment", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });

            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region UpdateBookingStatus
        public ActionResult UpdateBookingStatus(string AppointmentFid_Encrypted, string StatusType)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }


                // 🔹 Step 1: Get AppointmentFid from Encrypted
                DynamicParameters paramGetId = new DynamicParameters();
                paramGetId.Add("@p_Query", "SELECT pa.AppointmentFid, um.IsPaymentMandatory, ISNULL((SELECT SUM(ms.Charges) FROM dbo.SplitString(pa.ServiceFid, ',') AS s INNER JOIN Mas_Service ms ON s.Item = CAST(ms.ServiceFid AS NVARCHAR)), 0) AS TotalCharges FROM PatientAppoinment pa INNER JOIN UserMaster um ON pa.DoctorFid = um.UserId WHERE pa.AppointmentFid_Encrypted = '" + AppointmentFid_Encrypted + "' ");
                var appointmentFid = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramGetId).FirstOrDefault();
                ViewBag.FID = appointmentFid.AppointmentFid;
                var APPID = appointmentFid.AppointmentFid;

                bool isMandatory = appointmentFid.IsPaymentMandatory ?? false;
                decimal totalCharges = appointmentFid.TotalCharges != null ? Convert.ToDecimal(appointmentFid.TotalCharges) : 0m;

                // 🔹 Step 2: Check Bill Logic (Fakt "Check In" sathi aani jar setting ON asel tarach, and if Charges > 0)
                if (StatusType == "Check In" && isMandatory == true && totalCharges > 0)
                {
                    DynamicParameters paramCheckBill = new DynamicParameters();
                    paramCheckBill.Add("@p_Query", $@"SELECT Bill_Fid FROM GenerateBillAppointment where AppointmentFid = '" + APPID + "' ");
                    var billObj = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramCheckBill).FirstOrDefault();

                    if (billObj == null)
                    {
                        TempData["Message"] = "Please Generate bill first";
                        TempData["Icon"] = "warning";
                        return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                    }
                    else
                    {
                        DynamicParameters paramFBill = new DynamicParameters();
                        paramFBill.Add("@p_Query", $@"select Status from FinalBillAppointment WHERE AppointmentFid = '" + APPID + "' ");
                        var BSTATUS_OBJ = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFBill).FirstOrDefault();
                        string BSTATUS = BSTATUS_OBJ != null ? BSTATUS_OBJ.Status : "";

                        if (BSTATUS_OBJ == null || BSTATUS != "Paid")
                        {
                            TempData["Message"] = "Please bill Paid first";
                            TempData["Icon"] = "warning";
                            return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                        }
                    }
                }

                // 🔹 Step 2: Check Bill Generated or Not


                // bill exists → continue


                DynamicParameters param = new DynamicParameters();
                if (StatusType == "Check In")
                {
                    param.Add("@p_Query", "update PatientAppoinment set Status='" + StatusType + "' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
                }
                else if (StatusType == "Booked")
                {
                    param.Add("@p_Query", "update PatientAppoinment set Status='" + StatusType + "' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
                }
                else if (StatusType == "Check Out")
                {
                    param.Add("@p_Query", "update PatientAppoinment set Status='" + StatusType + "' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
                }
                var updateStatus = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", param);

                TempData["Message"] = "Status updated successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });


            }
            catch (Exception Ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetTimeSlotsAppointmentdateWise
        public ActionResult GetTimeSlotsAppointmentdateWise(int? DoctorID, string appointmentDate)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DateTime AppointmentDate = Convert.ToDateTime(appointmentDate);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DoctorId", DoctorID);
                param.Add("@p_AppointmentDate", appointmentDate);
                var GetDoctorSlote = BVGWellnessCenterDapparORM.DynamicList("sp_List_DoctorTimeSlote", param);



                // २. सर्व्हिस लिस्ट सुद्धा पुन्हा घ्या जेणेकरून UI वर सर्व्हिस ड्रॉपडाउन व्यवस्थित राहील.
                //DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment' and Status = 'Active' and Deactivate = 0");
                //var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);

                //DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='" + Session["ClinicFid"] + "' ");
                ////paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where Deactivate=0 ");
                //var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);


                //DynamicParameters paramservice = new DynamicParameters();
                ////paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='"+ Session["ClinicFid"] + "' ");               
                //paramservice.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment'  and  Status = 'Active'  and  Deactivate = 0  "); /*DoctorId='" + data.DoctorFid + "' and*/
                //var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);

                return Json(new { GetDoctorSlote = GetDoctorSlote,/* GetServiceName = GetServiceName*/ }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region CloseAppointmentStatus
        public ActionResult CloseAppointmentStatus(string AppointmentFid_Encrypted)
        {
            try
            {
                Appointment appointment = new Appointment();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", "select Status from PatientAppoinment where Deactivate=0 and AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "'");
                appointment = BVGWellnessCenterDapparORM.ReturnList<Appointment>("sp_ExecuteQuery", param1).FirstOrDefault();

                if (appointment.Status == "Check In")
                {
                    DynamicParameters paramcheckout = new DynamicParameters();
                    paramcheckout.Add("@p_Query", "update PatientAppoinment set Status='Check Out' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "'");
                    var CheckOut = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramcheckout);
                }
                else
                {
                    TempData["Message"] = "Please Close Billing";
                    TempData["Icon"] = "warning";
                }
                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region CancelAppointment
        public ActionResult CancelAppointment(string AppointmentFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", @"
    UPDATE PatientAppoinment 
    SET 
        Status = 'Cancelled',
        Deactivate = 1
    WHERE AppointmentFid_Encrypted = @AppointmentFid_Encrypted
    AND Deactivate = 0
");

                param.Add("@AppointmentFid_Encrypted", AppointmentFid_Encrypted);
                //  param.Add("@p_Query", "update PatientAppoinment set Status='Cancelled' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
                var CancelStatus = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", param);
                if (CancelStatus != null)
                {
                    TempData["Message"] = "Appointment Cancelled";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region Appoinement Billing 

        [HttpPost]
        public ActionResult SaveGenerateBillAppointment(Billing Bill, int? AppointmentFid)
        {
            try
            {

                //string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                //var GetBillList="";
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(Bill.Bill_Fid_Encrapted) ? "Save" : "Update");
                //param.Add("@p_MacId", LoginMachinId);
                //param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_Bill_Fid", Bill.Bill_Fid);
                param.Add("@p_Bill_Fid_Encrapted", Bill.Bill_Fid_Encrapted);
                //if (Bill.ServiceFid != 0 && Bill.ServiceFid != null)
                //{
                param.Add("@p_ServiceFid", Bill.ServiceFid);
                //}
                //else if (Bill.Service_Fid != 0 && Bill.Service_Fid != null)
                //{
                //    param.Add("@p_ServiceFid", Bill.Service_Fid);
                //}
                param.Add("@p_Price", Bill.Price);
                param.Add("@p_Quantity", Bill.Quantity);
                param.Add("@p_Total", Bill.Total);
                param.Add("@p_EncounterFid", Bill.EncounterFid);
                param.Add("@p_AppointmentFid", Bill.AppointmentFid);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillAppointment", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.AppointmentFid != 0 && Bill.AppointmentFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", Bill.AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                return Json(new { GetBillList = ViewBag.GenerateBill }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }


        //[HttpPost]
        //public ActionResult SaveGenerateBillAppointment(Billing Bill,int AppointmentFid/*int AppointmentFid,string Bill_Fid_Encrapted,int ServiceFid , float Total , int Quantity ,float Price*/)
        //{
        //    try
        //    {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        /// }

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
                    GetBillList = BVGWellnessCenterDapparORM.ReturnList<ClsPrescription>("sp_List_GenerateBillAppointment", paramBill).FirstOrDefault();
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
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion




        [HttpPost]
        public ActionResult SaveAndCloseEncounterAppointment(List<ClsPrescription> paymentList, string ISOutStandingAmt)
        {
            try
            {
                var appointmentId = paymentList.First().AppointmentFid;

                for (int i = 0; i < paymentList.Count(); i++)
                {
                    var FlageId = "";
                    if (ISOutStandingAmt == "Yes")
                    {
                        //FlageId = $"{i + 1}{paymentList[i].AppointmentFid}{paymentList[i].Id}";
                        //FlageId = Guid.NewGuid().ToString();
                        FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                    }
                    else
                    {
                        FlageId = $"{i + 1}{paymentList[i].AppointmentFid}";
                    }


                    DynamicParameters check = new DynamicParameters();
                    check.Add("@p_Query", "select FlageID from FinalBillAppointment where Deactivate=0 and AppointmentFid='" + paymentList[i].AppointmentFid + "' and FlageID='" + FlageId + "'");

                    var exist = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", check).FirstOrDefault();

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_MacId", Environment.MachineName);
                    param.Add("@p_MacIp", "0.0.0.0");

                    if (exist != null)
                        param.Add("@p_AlreadyExist", "Yesh");
                    else
                        param.Add("@p_AlreadyExist", null);

                    param.Add("@p_AppointmentFid", paymentList[i].AppointmentFid);
                    param.Add("@p_PatientFid", paymentList[i].PatientFid);
                    param.Add("@p_TotalAmount", paymentList[i].finalAmount);
                    param.Add("@p_DiscountAmt", paymentList[i].Discount);
                    param.Add("@p_PayableAmount", paymentList[i].FinalPaybalAmount);
                    param.Add("@p_Status", paymentList[i].PaymentStatus);
                    param.Add("@p_PaymentType", paymentList[i].PaymentType);
                    param.Add("@p_ReferenceNumber", paymentList[i].ReferenceNumber);
                    param.Add("@p_Amount", paymentList[i].Amount);
                    param.Add("@p_FlageID", exist != null ? exist.FlageID : FlageId);

                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBillAppointment", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");


                    if (ISOutStandingAmt == "Yes")
                    {

                        DynamicParameters OutStanding = new DynamicParameters();

                        //  DynamicParameters OutStanding = new DynamicParameters();
                        OutStanding.Add("@p_Query", "update FinalBillAppointment set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

                        var Standing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", OutStanding).FirstOrDefault();
                        TempData["Message"] = param.Get<string>("@p_Message");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }


                }

                // for outstanding 
                DynamicParameters outstandparam = new DynamicParameters();
                outstandparam.Add("@p_Query", $@"
            SELECT 
                AppointmentFid,
                MAX(PayableAmount) AS PayableAmount,
                SUM(Amount) AS TotalPaid,
                (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount
            FROM FinalBillAppointment
            WHERE AppointmentFid = {appointmentId}
            GROUP BY AppointmentFid");

                var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

                //if (summary.TotalAmount != summary.TotalPaid)
                if (summary != null)
                {
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_OutstandingAppointment", sumInsUpd);



                }


                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion  Appoinemnt Billing

        #region GetBillRecord
        public ActionResult GetMedicineBillRecordAppointment(int? PatientFid, int? AppointmentFid, int? Bill_Fid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string paymentStatus = "";
                //if (EncounterFid != null && EncounterFid!=0)
                //{
                //    DynamicParameters paramBill = new DynamicParameters();
                //    paramBill.Add("@p_EncounterFid", EncounterFid);
                //    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                //    ViewBag.GenerateBill = GetBillList;
                //    DynamicParameters paramFinal = new DynamicParameters();
                //    paramFinal.Add("@p_Query", "select Status,DiscountAmt from finalbill where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                //    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                //     paymentStatus = FinalBill?.Status ?? "Unknown";
                //    ViewBag.Discountamt = FinalBill?.DiscountAmt ?? "Unknown";
                //    DynamicParameters paramFinalbill = new DynamicParameters();
                //    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from finalbill where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                //    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                //    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                //}
                if (PatientFid != null && PatientFid != 0)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@PatientFid", PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    // ==================== नवीन लॉजिक सुरुवात ====================
                    // जर मोबाईल बुकिंगमुळे बिल लिस्ट रिकामी असेल, तर डिफॉल्ट एन्ट्री बनवणे
                    if (GetBillList == null || GetBillList.Count == 0)
                    {
                        // १. PatientAppoinment मधून ServiceFid शोधणे
                        DynamicParameters paramAppt = new DynamicParameters();
                        paramAppt.Add("@p_Query", "SELECT ServiceFid FROM PatientAppoinment WHERE AppointmentFid = " + AppointmentFid);
                        var apptData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppt).FirstOrDefault();

                        if (apptData != null && apptData.ServiceFid != null)
                        {
                            string serviceFid = Convert.ToString(apptData.ServiceFid);

                            // २. Mas_Service मधून चार्जेस (Charges) शोधणे
                            DynamicParameters paramPrice = new DynamicParameters();
                            paramPrice.Add("@p_Query", "SELECT Charges FROM Mas_Service WHERE ServiceFid = '" + serviceFid + "'");
                            var servicePriceObj = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramPrice).FirstOrDefault();
                            decimal charges = servicePriceObj != null ? Convert.ToDecimal(servicePriceObj.Charges) : 0;

                            // ३. GenerateBillAppointment मध्ये डिफॉल्ट बिल आयटम सेव्ह करणे
                            DynamicParameters paramgetBill = new DynamicParameters();
                            paramgetBill.Add("@p_ProcessType", "Save");
                            paramgetBill.Add("@p_AppointmentFid", AppointmentFid);
                            paramgetBill.Add("@p_Bill_Fid", null);
                            paramgetBill.Add("@p_Bill_Fid_Encrapted", null);
                            paramgetBill.Add("@p_ServiceFid", serviceFid);
                            paramgetBill.Add("@p_Price", charges);
                            paramgetBill.Add("@p_Quantity", 1);
                            paramgetBill.Add("@p_Total", charges);
                            paramgetBill.Add("@p_EncounterFid", null);
                            paramgetBill.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            paramgetBill.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillAppointment", paramgetBill);

                            // ४. बिल लिस्ट पुन्हा लोड करणे जेणेकरून ती टेबलमध्ये दिसेल
                            GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                        }
                    }
                    // ==================== नवीन लॉजिक शेवट ====================
                    ViewBag.GenerateBill = GetBillList;
                    ViewBag.AppointmentFid = AppointmentFid;
                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status from FinalBillAppointment  where Deactivate = 0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    paymentStatus = FinalBill?.Status ?? "Unknown";
                    DynamicParameters paramFinal1 = new DynamicParameters();
                    paramFinal1.Add("@p_Query", "select Status,DiscountAmt from FinalBillAppointment  where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill1 = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal1).FirstOrDefault();
                    paymentStatus = FinalBill1?.Status ?? "0";
                    ViewBag.Discountamt = FinalBill1?.DiscountAmt ?? "0";
                }
                //ViewBag.GenerateBill = GetBillList;
                return Json(new { GetBillList = ViewBag.GenerateBill, paymentStatus = paymentStatus, DiscountAmt = ViewBag.Discountamt, GetFinalbillDetails = ViewBag.GetFinalbillDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion

        #region PaidBillRecords
        public ActionResult AppoinmentsPaidBillRecords(int? AppointmentFid, string page)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // --- Clinic-wise details & Image fetch logic ---
                var clinicCode = Session["UserClinicCode"];
                if (clinicCode != null)
                {
                    DynamicParameters paramClinic = new DynamicParameters();
                    paramClinic.Add("@p_Query", "SELECT HospitalName, Address, ClinicImageFile FROM [JYAMDB].[dbo].[HospitalRegistration] WHERE Con_hospitalCode = '" + clinicCode + "'");
                    var clinicInfo = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramClinic).FirstOrDefault();
                    if (clinicInfo != null)
                    {
                        ViewBag.ClinicName = clinicInfo.HospitalName;
                        ViewBag.ClinicAddress = clinicInfo.Address != null
                            ? Convert.ToString(clinicInfo.Address).Replace("\r\n", "<br />").Replace("\n", "<br />")
                            : "";

                        // Image Path Processing
                        if (clinicInfo.ClinicImageFile != null)
                        {
                            string imagePath = Convert.ToString(clinicInfo.ClinicImageFile);
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                if (System.IO.File.Exists(imagePath))
                                {
                                    ViewBag.ClinicLogo = BVGWellnessCenterDapparORM.ConvertImageToBase64(imagePath);
                                }
                                else
                                {
                                    try
                                    {
                                        string mappedPath = Server.MapPath(imagePath);
                                        if (System.IO.File.Exists(mappedPath))
                                        {
                                            ViewBag.ClinicLogo = BVGWellnessCenterDapparORM.ConvertImageToBase64(mappedPath);
                                        }
                                        else
                                        {
                                            ViewBag.ClinicLogo = Url.Content(imagePath);
                                        }
                                    }
                                    catch
                                    {
                                        ViewBag.ClinicLogo = Url.Content(imagePath);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (Session["ClinicFid"] != null)
                {
                    DynamicParameters paramClinic = new DynamicParameters();
                    paramClinic.Add("@p_Query", "SELECT HospitalName, Address, ClinicImageFile FROM [JYAMDB].[dbo].[HospitalRegistration] WHERE HospitalId = '" + Session["ClinicFid"] + "'");
                    var clinicInfo = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramClinic).FirstOrDefault();
                    if (clinicInfo != null)
                    {
                        ViewBag.ClinicName = clinicInfo.HospitalName;
                        ViewBag.ClinicAddress = clinicInfo.Address != null
                            ? Convert.ToString(clinicInfo.Address).Replace("\r\n", "<br />").Replace("\n", "<br />")
                            : "";

                        // Image Path Processing
                        if (clinicInfo.ClinicImageFile != null)
                        {
                            string imagePath = Convert.ToString(clinicInfo.ClinicImageFile);
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                if (System.IO.File.Exists(imagePath))
                                {
                                    ViewBag.ClinicLogo = BVGWellnessCenterDapparORM.ConvertImageToBase64(imagePath);
                                }
                                else
                                {
                                    try
                                    {
                                        string mappedPath = Server.MapPath(imagePath);
                                        if (System.IO.File.Exists(mappedPath))
                                        {
                                            ViewBag.ClinicLogo = BVGWellnessCenterDapparORM.ConvertImageToBase64(mappedPath);
                                        }
                                        else
                                        {
                                            ViewBag.ClinicLogo = Url.Content(imagePath);
                                        }
                                    }
                                    catch
                                    {
                                        ViewBag.ClinicLogo = Url.Content(imagePath);
                                    }
                                }
                            }
                        }
                    }
                }

                if (ViewBag.ClinicName == null)
                {
                    ViewBag.ClinicName = "Ayurvijnana Chikitsalaya";
                    ViewBag.ClinicAddress = "80/635 Nandraj Sankul Society,<br />Famous Chowk, Main Road,<br />New Sangavi-411027,MAHARASHTRA,INDIA";
                }
                // ----------------------------------------

                // ----------------------------------------

                // ----------------------------------------

                if (AppointmentFid != null)
                {

                    ViewBag.Page = page;
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetails", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalBillAppointment where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Status,Amount,PaymentReferenceNumber,PaymentMode from FinalBillAppointment where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT AppointmentFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalBillAppointment WHERE AppointmentFid ='" + AppointmentFid + "' GROUP BY AppointmentFid ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_AppoinmentsPaidBills");
                ViewBag.GetPaidBillList = GetPaidBillList;

                return View();
            }
            catch (Exception Ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region CancelAppointment
        public ActionResult CancelAppointments(string AppointmentFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // 1. Pahile check kara ki appointment kontya date chi ahe
                DynamicParameters paramGet = new DynamicParameters();
                paramGet.Add("@p_Query", "SELECT AppoinmentDate FROM PatientAppoinment WHERE AppointmentFid_Encrypted = '" + AppointmentFid_Encrypted + "' AND Deactivate=0");

                var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramGet).FirstOrDefault();

                if (data != null)
                {
                    DateTime appointmentDate = Convert.ToDateTime(data.AppoinmentDate);

                    // 2. Jar appointment AAJCHI (Same Day) asel tar cancel hou deu naka
                    if (appointmentDate.Date == DateTime.Now.Date)
                    {
                        TempData["Message"] = "Same day appointment cannot be cancelled.";
                        TempData["Icon"] = "warning";
                        return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                    }

                    // 3. Jar aajchi nasei tar CANCEL kara
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Query", "UPDATE PatientAppoinment SET Status='Cancelled' WHERE AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' AND Deactivate=0");

                    var CancelStatus = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", param);

                    if (CancelStatus != null)
                    {
                        TempData["Message"] = "Appointment Cancelled Successfully";
                        TempData["Icon"] = "success";
                    }
                }
                else
                {
                    TempData["Message"] = "Appointment not found.";
                    TempData["Icon"] = "error";
                }

                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region Game 

        [HttpGet]
        public ActionResult ReceptionSkippedQuestions(int userId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@UserId", userId);
                param.Add("@Origin", "GetReceptionSkippedQuestion");

                var skippedQuestions = BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_GetFullGameReport", param).ToList();

                ViewBag.SkippedQuestions = skippedQuestions;
                ViewBag.TotalSkipped = skippedQuestions.Count;
                ViewBag.UserId = userId;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public JsonResult CheckUserSkippedStatus(int userId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@UserId", userId);
                param.Add("@Origin", "CheckUserSkippedQuestions");

                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_GetFullGameReport", param).FirstOrDefault();

                if (result != null)
                {
                    return Json(new
                    {
                        hasSkipped = Convert.ToBoolean(result.HasSkipped),
                        skippedCount = Convert.ToInt32(result.SkippedCount)
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { hasSkipped = false, skippedCount = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { hasSkipped = false, error = ex.Message });
            }
        }



        [HttpPost]
        public JsonResult UpdateSkippedAnswer(int userGameId, int questionId, int levelId, string newAnswer)
        {
            try
            {
                int receptionistId = Convert.ToInt32(Session["UserId"]);

                DynamicParameters param = new DynamicParameters();
                param.Add("@UserId", 0);  // Not needed for update
                param.Add("@Origin", "UpdateAnswerByReception");
                param.Add("@UserGameId", userGameId);
                param.Add("@QuestionId", questionId);
                param.Add("@LevelId", levelId);
                param.Add("@NewAnswer", newAnswer);
                //param.Add("@ReceptionistId", receptionistId);

                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_GetFullGameReport", param);

                return Json(new { success = true, message = "Answer updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpGet]
        public ActionResult GetSectionDetailsPartial(int AppointmentFid, string optionId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@AppointmentFid", AppointmentFid);
                param.Add("@OptionId", optionId);

                var details = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_GetSectionDetails_Simple", param).ToList();

                ViewBag.OptionId = optionId;
                ViewBag.Details = details;
                ViewBag.TotalCount = details.Count;

                // Section Name
                string sectionName = "";
                switch (optionId)
                {
                    case "A": sectionName = "Wellness Zone"; break;
                    case "B": sectionName = "Balance Zone"; break;
                    case "C": sectionName = "Imbalance Zone"; break;
                    case "D": sectionName = "Deep Imbalance"; break;
                }
                ViewBag.SectionName = sectionName;

                return PartialView("_SectionDetailsPartial");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Content($"<p class='text-danger'>Error: {ex.Message}</p>");
            }
        }


        #endregion Game

    }

}





