using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Filters;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
   
    public class MobileAppEHRController : Controller
    {
        // स्पेलिंग मिस्टेक हँडल करण्यासाठी दोन्ही पॅरामीटर्स घेतले आहेत

        public ActionResult MobilePrescription(int? AppointmentID, int? AppoinmentID, string ClinicCode = null, string token = null)
        {
            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }
            // int targetPatientFid = 1;
            int appId = AppointmentID ?? AppoinmentID ?? 0;
            var model = new MobileEHRViewModel();
            if (appId == 0)
            {
                ViewBag.Error = "Please enter a valid Appointment ID!";
                return View();
            }

            try
            {
                // १. मूळ क्वेरीवरून PatientFid मिळवा
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", $@"SELECT PA.*, P.*, CONCAT(P.FirstName, ' ', P.LName) AS PatientName, MC.*, CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName FROM PatientAppoinment PA INNER JOIN Patient P ON PA.PatientFid = P.PatientFid INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid INNER JOIN UserMaster UM on UM.UserId = PA.DoctorFid WHERE PA.AppointmentFid= {appId}");

                var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();

                if (TodayPatients == null)
                {
                    ViewBag.Error = $"No data found for Appointment ID {appId}!";
                    return View();
                }
                if (TodayPatients != null)
                {
                    // २. तुमच्या जुन्या कोडमधील एकदम परफेक्ट चालणारी SP
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", TodayPatients.PatientFid);
                    param.Add("@AppointmentFid", appId);
                    var patientDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = patientDetails;
                    ViewBag.AppointmentFid = appId;
                    ViewBag.ApptDate = TodayPatients.AppoinmentDate != null ? Convert.ToDateTime(TodayPatients.AppoinmentDate).ToString("dd/MM/yyyy") : "N/A";

                    // ३. Next Follow up
                    DynamicParameters paramFollowUp = new DynamicParameters();
                    paramFollowUp.Add("@p_Query", $@"SELECT TOP 1 FORMAT(FollowUpDate, 'dd/MM/yyyy') as FollowUpDate FROM CasePaper_Result WHERE PatientFid = {TodayPatients.PatientFid} ORDER BY CasePaperId DESC");
                    var followUpResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFollowUp).FirstOrDefault();
                    ViewBag.NextAppointmentDate = followUpResult != null ? followUpResult.FollowUpDate : "N/A";
                }

                // ४. Dispensed Prescription (Ayurveda + Homeopathy)
                DynamicParameters Dispensing = new DynamicParameters();
                // पॉईंट ४ मधील Dispensing क्वेरी अशी अपडेट करा:
                // पॉईंट ४ मधील Dispensing क्वेरी अशी अपडेट करा:
                Dispensing.Add("@p_Query", $@"

                    SELECT C.Fid, C.Fdate, C.AppointmentFid, C.IsConfirm, 'Ayurveda' AS Department, 

                    ISNULL(D.MedicineDescription, 'N/A') AS MedicineDescription, 

                    ISNULL(D.MorningSuchana, 'No Suchana') AS MorningSuchana, D.PathyaId, D.PathyaInstruction, PM.PathyaName

                    FROM Tbl_ConfirmMedicine C

                    INNER JOIN Save_MedicineDispensing D ON C.AppointmentFid = D.AppointmentFid AND C.PrescriptionID = D.PrescriptionID

                    LEFT JOIN tbl_PathyaMaster PM ON D.PathyaId = PM.Id

                    WHERE C.AppointmentFid = '{appId}' AND D.IsActive = 1 AND C.Deleted = 0

                    

                    UNION ALL

                    

                    SELECT C.Fid, C.Fdate, C.AppointmentFid, C.IsConfirm, 'Homeopathy' AS Department, 

                    ISNULL(DH.MedicineDescription, 'N/A') AS MedicineDescription, 

                    ISNULL(DH.MorningSuchana, 'No Suchana') AS MorningSuchana, DH.PathyaId, DH.PathyaInstruction, PM.PathyaName 

                    FROM Tbl_ConfirmMedicine C

                    INNER JOIN Save_MedicineDispensing_Homeopathy DH ON C.AppointmentFid = DH.AppointmentFid AND C.PrescriptionID = DH.PrescriptionID

                    LEFT JOIN tbl_PathyaMaster PM ON DH.PathyaId = PM.Id

                    WHERE C.AppointmentFid = '{appId}' AND DH.IsActive = 1 AND C.Deleted = 0");
                ViewBag.ResultView = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Dispensing);



                // कंट्रोलरमध्ये ही ओळ अशी अपडेट करा:
                // तिन्ही टेबल्स एकत्र फेच करा (Ayurveda + Yoga + Homeopathy)
                var AdvisedTherapy = new DynamicParameters();
                AdvisedTherapy.Add("@p_Query", $@"
    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Ayurveda' AS DeptType
    FROM Tbl_AdvisedTherapy AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = {appId}

    UNION ALL

    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Yoga' AS DeptType
    FROM AdvisedTherapyYoga AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = {appId}

    UNION ALL

    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Homeopathy' AS DeptType
    FROM Tbl_AdvisedTherapy_Homeopathy AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = {appId} AND AT.IsActive = 1");

                ViewBag.AdvisedTherapyResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", AdvisedTherapy).ToList();
                // ५. Counter Products
                DynamicParameters MEDICINEVIE = new DynamicParameters();
                MEDICINEVIE.Add("@p_Query", $@"
                    SELECT CounterProductID, AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Ayurveda' AS Department
                    FROM Save_CounterMedicineResult WHERE AppointmentFid = {appId} AND IsActive = 1
                    UNION ALL
                    SELECT CounterProductID, AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Homeopathy' AS Department
                    FROM Save_CounterMedicineResult_Homeopathy WHERE AppointmentFid = {appId} AND IsActive = 1");

                ViewBag.CounterResultView = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", MEDICINEVIE);
                //ViewBag.Error = "Appointment ID " + appId + " साठी डेटा सापडला नाही!";


            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }

        public ActionResult CounterProductDetails(int? AppointmentID, int? AppoinmentID, string ClinicCode = null, string token = null)
        {
            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }

            // ... (तुमचा बाकीचा जुना कोड तसाच ठेवा)

            // १. दोन्हीपैकी एक ID मिळवा (appId मध्ये स्टोर करा)
            // १. दोन्हीपैकी एक ID मिळवा
            int appId = AppointmentID ?? AppoinmentID ?? 0;
            if (appId == 0)
            {
                ViewBag.Error = "Please enter a valid Appointment ID!";
                return View();
            }
            var model = new MobileEHRViewModel();

            try
            {
                // १. मूळ क्वेरीवरून PatientFid आणि माहिती मिळवा (जेणेकरून हेडर डेटा दिसेल)
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", $@"SELECT PA.*, P.*, CONCAT(P.FirstName, ' ', P.LName) AS PatientName, MC.*, CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName FROM PatientAppoinment PA INNER JOIN Patient P ON PA.PatientFid = P.PatientFid INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid INNER JOIN UserMaster UM on UM.UserId = PA.DoctorFid WHERE PA.AppointmentFid= {appId}");

                var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();

                if (TodayPatients == null)
                {
                    ViewBag.Error = $"No data found for Appointment ID {appId}!";
                    return View();
                }

                if (TodayPatients != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", TodayPatients.PatientFid);
                    param.Add("@AppointmentFid", appId);

                    // पेशंटची माहिती
                    var patientDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();
                    ViewBag.GetPatientEncounterDetails = patientDetails;

                    ViewBag.AppointmentFid = appId;
                    ViewBag.ApptDate = TodayPatients.AppoinmentDate != null ? Convert.ToDateTime(TodayPatients.AppoinmentDate).ToString("dd/MM/yyyy") : "N/A";

                    // Next Follow up
                    DynamicParameters paramFollowUp = new DynamicParameters();
                    paramFollowUp.Add("@p_Query", $@"SELECT TOP 1 FORMAT(FollowUpDate, 'dd/MM/yyyy') as FollowUpDate FROM CasePaper_Result WHERE PatientFid = {TodayPatients.PatientFid} ORDER BY CasePaperId DESC");
                    var followUpResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFollowUp).FirstOrDefault();
                    ViewBag.NextAppointmentDate = followUpResult != null ? followUpResult.FollowUpDate : "N/A";
                }

                // २. Counter Products डेटा (तुझ्या लॉजिकप्रमाणे)
                DynamicParameters MEDICINEVIE = new DynamicParameters();
                MEDICINEVIE.Add("@p_Query", $@"
            SELECT CounterProductID, AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Ayurveda' AS Department
            FROM Save_CounterMedicineResult WHERE AppointmentFid = {appId} AND IsActive = 1
            UNION ALL
            SELECT CounterProductID, AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Homeopathy' AS Department
            FROM Save_CounterMedicineResult_Homeopathy WHERE AppointmentFid = {appId} AND IsActive = 1");

                ViewBag.CounterResultView = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", MEDICINEVIE);
                ViewBag.ApptId = appId;

                // डेटा सापडला नाही तर मेसेज
                //if (ViewBag.CounterResultView == null || !((IEnumerable<dynamic>)ViewBag.CounterResultView).Any())
                //{
                //    ViewBag.Error = "Appointment ID " + appId + " साठी काउंटर प्रॉडक्ट्स सापडले नाहीत!";
                //}

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
                return View();
            }
        }


        public ActionResult MobileBill(int? AppointmentID, int? AppoinmentID, string ClinicCode = null, string token = null)
        {
            int appId = AppointmentID ?? AppoinmentID ?? 0;
            if (appId == 0)
            {
                ViewBag.Error = "SearchForm"; // रिकाम्या आयडीसाठी सर्च बॉक्स उघडेल
                return View();
            }

            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }

            try
            {
                // १. पेशंट, डॉक्टर आणि क्लिनिक माहिती
                DynamicParameters param = new DynamicParameters();
                param.Add("@AppointmentFid", appId);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetails", param).FirstOrDefault();
                ViewBag.GetPatientEncounterDetails = result;

                if (result == null)
                {
                    // जर कनेक्शन झाले पण डेटा मिळाला नाही, तर हे दाखवेल
                    return Content($"Error: No bill data found for Appointment ID {appId} in this clinic's database!");
                }

                // २. बिलातील आयटम्सची लिस्ट (Services / Medicines)
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", appId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;

                // ३. फायनल बिल समरी (Total, Discount, Payable)
                DynamicParameters paramFinal = new DynamicParameters();
                paramFinal.Add("@p_Query", "SELECT FDate, TotalAmount, DiscountAmt, PayableAmount, Status FROM FinalBillAppointment WHERE Deactivate=0 AND AppointmentFid='" + appId + "'");
                var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                ViewBag.FinalBill = FinalBill;

                // ४. फायनल बिल पेमेंट ट्रांझॅक्शन्स
                DynamicParameters paramFinalbill = new DynamicParameters();
                paramFinalbill.Add("@p_Query", "SELECT FDate, Status, Amount, PaymentReferenceNumber, PaymentMode FROM FinalBillAppointment WHERE Deactivate=0 AND AppointmentFid='" + appId + "'");
                var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                ViewBag.GetFinalbillDetails = GetFinalbillDetails;

                // ५. आउटस्टँडिंग (Paid आणि Remaining Amount)
                DynamicParameters Outstanding = new DynamicParameters();
                Outstanding.Add("@p_Query", "SELECT AppointmentFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalBillAppointment WHERE AppointmentFid ='" + appId + "' GROUP BY AppointmentFid ");
                var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                ViewBag.GetOutstandingDetails = GetOutstandingDetails;
                ViewBag.AppointmentFid = appId;
            }
            catch (Exception ex)
            {
                return Content("System Error: " + ex.Message);
            }

            return View();
        }

        public ActionResult MobileDispensingBill(int? AppointmentID, int? AppoinmentID, string ClinicCode = null, string token = null)
        {
            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }

            // ... (तुमचा बाकीचा जुना कोड तसाच ठेवा)

            int appId = AppointmentID ?? AppoinmentID ?? 0;
            if (appId == 0)
            {
                ViewBag.Error = "SearchForm";
                return View();
            }
            try
            {
                // १. पेशंट, डॉक्टर आणि क्लिनिक माहिती
                DynamicParameters param = new DynamicParameters();
                param.Add("@AppointmentFid", appId);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetails", param).FirstOrDefault();
                ViewBag.GetPatientEncounterDetails = result;
                if (result == null)
                {
                    ViewBag.Error = $"No dispensing bill data found for Appointment ID {appId}!";
                    return View();
                }

                // २. बिलातील आयटम्सची लिस्ट (Services / Medicines)
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", appId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;

                // ३. फायनल बिल समरी (Total, Discount, Payable)
                DynamicParameters paramFinal = new DynamicParameters();
                paramFinal.Add("@p_Query", "SELECT FDate, TotalAmount, DiscountAmt, PayableAmount, Status FROM FinalBill WHERE Deactivate=0 AND AppointmentFid='" + appId + "'");
                var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                ViewBag.FinalBill = FinalBill;

                // ४. फायनल बिल पेमेंट ट्रांझॅक्शन्स
                DynamicParameters paramFinalbill = new DynamicParameters();
                paramFinalbill.Add("@p_Query", "SELECT FDate, Status, Amount, PaymentReferenceNumber, PaymentMode FROM FinalBill WHERE Deactivate=0 AND AppointmentFid='" + appId + "'");
                var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                ViewBag.GetFinalbillDetails = GetFinalbillDetails;

                // ५. आउटस्टँडिंग (Paid आणि Remaining Amount)
                DynamicParameters Outstanding = new DynamicParameters();
                Outstanding.Add("@p_Query", "SELECT AppointmentFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalBill WHERE AppointmentFid ='" + appId + "' GROUP BY AppointmentFid ");
                var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                ViewBag.AppointmentFid = appId;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "System Error: " + ex.Message;
            }
            return View();
        }

        public ActionResult MobileCounterBill(int? AppointmentID, int? AppoinmentID, string ClinicCode = null, string token = null)
        {
            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }

            // ... (तुमचा बाकीचा जुना कोड तसाच ठेवा)

            int appId = AppointmentID ?? AppoinmentID ?? 0;
            if (appId == 0)
            {
                ViewBag.Error = "SearchForm";
                return View();
            }
            try
            {
                // १. मूळ क्वेरीवरून PatientFid मिळवा
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", $"SELECT PatientFid FROM PatientAppoinment WHERE AppointmentFid = {appId}");
                var apptObj = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param1).FirstOrDefault();
                if (apptObj == null)
                {
                    ViewBag.Error = $"No details found for Appointment ID {appId}!";
                    return View();
                }
                // PatientFid सुरक्षितपणे काढणे
                // PatientFid सुरक्षितपणे काढणे
                int patientFid = 0;
                var dict = apptObj as IDictionary<string, object>;
                if (dict != null)
                {
                    patientFid = Convert.ToInt32(dict["PatientFid"]);
                }
                else
                {
                    patientFid = Convert.ToInt32(apptObj.PatientFid);
                }

                // २. पेशंट, डॉक्टर आणि क्लिनिक माहिती
                DynamicParameters param = new DynamicParameters();
                param.Add("@PatientFid", patientFid);
                param.Add("@AppointmentFid", appId);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();
                ViewBag.GetPatientEncounterDetails = result;
                if (result == null)
                {
                    ViewBag.Error = $"No patient details found for Appointment ID {appId}!";
                    return View();
                }
                // ३. बिलातील औषधांची यादी (Billed Medicine Items)
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_PatientID", patientFid);
                paramBill.Add("@p_AppointmentFid", appId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;
                // ४. फायनल बिल समरी (Total, Discount, Payable)
                DynamicParameters paramFinal = new DynamicParameters();
                paramFinal.Add("@p_Query", $"SELECT FDate, TotalAmount, DiscountAmt, PayableAmount, Status FROM FinalMedicineBill WHERE Deactivate=0 AND PatientFid='{patientFid}' AND AppointmentFid='{appId}'");
                var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                ViewBag.FinalBill = FinalBill;
                // ५. फायनल बिल पेमेंट ट्रांझॅक्शन्स
                DynamicParameters paramFinalbill = new DynamicParameters();
                paramFinalbill.Add("@p_Query", $"SELECT FDate, Status, Amount, PaymentReferenceNumber, PaymentMode FROM FinalMedicineBill WHERE Deactivate=0 AND PatientFid='{patientFid}' AND AppointmentFid='{appId}'");
                var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                // ६. आउटस्टँडिंग (Paid आणि Remaining Amount)
                DynamicParameters Outstanding = new DynamicParameters();
                Outstanding.Add("@p_Query", $"SELECT PatientFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalMedicineBill WHERE PatientFid ='{patientFid}' AND AppointmentFid='{appId}' GROUP BY PatientFid, AppointmentFid");
                var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                ViewBag.GetOutstandingDetails = GetOutstandingDetails;
                ViewBag.AppointmentFid = appId;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "System Error: " + ex.Message;
            }
            return View();
        }

        public ActionResult MobileServicesBill(string FlagId, int? AppointmentID, int? AppoinmentID, int? PatientFid, int? PatientID, string ClinicCode = null, string token = null)
        {
            if (!string.IsNullOrEmpty(ClinicCode) && !string.IsNullOrEmpty(token))
            {
                string errorMsg = "";
                if (!InitializeMobileSession(ClinicCode, token, out errorMsg))
                {
                    // सेशन इनिशियलायझेशन फेल झाल्यास एरर स्क्रीनवर दाखवा
                    return Content("Authorization / Connection Error: " + errorMsg);
                }
            }

            // ... (तुमचा बाकीचा जुना कोड तसाच ठेवा)

            int appId = AppointmentID ?? AppoinmentID ?? 0;
            int patId = PatientFid ?? PatientID ?? 0;

            // १. जर URL मध्ये PatientFid किंवा PatientID दिला असेल, तर थेट बिलांची यादी दाखवा
            if (string.IsNullOrEmpty(FlagId) && patId > 0)
            {
                try
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Query", @"
                SELECT DISTINCT 
                    FSB.BillFlageID,
                    MAX(FSB.FDate) AS BillDate,
                    MAX(FSB.TotalAmount) AS TotalAmount,
                    MAX(FSB.PayableAmount) AS PayableAmount,
                    MAX(FSB.Status) AS Status,
                    (SELECT TOP 1 S.ServiceName FROM MAS_Service S INNER JOIN GenerateBillServices G ON S.ServiceFid = G.ServiceFid WHERE G.FlagId = FSB.BillFlageID) AS ServiceName
                FROM FinalservicesBill FSB
                INNER JOIN GenerateBillServices GBS ON FSB.BillFlageID = GBS.FlagId
                WHERE GBS.PatientID = " + patId + @" AND GBS.Deactivate = 0 AND FSB.Deactivate = 0
                GROUP BY FSB.BillFlageID
                ORDER BY BillDate DESC");

                    var billsList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramList).ToList();

                    if (billsList != null && billsList.Any())
                    {
                        // मल्टिपल आणि सिंगल दोन्ही केसमध्ये थेट लिस्ट दाखवा
                        var formattedBills = new List<Dictionary<string, object>>();
                        foreach (var item in billsList)
                        {
                            var dict = item as IDictionary<string, object>;
                            if (dict != null)
                            {
                                formattedBills.Add(dict.ToDictionary(k => k.Key, v => v.Value));
                            }
                        }
                        ViewBag.PatientBillsList = formattedBills;
                        ViewBag.PatientID = patId;

                        // पेशंटचे नाव मिळवण्यासाठी
                        DynamicParameters paramPatientName = new DynamicParameters();
                        paramPatientName.Add("@p_Query", "SELECT TOP 1 CONCAT(FirstName, ' ', LName) AS PatientName FROM Patient WHERE PatientFid = " + patId);
                        var patObj = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramPatientName).FirstOrDefault();
                        if (patObj != null)
                        {
                            var dictPat = patObj as IDictionary<string, object>;
                            ViewBag.PatientName = dictPat != null ? Convert.ToString(dictPat["PatientName"]) : Convert.ToString(patObj.PatientName);
                        }
                        else
                        {
                            ViewBag.PatientName = "Patient #" + patId;
                        }

                        return View();
                    }
                    else
                    {
                        ViewBag.Error = "No therapy bills found for Patient ID " + patId + "!";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Patient Search Error: " + ex.Message;
                    return View();
                }
            }

            // २. जर केवळ AppointmentID दिला असेल, तर फॉलबॅकद्वारे बिलाचा FlagId मिळवणे
            if (string.IsNullOrEmpty(FlagId) && appId > 0)
            {
                try
                {
                    // २.१. प्रथम थेट FinalservicesBill मध्ये मॅच करण्याचा प्रयत्न करा
                    DynamicParameters paramDirect = new DynamicParameters();
                    paramDirect.Add("@p_Query", "SELECT TOP 1 BillFlageID FROM FinalservicesBill WHERE Deactivate = 0 AND AppointmentFid = " + appId + " ORDER BY Fid DESC");
                    var directBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramDirect).FirstOrDefault();

                    if (directBill != null)
                    {
                        var dict = directBill as IDictionary<string, object>;
                        FlagId = dict != null ? Convert.ToString(dict["BillFlageID"]) : Convert.ToString(directBill.BillFlageID);
                    }

                    // २.२. Fallback: जर थेट भेटला नाही, तर पेशंटच्या सर्वात जवळच्या तारखेचे बुकिंग शोधणे
                    if (string.IsNullOrEmpty(FlagId))
                    {
                        DynamicParameters paramAppt = new DynamicParameters();
                        paramAppt.Add("@p_Query", "SELECT PatientFid, AppoinmentDate FROM PatientAppoinment WHERE AppointmentFid = " + appId);
                        var appt = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppt).FirstOrDefault();

                        if (appt != null)
                        {
                            var dictAppt = appt as IDictionary<string, object>;
                            int patientFid = dictAppt != null ? Convert.ToInt32(dictAppt["PatientFid"]) : Convert.ToInt32(appt.PatientFid);
                            DateTime apptDate = dictAppt != null ? Convert.ToDateTime(dictAppt["AppoinmentDate"]) : Convert.ToDateTime(appt.AppoinmentDate);

                            DynamicParameters paramBooking = new DynamicParameters();
                            paramBooking.Add("@p_Query", "SELECT TOP 1 FlagId FROM MAS_TherapyBooking WHERE PatientFid = " + patientFid + " AND IsActive = 1 ORDER BY ABS(DATEDIFF(day, FromDate, '" + apptDate.ToString("yyyy-MM-dd HH:mm:ss") + "')) ASC");
                            var booking = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramBooking).FirstOrDefault();

                            if (booking != null)
                            {
                                var dictBook = booking as IDictionary<string, object>;
                                FlagId = dictBook != null ? Convert.ToString(dictBook["FlagId"]) : Convert.ToString(booking.FlagId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Search Error: " + ex.Message;
                    return View();
                }
            }

            // ३. जर कोणताच आयडी मिळाला नाही तर एरर दाखवा
            if (string.IsNullOrEmpty(FlagId))
            {
                if (patId > 0)
                {
                    ViewBag.Error = "No active therapy booking found for Patient ID " + patId + "!";
                }
                else if (appId > 0)
                {
                    ViewBag.Error = "No therapy bill found associated with Appointment ID " + appId + "!";
                }
                else
                {
                    ViewBag.Error = "SearchForm";
                }
                return View();
            }

            try
            {
                // १. पेशंट आणि बिल संदर्भ माहिती
                DynamicParameters param = new DynamicParameters();
                param.Add("@FlagId", FlagId);
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_ServicesPaymentDetails", param).FirstOrDefault();
                ViewBag.GetPatientEncounterDetails = result;
                if (result == null)
                {
                    ViewBag.Error = "No therapy bill data found for Bill ID " + FlagId + "!";
                    return View();
                }

                // २. बिलातील थेरपी/सेवांची यादी
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@FlagId", FlagId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;

                // ३. फायनल बिल समरी (Total, Discount, Payable)
                DynamicParameters paramFinal = new DynamicParameters();
                paramFinal.Add("@p_Query", "SELECT TOP 1 FDate, TotalAmount, DiscountAmt, PayableAmount, Status, Amount FROM FinalservicesBill WHERE Deactivate = 0 AND BillFlageID = '" + FlagId + "' ORDER BY Fid DESC");
                var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                ViewBag.FinalBill = FinalBill;

                // ४. फायनल बिल पेमेंट ट्रांझॅक्शन्स
                DynamicParameters paramFinalbill = new DynamicParameters();
                paramFinalbill.Add("@p_Query", "SELECT FDate, Amount, PaymentReferenceNumber, PaymentMode FROM FinalservicesBill WHERE Deactivate=0 AND BillFlageID='" + FlagId + "'");
                var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                ViewBag.GetFinalbillDetails = GetFinalbillDetails;

                // ५. आउटस्टँडिंग (Paid आणि Remaining Amount)
                DynamicParameters Outstanding = new DynamicParameters();
                Outstanding.Add("@p_Query", "SELECT BillFlageID, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalservicesBill WHERE BillFlageID ='" + FlagId + "' GROUP BY BillFlageID");
                var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                ViewBag.FlagId = FlagId;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "System Error: " + ex.Message;
            }
            return View();
        }

        private bool InitializeMobileSession(string clinicCode, string token, out string errorMessage)
        {
            errorMessage = "";

            // १. टोकन चेक करा
            string expectedToken = "BVGWellnessMobileApp2026";
            if (string.IsNullOrEmpty(token) || token != expectedToken)
            {
                errorMessage = "Invalid Access Token!";
                return false;
            }

            if (string.IsNullOrEmpty(clinicCode))
            {
                errorMessage = "Clinic Code is empty!";
                return false;
            }

            try
            {
                // २. थेट SQL Connection वापरून central registry मधून कनेक्शन डिटेल्स मिळवा
                using (SqlConnection conn = new SqlConnection(BVGWellnessCenterDapparORM.connectionStrings))
                {
                    conn.Open();
                    string query = @"SELECT Con_Server, Con_UserId, Con_Password, Con_Database, Con_hospitalCode
                             FROM HospitalRegistration
                             WHERE Con_hospitalCode = @ClinicCode
                               AND Deactivate = 0
                               AND Isactive = 1";

                    var connectionDetails = conn.Query<dynamic>(query, new { ClinicCode = clinicCode.Trim() }).FirstOrDefault();

                    if (connectionDetails != null)
                    {
                        var dict = connectionDetails as IDictionary<string, object>;
                        string server = dict != null ? Convert.ToString(dict["Con_Server"]) : Convert.ToString(connectionDetails.Con_Server);
                        string userId = dict != null ? Convert.ToString(dict["Con_UserId"]) : Convert.ToString(connectionDetails.Con_UserId);
                        string password = dict != null ? Convert.ToString(dict["Con_Password"]) : Convert.ToString(connectionDetails.Con_Password);
                        string database = dict != null ? Convert.ToString(dict["Con_Database"]) : Convert.ToString(connectionDetails.Con_Database);

                        // ३. सेशनमध्ये कनेक्शन सेट करा
                        BVGWellnessCenterDapparORM.SetConnection(server, userId, password, database);

                        // ४. तात्पुरते सेशन्स ॲड करा
                        if (Session["UserID"] == null)
                        {
                            Session["UserID"] = 0;
                            Session["UserRole"] = "MobileApp";
                        }
                        return true;
                    }
                    else
                    {
                        errorMessage = $"Clinic Code '{clinicCode}' not found in registry database!";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Database Connection Error: " + ex.Message;
                return false;
            }
        }




    }
}