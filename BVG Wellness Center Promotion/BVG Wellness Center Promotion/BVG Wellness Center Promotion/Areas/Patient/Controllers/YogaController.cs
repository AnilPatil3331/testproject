using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class YogaController : Controller
    {
        // GET: Patient/Yoga
        //public ActionResult Index()
        //{
        //    return View();
        //}
       

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


        public ActionResult YogaEHR(int? Id, int? AppoinmentID)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
       
                 //Case paper Masters 

                YOGA model = new YOGA();
                List<UserReportModel> reports = new List<UserReportModel>();

                DynamicParameters param;
                /* ================= Complaint ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_ComplaintMaster WHERE IsActive = 1");
                ViewBag.Complaints = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Duration Unit ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_DurationUnitMaster WHERE IsActive = 1");
                ViewBag.DurationUnits = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Nature Of Illness ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_NatureOfIllnessMaster WHERE IsActive = 1");
                ViewBag.NatureList = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();

                /* ================= Previous Treatment ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, TreatmentName as Name FROM Mas_PreviousTreatment WHERE IsActive = 1");
                ViewBag.PreviousTreatments = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Progression ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_ProgressionMaster WHERE IsActive = 1");
                ViewBag.Progressions = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Associated Symptoms ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_AssociatedSymptoms WHERE IsActive = 1");
                ViewBag.Symptoms = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Aggravating ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_AggravatingFactors WHERE IsActive = 1");
                ViewBag.Aggravating = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* ================= Relieving ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_RelievingFactors WHERE IsActive = 1");
                ViewBag.Relieving = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();



                /* ================= Response ================= */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_ResponsePreviousTreatment WHERE IsActive = 1");
                ViewBag.ResponseTreatment = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();

                //DynamicParameters param;

                /* Chronic Illness */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_ChronicIllness WHERE IsActive = 1");
                ViewBag.ChronicIllness = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* Past Surgery */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_PastSurgery WHERE IsActive = 1");
                ViewBag.PastSurgery = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* Trauma */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_TraumaHistory WHERE IsActive = 1");
                ViewBag.Trauma = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* Allergy */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_Allergy WHERE IsActive = 1");
                ViewBag.Allergy = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* Medication */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_LongTermMedication WHERE IsActive = 1");
                ViewBag.Medication = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();




                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM Mas_FamilyHistory WHERE IsActive = 1");
                ViewBag.FamilyHistory = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();

                
                /* Lab Tests */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM MAS_LabTestsAdvised WHERE IsActive = 1");
                ViewBag.LabTests = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                /* Imaging */
                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT Id, Name FROM MAS_ImagingAdvised WHERE IsActive = 1");
                ViewBag.Imaging = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param).ToList();


                param = new DynamicParameters();
                param.Add("@p_Query", "select DiseaseId as ID, DiseaseName as Name from Mas_Disease where IsActive = 1");
                ViewBag.Disease = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();

                // ðŸ‘‰ Prescription madhun direct load kara
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

                    // ðŸ”¥ HE IMPORTANT LINE (override)
                    ViewBag.Service = presService;
                }


                //DynamicParameters paramService = new DynamicParameters();
                //paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(ServiceName,'',MedicinePacking) as Name from mas_service where Deactivate=0");
                //var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                //ViewBag.Service = GetService;


                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var DaysNumber = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.DaysNumber = DaysNumber;


                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var Repetition = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.Repetition = Repetition;

                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var Duration = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.Duration = Duration;



                DynamicParameters paramGroup = new DynamicParameters();
                paramGroup.Add("@p_Query", $@"select ServicesGroupId as Id, ServiceGroupName as Name from ServicesGroup where IsActive = 1");
                ViewBag.ServiveGroup = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramGroup);


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
                    var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<YOGA>("sp_ExecuteQuery", param1).FirstOrDefault();
                    ViewBag.TodayPatients = TodayPatients;
                if (TodayPatients != null) ViewBag.AppointmentFid = TodayPatients.AppointmentFid;




                    //labhansh developemnt game 
                    var GamePatinetFid = TodayPatients.PatientFid;
                    DynamicParameters paramGame = new DynamicParameters();
                    paramGame.Add("@PatientFid", GamePatinetFid);
                    paramGame.Add("@Origin", "GetFullGameReport");

                    using (var multi = BVGWellnessCenterDapparORM.QueryMultiple("SP_GetFullGameReport", paramGame))
                    {
                        //var userInfo = multi.Read<dynamic>().ToList();
                        //var answersDetail = multi.Read<dynamic>().ToList();
                        //var totalScore = multi.Read<dynamic>().FirstOrDefault();
                        //var levelCounts = multi.Read<dynamic>().ToList();
                        //var sectionCounts = multi.Read<dynamic>().FirstOrDefault();
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

                    //labhansh developemnt end 






                    // ==================== REPORTS FETCH ====================
                    if (TodayPatients != null)
                    {
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
                    }


                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@AppointmentFid", AppoinmentID);

                    var list = BVGWellnessCenterDapparORM.ReturnList<YOGA>("Sp_GetAdvisedTherapy_ByAppointment", param2);

                    ViewBag.AdvisedTherapyList = list;

                    // ==================== ADVISED THERAPY ====================
                    var advisedTherapy = new DynamicParameters();
                    advisedTherapy.Add("@p_Query", $@"
            SELECT AT.ATId, S.ServiceName, AT.ServiceCategories, AT.DaysNumber, 
                   AT.Frequency, AT.InstructionPatinet, AT.InstructionTherapist, AT.Consent 
            FROM AdvisedTherapyYoga AT 
            INNER JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
            WHERE AT.AppointmentFid = '{AppoinmentID}'
        ");
                    ViewBag.AdvisedTherapyResult = BVGWellnessCenterDapparORM.ReturnList<AdvisedTherapyVM>("sp_ExecuteQuery", advisedTherapy).ToList();


                    DynamicParameters paramFull = new DynamicParameters();
                paramFull.Add("@AppointmentFid", AppoinmentID);

                var fullData = BVGWellnessCenterDapparORM.ReturnList<YOGA>("Sp_GetFullYogaEHR_ByAppointment", paramFull).FirstOrDefault();

                if (fullData != null)
                {
                    model = fullData;

                        // à¤œà¤° à¤¡à¥‡à¤Ÿà¤¾à¤¬à¥‡à¤¸à¤®à¤§à¥‚à¤¨ à¤†à¤²à¥‡à¤²à¤¾ ID 0 à¤…à¤¸à¥‡à¤² à¤¤à¤° à¤¤à¥à¤¯à¤¾à¤²à¤¾ à¤¯à¥‹à¤—à¥à¤¯ Appoinment ID à¤¦à¥à¤¯à¤¾
                        if (model.AppointmentFid == 0)
                        {
                            model.AppointmentFid = AppoinmentID.Value;
                        }

                        if (!string.IsNullOrEmpty(model.DiseaseID))
                    {
                        model.DiseaseIDList =
                          model.DiseaseID
                               .Split(',')
                               .Where(x => !string.IsNullOrWhiteSpace(x))
                               .Select(int.Parse)
                               .ToList();
                    }

                    // ðŸ”¹ 1ï¸âƒ£ FamilyHistory Split
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
                        familyParam.Add("@p_Query", $"SELECT Id, CasePaperId, FamilyHistoryId, Relation FROM CasePaper_FamilyHistoryYoga WHERE CasePaperId = '{model.CasePaperId}'");
                        var familyRows = BVGWellnessCenterDapparORM.ReturnList<YOGA.FamilyHistoryModel>("sp_ExecuteQuery", familyParam).ToList();
                        model.FamilyHistoryList = familyRows
                            .GroupBy(x => x.Relation ?? "")
                            .Select(g => new YOGA.FamilyHistoryModel
                            {
                                Relation = g.Key,
                                FamilyHistoryIds = g.Where(x => x.FamilyHistoryId.HasValue).Select(x => x.FamilyHistoryId.Value).ToList()
                            }).ToList();
                    }
                    else
                    {
                        model.FamilyHistoryList = new List<YOGA.FamilyHistoryModel>();
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


                    // ðŸ”¹ 2ï¸âƒ£ Complaints Load (If CasePaperId exists)
                    if (model.CasePaperId != 0)
                    {
                        DynamicParameters compParam = new DynamicParameters();

                        compParam.Add("@p_Query", $"SELECT * FROM CasePaperComplaintsYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");

                        var complaints = BVGWellnessCenterDapparORM
                                          .ReturnList<ComplaintVMYOGA>("sp_ExecuteQuery", compParam)
                                          .ToList();
                        model.Complaints = complaints ?? new List<ComplaintVMYOGA>();
                            // ðŸ”¹ Past History Load (If CasePaperId exists)
                            // ðŸ”¹ Past History Load (If CasePaperId exists)
                            if (model.CasePaperId != 0)
                            {
                                // âœ… Chronic Illness
                                DynamicParameters chronicParam = new DynamicParameters();
                                chronicParam.Add("@p_Query", $@"SELECT Id, CasePaperId, IllnessId AS ChronicIllnessId, Duration, DurationUnit FROM CasePaper_ChronicIllnessYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");
                                var chronic = BVGWellnessCenterDapparORM.ReturnList<YOGA.ChronicIllnessModel>("sp_ExecuteQuery", chronicParam).ToList();
                                model.ChronicIllnessList = chronic ?? new List<YOGA.ChronicIllnessModel>();

                                // âœ… Past Surgery
                                DynamicParameters surgeryParam = new DynamicParameters();
                                surgeryParam.Add("@p_Query", $@"SELECT Id, CasePaperId, SurgeryId AS PastSurgeryId, Duration AS PastSurgeryDuration, DurationUnit AS PastSurgeryDurationUnit FROM CasePaper_PastSurgeryYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");
                                var surgery = BVGWellnessCenterDapparORM.ReturnList<YOGA.PastSurgeryModel>("sp_ExecuteQuery", surgeryParam).ToList();
                                model.PastSurgeryList = surgery ?? new List<YOGA.PastSurgeryModel>();

                                // âœ… Trauma
                                DynamicParameters traumaParam = new DynamicParameters();
                                traumaParam.Add("@p_Query", $@"SELECT Id, CasePaperId, TraumaId AS TraumaHistoryId, Duration AS TraumaHistoryDuration, DurationUnit AS TraumaHistoryDurationUnit FROM CasePaper_TraumaYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");
                                var trauma = BVGWellnessCenterDapparORM.ReturnList<YOGA.TraumaHistoryModel>("sp_ExecuteQuery", traumaParam).ToList();
                                model.TraumaHistoryList = trauma ?? new List<YOGA.TraumaHistoryModel>();

                                // âœ… Allergy
                                DynamicParameters allergyParam = new DynamicParameters();
                                allergyParam.Add("@p_Query", $@"SELECT Id, CasePaperId, AllergyId, Duration AS AllergyHistoryDuration, DurationUnit AS AllergyHistoryDurationUnit FROM CasePaper_AllergyYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");
                                var allergy = BVGWellnessCenterDapparORM.ReturnList<YOGA.AllergyModel>("sp_ExecuteQuery", allergyParam).ToList();
                                model.AllergyList = allergy ?? new List<YOGA.AllergyModel>();

                                // âœ… Medication
                                DynamicParameters medicationParam = new DynamicParameters();
                                medicationParam.Add("@p_Query", $@"SELECT Id, CasePaperId, MedicationId AS LongTermMedicationId, Duration AS LongTermMedicationDuration, DurationUnit AS LongTermMedicationDurationUnit FROM CasePaper_MedicationYogaEHR WHERE CasePaperId = '{model.CasePaperId}'");
                                var med = BVGWellnessCenterDapparORM.ReturnList<YOGA.LongTermMedicationModel>("sp_ExecuteQuery", medicationParam).ToList();
                                model.LongTermMedicationList = med ?? new List<YOGA.LongTermMedicationModel>();
                            }
                            else
                            {
                                model.ChronicIllnessList = new List<YOGA.ChronicIllnessModel>();
                                model.PastSurgeryList = new List<YOGA.PastSurgeryModel>();
                                model.TraumaHistoryList = new List<YOGA.TraumaHistoryModel>();
                                model.AllergyList = new List<YOGA.AllergyModel>();
                                model.LongTermMedicationList = new List<YOGA.LongTermMedicationModel>();
                                model.FamilyHistoryList = new List<YOGA.FamilyHistoryModel>();
                            }
                        }
                    else
                    {
                            model.AppointmentFid = AppoinmentID.Value;
                            if (TodayPatients != null)
                            {
                                model.PatientFid = TodayPatients.PatientFid;
                                model.Gender = TodayPatients.Gender;
                                model.Age = TodayPatients.Age;
                                model.UHIDNO = TodayPatients.UHIDNO;
                                model.PatientName = TodayPatients.PatientName;
                            }
                            model.Complaints = new List<ComplaintVMYOGA>();
                            model.FamilyHistoryList = new List<YOGA.FamilyHistoryModel>();
                    }
                }
            }

                if (AppoinmentID.HasValue)
                {
                    DynamicParameters reportParam = new DynamicParameters();
                    reportParam.Add("@p_Query", $"SELECT * FROM UserReports WHERE AppointmentId = {AppoinmentID} AND Deactivate = 0 ORDER BY Date DESC");
                    var patientMobileReports = BVGWellnessCenterDapparORM.ReturnList<BVG_Wellness_Center_Promotion.Areas.Patient.Models.PatientMobileReportModel>("sp_ExecuteQuery", reportParam);
                    ViewBag.PatientMobileReports = patientMobileReports != null ? patientMobileReports.ToList() : new List<BVG_Wellness_Center_Promotion.Areas.Patient.Models.PatientMobileReportModel>();
                }
                else
                {
                    ViewBag.PatientMobileReports = new List<BVG_Wellness_Center_Promotion.Areas.Patient.Models.PatientMobileReportModel>();
                }

                model.UserReports = reports;
                return View(model);

            }
            catch (Exception Ex)
            {
                return Content(Ex.Message);
               // return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }

        public ActionResult CheckoutEncounter(int AppointmentFid)
        {
            // à¤‡à¤¥à¥‡ à¤¤à¥à¤®à¤šà¥à¤¯à¤¾ à¤¡à¥‡à¤Ÿà¤¾à¤¬à¥‡à¤¸à¤®à¤§à¥à¤¯à¥‡ à¤¸à¥à¤Ÿà¥‡à¤Ÿà¤¸ à¤…à¤ªà¤¡à¥‡à¤Ÿ à¤•à¤°à¤£à¤¾à¤°à¥€ SP à¤•à¤¿à¤‚à¤µà¤¾ Logic à¤²à¤¿à¤¹à¤¾
            DynamicParameters param = new DynamicParameters();
            param.Add("@AppointmentFid", AppointmentFid);
            param.Add("@Status", "Check Out"); // à¤‰à¤¦à¤¾. à¤¸à¥à¤Ÿà¥‡à¤Ÿà¤¸ 'Check Out' à¤•à¤°à¤¾

            // à¤¤à¥à¤®à¤šà¥à¤¯à¤¾ à¤¡à¥€à¤¬à¥€à¤¨à¥à¤¸à¤¾à¤° à¤•à¥‰à¤² à¤•à¤°à¤¾
            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_UpdateAppointmentStatus", param);

            return RedirectToAction("AayurvedEHRList", "AayurvedEHR"); // à¤œà¤¿à¤¥à¥‡ à¤²à¤¿à¤¸à¥à¤Ÿ à¤†à¤¹à¥‡ à¤¤à¤¿à¤¥à¥‡ à¤ªà¤¾à¤ à¤µà¤¾
        }
        //[HttpPost]
        //public ActionResult CasePaper(YOGA model)
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
        //        param.Add("@AppointmentFid", model.AppointmentFid);
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
        //        param.Add("@ChronicIllnessId", model.ChronicIllnessId);
        //        param.Add("@ChronicIllnessDuration", model.ChronicIllnessDuration);
        //        param.Add("@ChronicIllnessDurationUnit", model.ChronicIllnessDurationUnit);

        //        param.Add("@PastSurgeryId", model.PastSurgeryId);
        //        param.Add("@PastSurgeryDuration", model.PastSurgeryDuration);
        //        param.Add("@PastSurgeryDurationUnit", model.PastSurgeryDurationUnit);

        //        param.Add("@TraumaHistoryId", model.TraumaHistoryId);
        //        param.Add("@TraumaHistoryDuration", model.TraumaHistoryDuration);
        //        param.Add("@TraumaHistoryDurationUnit", model.TraumaHistoryDurationUnit);

        //        param.Add("@AllergyId", model.AllergyId);
        //        param.Add("@AllergyHistoryDuration", model.AllergyHistoryDuration);
        //        param.Add("@AllergyHistoryDurationUnit", model.AllergyHistoryDurationUnit);

        //        param.Add("@LongTermMedicationId", model.LongTermMedicationId);
        //        param.Add("@LongTermMedicationDuration", model.LongTermMedicationDuration);
        //        param.Add("@LongTermMedicationDurationUnit", model.LongTermMedicationDurationUnit);

        //        param.Add("@FamilyHistoryIds", model.FamilyHistoryIds);
        //        param.Add("@LabTestsAdvisedIds", model.LabTestsAdvisedIds);
        //        param.Add("@ImagingAdvisedIds", model.ImagingAdvisedIds);

        //        // ================= COMMA IDS =================
        //        //param.Add("@FamilyHistoryIds", string.Join(",", model.FamilyHistoryIds ?? new List<int>()));
        //        //param.Add("@LabTestsAdvisedIds", string.Join(",", model.LabTestsAdvisedIds ?? new List<int>()));
        //        //param.Add("@ImagingAdvisedIds", string.Join(",", model.ImagingAdvisedIds ?? new List<int>()));


        //        // ================= MENSTRUAL =================
        //        param.Add("@MenarcheAge", model.MenarcheAge);
        //        param.Add("@LMP", model.LMP);

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
        //        param.Add("@CasePaperId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        //        param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
        //        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperYogaEHR", param);
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
        //                p.Add("@AssociatedSymptomsId", c.AssociatedSymptomsId);
        //                p.Add("@AggravatingFactorsId", c.AggravatingFactorsId);
        //                p.Add("@RelievingFactorsId", c.RelievingFactorsId);
        //                p.Add("@PreviousTreatment", c.PreviousTreatment ?? "");
        //                p.Add("@PreviousTreatmentOther", c.PreviousTreatmentOther);
        //                p.Add("@ResponsePreviousTreatmentId", c.ResponsePreviousTreatmentId);

        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperComplaintYogaEHR", p);
        //            }
        //        }

        //        return RedirectToAction("YogaEHR", new { AppoinmentID = AppointmentFid });
        //    }
        //    catch (Exception EX)
        //    {
        //        return RedirectToAction("ErrorPage", "Home");
        //    }
        //}
        [HttpPost]
        public ActionResult CasePaper(YOGA model)
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
                param.Add("@AppointmentFid", model.AppointmentFid);
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@Height", model.Height);
                param.Add("@Weight", model.Weight);
                param.Add("@BMI", model.BMI);
                param.Add("@Pulse", model.Pulse);
                param.Add("@BloodPressure", model.BloodPressure);
                param.Add("@TemperatureCasepaper", model.TemperatureCasepaper);
                param.Add("@TempUnitCP", model.TempUnitCP);
                param.Add("@RespiratoryRate", model.RespiratoryRate);
                param.Add("@SpO2", model.SpO2);

                // ================= PAST HISTORY (null â€” lists à¤µà¤¾à¤ªà¤°à¤£à¤¾à¤°) =================
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
                param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperYogaEHR", param);
                int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;
                casePaperId = param.Get<int>("@CasePaperId");

                // Delete existing complaints for this case paper to avoid duplicates/orphaned records
                if (casePaperId > 0)
                {
                    DynamicParameters deleteParams = new DynamicParameters();
                    deleteParams.Add("@p_Query", $"DELETE FROM CasePaperComplaintsYogaEHR WHERE CasePaperId = {casePaperId}");
                    BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", deleteParams);
                }

                // ================= COMPLAINT LOOP =================
                if (model.Complaints != null)
                {
                    foreach (var c in model.Complaints)
                    {
                        // âœ… Empty row skip à¤•à¤°à¤¾
                        if ((c.ComplaintId == 0 || c.ComplaintId == null) &&
                            (c.DurationC == 0 || c.DurationC == null) &&
                            (c.DurationUnitId == 0 || c.DurationUnitId == null) &&
                            string.IsNullOrWhiteSpace(c.Onset) &&
                            (c.NatureOfIllnessId == 0 || c.NatureOfIllnessId == null) &&
                            (c.ProgressionId == 0 || c.ProgressionId == null) &&
                            (c.AssociatedSymptomsId == null || c.AssociatedSymptomsId.Length == 0) &&
                            (c.AggravatingFactorsId == null || c.AggravatingFactorsId.Length == 0) &&
                            (c.RelievingFactorsId == null || c.RelievingFactorsId.Length == 0) &&
                            !c.PreviousTreatment.HasValue &&
                            string.IsNullOrWhiteSpace(c.PreviousTreatmentOther) &&
                            (c.ResponsePreviousTreatmentId == 0 || c.ResponsePreviousTreatmentId == null))
                        {
                            continue;
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
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_SaveCasePaperComplaintYogaEHR", p);
                    }
                }

                // ========== CHRONIC ILLNESS (YOGA) - Delete + Insert/Update ==========
                var submittedIds = model.ChronicIllnessList?
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id.Value)
                    .ToList() ?? new List<int>();

                if (casePaperId > 0)
                {
                    var existingIds = GetExistingChronicIllnessIds(casePaperId);
                    var idsToDelete = existingIds.Except(submittedIds).ToList();
                    foreach (var id in idsToDelete)
                    {
                        var p = new DynamicParameters();
                        p.Add("@Id", id);
                        p.Add("@CasePaperId", casePaperId);
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_ChronicIllnessYogaEHR", p);
                    }
                }

                foreach (var item in model.ChronicIllnessList ?? new List<YOGA.ChronicIllnessModel>())
                {
                    if (item.ChronicIllnessId.HasValue && item.ChronicIllnessId.Value > 0)
                    {
                        if (item.Id.HasValue && item.Id.Value > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@Id", item.Id);
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@IllnessId", item.ChronicIllnessId);
                            p.Add("@Duration", item.Duration ?? 0m);
                            p.Add("@DurationUnit", item.DurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_ChronicIllnessYogaEHR", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@IllnessId", item.ChronicIllnessId);
                            p.Add("@Duration", item.Duration ?? 0m);
                            p.Add("@DurationUnit", item.DurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_ChronicIllnessYogaEHR", p);
                        }
                    }
                }

                // ========== PAST SURGERY (YOGA) - Delete + Insert/Update ==========
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
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_PastSurgeryYogaEHR", p);
                    }
                }

                foreach (var item in model.PastSurgeryList ?? new List<YOGA.PastSurgeryModel>())
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
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_PastSurgeryYogaEHR", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@SurgeryId", item.PastSurgeryId);
                            p.Add("@Duration", item.PastSurgeryDuration ?? 0m);
                            p.Add("@DurationUnit", item.PastSurgeryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_PastSurgeryYogaEHR", p);
                        }
                    }
                }

                // ========== TRAUMA (YOGA) - Delete + Insert/Update ==========
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
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_TraumaYogaEHR", p);
                    }
                }

                foreach (var item in model.TraumaHistoryList ?? new List<YOGA.TraumaHistoryModel>())
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
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_TraumaYogaEHR", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@TraumaId", item.TraumaHistoryId);
                            p.Add("@Duration", item.TraumaHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.TraumaHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_TraumaYogaEHR", p);
                        }
                    }
                }

                // ========== ALLERGY (YOGA) - Delete + Insert/Update ==========
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
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_AllergyYogaEHR", p);
                    }
                }

                foreach (var item in model.AllergyList ?? new List<YOGA.AllergyModel>())
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
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_AllergyYogaEHR", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@AllergyId", item.AllergyId);
                            p.Add("@Duration", item.AllergyHistoryDuration ?? 0m);
                            p.Add("@DurationUnit", item.AllergyHistoryDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_AllergyYogaEHR", p);
                        }
                    }
                }

                // ========== LONG TERM MEDICATION (YOGA) - Delete + Insert/Update ==========
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
                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Delete_MedicationYogaEHR", p);
                    }
                }

                foreach (var item in model.LongTermMedicationList ?? new List<YOGA.LongTermMedicationModel>())
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
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Update_MedicationYogaEHR", p);
                        }
                        else
                        {
                            var p = new DynamicParameters();
                            p.Add("@CasePaperId", casePaperId);
                            p.Add("@MedicationId", item.LongTermMedicationId);
                            p.Add("@Duration", item.LongTermMedicationDuration ?? 0m);
                            p.Add("@DurationUnit", item.LongTermMedicationDurationUnit ?? 0);
                            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_MedicationYogaEHR", p);
                        }
                    }
                }

                // ========== FAMILY HISTORY - Delete & Re-insert ==========
                if (casePaperId > 0)
                {
                    var pDelete = new DynamicParameters();
                    pDelete.Add("@p_Query", $"DELETE FROM CasePaper_FamilyHistoryYoga WHERE CasePaperId = {casePaperId}");
                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", pDelete);
                }

                foreach (var item in model.FamilyHistoryList ?? new List<YOGA.FamilyHistoryModel>())
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
                                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_Insert_FamilyHistoryYoga", p);
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
                    return RedirectToAction("YogaEHR", new { AppoinmentID = AppointmentFid }); // à¤•à¤¿à¤‚à¤µà¤¾ à¤œà¥‹ ID à¤µà¥‡à¤°à¤¿à¤à¤¬à¤² à¤…à¤¸à¥‡à¤² à¤¤à¥‹
                }

                //return RedirectToAction("YogaEHR", new { AppoinmentID = AppointmentFid });
            }
            catch (Exception EX)
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        private List<int> GetExistingChronicIllnessIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_ChronicIllnessYogaEHR WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingPastSurgeryIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_PastSurgeryYogaEHR WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingTraumaIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_TraumaYogaEHR WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingAllergyIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_AllergyYogaEHR WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingMedicationIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_MedicationYogaEHR WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        private List<int> GetExistingFamilyHistoryIds(int casePaperId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $"SELECT Id FROM CasePaper_FamilyHistoryYoga WHERE CasePaperId = {casePaperId}");
            var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
            return result.Select(x => (int)x.Id).ToList();
        }

        [HttpPost]
        public ActionResult MentalSufferingsExam(YOGA model)
        {
            try
            {
                
                    DynamicParameters param = new DynamicParameters();

                    param.Add("@ExamID", model.ExamID);
                    param.Add("@AppointmentFid", model.AppointmentFid);
                    param.Add("@DescriptionUnAware", model.DescriptionUnAware);
                    param.Add("@DescriptionEgoism", model.DescriptionEgoism);
                    param.Add("@DescriptionDesire", model.DescriptionDesire);
                    param.Add("@DescriptionHatred", model.DescriptionHatred);
                    param.Add("@Fearofdeath", model.Fearofdeath);

                //   int id = con.Query<int>("SP_SaveMentalSufferingsExam",  param,  commandType: CommandType.StoredProcedure  ).FirstOrDefault();
                param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_SaveMentalSufferingsExam", param);
                int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;
                            
                 TempData["Success"] = "Saved Successfully";
                return RedirectToAction("YogaEHR", new { AppoinmentID = AppointmentFid,area="Patient" });
             
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("YogaEHR", new { AppoinmentID = model.AppointmentFid, area = "Patient" });
            }

        }



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
                    return RedirectToAction("YogaEHR", new { AppoinmentID = model.AppointmentFid });
                }

                var list = JsonConvert.DeserializeObject<List<AdvisedTherapyVM>>(model.AdvisedTherapyJson);

                int appointmentId = model.AppointmentFid;

                foreach (var item in list)
                {


                    // ðŸ”¥ à¤«à¤•à¥à¤¤ à¤¨à¤µà¥€à¤¨ à¤°à¥‡à¤•à¥‰à¤°à¥à¤¡à¥à¤¸ (à¤œà¥à¤¯à¤¾à¤‚à¤šà¤¾ ATId 0 à¤†à¤¹à¥‡) INSERT à¤•à¤°à¤¾
                    if (item.ATId == 0)
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

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_InsertAdvisedTherapyYoga", param);

                        int atId = param.Get<int>("@ATId");
                        item.ATId = atId;
                        appointmentId = param.Get<int>("@OutAppointmentFid");
                    }
                    else
                    {
                        // ðŸ”¥ (à¤ªà¤°à¥à¤¯à¤¾à¤¯à¥€) à¤œà¤° à¤¤à¥à¤®à¥à¤¹à¤¾à¤²à¤¾ à¤œà¥à¤¨à¥‡ à¤°à¥‡à¤•à¥‰à¤°à¥à¤¡à¥à¤¸ UPDATE à¤•à¤°à¤¾à¤¯à¤šà¥‡ à¤…à¤¸à¤¤à¥€à¤² à¤¤à¤° à¤‡à¤¥à¥‡ Sp_UpdateAdvisedTherapyYoga à¤•à¥‰à¤² à¤•à¤°à¤¾.
                        // à¤¸à¤§à¥à¤¯à¤¾ à¤«à¤•à¥à¤¤ à¤¨à¤µà¥€à¤¨ à¤‡à¤¨à¥à¤¸à¤°à¥à¤Ÿ à¤•à¤°à¤¾à¤¯à¤šà¥‡ à¤…à¤¸à¤²à¥à¤¯à¤¾à¤¨à¥‡ à¤¹à¥‡ à¤°à¤¿à¤•à¤¾à¤®à¥‡ à¤ à¥‡à¤µà¤¾.
                    }

                }

                    return RedirectToAction("YogaEHR", new { AppoinmentID = appointmentId });
            }
         catch (Exception Ex)
            {
                return View();
            }
         
        }



        [HttpPost]
        public ActionResult YogaDiagnosis(YOGA model)
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

                param.Add("@Fid", model.YogaDiagnosisID);
                param.Add("@AppointmentFid", model.AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@DiseaseID", model.DiseaseID);

                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_YogaDiagnosis_Crud", param);

                int AppointmentFid = param.Get<int?>("@AppointmentFid") ?? 0;

                return RedirectToAction("YogaEHR", new { AppoinmentID = AppointmentFid });

            }
            catch (Exception ex)
            {
                TempData["error"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public JsonResult DeleteAdvisedTherapy(int id)
        {
            var param = new DynamicParameters();
            param.Add("@ATId", id);

            BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
                "SP_Delete_AdvisedTherapyYoga_ById",
                param
            );

            return Json(true);
        }

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

    }
}





