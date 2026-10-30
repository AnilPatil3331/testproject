using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BVG_Wellness_Center_Promotion.Filters;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    [AuthorizeUser]
    public class DashboardController : Controller
    {
        // GET: Patient/Dashboard
        public ActionResult Dashboard(string UpcomingAppoint, int? filterMonth, int? filterYear)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                int UserId = Convert.ToInt32(Session["UserID"]);
                var getWeeklyAppointCount = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetWeeklyAppointment").ToList();
                if(getWeeklyAppointCount!=null && getWeeklyAppointCount.Count > 0)
                {
                    var categories = getWeeklyAppointCount.Select(item => (string)item.DayOfWeek).ToList();
                    var appointmentCounts = getWeeklyAppointCount.Select(item => (int)item.AppointmentCount).ToList();
                    ViewBag.getWeeklyAppointCount = appointmentCounts;
                    ViewBag.categories = categories;
                    ViewBag.WeeklyTotalAppointments = appointmentCounts.Sum();
                }else
                {
                    ViewBag.getWeeklyAppointCount = null;
                    ViewBag.categories = null;
                    ViewBag.WeeklyTotalAppointments = 0;
                }
               
                if (UpcomingAppoint!=null)
                {
                    DynamicParameters paramAppointment = new DynamicParameters();
                    paramAppointment.Add("@p_AppointmentFid_Encrypted", null);
                    //paramAppointment.Add("@p_DoctorFid", Convert.ToInt32(Session["UserID"]));
                    var paramAppointmentList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramAppointment).ToList();
                    if (Session["UserRole"] != null &&  Session["UserRole"].ToString() == "IsDoctor")
                    {
                       
                        var todayAppointments = paramAppointmentList.Where(m => m.AppoinmentDate > DateTime.Today && m.DoctorFid == UserId).ToList();
                        ViewBag.todayAppointments = todayAppointments;
                    }
                    else
                    {
                        var todayAppointments = paramAppointmentList.Where(m => m.AppoinmentDate > DateTime.Today).ToList();
                        ViewBag.todayAppointments = todayAppointments;
                    }                  
                  
                    ViewBag.UpcomingAppointment = true;
                }
                else
                {
                    DynamicParameters paramAppointment = new DynamicParameters();
                    paramAppointment.Add("@p_AppointmentFid_Encrypted", null);
                    //paramAppointment.Add("@p_DoctorFid", Convert.ToInt32(Session["UserID"]));
                    var paramAppointmentList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramAppointment).ToList();
                    if(Session["UserRole"] != null && Session["UserRole"].ToString()== "IsDoctor")
                    {
                       
                        var todayAppointments = paramAppointmentList.Where(m => m.AppoinmentDate == DateTime.Today && m.DoctorFid == UserId).ToList();
                        ViewBag.todayAppointments = todayAppointments;
                    }
                    else
                    {
                        var todayAppointments = paramAppointmentList.Where(m => m.AppoinmentDate == DateTime.Today).ToList();
                        ViewBag.todayAppointments = todayAppointments;
                    }                 
                   
                }
                DynamicParameters paramRecep = new DynamicParameters();
                paramRecep.Add("@p_IsReceptionist", 1);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_UserList", paramRecep).ToList();
                ViewBag.ReceptionistCount = result.Count();

                DynamicParameters paramDoctor = new DynamicParameters();
                paramDoctor.Add("@p_IsDoctor", 1);
                var DoctorList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_UserList", paramDoctor).ToList();
                ViewBag.DoctorListCount = DoctorList.Count();

                DynamicParameters paramClinic = new DynamicParameters();
                paramClinic.Add("@p_Fid_Encrypted", null);
                var ClinicList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_Mas_Clinic", paramClinic).ToList();
                ViewBag.ClinicListCount = ClinicList.Count();



                int selectedMonth = filterMonth ?? 0;
                int selectedYear = filterYear ?? 0;

                string dateCondition = "";
                if (selectedMonth > 0)
                {
                    dateCondition += " AND MONTH(FDate) = " + selectedMonth;
                }
                if (selectedYear > 0)
                {
                    dateCondition += " AND YEAR(FDate) = " + selectedYear;
                }

                // Pass selected values back to View for binding
                ViewBag.SelectedMonth = selectedMonth;
                ViewBag.SelectedYear = selectedYear;

                // 1. Consultation / Encounter Revenue (FinalBill)
                DynamicParameters paramConsultation = new DynamicParameters();
                paramConsultation.Add("@p_Query", "SELECT ISNULL(SUM(Amount), 0) AS Total FROM FinalBill WHERE (Status='Paid' OR IsOutstanding=1) AND Deactivate=0" + dateCondition);
                var consultationData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramConsultation).FirstOrDefault();
                double consultationRevenue = consultationData != null && consultationData.Total != null ? Convert.ToDouble(consultationData.Total) : 0;
                ViewBag.ConsultationRevenue = Convert.ToDouble(String.Format("{0:0.00}", consultationRevenue));

                // 2. Appointment Booking Revenue (FinalBillAppointment)
                DynamicParameters paramAppointmentRev = new DynamicParameters();
                paramAppointmentRev.Add("@p_Query", "SELECT ISNULL(SUM(Amount), 0) AS Total FROM FinalBillAppointment WHERE (Status='Paid' OR IsOutstanding=1) AND Deactivate=0" + dateCondition);
                var appointmentData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppointmentRev).FirstOrDefault();
                double appointmentRevenue = appointmentData != null && appointmentData.Total != null ? Convert.ToDouble(appointmentData.Total) : 0;
                ViewBag.AppointmentRevenue = Convert.ToDouble(String.Format("{0:0.00}", appointmentRevenue));

                // 3. Therapy & Services Revenue (FinalservicesBill)
                DynamicParameters paramTherapy = new DynamicParameters();
                paramTherapy.Add("@p_Query", "SELECT ISNULL(SUM(Amount), 0) AS Total FROM FinalservicesBill WHERE (Status='Paid' OR IsOutstanding=1) AND Deactivate=0" + dateCondition);
                var therapyData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramTherapy).FirstOrDefault();
                double therapyRevenue = therapyData != null && therapyData.Total != null ? Convert.ToDouble(therapyData.Total) : 0;
                ViewBag.TherapyRevenue = Convert.ToDouble(String.Format("{0:0.00}", therapyRevenue));

                // 4. Prescription Medicine Revenue (FinalMedicineBill)
                DynamicParameters paramMedicine = new DynamicParameters();
                paramMedicine.Add("@p_Query", "SELECT ISNULL(SUM(Amount), 0) AS Total FROM FinalMedicineBill WHERE (Status='Paid' OR IsOutstanding=1) AND Deactivate=0" + dateCondition);
                var medicineData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramMedicine).FirstOrDefault();
                double medicineRevenue = medicineData != null && medicineData.Total != null ? Convert.ToDouble(medicineData.Total) : 0;
                ViewBag.MedicineRevenue = Convert.ToDouble(String.Format("{0:0.00}", medicineRevenue));

                // 5. Counter Medicine Revenue (FinalBillCounterMedicine)
                DynamicParameters paramCounterMed = new DynamicParameters();
                paramCounterMed.Add("@p_Query", "SELECT ISNULL(SUM(Amount), 0) AS Total FROM FinalBillCounterMedicine WHERE (Status='Paid' OR IsOutstanding=1) AND Deactivate=0" + dateCondition);
                var counterMedData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramCounterMed).FirstOrDefault();
                double counterMedRevenue = counterMedData != null && counterMedData.Total != null ? Convert.ToDouble(counterMedData.Total) : 0;
                ViewBag.CounterMedRevenue = Convert.ToDouble(String.Format("{0:0.00}", counterMedRevenue));

                // 6. Total Revenue (Sum of all paid departments)
                double totalCombinedRevenue = consultationRevenue + appointmentRevenue + therapyRevenue + medicineRevenue + counterMedRevenue;
                ViewBag.TotalRevenue = Convert.ToDouble(String.Format("{0:0.00}", totalCombinedRevenue));


                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
           
        }

        #region CloseAppointmentStatus
        public ActionResult CloseAppointmentStatus(string AppointmentFid_Encrypted, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                Appointment appointment = new Appointment();

                // ✅ Fetch status from FinalBill table safely
                //DynamicParameters param2 = new DynamicParameters();
                //param2.Add("@p_Query", $@"SELECT Status, Fid FROM FinalBill WHERE AppointmentFid = {AppointmentFid}");
                //appointment = BVGWellnessCenterDapparORM.ReturnList<Appointment>("sp_ExecuteQuery", param2).FirstOrDefault();

                //if (appointment == null || string.IsNullOrEmpty(appointment.Status) || appointment.Status == "Paid")
                //{
                    // ✅ If paid or no record found, allow checkout
                    DynamicParameters paramcheckout = new DynamicParameters();
                    paramcheckout.Add("@p_Query", $"UPDATE PatientAppoinment SET Status='Check Out' WHERE AppointmentFid_Encrypted='{AppointmentFid_Encrypted}'");
                    var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", paramcheckout);

                    TempData["Message"] = "Appointment checked out successfully.";
                    TempData["Icon"] = "success";
                //}
                //else
                //{
                //    // ✅ If bill not paid
                //    TempData["Message"] = "Please pay the bill before checking out.";
                //    TempData["Icon"] = "warning";
                //}

                return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                // Log error if needed
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


                DynamicParameters paramGetId = new DynamicParameters();
                paramGetId.Add("@p_Query", "SELECT pa.AppointmentFid, um.IsPaymentMandatory FROM PatientAppoinment pa INNER JOIN UserMaster um ON pa.DoctorFid = um.UserId WHERE pa.AppointmentFid_Encrypted = '" + AppointmentFid_Encrypted + "' ");
                // paramGetId.Add("@p_Query", "SELECT AppointmentFid FROM PatientAppoinment WHERE AppointmentFid_Encrypted = '" + AppointmentFid_Encrypted + "' ");
                var appointmentFid = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramGetId).FirstOrDefault();
                ViewBag.FID = appointmentFid.AppointmentFid;
                var APPID = appointmentFid.AppointmentFid;

                // Doctor sathi setting ON aahe ki OFF te baghayche
                bool isMandatory = appointmentFid.IsPaymentMandatory ?? false;

                // 🔹 Step 2: Check Bill Logic
                // Navin Condition: Jar Status "Check In" asel AANI Doctor chi setting "True" asel, tarach bill check kara
                if (StatusType == "Check In" && isMandatory == true)
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
                        paramFBill.Add("@p_Query", $@"select Status from FinalBillAppointment WHERE AppointmentFid = '" + ViewBag.FID + "' ");
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
                return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region PatientAppointmentList for Doctor
        public ActionResult PatientAppointmentList(int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                int UserId = Convert.ToInt32(Session["UserID"]);
                if (AppointmentFid != null)
                {
                    DynamicParameters paramDetails = new DynamicParameters();
                    paramDetails.Add("@p_AppointmentFid", AppointmentFid);
                    var Result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramDetails).FirstOrDefault();
                    ViewBag.PatientDetails = Result;
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_AppointmentFid_Encrypted", null);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_patientappoinment", paramList).ToList();
                if (Session["UserRole"] != null && Session["UserRole"].ToString() == "IsDoctor")
                {

                    //var PAtientAppointments = result.Where(m => m.DoctorFid == UserId && Convert.ToDateTime(m.AppoinmentDate) > DateTime.Today).ToList();
                    int userId = Convert.ToInt32(Session["UserID"]);
                    var today = DateTime.Today; // Get today's date without time

                    var patientAppointments = result
                        .Where(m => Convert.ToInt32(m.DoctorFid) == userId &&
                                    Convert.ToDateTime(m.AppoinmentDate).Date == today)
                        .ToList();
                    ViewBag.AppoinmentList = patientAppointments;
                }
                else
                {
                    var PAtientAppointments = result.Where(m => m.AppoinmentDate > DateTime.Today).ToList();
                    ViewBag.AppoinmentList = PAtientAppointments;
                }
                return View();
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

                // 🔹 Step 1: Appointment date fetch kara
                DynamicParameters paramGet = new DynamicParameters();
                paramGet.Add("@p_Query", "SELECT AppoinmentDate FROM PatientAppoinment WHERE AppointmentFid_Encrypted = '" + AppointmentFid_Encrypted + "' AND Deactivate=0");

                var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramGet).FirstOrDefault();

                if (data != null)
                {
                    DateTime appointmentDate = Convert.ToDateTime(data.AppoinmentDate);

                    // 🔹 Step 2: Same day cancel allow nahi
                    if (appointmentDate.Date == DateTime.Now.Date)
                    {
                        TempData["Message"] = "Same day appointment cannot be cancelled.";
                        TempData["Icon"] = "warning";
                        return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
                    }

                    DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "update PatientAppoinment set Status='Cancelled' where AppointmentFid_Encrypted='" + AppointmentFid_Encrypted + "' and Deactivate=0");
                var CancelStatus = BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", param);
                if(CancelStatus!=null)
                {
                    TempData["Message"] = "Appointment Cancelled";
                    TempData["Icon"] = "success";
                }
                }
                else
                {
                    TempData["Message"] = "Appointment not found.";
                    TempData["Icon"] = "error";
                }
                return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


    }
}