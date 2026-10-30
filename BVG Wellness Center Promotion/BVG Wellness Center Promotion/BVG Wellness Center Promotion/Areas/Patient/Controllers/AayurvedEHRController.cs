using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using static BVG_Wellness_Center_Promotion.Areas.Patient.Models.EHR;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class AayurvedEHRController : Controller
    {

        // GET: Patient/AayurvedEHR
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //private List<int> SafeSplit(string ids)
        //{
        //    if (string.IsNullOrEmpty(ids))
        //        return new List<int>();

        //    return ids.Split(',')
        //              .Where(x => !string.IsNullOrWhiteSpace(x))
        //              .Select(int.Parse)
        //              .ToList();
        //}


        public ActionResult AayurvedEHRList()
        {


            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            DynamicParameters param = new DynamicParameters();

            if (Session["UserRole"].ToString() == "IsAdmin")
            {
                param.Add("@p_Query", $@"SELECT 
    PA.*, 
    P.*, 
    MC.*,
 CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName	,
    UM.UserId,
    MD.DepartmentName


FROM 
    PatientAppoinment PA
INNER JOIN 
    Patient P ON PA.PatientFid = P.PatientFid
INNER JOIN 
    Mas_Clinic MC ON PA.ClinicFid = MC.Fid
	INNER JOIN 
	UserMaster UM on UM.UserId = PA.DoctorFid
	LEFT JOIN MAS_Department MD 
    ON MD.DepartmentID = UM.DepartmentID


WHERE 
    PA.Status = 'Check In' 
    AND CAST(PA.AppoinmentDate AS DATE) = CAST(GETDATE() AS DATE);
	");
                var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.TodayPatients = TodayPatients;
            }
            else if (Session["UserRole"].ToString() == "IsDoctor")
            {
                param.Add("@p_Query", $@"SELECT 
    PA.*, 
    P.*, 
    MC.*,
 CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName	,
    UM.UserId,
    MD.DepartmentName


FROM 
    PatientAppoinment PA
INNER JOIN 
    Patient P ON PA.PatientFid = P.PatientFid
INNER JOIN 
    Mas_Clinic MC ON PA.ClinicFid = MC.Fid
	INNER JOIN 
	UserMaster UM on UM.UserId = PA.DoctorFid
	LEFT JOIN MAS_Department MD 
    ON MD.DepartmentID = UM.DepartmentID


WHERE 
    PA.Status = 'Check In'  and  UM.UserId = '{Session["UserID"]}' 
    AND CAST(PA.AppoinmentDate AS DATE) = CAST(GETDATE() AS DATE);
	");
                var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.TodayPatients = TodayPatients;
            }
         
            return View();

        }

        [HttpGet]
        public ActionResult AayurvedEHR(int? OPDNO, int? PulseExamID, int? AyurvedaDiagnosisID, int? AppoinmentID)
            {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            EHR model = new EHR();
            DynamicParameters param;

          //  Forms
           param = new DynamicParameters();
          //   param.Add("@p_Query", "SELECT FormID, FormName FROM MAS_Form WHERE IsActive=1");
          //  ViewBag.Forms = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            param.Add("@p_Query", "SELECT FormID, FormName FROM MAS_Form WHERE IsActive=1");
            var data = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
            ViewBag.Forms = data;

            if (AppoinmentID.HasValue)
            {
                DynamicParameters pathyaMappingParam = new DynamicParameters();
                pathyaMappingParam.Add("@p_Query", $"SELECT PathyaId FROM Save_AppointmentPathya WHERE AppointmentFid = {AppoinmentID}");
                var savedPathyas = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pathyaMappingParam);
                
                var pathyaIdsList = new List<int>();
                if (savedPathyas != null && savedPathyas.Any())
                {
                    foreach(var sp in savedPathyas)
                    {
                        pathyaIdsList.Add(Convert.ToInt32(sp.PathyaId));
                    }
                }
                ViewBag.SavedPathyaIds = pathyaIdsList;

                DynamicParameters reportParam = new DynamicParameters();
                reportParam.Add("@p_Query", $"SELECT * FROM UserReports WHERE AppointmentId = {AppoinmentID} AND Deactivate = 0 ORDER BY Date DESC");
                var patientMobileReports = BVGWellnessCenterDapparORM.ReturnList<PatientMobileReportModel>("sp_ExecuteQuery", reportParam);
                ViewBag.PatientMobileReports = patientMobileReports != null ? patientMobileReports.ToList() : new List<PatientMobileReportModel>();
            }
            else
            {
                ViewBag.SavedPathyaIds = new List<int>();
                ViewBag.PatientMobileReports = new List<PatientMobileReportModel>();
            }


            //  Formulas
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT FormulaID, FormulaName FROM MAS_Formula WHERE IsActive=1");
            var FormulasMain = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.FormulasMain = FormulasMain;
          // Packages


             param = new DynamicParameters();
            param.Add("@p_Query", "SELECT PackageID, PackageName FROM MAS_Package");
            var Packages = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.Packages = Packages;

          //Quantities
           param = new DynamicParameters();
            param.Add("@p_Query", "SELECT QuantityID, QuantityName FROM MAS_Quantity");
           var Quantities = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.Quantities = Quantities;
           //  Time
           param = new DynamicParameters();
            param.Add("@p_Query", "select TimeID,TimeName from MAS_Time");
            var Time = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.Time = Time;


           // Numbers(?????????? dropdown ???? ????? assign)
            param = new DynamicParameters();
            param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
            var NumberLeft  = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.NumberLeft = NumberLeft;


            //param = new DynamicParameters();
            //param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
            //var NumberRight = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            //ViewBag.NumberRight = NumberRight;



            //param = new DynamicParameters();
            //param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
            //var DoseNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            //ViewBag.DoseNumbers = DoseNumbers;


           // Fluid Vehicle
            param = new DynamicParameters();
            param.Add("@p_Query", "select VehicleID, VehicleName from MAS_FluidVehicle");
             var FluidVehicle  = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.FluidVehicle = FluidVehicle;


            //param = new DynamicParameters();
            //param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
            //var FluidNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            //ViewBag.FluidNumbers = FluidNumbers;


           param = new DynamicParameters();
           param.Add("@p_Query", "SELECT QuantityID, QuantityName FROM MAS_Quantity");
           var FluidQuantities = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
           ViewBag.FluidQuantities = FluidQuantities;


            param = new DynamicParameters();
            param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
            var DaysNumber = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.DaysNumber = DaysNumber;


            //new dropdown Binding
            param = new DynamicParameters();
            param.Add("@p_Query", "select id as ID, NumberQuantity from MAS_NumberMList");
            var NumberRight = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.NumberRight = NumberRight;

            param = new DynamicParameters();
            param.Add("@p_Query", "select id as ID, NumberQuantity from MAS_NumberMList");
            var DoseNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.DoseNumbers = DoseNumbers;


            param = new DynamicParameters();
            param.Add("@p_Query", "select id as ID, NumberQuantity from MAS_NumberMList");
            var FluidNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.FluidNumbers = FluidNumbers;

            //CAse paper Masters 



            /* ================= Complaint ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ComplaintMaster WHERE IsActive = 1");
            ViewBag.Complaints = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Duration Unit ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_DurationUnitMaster WHERE IsActive = 1");
            ViewBag.DurationUnits = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Nature Of Illness ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_NatureOfIllnessMaster WHERE IsActive = 1");
            ViewBag.NatureList = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            /* ================= Previous Treatment ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, TreatmentName as Name FROM Mas_PreviousTreatment WHERE IsActive = 1");
            ViewBag.PreviousTreatments = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Progression ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ProgressionMaster WHERE IsActive = 1");
            ViewBag.Progressions = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Associated Symptoms ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_AssociatedSymptoms WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Symptoms = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Aggravating ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_AggravatingFactors WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Aggravating = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            /* ================= Relieving ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_RelievingFactors WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Relieving = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();



            /* ================= Response ================= */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ResponsePreviousTreatment WHERE IsActive = 1");
            ViewBag.ResponseTreatment = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            //DynamicParameters param;

            /* Chronic Illness */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ChronicIllness WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.ChronicIllness = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

             /* Past Surgery */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_PastSurgery WHERE IsActive = 1  ORDER BY Name ASC");
            ViewBag.PastSurgery = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            
            /* Trauma */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_TraumaHistory WHERE IsActive = 1  ORDER BY Name ASC");
            ViewBag.Trauma = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                    
            /* Allergy */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_Allergy WHERE IsActive = 1  ORDER BY Name ASC");
            ViewBag.Allergy = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            /* Medication */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_LongTermMedication WHERE IsActive = 1  ORDER BY Name ASC");
            ViewBag.Medication = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            DynamicParameters param10;
            param10 = new DynamicParameters();
            param10.Add("@p_Query", "SELECT Id, Name FROM Mas_FamilyHistory WHERE IsActive =1 ORDER BY Name ASC");
            ViewBag.FamilyHistory = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param10).ToList();

            /* Lab Tests */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM MAS_LabTestsAdvised WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.LabTests = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param) .ToList();


            /* Imaging */
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM MAS_ImagingAdvised WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Imaging = BVGWellnessCenterDapparORM  .ReturnList<EHR>("sp_ExecuteQuery", param) .ToList();


            //

            param = new DynamicParameters();
            param.Add("@p_Query", "select DiseaseId as ID, DiseaseName as Name from Mas_Disease where IsActive = 1");
            ViewBag.Disease = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();


            // Suchna dropdown
            param = new DynamicParameters();
            param.Add("@p_Query", $@"SELECT Fid, InstructionName , Description FROM MAS_SuchanaMaster where isActive = 1");
            var Suchana = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
            ViewBag.Suchana = Suchana;


            //services
            DynamicParameters paramService = new DynamicParameters();
            paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(ServiceName,'',MedicinePacking) as Name from mas_service where Deactivate=0 AND [Group] <> 'Appointment'");
            var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
            ViewBag.AdviseServices = GetService;

            DynamicParameters paramGroup = new DynamicParameters();
            paramGroup.Add("@p_Query", $@"select ServicesGroupId as Id, ServiceGroupName as Name from ServicesGroup where IsActive = 1");
            ViewBag.ServiveGroup = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramGroup);

            // ?? Prescription madhun direct load kara
            if (AppoinmentID != null && AppoinmentID != 0)
            {
                DynamicParameters paramPresService = new DynamicParameters();
                paramPresService.Add("@p_Query", $@"
        select MF.FormID as ID ,MF.FormName as Name 
        from MAS_Form MF 
        left join MedicineFormula on MF.FormID = MedicineFormula.FormID 
        where MedicineFormula.AppointmentFid = '{AppoinmentID}'  AND ISNULL(MedicineFormula.IsActive,1) = 1");

                var presService = BVGWellnessCenterDapparORM
                    .ReturnList<DropdownItem>("sp_ExecuteQuery", paramPresService);

                // ?? HE IMPORTANT LINE (override)
                ViewBag.Service = presService;
            }


            param = new DynamicParameters();
            param.Add("@p_Query", "select ServiceFid as ID, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0 ");/* where ServiceName='Health Check'*/
            var Medicine = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
            ViewBag.MedicineList = Medicine;

            DynamicParameters paramPathya =
    new DynamicParameters();

            paramPathya.Add(
                "@p_Query",
                @"SELECT
        Id,
        PathyaName,
        PathyaInstruction
      FROM tbl_PathyaMaster
      ORDER BY PathyaName"
            );

            ViewBag.PathyaList =
                BVGWellnessCenterDapparORM
                .ReturnList<dynamic>(
                    "sp_ExecuteQuery",
                    paramPathya
                );



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
WHERE PA.AppointmentFid = {AppoinmentID};
	");
            var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
            ViewBag.TodayPatients = TodayPatients;

                var GamePatinetFid = TodayPatients.PatientFid;



                DynamicParameters paramGame = new DynamicParameters();
                paramGame.Add("@PatientFid", GamePatinetFid);
                paramGame.Add("@Origin", "GetFullGameReport");

                using (var multi = BVGWellnessCenterDapparORM.QueryMultiple("SP_GetFullGameReport", paramGame))
                {
                    var userInfo = !multi.IsConsumed ? multi.Read<dynamic>().ToList() : new List<dynamic>();
                    var answersDetail = !multi.IsConsumed ? multi.Read<dynamic>().ToList() : new List<dynamic>();
                    var totalScore = !multi.IsConsumed ? multi.Read<dynamic>().FirstOrDefault() : null;
                    var levelCounts = !multi.IsConsumed ? multi.Read<dynamic>().ToList() : new List<dynamic>();
                    var sectionCounts = !multi.IsConsumed ? multi.Read<dynamic>().FirstOrDefault() : null;

                    ViewBag.UserInfo = userInfo;
                    ViewBag.AnswersDetail = answersDetail;
                    ViewBag.TotalScore = totalScore != null ? totalScore.TotalScore : 0;
                    ViewBag.LevelCounts = levelCounts;
                    ViewBag.SectionCounts = sectionCounts;
                    ViewBag.PatientFid = GamePatinetFid;
                }



                if (TodayPatients != null) ViewBag.AppointmentFid = TodayPatients.AppointmentFid;

                // FIRST assign patient
                model = TodayPatients;
                // ? REPORT FETCH CODE (??? paste ??)
                // ? REPORT FETCH
                // THEN assign reports
                List<UserReportModel> reports = new List<UserReportModel>();
                ViewBag.AppointmentReports = new List<UserReportModel>();

                if (TodayPatients != null)
                {
                    var userId = TodayPatients.PatientFid.ToString();
                    // Check if userId is not "0" and not null/empty
                    if (userId != "0" && !string.IsNullOrEmpty(userId))
                    {
                        DynamicParameters reportParam = new DynamicParameters();
                        reportParam.Add("@p_Query", $@"
                            SELECT * FROM (
                                SELECT ur.Fid, ur.UserId, ur.ReportType, ur.ReportFile1, ur.ReportFile2, ur.ReportFile3, ur.ReportFile4, ur.ReportFile5, ur.ReportTitle, ur.Date, ur.Deactivate 
                                FROM UserReports ur INNER JOIN Patient p ON ur.UserId = p.MUid 
                                WHERE ur.Deactivate = 0 AND p.PatientFid = '{userId}'
                                UNION ALL
                                SELECT uwr.Fid, uwr.UserId, uwr.ReportType, uwr.ReportFile1, uwr.ReportFile2, uwr.ReportFile3, uwr.ReportFile4, uwr.ReportFile5, uwr.ReportTitle, uwr.Date, uwr.Deactivate 
                                FROM [JYAMDB].[dbo].[UserWiseReports] uwr INNER JOIN Patient p ON uwr.UserId = p.MUid 
                                WHERE uwr.Deactivate = 0 AND p.PatientFid = '{userId}'
                            ) as AllReports
                            ORDER BY Date DESC
                        ");

                        reports = BVGWellnessCenterDapparORM
                            .ReturnList<UserReportModel>("sp_ExecuteQuery", reportParam)
                            .ToList();

                        DynamicParameters appReportParam = new DynamicParameters();
                        appReportParam.Add("@p_Query", $@"
                            SELECT ur.* 
                            FROM UserReports ur
                            INNER JOIN AppointmentReportsMapping arm ON ur.Fid = arm.UserReportFid
                            WHERE ur.Deactivate = 0 
                            AND arm.AppointmentFid = '{AppoinmentID ?? 0}'
                            ORDER BY ur.Date DESC
                        ");

                        ViewBag.AppointmentReports = BVGWellnessCenterDapparORM
                            .ReturnList<UserReportModel>("sp_ExecuteQuery", appReportParam)
                            .ToList();
                    }
                    else
                    {
                        // Optional: Ensure the list is empty if userId is 0
                        reports = new List<UserReportModel>();
                    }
                }
                //     // Set OPDNO in the model for later use
                //     model.OPDNO = OPDNO.Value;
                if (TodayPatients != null)
                {
                    var pFid = TodayPatients.PatientFid;
                    int currentAppId = AppoinmentID ?? 0;

                    // ??????? ??????: 'FDate' ???? ????? ??? ??? ?????? ??????
                    string lastDateQuery = $@"SELECT TOP 1 FDate AS LastDateTime
                             FROM PatientAppoinment 
                             WHERE PatientFid = {pFid} 
                             AND AppointmentFid < {currentAppId}
                             ORDER BY AppointmentFid DESC";

                    var lastDateData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", new DynamicParameters(new { p_Query = lastDateQuery })).FirstOrDefault();

                    if (lastDateData != null && lastDateData.LastDateTime != null)
                    {
                        // Fornat: "17-03-2026 05:30 PM"
                        ViewBag.LastFollowUpDate = ((DateTime)lastDateData.LastDateTime).ToString("dd-MM-yyyy hh:mm tt");
                    }
                    else
                    {
                        ViewBag.LastFollowUpDate = "No Previous Record";
                    }
                }

                // ? Check for existing Pulse Exam (Fid)
                var fidQuery = new DynamicParameters();
                 fidQuery.Add("@p_Query", $"SELECT Fid FROM EHRPulseExam WHERE AppointmentFid = { AppoinmentID}");
                 var pulseExam = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", fidQuery).FirstOrDefault();
                 PulseExamID = pulseExam?.Fid  ?? 0;
                 ViewBag.EightPlusID = PulseExamID;

                    // ? Check for existing Ayurveda Diagnosis
                 var diagQuery = new DynamicParameters();
                 diagQuery.Add("@p_Query", $"SELECT AyurvedaDiagnosisID FROM AyurvedaDiagnosis WHERE AppointmentFid = {AppoinmentID}");
                 var ayurDiag = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", diagQuery).FirstOrDefault();
                 AyurvedaDiagnosisID = ayurDiag?.AyurvedaDiagnosisID ?? 0;
                 ViewBag.AyurvedaDiagnosisID = AyurvedaDiagnosisID;


                  DynamicParameters presParam = new DynamicParameters();
                  presParam.Add("@AppoinmentFid", AppoinmentID);
                  var prescriptions = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam);

                  ViewBag.PrescriptionList = prescriptions;
                                
                  DynamicParameters presRParam = new DynamicParameters();
                  presRParam.Add("@AppoinmentFid", AppoinmentID);
                  var prescriptionsResult = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionRESULTByOPDNO", presRParam);
                  ViewBag.MedicineInstructions = prescriptionsResult;



                DynamicParameters Counter = new DynamicParameters();
                Counter.Add("@AppoinmentFid", AppoinmentID);
                var CounterMedicine = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetCounterMedicineFormula", Counter);
                ViewBag.CounterMedicineList = CounterMedicine;


                DynamicParameters CounterResult = new DynamicParameters();
                CounterResult.Add("@AppoinmentFid", AppoinmentID);
                var CounterView = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionRESULTForCounterMedicine", CounterResult);
                ViewBag.CounterFormulaView = CounterView;




                DynamicParameters MEDICINEVIEW = new DynamicParameters();
                MEDICINEVIEW.Add("@p_Query", $"SELECT * FROM Save_CounterMedicineResult WHERE AppointmentFid = {AppoinmentID} AND IsActive = 1");
                var MEDICINEVIEWw = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", MEDICINEVIEW);
                ViewBag.CounterResultView = MEDICINEVIEWw;
                if (MEDICINEVIEWw.Count() == 0)
                {
                    ViewBag.CounterResultView = null;
                }

                            

                DynamicParameters param11 = new DynamicParameters();
                param11.Add("@p_Query", "SELECT Fid, InstructionName, Description, FormID FROM MAS_SuchanaMaster WHERE IsActive = 1");
                var allSuchana = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param11).ToList();
                ViewBag.AllSuchna = allSuchana;

                // Group by FormID for JS
                var suchanaGrouped = allSuchana
                                     .GroupBy(x => x.FormID)
                                     .ToDictionary(g => g.Key.ToString(),  // <-- ??? .ToString() ???? ???
                                      g => g.Select(s => new
                                     {
                                        s.Fid,
                                        s.InstructionName,
                                        s.Description
                                      }).ToList());

                // ?? 3?? ViewBag ????? store ???
                ViewBag.SuchanaGrouped = suchanaGrouped;




                DynamicParameters Dispensing = new DynamicParameters();
                Dispensing.Add("@p_Query", $"SELECT * FROM Save_MedicineDispensing WHERE AppointmentFid ='{AppoinmentID}' and IsActive = 1");
                var MedicineDispensing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Dispensing);
                ViewBag.ResultView = MedicineDispensing;
                if (MedicineDispensing.Count() == 0)
                {
                    ViewBag.ResultView = null;
                }

          
                 var AdvisedTherapy = new DynamicParameters();
                AdvisedTherapy.Add("@p_Query", $"SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, AT.Frequency, AT.InstructionPatinet, AT.InstructionTherapist, AT.Consent FROM Tbl_AdvisedTherapy AT INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid WHERE AT.AppointmentFid = '{AppoinmentID}'");
                var AdvisedTherapyy = BVGWellnessCenterDapparORM.ReturnList<AdvisedTherapyVM>("sp_ExecuteQuery", AdvisedTherapy).ToList();
                ViewBag.AdvisedTherapyResult = AdvisedTherapyy;


                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", AppoinmentID);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;


                DynamicParameters paramFull = new DynamicParameters();
                    paramFull.Add("@AppointmentFid", AppoinmentID);

                    var fullData = BVGWellnessCenterDapparORM.ReturnList<EHR>("Sp_GetFullEHR_ByAppointment", paramFull).FirstOrDefault();

                    if (fullData != null)
                    {
                        model = fullData;

                    model.UserReports = reports;


                    if (!string.IsNullOrEmpty(model.DiseaseID))
                        {
                            model.DiseaseIDList =
                              model.DiseaseID
                                   .Split(',')
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Select(int.Parse)
                                   .ToList();
                        }

                    // ?? 1?? FamilyHistory Split
                         if (!string.IsNullOrEmpty(model.FamilyHistoryIds))
                         {
                     
                             model.FamilyHistoryIdsList =
                                     model.FamilyHistoryIds
                                          .Split(',')
                                          .Where(x => !string.IsNullOrWhiteSpace(x))
                                          .Select(int.Parse)
                                          .ToList();
                         }

                        // Load Family History List from database
                        if (model.CasePaperId != 0)
                        {
                            DynamicParameters familyParam = new DynamicParameters();
                            familyParam.Add("@p_Query", $"SELECT Id, CasePaperId, FamilyHistoryId, Relation FROM CasePaper_FamilyHistory WHERE CasePaperId = '{model.CasePaperId}'");
                            var familyRows = BVGWellnessCenterDapparORM.ReturnList<FamilyHistoryModel>("sp_ExecuteQuery", familyParam).ToList();
                            model.FamilyHistoryList = familyRows
                                .GroupBy(x => x.Relation ?? "")
                                .Select(g => new FamilyHistoryModel
                                {
                                    Relation = g.Key,
                                    FamilyHistoryIds = g.Where(x => x.FamilyHistoryId.HasValue).Select(x => x.FamilyHistoryId.Value).ToList()
                                }).ToList();
                        }
                        else
                        {
                            model.FamilyHistoryList = new List<FamilyHistoryModel>();
                        }
                     
                         if (!string.IsNullOrEmpty(model.ImagingAdvisedIds))
                         {
                             model.ImagingAdvisedIdsList =
                                    model.ImagingAdvisedIds
                                         .Split(',')
                                         .Where(x => !string.IsNullOrWhiteSpace(x))
                                         .Select(int.Parse)
                                         .ToList();
                         }
                        if (!string.IsNullOrEmpty(model.LabTestsAdvisedIds))
                        {
                            model.LabTestsAdvisedIdsList =
                               model.LabTestsAdvisedIds
                                    .Split(',')
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(int.Parse)
                                    .ToList();
                        }


                    // ?? 2?? Complaints Load (If CasePaperId exists)
                    if (model.CasePaperId != 0)
                    {
                        DynamicParameters compParam = new DynamicParameters();

                        compParam.Add("@p_Query", $"SELECT * FROM CasePaperComplaints WHERE CasePaperId = '{model.CasePaperId}'");

                        var complaints = BVGWellnessCenterDapparORM
                                          .ReturnList<ComplaintVM>("sp_ExecuteQuery", compParam)
                                          .ToList();
                        model.Complaints = complaints ?? new List<ComplaintVM>();



                    }

                    else
                    {
                        model.Complaints = new List<ComplaintVM>();

                    }

                    // ?? Past History Load (If CasePaperId exists)
                    // ================== ?? PAST HISTORY LOAD (FIXED) ==================

                    if (model.CasePaperId != 0)
                    {
                        // ? Chronic Illness
                        DynamicParameters chronicParam = new DynamicParameters();
                        chronicParam.Add("@p_Query", $@"SELECT 
        Id,
        CasePaperId,
        IllnessId AS ChronicIllnessId,
        Duration,
        DurationUnit
    FROM CasePaper_ChronicIllness
    WHERE CasePaperId = '{model.CasePaperId}'");
                        var chronic = BVGWellnessCenterDapparORM.ReturnList<ChronicIllnessModel>("sp_ExecuteQuery", chronicParam).ToList();
                        model.ChronicIllnessList = chronic ?? new List<ChronicIllnessModel>();

                        // ? Past Surgery
                        DynamicParameters surgeryParam = new DynamicParameters();
                        surgeryParam.Add("@p_Query", $@"SELECT 
        Id,
        CasePaperId,
        SurgeryId AS PastSurgeryId,
        Duration AS PastSurgeryDuration,
        DurationUnit AS PastSurgeryDurationUnit
    FROM CasePaper_PastSurgery
    WHERE CasePaperId = '{model.CasePaperId}'");
                        var surgery = BVGWellnessCenterDapparORM.ReturnList<PastSurgeryModel>("sp_ExecuteQuery", surgeryParam).ToList();
                        model.PastSurgeryList = surgery ?? new List<PastSurgeryModel>();

                        // ? Trauma
                        DynamicParameters traumaParam = new DynamicParameters();
                        traumaParam.Add("@p_Query", $@"SELECT 
        Id,
        CasePaperId,
        TraumaId AS TraumaHistoryId,
        Duration AS TraumaHistoryDuration,
        DurationUnit AS TraumaHistoryDurationUnit
    FROM CasePaper_Trauma
    WHERE CasePaperId = '{model.CasePaperId}'");
                        var trauma = BVGWellnessCenterDapparORM.ReturnList<TraumaHistoryModel>("sp_ExecuteQuery", traumaParam).ToList();
                        model.TraumaHistoryList = trauma ?? new List<TraumaHistoryModel>();

                        // ? Allergy
                        DynamicParameters allergyParam = new DynamicParameters();
                        allergyParam.Add("@p_Query", $@"SELECT 
        Id,
        CasePaperId,
        AllergyId,
        Duration AS AllergyHistoryDuration,
        DurationUnit AS AllergyHistoryDurationUnit
    FROM CasePaper_Allergy
    WHERE CasePaperId = '{model.CasePaperId}'");
                        var allergy = BVGWellnessCenterDapparORM.ReturnList<AllergyModel>("sp_ExecuteQuery", allergyParam).ToList();
                        model.AllergyList = allergy ?? new List<AllergyModel>();

                        // ? Medication
                        DynamicParameters medicationParam = new DynamicParameters();
                        medicationParam.Add("@p_Query", $@"SELECT 
        Id,
        CasePaperId,
        MedicationId AS LongTermMedicationId,
        Duration AS LongTermMedicationDuration,
        DurationUnit AS LongTermMedicationDurationUnit
    FROM CasePaper_Medication
    WHERE CasePaperId = '{model.CasePaperId}'");
                        var med = BVGWellnessCenterDapparORM.ReturnList<LongTermMedicationModel>("sp_ExecuteQuery", medicationParam).ToList();
                        model.LongTermMedicationList = med ?? new List<LongTermMedicationModel>();
                    }
                    else
                    {
                        model.ChronicIllnessList = new List<ChronicIllnessModel>();
                        model.PastSurgeryList = new List<PastSurgeryModel>();
                        model.TraumaHistoryList = new List<TraumaHistoryModel>();
                        model.AllergyList = new List<AllergyModel>();
                        model.LongTermMedicationList = new List<LongTermMedicationModel>();
                        model.FamilyHistoryList = new List<FamilyHistoryModel>();
                    }




                }//full data 

            } // if else main 
            else
             {
                    ViewBag.TodayPatients = null;
            }

            return View(model);


        }
      

        [HttpPost]
        public ActionResult PulseExam(PlusExam model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (ModelState.IsValid)
                {
                    DynamicParameters param = new DynamicParameters();

                    // Add all model properties to parameters
                    param.Add("@Fid", model.Fid);
                    param.Add("@AppointmentFid", model.AppointmentFid);
                    param.Add("@PatientFid", model.PatientFid);
                    param.Add("@Condition", model.Condition);
                    param.Add("@PulseRate", model.PulseRate);
                    param.Add("@BloodPressureDia", model.BloodPressureDia);
                    param.Add("@BloodPressureSys", model.BloodPressureSys);
                    param.Add("@Weight", model.Weight);

                    // Dosha parameters
                    param.Add("@Vat", model.Vat);
                    param.Add("@Vataf", model.Vataf);
                    param.Add("@Vatpit", model.Vatpit);
                    param.Add("@VarKafPit", model.VarKafPit);
                    param.Add("@VatPitKaf", model.VatPitKaf);
                    param.Add("@Leech", model.Leech);
                    param.Add("@Snake", model.Snake);
                    param.Add("@Pitta", model.Pitta);
                    param.Add("@PittaKaf", model.PittaKaf);
                    param.Add("@PittaVat", model.PittaVat);
                    param.Add("@PottaKafVat", model.PottaKafVat);
                    param.Add("@PittaVatKaf", model.PittaVatKaf);
                    param.Add("@Sparrow", model.Sparrow);
                    param.Add("@Crow", model.Crow);
                    param.Add("@Monkey", model.Monkey);
                    param.Add("@Frog", model.Frog);
                    param.Add("@kaf", model.kaf);
                    param.Add("@KaffPitta", model.KaffPitta);
                    param.Add("@Kaffvtta", model.Kaffvtta);
                    param.Add("@KafPittaVat", model.KafPittaVat);
                    param.Add("@KVP", model.KVP);
                    param.Add("@Swan", model.Swan);
                    param.Add("@Pigeon", model.Pigeon);
                    param.Add("@Peacock", model.Peacock);
                    param.Add("@SaniPat", model.SaniPat);
                    param.Add("@BustardQuail", model.BustardQuail);
                    param.Add("@GreyBird", model.GreyBird);
                    param.Add("@ButtonQuai", model.ButtonQuai);

                    // Movement parameters
                    param.Add("@GatiMovement1", model.GatiMovement1);
                    param.Add("@GatiMovement2", model.GatiMovement2);
                    param.Add("@GatiMovement3", model.GatiMovement3);
                    param.Add("@SamataUndigested", model.SamataUndigested);
                    param.Add("@Speed", model.Speed);
                    param.Add("@Rhythm", model.Rhythm);
                    param.Add("@Strength", model.Strength);
                    param.Add("@Volume", model.Volume);
                    param.Add("@Temperature", model.Temperature);

                    // Urine Exam parameters
                    param.Add("@UrinePrakritasama", model.UrinePrakritasama);
                    param.Add("@UrineVata", model.UrineVata);
                    param.Add("@UrinePitta", model.UrinePitta);
                    param.Add("@UrineRakta", model.UrineRakta);
                    param.Add("@UrineKapha", model.UrineKapha);
                    param.Add("@UrineTridosha", model.UrineTridosha);
                    param.Add("@UrinePrakritasamaOtherText", model.UrinePrakritasamaOtherText);
                    param.Add("@UrineTridoshaOtherText", model.UrineTridoshaOtherText);

                    // Stool Exam parameters
                    param.Add("@StoolNaturalState", model.StoolNaturalState);
                    param.Add("@StoolNaturalOtherText", model.StoolNaturalOtherText);
                    param.Add("@StoolVataKrishna", model.StoolVataKrishna);
                    param.Add("@StoolVataDrudha", model.StoolVataDrudha);
                    param.Add("@StoolVataShushka", model.StoolVataShushka);
                    param.Add("@StoolVataTrutita", model.StoolVataTrutita);
                    param.Add("@StoolVataPhena", model.StoolVataPhena);
                    param.Add("@StoolVataRuksha", model.StoolVataRuksha);
                    param.Add("@StoolVataDhumra", model.StoolVataDhumra);
                    param.Add("@StoolVataPravahana", model.StoolVataPravahana);
                    param.Add("@StoolVataAdhmana", model.StoolVataAdhmana);
                    param.Add("@StoolVataUdarshul", model.StoolVataUdarshul);
                    param.Add("@StoolVataKatishul", model.StoolVataKatishul);
                    param.Add("@StoolKapha", model.StoolKapha);
                    param.Add("@StoolKaphaOtherText", model.StoolKaphaOtherText);
                    param.Add("@StoolPitta", model.StoolPitta);
                    param.Add("@StoolPittaOtherText", model.StoolPittaOtherText);
                    param.Add("@StoolVataKapha", model.StoolVataKapha);
                    param.Add("@StoolVataPitta", model.StoolVataPitta);
                    param.Add("@StoolKaphaPitta", model.StoolKaphaPitta);
                    param.Add("@StoolTridosha", model.StoolTridosha);

                    // Tongue Exam parameters
                    param.Add("@NaturalStateTongue", model.NaturalStateTongue);
                    param.Add("@NaturalStateOther", model.NaturalStateOther);
                    param.Add("@RukshaSnigdha", model.RukshaSnigdha);
                    param.Add("@SamaNiramaAlpasama", model.SamaNiramaAlpasama);
                    param.Add("@VataCharacteristics", model.VataCharacteristics);
                    param.Add("@PittaCharacteristics", model.PittaCharacteristics);
                    param.Add("@PittaOther", model.PittaOther);
                    param.Add("@KaphaCharacteristics", model.KaphaCharacteristics);
                    param.Add("@TridoshaCharacteristics", model.TridoshaCharacteristics);

                    // Voice Exam parameters
                    param.Add("@NaturalStateVoiceExam", model.NaturalStateVoiceExam);
                    param.Add("@KaphaVoiceExam", model.KaphaVoiceExam);
                    param.Add("@PittaVoiceExam", model.PittaVoiceExam);
                    param.Add("@VataVoiceExam", model.VataVoiceExam);

                    // Touch parameters
                    param.Add("@NaturalStateTouch", model.NaturalStateTouch);
                    param.Add("@PittaTouch", model.PittaTouch);
                    param.Add("@VataTouch", model.VataTouch);
                    param.Add("@KaphaTouch", model.KaphaTouch);

                    // Eye Exam parameters
                    param.Add("@EyeNaturalState", model.EyeNaturalState);
                    param.Add("@EyeVata", model.EyeVata);
                    param.Add("@EyePitta", model.EyePitta);
                    param.Add("@EyeKapha", model.EyeKapha);

                    // Appearance Exam parameters
                    param.Add("@AppearanceNaturalState", model.AppearanceNaturalState);
                    param.Add("@AppearanceVata", model.AppearanceVata);
                    param.Add("@AppearancePitta", model.AppearancePitta);
                    param.Add("@AppearanceKapha", model.AppearanceKapha);

                    // Audit parameters
                    param.Add("@isActive", model.isActive);
                    param.Add("@Mip", Request.UserHostAddress);
                    param.Add("@Mid", Guid.NewGuid().ToString());

                    if (model.Fid == null || model.Fid == 0)
                    {
                        // INSERT
                        param.Add("@Createdby", "Admin");
                        param.Add("@CreatedDate", DateTime.Now);
                        param.Add("@Updatedby", null);
                        param.Add("@UpdatedDate", null);
                        param.Add("@ActionType", "INSERT");
                    }
                    else
                    {
                        // UPDATE
                        param.Add("@Createdby", null);
                        param.Add("@CreatedDate", null);
                        param.Add("@Updatedby", "Admin");
                        param.Add("@UpdatedDate", DateTime.Now);
                        param.Add("@ActionType", "UPDATE");
                    }
                    if (model.Fid == 0)
                    {
                        param.Add("@Fid", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    }
                    else
                    {
                        param.Add("@Fid", model.Fid, DbType.Int32, ParameterDirection.InputOutput);
                    }

                    param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
                   BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_EHRPulseExam_Crud", param);
                    int newFid = param.Get<int>("@Fid");
                    int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0; // DBNull safe handling
                    TempData["message"] = model.Fid == 0 ? "Inserted successfully!" : "Updated successfully!";
                    return RedirectToAction("AayurvedEHR", new { PulseExamID = newFid, AppoinmentID = AppointmentFid });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Something went wrong: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult AyurvedaDiagnosis(AyurvedaDiagnosis model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }



                model.DiseaseID = model.DiseaseIDList != null
        ? string.Join(",", model.DiseaseIDList)
        : "";

                DynamicParameters param = new DynamicParameters();

                // Add all model properties to parameters
                param.Add("@Fid", model.AyurvedaDiagnosisID);
                param.Add("@AppointmentFid", model.AppointmentFid);
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@EightPlusExamID", model.Fid);
                param.Add("@DiseaseID", model.DiseaseID);
                param.Add("@StatusCheck", model.StatusCheck);

                // Dosha Function parameters
                param.Add("@Sams", model.Sams);
                param.Add("@VatavFunction", model.VatavFunction);
                param.Add("@VatakafFunction", model.VatakafFunction);
                param.Add("@VatpittaFunction", model.VatpittaFunction);
                param.Add("@VatakafpittaFunction", model.VatakafpittaFunction);
                param.Add("@VatapittakafFunction", model.VatapittakafFunction);
                param.Add("@PittaFunction", model.PittaFunction);
                param.Add("@PittaKafFunction", model.PittaKafFunction);
                param.Add("@PittaVatFunction", model.PittaVatFunction);
                param.Add("@PittaKafVatFunction", model.PittaKafVatFunction);
                param.Add("@PittaVatKafFunction", model.PittaVatKafFunction);
                param.Add("@KafFunction", model.KafFunction);
                param.Add("@KafPittaFunction", model.KafPittaFunction);
                param.Add("@KafVatFunction", model.KafVatFunction);
                param.Add("@KafPittaVatFunction", model.KafPittaVatFunction);
                param.Add("@KafVatPittaFunction", model.KafVatPittaFunction);

                // Metabolism parameters
                param.Add("@JatharagniAcids", model.JatharagniAcids);
                param.Add("@DhatvagniEnzymes", model.DhatvagniEnzymes);
                param.Add("@MahabhutagniHormones", model.MahabhutagniHormones);
                param.Add("@Vatavishamagni", model.Vatavishamagni);
                param.Add("@PittaTikshnagni", model.PittaTikshnagni);
                param.Add("@KafMandagni", model.KafMandagni);
                param.Add("@Samagni", model.Samagni);

                // Bowels parameter
                param.Add("@Koshtha", model.Koshtha);

                // Structure parameters
                param.Add("@RasaSentiment", model.RasaSentiment);
                param.Add("@RaktaBlood", model.RaktaBlood);
                param.Add("@MansaMuscles", model.MansaMuscles);
                param.Add("@MedaFat", model.MedaFat);
                param.Add("@AsthiBones", model.AsthiBones);
                param.Add("@MajjaNerves", model.MajjaNerves);
                param.Add("@ShukraReproductive", model.ShukraReproductive);

                // Substructure parameters
                param.Add("@StanyaBreastMilk", model.StanyaBreastMilk);
                param.Add("@RajaMenstrualBlood", model.RajaMenstrualBlood);
                param.Add("@SnayuLigaments", model.SnayuLigaments);
                param.Add("@TwakSkin", model.TwakSkin);
                param.Add("@Danta", model.Danta);
                param.Add("@Oja", model.Oja);

                // Excretion parameters
                param.Add("@Purisha", model.Purisha);
                param.Add("@Mutra", model.Mutra);
                param.Add("@Sweda", model.Sweda);

                // Upmala parameters
                param.Add("@KaphaUpmala", model.KaphaUpmala);
                param.Add("@PittaUpmala", model.PittaUpmala);
                param.Add("@RomaHairsUpmala", model.RomaHairsUpmala);
                param.Add("@NakhaNails", model.NakhaNails);
                param.Add("@KarnaMal", model.KarnaMal);
                param.Add("@NetraVit", model.NetraVit);
                param.Add("@MukhikMal", model.MukhikMal);
                param.Add("@RomKupMal", model.RomKupMal);
                param.Add("@PrajananAngMal", model.PrajananAngMal);
                param.Add("@CharmSnehMal", model.CharmSnehMal);

                // Audit parameters
                param.Add("@isActive", model.isActive);
                param.Add("@Mip", Request.UserHostAddress);
                param.Add("@Mid", Guid.NewGuid().ToString());

                if (model.AyurvedaDiagnosisID == null || model.AyurvedaDiagnosisID == 0)
                {
                    // INSERT
                    param.Add("@Createdby", "Admin");
                    param.Add("@CreatedDate", DateTime.Now);
                    param.Add("@Updatedby", null);
                    param.Add("@UpdatedDate", null);
                    param.Add("@ActionType", "INSERT");
                }
                else
                {
                    // UPDATE
                    param.Add("@Createdby", null);
                    param.Add("@CreatedDate", null);
                    param.Add("@Updatedby", "Admin");
                    param.Add("@UpdatedDate", DateTime.Now);
                    param.Add("@ActionType", "UPDATE");
                }

                if (model.AyurvedaDiagnosisID == null || model.AyurvedaDiagnosisID == 0)
                {
                    param.Add("@AyurvedaDiagnosisID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                }
                else
                {
                    param.Add("@AyurvedaDiagnosisID", model.AyurvedaDiagnosisID, DbType.Int32, ParameterDirection.InputOutput);
                }


              
                param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_AyurvedaDiagnosis_Crud", param);
                int AyurvedaDiagnosisID = param.Get<int>("@AyurvedaDiagnosisID");
                int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;

                TempData["message"] = model.AyurvedaDiagnosisID == 0 ? "Inserted successfully!" : "Updated successfully!";
                return RedirectToAction("AayurvedEHR", new { AyurvedaDiagnosisID = AyurvedaDiagnosisID, AppoinmentID = AppointmentFid });

            }
            catch (Exception ex)
            {
                TempData["error"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult MedicineMaster(PrescriptionModel model, string prescriptionJson, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }


                List<PrescriptionModel> prescriptions = new List<PrescriptionModel>();
                if (!string.IsNullOrEmpty(prescriptionJson))
                {
                    prescriptions = JsonConvert.DeserializeObject<List<PrescriptionModel>>(prescriptionJson);
                }
                
                 foreach (var item in prescriptions)
                {

                  

                    var param = new DynamicParameters();
                    // --- UPDATE ???? ????? ??? ---
                    // ?? ???? ??? ???? ?? 0 ????, ?? ???? ???? ???? ?? ?????? ID ????
                    param.Add("@PrescriptionID", item.PrescriptionID ?? 0);
                    param.Add("@FormID", item.FormID);
                    param.Add("@PackageID", item.PackageID);
                    param.Add("@NumberLeftID", item.NumberLeftID);
                    param.Add("@ProcessingQty", item.ProcessingQty);
                    param.Add("@ProcessingUnit", item.ProcessingUnit);
                    param.Add("@DoseNumberID", item.DoseNumberID);
                    param.Add("@DoseID", item.QuantityIDDose);
                    param.Add("@TimeID", item.TimeID);
                    //param.Add("@FluidVehicleID", item.FluidVehicleID);
                    param.Add("@MixingData", item.MixingDataJson);
                    param.Add("@MixingDataFluid", item.MixingDataFluid);
                   param.Add("@AppointmentFid", AppointmentFid);
                    //param.Add("@MedicineDate", item.MedicineDate);
                    param.Add("@DaysNumber", item.DaysNumber);
                    param.Add("@AppointmentFid", AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);

                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertPrescription", param);
                    int outputFid = param.Get<int>("@AppointmentFid");

                }
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });

            //    return RedirectToAction("Index");
                //  connection.Execute("SP_InsertPrescription", param, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }



        //[HttpPost]
        //public ActionResult SaveDispensing(MedicineDispensingModel model , int? AppointmentFid)
        //{
        //    try
        //    {
        //        if (model.MedicineInstructions != null && model.MedicineInstructions.Count > 0)
        //        {

        //                foreach (var instruction in model.MedicineInstructions)
        //                {
        //                    var param = new DynamicParameters();
        //                    param.Add("@AppointmentFid", AppointmentFid);
        //                    param.Add("@PatientFid", model.PatientFid);
        //                    param.Add("@MedicineDescription", instruction.Description);
        //                    param.Add("@MorningSuchana", instruction.MorningSuchana);
        //                //param.Add("@AppointmentFid", dbType: DbType.Int32, direction: ParameterDirection.Output);
        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_MedicineDispensing_Insert", param);
        //               //int OUTPUT = param.Get<int>("@AppointmentFid");

        //            }


        //            TempData["SuccessMessage"] = "???? ????? ????????? ????? ?????? ????.";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = "????? ?????????? ??????? ????? ????.";
        //        }

        //        return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error: " + ex.Message;
        //        return RedirectToAction("YourViewName");
        //    }
        //}


        [HttpPost]
        public ActionResult SaveDispensing(MedicineDispensingModel model, int? AppointmentFid)
        {
            try
            {
                if (AppointmentFid.HasValue)
                {
                    var delParam = new DynamicParameters();
                    delParam.Add("@p_Query", $"DELETE FROM Save_AppointmentPathya WHERE AppointmentFid = {AppointmentFid}");
                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", delParam);

                    if (model.GlobalPathyaIds != null && model.GlobalPathyaIds.Any())
                    {
                        foreach(var pId in model.GlobalPathyaIds)
                        {
                            var insParam = new DynamicParameters();
                            insParam.Add("@p_Query", $"INSERT INTO Save_AppointmentPathya (AppointmentFid, PathyaId) VALUES ({AppointmentFid}, {pId})");
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", insParam);
                        }
                    }
                }

                if (model.MedicineInstructions != null)
                {
                    foreach (var instruction in model.MedicineInstructions)
                    {
                    var param = new DynamicParameters();
                    param.Add("@AppointmentFid", AppointmentFid);
                    param.Add("@PatientFid", model.PatientFid);
                    param.Add("@MedicineDescription", instruction.Description);
                    param.Add("@MorningSuchana", instruction.MorningSuchana);
                    // NEW
                    // ?? Pathya Details
                    param.Add("@PathyaInstruction", instruction.PathyaInstruction);
                    param.Add("@PathyaId", instruction.PathyaId); // ?? NEW
                    param.Add("@PrescriptionID", instruction.PrescriptionID);

                    // ?? SP should handle insert/update
                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_MedicineDispensing_Upsert", param);
                }
                }

                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
        }



        public ActionResult GetServices(string check , string FromPrescriptioncheck, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (FromPrescriptioncheck == "Checked")
                {
                    DynamicParameters paramService = new DynamicParameters();
                    paramService.Add("@p_Query", $"select    MF.FormID as ID ,MF.FormName as Name from MAS_Form MF left join MedicineFormula on MF.FormID = MedicineFormula.FormID where MedicineFormula.AppointmentFid = '{AppointmentFid}' ");/* where ServiceName='Health Check'*/
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
                else
                {
                    DynamicParameters paramService = new DynamicParameters();
                    paramService.Add("@p_Query", "select FormID as ID ,FormName as Name from MAS_Form");
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

        [HttpPost]
        public ActionResult SaveGenerateBill(Billing Bill, int? AppointmentFid)
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

        [HttpPost]
        public ActionResult SaveAndCloseEncounter(List<ClsPrescription> paymentList,string ISOutStandingAmt)
        {
            try
            {
                var appointmentId = paymentList.First().AppointmentFid;

                for (int i = 0; i < paymentList.Count(); i++)
                {
                    var FlageId = "";
                    if (ISOutStandingAmt=="Yes")
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
                    check.Add("@p_Query", "select FlageID from finalbill where Deactivate=0 and AppointmentFid='" + paymentList[i].AppointmentFid + "' and FlageID='" + FlageId + "'");

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
                    param.Add("@p_PatientFid",  paymentList[i].PatientFid);
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

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBill", param);


                    if (ISOutStandingAmt == "Yes")
                    {

                        DynamicParameters OutStanding = new DynamicParameters();

                      //  DynamicParameters OutStanding = new DynamicParameters();
                        OutStanding.Add("@p_Query", "update FinalBill set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

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
            FROM FinalBill
            WHERE AppointmentFid = {appointmentId}
            GROUP BY AppointmentFid");

                var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

                //if (summary.TotalAmount != summary.TotalPaid)
                if(summary != null )
                {
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_Outstanding", sumInsUpd);



                }


                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
      
        public ActionResult MedicinePrescription(int? AppoinmentID )
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
                ViewBag.Service = GetService;

                //if (OPDNO != null && OPDNO != 0)
                //{
                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@OPDNO", OPDNO);
                //    var result = BVGWellnessCenterDapparORM.ReturnList<OPDRegistration>("SP_Get_OPDDetails_ByOPDNO", param1).FirstOrDefault();
                //    ViewBag.OPDDetails = result

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
                    ViewBag.TodayPatients = TodayPatients;
                if (TodayPatients != null) ViewBag.AppointmentFid = TodayPatients.AppointmentFid;


                    DynamicParameters presParam = new DynamicParameters();
                    presParam.Add("@AppoinmentFid", AppoinmentID);
                    var prescriptions = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam);

                    ViewBag.PrescriptionList = prescriptions;


                    //DynamicParameters Counter = new DynamicParameters();
                    //Counter.Add("@AppoinmentFid", AppoinmentID);
                    //var CounterMedicine = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetCounterMedicineFormula", Counter);

                    //ViewBag.CounterMedicineList = CounterMedicine;


                    DynamicParameters presRParam = new DynamicParameters();
                    presRParam.Add("@AppoinmentFid", AppoinmentID);
                    var prescriptionsResult = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionRESULTByOPDNO", presRParam);
                    ViewBag.MedicineInstructions = prescriptionsResult;



                    DynamicParameters Dispensing = new DynamicParameters();
                    Dispensing.Add("@p_Query", $"SELECT * FROM Save_MedicineDispensing WHERE AppointmentFid = {AppoinmentID}");
                    var MEDICINEVIEW = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Dispensing);
                    ViewBag.ResultView = MEDICINEVIEW;
                    if (MEDICINEVIEW.Count() == 0)
                    {
                        ViewBag.ResultView = null;
                    }



                    //var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                    //    ViewBag.TodayPatients = TodayPatients;
                    //    ViewBag.AppointmentFid = TodayPatients.AppointmentFid;

                    //    DynamicParameters presParam = new DynamicParameters();
                    //presParam.Add("@OPDNO", OPDNO);
                    //var prescriptions = DapperORM.ReturnList<PrescriptionModel>("SP_GetPrescriptionByOPDNO", presParam).ToList();

                    //ViewBag.PrescriptionList = prescriptions;

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

        
        #region CounterMedicine

        [HttpPost]
        public ActionResult CounterMedicineMaster(PrescriptionModel model, string prescriptionJsonCounter, int? AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }


               List<PrescriptionModel> prescriptions = new List<PrescriptionModel>();
                if (!string.IsNullOrEmpty(prescriptionJsonCounter))
                {
                    prescriptions = JsonConvert.DeserializeObject<List<PrescriptionModel>>(prescriptionJsonCounter);
                }

                foreach (var item in prescriptions)
                {
                    var param = new DynamicParameters();
                    param.Add("@PrescriptionID", item.PrescriptionID > 0 ? item.PrescriptionID : 0);

                    param.Add("@FormID", item.FormID);
                    param.Add("@PackageID", item.PackageID);
                    param.Add("@NumberLeftID", item.NumberLeftID);
                    param.Add("@FormulaID", item.FormulaID);
                    param.Add("@DoseNumberID", item.DoseNumberID);
                    param.Add("@DoseID", item.QuantityIDDose);

                    param.Add("@TimeID", item.TimeID);
                    //param.Add("@FluidVehicleID", item.FluidVehicleID);
                    //param.Add("@MixingData", item.MixingDataJson);
                    param.Add("@MixingDataFluid", item.MixingDataFluid);
                    param.Add("@AppointmentFid", AppointmentFid);
                    //param.Add("@MedicineDate", item.MedicineDate);
                    param.Add("@DaysNumber", item.DaysNumber);
                    param.Add("@AppointmentFid", AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);

                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertCounterMedicinePrescription", param);
                    int outputFid = param.Get<int>("@AppointmentFid");

                }
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });

                //    return RedirectToAction("Index");
                //  connection.Execute("SP_InsertPrescription", param, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult SaveCounterResult(MedicineDispensingModel model, int? AppointmentFid)
        {
            try
            {
                if (model.MedicineInstructions != null && model.MedicineInstructions.Count > 0)
                {

                    foreach (var instruction in model.MedicineInstructions)
                    {
                        var param = new DynamicParameters();
                        param.Add("@AppointmentFid", AppointmentFid);
                        param.Add("@PatientFid", model.PatientFid);
                        param.Add("@MedicineDescription", instruction.Description);
                        param.Add("@MorningSuchana", instruction.MorningSuchana);
                        param.Add("@PrescriptionID", instruction.PrescriptionID);
                        //param.Add("@AppointmentFid", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_CounterMedcineResult_Insert", param);
                        //int OUTPUT = param.Get<int>("@AppointmentFid");

                    }


                    TempData["SuccessMessage"] = "Counter prescription saved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No data to save.";
                }

                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
        }

        #endregion COnterMedicine

        
        [HttpPost]
        public ActionResult AdvisedTherapy(AdvisedTherapySubmitVM model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                if (string.IsNullOrEmpty(model.AdvisedTherapyJson) || model.AdvisedTherapyJson == "[]")
                {
                    TempData["Error"] = "Please add at least one advised therapy.";
                    return RedirectToAction("AayurvedEHR", new { AppoinmentID = model.AppointmentFid });
                }

                var list = JsonConvert.DeserializeObject<List<AdvisedTherapyVM>>(model.AdvisedTherapyJson);

                int appointmentId = 0;

                foreach (var item in list)
                {
                    var param = new DynamicParameters();
                    param.Add("@ATId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    param.Add("@AppointmentFid", model.AppointmentFid);
                    param.Add("@PatientFid", model.PatientFid);
                    param.Add("@ServiceFid", item.ServiceFid);
                    param.Add("@CreatedBy", 1);
                    param.Add("@ServiceCategories", item.ServiceCategories);
                    param.Add("@DaysNumber", item.DaysNumber);
                    param.Add("@Frequency", item.Frequency);
                    param.Add("@InstructionPatinet", item.InstructionPatinet);
                    param.Add("@InstructionTherapist", item.InstructionTherapist);
                    param.Add("@Consent", item.Consent);

                    param.Add("@OutAppointmentFid", dbType: DbType.Int32, direction: ParameterDirection.Output);


                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_Insert_AdvisedTherapy", param);
                    int atId = param.Get<int>("@ATId");
                    item.ATId = atId;
                    appointmentId = param.Get<int>("@OutAppointmentFid");
                }
                return RedirectToAction("AayurvedEHR" ,  new { AppoinmentID = appointmentId });


       

                //    return RedirectToAction("Index");
                //  connection.Execute("SP_InsertPrescription", param, commandType: CommandType.StoredProcedure);
    }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

       
        //[HttpPost]
        //public ActionResult CasePaper(EHR model)
        //{
        //    try
        //    {
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }

        //        model.FamilyHistoryIds = model.FamilyHistoryIdsList != null
        //  ? string.Join(",", model.FamilyHistoryIdsList)
        //  : "";
        //        model.LabTestsAdvisedIds = model.LabTestsAdvisedIdsList != null
        //? string.Join(",", model.LabTestsAdvisedIdsList)
        //: "";

        //        model.ImagingAdvisedIds = model.ImagingAdvisedIdsList != null
        //? string.Join(",", model.ImagingAdvisedIdsList)
        //: "";


        //        int casePaperId = 0;


        //        DynamicParameters param = new DynamicParameters();

        //        // ================= VITALS =================
        //       // param.Add("@AppointmentFid", model.AppointmentFid);
        //        param.Add("@Height", model.Height);
        //        param.Add("@Weight", model.Weight);
        //        param.Add("@BMI", model.BMI);

        //        param.Add("@Pulse", model.Pulse);
        //        param.Add("@BloodPressure", model.BloodPressure);
        //        param.Add("@TemperatureCasepaper", model.TemperatureCasepaper);
        //        param.Add("@TempUnitCP", model.TempUnitCP);
        //        param.Add("@RespiratoryRate", model.RespiratoryRate);
        //        param.Add("@SpO2", model.SpO2);


        //        // ================= PAST HISTORY =================
        //        param.Add("@ChronicIllnessId", null);
        //        param.Add("@ChronicIllnessDuration", null);
        //        param.Add("@ChronicIllnessDurationUnit", null);

        //        param.Add("@PastSurgeryId", null);
        //        param.Add("@PastSurgeryDuration", null);
        //        param.Add("@PastSurgeryDurationUnit", null);

        //        param.Add("@TraumaHistoryId", null);
        //        param.Add("@TraumaHistoryDuration", null);
        //        param.Add("@TraumaHistoryDurationUnit", null);

        //        param.Add("@AllergyId", null);
        //        param.Add("@AllergyHistoryDuration", null);
        //        param.Add("@AllergyHistoryDurationUnit", null);

        //        param.Add("@LongTermMedicationId", null);
        //        param.Add("@LongTermMedicationDuration", null);
        //        param.Add("@LongTermMedicationDurationUnit", null);

        //        param.Add("@FamilyHistoryIds", model.FamilyHistoryIds);
        //        param.Add("@LabTestsAdvisedIds", model.LabTestsAdvisedIds);
        //        param.Add("@ImagingAdvisedIds",model.ImagingAdvisedIds);

        //        // ================= COMMA IDS =================
        //        //param.Add("@FamilyHistoryIds", string.Join(",", model.FamilyHistoryIds ?? new List<int>()));
        //        //param.Add("@LabTestsAdvisedIds", string.Join(",", model.LabTestsAdvisedIds ?? new List<int>()));
        //        //param.Add("@ImagingAdvisedIds", string.Join(",", model.ImagingAdvisedIds ?? new List<int>()));


        //        // ================= MENSTRUAL =================
        //        param.Add("@MenarcheAge", model.MenarcheAge);
        //        param.Add("@LMP", model.LMP);
        //        param.Add("@CycleRegular", model.CycleRegular);
        //        param.Add("@MENSTRUALDurationC", model.MENSTRUALDurationC);
        //        param.Add("@MENSTRUALDurationUnit", model.MENSTRUALDurationUnit);

        //        param.Add("@AmountOfFlow", model.AmountOfFlow ?? "");
        //        param.Add("@MenstrualPain", model.MenstrualPain ?? "");
        //        param.Add("@ObstetricHistory", model.ObstetricHistory ?? "");


        //        // ================= FOLLOWUP =================
        //        param.Add("@FollowUpDate", model.FollowUpDate);
        //        param.Add("@ExpectedOutcome", model.ExpectedOutcome ?? "");


        //        // ================= RED FLAGS =================
        //        param.Add("@RF_FeverIncrease", model.RF_FeverIncrease);
        //        param.Add("@RF_SeverePain", model.RF_SeverePain);
        //        param.Add("@RF_SuddenWorsening", model.RF_SuddenWorsening);
        //        param.Add("@RF_VomitingBleeding", model.RF_VomitingBleeding);
        //        param.Add("@RF_DizzinessFainting", model.RF_DizzinessFainting);
        //        param.Add("@RF_NoImprovement", model.RF_NoImprovement);
        //        param.Add("@RF_Other", model.RF_Other ?? "");


        //        // OUTPUT
        //        param.Add("@CasePaperId", model.CasePaperId, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
        //        param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
        //        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaper", param);
        //        int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;

        //        casePaperId = param.Get<int>("@CasePaperId");


        //        // ================= COMPLAINT LOOP =================
        //        if (model.Complaints != null)
        //        {
        //            foreach (var c in model.Complaints)
        //            {
        //                DynamicParameters p = new DynamicParameters();

        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@ComplaintId", c.ComplaintId);
        //                p.Add("@DurationC", c.DurationC);
        //                p.Add("@DurationUnitId", c.DurationUnitId);
        //                p.Add("@Onset", c.Onset ?? "");
        //                p.Add("@NatureOfIllnessId", c.NatureOfIllnessId);
        //                p.Add("@ProgressionId", c.ProgressionId);
        //                p.Add("@AssociatedSymptomsId", c.AssociatedSymptomsId != null ? string.Join(",", c.AssociatedSymptomsId) : null);
        //                p.Add("@AggravatingFactorsId", c.AggravatingFactorsId != null ? string.Join(",", c.AggravatingFactorsId) : null);
        //                p.Add("@RelievingFactorsId", c.RelievingFactorsId != null ? string.Join(",", c.RelievingFactorsId) : null);
        //                p.Add("@PreviousTreatment", c.PreviousTreatment ?? "");
        //                p.Add("@PreviousTreatmentOther", c.PreviousTreatmentOther);
        //                p.Add("@ResponsePreviousTreatmentId", c.ResponsePreviousTreatmentId);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperComplaint", p);
        //            }
        //        }


        //        // ? Chronic Illness
        //        foreach (var item in model.ChronicIllnessList ?? new List<ChronicIllnessModel>())
        //        {
        //            if (item.ChronicIllnessId.HasValue)
        //            {
        //                var p = new DynamicParameters();
        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@IllnessId", item.ChronicIllnessId);
        //                p.Add("@Duration", item.Duration ?? 0);
        //                p.Add("@DurationUnit", item.DurationUnit ?? 0);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_ChronicIllness", p);
        //            }
        //        }

        //        foreach (var item in model.PastSurgeryList ?? new List<PastSurgeryModel>())
        //        {
        //            if (item.PastSurgeryId.HasValue)
        //            {
        //                var p = new DynamicParameters();
        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@SurgeryId", item.PastSurgeryId);
        //                p.Add("@Duration", item.PastSurgeryDuration ?? 0);
        //                p.Add("@DurationUnit", item.PastSurgeryDurationUnit ?? 0);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_PastSurgery", p);
        //            }
        //        }

        //        foreach (var item in model.TraumaHistoryList ?? new List<TraumaHistoryModel>())
        //        {
        //            if (item.TraumaHistoryId.HasValue)
        //            {
        //                var p = new DynamicParameters();
        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@TraumaId", item.TraumaHistoryId);
        //                p.Add("@Duration", item.TraumaHistoryDuration ?? 0);
        //                p.Add("@DurationUnit", item.TraumaHistoryDurationUnit ?? 0);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Trauma", p);
        //            }
        //        }


        //        foreach (var item in model.AllergyList ?? new List<AllergyModel>())
        //        {
        //            if (item.AllergyId.HasValue)
        //            {
        //                var p = new DynamicParameters();
        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@AllergyId", item.AllergyId);
        //                p.Add("@Duration", item.AllergyHistoryDuration ?? 0);
        //                p.Add("@DurationUnit", item.AllergyHistoryDurationUnit ?? 0);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Allergy", p);
        //            }
        //        }


        //        foreach (var item in model.LongTermMedicationList ?? new List<LongTermMedicationModel>())
        //        {
        //            if (item.LongTermMedicationId.HasValue)
        //            {
        //                var p = new DynamicParameters();
        //                p.Add("@CasePaperId", casePaperId);
        //                p.Add("@MedicationId", item.LongTermMedicationId);
        //                p.Add("@Duration", item.LongTermMedicationDuration ?? 0);
        //                p.Add("@DurationUnit", item.LongTermMedicationDurationUnit ?? 0);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Medication", p);
        //            }
        //        }



        //        return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
        //    }
        //    catch (Exception EX)
        //    {
        //        return RedirectToAction("ErrorPage", "Home");
        //    }
        //}





        [HttpPost]
        public ActionResult CasePaper(EHR model)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                if (model.FamilyHistoryList != null)
                {
                    model.FamilyHistoryIdsList = model.FamilyHistoryList
                        .Where(x => x.FamilyHistoryIds != null)
                        .SelectMany(x => x.FamilyHistoryIds)
                        .ToList();
                    model.Relation = string.Join(",", model.FamilyHistoryList.Select(x => x.Relation));
                }

                model.FamilyHistoryIds = model.FamilyHistoryIdsList != null
                    ? string.Join(",", model.FamilyHistoryIdsList) : "";
                model.LabTestsAdvisedIds = model.LabTestsAdvisedIdsList != null
                    ? string.Join(",", model.LabTestsAdvisedIdsList) : "";
                model.ImagingAdvisedIds = model.ImagingAdvisedIdsList != null
                    ? string.Join(",", model.ImagingAdvisedIdsList) : "";

                int casePaperId = 0;
                DynamicParameters param = new DynamicParameters();

                // ================= VITALS =================

                param.Add("@Height", model.Height);
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@Weight", model.Weight);
                param.Add("@BMI", model.BMI);
                param.Add("@Pulse", model.Pulse);
                param.Add("@BloodPressure", model.BloodPressure);
                param.Add("@TemperatureCasepaper", model.TemperatureCasepaper);
                param.Add("@TempUnitCP", model.TempUnitCP);
                param.Add("@RespiratoryRate", model.RespiratoryRate);
                param.Add("@SpO2", model.SpO2);

                // ================= PAST HISTORY =================
                param.Add("@ChronicIllnessId", null);
                param.Add("@ChronicIllnessDuration", null);
                param.Add("@ChronicIllnessDurationUnit", null);
                param.Add("@PastSurgeryId", null);
                param.Add("@PastSurgeryDuration", null);
                param.Add("@PastSurgeryDurationUnit", null);
                param.Add("@TraumaHistoryId", null);
                param.Add("@TraumaHistoryDuration", null);
                param.Add("@TraumaHistoryDurationUnit", null);
                param.Add("@AllergyId", null);
                param.Add("@AllergyHistoryDuration", null);
                param.Add("@AllergyHistoryDurationUnit", null);
                param.Add("@LongTermMedicationId", null);
                param.Add("@LongTermMedicationDuration", null);
                param.Add("@LongTermMedicationDurationUnit", null);
                param.Add("@FamilyHistoryIds", model.FamilyHistoryIds);
                param.Add("@Relation", model.Relation ?? "");
                param.Add("@LabTestsAdvisedIds", model.LabTestsAdvisedIds);
                param.Add("@ImagingAdvisedIds", model.ImagingAdvisedIds);

                // ================= MENSTRUAL =================
                param.Add("@MenarcheAge", model.MenarcheAge);
                param.Add("@LMP", model.LMP);
                param.Add("@CycleRegular", model.CycleRegular);
                param.Add("@MENSTRUALDurationC", model.MENSTRUALDurationC);
                param.Add("@MENSTRUALDurationUnit", model.MENSTRUALDurationUnit);
                param.Add("@AmountOfFlow", model.AmountOfFlow ?? "");
                param.Add("@MenstrualPain", model.MenstrualPain ?? "");
                param.Add("@ObstetricHistory", model.ObstetricHistory ?? "");

                // ================= FOLLOWUP =================
                param.Add("@FollowUpDate", model.FollowUpDate);
                param.Add("@ExpectedOutcome", model.ExpectedOutcome ?? "");

                // ================= RED FLAGS =================
                param.Add("@RF_FeverIncrease", model.RF_FeverIncrease);
                param.Add("@RF_SeverePain", model.RF_SeverePain);
                param.Add("@RF_SuddenWorsening", model.RF_SuddenWorsening);
                param.Add("@RF_VomitingBleeding", model.RF_VomitingBleeding);
                param.Add("@RF_DizzinessFainting", model.RF_DizzinessFainting);
                param.Add("@RF_NoImprovement", model.RF_NoImprovement);
                param.Add("@RF_Other", model.RF_Other ?? "");

                // OUTPUT
                param.Add("@CasePaperId", model.CasePaperId, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, direction: ParameterDirection.InputOutput);
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaper", param);
                int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;
                casePaperId = param.Get<int>("@CasePaperId");

                // Delete existing complaints for this case paper to avoid duplicates/orphaned records
                if (casePaperId > 0)
                {
                    DynamicParameters deleteParams = new DynamicParameters();
                    deleteParams.Add("@p_Query", $"DELETE FROM CasePaperComplaints WHERE CasePaperId = {casePaperId}");
                    BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", deleteParams);
                }

                // ================= COMPLAINT LOOP =================
                if (model.Complaints != null)
                {
                    foreach (var c in model.Complaints)
                    {

                        if ((c.ComplaintId == 0 || c.ComplaintId == null) &&
           (c.DurationC == 0 || c.DurationC == null) &&
           (c.DurationUnitId == 0 || c.DurationUnitId == null) &&
           string.IsNullOrWhiteSpace(c.Onset) &&
           (c.NatureOfIllnessId == 0 || c.NatureOfIllnessId == null) &&
           (c.ProgressionId == 0 || c.ProgressionId == null) &&
           ((c.AssociatedSymptomsId == null || c.AssociatedSymptomsId.Length == 0) || c.AssociatedSymptomsId == null) &&
           ((c.AggravatingFactorsId == null || c.AggravatingFactorsId.Length == 0) || c.AggravatingFactorsId == null) &&
           ((c.RelievingFactorsId == null || c.RelievingFactorsId.Length == 0) || c.RelievingFactorsId == null) &&
           !c.PreviousTreatment.HasValue &&
           string.IsNullOrWhiteSpace(c.PreviousTreatmentOther) &&
           (c.ResponsePreviousTreatmentId == 0 || c.ResponsePreviousTreatmentId == null))
                        {
                            continue; // ?? skip
                        }


                        DynamicParameters p = new DynamicParameters();
                        p.Add("@CasePaperId", casePaperId);
                        p.Add("@ComplaintId", c.ComplaintId);
                        p.Add("@DurationC", c.DurationC);
                        p.Add("@DurationUnitId", c.DurationUnitId);
                        p.Add("@Onset", c.Onset ?? "");
                        p.Add("@NatureOfIllnessId", c.NatureOfIllnessId);
                        p.Add("@ProgressionId", c.ProgressionId);
                        p.Add("@AssociatedSymptomsId", c.AssociatedSymptomsId != null ? string.Join(",", c.AssociatedSymptomsId) : null);
                        p.Add("@AggravatingFactorsId", c.AggravatingFactorsId != null ? string.Join(",", c.AggravatingFactorsId) : null);
                        p.Add("@RelievingFactorsId", c.RelievingFactorsId != null ? string.Join(",", c.RelievingFactorsId) : null);
                        p.Add("@PreviousTreatment", c.PreviousTreatment);
                        p.Add("@PreviousTreatmentOther", c.PreviousTreatmentOther);
                        p.Add("@ResponsePreviousTreatmentId", c.ResponsePreviousTreatmentId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperComplaint", p);
                    }
                }

                // ========== UPDATE & DELETE FOR CHRONIC ILLNESS ==========
                // First, delete records that are not in the submitted list
                var submittedIds = model.ChronicIllnessList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    // Get existing IDs from database
                    var existingIds = GetExistingChronicIllnessIds(casePaperId);

                    // Delete records that are not in submitted list
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_ChronicIllness", p);
                    }
                }

                // Now Insert or Update each item
                foreach (var item in model.ChronicIllnessList ?? new List<ChronicIllnessModel>())
                {
                    if (item.ChronicIllnessId.HasValue && item.ChronicIllnessId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            // UPDATE existing record
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@IllnessId", item.ChronicIllnessId);
                            p.Add("@Duration", item.Duration ?? 0m);
                            p.Add("@DurationUnit", item.DurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_ChronicIllness", p);
                        }
                        else
                        {
                            // INSERT new record
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@IllnessId", item.ChronicIllnessId);
                            p.Add("@Duration", item.Duration ?? 0m);
                            p.Add("@DurationUnit", item.DurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_ChronicIllness", p);
                        }
                    }
                }

                // ========== UPDATE & DELETE FOR PAST SURGERY ==========
                submittedIds = model.PastSurgeryList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    var existingIds = GetExistingPastSurgeryIds(casePaperId);
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_PastSurgery", p);
                    }
                }

                foreach (var item in model.PastSurgeryList ?? new List<PastSurgeryModel>())
                {
                    if (item.PastSurgeryId.HasValue && item.PastSurgeryId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@SurgeryId", item.PastSurgeryId);
                            p.Add("@Duration", item.PastSurgeryDuration ?? 0m);
                            p.Add("@DurationUnit", item.PastSurgeryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_PastSurgery", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@SurgeryId", item.PastSurgeryId);
                            p.Add("@Duration", item.PastSurgeryDuration ?? 0m);
                            p.Add("@DurationUnit", item.PastSurgeryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_PastSurgery", p);
                        }
                    }
                }

                // ========== UPDATE & DELETE FOR TRAUMA ==========
                submittedIds = model.TraumaHistoryList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    var existingIds = GetExistingTraumaIds(casePaperId);
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_Trauma", p);
                    }
                }

                foreach (var item in model.TraumaHistoryList ?? new List<TraumaHistoryModel>())
                {
                    if (item.TraumaHistoryId.HasValue && item.TraumaHistoryId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@TraumaId", item.TraumaHistoryId);
                            p.Add("@Duration", item.TraumaHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.TraumaHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_Trauma", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@TraumaId", item.TraumaHistoryId);
                            p.Add("@Duration", item.TraumaHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.TraumaHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Trauma", p);
                        }
                    }
                }

                // ========== UPDATE & DELETE FOR ALLERGY ==========
                submittedIds = model.AllergyList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    var existingIds = GetExistingAllergyIds(casePaperId);
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_Allergy", p);
                    }
                }

                foreach (var item in model.AllergyList ?? new List<AllergyModel>())
                {
                    if (item.AllergyId.HasValue && item.AllergyId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@AllergyId", item.AllergyId);
                            p.Add("@Duration", item.AllergyHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.AllergyHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_Allergy", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@AllergyId", item.AllergyId);
                            p.Add("@Duration", item.AllergyHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.AllergyHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Allergy", p);
                        }
                    }
                }

                // ========== UPDATE & DELETE FOR MEDICATION ==========
                submittedIds = model.LongTermMedicationList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    var existingIds = GetExistingMedicationIds(casePaperId);
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_Medication", p);
                    }
                }

                foreach (var item in model.LongTermMedicationList ?? new List<LongTermMedicationModel>())
                {
                    if (item.LongTermMedicationId.HasValue && item.LongTermMedicationId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@MedicationId", item.LongTermMedicationId);
                            p.Add("@Duration", item.LongTermMedicationDuration ?? 0m);
                            p.Add("@DurationUnit", item.LongTermMedicationDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_Medication", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@MedicationId", item.LongTermMedicationId);
                            p.Add("@Duration", item.LongTermMedicationDuration ?? 0m);
                            p.Add("@DurationUnit", item.LongTermMedicationDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_Medication", p);
                        }
                    }
                }

                // ========== FAMILY HISTORY - Delete & Re-insert ==========
                if (casePaperId > 0)
                {
                    var pDelete = new DynamicParameters();
                    pDelete.Add("@p_Query", $"DELETE FROM CasePaper_FamilyHistory WHERE CasePaperId = {casePaperId}");
                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", pDelete);
                }

                foreach (var item in model.FamilyHistoryList ?? new List<FamilyHistoryModel>())
                {
                    if (item.FamilyHistoryIds != null)
                    {
                        foreach (var diseaseId in item.FamilyHistoryIds)
                        {
                            if (diseaseId > 0)
                            {
                                var p = new DynamicParameters();
                                p.Add("@CasePaperId", casePaperId);
                                p.Add("@FamilyHistoryId", diseaseId);
                                p.Add("@Relation", item.Relation ?? "");
                                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_FamilyHistory", p);
                            }
                        }
                    }
                }
                if (Request.Form["isCasePaperOnly"] == "true")
                {
                    return RedirectToAction("Appointment", "Appointment", new { area = "Patient" });
                }
                else
                {
                    return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
                }

                //return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
            catch (Exception EX)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        // Helper methods to get existing IDs
        private List<int> GetExistingChronicIllnessIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_ChronicIllness WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingPastSurgeryIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_PastSurgery WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingTraumaIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_Trauma WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingAllergyIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_Allergy WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingMedicationIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_Medication WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingFamilyHistoryIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_FamilyHistory WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }
        [HttpPost]
        public JsonResult DeleteAdvisedTherapy(int id)
        {
            var param = new DynamicParameters();
            param.Add("@ATId", id);

            BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
                "SP_Delete_AdvisedTherapy_ById",
                param
            );

            return Json(true);
        }

        [HttpPost]
        public JsonResult DeletePrescription(int id)
        {
            var param = new DynamicParameters();
            param.Add("@PrescriptionID", id);

            BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
                "SP_DeletePrescription",
                param
            );


            return Json(true);
        }

        [HttpPost]
        [Route("Patient/AayurvedEHR/DeleteCounterPrescription")]
        public JsonResult DeleteCounterPrescription(int id)
        {
            var param = new DynamicParameters();
            param.Add("@PrescriptionID", id);

            BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
                "SP_DeleteCounterMedicine",
                param
            );

            return Json(true);
        }

        //[HttpPost]
        //public ActionResult DeleteAdvisedTherapy(int id)
        //{
        //    try
        //    {
        //        var param = new DynamicParameters();
        //        param.Add("@ATId", id);

        //        BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
        //            "SP_Delete_AdvisedTherapy",
        //            param
        //        );

        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet]
        public ActionResult GetReport(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return HttpNotFound();

            
            filePath = HttpUtility.UrlDecode(filePath);

            string fullPath = filePath;
            if (!filePath.Contains("C:\\"))
            {
                fullPath = Path.Combine("C:\\Wellness\\UserReports\\", filePath.TrimStart('\\'));
            }
            
            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound("File not found on server." + fullPath);

            
            var ext = Path.GetExtension(fullPath).ToLower();
            string mimeType = "application/octet-stream"; // Default

            if (ext == ".jpg" || ext == ".jpeg") mimeType = "image/jpeg";
            else if (ext == ".png") mimeType = "image/png";
            else if (ext == ".pdf") mimeType = "application/pdf";
            else if (ext == ".mp4") mimeType = "video/mp4";

            
            return File(fullPath, mimeType);
        }
        //[HttpGet]
        //public ActionResult GetReport(string filePath)
        //{
        //    filePath = HttpUtility.UrlDecode(filePath);

        //    var fullPath = Server.MapPath("~/UserReports/" + filePath);

        //    if (!System.IO.File.Exists(fullPath))
        //        return HttpNotFound("File not found: " + fullPath);

        //    var ext = Path.GetExtension(fullPath).ToLower();
        //    string mimeType = "application/octet-stream";

        //    if (ext == ".jpg" || ext == ".jpeg") mimeType = "image/jpeg";
        //    else if (ext == ".png") mimeType = "image/png";
        //    else if (ext == ".pdf") mimeType = "application/pdf";

        //    return File(fullPath, mimeType);
        //}

                public ActionResult DownloadMobileReport(string filePath)
        {
            string fullPath = filePath;
            if (!filePath.Contains("C:\\"))
            {
                fullPath = System.IO.Path.Combine("C:\\Wellness\\UserReports\\", filePath.TrimStart('\\', '/'));
            }
            if (System.IO.File.Exists(fullPath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                string fileName = System.IO.Path.GetFileName(fullPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return HttpNotFound("File not found on server.");
        }

        [HttpGet]
        public JsonResult SearchPatient(string searchTerm)
        {
            if (Session["UserID"] == null)
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            DynamicParameters param = new DynamicParameters();
            param.Add("@p_Query", $@"
        SELECT TOP 20
            P.PatientFid,
            CONCAT(P.FirstName, ' ', P.LName) AS FullName,
            P.ContactNumber,
            P.UHIDNO,
            P.Age,
            P.Gender,
            CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
            PA.AppointmentFid,
            CONVERT(varchar, PA.AppoinmentDate, 105) AS FDate
        FROM Patient P
        LEFT JOIN PatientAppoinment PA 
            ON PA.PatientFid = P.PatientFid
            AND PA.AppointmentFid = (
                SELECT TOP 1 AppointmentFid 
                FROM PatientAppoinment 
                WHERE PatientFid = P.PatientFid 
                ORDER BY AppointmentFid DESC
            )
        LEFT JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
        WHERE 
            P.FirstName + ' ' + P.LName LIKE '%{searchTerm}%'
            OR P.ContactNumber LIKE '%{searchTerm}%'
            OR P.UHIDNO LIKE '%{searchTerm}%'
        ORDER BY P.PatientFid DESC
    ");

            var result = BVGWellnessCenterDapparORM
                .ReturnList<dynamic>("sp_ExecuteQuery", param)
                .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPatientAppointments(int patientFid)
        {
            if (Session["UserID"] == null)
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            DynamicParameters param = new DynamicParameters();
            param.Add("@p_Query", $@"
        SELECT 
            PA.AppointmentFid,
            CONVERT(varchar, PA.AppoinmentDate, 105) AS AppoinmentDate,
            PA.SloteStartTime,
            PA.SloteEndTime,
            PA.Status,
            PA.EncounterStatus,
            CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
            MD.DepartmentName,
            MC.ClinicName
        FROM PatientAppoinment PA
        INNER JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
        LEFT JOIN MAS_Department MD ON MD.DepartmentID = UM.DepartmentID
        INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
        WHERE PA.PatientFid = {patientFid}
        ORDER BY PA.AppointmentFid DESC
    ");

            var result = BVGWellnessCenterDapparORM
                .ReturnList<dynamic>("sp_ExecuteQuery", param)
                .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AppointmentList(int? PatientFid)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home", new { area = "" });

            if (PatientFid == null || PatientFid == 0)
                return RedirectToAction("AayurvedEHRList");

            // Patient Info
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_Query", $@"
        SELECT 
            P.PatientFid,
            CONCAT(P.FirstName, ' ', P.LName) AS PatientFullName,
            P.ContactNumber,
            P.Age,
            P.Gender,
            P.BloodGroup
        FROM Patient P
        WHERE P.PatientFid = {PatientFid}
    ");
            ViewBag.PatientInfo = BVGWellnessCenterDapparORM
                .ReturnList<dynamic>("sp_ExecuteQuery", param)
                .FirstOrDefault();

            // All Appointments of this Patient
            DynamicParameters param2 = new DynamicParameters();
            param2.Add("@p_Query", $@"
        SELECT 
            PA.AppointmentFid,
            PA.AppoinmentDate,
            PA.SloteStartTime,
            PA.SloteEndTime,
            PA.Status,
            PA.EncounterStatus,
            CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
            MD.DepartmentName,
            MC.ClinicName
        FROM PatientAppoinment PA
        INNER JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
        LEFT JOIN MAS_Department MD ON MD.DepartmentID = UM.DepartmentID
        INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
        WHERE PA.PatientFid = {PatientFid}
        ORDER BY PA.AppointmentFid DESC
    ");
            ViewBag.AppointmentList = BVGWellnessCenterDapparORM
                .ReturnList<dynamic>("sp_ExecuteQuery", param2)
                .ToList();

            ViewBag.PatientFid = PatientFid;

            return View();
        }
        [HttpGet]
        public ActionResult PatientHistory(int? OPDNO, int? PulseExamID, int? AyurvedaDiagnosisID, int? AppoinmentID)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            EHR model = new EHR();
            DynamicParameters param;

            // ==================== DROPDOWN MASTERS ====================

            // Forms
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT FormID, FormName FROM MAS_Form WHERE IsActive=1");
            ViewBag.Forms = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);

            // Formulas
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT FormulaID, FormulaName FROM MAS_Formula WHERE IsActive=1");
            ViewBag.FormulasMain = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // Packages
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT PackageID, PackageName FROM MAS_Package");
            ViewBag.Packages = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // Quantities
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT QuantityID, QuantityName FROM MAS_Quantity");
            ViewBag.Quantities = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // FluidQuantities
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT QuantityID, QuantityName FROM MAS_Quantity");
            ViewBag.FluidQuantities = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // Time
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT TimeID, TimeName FROM MAS_Time");
            ViewBag.Time = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // NumberList
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT id AS ID, NumberValue FROM MAS_NumberList");
            ViewBag.NumberLeft = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT id AS ID, NumberValue FROM MAS_NumberList");
            ViewBag.DaysNumber = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // NumberMList
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT id AS ID, NumberQuantity FROM MAS_NumberMList");
            ViewBag.NumberRight = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT id AS ID, NumberQuantity FROM MAS_NumberMList");
            ViewBag.DoseNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT id AS ID, NumberQuantity FROM MAS_NumberMList");
            ViewBag.FluidNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // Fluid Vehicle
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT VehicleID, VehicleName FROM MAS_FluidVehicle");
            ViewBag.FluidVehicle = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // CasePaper Masters
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ComplaintMaster WHERE IsActive = 1");
            ViewBag.Complaints = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_DurationUnitMaster WHERE IsActive = 1");
            ViewBag.DurationUnits = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_NatureOfIllnessMaster WHERE IsActive = 1");
            ViewBag.NatureList = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, TreatmentName as Name FROM Mas_PreviousTreatment WHERE IsActive = 1");
            ViewBag.PreviousTreatments = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ProgressionMaster WHERE IsActive = 1");
            ViewBag.Progressions = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_AssociatedSymptoms WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Symptoms = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_AggravatingFactors WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Aggravating = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_RelievingFactors WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Relieving = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ResponsePreviousTreatment WHERE IsActive = 1");
            ViewBag.ResponseTreatment = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_ChronicIllness WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.ChronicIllness = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_PastSurgery WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.PastSurgery = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_TraumaHistory WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Trauma = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_Allergy WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Allergy = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_LongTermMedication WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Medication = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM Mas_FamilyHistory WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.FamilyHistory = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM MAS_LabTestsAdvised WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.LabTests = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Id, Name FROM MAS_ImagingAdvised WHERE IsActive = 1 ORDER BY Name ASC");
            ViewBag.Imaging = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT DiseaseId AS ID, DiseaseName AS Name FROM Mas_Disease WHERE IsActive = 1");
            ViewBag.Disease = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT Fid, InstructionName, Description FROM MAS_SuchanaMaster WHERE isActive = 1");
            ViewBag.Suchana = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

            // Services
            DynamicParameters paramService = new DynamicParameters();
            paramService.Add("@p_Query", "SELECT ServiceFid AS id, CONCAT(ServiceName,'',MedicinePacking) AS Name FROM mas_service WHERE Deactivate=0 AND [Group] <> 'Appointment'");
            ViewBag.AdviseServices = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);

            // Medicine
            param = new DynamicParameters();
            param.Add("@p_Query", "SELECT ServiceFid AS ID, MedicineName AS Name FROM Mas_Medicine WHERE Status = 'Active' AND Deactivate = 0");
            ViewBag.MedicineList = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);

            // ==================== APPOINTMENT SPECIFIC DATA ====================

            if (AppoinmentID != null && AppoinmentID != 0)
            {
                // ? FIX: Status = 'Check In' AND today date ?????
                // ???? PatientHistory ????? PAST appointment ????
                // ?????? Status 'Completed' ???? ??? date ???? ????
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", $@"
            SELECT 
                PA.*, 
                P.*, 
                CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                MC.*,
                CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName
            FROM PatientAppoinment PA
            INNER JOIN Patient P ON PA.PatientFid = P.PatientFid
            INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
            INNER JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
            WHERE PA.AppointmentFid = {AppoinmentID}
        ");

                var TodayPatients = BVGWellnessCenterDapparORM
                    .ReturnList<EHR>("sp_ExecuteQuery", param1)
                    .FirstOrDefault();

                if (TodayPatients == null)
                {
                    ViewBag.TodayPatients = null;
                    return View(model);
                }

                ViewBag.TodayPatients = TodayPatients;
                if (TodayPatients != null) ViewBag.AppointmentFid = TodayPatients.AppointmentFid;
                model = TodayPatients;

                var patientFid = TodayPatients.PatientFid;
                int currentAppId = AppoinmentID ?? 0;

                // ==================== PAST VISIT HISTORY ====================
                // ? FIX: Status filter ????? — ???? visits ?????
                DynamicParameters pastVisitParam = new DynamicParameters();
                pastVisitParam.Add("@p_Query", $@"
            SELECT 
                PA.AppointmentFid,
                PA.AppoinmentDate,
                PA.SloteStartTime,
                PA.SloteEndTime,
                PA.Status,
                PA.EncounterStatus,
                CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
                MD.DepartmentName,
                MC.ClinicName
            FROM PatientAppoinment PA
            INNER JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
            LEFT JOIN MAS_Department MD ON MD.DepartmentID = UM.DepartmentID
            INNER JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
            WHERE PA.PatientFid = {patientFid}
              AND PA.AppointmentFid != {currentAppId}
            ORDER BY PA.AppointmentFid DESC
        ");

                var pastVisits = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", pastVisitParam)
                    .ToList();

                ViewBag.PastVisitHistory = pastVisits;

                // ==================== LAST FOLLOW UP DATE ====================
                string lastDateQuery = $@"
            SELECT TOP 1 FDate AS LastDateTime
            FROM PatientAppoinment 
            WHERE PatientFid = {patientFid} 
              AND AppointmentFid < {currentAppId}
            ORDER BY AppointmentFid DESC
        ";

                var lastDateData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", new DynamicParameters(new { p_Query = lastDateQuery }))
                    .FirstOrDefault();

                ViewBag.LastFollowUpDate = (lastDateData != null && lastDateData.LastDateTime != null)
                    ? ((DateTime)lastDateData.LastDateTime).ToString("dd-MM-yyyy hh:mm tt")
                    : "No Previous Record";

                // ==================== PRESCRIPTION (Form dropdown override) ====================
                DynamicParameters paramPresService = new DynamicParameters();
                paramPresService.Add("@p_Query", $@"
            SELECT MF.FormID AS ID, MF.FormName AS Name 
            FROM MAS_Form MF 
            LEFT JOIN MedicineFormula ON MF.FormID = MedicineFormula.FormID 
            WHERE MedicineFormula.AppointmentFid = '{AppoinmentID}' 
              AND ISNULL(MedicineFormula.IsActive, 1) = 1
        ");
                ViewBag.Service = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramPresService);

                // ==================== REPORTS ====================
                List<UserReportModel> reports = new List<UserReportModel>();
                ViewBag.AppointmentReports = new List<UserReportModel>();
                // ? ????? — int directly ToString() ???
                var userId = TodayPatients.PatientFid.ToString();

                if (!string.IsNullOrEmpty(userId) && userId != "0")
                {
                    DynamicParameters reportParam = new DynamicParameters();
                    reportParam.Add("@p_Query", $@"
                        SELECT * FROM (
                            SELECT ur.Fid, ur.UserId, ur.ReportType, ur.ReportFile1, ur.ReportFile2, ur.ReportFile3, ur.ReportFile4, ur.ReportFile5, ur.ReportTitle, ur.Date, ur.Deactivate 
                            FROM UserReports ur INNER JOIN Patient p ON ur.UserId = p.MUid 
                            WHERE ur.Deactivate = 0 AND p.PatientFid = '{userId}'
                            UNION ALL
                            SELECT uwr.Fid, uwr.UserId, uwr.ReportType, uwr.ReportFile1, uwr.ReportFile2, uwr.ReportFile3, uwr.ReportFile4, uwr.ReportFile5, uwr.ReportTitle, uwr.Date, uwr.Deactivate 
                            FROM [JYAMDB].[dbo].[UserWiseReports] uwr INNER JOIN Patient p ON uwr.UserId = p.MUid 
                            WHERE uwr.Deactivate = 0 AND p.PatientFid = '{userId}'
                        ) as AllReports
                        ORDER BY Date DESC
                    ");
                    reports = BVGWellnessCenterDapparORM
                        .ReturnList<UserReportModel>("sp_ExecuteQuery", reportParam)
                        .ToList();

                    DynamicParameters appReportParam = new DynamicParameters();
                    appReportParam.Add("@p_Query", $@"
                        SELECT ur.* 
                        FROM UserReports ur
                        INNER JOIN AppointmentReportsMapping arm ON ur.Fid = arm.UserReportFid
                        WHERE ur.Deactivate = 0 
                        AND arm.AppointmentFid = '{AppoinmentID ?? 0}'
                        ORDER BY ur.Date DESC
                    ");
                    ViewBag.AppointmentReports = BVGWellnessCenterDapparORM
                        .ReturnList<UserReportModel>("sp_ExecuteQuery", appReportParam)
                        .ToList();
                }

                // ==================== PULSE EXAM ====================
                var fidQuery = new DynamicParameters();
                fidQuery.Add("@p_Query", $"SELECT Fid FROM EHRPulseExam WHERE AppointmentFid = {AppoinmentID}");
                var pulseExam = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", fidQuery).FirstOrDefault();
                PulseExamID = pulseExam?.Fid ?? 0;
                ViewBag.EightPlusID = PulseExamID;

                string spPrescription = "SP_GetPrescriptionByOPDNO";
                string spPrescriptionResult = "SP_GetPrescriptionRESULTByOPDNO";
                string spCounterFormula = "SP_GetCounterMedicineFormula";
                string spCounterResult = "SP_GetPrescriptionRESULTForCounterMedicine";
                string tblSaveCounterMedicineResult = "Save_CounterMedicineResult";
                string tblSaveMedicineDispensing = "Save_MedicineDispensing";
                string tblAdvisedTherapy = "Tbl_AdvisedTherapy";
                string spName = "Sp_GetFullEHR_ByAppointment";
                string complaintsTable = "CasePaperComplaints";
                string tblChronicIllness = "CasePaper_ChronicIllness";
                string tblPastSurgery = "CasePaper_PastSurgery";
                string tblTrauma = "CasePaper_Trauma";
                string tblAllergy = "CasePaper_Allergy";
                string tblMedication = "CasePaper_Medication";
                string tblFamilyHistory = "CasePaper_FamilyHistory";

                if (AppoinmentID.HasValue)
                {
                    string queryDept = $@"SELECT MD.DepartmentName FROM PatientAppoinment PA INNER JOIN UserMaster UM ON PA.DoctorFid = UM.UserId LEFT JOIN MAS_Department MD ON UM.DepartmentID = MD.DepartmentID WHERE PA.AppointmentFid = {AppoinmentID.Value}";
                    var deptParam = new DynamicParameters();
                    deptParam.Add("@p_Query", queryDept);
                    var deptName = BVGWellnessCenterDapparORM.ReturnList<string>("sp_ExecuteQuery", deptParam).FirstOrDefault()?.ToLower() ?? "";

                    if (deptName.Contains("homeopathy"))
                    {
                        spPrescription = "SP_GetPrescriptionByOPDNO_Homeopathy";
                        spPrescriptionResult = "SP_GetPrescriptionRESULTByOPDNO_Homeopathy";
                        spCounterFormula = "SP_GetCounterMedicineFormula_Homeopathy";
                        spCounterResult = "SP_GetPrescriptionRESULTForCounterMedicine_Homeopathy";
                        tblSaveCounterMedicineResult = "Save_CounterMedicineResult_Homeopathy";
                        tblSaveMedicineDispensing = "Save_MedicineDispensing_Homeopathy";
                        tblAdvisedTherapy = "Tbl_AdvisedTherapy_Homeopathy";
                        spName = "Sp_GetFullEHR_ByAppointmentHomopathy";
                        complaintsTable = "CasePaperComplaints_Homeopathy";
                        tblChronicIllness = "Homeopathy_CasePaper_ChronicIllness";
                        tblPastSurgery = "Homeopathy_CasePaper_PastSurgery";
                        tblTrauma = "Homeopathy_CasePaper_Trauma";
                        tblAllergy = "Homeopathy_CasePaper_Allergy";
                        tblMedication = "Homeopathy_CasePaper_Medication";
                        tblFamilyHistory = "CasePaper_FamilyHistoryHomeopathy";
                    }
                    else if (deptName.Contains("yoga"))
                    {
                        spName = "Sp_GetFullEHR_ByAppointmentYoga";
                        complaintsTable = "CasePaperComplaintsYogaEHR";
                        tblAdvisedTherapy = "AdvisedTherapyYoga";
                        tblFamilyHistory = "CasePaper_FamilyHistoryYoga";
                    }
                }
                // ==================== AYURVEDA DIAGNOSIS ====================
                var diagQuery = new DynamicParameters();
                diagQuery.Add("@p_Query", $"SELECT AyurvedaDiagnosisID FROM AyurvedaDiagnosis WHERE AppointmentFid = {AppoinmentID}");
                var ayurDiag = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", diagQuery).FirstOrDefault();
                AyurvedaDiagnosisID = ayurDiag?.AyurvedaDiagnosisID ?? 0;
                ViewBag.AyurvedaDiagnosisID = AyurvedaDiagnosisID;

                // ==================== PRESCRIPTIONS ====================
                DynamicParameters presParam = new DynamicParameters();
                presParam.Add("@AppoinmentFid", AppoinmentID);
                ViewBag.PrescriptionList = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>(spPrescription, presParam);

                DynamicParameters presRParam = new DynamicParameters();
                presRParam.Add("@AppoinmentFid", AppoinmentID);
                ViewBag.MedicineInstructions = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>(spPrescriptionResult, presRParam);

                // ==================== COUNTER MEDICINE ====================
                DynamicParameters counter = new DynamicParameters();
                counter.Add("@AppoinmentFid", AppoinmentID);
                ViewBag.CounterMedicineList = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>(spCounterFormula, counter);

                DynamicParameters counterResult = new DynamicParameters();
                counterResult.Add("@AppoinmentFid", AppoinmentID);
                ViewBag.CounterFormulaView = BVGWellnessCenterDapparORM.ReturnList<PrescriptionModel>(spCounterResult, counterResult);

                DynamicParameters medicineView = new DynamicParameters();
                medicineView.Add("@p_Query", $"SELECT * FROM {tblSaveCounterMedicineResult} WHERE AppointmentFid = {AppoinmentID} AND IsActive = 1");
                var medicineViewResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", medicineView);
                ViewBag.CounterResultView = medicineViewResult.Any() ? medicineViewResult : null;

                // ==================== SUCHANA ====================
                DynamicParameters param11 = new DynamicParameters();
                param11.Add("@p_Query", "SELECT Fid, InstructionName, Description, FormID FROM MAS_SuchanaMaster WHERE IsActive = 1");
                var allSuchana = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param11).ToList();
                ViewBag.AllSuchna = allSuchana;

                ViewBag.SuchanaGrouped = allSuchana
                    .GroupBy(x => x.FormID)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => g.Select(s => new { s.Fid, s.InstructionName, s.Description }).ToList()
                    );

                // ==================== MEDICINE DISPENSING ====================
                DynamicParameters dispensing = new DynamicParameters();
                dispensing.Add("@p_Query", $"SELECT * FROM {tblSaveMedicineDispensing} WHERE AppointmentFid = '{AppoinmentID}' AND IsActive = 1");
                var medicineDispensing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", dispensing);
                ViewBag.ResultView = medicineDispensing.Any() ? medicineDispensing : null;

                // ==================== ADVISED THERAPY ====================
                var advisedTherapy = new DynamicParameters();
                advisedTherapy.Add("@p_Query", $@"
            SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, 
                   AT.Frequency, AT.InstructionPatinet, AT.InstructionTherapist, AT.Consent 
            FROM {tblAdvisedTherapy} AT 
            INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
            WHERE AT.AppointmentFid = '{AppoinmentID}'
        ");
                ViewBag.AdvisedTherapyResult = BVGWellnessCenterDapparORM.ReturnList<AdvisedTherapyVM>("sp_ExecuteQuery", advisedTherapy).ToList();

                // ==================== BILL ====================
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@p_AppointmentFid", AppoinmentID);
                ViewBag.GenerateBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();

                // ==================== FULL EHR DATA ====================
                DynamicParameters paramFull = new DynamicParameters();
                paramFull.Add("@AppointmentFid", AppoinmentID);
                var fullData = BVGWellnessCenterDapparORM.ReturnList<EHR>(spName, paramFull).FirstOrDefault();

                if (fullData != null)
                {
                    model = fullData;
                    model.UserReports = reports;

                    // Disease IDs split
                    if (!string.IsNullOrEmpty(model.DiseaseID))
                        model.DiseaseIDList = model.DiseaseID
                            .Split(',')
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(int.Parse).ToList();

                    // FamilyHistory split
                    if (!string.IsNullOrEmpty(model.FamilyHistoryIds))
                        model.FamilyHistoryIdsList = model.FamilyHistoryIds
                            .Split(',')
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(int.Parse).ToList();

                    // ImagingAdvised split
                    if (!string.IsNullOrEmpty(model.ImagingAdvisedIds))
                        model.ImagingAdvisedIdsList = model.ImagingAdvisedIds
                            .Split(',')
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(int.Parse).ToList();

                    // LabTestsAdvised split
                    if (!string.IsNullOrEmpty(model.LabTestsAdvisedIds))
                        model.LabTestsAdvisedIdsList = model.LabTestsAdvisedIds
                            .Split(',')
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(int.Parse).ToList();

                    // Complaints
                    if (model.CasePaperId != 0)
                    {
                        DynamicParameters compParam = new DynamicParameters();
                        compParam.Add("@p_Query", $"SELECT * FROM {complaintsTable} WHERE CasePaperId = '{model.CasePaperId}'");
                        model.Complaints = BVGWellnessCenterDapparORM
                            .ReturnList<ComplaintVM>("sp_ExecuteQuery", compParam)
                            .ToList() ?? new List<ComplaintVM>();
                    }
                    else
                    {
                        model.Complaints = new List<ComplaintVM>();
                    }

                    // Past History (Chronic, Surgery, Trauma, Allergy, Medication)
                    if (model.CasePaperId != 0)
                    {
                        DynamicParameters chronicParam = new DynamicParameters();
                        chronicParam.Add("@p_Query", $@"
                    SELECT Id, CasePaperId, IllnessId AS ChronicIllnessId, Duration, DurationUnit 
                    FROM {tblChronicIllness} WHERE CasePaperId = '{model.CasePaperId}'
                ");
                        model.ChronicIllnessList = BVGWellnessCenterDapparORM
                            .ReturnList<ChronicIllnessModel>("sp_ExecuteQuery", chronicParam)
                            .ToList() ?? new List<ChronicIllnessModel>();

                        DynamicParameters surgeryParam = new DynamicParameters();
                        surgeryParam.Add("@p_Query", $@"
                    SELECT Id, CasePaperId, SurgeryId AS PastSurgeryId, 
                           Duration AS PastSurgeryDuration, DurationUnit AS PastSurgeryDurationUnit 
                    FROM {tblPastSurgery} WHERE CasePaperId = '{model.CasePaperId}'
                ");
                        model.PastSurgeryList = BVGWellnessCenterDapparORM
                            .ReturnList<PastSurgeryModel>("sp_ExecuteQuery", surgeryParam)
                            .ToList() ?? new List<PastSurgeryModel>();

                        DynamicParameters traumaParam = new DynamicParameters();
                        traumaParam.Add("@p_Query", $@"
                    SELECT Id, CasePaperId, TraumaId AS TraumaHistoryId, 
                           Duration AS TraumaHistoryDuration, DurationUnit AS TraumaHistoryDurationUnit 
                    FROM {tblTrauma} WHERE CasePaperId = '{model.CasePaperId}'
                ");
                        model.TraumaHistoryList = BVGWellnessCenterDapparORM
                            .ReturnList<TraumaHistoryModel>("sp_ExecuteQuery", traumaParam)
                            .ToList() ?? new List<TraumaHistoryModel>();

                        DynamicParameters allergyParam = new DynamicParameters();
                        allergyParam.Add("@p_Query", $@"
                    SELECT Id, CasePaperId, AllergyId, 
                           Duration AS AllergyHistoryDuration, DurationUnit AS AllergyHistoryDurationUnit 
                    FROM {tblAllergy} WHERE CasePaperId = '{model.CasePaperId}'
                ");
                        model.AllergyList = BVGWellnessCenterDapparORM
                            .ReturnList<AllergyModel>("sp_ExecuteQuery", allergyParam)
                            .ToList() ?? new List<AllergyModel>();

                        DynamicParameters medicationParam = new DynamicParameters();
                        medicationParam.Add("@p_Query", $@"
                    SELECT Id, CasePaperId, MedicationId AS LongTermMedicationId, 
                           Duration AS LongTermMedicationDuration, DurationUnit AS LongTermMedicationDurationUnit 
                    FROM {tblMedication} WHERE CasePaperId = '{model.CasePaperId}'
                ");
                        model.LongTermMedicationList = BVGWellnessCenterDapparORM
                            .ReturnList<LongTermMedicationModel>("sp_ExecuteQuery", medicationParam)
                            .ToList() ?? new List<LongTermMedicationModel>();
                        // Load Family History List from database
                        DynamicParameters familyParam = new DynamicParameters();
                        familyParam.Add("@p_Query", $"SELECT Id, CasePaperId, FamilyHistoryId, Relation FROM {tblFamilyHistory} WHERE CasePaperId = '{model.CasePaperId}'");
                        var familyRows = BVGWellnessCenterDapparORM.ReturnList<FamilyHistoryModel>("sp_ExecuteQuery", familyParam).ToList();
                        model.FamilyHistoryList = familyRows
                            .GroupBy(x => x.Relation ?? "")
                            .Select(g => new FamilyHistoryModel
                            {
                                Relation = g.Key,
                                FamilyHistoryIds = g.Where(x => x.FamilyHistoryId.HasValue).Select(x => x.FamilyHistoryId.Value).ToList()
                            }).ToList();
                    }
                    else
                    {
                        model.ChronicIllnessList = new List<ChronicIllnessModel>();
                        model.PastSurgeryList = new List<PastSurgeryModel>();
                        model.TraumaHistoryList = new List<TraumaHistoryModel>();
                        model.AllergyList = new List<AllergyModel>();
                        model.LongTermMedicationList = new List<LongTermMedicationModel>();
                        model.FamilyHistoryList = new List<FamilyHistoryModel>();
                    }
                }
            }
            else
            {
                ViewBag.TodayPatients = null;
                ViewBag.PastVisitHistory = null;
            }

            return View(model);
        }

        [HttpGet]
        public JsonResult GetServicesByGroup(int groupId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"
            SELECT 
                ServiceFid AS Id, 
                CONCAT(ServiceName, ' ', ISNULL(MedicinePacking,'')) AS Name 
            FROM Mas_Service 
            WHERE ServiceGroupID = {groupId} 
              AND Deactivate = 0 
              AND [Group] <> 'Appointment'
            ORDER BY ServiceName ASC
        ");

                // ? anonymous object ????? - DropdownItem ????
                var services = BVGWellnessCenterDapparORM
                         .ReturnList<DropdownItem>("sp_ExecuteQuery", param)
                         .ToList();

                return Json(services, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
















