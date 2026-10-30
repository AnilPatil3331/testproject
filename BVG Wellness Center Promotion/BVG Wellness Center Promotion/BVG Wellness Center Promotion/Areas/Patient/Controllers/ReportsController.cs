using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Patient/Reports
        public ActionResult Reports()
        {
            try
            {              

                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var ClinicFid=Session["ClinicFid"];
                DynamicParameters paramClinicName = new DynamicParameters();
                paramClinicName.Add("@p_Query", "select ClinicName from mas_clinic where Deactivate=0 and Fid='"+ ClinicFid + "'");
                var GetClinicName = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramClinicName).FirstOrDefault();
                ViewBag.ClinicName = GetClinicName.ClinicName;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ClinicFid", 1);
                var GetClinicRevanue = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetClinicRevenueDetail", param);
                if(GetClinicRevanue!=null)
                {
                    var MonthYear = GetClinicRevanue.Select(item => (string)item.MonthYear).ToList();
                    var TotalAmount = GetClinicRevanue.Select(item => Convert.ToDecimal(item.TotalAmount)).ToList(); // Convert properly
                    //var TotalAmount = GetClinicRevanue.Select(item => (int)item.TotalAmount).ToList();
                    ViewBag.MonthYear = MonthYear;
                    ViewBag.TotalAmount = TotalAmount;
                }else
                {
                    ViewBag.MonthYear = 0;
                    ViewBag.TotalAmount = 0;
                }
                DynamicParameters paramRevenue = new DynamicParameters();
                paramRevenue.Add("@p_Query", "select sum(PayableAmount) as TotalRevenue from FinalBill where Status='Paid' and Deactivate=0");
                var TotalRevenue = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramRevenue).FirstOrDefault();
                if (TotalRevenue != null)
                {
                    if (TotalRevenue.TotalRevenue != null)
                    {
                        ViewBag.TotalRevenue = Convert.ToDouble(String.Format("{0:0.00}", TotalRevenue.TotalRevenue));

                    }
                    else
                    {
                        ViewBag.TotalRevenue = 0;
                    }
                }
                else
                {

                }
                DynamicParameters paramMedicineRevenue = new DynamicParameters();
                paramMedicineRevenue.Add("@p_Query", "select  sum(Total) as TotalMedicineCount from GenerateBill where Status='Close' and Deactivate=0");
                var TotalMedicineRevenue = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramMedicineRevenue).FirstOrDefault();
                if(TotalMedicineRevenue!=null)
                {
                    if(TotalMedicineRevenue.TotalMedicineCount!=null)
                    {
                        ViewBag.TotalMedicineRevenue = Convert.ToDouble(String.Format("{0:0.00}", TotalMedicineRevenue.TotalMedicineCount));
                    }else
                    {
                        ViewBag.TotalMedicineRevenue = 0;
                    }
                  
                }else
                {
                    ViewBag.TotalMedicineRevenue = 0;
                }
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", "select ServiceFid as id,CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Servicecategory='Medicine' and Deactivate=0");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);
                ViewBag.GetMedicine = data;
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
           
        }

        #region GetMedicineTotalRevanue
        public ActionResult GetMedicineTotalRevanue(int MedicineID)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters paramMedicineRevenue = new DynamicParameters();
                paramMedicineRevenue.Add("@p_Query", "select  sum(Total) as TotalMedicineCount from GenerateBill where Status='Close' and Deactivate=0 and ServiceFid='"+ MedicineID + "'");
                var TotalMedicineRevenue = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramMedicineRevenue).FirstOrDefault();
             
                DynamicParameters paramMedicine = new DynamicParameters();
                paramMedicine.Add("@p_Query", "select CONCAT(MedicineName,'',MedicinePacking) as Name from mas_service where Servicecategory='Medicine' and Deactivate=0 and ServiceFid='"+ MedicineID + "'");
                var GetMedicineName = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramMedicine).FirstOrDefault();
                //ViewBag.GetMediName = GetMedicineName.Name;
                return Json(new { TotalMedicineRevenue= TotalMedicineRevenue, GetMedicineName= GetMedicineName } , JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
    }
}