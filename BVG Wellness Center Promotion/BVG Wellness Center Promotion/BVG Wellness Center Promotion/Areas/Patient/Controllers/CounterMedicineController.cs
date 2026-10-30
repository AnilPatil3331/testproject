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
    public class CounterMedicineController : Controller
    {
        // GET: Patient/CounterMedicine
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CounterMedicine(int? PatientFid, int? medicine5PFid, string Source, string FlageId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param;


                param = new DynamicParameters();

                //DropDown List Data 
                param.Add("@p_Query", "SELECT FormID, FormName FROM MAS_Form WHERE IsActive=1");
                var data = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
                ViewBag.Forms = data;

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


                // Numbers(वेगवेगळ्या dropdown साठी वेगळं assign)
                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var NumberLeft = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.NumberLeft = NumberLeft;


                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var NumberRight = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.NumberRight = NumberRight;



                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var DoseNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.DoseNumbers = DoseNumbers;


                // Fluid Vehicle
                param = new DynamicParameters();
                param.Add("@p_Query", "select VehicleID, VehicleName from MAS_FluidVehicle");
                var FluidVehicle = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.FluidVehicle = FluidVehicle;


                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var FluidNumbers = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.FluidNumbers = FluidNumbers;


                param = new DynamicParameters();
                param.Add("@p_Query", "SELECT QuantityID, QuantityName FROM MAS_Quantity");
                var FluidQuantities = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.FluidQuantities = FluidQuantities;


                param = new DynamicParameters();
                param.Add("@p_Query", "select id as ID, NumberValue from MAS_NumberList");
                var DaysNumber = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.DaysNumber = DaysNumber;




                // Suchna dropdown
                param = new DynamicParameters();
                param.Add("@p_Query", $@"SELECT Fid, InstructionName , Description FROM MAS_SuchanaMaster where isActive = 1");
                var Suchana = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param).ToList();
                ViewBag.Suchana = Suchana;


                //services
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(ServiceName,'',MedicinePacking) as Name from mas_service where Deactivate=0");
                var GetService = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Service = GetService;



                param = new DynamicParameters();
                param.Add("@p_Query", "select ServiceFid as ID, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0 ");/* where ServiceName='Health Check'*/
                var Medicine = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
                ViewBag.MedicineList = Medicine;


                //string deptID = "";
                //string deptValue = "";

                //// 1. Try to get DepartmentID from the doctor who created the prescription/bill (using FlageId)
                //if (!string.IsNullOrEmpty(FlageId))
                //{
                //    DynamicParameters deptParam = new DynamicParameters();
                //    deptParam.Add("@p_Query", $@"
                //        SELECT TOP 1 UM.DepartmentID 
                //        FROM UserMaster UM 
                //        INNER JOIN Tbl_MedicinePrescription MP ON UM.UserId = MP.CreatedBy 
                //        WHERE MP.FlageId = '{FlageId}' AND UM.DepartmentID IS NOT NULL");

                //    var deptResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", deptParam).FirstOrDefault();
                //    if (deptResult != null && deptResult.DepartmentID != null)
                //    {
                //        deptID = deptResult.DepartmentID.ToString().Trim();
                //    }
                //}

                //// 2. Fallback: Logged-in user's department
                //if (string.IsNullOrEmpty(deptID) && Session["UserID"] != null)
                //{
                //    DynamicParameters deptParam = new DynamicParameters();
                //    deptParam.Add("@p_Query", $"SELECT DepartmentID FROM UserMaster WHERE UserId = '{Session["UserID"]}'");

                //    var deptResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", deptParam).FirstOrDefault();
                //    if (deptResult != null && deptResult.DepartmentID != null)
                //    {
                //        deptID = deptResult.DepartmentID.ToString().Trim();
                //    }
                //}

                //// 3. Map DepartmentID to Department Name (1 = Ayurveda, 2 = Yoga, 3 = Homeopathy)
                //if (!string.IsNullOrEmpty(deptID))
                //{
                //    if (deptID == "1")
                //        deptValue = "Ayurveda";
                //    else if (deptID == "2")
                //        deptValue = "Yoga";
                //    else if (deptID == "3")
                //        deptValue = "Homeopathy";
                //}

                //// 4. Load Medicine List filtered by department
                //string medicineQuery = "select ServiceFid as ID, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0";
                //if (!string.IsNullOrEmpty(deptValue))
                //{
                //    medicineQuery += $" AND (Department = '{deptValue}' OR Department IS NULL OR Department = '')";
                //}

                //param = new DynamicParameters();
                //param.Add("@p_Query", medicineQuery);
                //var Medicine = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
                //ViewBag.MedicineList = Medicine;


                //DynamicParameters MEDICINE = new DynamicParameters();
                //MEDICINE.Add("@p_Query", "select ServiceFid as id, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0 ");/* where ServiceName='Health Check'*/
                //var MEDICINEList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", MEDICINE);
                //ViewBag.Getdata = MEDICINEList;







                // Get Medicines from Mas_Medicine table
                DynamicParameters paramMedicine = new DynamicParameters();
                paramMedicine.Add("@p_Query",
                    "SELECT ServiceFid as Id, MedicineName as Name, MedicinePacking, Charges " +
                    "FROM Mas_Service " +
                    "WHERE Servicecategory = 'Medicine' AND Status = 'Active' AND Deactivate = 0");
                var medicines = BVGWellnessCenterDapparORM.ReturnList<MedicineVM>("sp_ExecuteQuery", paramMedicine);
                ViewBag.GetMedicines = medicines;

                // Get Doctors Dropdown
                int ClinicFid = Convert.ToInt32(Session["ClinicFid"]);
                DynamicParameters Doctor = new DynamicParameters();
                Doctor.Add("@p_Query",
                    $"SELECT UserId as Id, FirstName + ' ' + LName as Name " +
                    $"FROM UserMaster " +
                    $"WHERE Deactivate=0 AND ClinicFid='{ClinicFid}' AND IsDoctor=1");
                var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", Doctor);
                ViewBag.GetDoctorName = GetDoctor ?? new List<DropdownItem>();




                ViewBag.FlagId = FlageId;
                ViewBag.Other = (Source == "other") ? "other" : "null";

                if (FlageId != null)

                {

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlageId);
                    var  GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;


                    //param = new DynamicParameters();
                    //param.Add("@p_Query", $@"select MM.MedicineName as Name, MM.ServiceFid as ID from CounterMedicineByDesk  CM left join  Mas_Medicine MM on CM.FormulaID = MM.ServiceFid where CM.FlageId ='{FlageId}'");/* where ServiceName='Health Check'*/
                    //var MedicineL = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param);
                    //ViewBag.MedicineList = MedicineL;

                    // Scenario 2: Manual patient (from Tbl_ServicesP)
                    DynamicParameters Services = new DynamicParameters();
                    Services.Add("@FlageId", FlageId);
                    var GetServicesbypatints = BVGWellnessCenterDapparORM.ReturnList<MedicinePrescriptionVM>("SP_GetCounterMedicineFormulabyCounter", Services).ToList();
                    ViewBag.CounterMedicineList = GetServicesbypatints;
                    ViewBag.PatientFid = GetServicesbypatints.FirstOrDefault()?.PatientFid;
                    ViewBag.ManualPatientId = GetServicesbypatints.FirstOrDefault()?.ManualPatientId;

                    if (ViewBag.PatientFid != null && Convert.ToInt32(ViewBag.PatientFid) != 0)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Query", $@"SELECT   
                                    P.PatientFid,
                                    CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                                    P.ContactNumber,
                                    P.Age,
                                    P.Gender,
                                    P.DateOfBirth                                
                                   FROM 
                                    Patient P                                                          
                                WHERE 
                                    P.PatientFid = {ViewBag.PatientFid}");

                        var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                        ViewBag.TodayPatients = TodayPatients;
                    }
                    else
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Query", $@"select * from MAS_ManualPatient where ManualPatientId  = {ViewBag.ManualPatientId} ");
                        var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                        ViewBag.TodayPatients = TodayPatients;
                        //ViewBag.ManualPatientId = TodayPatients.ManualPatientId;
                    }

                    // ✅ Set PatientFid from database (could be null for manual patients)

                }
                // ✅ CORRECTED: Handle Patient Data - IMPORTANT LOGIC
                else if (PatientFid != null && PatientFid > 0)
                {
                    // Scenario 1: Patient searched and found
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_Query", $@"SELECT   
                                    P.PatientFid,
                                    CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                                    P.ContactNumber,
                                    P.Age,
                                    P.Gender,
                                    P.DateOfBirth                                
                                   FROM 
                                    Patient P                                                          
                                WHERE 
                                    P.PatientFid = {PatientFid}");

                    var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                    ViewBag.TodayPatients = TodayPatients;
                    ViewBag.PatientFid = PatientFid;
                }

                else
                {

                }
                return View();
            }
            catch (Exception ex)
            {
                // Log error
                return View();
            }
        }


        [HttpGet]
        public JsonResult GetMedicineDetails(int medicineId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"SELECT ServiceFid, MedicineName, MedicinePacking, Charges  FROM Mas_Medicine WHERE ServiceFid = '{medicineId}' AND Servicecategory = 'Medicine' ");

                var medicine = BVGWellnessCenterDapparORM.ReturnList<MedicineVM>("sp_ExecuteQuery", param).FirstOrDefault();

                return Json(medicine, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public JsonResult SaveMedicines(SaveMedicineRequestVM model , PrescriptionModel PM, string prescriptionJsonCounter, int? PatientFid)
        //{
        //    try
        //    {
        //        if (model?.Medicines == null || model.Medicines.Count == 0)
        //            return Json(new { success = false, message = "No medicines received" });

        //        var manualPatient = model.ManualPatient;
        //        int lastPrescriptionId = 0;
        //        int patientId = 0;
        //        // 🔥 ONE FlageId FOR ALL ROWS
        //        string flageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

        //        foreach (var item in model.Medicines)
        //        {
        //            DynamicParameters param = new DynamicParameters();

        //            param.Add("@MedicineFid", item.MedicineFid);
        //            param.Add("@Rate", item.Rate);
        //            param.Add("@Quantity", item.Quantity);
        //            param.Add("@Dosage", item.Dosage);
        //            param.Add("@Frequency", item.Frequency);
        //            param.Add("@Route", item.Route);
        //            param.Add("@Duration", item.Duration);
        //            param.Add("@Instruction", item.Instruction);
        //            param.Add("@Amount", item.Amount);
        //            param.Add("@Note", item.Note);
        //            param.Add("@PatientID", item.PatientID);
        //            param.Add("@CreatedBy", Convert.ToInt32(Session["UserID"]));
        //            param.Add("@FlageId", flageId);



        //                //param.Add("@Name", manualPatient.PatientName);
        //                //param.Add("@MobileNo", manualPatient.MobileNo);
        //                //param.Add("@Age", manualPatient.Age);
        //                //param.Add("@Gender", manualPatient.Gender);
        //                //param.Add("@VisitedDate", manualPatient.VisitDate);

        //            param.Add("@Name", patientId == 0 ? manualPatient?.PatientName : null);
        //            param.Add("@MobileNo", patientId == 0 ? manualPatient?.MobileNo : null);
        //            param.Add("@Age", patientId == 0 ? manualPatient?.Age : null);
        //            param.Add("@Gender", patientId == 0 ? manualPatient?.Gender : null);
        //            param.Add("@VisitedDate", patientId == 0 ? manualPatient?.VisitDate : null);


        //            param.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        //            //BVGWellnessCenterDapparORM.ExcuteSp("sp_Save_ServicesForPatients", param);
        //            BVGWellnessCenterDapparORM.ExcuteSp("sp_Save_MedicinePrescription", param);

        //            lastPrescriptionId = param.Get<int>("@NewId");
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            MedicinePFid = lastPrescriptionId,
        //            FlageId = flageId
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}


        [HttpGet]
        public JsonResult GetMedicineDropdown(string flageId)
        {
            var param = new DynamicParameters();
            param.Add("@p_Query", $@"select MM.MedicineName as Name, MM.ServiceFid as ID 
                             from CounterMedicineByDesk CM
                             left join Mas_Medicine MM on CM.FormulaID = MM.ServiceFid
                             where CM.FlageId = '{flageId}' AND CM.isActive = 1");

            var list = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveMedicines(SaveMedicineRequestVM request)
        {


            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
          
                  string flageId;

                // जर model मधून FlagId आला असेल (null नाही, empty नाही)
                if (!string.IsNullOrWhiteSpace(request.FlageId) && request.FlageId != "0")
                {
                    flageId = request.FlageId;   // 🔁 existing वापर
                }
                else
                {
                    flageId = Guid.NewGuid()
                                    .ToString("N")
                                    .Substring(0, 4)
                                    .ToUpper(); // 🆕 new generate
                }




                int? patientFid = request.PatientFid;
                int? ManualPatientId = request.ManualPatientId;




                if (patientFid == null || patientFid == 0 )
                {
                    if (ManualPatientId == null || ManualPatientId == 0)
                    {

                        var patientParam = new DynamicParameters();
                        patientParam.Add("@PatientName", request.PatientName);
                        patientParam.Add("@MobileNo", request.MobileNo);
                        patientParam.Add("@Age", request.Age);
                        patientParam.Add("@Gender", request.Gender);
                        patientParam.Add("@VisitDate", request.VisitDate);

                        // 👉 OUTPUT PARAM
                        patientParam.Add("@ManualPatientId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertManualPatient", patientParam);

                        // 👉 OUTPUT VALUE
                        ManualPatientId = patientParam.Get<int>("@ManualPatientId");
                        // 🔥 IMPORTANT FIX
                        //request.ManualPatientId = ManualPatientId;
                    }
                }


                if (request.Medicines != null)
                {
                    // 🔥 SAVE MULTIPLE MEDICINES
                    foreach (var item in request.Medicines)
                    {
                        var param = new DynamicParameters();
                        param.Add("@FormID", item.FormID);
                        param.Add("@PackageID", item.PackageID);
                        param.Add("@NumberLeftID", item.NumberLeftID);
                        param.Add("@FormulaID", item.FormulaID);
                        param.Add("@DoseNumberID", item.DoseNumberID);
                        param.Add("@DoseID", item.QuantityIDDose);
                        param.Add("@TimeID", item.TimeID);
                        param.Add("@MixingDataFluid", item.MixingDataFluid);
                        param.Add("@DaysNumber", item.DaysNumber);
                        param.Add("@PatientFid", patientFid);
                        param.Add("@ManualPatientId", ManualPatientId);
                        param.Add("@FlageId", flageId);
                        //param.Add("@AppointmentFid", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertCounterMedicineByDesk", param);
                        //TempData["Success"] = "Selected prescriptions confirmed successfully!";
                        //TempData["OpenBillPopup"] = true;
                                                              
                    }

                }

               DynamicParameters medicine = new DynamicParameters();
                medicine.Add("@p_Query", $@"select MM.MedicineName as Name, MM.ServiceFid as ID from CounterMedicineByDesk  CM left join  Mas_Medicine MM on CM.FormulaID = MM.ServiceFid where CM.FlageId ='{flageId}'");/* where ServiceName='Health Check'*/
                var MedicineL = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", medicine);
                ViewBag.MedicineList = MedicineL;

                List<dynamic> GetBillList = new List<dynamic>();
                if (flageId != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", flageId);
                     GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }




                return Json(new
                {
                    success = true,
                    FlageId = flageId,
                    GenerateBill = GetBillList

                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }


        public JsonResult SearchOPD(string searchTerm)
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@searchTerm", searchTerm);
            //param.Add("@HospitalID", Session["HospitalID"]);
            var result = BVGWellnessCenterDapparORM.ReturnList<Appointment>("Sp_searchpatient", param).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ValidateMedicineByFlage(string FlageId)
        {
            // example validation

            DynamicParameters param2 = new DynamicParameters();
            param2.Add("@p_Query", $@"select MM.MedicineName as Name, MM.ServiceFid as ID from CounterMedicineByDesk  CM left join  Mas_Medicine MM on CM.FormulaID = MM.ServiceFid where CM.FlageId ='{FlageId}'");/* where ServiceName='Health Check'*/
            var MedicineL = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param2);
            ViewBag.MedicineList = MedicineL;


            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CounterMedicineMaster(SaveMedicineRequestVM VM, PrescriptionModel model, string prescriptionJsonCounter, int? patientId, string PatientName, DateTime VisitDate, string Gender, string MobileNo, int? Age)
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
                    param.Add("@AppointmentFid", patientId);
                    //param.Add("@MedicineDate", item.MedicineDate);
                    param.Add("@DaysNumber", item.DaysNumber);
                    param.Add("@AppointmentFid", patientId, DbType.Int32, ParameterDirection.InputOutput);

                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertCounterMedicinePrescription", param);
                    int outputFid = param.Get<int>("@AppointmentFid");

                }
                return RedirectToAction("AayurvedEHR", new { AppoinmentID = patientId });

                //    return RedirectToAction("Index");
                //  connection.Execute("SP_InsertPrescription", param, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////



        #region GetServicePrice
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
        #endregion

        [HttpPost]
        public ActionResult SaveGenerateBillCounter(Billing Bill, int? AppointmentFid)
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
                param.Add("@p_PatientID", Bill.PatientID);
                param.Add("@FlagId", Bill.FlagId);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateMedicineBill", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.FlagId != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", Bill.FlagId);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.AppointmentFid != 0 && Bill.AppointmentFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", Bill.AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
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
        public ActionResult SaveAndCloseBilling(List<ClsPrescription> paymentList, string ISOutStandingAmt)
        {
            try
            {
                var appointmentId = paymentList.First().AppointmentFid;
                var PatientID = paymentList.First().PatientFid;
                var BillFlageID = paymentList.First().BillFlageID;



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
                        FlageId = $"{i + 1}{paymentList[i].BillFlageID}";
                    }


                    DynamicParameters check = new DynamicParameters();
                    //check.Add("@p_Query", "select FlageID from FinalBillCounterMedicine where Deactivate=0 and  FlageID='" + FlageId + "'  and BillFlageID = ' " + BillFlageID + " '");
                    check.Add("@p_Query", "select FlageID from FinalBillCounterMedicine where Deactivate=0  and   FlageID='" + FlageId + "' and BillFlageID='" + BillFlageID + "'");
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
                    param.Add("@BillFlageID", paymentList[i].BillFlageID);
                    param.Add("@p_ReferenceNumber", paymentList[i].ReferenceNumber);
                    param.Add("@p_Amount", paymentList[i].Amount);
                    param.Add("@p_FlageID", exist != null ? exist.FlageID : FlageId);

                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBillCounterMedicine", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");

                    if (ISOutStandingAmt == "Yes")
                    {

                        DynamicParameters OutStanding = new DynamicParameters();

                        //  DynamicParameters OutStanding = new DynamicParameters();
                        OutStanding.Add("@p_Query", "update FinalBillCounterMedicine set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

                        var Standing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", OutStanding).FirstOrDefault();

                    }


                }

                // for outstanding 
                DynamicParameters outstandparam = new DynamicParameters();
                outstandparam.Add("@p_Query", $@"
            SELECT 
                BillFlageID,
                MAX(PayableAmount) AS PayableAmount,
                SUM(Amount) AS TotalPaid,
                (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount
            FROM FinalBillCounterMedicine
            WHERE BillFlageID = '{BillFlageID}'
            GROUP BY BillFlageID");

                var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

                //if (summary.TotalAmount != summary.TotalPaid)
                if (summary != null)
                {
                    // ✅ Unpaid असेल तर Outstanding table ला entry नको
                    var currentStatus = paymentList.First().PaymentStatus;
                    if (currentStatus == "Unpaid")
                    {
                        return Json(new { success = true, message = "Payments processed." });
                    }
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    sumInsUpd.Add("@AppointmentFid", appointmentId);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
                    //sumInsUpd.Add("@TotalAmount", summary.PayableAmount);

                    sumInsUpd.Add("@PatientFid", PatientID);
                    sumInsUpd.Add("@BillFlageID", BillFlageID);

                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_OutstandingcounterMedicine", sumInsUpd);



                }


                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult BillingRecordCounterMedicine(string FlagId)
        {
            try
            {

                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (FlagId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@FlagId ", FlagId);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_CounterMedicinePaymentList", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId ", FlagId); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalBillCounterMedicine where Deactivate=0 and BillFlageID ='" + FlagId + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from FinalBillCounterMedicine where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }

                var GetUnPaid = BVGWellnessCenterDapparORM.DynamicList("sp_List_CounterMedicineUnapidList");
                ViewBag.GetUnPaid = GetUnPaid;
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id, MedicineName as Name  from Mas_Medicine where Status = 'Active' and Deactivate= 0 ");/* where ServiceName='Health Check'*/
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Getdata = data;

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }

        #region GetBillRecord
        public ActionResult GetBillRecord(int? Bill_Fid, string FlagId)
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
                if (FlagId != null)
                {
                    //ViewBag.FlagId = FlagId;
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<Billing>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal1 = new DynamicParameters();
                    paramFinal1.Add("@p_Query", "select Status,DiscountAmt from FinalBillCounterMedicine where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal1).FirstOrDefault();
                    paymentStatus = FinalBill?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill?.DiscountAmt ?? "Unknown";


                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from FinalBillCounterMedicine where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                }


                return Json(new { GetBillList = ViewBag.GenerateBill, paymentStatus = paymentStatus, DiscountAmt = ViewBag.Discountamt, GetFinalbillDetails = ViewBag.GetFinalbillDetails  }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion

        #region PaidBillRecords
        public ActionResult PaidBillRecordCounter(string FlagId, string returnUrl)
        {
            
            try
            {
                ViewBag.ReturnUrl = returnUrl;
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (FlagId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@FlagId", FlagId);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_CounterMedicinePaymentList", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalBillCounterMedicine where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalBillCounterMedicine where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT BillFlageID, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalBillCounterMedicine WHERE BillFlageID ='" + FlagId + "' GROUP BY BillFlageID ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_counterPaymentpaidList");
                ViewBag.GetPaidBillList = GetPaidBillList;

                return View();
            }
            catch (Exception Ex)
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
                    GetBillList = BVGWellnessCenterDapparORM.ReturnList<ClsPrescription>("sp_List_GenerateMedicineBill", paramBill).FirstOrDefault();
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


        #region Outstanding

        public ActionResult OutstandingBillCounter()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }


                DynamicParameters paramService = new DynamicParameters();
                //                paramService.Add("@p_Query", $@"SELECT 
                //    ob.*,

                //     CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                // CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName	,
                //    MC.ClinicName,

                //    pa.AppointmentFid

                //FROM OutstandingBillCounterMedicine ob

                //LEFT JOIN PatientAppoinment pa  
                //    ON ob.AppointmentFid = pa.AppointmentFid

                //LEFT JOIN Patient p 
                //    ON pa.PatientFid = p.PatientFid

                //LEFT JOIN Mas_Clinic MC 
                //    ON pa.ClinicFid = MC.Fid

                //LEFT JOIN UserMaster UM 
                //    ON UM.UserId = pa.DoctorFid

                //WHERE ISNULL(ob.RemainingAmount, 0) > 0

                //ORDER BY CreatedDate DESC;");
                paramService.Add("@p_Query", $@"
SELECT 
    ob.Id,
    ob.BillFlageID,

    COALESCE(
        MAX(NULLIF(LTRIM(RTRIM(ISNULL(p.FirstName,'') + ' ' + ISNULL(p.LName,''))), '')),
        MAX(NULLIF(LTRIM(RTRIM(mp.PatientName)), '')),
        MAX(NULLIF(LTRIM(RTRIM(sp.Name)), '')),
        'N/A'
    ) AS PatientName,

    COALESCE(MAX(p.ContactNumber), MAX(mp.MobileNo), MAX(sp.MobileNo), 'N/A') AS MobileNo,
    COALESCE(MAX(CAST(p.Age AS VARCHAR)), MAX(CAST(mp.Age AS VARCHAR)), MAX(CAST(sp.Age AS VARCHAR)), '0') AS Age,
    COALESCE(MAX(p.Gender), MAX(mp.Gender), MAX(sp.Gender), 'N/A') AS Gender,

    -- Date & Status
    ISNULL(MAX(ob.CreatedDate), MAX(sp.VisitedDate)) AS VisitedDate,
    MAX(ob.Status) AS Status,

    -- Billing Details (येथे सर्व कॉलम्सना MAX लावले आहे जेणेकरून GROUP BY एरर येणार नाही)
    ISNULL(MAX(fsb.TotalAmount), 0) AS TotalAmount,
    ISNULL(MAX(fsb.PayableAmount), 0) AS PayableAmount,
    ISNULL(MAX(fsb.DiscountAmt), 0) AS DiscountAmt,
    ISNULL(MAX(ob.RemainingAmount), 0) AS RemainingAmount,
    ISNULL(MAX(ob.TotalPaid), 0) AS TotalPaid,

    -- Doctor Name
    ISNULL(MAX(um.FirstName + ' ' + um.LName), 'N/A') AS DoctorName,
    -- BILL ID (FIXED)
    MAX(fsb.Fid) AS Bill_Fid

FROM OutstandingBillCounterMedicine ob

LEFT JOIN CounterMedicineByDesk cm 
    ON ob.BillFlageID = cm.FlageId

-- १. Prescription टेबल
LEFT JOIN Tbl_MedicinePrescription sp ON ob.BillFlageID = sp.FlageId

-- 🔹 REGISTERED
LEFT JOIN Patient p 
    ON p.PatientFid = NULLIF(cm.PatientFid, 0)
 
-- ३. मॅन्युअल पेशंट
LEFT JOIN CounterMedicineByDesk cmd ON ob.BillFlageID = cmd.FlageId
LEFT JOIN MAS_ManualPatient mp ON cmd.ManualPatientId = mp.ManualPatientId

-- ४. बिलाची माहिती

LEFT JOIN FinalBillCounterMedicine fsb ON ob.BillFlageID = fsb.BillFlageID


-- ५. डॉक्टर माहिती
LEFT JOIN UserMaster um ON um.UserId = sp.CreatedBy

-- फिल्टर: फक्त अनपेयड आणि ज्यांची बाकी शून्य नाही अशा एन्ट्रीज
WHERE ob.Status = 'Unpaid' 
  AND ISNULL(ob.RemainingAmount, 0) > 0

GROUP BY ob.Id, ob.BillFlageID
ORDER BY ob.Id DESC;
");
                //paramService.Add("@p_Query", $@"
                //      SELECT 
                //             SP.FlageId,
                //             SP.FlageId AS FlagId,
                //             SP.FlageId AS BillFlageID,
                //             ob.Id,                                               
                //             MAX(ISNULL(P.FirstName + ' ' + P.LName, SP.Name)) AS PatientName,
                //             MAX(ISNULL(P.ContactNumber, SP.MobileNo))         AS MobileNo,
                //             MAX(ISNULL(P.Age, SP.Age))                         AS Age,
                //             MAX(ISNULL(P.Gender, SP.Gender))                   AS Gender,                                                 
                //             MAX(SP.PatientFid)                                 AS PatientID,
                //             MAX(SP.CreatedBy)                                 AS DoctorFid,
                //             MAX(SP.VisitedDate)                               AS VisitedDate,                                            	                    
                //             MAX(FSB.TotalAmount)                              AS TotalAmount,
                //             MAX(FSB.PayableAmount)                            AS PayableAmount,
                //             MAX(ob.Status)                                   AS Status,
                //      MAX(FSB.DiscountAmt)   As DiscountAmt,                            
                //             MAX(CONCAT(UM.FirstName, ' ', UM.LName))           AS DoctorName,
                //      MAX(GBS.Bill_Fid)  as Bill_Fid,
                //           MAX(ob.RemainingAmount) as RemainingAmount,
                //       MAX(ob.TotalAmount)   as TotalAmount   ,
                //       MAX(ob.TotalPaid) as TotalPaid,
                //       MAX(ob.Status) as Status
                //                                    FROM OutstandingBillCounterMedicine ob                                         

                //                             LEFT JOIN Patient P ON P.PatientFid = sp.PatientFid
                //                              INNER JOIN GenerateBillCounterMedicine GBS ON ob.BillFlageID = GBS.FlagId
                //                              INNER JOIN FinalBillCounterMedicine FSB ON ob.BillFlageID = FSB.BillFlageID
                //                              LEFT JOIN UserMaster UM ON sp.CreatedBy = UM.UserId
                //                            WHERE 
                //                                 ob.Status = 'Unpaid' 

                //                          		  GROUP BY SP.FlageId , ob.Id
                //                              ORDER BY MAX(SP.VisitedDate) DESC;");
                var Outstanding = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramService);
                ViewBag.OutstandingList = Outstanding;
                //ViewBag.BillFlageID = Outstanding.FlagId;
                return View();


            }
            catch (Exception Ex)
            {
                return View();
            }
        }


        public ActionResult GetOutstandingBillDetails(string Id, string FlagId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session expired" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"
                 SELECT 
                    ob.Id,
                    ob.BillFlageID,
                    ob.BillFlageID AS FlagId,
                    -- Patient Name Details
                   -- ✅ Patient Name (FINAL LOGIC)
    COALESCE(
        NULLIF(LTRIM(RTRIM(ISNULL(p.FirstName,'') + ' ' + ISNULL(p.LName,''))), ''),
        NULLIF(LTRIM(RTRIM(mp.PatientName)), ''),
        NULLIF(LTRIM(RTRIM(sp.Name)), ''),
        'N/A'
    ) AS PatientName,

                    COALESCE(p.ContactNumber, mp.MobileNo, sp.MobileNo, 'N/A') AS MobileNo,
                    COALESCE(CAST(p.Age AS VARCHAR), CAST(mp.Age AS VARCHAR), CAST(sp.Age AS VARCHAR), '0') AS Age,
                    COALESCE(p.Gender, mp.Gender, sp.Gender, 'N/A') AS Gender,

                    -- Date
                    ISNULL(ob.CreatedDate, sp.VisitedDate) AS VisitedDate,

                    -- Billing Details
                    ISNULL(fsb.TotalAmount, 0) AS TotalAmount,
                    ISNULL(fsb.PayableAmount, 0) AS PayableAmount,
                    ISNULL(fsb.DiscountAmt, 0) AS DiscountAmt,
                    ISNULL(ob.RemainingAmount, 0) AS RemainingAmount,
                    ISNULL(ob.TotalPaid, 0) AS TotalPaid,
                    ob.Status,

                    -- Doctor Name
                    ISNULL(um.FirstName + ' ' + um.LName, 'N/A') AS DoctorName,
                    fsb.Fid AS Bill_Fid

                FROM OutstandingBillCounterMedicine ob

                -- १. सर्वात आधी Prescription टेबलसोबत जोडा (जेणेकरून डेटा मिस होणार नाही)
                LEFT JOIN Tbl_MedicinePrescription sp 
                    ON ob.BillFlageID = sp.FlageId

                -- २. रजिस्टर्ड पेशंट
                LEFT JOIN Patient p 
                    ON ob.PatientFid = p.PatientFid

                -- ३. मॅन्युअल पेशंटसाठी CounterMedicineByDesk चा आधार घ्या
                LEFT JOIN CounterMedicineByDesk cmd 
                    ON ob.BillFlageID = cmd.FlageId

                LEFT JOIN MAS_ManualPatient mp 
                    ON cmd.ManualPatientId = mp.ManualPatientId

                -- ४. बिलाची माहिती
                LEFT JOIN GenerateBillCounterMedicine gbs 
                    ON ob.BillFlageID = gbs.FlagId

                LEFT JOIN FinalBillCounterMedicine fsb 
                    ON ob.BillFlageID = fsb.BillFlageID

                -- ५. डॉक्टर माहिती
                LEFT JOIN UserMaster um 
                    ON um.UserId = sp.CreatedBy

                WHERE ob.Status = 'Unpaid'
AND ob.Id = '{Id}'
AND ob.BillFlageID = '{FlagId}'
                ORDER BY ob.Id DESC;
                ");
                //param.Add("@p_Query", $@"
                //					         SELECT 
                //                             SP.FlageId,
                //                             SP.FlageId AS FlagId,
                //                             SP.FlageId AS BillFlageID,
                //                             ob.Id,
                //                             MAX(ISNULL(P.FirstName + ' ' + P.LName, SP.Name)) AS PatientName,
                //                             MAX(ISNULL(P.ContactNumber, SP.MobileNo))         AS MobileNo,
                //                             MAX(ISNULL(P.Age, SP.Age))                         AS Age,
                //                             MAX(ISNULL(P.Gender, SP.Gender))                   AS Gender,                                                 
                //                             MAX(SP.PatientFid)                                 AS PatientID,
                //                             MAX(SP.CreatedBy)                                 AS DoctorFid,
                //                             MAX(SP.VisitedDate)                               AS VisitedDate,                                            	                    
                //                             MAX(FSB.TotalAmount)                              AS TotalAmount,
                //                             MAX(FSB.PayableAmount)                            AS PayableAmount,                           
                // 	                         MAX(FSB.DiscountAmt)                              As DiscountAmt,                            
                //                             MAX(CONCAT(UM.FirstName, ' ', UM.LName))           AS DoctorName,
                // 	                         MAX(GBS.Bill_Fid)                                  as Bill_Fid,
                //                             MAX(ob.RemainingAmount)                           as RemainingAmount,
                //                             MAX(ob.TotalAmount)                               as TotalAmount   ,
                //                             MAX(ob.TotalPaid)                                    as TotalPaid,
                //                             MAX(ob.Status)                                     as Status

                //                                                  FROM OutstandingBillCounterMedicine ob                                         
                //                                          inner join Tbl_MedicinePrescription sp on sp.FlageId = ob.BillFlageID
                //                                             LEFT JOIN Patient P ON P.PatientFid = sp.PatientFid
                //                                              INNER JOIN GenerateBillCounterMedicine GBS ON ob.BillFlageID = GBS.FlagId
                //                                              INNER JOIN FinalBillCounterMedicine FSB ON ob.BillFlageID = FSB.BillFlageID
                //                                              LEFT JOIN UserMaster UM ON sp.CreatedBy = UM.UserId
                //                                            WHERE 
                //                                                ob.Status = 'Unpaid' and  ob.BillFlageID = '{FlagId}'  and ob.Id = '{Id}'                                           	
                //                                          		  GROUP BY SP.FlageId , ob.Id
                //                                              ORDER BY MAX(SP.VisitedDate) DESC
                //;");

                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();
                //ViewBag.GenerateBill = GetBillList;
                ViewBag.BillFlageID = GetBillList.FlagId;
                ViewBag.TotalAmount = GetBillList.PayableAmount;
                ViewBag.TotalPaid = GetBillList.TotalPaid;
                ViewBag.RemainingAmount = GetBillList.RemainingAmount;
                return Json(new { TotalAmount = ViewBag.TotalAmount, TotalPaid = ViewBag.TotalPaid, RemainingAmount = ViewBag.RemainingAmount, BillFlageID = ViewBag.BillFlageID, FinalBillFid = GetBillList.Bill_Fid }, JsonRequestBehavior.AllowGet);
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

        #region DeleteBill
        public ActionResult DeleteBill(string Bill_Fid_Encrapted, string FlagId)
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
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateMedicineBill", paramDelete);
                TempData["Message"] = paramDelete.Get<string>("@p_Message");
                TempData["Icon"] = paramDelete.Get<string>("@p_Icon");


                // AFTER DELETE: Fetch the list again
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@FlagId", FlagId);

                // Ensure this SP is returning the correct rows
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateMedicineBill", paramBill).ToList();

                // IMPORTANT: Return the list as a property named 'GenerateBill' 
                // to match your JavaScript 'data.GenerateBill'
                return Json(new { GenerateBill = GetBillList }, JsonRequestBehavior.AllowGet);
            }
            
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        //#region CounterMedicine

        //[HttpPost]
        //public ActionResult CounterMedicineMaster(PrescriptionModel model, string prescriptionJsonCounter, int? AppointmentFid)
        //{
        //    try
        //    {
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }


        //        List<PrescriptionModel> prescriptions = new List<PrescriptionModel>();
        //        if (!string.IsNullOrEmpty(prescriptionJsonCounter))
        //        {
        //            prescriptions = JsonConvert.DeserializeObject<List<PrescriptionModel>>(prescriptionJsonCounter);
        //        }

        //        foreach (var item in prescriptions)
        //        {
        //            var param = new DynamicParameters();
        //            param.Add("@FormID", item.FormID);
        //            param.Add("@PackageID", item.PackageID);
        //            param.Add("@NumberLeftID", item.NumberLeftID);
        //            param.Add("@FormulaID", item.FormulaID);
        //            param.Add("@DoseNumberID", item.DoseNumberID);
        //            param.Add("@DoseID", item.QuantityIDDose);

        //            param.Add("@TimeID", item.TimeID);
        //            //param.Add("@FluidVehicleID", item.FluidVehicleID);
        //            //param.Add("@MixingData", item.MixingDataJson);
        //            param.Add("@MixingDataFluid", item.MixingDataFluid);
        //            param.Add("@AppointmentFid", AppointmentFid);
        //            //param.Add("@MedicineDate", item.MedicineDate);
        //            param.Add("@DaysNumber", item.DaysNumber);
        //            param.Add("@AppointmentFid", AppointmentFid, DbType.Int32, ParameterDirection.InputOutput);

        //            BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertCounterMedicinePrescription", param);
        //            int outputFid = param.Get<int>("@AppointmentFid");

        //        }
        //        return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });

        //        //    return RedirectToAction("Index");
        //        //  connection.Execute("SP_InsertPrescription", param, commandType: CommandType.StoredProcedure);
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //}


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
                    param.Add("@FormID", item.FormID);
                    param.Add("@PackageID", item.PackageID);
                    param.Add("@NumberLeftID", item.NumberLeftID);
                    param.Add("@FormulaID", item.FormulaID);
                    param.Add("@DoseNumberID", item.DoseNumberID);
                    param.Add("@DoseID", item.QuantityIDDose);
                    param.Add("@TimeID", item.TimeID);
                    param.Add("@MixingDataFluid", item.MixingDataFluid);
                    param.Add("@DaysNumber", item.DaysNumber);

                    // Badleli Line: Input ani Output ekach veli handle kela ahe
                    param.Add("@AppointmentFid", AppointmentFid, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                    BVGWellnessCenterDapparORM.ExecuteWithoutReturn("SP_InsertCounterMedicinePrescription", param);

                    // Output value ghenya sathi:
                    int outputFid = param.Get<int>("@AppointmentFid");
                }

                return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
      // तुमचा राउट तपासा
        public JsonResult DeleteRow(int id)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@PrescriptionID", id);

                // नवीन बनवलेली किंवा अपडेट केलेली SP कॉल करा
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn(
                    "OutsideDeleteCounterMedicine",
                    param
                );

                return Json(new { success = true, message = "Medicine removed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "त्रुटी: " + ex.Message });
            }
        }
        //[HttpPost]
        //public ActionResult SaveCounterResult(MedicineDispensingModel model, int? AppointmentFid)
        //{
        //    try
        //    {
        //        if (model.MedicineInstructions != null && model.MedicineInstructions.Count > 0)
        //        {

        //            foreach (var instruction in model.MedicineInstructions)
        //            {
        //                var param = new DynamicParameters();
        //                param.Add("@AppointmentFid", AppointmentFid);
        //                param.Add("@PatientFid", model.PatientFid);
        //                param.Add("@MedicineDescription", instruction.Description);
        //                param.Add("@MorningSuchana", instruction.MorningSuchana);
        //                //param.Add("@AppointmentFid", dbType: DbType.Int32, direction: ParameterDirection.Output);
        //                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("Sp_CounterMedcineResult_Insert", param);
        //                //int OUTPUT = param.Get<int>("@AppointmentFid");

        //            }


        //            TempData["SuccessMessage"] = "औषधी सूचना यशस्वीपणे सेव्ह झाल्या आहेत.";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = "सेव्ह करण्यासाठी कोणतीही सूचना नाही.";
        //        }

        //        return RedirectToAction("AayurvedEHR", new { AppoinmentID = AppointmentFid });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error: " + ex.Message;
        //        return RedirectToAction("YourViewName");
        //    }
        //}



        //#endregion COnterMedicine



    }
}