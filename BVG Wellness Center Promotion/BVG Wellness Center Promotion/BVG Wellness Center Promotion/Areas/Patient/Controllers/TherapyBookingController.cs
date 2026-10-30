
using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Areas.Patient.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;

using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Controllers
{
    public class TherapyBookingController : Controller
    {
        // GET: Patient/TherapyBooking
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult TherapyBooking(int? id, int? PatientFid, string FlagId)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                TherapyBookingMaster model = new TherapyBookingMaster();

                // Populate global payment viewbags based on FlagId or id (Naveen Code add kela)
                ViewBag.PreviouslyPaid = 0;
                ViewBag.AlreadyPaid = 0;
                string activeFlagId = FlagId;
                if (string.IsNullOrEmpty(activeFlagId) && id.HasValue && id > 0)
                {
                    DynamicParameters getFlag = new DynamicParameters();
                    getFlag.Add("@p_Query", $"SELECT FlagId FROM MAS_TherapyBooking WHERE TherapyBookingId = {id}");
                    var flagResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", getFlag).FirstOrDefault();
                    if (flagResult != null)
                    {
                        activeFlagId = Convert.ToString(flagResult.FlagId);
                    }
                }

                if (!string.IsNullOrEmpty(activeFlagId))
                {
                    DynamicParameters pPaidDirect = new DynamicParameters();
                    pPaidDirect.Add("@p_Query", $@"
                        SELECT ISNULL(SUM(Amount),0) AS TotalPaid
                        FROM FinalservicesBill
                        WHERE BillFlageID = '{activeFlagId}'
                        AND Deactivate = 0
                    ");
                    var paymentDataDirect = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pPaidDirect).FirstOrDefault();
                    decimal paidDirect = paymentDataDirect != null ? Convert.ToDecimal(paymentDataDirect.TotalPaid) : 0;
                    ViewBag.PreviouslyPaid = paidDirect;
                    ViewBag.AlreadyPaid = paidDirect;
                }

                /* ===============================
                   ?? DISEASE DROPDOWN
                =============================== */
                DynamicParameters paramDisease = new DynamicParameters();
                paramDisease.Add("@p_Query", "SELECT DiseaseId as ID, DiseaseName as Name FROM MAS_Disease WHERE IsActive = 1");
                ViewBag.GetDiseaseList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramDisease);
                ViewBag.PatientFid = PatientFid;


                DynamicParameters paramGroup = new DynamicParameters();
                paramGroup.Add("@p_Query", $@"select ServicesGroupId as Id, ServiceGroupName as Name from ServicesGroup where IsActive = 1");
                ViewBag.ServiveGroup = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramGroup);

                /* ===============================
                   ?? PATIENT DETAILS (if PatientFid provided)
                =============================== */
                if (PatientFid.HasValue && PatientFid.Value > 0)
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_Query", $@"
                        SELECT 
                            PA.*, 
                            P.*, 
                            CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                            MC.*,
                            CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName
                        FROM Patient P
                        LEFT JOIN PatientAppoinment PA ON PA.PatientFid = P.PatientFid
                        LEFT JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
                        LEFT JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
                        WHERE P.PatientFid = {PatientFid}
                    ");

                    var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param2).FirstOrDefault();
                    ViewBag.TodayPatients = TodayPatients;
                    ViewBag.AppointmentFid = TodayPatients?.AppointmentFid;

                    if (PatientFid != null)
                    {
                        var AdvisedTherapy = new DynamicParameters();
                        AdvisedTherapy.Add("@p_Query", $@"
                            SELECT 
                                AT.ATId, 
                                ISNULL(S.ServiceName, 'No Service') AS ServiceName, 
                                AT.ServiceFid,
                                AT.ServiceCategories, 
                                AT.DaysNumber, 
                                AT.Frequency, 
                                AT.InstructionPatinet, 
                                AT.InstructionTherapist, 
                                AT.Consent 
                            FROM (
                                SELECT ATId, ServiceFid, ServiceCategories, DaysNumber, Frequency, InstructionPatinet, InstructionTherapist, Consent, AppointmentFid FROM Tbl_AdvisedTherapy
                                UNION ALL
                                SELECT ATId, ServiceFid, ServiceCategories, DaysNumber, Frequency, InstructionPatinet, InstructionTherapist, Consent, AppointmentFid FROM AdvisedTherapyYoga
                                UNION ALL
                                SELECT ATId, ServiceFid, ServiceCategories, DaysNumber, Frequency, InstructionPatinet, InstructionTherapist, Consent, AppointmentFid FROM Tbl_AdvisedTherapy_Homeopathy
                            ) AT 
                            LEFT JOIN MAS_Service S ON AT.ServiceFid = S.ServiceFid 
                            WHERE AT.AppointmentFid = (
                                SELECT TOP 1 AppointmentFid 
                                FROM PatientAppoinment 
                                WHERE PatientFid = {PatientFid}
                                ORDER BY AppointmentFid DESC
                            )
                            ORDER BY AT.ATId DESC
                        ");
                        ViewBag.AdvisedTherapyResult = BVGWellnessCenterDapparORM.ReturnList<AdvisedTherapyVMs>("sp_ExecuteQuery", AdvisedTherapy).ToList();
                    }
                }

                /* ===============================
                   ?? DROPDOWNS
                =============================== */
                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", @"SELECT ServiceFid AS Id, ServiceName AS Name , ServiceGroupID FROM Mas_Service WHERE Status='Active' AND Deactivate=0 AND IsTherapy=1");
                ViewBag.Getdata = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramService);

                DynamicParameters paramRoom = new DynamicParameters();
                paramRoom.Add("@p_Query", @"SELECT ServiceRoomID as Id, RoomName as Name FROM MAS_ServiceRoomType WHERE IsActive = 1");
                ViewBag.GetRoom = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramRoom);

                DynamicParameters paramReceptionist = new DynamicParameters();
                paramReceptionist.Add("@p_Query", "SELECT UserId AS Id, ISNULL(FirstName,'') + ' ' + ISNULL(LName,'') AS Name FROM UserMaster WHERE Deactivate=0 AND IsReceptionist=1");
                ViewBag.GetReceptionists = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramReceptionist);

                DynamicParameters paramDoctor = new DynamicParameters();
                paramDoctor.Add("@p_Query", "SELECT UserId AS Id, FirstName + ' ' + LName AS Name FROM UserMaster WHERE Deactivate=0 AND IsDoctor=1");
                ViewBag.GetDoctor = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramDoctor);

                DynamicParameters paramMed = new DynamicParameters();
                paramMed.Add("@p_Query", "SELECT ServiceFid AS Id, MedicineName AS Name FROM Mas_Medicine WHERE Deactivate = 0");
                var medList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", paramMed) ?? new List<DropdownItem>();
                ViewBag.MedicineList = medList.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();



                /* ===============================
                   ?? THERAPY LIST TABLE (WITH SLOTS GROUPED)
                =============================== */
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Query", @" 
                    SELECT 
                        TB.TherapyBookingId,
                        P.FirstName + ' ' + P.LName AS PatientName,
                        S.ServiceName,
                        SR.RoomName,
                        T.FirstName + ' ' + T.LName AS TherapistName,
                        TB.FromDate,
                        TB.ToDate,
 	                    P.PatientFid,
                        TB.TotalAmount,
                        TB.FinalPaybalAmount,
                        TB.FlagId,
                        ISNULL((
                            SELECT COUNT(1) 
                            FROM FinalservicesBill FSB
                            WHERE FSB.BillFlageID = TB.FlagId 
                              AND TB.FlagId IS NOT NULL
                              AND FSB.Status = 'Paid'
                              AND FSB.Deactivate = 0
                        ), 0) AS TotalPaid
                    FROM MAS_TherapyBooking TB
                    LEFT JOIN MAS_Service S ON S.ServiceFid = TB.ServiceFid
                    LEFT JOIN MAS_ServiceRoomType SR ON SR.ServiceRoomID = TB.ServiceRoomID
                    LEFT JOIN UserMaster T ON T.UserId = TB.TherapistId
                    LEFT JOIN Patient P ON P.PatientFid = TB.PatientFid
                    WHERE TB.IsActive = 1
                    ORDER BY TB.TherapyBookingId DESC
                ");

                var therapyList = BVGWellnessCenterDapparORM.ReturnList<TherapyBookingListVM>("sp_ExecuteQuery", paramList).ToList();

                // ? Load slots for each booking
                foreach (var booking in therapyList)
                {
                    // ? ?? add ???
                    booking.IsPaid = IsTherapyBookingPaid(booking.FlagId);
                    System.Diagnostics.Debug.WriteLine($"BookingId: {booking.TherapyBookingId} | FlagId: {booking.FlagId} | IsPaid: {booking.IsPaid}");

                    DynamicParameters slotParam = new DynamicParameters();
                    slotParam.Add("@p_Query", $@"
                        SELECT SlotDate, SlotStart, SlotEnd 
                        FROM MAS_TherapySlotBooking 
                        WHERE TherapyBookingId = {booking.TherapyBookingId} 
                          AND IsBooked = 1
                        ORDER BY SlotDate, SlotStart
                    ");
                    booking.Slots = BVGWellnessCenterDapparORM.ReturnList<TherapySlotDisplayVM>("sp_ExecuteQuery", slotParam).ToList();
                }
                ViewBag.TherapyList = therapyList;

                /* ===============================
                   ?? EDIT MODE
                =============================== */
                if (id.HasValue && id > 0)
                {
                    DynamicParameters paramEdit = new DynamicParameters();
                    paramEdit.Add("@p_Query", $"SELECT * FROM MAS_TherapyBooking WHERE TherapyBookingId = {id}");
                    var editData = BVGWellnessCenterDapparORM.ReturnList<TherapyBookingMaster>("sp_ExecuteQuery", paramEdit).FirstOrDefault();

                    if (editData != null)
                    {
                        model = editData;
                        ViewBag.PatientFid = model.PatientFid;
                        // Naveen Code add kela
                        ViewBag.IsPaid = IsTherapyBookingPaid(model.FlagId);

                        if (!string.IsNullOrEmpty(model.FlagId))
                        {
                            DynamicParameters pTotal = new DynamicParameters();
                            pTotal.Add("@p_Query", $@"
                                SELECT ISNULL(SUM(Amount), 0) as CalculatedTotal 
                                FROM FinalservicesBill 
                                WHERE BillFlageID = '{model.FlagId}' AND Deactivate = 0");

                            var totalResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pTotal).FirstOrDefault();
                            if (totalResult != null)
                            {
                                model.TotalAmount = Convert.ToDecimal(totalResult.CalculatedTotal);
                            }
                        }
                        // --- ???? ??? ???? ---
                        // FlagId ???? ??? ?????? ???? ????? ?????
                        // ? FETCH LAST PAYMENT USING FLAG ID
                        ViewBag.PreviouslyPaid = 0;
                        ViewBag.AlreadyPaid = 0; // ðŸ‘ˆ Naveen Code add kela
                        ViewBag.RemainingAmount = 0;
                        if (!string.IsNullOrEmpty(editData.FlagId))
                        {
                            DynamicParameters pPaid = new DynamicParameters();
                            pPaid.Add("@p_Query", $@"
                                                    SELECT ISNULL(SUM(Amount),0) AS TotalPaid
                                                    FROM FinalservicesBill
                                                    WHERE BillFlageID = '{editData.FlagId}'
                                                    AND Deactivate = 0
                                                ");
                            var paymentData =
                        BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>(
                            "sp_ExecuteQuery",
                            pPaid
                        )
                        .FirstOrDefault();

                            decimal previousPaid = 0;

                            if (paymentData != null &&
                                paymentData.TotalPaid != null)
                            {
                                previousPaid =
                                    Convert.ToDecimal(paymentData.TotalPaid);
                            }

                            // Juna Code comment kela
                            // ViewBag.PreviouslyPaid = previousPaid;
                            ViewBag.PreviouslyPaid = previousPaid;
                            ViewBag.AlreadyPaid = previousPaid;

                            // ? FINAL PAYABLE
                            decimal finalAmount =
                                model.FinalPaybalAmount > 0
                                ? Convert.ToDecimal(model.FinalPaybalAmount)
                                : Convert.ToDecimal(model.TotalAmount);

                            // ? REMAINING
                            decimal remainingAmount =
                                finalAmount - previousPaid;

                            if (remainingAmount < 0)
                            {
                                remainingAmount = 0;
                            }

                            ViewBag.RemainingAmount = remainingAmount;

                            // ? LAST PAYMENT
                            DynamicParameters payParam =
                                new DynamicParameters();

                            payParam.Add("@p_Query", $@"
                        SELECT TOP 1 *
                        FROM FinalservicesBill
                        WHERE BillFlageID = '{editData.FlagId}'
                        AND Deactivate = 0
                        ORDER BY Fid DESC
                    ");

                            ViewBag.LastPayment =
                                BVGWellnessCenterDapparORM
                                .ReturnList<dynamic>(
                                    "sp_ExecuteQuery",
                                    payParam
                                )
                                .FirstOrDefault();

                            // Juna Code comment kela (Naveen Code add kela for Discountamt)
                            if (ViewBag.LastPayment != null)
                            {
                                var dict = (IDictionary<string, object>)ViewBag.LastPayment;
                                if (dict.ContainsKey("DiscountAmt"))
                                {
                                    ViewBag.Discountamt = dict["DiscountAmt"];
                                }
                            }
                        }
                        else
                        {
                            ViewBag.PreviouslyPaid = 0;
                        }
                        // ? FIX: Set ServiceGroupID from ServiceFid
                        if (model.ServiceFid > 0)
                        {
                            DynamicParameters paramGroupp = new DynamicParameters();
                            paramGroupp.Add("@p_Query", $"SELECT ServiceGroupID FROM Mas_Service WHERE ServiceFid = {model.ServiceFid}");
                            var serviceData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramGroupp).FirstOrDefault();
                            if (serviceData != null)
                            {
                                model.ServiceGroupID = serviceData.ServiceGroupID;
                                ViewBag.SelectedServiceGroupId = model.ServiceGroupID;
                            }
                        }



                        model.AdvisedBy = editData.AdvisedBy;
                        model.AttendantName = editData.AttendantName;
                        model.SelfName = editData.SelfName ?? editData.Self;
                        model.IPDCategory = editData.IPDCategory;
                        model.FacilityType = editData.FacilityType;
                        model.ConsentTaken = editData.ConsentTaken;
                        model.ECGStatus = editData.ECGStatus;
                        model.BloodTestStatus = editData.BloodTestStatus;
                        model.XRayStatus = editData.XRayStatus;
                        model.OtherTestsStatus = editData.OtherTestsStatus;
                        model.Duration = editData.Duration;
                        model.RoomFrequency = editData.RoomFrequency;        // ? ADD
                        model.NumberOfSessions = editData.NumberOfSessions;

                        model.NumberOfSessions = editData.NumberOfSessions;
                        // ? LOAD MEDICINES - FIXED
                        // ? LOAD MEDICINES - COMPLETE WORKING CODE
                        // ? LOAD MEDICINES - WITH MEDICINEFID AND ISCHARGEABLE
                        DynamicParameters medParam = new DynamicParameters();
                        medParam.Add("@p_Query", $@"
                            SELECT 
                                MedicineName, 
                                MedicineFid,
                                ISNULL(IsChargeable, 0) AS IsChargeable,
                                ISNULL(Doses, '') AS Doses, 
                                ISNULL(Frequncy, '') AS Frequncy, 
                                Duration as DurationM, 
                                Quantity,
                                ISNULL(DurationType, 'Days') AS DurationType
                            FROM Therpy_Medicine 
                            WHERE TherapyBookingId = {id}
                        ");
                        var medicines = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", medParam).ToList();

                        if (medicines.Any())
                        {
                            var medNames = new List<string>();
                            var medFids = new List<int>();
                            var isChargeableList = new List<bool>();
                            var dosesList = new List<string>();
                            var freqList = new List<string>();
                            var durationList = new List<int?>();
                            var quantityList = new List<int?>();
                            var durationTypeList = new List<string>();

                            foreach (var med in medicines)
                            {
                                medNames.Add(med.MedicineName?.ToString() ?? "");
                                medFids.Add(med.MedicineFid != null ? Convert.ToInt32(med.MedicineFid) : 0);
                                isChargeableList.Add(med.IsChargeable != null && Convert.ToBoolean(med.IsChargeable));
                                dosesList.Add(med.Doses?.ToString() ?? "");
                                freqList.Add(med.Frequncy?.ToString() ?? "");

                                // Duration
                                int durVal = 0;
                                if (med.DurationM != null && int.TryParse(med.DurationM.ToString(), out durVal))
                                    durationList.Add(durVal);
                                else
                                    durationList.Add((int?)null);

                                // Quantity
                                int qtyVal = 0;
                                if (med.Quantity != null && int.TryParse(med.Quantity.ToString(), out qtyVal))
                                    quantityList.Add(qtyVal);
                                else
                                    quantityList.Add((int?)null);

                                durationTypeList.Add(med.DurationType?.ToString() ?? "Days");
                            }

                            model.MedicineName = medNames.ToArray();
                            model.MedicineFid = medFids.ToArray();
                            model.IsChargeableArray = isChargeableList.ToArray();
                            model.Doses = dosesList.ToArray();
                            model.Frequncy = freqList.ToArray();
                            model.DurationM = durationList.ToArray();
                            model.Quantity = quantityList.ToArray();
                            model.DurationType = durationTypeList.ToArray();

                            // Create JSON for View
                            var medicinesJsonObj = new
                            {
                                MedicineName = medNames.ToArray(),
                                MedicineFid = medFids.ToArray(),
                                IsChargeable = isChargeableList.ToArray(),
                                Doses = dosesList.ToArray(),
                                Frequncy = freqList.ToArray(),
                                Duration = durationList.Select(d => d.HasValue ? d.Value.ToString() : "").ToArray(),
                                Quantity = quantityList.Select(q => q.HasValue ? q.Value.ToString() : "").ToArray(),
                                DurationType = durationTypeList.ToArray()
                            };

                            ViewBag.MedicinesJson = Newtonsoft.Json.JsonConvert.SerializeObject(medicinesJsonObj);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No medicines found");
                            ViewBag.MedicinesJson = "null";
                        }




                        // ? Load Slots for Edit Mode - Format properly for SelectedSlots
                        DynamicParameters slotParam = new DynamicParameters();
                        slotParam.Add("@p_Query", $"SELECT * FROM MAS_TherapySlotBooking WHERE TherapyBookingId = {id} AND IsBooked = 1");
                        var slots = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", slotParam).ToList();

                        if (slots.Any())
                        {
                            // ? Create SelectedSlots list for JSON serialization
                            var selectedSlotsList = new List<TherapySlotVM>();
                            foreach (var slot in slots)
                            {
                                selectedSlotsList.Add(new TherapySlotVM
                                {
                                    date = Convert.ToDateTime(slot.SlotDate).ToString("yyyy-MM-dd"),
                                    startTime = Convert.ToDateTime(slot.SlotStart).ToString("h:mm tt"),
                                    endTime = Convert.ToDateTime(slot.SlotEnd).ToString("h:mm tt"),
                                    ServiceRoomID = (int)slot.ServiceRoomID,
                                    IsBooked = 1
                                });
                            }
                            model.SelectedSlots = selectedSlotsList;

                            // ? Also set the hidden field value for JavaScript
                            ViewBag.SelectedSlotsJson = JsonConvert.SerializeObject(selectedSlotsList);
                        }

                        ViewBag.Update = true;
                    }
                }
                if (!string.IsNullOrEmpty(FlagId))
                {
                    DynamicParameters paramBill = new DynamicParameters();
                    paramBill.Add("@FlagId", FlagId);

                    var GetBillList = BVGWellnessCenterDapparORM
                                        .ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill)
                                        .ToList();

                    ViewBag.GenerateBill = GetBillList;
                    ViewBag.FlagId = FlagId;

                    // ? List ???? PatientFid ?? (First record)
                    ViewBag.PatientFid = GetBillList.Any()
                                            ? GetBillList.First().PatientFid
                                            : null;

                    // Set ViewBag.AppointmentFid and TodayPatients if PatientFid is set but they are not loaded yet
                    if (ViewBag.PatientFid != null && ViewBag.TodayPatients == null)
                    {
                        int patFid = Convert.ToInt32(ViewBag.PatientFid);
                        DynamicParameters param2 = new DynamicParameters();
                        param2.Add("@p_Query", $@"
                            SELECT 
                                PA.*, 
                                P.*, 
                                CONCAT(P.FirstName, ' ', P.LName) AS PatientName,
                                MC.*,
                                CONCAT(UM.FirstName, ' ', UM.LName) AS DoctorName
                            FROM Patient P
                            LEFT JOIN PatientAppoinment PA ON PA.PatientFid = P.PatientFid
                            LEFT JOIN Mas_Clinic MC ON PA.ClinicFid = MC.Fid
                            LEFT JOIN UserMaster UM ON UM.UserId = PA.DoctorFid
                            WHERE P.PatientFid = {patFid}
                        ");
                        var TodayPatients = BVGWellnessCenterDapparORM.ReturnList<EHR>("sp_ExecuteQuery", param2).FirstOrDefault();
                        ViewBag.TodayPatients = TodayPatients;
                        ViewBag.AppointmentFid = TodayPatients?.AppointmentFid;
                    }

                    // Juna Code comment kela (Naveen Code add kela for Discount and Last Payment when FlagId is directly in GET)
                    DynamicParameters payParamDirect = new DynamicParameters();
                    payParamDirect.Add("@p_Query", $@"
                        SELECT TOP 1 *
                        FROM FinalservicesBill
                        WHERE BillFlageID = '{FlagId}'
                        AND Deactivate = 0
                        ORDER BY Fid DESC
                    ");
                    ViewBag.LastPayment = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", payParamDirect).FirstOrDefault();
                    if (ViewBag.LastPayment != null)
                    {
                        var dict = (IDictionary<string, object>)ViewBag.LastPayment;
                        if (dict.ContainsKey("DiscountAmt"))
                        {
                            ViewBag.Discountamt = dict["DiscountAmt"];
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new TherapyBookingMaster());
            }
        }


        [HttpPost]
        public ActionResult SaveTherapyBooking(TherapyBookingMaster model, string SelectedSlots)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home");

                //string FlagId;

                //if (!string.IsNullOrWhiteSpace(model.FlagId) && model.FlagId != "0")
                //{
                //    FlagId = model.FlagId;
                //}
                //else
                //{
                //    FlagId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                //}

                string FlagId = "";

                // ✅ EDIT MODE
                if (model.TherapyBookingId > 0)
                {
                    DynamicParameters getFlagParam = new DynamicParameters();

                    getFlagParam.Add("@p_Query", $@"
        SELECT TOP 1 *
        FROM MAS_TherapyBooking
        WHERE TherapyBookingId = {model.TherapyBookingId}
    ");

                    var existingData = BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>("sp_ExecuteQuery", getFlagParam)
                        .FirstOrDefault();

                    if (existingData != null)
                    {
                        var dict = (IDictionary<string, object>)existingData;
                        if (dict.ContainsKey("FlagId") && dict["FlagId"] != null)
                        {
                            FlagId = Convert.ToString(dict["FlagId"]);
                        }

                        // Restore disabled form fields from existing record
                        if (model.ServiceFid <= 0 && dict.ContainsKey("ServiceFid") && dict["ServiceFid"] != null)
                        {
                            model.ServiceFid = Convert.ToInt32(dict["ServiceFid"]);
                        }
                        if (model.ServiceGroupID <= 0 && dict.ContainsKey("ServiceGroupID") && dict["ServiceGroupID"] != null)
                        {
                            model.ServiceGroupID = Convert.ToInt32(dict["ServiceGroupID"]);
                        }
                        if (model.PatientFid <= 0 && dict.ContainsKey("PatientFid") && dict["PatientFid"] != null)
                        {
                            model.PatientFid = Convert.ToInt32(dict["PatientFid"]);
                        }
                        if (model.DoctorFid <= 0 && dict.ContainsKey("DoctorFid") && dict["DoctorFid"] != null)
                        {
                            model.DoctorFid = Convert.ToInt32(dict["DoctorFid"]);
                        }
                        if (model.TherapistId <= 0 && dict.ContainsKey("TherapistId") && dict["TherapistId"] != null)
                        {
                            model.TherapistId = Convert.ToInt32(dict["TherapistId"]);
                        }
                        if (model.ServiceRoomID <= 0 && dict.ContainsKey("ServiceRoomID") && dict["ServiceRoomID"] != null)
                        {
                            model.ServiceRoomID = Convert.ToInt32(dict["ServiceRoomID"]);
                        }
                        if ((model.NumberOfSessions ?? 0) <= 0 && dict.ContainsKey("NumberOfSessions") && dict["NumberOfSessions"] != null)
                        {
                            model.NumberOfSessions = Convert.ToInt32(dict["NumberOfSessions"]);
                        }
                        if (string.IsNullOrEmpty(model.RoomFrequency) && dict.ContainsKey("RoomFrequency") && dict["RoomFrequency"] != null)
                        {
                            model.RoomFrequency = Convert.ToString(dict["RoomFrequency"]);
                        }
                        if (model.DiseaseId <= 0 && dict.ContainsKey("DiseaseId") && dict["DiseaseId"] != null)
                        {
                            model.DiseaseId = Convert.ToInt32(dict["DiseaseId"]);
                        }
                        if (model.Duration <= 0 && dict.ContainsKey("Duration") && dict["Duration"] != null)
                        {
                            model.Duration = Convert.ToInt32(dict["Duration"]);
                        }
                        if (!model.ECGStatus && dict.ContainsKey("ECGStatus") && dict["ECGStatus"] != null)
                        {
                            model.ECGStatus = Convert.ToBoolean(dict["ECGStatus"]);
                        }
                        if (!model.BloodTestStatus && dict.ContainsKey("BloodTestStatus") && dict["BloodTestStatus"] != null)
                        {
                            model.BloodTestStatus = Convert.ToBoolean(dict["BloodTestStatus"]);
                        }
                        if (!model.XRayStatus && dict.ContainsKey("XRayStatus") && dict["XRayStatus"] != null)
                        {
                            model.XRayStatus = Convert.ToBoolean(dict["XRayStatus"]);
                        }
                        if (!model.OtherTestsStatus && dict.ContainsKey("OtherTestsStatus") && dict["OtherTestsStatus"] != null)
                        {
                            model.OtherTestsStatus = Convert.ToBoolean(dict["OtherTestsStatus"]);
                        }
                        if (!model.ConsentTaken && dict.ContainsKey("ConsentTaken") && dict["ConsentTaken"] != null)
                        {
                            model.ConsentTaken = Convert.ToBoolean(dict["ConsentTaken"]);
                        }
                        if (!model.PatientPrepared && dict.ContainsKey("PatientPrepared") && dict["PatientPrepared"] != null)
                        {
                            model.PatientPrepared = Convert.ToBoolean(dict["PatientPrepared"]);
                        }
                        if (!model.TherapyPrepared && dict.ContainsKey("TherapyPrepared") && dict["TherapyPrepared"] != null)
                        {
                            model.TherapyPrepared = Convert.ToBoolean(dict["TherapyPrepared"]);
                        }
                    }
                }

                // ✅ NEW BOOKING
                if (string.IsNullOrWhiteSpace(FlagId))
                {
                    FlagId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                }
                bool isNewBooking = (model.TherapyBookingId == 0);

                System.Diagnostics.Debug.WriteLine("=== POST RECEIVED ===");
                System.Diagnostics.Debug.WriteLine("SelectedSlots parameter: '" + SelectedSlots + "'");

                // Deserialize only once
                if (!string.IsNullOrEmpty(SelectedSlots))
                {
                    model.SelectedSlots = JsonConvert.DeserializeObject<List<TherapySlotVM>>(SelectedSlots);
                    System.Diagnostics.Debug.WriteLine($"Deserialized {model.SelectedSlots.Count} slots");

                    foreach (var slot in model.SelectedSlots)
                    {
                        System.Diagnostics.Debug.WriteLine($"Slot - Date: {slot.date}, Start: {slot.startTime}, End: {slot.endTime}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: SelectedSlots is NULL or EMPTY!");
                    model.SelectedSlots = new List<TherapySlotVM>();
                }

                // Fetch service charges before calling SP_SaveTherapyBooking to set TotalAmount correctly
                DynamicParameters paramPricePre = new DynamicParameters();
                paramPricePre.Add("@p_Query", $"SELECT Charges FROM Mas_Service WHERE ServiceFid = '{model.ServiceFid}' AND Status = 'Active' AND Deactivate = 0 AND IsTherapy = 1");
                var serviceDataPre = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramPricePre).FirstOrDefault();
                decimal pricePre = serviceDataPre != null ? Convert.ToDecimal(serviceDataPre.Charges) : 0;
                model.Total = (float)(pricePre * (model.NumberOfSessions ?? 0));
                model.TotalAmount = (decimal)model.Total;

                DynamicParameters param = new DynamicParameters();
                param.Add("@TherapyBookingId", model.TherapyBookingId);
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@FlagId", FlagId);
                param.Add("@DiseaseId", model.DiseaseId);
                param.Add("@ECGStatus", model.ECGStatus);
                param.Add("@BloodTestStatus", model.BloodTestStatus);
                param.Add("@XRayStatus", model.XRayStatus);
                param.Add("@OtherTestsStatus", model.OtherTestsStatus);

                param.Add("@ServiceGroupID", model.ServiceGroupID);

                param.Add("@ServiceFid", model.ServiceFid);
                param.Add("@ServiceRoomID", model.ServiceRoomID);
                param.Add("@DoctorFid", model.DoctorFid);
                param.Add("@TherapistId", model.TherapistId);
                param.Add("@Duration", model.Duration);
                param.Add("@RoomFrequency", model.RoomFrequency);        // ✅ ADD
                param.Add("@NumberOfSessions", model.NumberOfSessions);  // ✅ ADD
                param.Add("@FromDate", model.FromDate);
                param.Add("@ToDate", model.ToDate);
                param.Add("@TotalAmount", model.TotalAmount);
                param.Add("@AdvancePaid", model.AdvancePaid);
                param.Add("@BalanceAmount", model.BalanceAmount);
                param.Add("@Charges", model.Charges);
                param.Add("@LocalName", model.LocalName);
                param.Add("@Slogan", model.Slogan);
                param.Add("@Uses", model.Uses);
                param.Add("@Description", model.Description);
                param.Add("@Instruments", model.Instruments);
                param.Add("@PrerequisiteProcedure", model.PrerequisiteProcedure);
                param.Add("@MainProcedure", model.MainProcedure);
                param.Add("@PostProcedureCare", model.PostProcedureCare);
                param.Add("@PatientInstructions", model.PatientInstructions);
                param.Add("@RelativeInstructions", model.RelativeInstructions);
                param.Add("@TherapistInstructions", model.TherapistInstructions);
                param.Add("@Precautions", model.Precautions);
                param.Add("@ContraIndications", model.ContraIndications);
                param.Add("@RelatedDisorders", model.RelatedDisorders);
                param.Add("@PatientPrepared", model.PatientPrepared);
                param.Add("@TherapyPrepared", model.TherapyPrepared);
                param.Add("@AdvisedBy", model.AdvisedBy);
                param.Add("@AttendantName", model.AttendantName);
                param.Add("@SelfName", model.SelfName ?? model.Self);
                param.Add("@IPDCategory", model.IPDCategory);
                param.Add("@FacilityType", model.FacilityType);
                param.Add("@ConsentTaken", model.ConsentTaken);
                param.Add("@CommunityType", model.CommunityType != null ? string.Join(",", model.CommunityType) : null);
                param.Add("@LMPDate", model.LMPDate);
                param.Add("@CreatedBy", Convert.ToInt32(Session["UserID"]));

                var savedBooking = BVGWellnessCenterDapparORM
                    .ReturnList<TherapyBookingMaster>("SP_SaveTherapyBooking", param)
                    .FirstOrDefault();

                int therapyBookingId = savedBooking?.TherapyBookingId ?? 0;

                System.Diagnostics.Debug.WriteLine($"TherapyBookingId: {therapyBookingId}");

                // ✅ SAVE SLOTS
                if (therapyBookingId > 0 && model.SelectedSlots != null && model.SelectedSlots.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Saving {model.SelectedSlots.Count} slots");

                    // Delete all existing slots for this booking
                    DynamicParameters deleteAllSlotsParam = new DynamicParameters();
                    deleteAllSlotsParam.Add("@TherapyBookingId", therapyBookingId);
                    BVGWellnessCenterDapparORM.ExcuteSp("SP_DeleteAllTherapySlots", deleteAllSlotsParam);

                    System.Diagnostics.Debug.WriteLine("Deleted existing slots");

                    // Insert all selected slots
                    foreach (var slot in model.SelectedSlots)
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing slot - Date: {slot.date}, Start: {slot.startTime}, End: {slot.endTime}");

                        if (string.IsNullOrEmpty(slot.startTime) || string.IsNullOrEmpty(slot.endTime) || string.IsNullOrEmpty(slot.date))
                        {
                            System.Diagnostics.Debug.WriteLine("Skipping - empty values");
                            continue;
                        }

                        DateTime slotStart;
                        DateTime slotEnd;
                        DateTime slotDate;

                        // Safe parsing - compatible with older C#
                        if (!DateTime.TryParse(slot.startTime, out slotStart))
                        {
                            System.Diagnostics.Debug.WriteLine("Skipping - startTime parse failed");
                            continue;
                        }
                        if (!DateTime.TryParse(slot.endTime, out slotEnd))
                        {
                            System.Diagnostics.Debug.WriteLine("Skipping - endTime parse failed");
                            continue;
                        }
                        if (!DateTime.TryParse(slot.date, out slotDate))
                        {
                            System.Diagnostics.Debug.WriteLine("Skipping - date parse failed");
                            continue;
                        }

                        slotStart = slotDate.Date.Add(slotStart.TimeOfDay);
                        slotEnd = slotDate.Date.Add(slotEnd.TimeOfDay);

                        System.Diagnostics.Debug.WriteLine($"Parsed - SlotDate: {slotDate}, SlotStart: {slotStart}, SlotEnd: {slotEnd}");

                        DynamicParameters slotParam = new DynamicParameters();
                        slotParam.Add("@TherapyBookingId", therapyBookingId);
                        slotParam.Add("@ServiceRoomID", model.ServiceRoomID);
                        slotParam.Add("@SlotDate", slotDate.Date);
                        slotParam.Add("@SlotStart", slotStart);
                        slotParam.Add("@SlotEnd", slotEnd);
                        slotParam.Add("@CreatedBy", Convert.ToInt32(Session["UserID"]));

                        BVGWellnessCenterDapparORM.ExcuteSp("SP_SaveTherapySlotBooking", slotParam);
                        System.Diagnostics.Debug.WriteLine("Slot saved");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No slots to save - therapyBookingId: " + therapyBookingId + ", SelectedSlots count: " + (model.SelectedSlots?.Count ?? 0));
                }





                // ✅ MEDICINE SAVE - CORRECTED CODE (हे कॉपी करून पेस्ट कर)
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== SAVING MEDICINES ===");

                    // Delete existing medicines
                    DynamicParameters delParam = new DynamicParameters();
                    delParam.Add("@TherapyBookingId", therapyBookingId);
                    BVGWellnessCenterDapparORM.ExcuteSp("SP_DeleteTherapyMedicines", delParam);

                    int savedCount = 0;

                    // Loop through all form keys - View मध्ये MedicineFid_0, MedicineFid_1 असे आहेत
                    for (int i = 0; i < 100; i++)  // Max 100 medicines
                    {
                        string fidKey = $"MedicineFid_{i}";
                        string fidValue = Request.Form[fidKey];

                        if (string.IsNullOrEmpty(fidValue))
                            continue;

                        int medicineFid = Convert.ToInt32(fidValue);
                        string medicineName = Request.Form[$"MedicineName_{i}"] ?? "";
                        string doses = Request.Form[$"Doses_{i}"] ?? "";
                        string frequency = Request.Form[$"Frequncy_{i}"] ?? "";

                        int duration = 0;
                        int.TryParse(Request.Form[$"DurationM_{i}"], out duration);

                        int quantity = 1;
                        int.TryParse(Request.Form[$"Quantity_{i}"], out quantity);

                        string durationType = Request.Form[$"DurationType_{i}"] ?? "Days";

                        bool isChargeable = Request.Form[$"IsChargeable_{i}"] == "true";

                        System.Diagnostics.Debug.WriteLine($"Saving Medicine {i}: Fid={medicineFid}, Name={medicineName}, Chargeable={isChargeable}");

                        DynamicParameters medParam = new DynamicParameters();
                        medParam.Add("@TherapyBookingId", therapyBookingId);
                        medParam.Add("@MedicineName", medicineName);
                        medParam.Add("@MedicineFid", medicineFid);
                        medParam.Add("@Doses", doses);
                        medParam.Add("@Frequncy", "0");
                        medParam.Add("@Duration", "0");
                        medParam.Add("@Quantity", quantity > 0 ? quantity : 1);
                        medParam.Add("@DurationType", durationType);
                        medParam.Add("@IsChargeable", isChargeable);
                        medParam.Add("@Status", "Active");
                        medParam.Add("@Deactivate", 0);

                        BVGWellnessCenterDapparORM.ExcuteSp("SP_Therpymedicine", medParam);
                        savedCount++;
                    }

                    System.Diagnostics.Debug.WriteLine($"Total Medicines Saved: {savedCount}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Medicine Save Error: {ex.Message}");
                }







                DynamicParameters paramService = new DynamicParameters();
                paramService.Add("@p_Query", $@"SELECT ServiceFid AS Id, ServiceName AS Name , Charges  as Price FROM Mas_Service WHERE ServiceFid ='{model.ServiceFid}' and  Status='Active' AND Deactivate=0 AND IsTherapy=1");
                var serviceData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", paramService).FirstOrDefault();
                ViewBag.Getdata = serviceData;
                ViewBag.Price = serviceData != null ? serviceData.Price : 0;



                // ✅ Total calculate
                model.Total = ViewBag.Price * (model.NumberOfSessions ?? 0);

                // FlagId logic same ठेव

                string processType = "Save";
                string existingBillEncrypted = "";

                if (model.TherapyBookingId > 0)
                {
                    DynamicParameters chkBillParam = new DynamicParameters();

                    chkBillParam.Add("@p_Query", $@"
        SELECT TOP 1 Bill_Fid_Encrapted
        FROM GenerateBillServices
        WHERE FlagId = '{FlagId}'
        AND Deactivate = 0
    ");

                    var existingBill = BVGWellnessCenterDapparORM
                        .ReturnList<dynamic>("sp_ExecuteQuery", chkBillParam)
                        .FirstOrDefault();

                    if (existingBill != null)
                    {
                        processType = "Update";
                        existingBillEncrypted = Convert.ToString(existingBill.Bill_Fid_Encrapted);
                    }
                }
                // ✅ Correct parameter object use kar
                DynamicParameters paramBILL = new DynamicParameters();

                //paramBILL.Add("@p_ProcessType", string.IsNullOrEmpty(model.Bill_Fid_Encrapted) ? "Save" : "Update");
                paramBILL.Add("@p_ProcessType", processType);
                paramBILL.Add("@p_Bill_Fid", model.Bill_Fid);
                paramBILL.Add("@p_Bill_Fid_Encrapted",
     !string.IsNullOrEmpty(existingBillEncrypted)
         ? existingBillEncrypted
         : model.Bill_Fid_Encrapted);

                paramBILL.Add("@p_ServiceFid", model.ServiceFid);
                paramBILL.Add("@p_Price", ViewBag.Price);
                paramBILL.Add("@p_Quantity", model.NumberOfSessions);
                paramBILL.Add("@p_Total", model.Total);
                paramBILL.Add("@p_PatientID", model.PatientFid);
                paramBILL.Add("@FlagId", FlagId); // ⚠️ IMPORTANT FIX

                paramBILL.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramBILL.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                // ❌ तू param pass करतोयस (bug)
                // ✅ paramBILL pass कर
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_GenerateBillServices", paramBILL);

                TempData["Message"] = paramBILL.Get<string>("@p_Message");
                TempData["Icon"] = paramBILL.Get<string>("@p_Icon");

                // Load bill list
                DynamicParameters paramBill = new DynamicParameters();
                paramBill.Add("@FlagId", FlagId);
                var GetBillList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_List_GenerateBillServices", paramBill).ToList();
                ViewBag.GenerateBill = GetBillList;

                ViewBag.FlagId = FlagId;
                // ✅ Already paid amount fetch करा
                DynamicParameters paramPaid = new DynamicParameters();
                paramPaid.Add("@p_Query", $@"
    SELECT ISNULL(SUM(Amount), 0) AS TotalPaid 
    FROM FinalservicesBill 
    WHERE BillFlageID = '{FlagId}' AND Deactivate = 0
");
                var paidData = BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", paramPaid)
                    .FirstOrDefault();

                decimal alreadyPaid = paidData != null ? Convert.ToDecimal(paidData.TotalPaid) : 0;
                ViewBag.AlreadyPaid = alreadyPaid;

                // ✅ Update OutstandingBillServices immediately so lists and details are accurate
                try
                {
                    decimal totalBillAmt = (decimal)model.Total;
                    decimal remainingAmt = totalBillAmt - alreadyPaid;
                    if (remainingAmt < 0) remainingAmt = 0;
                    string outStatus = remainingAmt <= 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    sumInsUpd.Add("@AppointmentFid", model.AppointmentFid ?? 0);
                    sumInsUpd.Add("@TotalAmount", totalBillAmt);
                    sumInsUpd.Add("@PatientFid", model.PatientFid);
                    sumInsUpd.Add("@BillFlageID", FlagId);
                    sumInsUpd.Add("@TotalPaid", alreadyPaid);
                    sumInsUpd.Add("@RemainingAmount", remainingAmt);
                    sumInsUpd.Add("@Status", outStatus);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_ServicesOutstanding", sumInsUpd);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to update outstanding services summary: {ex.Message}");
                }

                //if (ViewBag.GenerateBill != null)
                //{
                //    TempData["OpenBillPopup"] = true;
                //}
                //else
                //{
                //    // जर आधीच 'Paid' असेल तर पॉपअप उघडणार नाही
                //    TempData["OpenBillPopup"] = false;
                //}
                //if (isNewBooking && GetBillList != null && GetBillList.Count > 0)
                //{
                //    TempData["OpenBillPopup"] = true;
                //}
                //else
                //{
                //    TempData["OpenBillPopup"] = false;
                //}
                // Get original values from form
                // Get original values from form
                int originalSessions = 0;
                int.TryParse(Request.Form["OriginalSessions"], out originalSessions);

                int originalMedCount = 0;
                int.TryParse(Request.Form["OriginalMedicineCount"], out originalMedCount);

                // Count current medicines
                int currentMedCount = 0;

                for (int i = 0; i < 100; i++)
                {
                    if (!string.IsNullOrEmpty(Request.Form[$"MedicineFid_{i}"]))
                        currentMedCount++;
                }

                // ✅ session increased?
                // ✅ session changed? (वाढवले किंवा कमी केले दोन्ही चालेल)
                bool sessionsChanged =
                    (model.NumberOfSessions ?? 0) != originalSessions;

                // ✅ medicines changed? (तुम्हाला हवे असल्यास औषधे कमी केल्यावरही पॉपअप उघडेल)
                bool medicinesChanged =
                    currentMedCount != originalMedCount;

                // ✅ FINAL CONDITION
                bool shouldOpenBill =
                    GetBillList != null &&
                    GetBillList.Count > 0 &&
                    (
                        isNewBooking ||
                        sessionsChanged ||
                        medicinesChanged
                    );

                TempData["OpenBillPopup"] = shouldOpenBill;

                // DEBUG
                System.Diagnostics.Debug.WriteLine("=== BILL POPUP DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"isNewBooking: {isNewBooking}");
                System.Diagnostics.Debug.WriteLine($"originalSessions: {originalSessions}");
                System.Diagnostics.Debug.WriteLine($"currentSessions: {model.NumberOfSessions}");
                System.Diagnostics.Debug.WriteLine($"originalMedCount: {originalMedCount}");
                System.Diagnostics.Debug.WriteLine($"currentMedCount: {currentMedCount}");
                System.Diagnostics.Debug.WriteLine($"sessionsIncreased: {sessionsChanged}");
                System.Diagnostics.Debug.WriteLine($"medicinesAdded: {medicinesChanged}");
                System.Diagnostics.Debug.WriteLine($"shouldOpenBill: {shouldOpenBill}");


                // ✅ Send Email Confirmation to Patient
                //if (therapyBookingId > 0 && model.PatientFid > 0)
                //{
                //    try
                //    {
                //        DynamicParameters pInfo = new DynamicParameters();
                //        pInfo.Add("@p_Query", $@"
                //            SELECT 
                //                (P.FirstName + ' ' + ISNULL(P.LName, '')) AS PatientName,
                //                P.Email AS PatientEmail,
                //                S.ServiceName,
                //                (U.FirstName + ' ' + ISNULL(U.LName, '')) AS DocName
                //            FROM Patient P
                //            LEFT JOIN MAS_Service S ON S.ServiceFid = {model.ServiceFid}
                //            LEFT JOIN UserMaster U ON U.UserId = {model.DoctorFid}
                //            WHERE P.PatientFid = {model.PatientFid}");

                //        var info = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", pInfo).FirstOrDefault();
                //        if (info != null && info.PatientEmail != null && !string.IsNullOrEmpty(info.PatientEmail.ToString()))
                //        {
                //            string pEmail = info.PatientEmail.ToString().Trim();
                //            if (!string.IsNullOrEmpty(pEmail))
                //            {
                //                string pName = info.PatientName?.ToString() ?? "Patient";
                //                string sName = info.ServiceName?.ToString() ?? "Therapy Service";
                //                string docName = info.DocName?.ToString() ?? "Doctor";
                //                string clinicName = Session["ClinicName"]?.ToString() ?? "BVG Wellness Center";

                //                string fromDateStr = model.FromDate.HasValue ? model.FromDate.Value.ToString("dd/MM/yyyy") : "-";
                //                string toDateStr = model.ToDate.HasValue ? model.ToDate.Value.ToString("dd/MM/yyyy") : "-";
                //                int sessionsCount = model.NumberOfSessions ?? 0;

                //                SendTherapyBookingEmail(pEmail, pName, sName, docName, fromDateStr, toDateStr, sessionsCount, clinicName, model.SelectedSlots);
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        System.Diagnostics.Debug.WriteLine($"Failed to send therapy booking email: {ex.Message}");
                //    }
                //}



                TempData["Message"] = "Booking saved successfully";
                if (shouldOpenBill)
                {
                    return RedirectToAction("TherapyBooking", new { id = therapyBookingId, PatientFid = model.PatientFid, FlagId = FlagId });
                }
                else
                {
                    return RedirectToAction("TherapyBookingList");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                TempData["Message"] = ex.Message;
                return RedirectToAction("TherapyBooking");
            }
        }


        private void SendTherapyBookingEmail(string patientEmail, string patientName, string serviceName, string doctorName, string fromDate, string toDate, int numberOfSessions, string clinicName, List<TherapySlotDisplayVM> selectedSlots)
        {
            try
            {
                var fromAddress = new MailAddress("apiprobus@gmail.com", clinicName);
                var toAddress = new MailAddress(patientEmail);
                string fromPassword = "nibd kmnr fbdb pguf";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                string slotsHtml = "";
                if (selectedSlots != null && selectedSlots.Count > 0)
                {
                    // Merge consecutive slots (e.g. 4:00 to 4:30 and 4:30 to 5:00 becomes 4:00 to 5:00)
                    var mergedSlots = new List<TherapySlotDisplayVM>();
                    var groupedByDate = selectedSlots
                        .Where(s => s.SlotDate != null)
                        .GroupBy(s => s.SlotDate.Date)
                        .OrderBy(g => g.Key);

                    foreach (var group in groupedByDate)
                    {
                        var sortedSlots = group.OrderBy(s => s.SlotStart).ToList();
                        if (!sortedSlots.Any()) continue;

                        var currentMerged = new TherapySlotDisplayVM
                        {
                            SlotDate = group.Key,
                            SlotStart = sortedSlots[0].SlotStart,
                            SlotEnd = sortedSlots[0].SlotEnd
                        };

                        for (int i = 1; i < sortedSlots.Count; i++)
                        {
                            var nextSlot = sortedSlots[i];

                            // Check if nextSlot starts exactly when currentMerged ends (within 1 min tolerance)
                            if (Math.Abs((nextSlot.SlotStart - currentMerged.SlotEnd).TotalMinutes) < 1.0)
                            {
                                currentMerged.SlotEnd = nextSlot.SlotEnd;
                            }
                            else
                            {
                                mergedSlots.Add(currentMerged);
                                currentMerged = new TherapySlotDisplayVM
                                {
                                    SlotDate = group.Key,
                                    SlotStart = nextSlot.SlotStart,
                                    SlotEnd = nextSlot.SlotEnd
                                };
                            }
                        }
                        mergedSlots.Add(currentMerged);
                    }

                    slotsHtml = @"<h4 style='color: #2c3e50; margin-top: 20px; border-bottom: 1px solid #e0e0e0; padding-bottom: 5px;'>Scheduled Sessions:</h4>
                                  <table style='width: 100%; border-collapse: collapse;'>
                                    <thead>
                                        <tr style='background-color: #f2f4f8;'>
                                            <th style='padding: 8px; border: 1px solid #dddddd; text-align: left;'>Date</th>
                                            <th style='padding: 8px; border: 1px solid #dddddd; text-align: left;'>Time Slot</th>
                                        </tr>
                                    </thead>
                                    <tbody>";
                    foreach (var slot in mergedSlots)
                    {
                        string dateStr = slot.SlotDate.ToString("dd/MM/yyyy");
                        string startStr = slot.SlotStart.ToString("h:mm tt");
                        string endStr = slot.SlotEnd.ToString("h:mm tt");

                        slotsHtml += $@"<tr>
                                            <td style='padding: 8px; border: 1px solid #dddddd;'>{dateStr}</td>
                                            <td style='padding: 8px; border: 1px solid #dddddd;'>{startStr} - {endStr}</td>
                                        </tr>";
                    }
                    slotsHtml += "</tbody></table>";
                }

                string emailBody = $@"
                    <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #fdfdfd;'>
                        <div style='text-align: center; border-bottom: 2px solid #5881ed; padding-bottom: 20px; margin-bottom: 20px;'>
                            <h2 style='color: #2c3e50; margin: 0;'>Therapy Booking Confirmation</h2>
                            <p style='color: #7f8c8d; margin: 5px 0 0 0;'>{clinicName}</p>
                        </div>
                        
                        <div style='color: #34495e; line-height: 1.6;'>
                            <p>Dear <b>{patientName}</b>,</p>
                            <p>Your therapy session booking has been successfully confirmed at <b>{clinicName}</b>. Below are the details of your therapy plan:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                                <tr style='background-color: #f2f4f8;'>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left; width: 35%;'>Therapy Service</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{serviceName}</td>
                                </tr>
                                <tr>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>Advised By</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>Dr. {doctorName}</td>
                                </tr>
                                <tr style='background-color: #f2f4f8;'>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>Total Sessions</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{numberOfSessions} Sessions</td>
                                </tr>
                                <tr>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>Start Date</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{fromDate}</td>
                                </tr>
                                <tr style='background-color: #f2f4f8;'>
                                    <th style='padding: 10px; border: 1px solid #dddddd; text-align: left;'>End Date</th>
                                    <td style='padding: 10px; border: 1px solid #dddddd;'>{toDate}</td>
                                </tr>
                            </table>
                            
                            {slotsHtml}
                            
                            <p style='margin-top: 30px;'>If you have any questions or need to reschedule, please contact the clinic reception.</p>
                            <p style='margin-bottom: 0;'>Warm regards,<br/><b>Team {clinicName}</b></p>
                        </div>
                        
                        <div style='margin-top: 30px; padding-top: 15px; border-top: 1px solid #e0e0e0; text-align: center; font-size: 12px; color: #95a5a6;'>
                            This is an automated email confirmation. Please do not reply directly to this message.
                        </div>
                    </div>";

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Therapy Booking Confirmation - " + clinicName,
                    Body = emailBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Email Error: " + ex.Message);
            }
        }



        private bool IsTherapyBookingPaid(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
                return false;

            DynamicParameters param = new DynamicParameters();
            // Juna Code comment kela
            // param.Add("@p_Query", $@"
            // SELECT COUNT(1) 
            // FROM FinalservicesBill 
            // WHERE BillFlageID = '{flagId}' 
            //   AND Status = 'Paid' 
            //   AND Deactivate = 0
            // ");
            // Naveen Code add kela
            param.Add("@p_Query", $@"
                SELECT CASE WHEN ISNULL(SUM(Amount), 0) >= ISNULL(MAX(PayableAmount), 0) AND ISNULL(MAX(PayableAmount), 0) > 0 THEN 1 ELSE 0 END
                FROM FinalservicesBill 
                WHERE BillFlageID = '{flagId}' 
                  AND Deactivate = 0
            ");

            var result = BVGWellnessCenterDapparORM
                .ReturnList<dynamic>("sp_ExecuteQuery", param)
                .FirstOrDefault();

            int count = 0;
            if (result != null)
                count = Convert.ToInt32(((IDictionary<string, object>)result).Values.First());

            return count > 0;
        }

        // Helper class - Controller च्या आत किंवा बाहेर ठेवा
        public class TherapyMedicineData
        {
            public int MedicineFid { get; set; }
            public string MedicineName { get; set; }
            public string Doses { get; set; }
            public string Frequncy { get; set; }
            public int Duration { get; set; }
            public int Quantity { get; set; }
            public string DurationType { get; set; }
            public bool IsChargeable { get; set; }
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


        #region GetTherapyRoomSlots
        [HttpGet]
        public ActionResult GetTherapyRoomSlots(int ServiceRoomID, DateTime FromDate, DateTime ToDate)

        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                DynamicParameters param = new DynamicParameters();

                param.Add("@FromDate", FromDate);
                param.Add("@ToDate", ToDate);
                param.Add("@ServiceRoomID", ServiceRoomID);

                var slots = BVGWellnessCenterDapparORM
                                .DynamicList("sp_List_TherapyRoomSlots", param);

                return Json(slots, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        [HttpGet]
        public JsonResult GetRoomsByService(int serviceId)
        {
            DynamicParameters param = new DynamicParameters();

            //param.Add("@p_Query", $@" SELECT SR.ServiceRoomID AS Id, SR.RoomName AS Name FROM MAS_ServiceRoomType SR INNER JOIN Mas_Service S ON ',' + S.ServiceRoomIds + ',' LIKE '%,' + CAST(SR.ServiceRoomID AS VARCHAR) + ',%' WHERE SR.IsActive = 1 AND S.ServiceFid = {serviceId} ");

            param.Add("@p_Query", $@"SELECT 
            SR.ServiceRoomID AS Id,
            SR.RoomName AS Name
        FROM MAS_ServiceRoomType SR
       
        WHERE 
            SR.IsActive = 1
           ");
            var rooms = BVGWellnessCenterDapparORM
                            .ReturnList<DropdownItem>("sp_ExecuteQuery", param);

            return Json(rooms, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetServiceAutoData(int serviceId)
        {
            try
            {
                // Service डिटेल्स घे
                string query = $@"SELECT 
                            ServiceFid,
                            LocalName, 
                            Duration, 
                            IsDouble, 
                            Slogan, 
                            Uses, 
                            Description, 
                            Charges,
                            Instruments, 
                            PrerequisiteProcedure, 
                            MainProcedure, 
                            PostProcedureCare,
                            PatientInstructions, 
                            RelativeInstructions, 
                            TherapistInstructions,
                            Precautions, 
                            ContraIndications, 
                            RelatedDisorders,
                            MedicineName
                          FROM Mas_Service 
                          WHERE ServiceFid = {serviceId} AND Deactivate = 0";

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", query);
                var serviceData = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();

                // ✅ CHECK IF serviceData IS NULL
                if (serviceData == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Service not found for ID: " + serviceId
                    }, JsonRequestBehavior.AllowGet);
                }

                List<object> medicines = new List<object>();

                // Medicine IDs parse कर
                if (!string.IsNullOrEmpty(serviceData.MedicineName))
                {
                    string medIds = serviceData.MedicineName.ToString();
                    string[] ids = medIds.Split(',');

                    foreach (string idStr in ids)
                    {
                        if (!string.IsNullOrEmpty(idStr.Trim()))
                        {
                            int medId = Convert.ToInt32(idStr.Trim());
                            string singleQuery = $@"SELECT ServiceFid as MedicineServiceFid, MedicineName, Doses, Frequncy 
                                             FROM Mas_Medicine 
                                             WHERE ServiceFid = {medId} AND Deactivate = 0";

                            DynamicParameters singleParam = new DynamicParameters();
                            singleParam.Add("@p_Query", singleQuery);
                            var med = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", singleParam).FirstOrDefault();

                            if (med != null)
                            {
                                medicines.Add(new
                                {
                                    MedicineServiceFid = med.MedicineServiceFid,
                                    MedicineName = med.MedicineName?.ToString() ?? "",
                                    Doses = med.Doses?.ToString() ?? "",
                                    Frequncy = med.Frequncy?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }

                var result = new
                {
                    success = true,
                    serviceData = new
                    {
                        LocalName = serviceData.LocalName?.ToString() ?? "",
                        Slogan = serviceData.Slogan?.ToString() ?? "",
                        Uses = serviceData.Uses?.ToString() ?? "",
                        Description = serviceData.Description?.ToString() ?? "",
                        Duration = serviceData.Duration?.ToString() ?? "",
                        Charges = serviceData.Charges?.ToString() ?? "",
                        IsDouble = serviceData.IsDouble?.ToString() ?? "False",
                        Instruments = serviceData.Instruments?.ToString() ?? "",
                        PrerequisiteProcedure = serviceData.PrerequisiteProcedure?.ToString() ?? "",
                        MainProcedure = serviceData.MainProcedure?.ToString() ?? "",
                        PostProcedureCare = serviceData.PostProcedureCare?.ToString() ?? "",
                        PatientInstructions = serviceData.PatientInstructions?.ToString() ?? "",
                        RelativeInstructions = serviceData.RelativeInstructions?.ToString() ?? "",
                        TherapistInstructions = serviceData.TherapistInstructions?.ToString() ?? "",
                        Precautions = serviceData.Precautions?.ToString() ?? "",
                        ContraIndications = serviceData.ContraIndications?.ToString() ?? "",
                        RelatedDisorders = serviceData.RelatedDisorders?.ToString() ?? ""
                    },
                    medicines = medicines
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult DeleteTherapySlot(int TherapyBookingId, DateTime SlotDate)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@TherapyBookingId", TherapyBookingId);
                param.Add("@SlotDate", SlotDate.Date);

                // Execute SP that deletes slots + master if last slot
                BVGWellnessCenterDapparORM.ExcuteSp("SP_DeleteTherapySlotByExactTime", param);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetBookedSlots(int serviceRoomId, string fromDate, string toDate, int therapyBookingId = 0, int serviceId = 0)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ServiceRoomID", serviceRoomId);
                param.Add("@FromDate", DateTime.Parse(fromDate));
                param.Add("@ToDate", DateTime.Parse(toDate));
                param.Add("@CurrentTherapyBookingId", therapyBookingId);
                param.Add("@ServiceId", serviceId);

                var bookedSlots = BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_GetBookedSlotsForRoom", param);

                Console.WriteLine($"Booked slots count: {bookedSlots?.Count() ?? 0}");

                var result = new List<object>();

                if (bookedSlots != null && bookedSlots.Any())
                {
                    foreach (var slot in bookedSlots)
                    {
                        try
                        {
                            // Use correct column names
                            var slotDate = Convert.ToDateTime(slot.SlotDate);
                            var slotStart = Convert.ToDateTime(slot.SlotStart);
                            var slotEnd = Convert.ToDateTime(slot.SlotEnd);
                            var bookingCount = Convert.ToInt32(slot.BookingCount);

                            result.Add(new
                            {
                                SlotDate = slotDate.ToString("yyyy-MM-dd"),
                                SlotStart = slotStart.ToString("h:mm tt"),
                                SlotEnd = slotEnd.ToString("h:mm tt"),
                                BookingCount = bookingCount
                            });

                            System.Diagnostics.Debug.WriteLine($"Added: {slotDate:yyyy-MM-dd} {slotStart:h:mm tt} Count: {bookingCount}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing slot: {ex.Message}");
                        }
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookedSlots Error: {ex.Message}");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        //       [HttpGet]
        //public JsonResult GetBookedSlots(int serviceRoomId, string fromDate, string toDate, int therapyBookingId = 0, int serviceId = 0)
        //{
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@ServiceRoomID", serviceRoomId);
        //        param.Add("@FromDate", DateTime.Parse(fromDate));
        //        param.Add("@ToDate", DateTime.Parse(toDate));
        //        param.Add("@CurrentTherapyBookingId", therapyBookingId);
        //        param.Add("@ServiceId", serviceId);

        //        var bookedSlots = BVGWellnessCenterDapparORM.ReturnList<dynamic>("SP_GetBookedSlotsForRoom", param);

        //        // ? Ensure we return an array, not an object
        //        var result = new List<object>();

        //        if (bookedSlots != null && bookedSlots.Any())
        //        {
        //            result = bookedSlots.Select(s => new {
        //                SlotDate = Convert.ToDateTime(s.SlotDate).ToString("yyyy-MM-dd"),
        //                SlotStart = Convert.ToDateTime(s.SlotStart).ToString("h:mm tt"),
        //                SlotEnd = Convert.ToDateTime(s.SlotEnd).ToString("h:mm tt"),
        //                BookingCount = 1, // Will be grouped in JS
        //                IsDoubleService = serviceId == 99 ? true : false // For testing
        //            }).Cast<object>().ToList();
        //        }

        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new List<object>(), JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult TherapyBookingList()
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                // Get all therapy bookings with slots
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Query", @" 
    SELECT 
        TB.TherapyBookingId,
        P.PatientFid,
        P.FirstName + ' ' + P.LName AS PatientName,
        S.ServiceName,
        SR.RoomName,
        T.FirstName + ' ' + T.LName AS TherapistName,
        TB.FromDate,
        TB.ToDate,
       
 TB.FlagId,
TB.BookingSource,
ISNULL(LatestBill.TotalAmount, 0) AS TotalAmount,
        ISNULL(TB.TherapyStatus, 'Pending') AS Status
    FROM MAS_TherapyBooking TB
LEFT JOIN (
        -- ???????? BillFlageID ???? ??????? ??????? ?????
        SELECT BillFlageID, TotalAmount,
               ROW_NUMBER() OVER (PARTITION BY BillFlageID ORDER BY Fid DESC) as RowNum
        FROM FinalservicesBill
        WHERE Deactivate = 0
    ) AS LatestBill ON TB.FlagId = LatestBill.BillFlageID AND LatestBill.RowNum = 1
    LEFT JOIN MAS_Service S ON S.ServiceFid = TB.ServiceFid
    LEFT JOIN MAS_ServiceRoomType SR ON SR.ServiceRoomID = TB.ServiceRoomID
    LEFT JOIN UserMaster T ON T.UserId = TB.TherapistId
    LEFT JOIN Patient P ON P.PatientFid = TB.PatientFid
    WHERE TB.IsActive = 1
      AND (TB.BookingStatus = 'Confirmed' OR TB.BookingStatus IS NULL)
    ORDER BY TB.TherapyBookingId DESC
");

                var therapyList = BVGWellnessCenterDapparORM.ReturnList<TherapyBookingListVM>("sp_ExecuteQuery", paramList).ToList();
                // ? ???????? booking ???? paid check ???
                foreach (var booking in therapyList)
                {
                    booking.IsPaid = IsTherapyBookingPaid(booking.FlagId);

                    // slots load
                    DynamicParameters slotParam = new DynamicParameters();
                    slotParam.Add("@p_Query", $@"
        SELECT SlotDate, SlotStart, SlotEnd 
        FROM MAS_TherapySlotBooking 
        WHERE TherapyBookingId = {booking.TherapyBookingId} 
          AND IsBooked = 1
        ORDER BY SlotDate, SlotStart
    ");
                    booking.Slots = BVGWellnessCenterDapparORM
                        .ReturnList<TherapySlotDisplayVM>("sp_ExecuteQuery", slotParam).ToList();
                }
                // DEBUG: Check if data is coming
                System.Diagnostics.Debug.WriteLine($"Therapy List Count: {therapyList.Count}");

                // If no data, return empty list
                if (therapyList == null || therapyList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No data found in database");
                    ViewBag.TotalCount = 0;
                    return View(new List<TherapyBookingListVM>());
                }

                // Load and group slots for each booking
                foreach (var booking in therapyList)
                {
                    try
                    {
                        DynamicParameters slotParam = new DynamicParameters();
                        slotParam.Add("@p_Query", $@"
            SELECT 
                SlotDate, 
                SlotStart, 
                SlotEnd 
            FROM MAS_TherapySlotBooking 
            WHERE TherapyBookingId = {booking.TherapyBookingId} 
              AND IsBooked = 1
            ORDER BY SlotDate, SlotStart
        ");

                        var slots = BVGWellnessCenterDapparORM.ReturnList<TherapySlotDisplayVM>("sp_ExecuteQuery", slotParam).ToList();
                        booking.Slots = slots;

                        // Group slots by date and contiguous time intervals
                        if (slots != null && slots.Any())
                        {
                            var grouped = new List<GroupedSlotVM>();
                            var slotsByDate = slots
                                .Where(s => s.SlotDate != null)
                                .GroupBy(s => Convert.ToDateTime(s.SlotDate).Date)
                                .OrderBy(g => g.Key);

                            foreach (var dateGroup in slotsByDate)
                            {
                                var date = dateGroup.Key;
                                var sortedSlots = dateGroup.OrderBy(s => Convert.ToDateTime(s.SlotStart)).ToList();

                                if (sortedSlots.Any())
                                {
                                    var currentStart = Convert.ToDateTime(sortedSlots[0].SlotStart);
                                    var currentEnd = Convert.ToDateTime(sortedSlots[0].SlotEnd);

                                    for (int i = 1; i < sortedSlots.Count; i++)
                                    {
                                        var nextStart = Convert.ToDateTime(sortedSlots[i].SlotStart);
                                        var nextEnd = Convert.ToDateTime(sortedSlots[i].SlotEnd);

                                        // If contiguous or overlapping (within 1 min tolerance), merge them
                                        if (nextStart <= currentEnd || Math.Abs((nextStart - currentEnd).TotalMinutes) <= 1.0)
                                        {
                                            if (nextEnd > currentEnd)
                                            {
                                                currentEnd = nextEnd;
                                            }
                                        }
                                        else
                                        {
                                            grouped.Add(new GroupedSlotVM
                                            {
                                                Date = date,
                                                StartTime = currentStart.ToString("HH:mm"),
                                                EndTime = currentEnd.ToString("HH:mm")
                                            });
                                            currentStart = nextStart;
                                            currentEnd = nextEnd;
                                        }
                                    }

                                    grouped.Add(new GroupedSlotVM
                                    {
                                        Date = date,
                                        StartTime = currentStart.ToString("HH:mm"),
                                        EndTime = currentEnd.ToString("HH:mm")
                                    });
                                }
                            }
                            booking.GroupedSlots = grouped;
                        }
                        else
                        {
                            booking.GroupedSlots = new List<GroupedSlotVM>();
                        }

                        System.Diagnostics.Debug.WriteLine($"Booking {booking.TherapyBookingId} has {booking.Slots?.Count ?? 0} slots and {booking.GroupedSlots?.Count ?? 0} grouped slots");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading slots for booking {booking.TherapyBookingId}: {ex.Message}");
                        booking.Slots = new List<TherapySlotDisplayVM>();
                        booking.GroupedSlots = new List<GroupedSlotVM>();
                    }
                }

                ViewBag.TotalCount = therapyList.Count;
                return View(therapyList);
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TherapyBookingList: {Ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {Ex.StackTrace}");
                ViewBag.Error = Ex.Message;
                return View(new List<TherapyBookingListVM>());
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

                bool isEditMode = false;

                DynamicParameters chkBill = new DynamicParameters();

                chkBill.Add("@p_Query", $@"
    SELECT TOP 1 BillFlageID
    FROM FinalservicesBill
    WHERE BillFlageID = '{BillFlageID}'
    AND Deactivate = 0
");

                var existingBill =
                    BVGWellnessCenterDapparORM
                    .ReturnList<dynamic>("sp_ExecuteQuery", chkBill)
                    .FirstOrDefault();

                if (existingBill != null)
                {
                    isEditMode = true;
                }

                for (int i = 0; i < paymentList.Count(); i++)
                {
                    //var FlageId = "";
                    //if (ISOutStandingAmt == "Yes")
                    //{
                    //    //FlageId = $"{i + 1}{paymentList[i].AppointmentFid}{paymentList[i].Id}";
                    //    //FlageId = Guid.NewGuid().ToString();
                    //    FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                    //}
                    //else
                    //{
                    //    FlageId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                    //}
                    var FlageId = "";

                    if (isEditMode)
                    {
                        // KEEP SAME BILL FLAG ID
                        FlageId = BillFlageID;
                    }
                    else
                    {
                        // NEW BILL
                        FlageId = Guid.NewGuid()
                            .ToString("N")
                            .Substring(0, 4)
                            .ToUpper();
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
                //if (ISOutStandingAmt == "Yes")
                //{
                //    return Json(new
                //    {
                //        success = true,
                //        message = "Unpaid bill saved successfully."
                //    });
                //}
                //if (summary.TotalAmount != summary.TotalPaid)
                if (summary != null && Convert.ToDecimal(summary.TotalPaid) > 0)
                {
                    // decide status based on remaining amount
                    string status = summary.RemainingAmount == 0 ? "Paid" : "Unpaid";

                    DynamicParameters sumInsUpd = new DynamicParameters();
                    // Juna Code comment kela
                    // sumInsUpd.Add("@AppointmentFid", summary.AppointmentFid);
                    // Naveen Code add kela
                    sumInsUpd.Add("@AppointmentFid", appointmentId);
                    sumInsUpd.Add("@TotalAmount", summary.PayableAmount);


                    sumInsUpd.Add("@PatientFid", PatientID);
                    sumInsUpd.Add("@BillFlageID", BillFlageID);

                    sumInsUpd.Add("@TotalPaid", summary.TotalPaid);
                    sumInsUpd.Add("@RemainingAmount", summary.RemainingAmount);
                    sumInsUpd.Add("@Status", status);

                    BVGWellnessCenterDapparORM.ExcuteSp("sp_InsertOrUpdate_ServicesOutstanding", sumInsUpd);



                }
                // Update MAS_TherapyBooking BookingStatus to Confirmed and Send Confirmation Email
                if (!string.IsNullOrEmpty(BillFlageID))
                {
                    var cleanBillFlagId = BillFlageID.Trim();
                    DynamicParameters updateBooking = new DynamicParameters();
                    updateBooking.Add("@p_Query", $@"
                        UPDATE MAS_TherapyBooking 
                        SET BookingStatus = 'Confirmed' 
                        WHERE LTRIM(RTRIM(FlagId)) = '{cleanBillFlagId}'");
                    BVGWellnessCenterDapparORM.ExcuteSp("sp_ExecuteQuery", updateBooking);
                    // Send Email Confirmation to Patient on Billing Completion
                    try
                    {
                        DynamicParameters pInfo = new DynamicParameters();
                        pInfo.Add("@p_Query", $@"
                            SELECT TOP 1
                                TB.TherapyBookingId,
                                TB.PatientFid,
                                TB.ServiceFid,
                                TB.DoctorFid,
                                TB.FromDate,
                                TB.ToDate,
                                TB.NumberOfSessions,
                                (P.FirstName + ' ' + ISNULL(P.LName, '')) AS PatientName,
                                P.Email AS PatientEmail,
                                S.ServiceName,
                                (U.FirstName + ' ' + ISNULL(U.LName, '')) AS DocName
                            FROM MAS_TherapyBooking TB
                            LEFT JOIN Patient P ON P.PatientFid = TB.PatientFid
                            LEFT JOIN MAS_Service S ON S.ServiceFid = TB.ServiceFid
                            LEFT JOIN UserMaster U ON U.UserId = TB.DoctorFid
                            WHERE LTRIM(RTRIM(TB.FlagId)) = '{cleanBillFlagId}' AND TB.IsActive = 1
                            ORDER BY TB.TherapyBookingId DESC");
                        var info = BVGWellnessCenterDapparORM.ReturnList<TherapyBookingEmailInfo>("sp_ExecuteQuery", pInfo).FirstOrDefault();
                        if (info != null && !string.IsNullOrEmpty(info.PatientEmail))
                        {
                            string pEmail = info.PatientEmail.Trim();
                            if (!string.IsNullOrEmpty(pEmail))
                            {
                                string pName = info.PatientName ?? "Patient";
                                string sName = info.ServiceName ?? "Therapy Service";
                                string docName = info.DocName ?? "Doctor";
                                string clinicName = Session["ClinicName"]?.ToString() ?? "BVG Wellness Center";
                                string fromDateStr = info.FromDate.HasValue ? info.FromDate.Value.ToString("dd/MM/yyyy") : "-";
                                string toDateStr = info.ToDate.HasValue ? info.ToDate.Value.ToString("dd/MM/yyyy") : "-";
                                int sessionsCount = info.NumberOfSessions ?? 0;
                                // Fetch booked slots for the email
                                DynamicParameters slotParam = new DynamicParameters();
                                slotParam.Add("@p_Query", $@"
                                    SELECT SlotDate, SlotStart, SlotEnd 
                                    FROM MAS_TherapySlotBooking 
                                    WHERE TherapyBookingId = {info.TherapyBookingId} 
                                      AND IsBooked = 1
                                    ORDER BY SlotDate, SlotStart");
                                var slots = BVGWellnessCenterDapparORM.ReturnList<TherapySlotDisplayVM>("sp_ExecuteQuery", slotParam).ToList();
                                SendTherapyBookingEmail(pEmail, pName, sName, docName, fromDateStr, toDateStr, sessionsCount, clinicName, slots);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to send therapy booking email: {ex.Message}");
                    }
                }
                if (ISOutStandingAmt == "Yes")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Unpaid bill saved successfully."
                    });
                }
                return Json(new { success = true, message = "Payments processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }





        public JsonResult GetServiceByCategory(int categoryId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ServiceGroupID", categoryId);

            var data = BVGWellnessCenterDapparORM.ReturnList<dynamic>(
                "sp_GetServiceByCategory", param).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public class TherapyBookingEmailInfo
        {
            public int TherapyBookingId { get; set; }
            public int PatientFid { get; set; }
            public int ServiceFid { get; set; }
            public int DoctorFid { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int? NumberOfSessions { get; set; }
            public string PatientName { get; set; }
            public string PatientEmail { get; set; }
            public string ServiceName { get; set; }
            public string DocName { get; set; }
        }

        /*
        private bool IsTherapyBookingPaid(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
                return false;

            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"
                    SELECT CASE WHEN ISNULL(SUM(Amount), 0) >= ISNULL(MAX(PayableAmount), 0) AND ISNULL(MAX(PayableAmount), 0) > 0 THEN 1 ELSE 0 END AS IsPaid
                    FROM FinalservicesBill 
                    WHERE BillFlageID = '{flagId}' 
                      AND Deactivate = 0
                ");
                
                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param).FirstOrDefault();
                if (result != null)
                {
                    var dict = (IDictionary<string, object>)result;
                    if (dict.ContainsKey("IsPaid"))
                    {
                        return Convert.ToInt32(dict["IsPaid"]) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in IsTherapyBookingPaid: " + ex.Message);
            }
            return false;
        }

        private void SendTherapyBookingEmail(
            string email, 
            string patientName, 
            string serviceName, 
            string docName, 
            string fromDate, 
            string toDate, 
            int sessions, 
            string clinicName, 
            List<TherapySlotDisplayVM> slots)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return;

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("apiprobus@gmail.com", "BVG Wellness Center");
                mail.To.Add(email.Trim());
                mail.Subject = "Therapy Booking Confirmation";

                string slotsHtml = "";
                if (slots != null && slots.Any())
                {
                    slotsHtml = "<h3>Booked Slots:</h3><ul>";
                    foreach (var slot in slots)
                    {
                        string sDate = slot.SlotDate.ToString("dd/MM/yyyy");
                        string sTime = $"{slot.SlotStart:h:mm tt} - {slot.SlotEnd:h:mm tt}";
                        slotsHtml += $"<li><b>Date:</b> {sDate} | <b>Time:</b> {sTime}</li>";
                    }
                    slotsHtml += "</ul>";
                }

                mail.Body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;'>
                <h2 style='color: #4CAF50;'>Therapy Booking Confirmed!</h2>
                <p>Hello <b>{patientName}</b>,</p>
                <p>Your therapy booking has been successfully confirmed at <b>{clinicName}</b>.</p>
                <hr/>
                <p><b>Therapy Service:</b> {serviceName}</p>
                <p><b>Doctor:</b> {docName}</p>
                <p><b>Duration:</b> {fromDate} to {toDate}</p>
                <p><b>Total Sessions:</b> {sessions}</p>
                {slotsHtml}
                <br/>
                <p>Regards,<br/>Team BVG Wellness</p>
            </div>";
                
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("apiprobus@gmail.com", "nibd kmnr fbdb pguf");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send therapy booking email: {ex.Message}");
            }
        }
        */

        [HttpGet]
        public JsonResult GetRoomSlots(int roomId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", $@"
                    SELECT 
                        CONVERT(VARCHAR(5), StartTime, 108) AS StartTime, 
                        CONVERT(VARCHAR(5), EndTime, 108) AS EndTime, 
                        IsBreak, 
                        BreakLabel 
                    FROM MAS_TherapySlotConfig
                    WHERE ServiceRoomID = {roomId}
                    ORDER BY StartTime ASC
                ");

                var slots = BVGWellnessCenterDapparORM.ReturnList<TherapySlotConfigVM>("sp_ExecuteQuery", param).ToList();
                return Json(new { success = true, slots = slots }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MarkTherapyAsComplete(int id)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TherapyBookingId", id);
                param.Add("@p_Message", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 200);

                BVGWellnessCenterDapparORM.ExcuteSp("sp_Update_TherapyCompletionStatus", param);

                string message = param.Get<string>("@p_Message");
                if (message == "Therapy marked as completed successfully.")
                {
                    return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}
