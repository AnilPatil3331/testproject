using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class TaxesController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Patient/Taxes
        public ActionResult Tax()
        {
            try
            {

                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TaxFid_Encrypted", null);
                var result = BVGWellnessCenterDapparORM.DynamicList("sp_List_Taxes", param);
                ViewBag.Taxes = result;

                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
          
        }

        #region ChangeTaxStatus
        public ActionResult ChangeTaxStatus(string TaxFid_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update Taxes set Status='" + Status + "' where  Deactivate=0 and TaxFid_Encrypted='" + TaxFid_Encrypted + "'";
                var data = BVGWellnessCenterDapparORM.ExecuteQuery(Query);
                if (Status == "Active")
                {
                    TempData["Message"] = "Status changed to Active";
                    TempData["Icon"] = "Success";
                }
                else if (Status == "Inactive")
                {
                    TempData["Message"] = "Status changed to Inactive";
                    TempData["Icon"] = "Success";
                }

                return RedirectToAction("Tax", "Taxes", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region AddTax
        public ActionResult AddTax(string TaxFid_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                ClsTaxes taxes = new ClsTaxes();
                 if(TaxFid_Encrypted!=null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_TaxFid_Encrypted", TaxFid_Encrypted);
                    taxes = BVGWellnessCenterDapparORM.ReturnList<ClsTaxes>("sp_List_Taxes", paramList).FirstOrDefault();
                    ViewBag.UpdateProcess = "UpdateProcess";                
                    ViewBag.ClinicFid = taxes.ClinicFid.Split(',').ToList();
                    ViewBag.DoctorFid = taxes.DoctorFid.Split(',').ToList();
                    ViewBag.Service = taxes.Service.Split(',').ToList();
                }
                else
                {
                    ViewBag.ClinicFid = null;
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select Fid as Id, ClinicName as Name from Mas_Clinic where Deactivate=0");
                var GetClinic = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                if (GetClinic.Count() == 0)
                {
                    ViewBag.ClinicName = "";
                }
                else
                {
                    ViewBag.ClinicName = GetClinic;
                }
                DynamicParameters paramDoctor = new DynamicParameters();
                paramDoctor.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where IsDoctor=1 and Deactivate=0 ");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramDoctor);
                ViewBag.GetDoctor = data;
                return View(taxes);
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveTaxes
        [HttpPost]
        public ActionResult SaveTaxes(ClsTaxes tax)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                string ClinicFid = "";
                string DoctorFid = "";
                string Service = "";
                DynamicParameters param = new DynamicParameters();
                if (tax.ListClinicFid == null)
                {
                    ClinicFid = "All";
                }
                else
                {
                    ClinicFid = string.Join(",", tax.ListClinicFid);
                }
                if (tax.ListDoctorFid == null)
                {
                    DoctorFid = "All";
                }
                else
                {
                    DoctorFid = string.Join(",", tax.ListDoctorFid);
                }
                if (tax.ListService == null)
                {
                    Service = "All";
                }
                else
                {
                    Service = string.Join(",", tax.ListService);
                }
                param.Add("@p_ProcessType", string.IsNullOrEmpty(tax.TaxFid_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_TaxFid", tax.TaxFid);
                param.Add("@p_TaxFid_Encrypted", tax.TaxFid_Encrypted);
                param.Add("@p_Tax_Name", tax.Tax_Name);
                param.Add("@p_Tax_Rate", tax.Tax_Rate);
                param.Add("@p_ClinicFid", ClinicFid);
                param.Add("@p_DoctorFid", DoctorFid);
                param.Add("@p_Service", Service);
                param.Add("@p_Status", tax.Status);
                param.Add("@p_TaxType", tax.TaxType);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Tax", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("AddTax", "Taxes",new {area= "Patient" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteTax
        public ActionResult DeleteTax(string TaxFid_Encrypted)
        {
            try
            {
                if(Session["UserId"]==null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_TaxFid_Encrypted", TaxFid_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Tax", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Tax", "Taxes", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

    }
}