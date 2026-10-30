using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class CloseEncounterController : Controller
    {
        // GET: Patient/CloseEncounter
        public ActionResult CloseEncounterList(int? EncounterFid,string FromUploadFile)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                if (EncounterFid != null)
                {
                    if (FromUploadFile != null)
                    {
                        DynamicParameters paramFile = new DynamicParameters();
                        paramFile.Add("@p_Query", "select AttachFile,FileType from PatientEncounterFile where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                        var GetFile = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFile);
                        ViewBag.Files = GetFile;
                        ViewBag.UploadedFiles = true;
                    }                   

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_EncountrFid", EncounterFid);
                    var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GetPatientEncounterDetails", param).FirstOrDefault();
                    ViewBag.GetPatientEncounterDetails = result;

                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@p_EncounterFid", EncounterFid);
                    var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBill", paramBill).ToList();
                    ViewBag.GenerateBill = GetBillList;

                    DynamicParameters paramFinal = new DynamicParameters();
                    paramFinal.Add("@p_Query", "select TotalAmount,DiscountAmt,PayableAmount, Status from finalbill where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                    var FinalBill = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinal).FirstOrDefault();
                    ViewBag.FinalBill = FinalBill;

                    DynamicParameters paramFinalbill = new DynamicParameters();
                    paramFinalbill.Add("@p_Query", "select Amount,PaymentReferenceNumber,PaymentMode from finalbill where Deactivate=0 and EncounterFid='" + EncounterFid + "'");
                    var GetFinalbillDetails = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramFinalbill);
                    ViewBag.GetFinalbillDetails = GetFinalbillDetails;


                }
                else
                {
                    ViewBag.FinalBill = "";
                    ViewBag.GenerateBill = "";
                    ViewBag.GetPatientEncounterDetails = "";
                }
                var GetPaidBillList = BVGWellnessCenterDapparORM.DynamicList("sp_List_PaidBills");
                ViewBag.GetPaidBillList = GetPaidBillList;
                return View();
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
          
        }


        #region showUploadedFile
        public ActionResult ShowUploadedFile(string fileName)
        {
            try
            {
              
                string fullPath = fileName;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    return HttpNotFound(); // Return 404 if the file doesn't exist
                }

                // Determine the content type based on the file extension (for example, PDF or Image)
                string contentType = "application/pdf"; // Default for PDF files
                string fileExtension = Path.GetExtension(fileName).ToLower();

                if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    contentType = "image/jpeg"; // Content type for JPEG image
                }
                else if (fileExtension == ".png")
                {
                    contentType = "image/png"; // Content type for PNG image
                }
                else if (fileExtension == ".docx")
                {
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; // Content type for Word document
                }
                // You can add more conditions here for other file types (Excel, Text, etc.)

                // Return the file to the browser
                return File(fullPath, contentType, fileName); // This will either open or download the file
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, show error message, etc.)
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message);
            }
        }

        #endregion
    }
}