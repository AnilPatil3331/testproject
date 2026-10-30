using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class ServicesController : Controller
    {
        string LoginMachinId = Dns.GetHostName();
        // GET: Admin/Services

                        

        public ActionResult ServicesList(string ServiceId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                ClinicService service = new ClinicService();
                if (ServiceId_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_ServiceId_Encrypted", ServiceId_Encrypted);


                    service = BVGWellnessCenterDapparORM.ReturnList<ClinicService>("sp_List_Mas_Service", param1).FirstOrDefault();


                 
                    if (service != null)
                    {
                        // Rooms
                        service.SelectedRoomIds = string.IsNullOrEmpty(service.ServiceRoomIds)
                            ? new List<int>()
                            : service.ServiceRoomIds.Split(',')
                                                    .Select(int.Parse)
                                                    .ToList();


                        service.MedicineNamesList = !string.IsNullOrEmpty(service.MedicineName)
      ? service.MedicineName
          .Split(',')
          .Where(x => !string.IsNullOrWhiteSpace(x))
          .Select(x => x.Trim())
          .ToList()
      : new List<string>();


                      

                        //service.MedicineNamesList = string.IsNullOrEmpty(service.MedicineName)
                        //    ? new List<string>()
                        //    : service.MedicineName.Split(',')
                        //                           .ToList();  // Already string
                    }



                  

                    ViewBag.Update = "update";




                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where Deactivate=0 ");
                    var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param2);
                    ViewBag.GetDoctorName = GetDoctor;
                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(service.ServiceImageFile);
                    ViewBag.ServiceImageFile = base64Image;
                    ViewBag.PraviousServiceImageFile = service.ServiceImageFile;
                    ViewBag.telemedservice = service.telemed_service;
                }
                else
                {
                    ViewBag.GetDoctorName = "";
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
                //Service List
                DynamicParameters paramservicelist = new DynamicParameters();
                paramservicelist.Add("@p_Servicecategory", "Diagnostic Services");
                var data = BVGWellnessCenterDapparORM.DynamicList("sp_List_Mas_Service", paramservicelist);

                ViewBag.ServiceList = data;

                //// Fetch all medicines for dropdown
                //DynamicParameters paramMed = new DynamicParameters();
                //paramMed.Add("@p_Query", "select ServiceFid as Id, MedicineName as Name from Mas_Medicine where Deactivate=0");
                //ViewBag.MedicineList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramMed);



                DynamicParameters paramGroup = new DynamicParameters();
                paramGroup.Add("@p_Query", $@"select ServicesGroupId as Id, ServiceGroupName as Name from ServicesGroup where IsActive = 1");
                ViewBag.ServiveGroup = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramGroup);




                DynamicParameters paramRoom = new DynamicParameters();
                paramRoom.Add("@p_Query", "select ServiceRoomID as Id, RoomName as Name from MAS_ServiceRoomType where IsActive=1");

                // Store result in a local variable
                var roomList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramRoom) ?? new List<DropdownItem>();

                // 2️⃣ Map to SelectListItem with pre-selected values
                ViewBag.GetRoom = roomList.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name,
                    Selected = service.SelectedRoomIds != null && service.SelectedRoomIds.Contains(r.Id)
                }).ToList();






                // Load Medicines
                DynamicParameters paramMed = new DynamicParameters();
                paramMed.Add("@p_Query", "select ServiceFid as Id, MedicineName as Name from Mas_Medicine where Deactivate=0");
                var medList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramMed) ?? new List<DropdownItem>();


                // Map to SelectListItem with pre-selection
                ViewBag.MedicineList = medList.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Selected = service.MedicineNamesList != null && service.MedicineNamesList.Contains(m.Id.ToString())
                }).ToList();

       


                return View(service);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }



        [HttpPost]
        public ActionResult SaveServices(ClinicService service, HttpPostedFileBase file, string PreviousServiceImage)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId)
                                          .AddressList[0].ToString();

                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                // 🔥 FIX: checkbox int handling
                service.IsTherapy = Request.Form["IsTherapy"] == "1" ? 1 : 0;




                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(service.ServiceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_FDate", DateTime.Now);
                param.Add("@p_ServiceFid", service.ServiceFid);
                param.Add("@p_ServiceId_Encrypted", service.ServiceId_Encrypted);
                param.Add("@p_Servicecategory", service.Servicecategory);
                param.Add("@p_ServiceGroupID", service.ServiceGroupID);
               // param.Add("@p_ServiceName", service.ServiceName);
                param.Add("@p_ServiceName", service.MedicineName); // <--- service.ServiceName ऐवजी service.MedicineName वापरा

                //param.Add("@p_ServiceRoomID", service.ServiceRoomID);

                //       param.Add("@p_ServiceRoomID",
                //    service.SelectedRoomIds != null && service.SelectedRoomIds.Any()
                //        ? string.Join(",", service.SelectedRoomIds)
                //        : null
                //);

                param.Add("@p_ServiceRoomIds",
    service.SelectedRoomIds != null && service.SelectedRoomIds.Any()
        ? string.Join(",", service.SelectedRoomIds)
        : null
);

                param.Add("@p_Charges", service.Charges);
                param.Add("@p_telemed_service", service.telemed_service);
                param.Add("@p_ClinicFid", service.ClinicFid);
                param.Add("@p_DoctorId", service.DoctorId);
                param.Add("@p_Duration", service.Duration);
                param.Add("@p_Status", service.Status);
                param.Add("@p_Group", "Group");
                param.Add("@p_MedicineName",
    service.MedicineNamesList != null && service.MedicineNamesList.Any()
        ? string.Join(",", service.MedicineNamesList)
        : null);

              param.Add("@p_MedicinePacking", service.MedicinePacking);
                param.Add("@p_MedicineIndication", service.MedicineIndication);

                // 🔥 THERAPY FLAG
                param.Add("@p_IsTherapy", service.IsTherapy);
                param.Add("@p_IsDouble", service.IsDouble);
                param.Add("@TherapyValue", service.TherapyValue);

                param.Add("@LocalName", service.LocalName);
                param.Add("@Slogan", service.Slogan);
                param.Add("@Uses", service.Uses);
                param.Add("@Description", service.Description);

                param.Add("@Instruments", service.Instruments);
                param.Add("@PrerequisiteProcedure", service.PrerequisiteProcedure);
                param.Add("@MainProcedure", service.MainProcedure);
                param.Add("@PostProcedureCare", service.PostProcedureCare);

                param.Add("@PatientInstructions", service.PatientInstructions);
                param.Add("@RelativeInstructions", service.RelativeInstructions);
                param.Add("@TherapistInstructions", service.TherapistInstructions);
                param.Add("@Precautions", service.Precautions);
                param.Add("@ContraIndications", service.ContraIndications);
                param.Add("@RelatedDisorders", service.RelatedDisorders);

                string FilePath = @"C:\BVGWellnessCenter\ServiceImage\\" + service.ClinicFid + "\\";

                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                        Directory.CreateDirectory(FilePath);

                    File.SaveAs(FilePath + Path.GetFileName(File.FileName));
                    param.Add("@p_ServiceImageFile", FilePath + Path.GetFileName(File.FileName));
                }
                else if (!string.IsNullOrEmpty(PreviousServiceImage))
                {
                    param.Add("@p_ServiceImageFile", PreviousServiceImage);
                }

                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_mas_Service", param);




                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ServicesList", "Services", new { area = "Admin" });
            }

            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #region AddMedicine
        public ActionResult AddMedicine(string ServiceId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Fix ClinicFid for previously saved records where it was saved as 0
                try
                {
                    int currentClinicFid = Session["ClinicFid"] != null ? Convert.ToInt32(Session["ClinicFid"]) : 1;
                    if (currentClinicFid > 0)
                    {
                        string updateQuery = $"UPDATE Mas_Medicine SET ClinicFid = {currentClinicFid} WHERE ClinicFid = 0";
                        BVGWellnessCenterDapparORM.ExecuteQuery(updateQuery);
                    }
                }
                catch (Exception) { }

                ClinicService service = new ClinicService();
                if (ServiceId_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_ServiceId_Encrypted", ServiceId_Encrypted);
                    service = BVGWellnessCenterDapparORM.ReturnList<ClinicService>("sp_List_MAS_Medicine", param1).FirstOrDefault();
                    ViewBag.Update = "update";
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where Deactivate=0 ");
                    var GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param2);
                    ViewBag.GetDoctorName = GetDoctor;
                    string base64Image = BVGWellnessCenterDapparORM.ConvertImageToBase64(service.ServiceImageFile);
                    ViewBag.ServiceImageFile = base64Image;
                    ViewBag.PraviousServiceImageFile = service.ServiceImageFile;
                    ViewBag.telemedservice = service.telemed_service;
                }
                else
                {
                    ViewBag.GetDoctorName = "";
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
                //Service List
                DynamicParameters paramservicelist = new DynamicParameters();
                var Servicecategory = "Medicine";
                paramservicelist.Add("@p_Servicecategory", Servicecategory);
                var data = BVGWellnessCenterDapparORM.DynamicList("sp_List_MAS_Medicine", paramservicelist);

                ViewBag.ServiceList = data;
                //var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_Mas_Service").ToList();
                //ViewBag.ServiceList = data.Where(m => m.Servicecategory == "Medicine").ToList();

                return View(service);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region SaveAddMedicine
        [HttpPost]
        public ActionResult SaveAddMedicine(ClinicService service, HttpPostedFileBase file, string PreviousServiceImage)
        {
            try
            {
                string LoginmachinIp = Dns.GetHostByName(LoginMachinId).AddressList[0].ToString();
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                if (service.ClinicFid == 0 && Session["ClinicFid"] != null)
                {
                    service.ClinicFid = Convert.ToInt32(Session["ClinicFid"]);
                }
                else if (service.ClinicFid == 0)
                {
                    service.ClinicFid = 1; // Default fallback
                }

                HttpPostedFile File = System.Web.HttpContext.Current.Request.Files["file"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", string.IsNullOrEmpty(service.ServiceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MacId", LoginMachinId);
                param.Add("@p_MacIp", LoginmachinIp);
                param.Add("@p_FDate", DateTime.Now);
                param.Add("@p_ServiceFid", service.ServiceFid);
                param.Add("@p_ServiceId_Encrypted", service.ServiceId_Encrypted);
                param.Add("@p_Servicecategory", "Medicine");
                param.Add("@p_ServiceName", service.ServiceName);
                param.Add("@p_Charges", service.Charges);
                param.Add("@p_telemed_service", service.telemed_service);
                param.Add("@p_ClinicFid", service.ClinicFid);
                param.Add("@p_Duration", service.Duration);
                param.Add("@p_Status", service.Status);
                param.Add("@p_MedicineName", service.MedicineName);
                param.Add("@p_MedicinePacking", service.MedicinePacking);
                param.Add("@p_MedicineIndication", service.MedicineIndication);
                //param.Add("@p_Doses", service.Doses);
                param.Add("@p_Instructions", service.Instructions);
                param.Add("@p_Department", service.Department); // <--- ही नवीन लाईन ॲड करा
                string FilePath = @"C:\BVGWellnessCenter\ServiceImage\\" + service.ClinicFid + "\\";

                if (File != null && File.ContentLength > 0)
                {
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    File.SaveAs(FilePath + Path.GetFileName((File.FileName)));
                    param.Add("@p_ServiceImageFile", Convert.ToString(FilePath + Path.GetFileName(File.FileName)));
                }
                else if (PreviousServiceImage != null && PreviousServiceImage != "")
                {
                    param.Add("@p_ServiceImageFile", PreviousServiceImage);
                }
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_Medicine", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("AddMedicine", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region ChangeServiceStatus
        public ActionResult ChangeServiceStatus(string ServiceId_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update Mas_Service set Status='" + Status + "' where  Deactivate=0 and ServiceId_Encrypted='" + ServiceId_Encrypted + "'";
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

                return RedirectToAction("ServicesList", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region GetDoctor
        public ActionResult GetDoctor(int? ClinicFid)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select UserId as Id, FirstName + ' ' + LName as Name from UserMaster where ClinicFid='" + ClinicFid + "' and IsDoctor=1 and Deactivate=0 ");
                var data = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);

                return Json(new { data }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteService
        public ActionResult DeleteService(string ServiceId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_ServiceId_Encrypted", ServiceId_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_mas_Service", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ServicesList", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region DeleteMedicine
        public ActionResult DeleteMedicine(string ServiceId_Encrypted)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Delete");
                param.Add("@p_ServiceId_Encrypted", ServiceId_Encrypted);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_Mas_Medicine", param);
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("AddMedicine", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region ChangeServiceStatus
        public ActionResult ChangeMedicineStatus(string ServiceId_Encrypted, string Status)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                var Query = "Update Mas_Medicine set Status='" + Status + "' where  Deactivate=0 and ServiceId_Encrypted='" + ServiceId_Encrypted + "'";
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

                return RedirectToAction("AddMedicine", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

    }
}