using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
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
    public class BillingRecordController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/BillingRecord
        public ActionResult BillingRecord(int? AppointmentFid)
        {
            try
            {

                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                //if (EncounterFid != null)
                //{
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_EncountrFid", EncounterFid);
                //    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                //    ViewBag.GetPatientEncounterDetails = result;

                //    DynamicParameters paramBill = new DynamicParameters();
                //    paramBill.Add("@p_EncounterFid", EncounterFid);
                //    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                //    ViewBag.GenerateBill = GetBillList;

                //    DynamicParameters paramFinal = new DynamicParameters();
                //    paramFinal.Add("@p_Query", "select TotalAmount,DiscountAmt,PayableAmount, Status from finalbill where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                //    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                //    ViewBag.FinalBill = FinalBill;  

                //}
                //else
                //{
                //    ViewBag.FinalBill = "";
                //    ViewBag.GenerateBill = "";
                //    ViewBag.GetPatientEncounterDetails = "";
                //}
                if (AppointmentFid != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetails", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;
                    }
                    else
                    {
                        ViewBag.FinalBill = "";
                        ViewBag.GenerateBill = "";
                        ViewBag.GetPatientEncounterDetails = "";
                    }

                  var GetUnPaid = BVGWellnessCenterDapparORM.DynamicList("sp_List_UnPaidBill");
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
        public ActionResult GetBillRecord(int? Bill_Fid, int? AppointmentFid)
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
                if (AppointmentFid != null && AppointmentFid != 0)
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal1 = new DynamicParameters();
                    paramFinal1.Add("@p_Query", "select Status,DiscountAmt from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill1 = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal1).FirstOrDefault();
                    paymentStatus = FinalBill1?.Status ?? "Unknown";
                    ViewBag.Discountamt = FinalBill1?.DiscountAmt ?? "Unknown";





                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
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
        public ActionResult PaidBillRecords(int? AppointmentFid, string fromPage)
        {
            
            try
            {
                ViewBag.FromPage = fromPage;
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (AppointmentFid != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@AppointmentFid", AppointmentFid);
                    //var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("Sp_Get_AppointmentDetails", param).FirstOrDefault();

                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_AppointmentFid", AppointmentFid); // apointment che condition add karayche ahe 
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select FDate,TotalAmount,DiscountAmt,PayableAmount, Status from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select FDate,Amount,PaymentReferenceNumber,PaymentMode from finalbill where Deactivate=0 and AppointmentFid='" + AppointmentFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                    DynamicParameters Outstanding = new DynamicParameters();
                    Outstanding.Add("@p_Query", "SELECT AppointmentFid, MAX(PayableAmount) AS PayableAmount, SUM(Amount) AS TotalPaid, (MAX(PayableAmount) - SUM(Amount)) AS RemainingAmount FROM FinalBill WHERE AppointmentFid ='" + AppointmentFid + "' GROUP BY AppointmentFid ");
                    var GetOutstandingDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", Outstanding).FirstOrDefault();
                    ViewBag.GetOutstandingDetails = GetOutstandingDetails;

                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_PaidBills");
                ViewBag.GetPaidBillList = GetPaidBillList;
                // ✅ IMPORTANT LINE
                if (Request.IsAjaxRequest())
                {
                    return PartialView(); // same view but without layout
                }

                return View();
            }
            catch (Exception Ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveGenerateBill
        //[HttpPost]
        //public ActionResult SaveGenerateBill(ClsPrescription Bill)
        //{
        //    try
        //    {
        //        string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }
        //        //var GetBillList = "";
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_ProcessType", string.IsNullOrEmpty(Bill.Bill_Fid_Encrapted) ? "Save" : "Update");
        //        param.Add("@p_MacId", LoginMachinId);
        //        param.Add("@p_MacIp", LoginmachinIp);
        //        param.Add("@p_Bill_Fid", Bill.Bill_Fid);
        //        param.Add("@p_Bill_Fid_Encrapted", Bill.Bill_Fid_Encrapted);
        //        if (Bill.ServiceFid != 0 && Bill.ServiceFid != null)
        //        {
        //            param.Add("@p_ServiceFid", Bill.ServiceFid);
        //        } else if (Bill.Service_Fid != 0 && Bill.Service_Fid != null)
        //        {
        //            param.Add("@p_ServiceFid", Bill.Service_Fid);
        //        }

        //        param.Add("@p_Price", Bill.Price);
        //        param.Add("@p_Quantity", Bill.Quantity);
        //        param.Add("@p_Total", Bill.Total);
        //        param.Add("@p_EncounterFid", Bill.EncounterFid);
        //        param.Add("@p_AppointmentFid", Bill.AppointmentFid);
        //        param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBill", param);
        //        TempData["Message"] = param.Get<string>("@p_Message");
        //        TempData["Icon"] = param.Get<string>("@p_Icon");

        //        if (Bill.EncounterFid != 0 && Bill.EncounterFid != null)
        //        {
        //            DynamicParameters paramBill = new DynamicParameters();
        //            paramBill.Add("@p_EncounterFid", Bill.EncounterFid);
        //            var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
        //            ViewBag.GenerateBill = GetBillList;
        //        }
        //        else if (Bill.AppointmentFid != 0 && Bill.AppointmentFid != null)
        //        {
        //            DynamicParameters paramBill = new DynamicParameters();
        //            paramBill.Add("@p_AppointmentFid", Bill.AppointmentFid);
        //            var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
        //            ViewBag.GenerateBill = GetBillList;
        //        }
        //        return Json(new { GetBillList = ViewBag.GenerateBill }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception)
        //    {

        //        return RedirectToAction("ErrorPage", "Home", new { area = "" });
        //    }
        //}




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
        //        DynamicParameters paramEncounter = new DynamicParameters();
        //        paramEncounter.Add("@p_Query", "select PatientFid,DoctorFid,EncounterDate from PatientEncounter where Deactivate=0 and EncounterFid='" + finalbill.EncounterFid + "'");
        //        PEncounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_ExecuteQuery", paramEncounter).FirstOrDefault();

        //        DynamicParameters paramAppointmentFid = new DynamicParameters();
        //        paramAppointmentFid.Add("@p_Query", "select AppointmentFid from PatientAppoinment where Deactivate=0 and PatientFid='" + PEncounter.PatientFid + "' and AppoinmentDate='" + PEncounter.EncounterDate + "' and DoctorFid='" + PEncounter.DoctorFid + "'");
        //        var GetAppointFid = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramAppointmentFid).FirstOrDefault();
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_ProcessType", "Save");
        //        param.Add("@p_MacId", LoginMachinId);
        //        param.Add("@p_MacIp", LoginmachinIp);
        //        if (finalbill.EncounterFid != 0 && finalbill.EncounterFid != null)
        //        {
        //            param.Add("@p_EncounterFid", finalbill.EncounterFid);
        //            DynamicParameters paramFinal = new DynamicParameters();
        //            paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and EncounterFid='" + finalbill.EncounterFid + "'");
        //            var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();

        //            if (FinalBill != null)
        //            {
        //                if (FinalBill?.Status == "Unpaid" || FinalBill?.Status == "Paid")
        //                {
        //                    param.Add("@p_AlreadyExist", "Yesh");
        //                    param.Add("@p_PatientFid", PEncounter.PatientFid);
        //                    param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
        //                    param.Add("@p_DoctorFid", PEncounter.DoctorFid);
        //                }
        //                else
        //                {
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
        //        if (finalbill.AppointmentFid != 0 && finalbill.AppointmentFid != null)
        //        {
        //            param.Add("@p_AppointmentFid", finalbill.AppointmentFid);
        //            DynamicParameters paramFinal = new DynamicParameters();
        //            paramFinal.Add("@p_Query", "select Status from finalbill where Deactivate=0 and AppointmentFid='" + finalbill.AppointmentFid + "'");
        //            var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
        //            //DynamicParameters paramEncounter = new DynamicParameters();
        //            //paramEncounter.Add("@p_Query", "select PatientFid,DoctorFid,EncounterDate from PatientEncounter where Deactivate=0 and EncounterFid='" + finalbill.EncounterFid + "'");
        //            //PEncounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_ExecuteQuery", paramEncounter).FirstOrDefault();
        //            if (FinalBill != null)
        //            {
        //                if (FinalBill?.Status == "Unpaid" || FinalBill?.Status == "Paid")
        //                {
        //                    param.Add("@p_AlreadyExist", "Yesh");
        //                }
        //                else
        //                {
        //                    param.Add("@p_PatientFid", PEncounter.PatientFid);
        //                    param.Add("@p_AppointmentDate", PEncounter.EncounterDate);
        //                    param.Add("@p_DoctorFid", PEncounter.DoctorFid);
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
        //        param.Add("@p_PaymentType", finalbill.PaymentType);
        //        param.Add("@p_ReferenceNumber", finalbill.ReferenceNumber);
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
        public ActionResult SaveAndCloseEncounter(List<ClsPrescription> paymentList, ClsPrescription finalbill, string PaymentStatus)
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
                int AppointmentFid = 0;

                for (int i = 0; i < paymentList.Count(); i++)
                {
                    EncounterFid = Convert.ToInt32(paymentList[i].EncounterFid);
                    var FlageId = $"{i + 1}{paymentList[i].EncounterFid}";

                    DynamicParameters paramEncounter = new DynamicParameters();
                    paramEncounter.Add("@p_Query", "select PatientFid,DoctorFid,EncounterDate from PatientEncounter where Deactivate=0 and EncounterFid='" + paymentList[i].EncounterFid + "'");
                    PEncounter = BVGWellnessCenterDapparORM.ReturnList<ClsPatientEncounter>("sp_ExecuteQuery", paramEncounter).FirstOrDefault();

                    DynamicParameters paramAppointmentFid = new DynamicParameters();
                    paramAppointmentFid.Add("@p_Query", "select AppointmentFid from PatientAppoinment where Deactivate=0 and PatientFid='" + PEncounter.PatientFid + "' and AppoinmentDate='" + PEncounter.EncounterDate + "' and DoctorFid='" + PEncounter.DoctorFid + "'");
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
                    } else
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
                else if (finalbill.AppointmentFid != 0 && finalbill.AppointmentFid != null)
                {
                    AppointmentFid = Convert.ToInt32(finalbill.AppointmentFid);
                    return Json(new { AppointmentFid = AppointmentFid }, JsonRequestBehavior.AllowGet);
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

        //public ActionResult GetServices(string check)
        //{
        //    try
        //    {
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }
        //        if (check == "Checked")
        //        {
        //            DynamicParameters paramService = new DynamicParameters();
        //            paramService.Add("@p_Query", "select ServiceFid as id, concat(Servicecategory,' ',ServiceName) as Name  from mas_service where ServiceName='Health Check'");
        //            var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
        //            ViewBag.Getdata = data;
        //        }
        //        else
        //        {
        //            DynamicParameters paramService = new DynamicParameters();
        //            paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Servicecategory='Medicine'");
        //            var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
        //            ViewBag.Getdata = data;
        //        }

        //        return Json(new { GetData = ViewBag.Getdata }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        return RedirectToAction("ErrorPage", "Home", new { area = "" });
        //    }
        //}

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
                } else if (GetBillList.MedicineName != null && GetBillList.MedicineName != "")
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
                TempData["Message"] = paramDelete.Get<string>("@p_Message");
                TempData["Icon"] = paramDelete.Get<string>("@p_Icon");


                DynamicParameters paramBill = new DynamicParameters();
               paramBill.Add("@p_AppointmentFid", AppointmentFid);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                var GenerateBill = GetBillList;

                    return Json(new { GenerateBill = GenerateBill }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SendBillEmail
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SendBillEmail(HttpPostedFileBase pdfFile)
        //{
        //    try
        //    {
        //        if (Session["UserID"] == null)
        //        {
        //            return RedirectToAction("Login", "Home", new { area = "" });
        //        }

        //        if (Request.Files.Count > 0)
        //        {
        //            var file1 = Request.Files[0];
        //        }
        //            var SMTPServerName = "probus.icewarpcloud.in";
        //        var PortNo = 587;
        //        var FromEmailId = "qa@probus.co.in";
        //        var SMTPPassword = "Ui@123@.#";
        //        var SSL = 1;

        //        //SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
        //        //smtp.EnableSsl = true;
        //        //smtp.UseDefaultCredentials = true;
        //        //smtp.Timeout = 100000;
        //        //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

        //        //smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, SMTPPassword);

        //        //MailMessage mail = new MailMessage(FromEmailId, ToEmailID);
        //        //mail.Subject = mailtitle;
        //        //var GetBody = mailBody;
        //        //mail.Body = GetBody;
        //        ////  mail.Attachments.Add(new Attachment(Fbank.ExportToStream(ExportFormatType.PortableDocFormat), "PaymentSlip.pdf"));
        //        //mail.IsBodyHtml = true;
        //        //smtp.Send(mail);




        //        //if (pdfFile != null && pdfFile.ContentLength > 0)
        //        //{
        //        //    string recipientEmail = "manishbadgujar902@gmail.com"; // Change as needed
        //        //    string subject = "Your Bill Details";
        //        //    string body = "Please find attached your bill details.";

        //        //    using (MemoryStream ms = new MemoryStream())
        //        //    {
        //        //        pdfFile.InputStream.CopyTo(ms);
        //        //        byte[] fileBytes = ms.ToArray();

        //        //        MailMessage mail = new MailMessage();
        //        //        mail.From = new MailAddress(FromEmailId);
        //        //        mail.To.Add(recipientEmail);
        //        //        mail.Subject = subject;
        //        //        mail.Body = body;
        //        //        mail.IsBodyHtml = true;
        //        //        mail.Attachments.Add(new Attachment(new MemoryStream(fileBytes), "BillDetails.pdf"));

        //        //        SmtpClient smtp = new SmtpClient
        //        //        {
        //        //            Host = SMTPServerName,
        //        //            Port = PortNo,
        //        //            Credentials = new NetworkCredential(FromEmailId, SMTPPassword),
        //        //            EnableSsl = true
        //        //        };

        //        //        smtp.Send(mail);
        //        //    }
        //        //    return Json(new { success = true, message = "Email Sent Successfully!" });
        //        //}
        //        return Json(new { success = false, message = "Failed to send email." });


        //    }
        //    catch (Exception ex)
        //    {

        //        System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
        //        return Json(new { success = false, message = "Internal Error: " + ex.Message });
        //    }

        //}

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
                        return Json(new { success = true, message = "Bill details sent successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "No file received!" });
                    }
                } else
                {
                    return Json(new { success = false, message = "No file received!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion


        #region Outstanding

        public ActionResult OutstandingBill()
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
FROM OutstandingBill ob
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
FROM OutstandingBill ob
LEFT JOIN PatientAppoinment pa ON pa.AppointmentFid = ob.AppointmentFid
LEFT JOIN Patient p ON p.PatientFid = pa.PatientFid WHERE ob.AppointmentFid = '{AppointmentFid}' AND ob.Id = '{Id}' AND ob.Deactivate = 0");

                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();
                //ViewBag.GenerateBill = GetBillList;
                ViewBag.TotalAmount = GetBillList.TotalAmount;
                ViewBag.TotalPaid = GetBillList.TotalPaid;
                ViewBag.RemainingAmount = GetBillList.RemainingAmount;
                return Json(new { TotalAmount = ViewBag.TotalAmount, TotalPaid = ViewBag.TotalPaid, RemainingAmount= ViewBag.RemainingAmount }, JsonRequestBehavior.AllowGet);
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



        [HttpGet]
        public ActionResult GetBillSummary(int AppointmentFid)
        {
            try
            {
                var billSummary = new List<object>();

                // ================== 1. APPOINTMENT BILL ==================
                DynamicParameters ap = new DynamicParameters();
                ap.Add("@p_Query", $@"
                    SELECT 
                        COUNT(*) AS TotalCount,
                        SUM(CASE WHEN LOWER(LTRIM(RTRIM(Status))) = 'paid' THEN 1 ELSE 0 END) AS PaidCount,
                        SUM(CASE WHEN LOWER(LTRIM(RTRIM(Status))) = 'unpaid' THEN 1 ELSE 0 END) AS UnpaidCount
                    FROM FinalBillAppointment
                    WHERE AppointmentFid = {AppointmentFid} AND Deactivate=0");

                var apData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", ap)
                    .FirstOrDefault();

                int apUnpaid = apData != null ? Convert.ToInt32(apData.UnpaidCount ?? 0) : 0;
                int apPaid = apData != null ? Convert.ToInt32(apData.PaidCount ?? 0) : 0;
                int apTotal = apData != null ? Convert.ToInt32(apData.TotalCount ?? 0) : 0;

                string apStatus = "Unpaid";
                if (apUnpaid > 0 && apPaid > 0)
                    apStatus = "Outstanding";
                else if (apPaid == apTotal && apTotal > 0)
                    apStatus = "Paid";
                else
                    apStatus = "Unpaid";

                billSummary.Add(new { BillName = "Appointment", Status = apStatus });                // ================== 2. BG / COUNTER BILL ==================
                DynamicParameters other = new DynamicParameters();
                other.Add("@p_Query", $@"
                    SELECT TOP 1 Status
                    FROM FinalBill
                    WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0
                    ORDER BY Fid DESC");

                var otherData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", other)
                    .FirstOrDefault();

                // active items check
                DynamicParameters checkItems = new DynamicParameters();
                checkItems.Add("@p_Query", $"SELECT COUNT(*) as Count FROM GenerateBill WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0");
                var itemsCountData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", checkItems)
                    .FirstOrDefault();
                int itemsCount = itemsCountData != null ? Convert.ToInt32(itemsCountData.Count ?? 0) : 0;

                // outstanding check
                DynamicParameters checkOut = new DynamicParameters();
                checkOut.Add("@p_Query", $"SELECT COUNT(*) as Count FROM OutstandingBill WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0");
                var outCountData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", checkOut)
                    .FirstOrDefault();
                int outCount = outCountData != null ? Convert.ToInt32(outCountData.Count ?? 0) : 0;

                // prescription check
                DynamicParameters checkPres = new DynamicParameters();
                checkPres.Add("@p_Query", $@"
                    SELECT 
                        (SELECT COUNT(*) FROM MedicineFormula WHERE AppointmentFid = {AppointmentFid} AND ISNULL(IsActive, 1) = 1) +
                        (SELECT COUNT(*) FROM MedicineFormula_Homeopathy WHERE AppointmentFid = {AppointmentFid} AND ISNULL(IsActive, 1) = 1) AS Count");
                var presCountData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", checkPres)
                    .FirstOrDefault();
                int presCount = presCountData != null ? Convert.ToInt32(presCountData.Count ?? 0) : 0;

                bool showDispensing = (otherData != null) || (itemsCount > 0) || (outCount > 0);
                bool hasPrescriptionWithoutBill = false;
                string otherStatus = "Unpaid";

                if (showDispensing)
                {
                    if (otherData != null)
                    {
                        string finalStatus = Convert.ToString(otherData.Status ?? "").ToLower().Trim();

                        if (finalStatus == "paid")
                        {
                            // ================== 3. OUTSTANDING BILL नवीन लॉजिक ==================
                            DynamicParameters outParam = new DynamicParameters();
                            outParam.Add("@p_Query", $@"
                                SELECT 
                                    COUNT(*) AS TotalCount,
                                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(Status))) = 'paid' THEN 1 ELSE 0 END) AS PaidCount,
                                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(Status))) = 'unpaid' THEN 1 ELSE 0 END) AS UnpaidCount
                                FROM OutstandingBill
                                WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0");

                            var outData = BVGWellnessCenterDapparORM
                                .ReturnList<dynamic>("sp_ExecuteQuery", outParam)
                                .FirstOrDefault();

                            if (outData != null)
                            {
                                int outUnpaid = Convert.ToInt32(outData.UnpaidCount ?? 0);
                                int outPaid = Convert.ToInt32(outData.PaidCount ?? 0);

                                if (outUnpaid > 0)
                                {
                                    if (outPaid >= 1)
                                        otherStatus = "Outstanding";
                                    else
                                        otherStatus = "Unpaid";
                                }
                                else
                                {
                                    otherStatus = "Paid";
                                }
                            }
                            else
                            {
                                otherStatus = "Paid";
                            }
                        }
                        else
                        {
                            otherStatus = "Unpaid";
                        }
                    }
                    else
                    {
                        otherStatus = "Unpaid";
                    }

                    billSummary.Add(new { BillName = "Dispensing", Status = otherStatus });
                }
                else if (presCount > 0)
                {
                    hasPrescriptionWithoutBill = true;
                    otherStatus = "NotGenerated";
                    billSummary.Add(new { BillName = "Dispensing", Status = "NotGenerated" });
                }

                bool hasUnpaid = (apStatus == "Unpaid") || (showDispensing && otherStatus == "Unpaid");
                bool hasOutstanding = (apStatus == "Outstanding") || (showDispensing && otherStatus == "Outstanding");

                return Json(new
                {
                    success = true,
                    BillSummary = billSummary,
                    hasUnpaid = hasUnpaid,
                    hasOutstanding = hasOutstanding,
                    hasPrescriptionWithoutBill = hasPrescriptionWithoutBill
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AutoGenerateBillFromPrescription(int AppointmentFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                // 1. Fetch Ayurvedic/General prescriptions
                DynamicParameters paramAyur = new DynamicParameters();
                paramAyur.Add("@p_Query", $@"
                    SELECT 
                        MF.FormID AS ServiceFid, 
                        ISNULL(F.Charges, 0) AS Price, 
                        1 AS Quantity 
                    FROM MedicineFormula MF
                    INNER JOIN MAS_Form F ON MF.FormID = F.FormID
                    WHERE MF.AppointmentFid = {AppointmentFid} AND ISNULL(MF.IsActive, 1) = 1");
                var ayurItems = BVGWellnessCenterDapparORM.ReturnList<PrescriptionBillingItem>("sp_ExecuteQuery", paramAyur).ToList();

                // 2. Fetch Homeopathy prescriptions
                DynamicParameters paramHomeo = new DynamicParameters();
                paramHomeo.Add("@p_Query", $@"
                    SELECT 
                        MF.FormID AS ServiceFid, 
                        ISNULL(F.Charges, 0) AS Price, 
                        1 AS Quantity 
                    FROM MedicineFormula_Homeopathy MF
                    INNER JOIN MAS_Form F ON MF.FormID = F.FormID
                    WHERE MF.AppointmentFid = {AppointmentFid} AND ISNULL(MF.IsActive, 1) = 1");
                var homeoItems = BVGWellnessCenterDapparORM.ReturnList<PrescriptionBillingItem>("sp_ExecuteQuery", paramHomeo).ToList();

                // Combine them
                var allItems = ayurItems.Concat(homeoItems).ToList();

                if (allItems.Count == 0)
                {
                    return Json(new { success = false, message = "No active prescription items found to bill." });
                }

                // Deactivate any existing unfinalized items in GenerateBill for this appointment to start fresh
                DynamicParameters deleteParam = new DynamicParameters();
                deleteParam.Add("@p_Query", $"UPDATE GenerateBill SET Deactivate = 1 WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0");
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", deleteParam);

                decimal totalBillAmount = 0;

                // 3. For each item, insert using sp_SUD_GenerateBill
                foreach (var item in allItems)
                {
                    decimal total = item.Price * item.Quantity;
                    totalBillAmount += total;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProcessType", "Save");
                    param.Add("@p_Bill_Fid", 0);
                    param.Add("@p_Bill_Fid_Encrapted", null);
                    param.Add("@p_ServiceFid", item.ServiceFid);
                    param.Add("@p_Price", item.Price);
                    param.Add("@p_Quantity", item.Quantity);
                    param.Add("@p_Total", total);
                    param.Add("@p_EncounterFid", 0);
                    param.Add("@p_AppointmentFid", AppointmentFid);
                    param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBill", param);
                }

                // 4. Retrieve patient details from PatientAppoinment for this appointment
                DynamicParameters appDetailsParam = new DynamicParameters();
                appDetailsParam.Add("@p_Query", $"SELECT PatientFid, DoctorFid, AppoinmentDate FROM PatientAppoinment WHERE AppointmentFid = {AppointmentFid}");
                var appDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", appDetailsParam).FirstOrDefault();

                int patientFid = 0;
                int doctorFid = 0;
                DateTime? appointmentDate = null;
                if (appDetails != null)
                {
                    patientFid = Convert.ToInt32(appDetails.PatientFid ?? 0);
                    doctorFid = Convert.ToInt32(appDetails.DoctorFid ?? 0);
                    if (appDetails.AppoinmentDate != null)
                    {
                        appointmentDate = Convert.ToDateTime(appDetails.AppoinmentDate);
                    }
                }

                // 5. Check if FinalBill already exists for this appointment
                DynamicParameters checkFinalParam = new DynamicParameters();
                checkFinalParam.Add("@p_Query", $"SELECT Fid, FlageID FROM FinalBill WHERE AppointmentFid = {AppointmentFid} AND Deactivate = 0");
                var existingFinal = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", checkFinalParam).FirstOrDefault();

                string alreadyExists = existingFinal != null ? "Yesh" : null;
                string flageId = existingFinal != null ? Convert.ToString(existingFinal.FlageID) : $"1{AppointmentFid}";

                // 6. Save or Update FinalBill as Unpaid with the total bill amount
                DynamicParameters finalBillParam = new DynamicParameters();
                finalBillParam.Add("@p_ProcessType", "Save");
                finalBillParam.Add("@p_AlreadyExist", alreadyExists);
                finalBillParam.Add("@p_FlageID", flageId);
                finalBillParam.Add("@p_AppointmentFid", AppointmentFid);
                finalBillParam.Add("@p_EncounterFid", 0);
                finalBillParam.Add("@p_TotalAmount", (double)totalBillAmount);
                finalBillParam.Add("@p_DiscountAmt", 0.0);
                finalBillParam.Add("@p_PayableAmount", (double)totalBillAmount);
                finalBillParam.Add("@p_Status", "Unpaid");
                finalBillParam.Add("@p_PatientFid", patientFid);
                finalBillParam.Add("@p_DoctorFid", doctorFid);
                finalBillParam.Add("@p_AppointmentDate", appointmentDate ?? DateTime.Now);
                finalBillParam.Add("@p_PaymentType", null);
                finalBillParam.Add("@p_ReferenceNumber", null);
                finalBillParam.Add("@p_Amount", "0.0");
                finalBillParam.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                finalBillParam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_FinalBill", finalBillParam);

                return Json(new { success = true, message = "Dispensing bill generated successfully from prescription." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private class PrescriptionBillingItem
        {
            public int ServiceFid { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }


    }
}