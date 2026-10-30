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
    public class ServicesBillController : Controller
    {
        // GET: Patient/ServicesBill
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult AddServices(int? PatientFid, int? ServicesPFid, string Source, string FlageId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get Services Dropdown
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id, ServiceName as Name from Mas_Service where Status = 'Active' and Deactivate=0");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.Getdata = data;

                // Get Doctors Dropdown
                int ClinicFid = Convert.ToInt32(Session["ClinicFid"]);
                DynamicParameters Doctor = new DynamicParameters();
                Doctor.Add("@p_Query", $"select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where Deactivate=0 and ClinicFid='{ClinicFid}' and IsDoctor=1");
                var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", Doctor);
                ViewBag.GetDoctorName = GetDoctor ?? new List<DropdownItem>();

                ViewBag.FlagId = FlageId;


                ViewBag.Other = (Source == "other") ? "other" : "null";
                if (FlageId != null)
                {
                    // Scenario 2: Manual patient (from Tbl_ServicesP)
                    DynamicParameters Services = new DynamicParameters();
                    Services.Add("@FlageId", FlageId);
                    var GetServicesbypatints = BVGWellnessCenterDapparORM.ReturnList<PrescriptionServiceVM>("sp_Get_Services_ByPFid", Services).ToList();
                    ViewBag.GetServicesList = GetServicesbypatints;
                    ViewBag.PatientFid = GetServicesbypatints.FirstOrDefault()?.PatientFid;

                    if (ViewBag.PatientFid != null)
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
                        param1.Add("@p_Query", $@" select top 1 
											ES.Name as PatientName , 
											ES.MobileNo as ContactNumber,
											ES.Age,
											ES.Gender,
											ES.VisitedDate
									     	 FROM Tbl_ServicesP ES                                             
                                            WHERE ES.FlageId = '{FlageId}' ");
                        var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param1).FirstOrDefault();
                        ViewBag.TodayPatients = TodayPatients;
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

        [HttpPost]
        public ActionResult SaveServices(List<PrescriptionServiceVM> services, ManualPatientVM manualPatient)
        {
            try
            {
                if (services == null || services.Count == 0)
                {
                    return Json(new { success = false, message = "No data received" });
                }

                int lastServicePFid = 0;
                int patientId = 0;


                string FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();


                {
                    foreach (var item in services)
                    {
                        DynamicParameters param = new DynamicParameters();

                        param.Add("@ServiceFid", item.ServiceFid);
                        param.Add("@Rate", item.Rate);
                        param.Add("@Quantity", item.Quantity);
                        param.Add("@Discount", item.Discount);
                        param.Add("@Amount", item.Amount);
                        param.Add("@Note", item.Note);

                        param.Add("@PatientID", item.PatientID);
                        param.Add("@DoctorFid", item.DoctorFid);
                        param.Add("@CreatedBy", Session["UserID"]);
                        //  param.Add("@FlageId", FlageId, DbType.String, ParameterDirection.InputOutput);
                        param.Add("@FlageId", FlageId, dbType: DbType.String, size: 50, direction: ParameterDirection.InputOutput);

                        param.Add("@Name", patientId == 0 ? manualPatient?.PatientName : null);
                        param.Add("@MobileNo", patientId == 0 ? manualPatient?.MobileNo : null);
                        param.Add("@Age", patientId == 0 ? manualPatient?.Age : null);
                        param.Add("@Gender", patientId == 0 ? manualPatient?.Gender : null);
                        param.Add("@VisitedDate", patientId == 0 ? manualPatient?.VisitDate : null);

                        // param.Add("@PatientID", item.PatientID,dbType: DbType.Int32,direction: ParameterDirection.InputOutput);
                        param.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        //con.Execute("sp_Save_EncounterServices",
                        //            param,
                        //            commandType: CommandType.StoredProcedure);

                        BVGWellnessCenterDapparORM.ExcuteSp("sp_Save_ServicesForPatients", param);
                        lastServicePFid = param.Get<int>("@NewId");

                        FlageId = param.Get<string>("@FlageId");

                    }
                }

                return Json(new { servicePFid = lastServicePFid, flageId = FlageId, success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult SearchOPD(string searchTerm)
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@searchTerm", searchTerm);
            //param.Add("@HospitalID", Session["HospitalID"]);
            var result = BVGWellnessCenterDapparORM.ReturnList<Appointment>("Sp_searchpatient", param).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region GetBillRecord
        public ActionResult GetServicesBillRecord(int? PatientFid, int? AppointmentFid, int? Bill_Fid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string paymentStatus = "";


                if (PatientFid != null && PatientFid != 0)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@PatientFid", PatientFid);
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillAppointment", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                    ViewBag.AppointmentFid = AppointmentFid;
                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status from FinalBillAppointment  where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    paymentStatus = FinalBill?.Status ?? "Unknown";


                    DynamicParameters paramFinal1 = new DynamicParameters();
                    paramFinal1.Add("@p_Query", "select Status,DiscountAmt from FinalBillAppointment  where Deactivate=0 and PatientFid='" + PatientFid + "'  and  AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill1 = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal1).FirstOrDefault();
                    paymentStatus = FinalBill1?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill1?.DiscountAmt ?? "Unknown";

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
                paramprice.Add("@p_Query", "select Charges  from Mas_Service where Deactivate=0 and ServiceFid='" + GetServiceId + "'");
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
        public ActionResult SaveGenerateBillServices(EHR Bill, int? AppointmentFid)
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
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillServices", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;
                }
                else if (Bill.FlagId != null)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", Bill.FlagId);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
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

        //[HttpPost]
        //public ActionResult SaveAndCloseBilling(List<ClsPrescription> paymentList, string ISOutStandingAmt)
        //{
        //    try
        //    {
        //        var appointmentId = paymentList.First().AppointmentFid;
        //        var  PatientID = paymentList.First().PatientFid;
        //        var BillFlageID = paymentList.First().BillFlageID;



        //        for (int i = 0; i < paymentList.Count(); i++)
        //        {
        //            var FlageId = "";
        //            if (ISOutStandingAmt == "Yes")
        //            {
        //                //FlageId = $"{i + 1}{paymentList[i].AppointmentFid}{paymentList[i].Id}";
        //                //FlageId = Guid.NewGuid().ToString();
        //                FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
        //            }
        //            else
        //            {
        //                FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
        //            }


        //            DynamicParameters check = new DynamicParameters();
        //            check.Add("@p_Query", "select FlageID from FinalservicesBill where Deactivate=0 and  FlageID='" + FlageId + "'  and BillFlageID = ' " + BillFlageID + " '");

        //            var exist = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", check).FirstOrDefault();

        //            DynamicParameters param = new DynamicParameters();
        //            param.Add("@p_ProcessType", "Save");
        //            param.Add("@p_MacId", Environment.MachineName);
        //            param.Add("@p_MacIp", "0.0.0.0");

        //            if (exist != null)
        //                param.Add("@p_AlreadyExist", "Yesh");
        //            else
        //                param.Add("@p_AlreadyExist", null);

        //            param.Add("@p_AppointmentFid", paymentList[i].AppointmentFid);
        //            param.Add("@p_PatientFid", paymentList[i].PatientFid);
        //            param.Add("@p_TotalAmount", paymentList[i].finalAmount);
        //            param.Add("@p_DiscountAmt", paymentList[i].Discount);
        //            param.Add("@p_PayableAmount", paymentList[i].FinalPaybalAmount);
        //            param.Add("@p_Status", paymentList[i].PaymentStatus);
        //            param.Add("@p_PaymentType", paymentList[i].PaymentType);
        //            param.Add("@BillFlageID", paymentList[i].BillFlageID);
        //            param.Add("@p_ReferenceNumber", paymentList[i].ReferenceNumber);
        //            param.Add("@p_Amount", paymentList[i].Amount);
        //            param.Add("@p_FlageID", exist != null ? exist.FlageID : FlageId);

        //            param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

        //            BVGWellnessCenterDapparORM.ExcuteSp("FinalBillServices", param);
        //            TempData["Message"] = param.Get<string>("@p_Message");
        //            TempData["Icon"] = param.Get<string>("@p_Icon");

        //            if (ISOutStandingAmt == "Yes")
        //            {

        //                DynamicParameters OutStanding = new DynamicParameters();

        //                //  DynamicParameters OutStanding = new DynamicParameters();
        //                OutStanding.Add("@p_Query", "update FinalservicesBill set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

        //                var Standing = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", OutStanding).FirstOrDefault();

        //            }


        //        }

        //        // for outstanding 
        //        DynamicParameters outstandparam = new DynamicParameters();
        //        outstandparam.Add("@p_Query", $@"
        //    SELECT 
        //        BillFlageID,
        //        MAX(PayableAmount) AS PayableAmount,
        //        SUM(Amount) AS TotalPaid,
        //        (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount
        //    FROM FinalservicesBill
        //    WHERE BillFlageID = '{BillFlageID}'
        //    GROUP BY BillFlageID");

        //        var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

        //        //if (summary.TotalAmount != summary.TotalPaid)
        //        if (summary != null)
        //        {
        //            // decide status based on remaining amount
        //            string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

        //            DynamicParameters sumInsUpd = new DynamicParameters();
        //            sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
        //            sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
        //            sumInsUpd.Add("@TotalAmount", summary.PayableAmount);

        //            sumInsUpd.Add("@PatientFid", PatientID);
        //            sumInsUpd.Add("@BillFlageID", BillFlageID);

        //            sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
        //            sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
        //            sumInsUpd.Add("@Status", status);

        //            BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_ServicesOutstanding", sumInsUpd);



        //        }


        //        return Json(new { success = true, message = "Payments processed." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}


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
                        FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                    }


                    // DynamicParameters check = new DynamicParameters();
                    // check.Add("@p_Query", "select FlageID from FinalservicesBill where Deactivate=0 and  FlageID='" + FlageId + "'  and BillFlageID = ' " + BillFlageID + " '");
                    // var exist = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", check).FirstOrDefault();
                    DynamicParameters check = new DynamicParameters();
                    check.Add("@p_Query", "select TOP 1 FlageID from FinalservicesBill where Deactivate=0 and BillFlageID = '" + BillFlageID + "' and Status = 'Unpaid'");
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
                    double finalAmount = paymentList[i].finalAmount;
                    double discountPercent = paymentList[i].Discount;
                    double payableAmount = finalAmount - (finalAmount * (discountPercent / 100));

                    param.Add("@p_TotalAmount", finalAmount);
                    param.Add("@p_DiscountAmt", discountPercent);
                    param.Add("@p_PayableAmount", payableAmount);
                    param.Add("@p_Status", paymentList[i].PaymentStatus);
                    param.Add("@p_PaymentType", paymentList[i].PaymentType);
                    param.Add("@BillFlageID", paymentList[i].BillFlageID);
                    param.Add("@p_ReferenceNumber", paymentList[i].ReferenceNumber);
                    param.Add("@p_Amount", paymentList[i].Amount);
                    param.Add("@p_FlageID", exist != null ? exist.FlageID : FlageId);

                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                    BVGWellnessCenterDapparORM.ExcuteSp("FinalBillServices", param);
                    TempData["Message"] = param.Get<string>("@p_Message");
                    TempData["Icon"] = param.Get<string>("@p_Icon");

                    if (ISOutStandingAmt == "Yes")
                    {

                        DynamicParameters OutStanding = new DynamicParameters();

                        //  DynamicParameters OutStanding = new DynamicParameters();
                        OutStanding.Add("@p_Query", "update FinalservicesBill set IsOutstanding = '1' where FlageId = '" + FlageId + "'");

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
            FROM FinalservicesBill
            WHERE BillFlageID = '{BillFlageID}'
            GROUP BY BillFlageID");

                var summary = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", outstandparam).FirstOrDefault();

                //if (summary.TotalAmount != summary.TotalPaid)
                if (summary != null)
                {
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    // Juna Code comment kela
                    // sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
                    // Naveen Code add kela
                    sumInsUpd.Add("@AppointmentFid", appointmentId);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);

                    sumInsUpd.Add("@BillFlageID", BillFlageID);
                    sumInsUpd.Add("@PatientFid", PatientID);

                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_ServicesOutstanding", sumInsUpd);



                }


                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public ActionResult BillingRecordServices(string FlagId)
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
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_ServicesPaymentDetails", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId ", FlagId); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", @"
select top 1 
    FDate,
    TotalAmount,
    DiscountAmt,
    PayableAmount,
    Status,
    Amount
from FinalservicesBill
where Deactivate = 0
and BillFlageID = '" + FlagId + @"'
order by Fid desc
");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT BillFlageID, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalservicesBill WHERE BillFlageID ='" + FlagId + "' GROUP BY BillFlageID ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }

                var GetUnPaid = BVGWellnessCenterDapparORM.DynamicList("sp_List_ServicesPaymentUnapidList");
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
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal1 = new DynamicParameters();
                    paramFinal1.Add("@p_Query", "select Status,DiscountAmt from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var FinalBill1 = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal1).FirstOrDefault();
                    paymentStatus = FinalBill1?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill1?.DiscountAmt ?? "Unknown";

                    ViewBag.FlagId = FlagId;



                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    paymentStatus = FinalBill?.Status ?? "Unknown";
                }


                return Json(new { GetBillList = ViewBag.GenerateBill, paymentStatus = paymentStatus, DiscountAmt = ViewBag.Discountamt, GetFinalbillDetails = ViewBag.GetFinalbillDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion




        #region PaidBillRecords
        public ActionResult PaidBillRecordsServices(string FlagId)
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
                    param.Add("@FlagId", FlagId);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_ServicesPaymentDetails", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", @"
                        select top 1 
                            FDate,
                            TotalAmount,
                            DiscountAmt,
                            PayableAmount,
                            Status,
                            Amount
                        from FinalservicesBill
                        where Deactivate = 0
                        and BillFlageID = '" + FlagId + @"'
                        order by Fid desc
                        ");
                    //paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT BillFlageID, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalservicesBill WHERE BillFlageID ='" + FlagId + "' GROUP BY BillFlageID ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_ServicesPaymentpaidList");
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

        public ActionResult OutstandingBill(string FlagId)
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
                    param.Add("@FlagId", FlagId);
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_ServicesPaymentDetails", param).FirstOrDefault();
                    
                    if (result == null)
                    {
                        // Fallback query directly in C# if no payments exist yet in FinalservicesBill
                        DynamicParameters paramFallback = new DynamicParameters();
                        paramFallback.Add("@FlagId", FlagId);
                        result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", new DynamicParameters(new {
                            p_Query = $@"
                                SELECT TOP 1
                                    SP.FlagId,
                                    CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                                    P.ContactNumber AS MobileNo,
                                    P.Age,
                                    P.Gender,
                                    SP.PatientFid AS PatientID,
                                    SP.DoctorFid,
                                    SP.TotalAmount,
                                    SP.TotalAmount AS PayableAmount,
                                    0 AS DiscountAmt,
                                    'Unpaid' AS Status,
                                    NULL AS PaymentMode,
                                    SP.CreatedDate AS GeneratedBillDate,
                                    0 AS Bill_Fid,
                                    CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName
                                FROM MAS_TherapyBooking SP
                                LEFT JOIN Patient P ON SP.PatientFid = P.PatientFid 
                                LEFT JOIN UserMaster UM ON SP.DoctorFid = UM.UserId
                                WHERE SP.IsActive = 1 AND SP.FlagId = '{FlagId}'"
                        })).FirstOrDefault();
                    }
                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId); 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", @"
select top 1 
    FDate,
    TotalAmount,
    DiscountAmt,
    PayableAmount,
    Status,
    Amount
from FinalservicesBill
where Deactivate = 0
and BillFlageID = '" + FlagId + @"'
order by Fid desc
");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    
                    if (FinalBill == null)
                    {
                        DynamicParameters paramDefaultBill = new DynamicParameters();
                        paramDefaultBill.Add("@p_Query", $@"
                            SELECT TOP 1 
                                SP.CreatedDate AS FDate,
                                ISNULL(SP.TotalAmount, 0) AS TotalAmount,
                                0.00 AS DiscountAmt,
                                ISNULL(SP.TotalAmount, 0) AS PayableAmount,
                                'Unpaid' AS Status,
                                0.00 AS Amount
                            FROM MAS_TherapyBooking SP
                            WHERE SP.FlagId = '{FlagId}'");
                        FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramDefaultBill).FirstOrDefault();
                    }
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from FinalservicesBill where Deactivate=0 and BillFlageID='" + FlagId + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT BillFlageID, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalservicesBill WHERE BillFlageID ='" + FlagId + "' GROUP BY BillFlageID ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    
                    if (GetOutstandingDetails == null)
                    {
                        DynamicParameters paramDefaultOutstanding = new DynamicParameters();
                        paramDefaultOutstanding.Add("@p_Query", $@"
                            SELECT TOP 1 
                                SP.FlagId AS BillFlageID,
                                ISNULL(SP.TotalAmount, 0) AS PayableAmount,
                                0.00 AS TotalPaid,
                                ISNULL(SP.TotalAmount, 0) AS RemainingAmount
                            FROM MAS_TherapyBooking SP
                            WHERE SP.FlagId = '{FlagId}'");
                        GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramDefaultOutstanding).FirstOrDefault();
                    }
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;
                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                    ViewBag.GetOutstandingDetails = "";
                }

                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", $@"
    SELECT 
        MAX(SP.FlagId) AS FlagId,
        ob.Id,                                                 
        CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
        MAX(P.ContactNumber) AS MobileNo,
        MAX(P.Age) AS Age,
        MAX(P.Gender) AS Gender,                                                  
        MAX(SP.PatientFid) AS PatientID,
        MAX(SP.DoctorFid) AS DoctorFid,
        ob.TotalAmount AS TotalAmount,
        ob.TotalAmount AS PayableAmount,
        MAX(FSB.DiscountAmt) AS DiscountAmt,
        MAX(FSB.Status) AS FinalBillStatus,
        MAX(FSB.PaymentMode) AS PaymentMode,
        MAX(FSB.FDate) AS GeneratedBillDate,
        MAX(FSB.Fid) AS Bill_Fid,
        CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
        MAX(GBS.Bill_Fid) AS GBS_Bill_Fid,
        ob.RemainingAmount,
        ob.TotalPaid,
        ob.Status
    FROM OutstandingBillServices ob                                                                 
    INNER JOIN MAS_TherapyBooking sp ON sp.FlagId = ob.BillFlageID
    LEFT JOIN Patient P ON P.PatientFid = sp.PatientFid
    INNER JOIN GenerateBillServices GBS ON ob.BillFlageID = GBS.FlagId
    INNER JOIN FinalservicesBill FSB ON ob.BillFlageID = FSB.BillFlageID
    LEFT JOIN UserMaster UM ON sp.DoctorFid = UM.UserId
    WHERE SP.IsActive = 1  
    AND ob.Status = 'Unpaid'
    AND ob.TotalPaid > 0
    GROUP BY 
        ob.Id, 
        P.FirstName, P.LName, 
        UM.FirstName, UM.LName, 
        ob.RemainingAmount, 
        ob.TotalAmount, 
        ob.TotalPaid, 
        ob.Status,
        ob.BillFlageID
    ORDER BY GeneratedBillDate DESC;");
                var OutstandingList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramService);
                ViewBag.OutstandingList = OutstandingList;
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
                param.Add("@p_Query", $@"SELECT 
SP.FlagId,
SP.FlagId AS FlagId,
ob.Id,                                               
CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
P.ContactNumber AS MobileNo,
P.Age,
P.Gender,                                                 
SP.PatientFid AS PatientID,
SP.DoctorFid,
ob.TotalAmount,
ob.TotalAmount AS PayableAmount,
FSB.DiscountAmt,
FSB.Status AS FinalBillStatus,
FSB.PaymentMode,
FSB.FDate,
FSB.FDate AS GeneratedBillDate,
FSB.Fid AS Bill_Fid,
CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName,
GBS.Bill_Fid,
ob.RemainingAmount,
ob.TotalPaid,
ob.Status
              
FROM OutstandingBillServices ob                                                                
INNER JOIN MAS_TherapyBooking sp ON sp.FlagId = ob.BillFlageID
LEFT JOIN Patient P ON P.PatientFid = sp.PatientFid
INNER JOIN GenerateBillServices GBS ON ob.BillFlageID = GBS.FlagId
INNER JOIN FinalservicesBill FSB ON ob.BillFlageID = FSB.BillFlageID
LEFT JOIN UserMaster UM ON sp.DoctorFid = UM.UserId

WHERE 
SP.IsActive = 1  
AND ob.Status = 'Unpaid' 
AND ob.BillFlageID = '{FlagId}'  
AND ob.Id = '{Id}'

ORDER BY FSB.FDate DESC;");

                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();
                //ViewBag.GenerateBill = GetBillList;
                ViewBag.TotalAmount = GetBillList.TotalAmount;
                ViewBag.BillFlageID = GetBillList.FlagId;
                ViewBag.TotalPaid = GetBillList.TotalPaid;
                ViewBag.RemainingAmount = GetBillList.RemainingAmount;
                return Json(new { 
                    TotalAmount = ViewBag.TotalAmount, 
                    TotalPaid = ViewBag.TotalPaid, 
                    RemainingAmount = ViewBag.RemainingAmount, 
                    BillFlageID = ViewBag.BillFlageID,
                    PatientFid = GetBillList.PatientID,
                    DiscountAmt = GetBillList.DiscountAmt,
                    FinalBillFid = GetBillList.Bill_Fid
                }, JsonRequestBehavior.AllowGet);
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
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillServices", paramDelete);
                TempData["Message"] = paramDelete.Get<string>("@p_Message");
                TempData["Icon"] = paramDelete.Get<string>("@p_Icon");


                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@FlagId", FlagId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                var GenerateBill = GetBillList;

                return Json(new { GenerateBill = GenerateBill }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion



    }
}
