using BVG_Wellness_Center_Promotion.Areas.IPD.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Controllers
{
    public class IPD_BookingController : Controller
    {
        private bool IsSessionValid() => Session["UserID"] != null;
        private int GetUserId() => Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

        private IPD_HospitalSettingModel GetSettings()
        {
            var setting = Session["IPD_Settings"] as IPD_HospitalSettingModel;
            if (setting == null)
            {
                setting = BVGWellnessCenterDapparORM.ReturnList<IPD_HospitalSettingModel>("sp_IPD_HospitalSetting_Get", null).FirstOrDefault()
                          ?? new IPD_HospitalSettingModel();
                Session["IPD_Settings"] = setting;
            }
            return setting;
        }

        // =============================================
        // BED DASHBOARD
        // =============================================
        public ActionResult BedDashboard()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            var beds = BVGWellnessCenterDapparORM.ReturnList<IPD_BedDashboardModel>("sp_IPD_BedDashboard", param).ToList();
            ViewBag.Settings = GetSettings();

            // Summary counts
            ViewBag.TotalBeds = beds.Count;
            ViewBag.AvailableBeds = beds.Count(b => b.BedStatus == "Available");
            ViewBag.OccupiedBeds = beds.Count(b => b.BedStatus == "Occupied");
            ViewBag.OtherBeds = beds.Count(b => b.BedStatus != "Available" && b.BedStatus != "Occupied");

            return View(beds);
        }

        // =============================================
        // ACTIVE BOOKINGS LIST
        // =============================================
        public ActionResult ActiveBookings()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@Status", "Active");
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_List", param).ToList();
            return View(list);
        }

        public ActionResult DischargedBookings()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@Status", "Discharged");
            var list = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_List", param).ToList();
            
            return View(list);
        }

        public ActionResult PrintBill(int bookingId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@BookingId", bookingId);
            var booking = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_GetById", param).FirstOrDefault();
            
            if (booking == null) return HttpNotFound();

            var settings = GetSettings();
            var bParam = new DynamicParameters();
            bParam.Add("@BookingId", bookingId);
            bParam.Add("@DischargeDate", booking.ActualDischarge ?? booking.ExpectedDischarge);
            var lines = BVGWellnessCenterDapparORM.ReturnList<IPD_BillLineModel>("sp_IPD_GenerateBill", bParam).ToList();

            var aParam = new DynamicParameters();
            aParam.Add("@BookingId", bookingId);
            decimal advancePaid = BVGWellnessCenterDapparORM.ReturnList<decimal>("sp_IPD_GetAdvanceTotal", aParam).FirstOrDefault();

            var summary = new IPD_BillSummaryModel
            {
                BookingId = bookingId,
                Lines = lines,
                GSTEnabled = settings.GSTEnabled,
                GSTRate = settings.GSTRate
            };

            // Check for frozen billing record
            var billParam = new DynamicParameters();
            billParam.Add("@p_Query", "SELECT * FROM tbl_IPD_Billing WHERE BookingId = " + bookingId);
            var frozenBill = BVGWellnessCenterDapparORM.ReturnList<IPD_BillingModel>("sp_ExecuteQuery", billParam).FirstOrDefault();

            if (frozenBill != null)
            {
                summary.RoomRentTotal = frozenBill.RoomTotal;
                summary.CompulsoryTotal = frozenBill.CompulsoryTotal;
                summary.OptionalTotal = frozenBill.OptionalTotal;
                summary.FoodTotal = frozenBill.FoodTotal;
                summary.Subtotal = frozenBill.Subtotal;
                summary.GSTAmount = frozenBill.GSTAmount;
                summary.GrossTotal = frozenBill.GrossTotal;
                summary.AdvancePaid = frozenBill.AdvancePaid;
                summary.NetPayable = frozenBill.NetPayable;
                summary.DiscountType = frozenBill.DiscountType;
                summary.DiscountValue = frozenBill.DiscountValue;
                summary.DiscountAmount = frozenBill.DiscountAmount;
                summary.DiscountReason = frozenBill.DiscountReason;
                booking.BookingNo = frozenBill.BillNo; // Override booking no with Invoice No
            }
            else
            {
                summary.RoomRentTotal = lines.Where(l => l.BillSection == "RoomRent").Sum(l => l.Amount);
                summary.CompulsoryTotal = lines.Where(l => l.BillSection == "CompulsoryCharge").Sum(l => l.Amount);
                summary.OptionalTotal = lines.Where(l => l.BillSection == "OptionalCharge").Sum(l => l.Amount);
                summary.FoodTotal = lines.Where(l => l.BillSection == "FoodCharge").Sum(l => l.Amount);
                summary.AdvancePaid = advancePaid;
                summary.Subtotal = summary.RoomRentTotal + summary.CompulsoryTotal + summary.OptionalTotal + summary.FoodTotal;
                summary.GSTAmount = settings.GSTEnabled ? Math.Round(summary.Subtotal * settings.GSTRate / 100, 2) : 0;
                summary.GrossTotal = summary.Subtotal + summary.GSTAmount;
                summary.NetPayable = summary.GrossTotal - summary.AdvancePaid;
            }

            ViewBag.Summary = summary;
            ViewBag.Settings = settings;

            return View(booking);
        }

        // =============================================
        // NEW ADMISSION FORM (GET)
        // =============================================
        public ActionResult NewAdmission(int? bedId = null, int? roomId = null)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var model = new IPD_StayBookingModel
            {
                AdmissionDate = DateTime.Now,
                ExpectedDischarge = DateTime.Now.AddDays(1),
                BedId = bedId ?? 0,
                RoomId = roomId ?? 0
            };
            LoadAdmissionDropdowns();
            return View(model);
        }

        // =============================================
        // SAVE ADMISSION (POST)
        // =============================================
        [HttpPost]
        public JsonResult SaveAdmission(IPD_StayBookingModel model, string selectedChargeIds)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                // 1. Save main booking
                var param = new DynamicParameters();
                param.Add("@PatientFid", model.PatientFid);
                param.Add("@DoctorId", model.DoctorId);
                param.Add("@ReferredBy", model.ReferredBy);
                param.Add("@AdmissionDate", model.AdmissionDate);
                param.Add("@ExpectedDischarge", model.ExpectedDischarge);
                param.Add("@AdmissionType", model.AdmissionType);
                param.Add("@FloorId", model.FloorId);
                param.Add("@WardId", model.WardId);
                param.Add("@CategoryId", model.CategoryId);
                param.Add("@RoomId", model.RoomId);
                param.Add("@BedId", model.BedId);
                param.Add("@RoomChargeSnapshot", model.RoomChargeSnapshot);
                param.Add("@FoodPlanId", model.FoodPlanId);
                param.Add("@FoodDailyCharge", model.FoodDailyCharge);
                param.Add("@AdvanceAmount", model.AdvanceAmount);
                param.Add("@AdvancePayMode", model.AdvancePayMode);
                param.Add("@Remarks", model.Remarks);
                param.Add("@CreatedBy", GetUserId());
                param.Add("@NewBookingId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_StayBooking_Admit", param);

                int newBookingId = param.Get<int>("@NewBookingId");
                bool ok = param.Get<string>("@p_Icon") == "success";

                if (ok && newBookingId > 0)
                {
                    // 2. Save selected charges
                    var cparam2 = new DynamicParameters();
                    cparam2.Add("@RoomId", model.RoomId);
                    var allRoomCharges = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMapping_GetByRoom", cparam2).ToList();

                    // Get the LocationId just created
                    var lParam = new DynamicParameters();
                    lParam.Add("@BookingId", newBookingId);
                    var bookingDetail = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_GetById", lParam).FirstOrDefault();
                    int locationId = bookingDetail != null ? 0 : 0; // Will get from SP below

                    // Get location id via query
                    var qParam = new DynamicParameters();
                    qParam.Add("@p_Query", $"SELECT TOP 1 LocationId FROM tbl_IPD_StayBookingLocation WHERE BookingId={newBookingId} ORDER BY LocationId DESC");
                    locationId = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", qParam).FirstOrDefault();

                    // Parse selected optional charge IDs
                    List<int> selectedIds = new List<int>();
                    int tempVal;
                    if (!string.IsNullOrEmpty(selectedChargeIds))
                        selectedIds = selectedChargeIds.Split(',').Where(x => int.TryParse(x, out tempVal)).Select(int.Parse).ToList();

                    foreach (var charge in allRoomCharges)
                    {
                        bool isSelected = charge.IsCompulsory || selectedIds.Contains(charge.ChargeId);
                        var cParam = new DynamicParameters();
                        cParam.Add("@BookingId", newBookingId);
                        cParam.Add("@LocationId", locationId);
                        cParam.Add("@ChargeId", charge.ChargeId);
                        cParam.Add("@ChargeName", charge.ChargeName);
                        cParam.Add("@ChargeType", charge.ChargeType);
                        cParam.Add("@IsCompulsory", charge.IsCompulsory);
                        cParam.Add("@AmountPerDay", charge.ChargeAmount);
                        cParam.Add("@IsSelected", isSelected);
                        cParam.Add("@EffectiveFrom", model.AdmissionDate);
                        BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_StayBookingCharges_Save", cParam);
                    }
                }

                return Json(new { success = ok, message = param.Get<string>("@p_Message"), bookingId = newBookingId });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // =============================================
        // BED TRANSFER (GET)
        // =============================================
        public ActionResult BedTransfer(int bookingId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@BookingId", bookingId);
            var booking = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_GetById", param).FirstOrDefault();

            // Transfer history
            var hParam = new DynamicParameters();
            hParam.Add("@BookingId", bookingId);
            ViewBag.TransferHistory = BVGWellnessCenterDapparORM.ReturnList<IPD_TransferHistoryModel>("sp_IPD_TransferHistory_GetByBooking", hParam).ToList();

            LoadAdmissionDropdowns();
            return View(booking);
        }

        // =============================================
        // BED TRANSFER (POST)
        // =============================================
        [HttpPost]
        public JsonResult SaveBedTransfer(int bookingId, int? newFloorId, int? newWardId, int? newCategoryId,
            int newRoomId, int newBedId, decimal newRoomCharge,
            DateTime transferDate, string reason, string remarks, string selectedChargeIds)
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var param = new DynamicParameters();
                param.Add("@BookingId", bookingId);
                param.Add("@NewFloorId", newFloorId);
                param.Add("@NewWardId", newWardId);
                param.Add("@NewCategoryId", newCategoryId);
                param.Add("@NewRoomId", newRoomId);
                param.Add("@NewBedId", newBedId);
                param.Add("@NewRoomCharge", newRoomCharge);
                param.Add("@TransferDate", transferDate);
                param.Add("@Reason", reason);
                param.Add("@Remarks", remarks);
                param.Add("@TransferredBy", GetUserId());
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_BedTransfer_Save", param);

                bool ok = param.Get<string>("@p_Icon") == "success";

                // Save new room charges if transfer was successful
                if (ok)
                {
                    var qParam = new DynamicParameters();
                    qParam.Add("@p_Query", $"SELECT TOP 1 LocationId FROM tbl_IPD_StayBookingLocation WHERE BookingId={bookingId} ORDER BY LocationId DESC");
                    int newLocationId = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", qParam).FirstOrDefault();

                    var cparam2 = new DynamicParameters();
                    cparam2.Add("@RoomId", newRoomId);
                    var newCharges = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomChargesModel>("sp_IPD_RoomChargesMapping_GetByRoom", cparam2).ToList();

                    List<int> selectedIds = new List<int>();
                    int tempVal2;
                    if (!string.IsNullOrEmpty(selectedChargeIds))
                        selectedIds = selectedChargeIds.Split(',').Where(x => int.TryParse(x, out tempVal2)).Select(int.Parse).ToList();

                    foreach (var charge in newCharges)
                    {
                        bool isSelected = charge.IsCompulsory || selectedIds.Contains(charge.ChargeId);
                        var cParam = new DynamicParameters();
                        cParam.Add("@BookingId", bookingId);
                        cParam.Add("@LocationId", newLocationId);
                        cParam.Add("@ChargeId", charge.ChargeId);
                        cParam.Add("@ChargeName", charge.ChargeName);
                        cParam.Add("@ChargeType", charge.ChargeType);
                        cParam.Add("@IsCompulsory", charge.IsCompulsory);
                        cParam.Add("@AmountPerDay", charge.ChargeAmount);
                        cParam.Add("@IsSelected", isSelected);
                        cParam.Add("@EffectiveFrom", transferDate);
                        BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_StayBookingCharges_Save", cParam);
                    }
                }

                return Json(new { success = ok, message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // =============================================
        // DISCHARGE SCREEN (GET)
        // =============================================
        public ActionResult Discharge(int bookingId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Home", new { area = "" });
            var param = new DynamicParameters();
            param.Add("@BookingId", bookingId);
            var booking = BVGWellnessCenterDapparORM.ReturnList<IPD_StayBookingModel>("sp_IPD_StayBooking_GetById", param).FirstOrDefault();
            ViewBag.Settings = GetSettings();
            return View(booking);
        }

        // =============================================
        // GENERATE BILL (AJAX)
        // =============================================
        [HttpPost]
        public JsonResult GenerateBill(int bookingId, DateTime? dischargeDate, string discountType = "None", decimal discountValue = 0)
        {
            if (!IsSessionValid()) return Json(new { success = false });
            try
            {
                var settings = GetSettings();
                var param = new DynamicParameters();
                param.Add("@BookingId", bookingId);
                param.Add("@DischargeDate", dischargeDate);
                var lines = BVGWellnessCenterDapparORM.ReturnList<IPD_BillLineModel>("sp_IPD_GenerateBill", param).ToList();

                // Advance total
                var aParam = new DynamicParameters();
                aParam.Add("@BookingId", bookingId);
                decimal advancePaid = BVGWellnessCenterDapparORM.ReturnList<decimal>("sp_IPD_GetAdvanceTotal", aParam).FirstOrDefault();

                var summary = new IPD_BillSummaryModel
                {
                    BookingId = bookingId,
                    Lines = lines,
                    RoomRentTotal = lines.Where(l => l.BillSection == "RoomRent").Sum(l => l.Amount),
                    CompulsoryTotal = lines.Where(l => l.BillSection == "CompulsoryCharge").Sum(l => l.Amount),
                    OptionalTotal = lines.Where(l => l.BillSection == "OptionalCharge").Sum(l => l.Amount),
                    FoodTotal = lines.Where(l => l.BillSection == "FoodCharge").Sum(l => l.Amount),
                    GSTEnabled = settings.GSTEnabled,
                    GSTRate = settings.GSTRate,
                    AdvancePaid = advancePaid
                };
                summary.Subtotal = summary.RoomRentTotal + summary.CompulsoryTotal + summary.OptionalTotal + summary.FoodTotal;
                
                // Calculate discount
                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(discountType) && discountType != "None" && discountValue > 0)
                {
                    if (discountType == "Flat") discountAmount = discountValue;
                    else if (discountType == "Percentage") discountAmount = Math.Round(summary.Subtotal * discountValue / 100, 2);
                    
                    if (discountAmount > summary.Subtotal) discountAmount = summary.Subtotal;
                }
                summary.DiscountType = discountType;
                summary.DiscountValue = discountValue;
                summary.DiscountAmount = discountAmount;
                
                decimal discountedSubtotal = summary.Subtotal - discountAmount;
                summary.GSTAmount = settings.GSTEnabled ? Math.Round(discountedSubtotal * settings.GSTRate / 100, 2) : 0;
                summary.GrossTotal = discountedSubtotal + summary.GSTAmount;
                summary.NetPayable = summary.GrossTotal - advancePaid;

                return Json(new { success = true, bill = summary });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // =============================================
        // CONFIRM DISCHARGE (POST)
        // =============================================
        [HttpPost]
        public JsonResult ConfirmDischarge(int bookingId, DateTime dischargeDate, string discountType = "None", decimal discountValue = 0, string discountReason = "")
        {
            if (!IsSessionValid()) return Json(new { success = false, message = "Session expired" });
            try
            {
                var settings = GetSettings();
                
                // 1. Pre-calculate final bill summary
                var bParam = new DynamicParameters();
                bParam.Add("@BookingId", bookingId);
                bParam.Add("@DischargeDate", dischargeDate);
                var lines = BVGWellnessCenterDapparORM.ReturnList<IPD_BillLineModel>("sp_IPD_GenerateBill", bParam).ToList();

                var aParam = new DynamicParameters();
                aParam.Add("@BookingId", bookingId);
                decimal advancePaid = BVGWellnessCenterDapparORM.ReturnList<decimal>("sp_IPD_GetAdvanceTotal", aParam).FirstOrDefault();

                decimal roomTotal = lines.Where(l => l.BillSection == "RoomRent").Sum(l => l.Amount);
                decimal compulsoryTotal = lines.Where(l => l.BillSection == "CompulsoryCharge").Sum(l => l.Amount);
                decimal optionalTotal = lines.Where(l => l.BillSection == "OptionalCharge").Sum(l => l.Amount);
                decimal foodTotal = lines.Where(l => l.BillSection == "FoodCharge").Sum(l => l.Amount);
                decimal subtotal = roomTotal + compulsoryTotal + optionalTotal + foodTotal;
                
                // Calculate discount
                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(discountType) && discountType != "None" && discountValue > 0)
                {
                    if (discountType == "Flat") discountAmount = discountValue;
                    else if (discountType == "Percentage") discountAmount = Math.Round(subtotal * discountValue / 100, 2);
                    
                    if (discountAmount > subtotal) discountAmount = subtotal; // cap discount
                }
                
                decimal discountedSubtotal = subtotal - discountAmount;
                decimal gstAmount = settings.GSTEnabled ? Math.Round(discountedSubtotal * settings.GSTRate / 100, 2) : 0;
                decimal grossTotal = discountedSubtotal + gstAmount;
                decimal netPayable = grossTotal - advancePaid;

                // 2. Call Discharge SP with billing params
                var param = new DynamicParameters();
                param.Add("@BookingId", bookingId);
                param.Add("@DischargeDate", dischargeDate);
                param.Add("@AutoReleaseBed", settings.AutoReleaseBedOnDischarge);
                param.Add("@GeneratedBy", GetUserId());
                param.Add("@RoomTotal", roomTotal);
                param.Add("@CompulsoryTotal", compulsoryTotal);
                param.Add("@OptionalTotal", optionalTotal);
                param.Add("@FoodTotal", foodTotal);
                param.Add("@Subtotal", subtotal);
                param.Add("@GSTAmount", gstAmount);
                param.Add("@GrossTotal", grossTotal);
                param.Add("@AdvancePaid", advancePaid);
                param.Add("@NetPayable", netPayable);
                param.Add("@DiscountType", discountType);
                param.Add("@DiscountValue", discountValue);
                param.Add("@DiscountAmount", discountAmount);
                param.Add("@DiscountReason", discountReason);
                param.Add("@p_Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                BVGWellnessCenterDapparORM.ExcuteSp("sp_IPD_Discharge_Confirm", param);
                return Json(new { success = param.Get<string>("@p_Icon") == "success", message = param.Get<string>("@p_Message") });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // AJAX — Patient search (reuse existing SP)
        public JsonResult SearchPatient(string term)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_Query", $"SELECT TOP 10 PatientFid AS Id, FirstName + ' ' + ISNULL(LName,'') + ' (' + ContactNumber + ')' AS Name, PatientFid FROM Patient WHERE (FirstName LIKE '%{term}%' OR ContactNumber LIKE '%{term}%') AND Deactivate=0");
                var list = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch { return Json(new List<object>(), JsonRequestBehavior.AllowGet); }
        }

        private void LoadAdmissionDropdowns()
        {
            var settings = GetSettings();
            ViewBag.Settings = settings;
            if (settings.UseFloor)
                ViewBag.Floors = BVGWellnessCenterDapparORM.ReturnList<IPD_FloorModel>("sp_IPD_FloorMaster_List", null).ToList();
            if (settings.UseWard)
                ViewBag.Wards = BVGWellnessCenterDapparORM.ReturnList<IPD_WardModel>("sp_IPD_WardMaster_List", null).ToList();
            if (settings.UseCategory)
                ViewBag.Categories = BVGWellnessCenterDapparORM.ReturnList<IPD_CategoryModel>("sp_IPD_CategoryMaster_List", null).ToList();
            if (settings.FoodModuleEnabled)
                ViewBag.FoodPlans = BVGWellnessCenterDapparORM.ReturnList<IPD_FoodPlanModel>("sp_IPD_FoodPlanMaster_List", null).ToList();
            ViewBag.Rooms = BVGWellnessCenterDapparORM.ReturnList<IPD_RoomModel>("sp_IPD_RoomMaster_List", null).ToList();

            var doctors = new List<SelectListItem>();
            using (var conn = new System.Data.SqlClient.SqlConnection(Session["NewConnetionString"]?.ToString()))
            {
                conn.Open();
                var cmd = new System.Data.SqlClient.SqlCommand("SELECT UserId as Value, FirstName + ' ' + LName as Text FROM UserMaster WHERE IsDoctor=1 AND Deactivate=0", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doctors.Add(new SelectListItem { Value = reader["Value"].ToString(), Text = reader["Text"].ToString() });
                    }
                }
            }
            ViewBag.Doctors = doctors;
        }
    }
}
