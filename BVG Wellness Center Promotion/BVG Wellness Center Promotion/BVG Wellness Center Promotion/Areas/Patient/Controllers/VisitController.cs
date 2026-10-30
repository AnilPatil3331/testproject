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
    public class VisitController : Controller
    {
        // GET: Patient/Visit
        public ActionResult Index()
        {
            return View();
        }


        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/Appointment
        public ActionResult Visit(string VisitFid_Encrypted, int? VisitFid, string FromPatientList)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (FromPatientList != null)
                {
                    ViewBag.ShowAddPatientForm = true;
                }
                if (VisitFid != null)
                {
                    DynamicParameters paramDetails = new DynamicParameters();
                    paramDetails.Add("@p_VisitFid", VisitFid);
                    var Result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientVisiting", paramDetails).FirstOrDefault();
                    ViewBag.PatientDetails = Result;
                }
                Appointment appointment = new Appointment();
                if (VisitFid_Encrypted != null)
                {
                    DynamicParameters ParamUpdate = new DynamicParameters();
                    ParamUpdate.Add("@p_VisitFid_Encrypted", VisitFid_Encrypted);
                    appointment = BVGWellnessCenterDapparORM.ReturnList<Appointment>("sp_List_patientVisiting", ParamUpdate).FirstOrDefault();
                    if (appointment != null)
                    {
                        ViewBag.GetServiceFid = appointment.ServiceFidString.Split(',').ToList();

                    }

                    //DynamicParameters paramDoctor = new DynamicParameters();
                    //paramDoctor.Add("@p_DoctorId", appointment.DoctorFid);
                    //paramDoctor.Add("@p_AppointmentDate", appointment.DoctorFid);
                    //var GetDoctorSlote = BVGWellnessCenterDapparORM.DynamicList("sp_List_DoctorTimeSlote", paramDoctor);
                    //ViewBag.DoctorSlote = GetDoctorSlote;
                    //ViewBag.timeSlote = "True";
                    //ViewBag.targetStartTime = appointment.SloteStartTime;
                    //ViewBag.targetEndTime = appointment.SloteEndTime;
                    //// ViewBag.GetServiceDetail = appointment.ServiceDetails.Split(',').ToList();
                    //ViewBag.GetServiceDetail = appointment.ServiceDetails?.Split(',')?.ToList() ?? new List<string>();
                    //ViewBag.OpenAppointmentForm = "true";
                    //ViewBag.GetFileName = Path.GetFileName(appointment.ReportFile);
                    //ViewBag.PreviousReportFile = appointment.ReportFile;


                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_Query", "select  ClinicFid , DoctorFid  from PatientVisit where VisitFid_Encrypted='" + VisitFid_Encrypted + "'");
                    var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param3).FirstOrDefault();
                    var DoctorFid = data.DoctorFid;
                    var Clinicid = data.ClinicFid;
                    DynamicParameters paramservice = new DynamicParameters();
                    paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where  ClinicFid='" + data.ClinicFid + "' "); /*DoctorId='" + data.DoctorFid + "' and*/
                    var GetService1 = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);
                    ViewBag.GetServiceName = GetService1;
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
                param1.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where Deactivate=0 and ClinicFid='" + ClinicFid + "' and IsDoctor=1");
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
                paramList.Add("@p_VisitFid_Encrypted", null);
                var result = BVGWellnessCenterDapparORM.DynamicList("sp_List_patientVisiting", paramList);

                ViewBag.AppoinmentList = result;

                //services
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as Id, ServiceName as Name from mas_service where [Group] = 'Appointment'  and  Status = 'Active'  and  Deactivate = 0  ");  // ClinicFid = '" + Session["ClinicFid"] + "'AND Servicecategory IN('Both')
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;


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
        public ActionResult SaveVisit(string startTime, string endTime,/* string ServiceDetaiAmount,*/ List<int> ServiceFid, Appointment appoinment, HttpPostedFileBase file, string PreviousFileName)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //string serviceFid = string.Join(",", appoinment.ServiceFid);
                string serviceFid = appoinment.ServiceFid != null ? string.Join(",", appoinment.ServiceFid) : "";
                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(appoinment.VisitFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_VisitFid", appoinment.VisitFid);
                param.Add("@p_VisitFid_Encrypted", appoinment.VisitFid_Encrypted);
                param.Add("@p_ClinicFid", appoinment.ClinicFid);
                param.Add("@p_DoctorFid", appoinment.DoctorFid);
                param.Add("@p_PatientFid", appoinment.PatientFid);
                param.Add("@p_ServiceFid", serviceFid);
                param.Add("@p_AppoinmentDate", appoinment.AppoinmentDate);
                param.Add("@p_Status", appoinment.Status);
                string FilePath = @"C:\BVGWellnessCenter\PatientReportFile\\" + appoinment.PatientFid + "\\";
                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@p_ReportFile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                else if (PreviousFileName != null && PreviousFileName != "")
                {
                    param.Add("@p_ReportFile", PreviousFileName);
                }
                param.Add("@p_SloteStartTime", startTime);
                param.Add("@p_SloteEndTime", endTime);
                param.Add("@p_ServiceDetails", appoinment.ServiceDetails);
                param.Add("@p_TaxDetails", appoinment.TaxDetails);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_PatientVisit", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Visit", "Visit", new { area = "Patient" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetTimeSlots
        public ActionResult GetTimeSlots(int DoctorID)
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
                paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where Status =  'Active'  and  Deactivate= 0 and  ClinicFid='" + Session["ClinicFid"] + "'AND Servicecategory IN ('OPD', 'Both') "); /*DoctorId='" + data.DoctorFid + "' and*/
                var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);
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
                return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
            }
            catch (Exception)
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

                //DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='" + Session["ClinicFid"] + "' ");
                ////paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where Deactivate=0 ");
                //var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);


                DynamicParameters paramservice = new DynamicParameters();
                //paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where DoctorId='" + DoctorID + "' and ClinicFid='"+ Session["ClinicFid"] + "' ");               
                paramservice.Add("@p_Query", "select ServiceFid as Id,ServiceName as Name from mas_service where Status =  'Active'  and  Deactivate= 0 and  ClinicFid='" + Session["ClinicFid"] + "'AND Servicecategory IN ('OPD', 'Both') "); /*DoctorId='" + data.DoctorFid + "' and*/
                var GetServiceName = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramservice);

                return Json(new { GetDoctorSlote = GetDoctorSlote }, JsonRequestBehavior.AllowGet);
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
                if (appointment.Status == "Close")
                {
                    DynamicParameters paramcheckout = new DynamicParameters();
                    paramcheckout.Add("@p_Query", "update PatientAppoinment set EncounterStatus='Close' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "'");
                    var CheckOut = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramcheckout);
                }
                else
                {
                    TempData["Message"] = "Please Close Encounter";
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
                param.Add("@p_Query", "update PatientAppoinment set Status='Cancelled' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
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


    }
}