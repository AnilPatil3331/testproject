using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class PrescriptionController : Controller
    {
        // GET: Patient/Prescription
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MedicinePrescription(int? AppoinmentID)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                //services
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Deactivate=0 and Servicecategory='Medicine'");
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.ProductList = GetService;


                if (AppoinmentID != null && AppoinmentID != 0)
                {
                    // 1. आयुर्वेद डेटा (Existing)
                    var presParam = new DynamicParameters();
                    presParam.Add("@AppoinmentFid", AppoinmentID);
                    ViewBag.PrescriptionList = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam);

                    // 🟢 2. होमियोपॅथी डेटा (New - हे महत्त्वाचे आहे)
                    // ही SP 'MedicineFormula_Homeopathy' टेबल मधून डेटा आणेल
                    ViewBag.HomeopathyPrescriptionList = BVGWellnessCenterDapparORM.ReturnList<HomopathyPrescriptionModel>("SP_GetPrescriptionByOPDNO_Homeopathy", presParam);
                }
                //if (OPDNO != null && OPDNO != 0)
                //{
                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@OPDNO", OPDNO);
                //    var result = BVGWellnessCenterDapparORM.ReturnList<OPDRegistration>("SP_Get_OPDDetails_ByOPDNO", param1).FirstOrDefault();
                //    ViewBag.OPDDetails = result;

                if (AppoinmentID != null && AppoinmentID != 0)

                {

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_Query", $@"SELECT 
    PA.*, 
    P.*, 
 CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
    MC.*,
 CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName	

FROM 
    PatientAppoinment PA
INNER JOIN 
    Patient P ON PA.PatientFid = P.PatientFid
INNER JOIN 
    Mas_Clinic MC ON PA.ClinicFid = MC.Fid
	INNER JOIN 
	UserMaster UM on UM.UserId = PA.DoctorFid
WHERE 
     PA.AppointmentFid= {AppoinmentID}
	");
                    var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                    // 2. तो डेटा 'null' नाहीये ना, हे चेक करा (इथे टाका तो कोड)
                    if (TodayPatients != null)
                    {
                        ViewBag.TodayPatients = TodayPatients;
                        ViewBag.AppointmentFid = TodayPatients.AppointmentFid;
                        ViewBag.PatientFid = TodayPatients.PatientFid;

                        // हे केल्यामुळे पेज रिफ्रेश झाल्यावर 'Patient Name', 'Doctor Name' 
                        // आणि 'Billing' ची माहिती रिकामी दिसणार नाही.
                    }
                    else
                    {
                        // जर डेटा मिळाला नाही, तर तुम्ही मेसेज सेट करू शकता
                        ViewBag.Message = "Patient details not found.";
                    }


                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", TodayPatients.PatientFid);
                    param.Add("@AppointmentFid", AppoinmentID);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;



                    //DynamicParameters presParam = new DynamicParameters();
                    //presParam.Add("@AppoinmentFid", AppoinmentID);
                    //var prescriptions = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam);

                    //ViewBag.PrescriptionList = prescriptions;


                    DynamicParameters presRParam = new DynamicParameters();
                    presRParam.Add("@AppoinmentFid", AppoinmentID);
                    var prescriptionsResult = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionRESULTByOPDNO", presRParam);
                    ViewBag.MedicineInstructions = prescriptionsResult;



                    DynamicParameters checkbox = new DynamicParameters();
                    checkbox.Add("@p_Query", $"select AppointmentFid, PatientFid,PrescriptionID,IsConfirm from Tbl_ConfirmMedicine where AppointmentFid = {AppoinmentID}");
                    var Selectedcheckbox = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", checkbox);
                    ViewBag.checkbox = Selectedcheckbox;


                    DynamicParameters CounterPCheckBox = new DynamicParameters();
                    CounterPCheckBox.Add("@p_Query", $"select AppointmentFid, PatientFid,PrescriptionID,IsConfirm from Tbl_ConfirmMedicine where AppointmentFid = {AppoinmentID}");
                    var CounterCheckBOX = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", CounterPCheckBox);
                    ViewBag.checkboxCounter = CounterCheckBOX;




                    //                    DynamicParameters Dispensing = new DynamicParameters();

                    //                    Dispensing.Add("@p_Query", $@"SELECT 
                    //    C.Fid, 
                    //    C.Fdate, 
                    //    C.AppointmentFid, 
                    //    C.IsConfirm, 
                    //C.Department,
                    //    ISNULL(D.MedicineDescription, 'Medicine Name Not Found') AS MedicineDescription, 
                    //    ISNULL(D.MorningSuchana, 'No Suchana') AS MorningSuchana,
                    //D.PathyaId,
                    //        D.PathyaInstruction,
                    //        PM.PathyaName
                    //    FROM Tbl_ConfirmMedicine C
                    //    INNER JOIN Save_MedicineDispensing D 
                    //        ON C.AppointmentFid = D.AppointmentFid 
                    //AND C.PrescriptionID = D.PrescriptionID
                    //LEFT JOIN tbl_PathyaMaster PM 
                    //        ON D.PathyaId = PM.Id
                    //    WHERE C.AppointmentFid = '{AppoinmentID}' 
                    // AND D.IsActive = 1
                    //    AND C.Deleted = 0");
                    //                    var MEDICINEVIEW = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Dispensing);
                    //                    ViewBag.ResultView = MEDICINEVIEW;
                    //                    if (MEDICINEVIEW.Count() == 0)
                    //                    {
                    //                        ViewBag.ResultView = null;
                    //                    }

                    DynamicParameters Dispensing = new DynamicParameters();
                    Dispensing.Add("@p_Query", $@"
    SELECT 
        C.Fid, 
        C.Fdate, 
        C.AppointmentFid, 
        C.IsConfirm, 
        'Ayurveda' AS Department, 
        ISNULL(D.MedicineDescription, 'N/A') AS MedicineDescription, 
        ISNULL(D.MorningSuchana, 'No Suchana') AS MorningSuchana,
        0 AS PathyaId,
        (SELECT STRING_AGG(CAST(PM.PathyaInstruction AS NVARCHAR(MAX)), CHAR(10)) FROM Save_AppointmentPathya SP JOIN tbl_PathyaMaster PM ON SP.PathyaId = PM.Id WHERE SP.AppointmentFid = C.AppointmentFid) AS PathyaInstruction,
        (SELECT STRING_AGG(CAST(PM.PathyaName AS NVARCHAR(MAX)), ', ') FROM Save_AppointmentPathya SP JOIN tbl_PathyaMaster PM ON SP.PathyaId = PM.Id WHERE SP.AppointmentFid = C.AppointmentFid) AS PathyaName
    FROM Tbl_ConfirmMedicine C
    INNER JOIN Save_MedicineDispensing D ON C.AppointmentFid = D.AppointmentFid AND C.PrescriptionID = D.PrescriptionID
    WHERE C.AppointmentFid = '{AppoinmentID}' AND D.IsActive = 1 AND C.Deleted = 0

    UNION ALL

    SELECT 
        C.Fid, 
        C.Fdate, 
        C.AppointmentFid, 
        C.IsConfirm, 
        'Homeopathy' AS Department, 
        ISNULL(DH.MedicineDescription, 'N/A') AS MedicineDescription, 
        ISNULL(DH.MorningSuchana, 'No Suchana') AS MorningSuchana,
        0 AS PathyaId,
        (SELECT STRING_AGG(CAST(PM.PathyaInstruction AS NVARCHAR(MAX)), CHAR(10)) FROM Save_AppointmentPathya_Homeopathy SPH JOIN tbl_PathyaMaster PM ON SPH.PathyaId = PM.Id WHERE SPH.AppointmentFid = C.AppointmentFid) AS PathyaInstruction,
        (SELECT STRING_AGG(CAST(PM.PathyaName AS NVARCHAR(MAX)), ', ') FROM Save_AppointmentPathya_Homeopathy SPH JOIN tbl_PathyaMaster PM ON SPH.PathyaId = PM.Id WHERE SPH.AppointmentFid = C.AppointmentFid) AS PathyaName
    FROM Tbl_ConfirmMedicine C
    INNER JOIN Save_MedicineDispensing_Homeopathy DH ON C.AppointmentFid = DH.AppointmentFid AND C.PrescriptionID = DH.PrescriptionID
    WHERE C.AppointmentFid = '{AppoinmentID}' AND DH.IsActive = 1 AND C.Deleted = 0");

                    var MEDICINEVIEW = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Dispensing);
                    ViewBag.ResultView = MEDICINEVIEW;
                    DynamicParameters Counter = new DynamicParameters();
                    Counter.Add("@AppoinmentFid", AppoinmentID);
                    var CounterMedicine = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetCounterMedicineFormula", Counter);
                    ViewBag.CounterMedicineList = CounterMedicine;

                    // 2. होमिओपॅथीसाठी (येथे दुरुस्ती केली आहे)
                    DynamicParameters CounterHomo = new DynamicParameters(); // नवीन ऑब्जेक्ट
                    CounterHomo.Add("@AppoinmentFid", AppoinmentID); // यात व्हॅल्यू ॲड करा
                    var CounterMedicineHomeo = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetCounterMedicineFormula_Homeopathy", CounterHomo);
                    ViewBag.CounterMedicineListHomeo = CounterMedicineHomeo;

                    //                DynamicParameters CounterDispensing = new DynamicParameters();
                    //                CounterDispensing.Add("@AppoinmentFid", AppoinmentID);

                    //                CounterDispensing.Add("@p_Query", $@"
                    //SELECT AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Ayurveda' AS Department 
                    //FROM Save_CounterMedicineResult WHERE AppointmentFid = @AppoinmentFid AND IsActive = 1
                    //UNION ALL
                    //SELECT AppointmentFid, PatientFid, MedicineDescription, MorningSuchana, 'Homeopathy' AS Department 
                    //FROM Save_CounterMedicineResult_Homeopathy WHERE AppointmentFid = @AppoinmentFid AND IsActive = 1");

                    //                var CounterResultData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", CounterDispensing);
                    //                ViewBag.CounterMedicineList = CounterResultData;

                    DynamicParameters MEDICINEVIE = new DynamicParameters();
                    MEDICINEVIE.Add("@p_Query", $@"
    SELECT 
        CounterProductID, 
        AppointmentFid, 
        PatientFid, 
        MedicineDescription, 
        MorningSuchana, 
        'Ayurveda' AS Department
    FROM Save_CounterMedicineResult 
    WHERE AppointmentFid = {AppoinmentID} AND IsActive = 1

    UNION ALL

    SELECT 
        CounterProductID, 
        AppointmentFid, 
        PatientFid, 
        MedicineDescription, 
        MorningSuchana, 
        'Homeopathy' AS Department
    FROM Save_CounterMedicineResult_Homeopathy 
    WHERE AppointmentFid = {AppoinmentID} AND IsActive = 1");
                    var MEDICINEVIEWw = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", MEDICINEVIE);
                    ViewBag.CounterResultView = MEDICINEVIEWw;
                    if (MEDICINEVIEWw.Count() == 0)
                    {
                        ViewBag.CounterResultView = null;
                    }

                    //var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                    //    ViewBag.TodayPatients = TodayPatients;
                    //    ViewBag.AppointmentFid = TodayPatients.AppointmentFid;

                    //    DynamicParameters presParam = new DynamicParameters();
                    //presParam.Add("@OPDNO", OPDNO);
                    //var prescriptions = DapperORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam).ToList();

                    //ViewBag.PrescriptionList = prescriptions;
                    DynamicParameters popupdorop = new DynamicParameters();
                    popupdorop.Add("@p_Query", $@"
    -- आयुर्वेदाची औषधे
    SELECT MM.MedicineName AS Name, MM.ServiceFid AS ID 
    FROM CounterMedicineFormula CM 
    LEFT JOIN Mas_Medicine MM ON CM.FormulaID = MM.ServiceFid 
    WHERE CM.AppointmentFid = '{AppoinmentID}' AND CM.IsActive = 1

    UNION

    -- होमिओपॅथीची औषधे
    SELECT MM.MedicineName AS Name, MM.ServiceFid AS ID 
    FROM CounterMedicineFormula_Homeopathy CMH 
    LEFT JOIN Mas_Medicine MM ON CMH.FormulaID = MM.ServiceFid 
    WHERE CMH.AppointmentFid = '{AppoinmentID}' AND CMH.IsActive = 1");

                    var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", popupdorop);
                    ViewBag.ProductList = data;

                    //DynamicParameters popupdorop = new DynamicParameters();
                    //popupdorop.Add("@p_Query", $"select MM.MedicineName as Name, MM.ServiceFid as ID from CounterMedicineFormula   CM left join  Mas_Medicine MM on CM.FormulaID = MM.ServiceFid where AppointmentFid ='{AppoinmentID}'  and CM.isActive = 1");/* where ServiceName='Health Check'*/
                    //var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", popupdorop);
                    //ViewBag.ProductList = data;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_PatientID", ViewBag.PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppoinmentID);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    // तिन्ही टेबल्स एकत्र फेच करा (Ayurveda + Yoga + Homeopathy)
                    var AdvisedTherapy = new DynamicParameters();
                    AdvisedTherapy.Add("@p_Query", $@"
    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Ayurveda' AS DeptType
    FROM Tbl_AdvisedTherapy AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = '{AppoinmentID}'

    UNION ALL

    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Yoga' AS DeptType
    FROM AdvisedTherapyYoga AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = '{AppoinmentID}'

    UNION ALL

    SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, 
           AT.InstructionPatinet, AT.InstructionTherapist, 'Homeopathy' AS DeptType
    FROM Tbl_AdvisedTherapy_Homeopathy AT 
    INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
    WHERE AT.AppointmentFid = '{AppoinmentID}' AND AT.IsActive = 1");

                    var AdvisedTherapyy = BVGWellnessCenterDapparORM.ReturnList<AdvisedTherapyVM>("sp_ExecuteQuery", AdvisedTherapy).ToList();
                    ViewBag.AdvisedTherapyResult = AdvisedTherapyy;

                    // FollowUpDate मिळवण्यासाठी क्वेरी
                    // PatientFid वरून सर्वात शेवटची FollowUpDate मिळवण्यासाठी
                    DynamicParameters paramFollowUp = new DynamicParameters();
                    paramFollowUp.Add("@p_Query", $@"
    SELECT TOP 1 FORMAT(FollowUpDate, 'dd/MM/yyyy') as FollowUpDate 
    FROM CasePaper_Result 
    WHERE PatientFid = {TodayPatients.PatientFid} 
    ORDER BY CasePaperId DESC");

                    var followUpResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFollowUp).FirstOrDefault();

                    ViewBag.NextAppointmentDate = followUpResult != null ? followUpResult.FollowUpDate : "N/A";
                }

                return View();

            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpGet]
        public JsonResult SearchOPD(string searchTerm)
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@searchTerm", searchTerm);
            //param.Add("@HospitalID", Session["HospitalID"]);
            var result = BVGWellnessCenterDapparORM.ReturnList<Appointment>("Sp_searchOPD", param).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveSelectedPrescriptions(int? AppointmentFid, int[] selectedIds, int? PatientFid, int[] selectedIdsHomeo)
        {
            try
            {
                // Prescription ची यादी DB मधून घ्या (सर्व)
                if (selectedIds != null && selectedIds.Length > 0)
                {
                    foreach ( var id in selectedIds )
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@PrescriptionID", id);
                        param.Add("@AppointmentFid", AppointmentFid);
                        param.Add("@PatientFid", PatientFid);
                        param.Add("@CreatedBy", Session["LoginUserID"]);
                        param.Add("@IsConfirm", 1);  // कारण selected आहेत, confirm = 
                        param.Add("@Department", "Ayurveda");

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_Tbl_ConfirmMedicinePrescription", param);
                    }

                    TempData["Success"] = "Selected prescriptions confirmed successfully!";
                }

                if (selectedIdsHomeo != null)
                {
                    foreach (var id in selectedIdsHomeo)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@PrescriptionID", id);
                        param.Add("@AppointmentFid", AppointmentFid);
                        param.Add("@PatientFid", PatientFid);
                        param.Add("@CreatedBy", Session["LoginUserID"]);
                        param.Add("@IsConfirm", 1);  // कारण selected आहेत, confirm = 
                        param.Add("@Department", "Homeopathy");

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_Tbl_ConfirmMedicinePrescription", param);
                    }

                    TempData["Success"] = "Selected prescriptions confirmed successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("MedicinePrescription", "Prescription", new { AppoinmentID = AppointmentFid });
        }



        [HttpPost]
        public ActionResult SaveSelectedCounterProduct(int? AppointmentFid, int[] selectedIds, int? PatientFid, int[] selectedIdsHomeo)
        {
            try
            {
                // Prescription ची यादी DB मधून घ्या (सर्व)
                if (selectedIds != null && selectedIds.Length > 0)
                {
                    foreach (var id in selectedIds)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@PrescriptionID", id);
                        param.Add("@AppointmentFid", AppointmentFid);
                        param.Add("@PatientFid", PatientFid);
                        param.Add("@CreatedBy", Session["LoginUserID"]);
                        param.Add("@IsConfirm", 1);  // कारण selected आहेत, confirm = true

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_Tbl_ConfirmCounterProduct", param);
                    }

                    TempData["Success"] = "Selected prescriptions confirmed successfully!";
                    TempData["OpenBillPopup"] = true;
                }

                if (selectedIdsHomeo != null && selectedIdsHomeo.Length > 0)
                {
                    foreach (var id in selectedIdsHomeo)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@PrescriptionID", id);
                        param.Add("@AppointmentFid", AppointmentFid);
                        param.Add("@PatientFid", PatientFid);
                        param.Add("@CreatedBy", Session["LoginUserID"]);
                        param.Add("@IsConfirm", 1);  // कारण selected आहेत, confirm = true

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_Tbl_ConfirmCounterProduct", param);
                    }

                    TempData["Success"] = "Selected prescriptions confirmed successfully!";
                    TempData["OpenBillPopup"] = true;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("MedicinePrescription", "Prescription", new { AppoinmentID = AppointmentFid });
        }




        public ActionResult GetMedicine(string check  , int? appointmentFid/*string FromPrescriptioncheck, int? EncounterFid*/)
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
                    paramService.Add("@p_Query", "select ServiceFid as id, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0 ");/* where ServiceName='Health Check'*/
                    var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                    ViewBag.Getdata = data;
                }
                //else if (FromPrescriptioncheck == "Checked")
                //{
                //    DynamicParameters paramFromPres = new DynamicParameters();
                //    paramFromPres.Add("@p_EncounterFid", EncounterFid);
                //    var Result = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_List_GetMedicineFromPrescription", paramFromPres);
                //    ViewBag.Getdata = Result;
                //}
                //else
                //{
                //    DynamicParameters paramService = new DynamicParameters();
                //    paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Servicecategory='Medicine'");
                //    var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                //    ViewBag.Getdata = data;
                //}

                return Json(new { GetData = ViewBag.Getdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        public ActionResult GetMedicinePrice(int GetServiceId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramprice = new DynamicParameters();
                paramprice.Add("@p_Query", "select Charges  from Mas_Medicine where Deactivate=0 and ServiceFid='" + GetServiceId + "'");
                var GetServicePrice = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramprice).FirstOrDefault();

                return Json(GetServicePrice.Charges, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult SaveGenerateBill(Billing Bill, int? PatientID)
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
                param.Add("@p_PatientID", Bill.PatientID);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillCounterMedicinel", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.PatientID != 0 && Bill.PatientID != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_PatientID", Bill.PatientID);
                    paramBill.Add("@p_AppointmentFid", Bill.AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                return Json(new { GetBillList = ViewBag.GenerateBill }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult SaveAndCloseEncounter(List<ClsPrescription> paymentList, string ISOutStandingAmt)
        {
            try
            {
                var appointmentId = paymentList.First().AppointmentFid;
                var PatientID = paymentList.First().PatientFid;

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
                    check.Add("@p_Query", "select FlageID from FinalMedicineBill where Deactivate=0 and AppointmentFid='" + paymentList[i].AppointmentFid + "' and FlageID='" + FlageId + "'");

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
                  //  param.Add("@p_FlageID", FlageId);
                    param.Add("@p_FlageID", exist != null ? exist.FlageID : FlageId);

                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalMedicineBill", param);


                    if (ISOutStandingAmt == "Yes")
                    {

                        DynamicParameters OutStanding = new DynamicParameters();

                        //  DynamicParameters OutStanding = new DynamicParameters();
                        OutStanding.Add("@p_Query", "update FinalMedicineBill set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

                        var Standing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", OutStanding).FirstOrDefault();

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
            FROM FinalMedicineBill
            WHERE AppointmentFid = {appointmentId}
            GROUP BY AppointmentFid ");

                var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

                //if (summary.TotalAmount != summary.TotalPaid)
                if (summary != null)
                {
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
                    sumInsUpd.Add("@PatientFid", PatientID);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_MedicineOutstanding", sumInsUpd);



                }


                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult BillingRecordMedicine(int? PatientFid , int? AppointmentFid)
        {
            try
            {

                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                if (PatientFid != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", PatientFid);
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_PatientID", PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppointmentFid); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "SELECT FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();

                    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'" );
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }

                var GetUnPaid = BVGWellnessCenterDapparORM.DynamicList("sp_List_MedicineUnPaidBill");
                ViewBag.GetUnPaid = GetUnPaid;
              
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,' ',MedicinePacking) as Name from mas_service where Deactivate=0 and Servicecategory='Medicine'");
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        //public ActionResult MedicineOutstandingBillPdf(int? PatientFid, int? AppointmentFid)
        //{
        //    // 1. List cha data (Existing Code)
        //    // 1. खालची लिस्ट दाखवण्यासाठी - या SP ला parameters नको असतात
        //    ViewBag.OutstandingList = BVGWellnessCenterDapparORM.DynamicList("sp_List_MedicineOutstandingBill");

        //    if (PatientFid != null && AppointmentFid != null)
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@PatientFid", PatientFid);
        //        param.Add("@AppointmentFid", AppointmentFid);

        //        // Patient Details
        //        ViewBag.GetPatientEncounterDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

        //        // ✅ FIX: 'sp_List_MedicineOutstandingBill' ऐवजी 'sp_GetOutstandingMedicineBillDetails' वापरा
        //        // कारण या SP ला Parameters चालतात.
        //        ViewBag.FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetOutstandingMedicineBillDetails", param).FirstOrDefault();
        //        DynamicParameters paramFinal = new DynamicParameters();
        //        paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
        //        var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
        //        ViewBag.FinalBill = FinalBill;

        //        // Bill Items
        //        DynamicParameters paramItems = new DynamicParameters();
        //        paramItems.Add("@p_PatientID", PatientFid);
        //        paramItems.Add("@p_AppointmentFid", AppointmentFid);
        //        ViewBag.GenerateBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramItems).ToList();
        //        // 4. Payment History (History Table sathi)
        //        DynamicParameters paramHist = new DynamicParameters();
        //        string queryHistory = "SELECT Amount, PaymentReferenceNumber, PaymentMode, FDate FROM FinalMedicineBill WHERE Deactivate=0 AND PatientFid='" + PatientFid + "' AND AppointmentFid='" + AppointmentFid + "' ORDER BY FDate DESC";
        //        paramHist.Add("@p_Query", queryHistory);
        //        // .ToList() mule sarv records (1000 ani 60) list madhe yetil
        //        var historyList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramHist).ToList();
        //        ViewBag.GetFinalbillDetails = historyList;
        //    }

        //    return View("MedicineOutstandingBill");
        //}
        #region GetBillRecord
        public ActionResult GetMedicineBillRecord(int? Bill_Fid, int? PatientFid, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string paymentStatus = "";
                if (AppointmentFid != null && AppointmentFid != 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", PatientFid);
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_PatientID", PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    if (FinalBill != null)
                    {
                        ViewBag.Discountamt = FinalBill.DiscountAmt;
                    }
                    else
                    {
                        ViewBag.Discountamt = 0;
                    }


                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT PatientFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalMedicineBill WHERE PatientFid ='" + PatientFid + "' and  AppointmentFid='" + AppointmentFid + "'  GROUP BY PatientFid, AppointmentFid ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                    if (GetOutstandingDetails != null)
                    {
                        paymentStatus = GetOutstandingDetails.RemainingAmount == 0 ? "Paid" : "Unpaid";
                    }

                }

                return Json(new { GetBillList = ViewBag.GenerateBill, paymentStatus = paymentStatus, DiscountAmt = ViewBag.Discountamt, GetFinalbillDetails = ViewBag.GetFinalbillDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion

        #region PaidBillRecords
        public ActionResult PaidBillMedicineRecords(int? PatientFid, int? AppointmentFid, string source)

        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Store the source in ViewBag to use in the "Back" or "Close" button
                ViewBag.SourcePage = source;
                if (PatientFid != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@PatientFid", PatientFid);
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetailsforMedicine", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_PatientID", PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalMedicineBill where Deactivate=0 and PatientFid='" + PatientFid + "' and  AppointmentFid='" + AppointmentFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT PatientFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalMedicineBill WHERE PatientFid ='" + PatientFid + "' and  AppointmentFid='" + AppointmentFid + "'  GROUP BY PatientFid, AppointmentFid ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_MedicinePaidBill");
                ViewBag.GetPaidBillList = GetPaidBillList;

                return View();
            }
            catch (Exception Ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region Outstanding
        public ActionResult MedicineOutstandingBill()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }


                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", $@"SELECT 
    ob.*,
	 CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
 CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName	,
 MC.ClinicName,
    pa.*,
    p.*
FROM MedicineOutstandingBill ob
INNER JOIN PatientAppoinment pa  ON ob.AppointmentFid = pa.AppointmentFid
INNER JOIN Patient p ON pa.PatientFid = p.PatientFid
INNER JOIN  Mas_Clinic MC ON PA.ClinicFid = MC.Fid
INNER JOIN UserMaster UM on UM.UserId = PA.DoctorFid
WHERE ob.Status = 'Unpaid' ORDER BY ob.Id DESC;");
                var Outstanding = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramService);
                ViewBag.OutstandingList = Outstanding;



                return View();


            }
            catch (Exception Ex)
            {
                return View();
            }
        }
        public ActionResult GetOutstandingBillDetails(string Id, string AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session expired" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"SELECT 
    ob.Id,
    ob.AppointmentFid,
    ob.TotalAmount,
    ob.TotalPaid,
    ob.RemainingAmount,
    ob.Status AS BillStatus,
    
    pa.AppoinmentDate,
    pa.SloteStartTime,
    pa.SloteEndTime,
    pa.ServiceDetails,
    
    p.PatientFid,
    p.FirstName,
    p.LName,
    p.Email,
    p.ContactNumber,
    p.DateOfBirth,
    p.Age,
    p.Gender,
    p.Address,
    p.CityName,
    p.PostalCode
FROM MedicineOutstandingBill ob
LEFT JOIN PatientAppoinment pa ON pa.AppointmentFid = ob.AppointmentFid
LEFT JOIN Patient p ON p.PatientFid = pa.PatientFid WHERE ob.AppointmentFid = '{AppointmentFid}' AND ob.Id = '{Id}' AND ob.Deactivate = 0");

                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();
                //ViewBag.GenerateBill = GetBillList; 
                ViewBag.PatientFid = GetBillList.PatientFid;
                ViewBag.TotalAmount = GetBillList.TotalAmount;
                ViewBag.TotalPaid = GetBillList.TotalPaid;
                ViewBag.RemainingAmount = GetBillList.RemainingAmount;
                return Json(new { TotalAmount = ViewBag.TotalAmount, TotalPaid = ViewBag.TotalPaid, RemainingAmount = ViewBag.RemainingAmount, PatientFid = ViewBag.PatientFid }, JsonRequestBehavior.AllowGet);
                //return Json(new
                //{
                //    success = true,
                //    billDetails = billDetails,
                //    //paymentDetails = paymentDetails
                //});
            }
            catch (Exception Ex)
            {
                return Json(new { success = false, message = Ex.Message });
            }
        }
        public ActionResult GetOutstandingBillRecord(int? Id, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string paymentStatus = "";
                if (AppointmentFid != null && AppointmentFid != 0)
                {

                }


                return Json(new { GetBillList = ViewBag.GenerateBill, paymentStatus = paymentStatus, DiscountAmt = ViewBag.Discountamt, GetFinalbillDetails = ViewBag.GetFinalbillDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion Outsanding

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
                    GetBillList = BVGWellnessCenterDapparORM.ReturnList<ClsPrescription>("sp_List_GenerateBillCounterMedicine", paramBill).FirstOrDefault();
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

        public ActionResult DeleteBill(string Bill_Fid_Encrapted, int? AppointmentFid , int? PatientID)
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
                //paramDelete.Add("@p_Bill_Fid_Encrapted", Bill_Fid_Encrapted);
                paramDelete.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramDelete.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillCounterMedicinel", paramDelete);

                var SuccesMsg = paramDelete.Get<string>("@p_Message");
                var Icon = paramDelete.Get<string>("@p_Icon");

              

                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", AppointmentFid);
                paramBill.Add("@p_PatientID", PatientID);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillCounterMedicine", paramBill).ToList();
                var GenerateBill = GetBillList;

                return Json(new { GenerateBill = GenerateBill, SuccesMsg = SuccesMsg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        
    }
  }