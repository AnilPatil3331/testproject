using BVG_Wellness_Center_Promotion.Areas.IPD.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Controllers
{
    public class IPDController : Controller
    {
        // GET: IPD/IPD (Dashboard)
        public ActionResult Index()
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                // 1. Total Rooms (Active)
                var paramTotal = new DynamicParameters();
                paramTotal.Add("@p_Query", "SELECT COUNT(*) FROM RoomTypes WHERE IsActive = 1");
                int totalRooms = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", paramTotal).FirstOrDefault();
                ViewBag.TotalRooms = totalRooms;

                // 2. Occupied Rooms (Today)
                var paramOccupied = new DynamicParameters();
                paramOccupied.Add("@p_Query", @"
                    SELECT COUNT(DISTINCT RoomTypeId) 
                    FROM StayBooking 
                    WHERE CAST(GETDATE() AS DATE) >= CheckInDate 
                      AND CAST(GETDATE() AS DATE) < CheckOutDate 
                      AND BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn')");
                int occupiedRooms = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", paramOccupied).FirstOrDefault();
                ViewBag.OccupiedRooms = occupiedRooms;

                // 3. Available Rooms
                ViewBag.AvailableRooms = Math.Max(0, totalRooms - occupiedRooms);

                // 4. Today's Check-In Count
                var paramCheckIn = new DynamicParameters();
                paramCheckIn.Add("@p_Query", @"
                    SELECT COUNT(*) 
                    FROM StayBooking 
                    WHERE CheckInDate = CAST(GETDATE() AS DATE) 
                      AND BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn')");
                int todayCheckIn = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", paramCheckIn).FirstOrDefault();
                ViewBag.TodayCheckIn = todayCheckIn;

                // 5. Today's Check-Out Count
                var paramCheckOut = new DynamicParameters();
                paramCheckOut.Add("@p_Query", @"
                    SELECT COUNT(*) 
                    FROM StayBooking 
                    WHERE CheckOutDate = CAST(GETDATE() AS DATE) 
                      AND BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn', 'CheckedOut')");
                int todayCheckOut = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", paramCheckOut).FirstOrDefault();
                ViewBag.TodayCheckOut = todayCheckOut;

                // 6. Todays Check-In and Check-Out list
                var paramList = new DynamicParameters();
                paramList.Add("@p_Query", @"
                    SELECT sb.*, sbd.Gender, sbd.Age, rt.RoomName 
                    FROM StayBooking sb 
                    LEFT JOIN StayBookingDetails sbd ON sb.BookingId = sbd.BookingId 
                    LEFT JOIN RoomTypes rt ON sb.RoomTypeId = rt.RoomTypeId 
                    WHERE (sb.CheckInDate = CAST(GETDATE() AS DATE) OR sb.CheckOutDate = CAST(GETDATE() AS DATE))
                      AND sb.BookingStatus != 'Cancelled'
                    ORDER BY sb.BookingId DESC");
                var todayActivities = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramList).ToList();
                ViewBag.TodayActivities = todayActivities;

                // 7. Room Status Grid
                var paramGrid = new DynamicParameters();
                paramGrid.Add("@p_Query", @"
                    SELECT rt.RoomTypeId, rt.RoomName, rt.MaxOccupancy, rt.BasePrice,
                           (CASE WHEN EXISTS (
                               SELECT 1 FROM StayBooking sb 
                               WHERE sb.RoomTypeId = rt.RoomTypeId 
                                 AND CAST(GETDATE() AS DATE) >= sb.CheckInDate 
                                 AND CAST(GETDATE() AS DATE) < sb.CheckOutDate 
                                 AND sb.BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn')
                           ) THEN 'Occupied' ELSE 'Available' END) as Status,
                           (SELECT TOP 1 sb.GuestFullName FROM StayBooking sb 
                            WHERE sb.RoomTypeId = rt.RoomTypeId 
                              AND CAST(GETDATE() AS DATE) >= sb.CheckInDate 
                              AND CAST(GETDATE() AS DATE) < sb.CheckOutDate 
                              AND sb.BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn')) as GuestFullName
                    FROM RoomTypes rt 
                    WHERE rt.IsActive = 1");
                var roomGrid = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramGrid).ToList();
                ViewBag.RoomGrid = roomGrid;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading dashboard: " + ex.Message;
                TempData["Icon"] = "error";
                return View();
            }
        }

        // GET: IPD/IPD/RoomBookingList
        public ActionResult RoomBookingList()
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                var param = new DynamicParameters();
                param.Add("@p_Query", @"
                    SELECT sb.*, sbd.Gender, sbd.Age, sbd.Address, sbd.RoomNo, sbd.ChildAgeDetails, 
                           sbd.PetCount, sbd.SpecialNotes, sbd.RefDoctor, sbd.EmergencyContact,
                           rt.RoomName, rt.BasePrice,
                           pt.PaymentStatus AS BillStatus,
                           pt.Id AS PaymentId
                    FROM StayBooking sb
                    LEFT JOIN StayBookingDetails sbd ON sb.BookingId = sbd.BookingId
                    LEFT JOIN RoomTypes rt ON sb.RoomTypeId = rt.RoomTypeId
                    OUTER APPLY (
                        SELECT TOP 1 PaymentStatus, Id FROM SWG001.dbo.PaymentTransactions 
                        WHERE BookingId = sb.BookingId AND PaymentFor IN ('StayBooking', 'RoomBooking')
                        ORDER BY Id DESC
                    ) pt
                    ORDER BY sb.BookingId DESC");
                var list = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", param).ToList();
                ViewBag.BookingList = list;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading list: " + ex.Message;
                TempData["Icon"] = "error";
                return View();
            }
        }

        // GET: IPD/IPD/RoomBooking (Add / Edit Form)
        public ActionResult RoomBooking(int? id)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                // Load Room Types details
                var paramTypes = new DynamicParameters();
                paramTypes.Add("@p_Query", "SELECT RoomTypeId, RoomName, MaxOccupancy, BasePrice FROM RoomTypes WHERE IsActive = 1");
                var roomDetails = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramTypes).ToList();
                ViewBag.RoomTypes = roomDetails.Select(r => new DropdownItem { Id = r.RoomTypeId, Name = r.RoomName + " (₹" + r.BasePrice + "/day)" }).ToList();
                ViewBag.RoomDetailsJson = Newtonsoft.Json.JsonConvert.SerializeObject(roomDetails);

                // Load Pet Surcharge Percentage from settings
                var paramSettings = new DynamicParameters();
                paramSettings.Add("@p_Query", "SELECT SettingValue FROM MAS_IPDSettings WHERE SettingKey = 'PetChargePercentage'");
                var petPctStr = BVGWellnessCenterDapparORM.ReturnList<string>("sp_ExecuteQuery", paramSettings).FirstOrDefault();
                ViewBag.PetChargePercentage = string.IsNullOrEmpty(petPctStr) ? "25.00" : petPctStr;

                StayBookingModel model = new StayBookingModel
                {
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(1),
                    NoOfPersons = 1,
                    NoOfChildren = 0,
                    StayType = "Single",
                    Status = "Booked",
                    BookingStatus = "Confirmed"
                };

                if (id.HasValue && id > 0)
                {
                    var paramEdit = new DynamicParameters();
                    paramEdit.Add("@p_Query", $@"
                        SELECT sb.*, sbd.Gender, sbd.Age, sbd.Address, sbd.RoomNo, sbd.ChildAgeDetails, 
                               sbd.PetCount, sbd.SpecialNotes, sbd.RefDoctor, sbd.EmergencyContact,
                               rt.RoomName, rt.BasePrice
                        FROM StayBooking sb
                        LEFT JOIN StayBookingDetails sbd ON sb.BookingId = sbd.BookingId
                        LEFT JOIN RoomTypes rt ON sb.RoomTypeId = rt.RoomTypeId
                        WHERE sb.BookingId = {id}");
                    var editModel = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramEdit).FirstOrDefault();
                    if (editModel != null)
                    {
                        model = editModel;

                        // Fetch family members for edit mode
                        var paramFam = new DynamicParameters();
                        paramFam.Add("@p_Query", $"SELECT * FROM StayBookingFamilyMember WHERE BookingId = {id}");
                        var famList = BVGWellnessCenterDapparORM.ReturnList<StayBookingFamilyMemberModel>("sp_ExecuteQuery", paramFam).ToList();
                        if (famList != null && famList.Count > 0)
                        {
                            model.FamilyMembers = famList;
                            model.FamilyMember = true;
                            model.Own = false;
                        }
                        else
                        {
                            model.Own = !model.TwinSharing && !model.FamilyMember;
                        }

                        ViewBag.IsEdit = true;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading form: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("RoomBookingList");
            }
        }

        [HttpPost]
        public JsonResult CheckAvailability(int roomTypeId, string checkInDate, string checkOutDate, bool isTwinSharing, string gender, bool sameGender, int excludeBookingId = 0)
        {
            try
            {
                DateTime checkIn;
                DateTime checkOut;
                if (!DateTime.TryParse(checkInDate, out checkIn) || !DateTime.TryParse(checkOutDate, out checkOut))
                {
                    return Json(new { success = false, message = "Invalid check-in or check-out date format." });
                }

                if (checkIn.Date >= checkOut.Date)
                {
                    return Json(new { success = false, message = "Check-In Date must be less than Check-Out Date." });
                }

                // Query Room details to verify capacity
                var paramRoom = new DynamicParameters();
                paramRoom.Add("@p_Query", $"SELECT RoomTypeId, RoomName, MaxOccupancy, BasePrice FROM RoomTypes WHERE RoomTypeId = {roomTypeId} AND IsActive = 1");
                var roomInfo = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramRoom).FirstOrDefault();
                if (roomInfo == null)
                {
                    return Json(new { success = false, message = "Selected room type is invalid or inactive." });
                }

                // Get overlapping bookings
                var overlapParam = new DynamicParameters();
                overlapParam.Add("@p_Query", $@"
                    SELECT sb.*, p.Gender
                    FROM StayBooking sb
                    LEFT JOIN Patient p ON sb.PatientId = p.PatientFid
                    WHERE sb.RoomTypeId = {roomTypeId}
                      AND sb.BookingStatus = 'Confirmed'
                      AND sb.CheckInDate < '{checkOut:yyyy-MM-dd}'
                      AND sb.CheckOutDate > '{checkIn:yyyy-MM-dd}'
                      {(excludeBookingId > 0 ? $"AND sb.BookingId != {excludeBookingId}" : "")}");

                var overlaps = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", overlapParam).ToList();

                bool isAvailable = true;
                string message = "Room is available.";
                List<string> bookedIntervals = new List<string>();
                string existingChildType = "None";

                // 1. Overlap Check:
                if (overlaps.Count > 0)
                {
                    // Case A: If any existing booking has TwinSharing = False, the entire room is blocked!
                    bool entireRoomBlocked = overlaps.Any(o => !o.TwinSharing);

                    // Case B: If new booking does not want sharing (TwinSharing = False), it needs a completely empty room!
                    bool wantsPrivateRoom = !isTwinSharing;

                    // Fetch MaxOccupancy for the room to handle dormitory style (like Outdoor Living Room with 20)
                    var rtParam = new DynamicParameters();
                    rtParam.Add("@p_Query", $"SELECT MaxOccupancy FROM RoomTypes WHERE RoomTypeId = {roomTypeId}");
                    int maxOcc = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", rtParam).FirstOrDefault();
                    int twinSharingLimit = (maxOcc > 3) ? maxOcc : 2;

                    // Case C: Shared room occupied limit (Max 2 bookings sharing, or more for dorms)
                    bool sharedRoomFull = overlaps.Count >= twinSharingLimit;

                    // Calculate existingChildType if TwinSharing (only for strict 2-person sharing, not dorms)
                    if (isTwinSharing && overlaps.Count == 1 && twinSharingLimit <= 2)
                    {
                        var existing = overlaps.First();
                        if (existing.NoOfChildren == 2)
                        {
                            existingChildType = "Two_0_5";
                        }
                        else if (existing.NoOfChildren == 1)
                        {
                            int age = 0;
                            if (!string.IsNullOrEmpty(existing.ChildAgeDetails))
                            {
                                int.TryParse(existing.ChildAgeDetails.Split(',')[0], out age);
                            }
                            if (age >= 6 && age <= 12) {
                                existingChildType = "One_6_12";
                            } else {
                                existingChildType = "One_0_5";
                            }
                        }
                    }

                    if (entireRoomBlocked || wantsPrivateRoom || sharedRoomFull)
                    {
                        isAvailable = false;
                    }
                    else if (sameGender)
                    {
                        // sharing: Check gender rules only if user opted for Same Gender Sharing
                        foreach (var existing in overlaps)
                        {
                            if (!string.Equals(gender, existing.Gender, StringComparison.OrdinalIgnoreCase))
                            {
                                isAvailable = false;
                                message = $"Room already has an occupant of gender '{existing.Gender}'. You opted for Same Gender Sharing.";
                                break;
                            }
                        }
                    }

                    if (!isAvailable && string.Equals(message, "Room is available."))
                    {
                        message = "Selected room is not available for the chosen dates.";
                    }

                    // Compute booked intervals for suggestion message
                    foreach (var o in overlaps)
                    {
                        bookedIntervals.Add($"{o.CheckInDate:dd-MMM} to {o.CheckOutDate:dd-MMM}");
                    }
                }

                // 2. Twin Sharing Disabled Gender Policy (If Twin Sharing is Disabled)
                if (isAvailable && !isTwinSharing && overlaps.Count > 0)
                {
                    foreach (var existing in overlaps)
                    {
                        if (string.Equals(existing.Gender, "Female", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(gender, "Male", StringComparison.OrdinalIgnoreCase))
                        {
                            isAvailable = false;
                            message = "Selected room contains a Female occupant. Male guest is not allowed to join.";
                            break;
                        }
                    }
                }

                // 3. Dynamic Slots / Next Available Suggestions if Unavailable
                List<string> freeSlots = new List<string>();
                DateTime? nextAvailableDate = null;

                if (!isAvailable)
                {
                    // Scan availability day-by-day from requested check-in date onwards up to 30 days
                    DateTime scanStart = checkIn.Date;
                    DateTime scanEnd = checkIn.Date.AddDays(30);

                    // Fetch all bookings in this room type in the scan window
                    var scanParam = new DynamicParameters();
                    scanParam.Add("@p_Query", $@"
                        SELECT sb.*, sbd.Gender
                        FROM StayBooking sb
                        LEFT JOIN StayBookingDetails sbd ON sb.BookingId = sbd.BookingId
                        WHERE sb.RoomTypeId = {roomTypeId}
                          AND sb.BookingStatus IN ('Booked', 'Confirmed', 'Pending', 'CheckedIn')
                          AND sb.CheckInDate < '{scanEnd:yyyy-MM-dd}'
                          AND sb.CheckOutDate > '{scanStart:yyyy-MM-dd}'
                          {(excludeBookingId > 0 ? $"AND sb.BookingId != {excludeBookingId}" : "")}");

                    var scanBookings = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", scanParam).ToList();

                    // Find day availability
                    List<DateTime> availableDays = new List<DateTime>();
                    for (DateTime day = scanStart; day < scanEnd; day = day.AddDays(1))
                    {
                        // Check active bookings on this specific day
                        var dayBookings = scanBookings.Where(b => day >= b.CheckInDate.Date && day < b.CheckOutDate.Date).ToList();

                        bool dayOk = true;
                        if (dayBookings.Count > 0)
                        {
                            bool entireBlocked = dayBookings.Any(b => !b.TwinSharing);
                            bool wantsPrivate = !isTwinSharing;
                            bool full = dayBookings.Count >= 2;

                            if (entireBlocked || wantsPrivate || full)
                            {
                                dayOk = false;
                            }
                            else
                            {
                                // sharing check
                                foreach (var db in dayBookings)
                                {
                                    if (!string.Equals(gender, db.Gender, StringComparison.OrdinalIgnoreCase))
                                    {
                                        dayOk = false;
                                        break;
                                    }
                                }
                            }

                            // Private Room gender checks
                            if (dayOk && !isTwinSharing)
                            {
                                foreach (var db in dayBookings)
                                {
                                    if (string.Equals(db.Gender, "Female", StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(gender, "Male", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dayOk = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (dayOk)
                        {
                            availableDays.Add(day);
                        }
                    }

                    // Group available days into contiguous slots
                    if (availableDays.Count > 0)
                    {
                        DateTime? slotStart = null;
                        DateTime? prevDay = null;
                        int durationNights = (checkOut.Date - checkIn.Date).Days;

                        for (int i = 0; i < availableDays.Count; i++)
                        {
                            DateTime current = availableDays[i];
                            if (!slotStart.HasValue)
                            {
                                slotStart = current;
                            }
                            else if (current != prevDay.Value.AddDays(1))
                            {
                                // Close slot
                                freeSlots.Add($"{slotStart.Value:dd-MMM} to {prevDay.Value.AddDays(1):dd-MMM}");
                                slotStart = current;
                            }
                            prevDay = current;
                        }
                        if (slotStart.HasValue && prevDay.HasValue)
                        {
                            freeSlots.Add($"{slotStart.Value:dd-MMM} to {prevDay.Value.AddDays(1):dd-MMM}");
                        }

                        // Determine next available date where a continuous block matching duration is free
                        for (DateTime d = scanStart; d <= scanEnd.AddDays(-durationNights); d = d.AddDays(1))
                        {
                            bool blockOk = true;
                            for (int n = 0; n < durationNights; n++)
                            {
                                if (!availableDays.Contains(d.AddDays(n)))
                                {
                                    blockOk = false;
                                    break;
                                }
                            }
                            if (blockOk)
                            {
                                nextAvailableDate = d;
                                break;
                            }
                        }
                    }
                }

                string suggestMessage = "";
                if (!isAvailable)
                {
                    string bookedStr = string.Join(", ", bookedIntervals);
                    suggestMessage = $"Room is booked from {bookedStr}.";
                    if (nextAvailableDate.HasValue)
                    {
                        suggestMessage += $" Next available slot starts from {nextAvailableDate.Value:dd-MMM-yyyy}.";
                    }
                }

                return Json(new
                {
                    success = isAvailable,
                    message = isAvailable ? "Selected room is available." : message,
                    suggestMessage = suggestMessage,
                    freeSlots = freeSlots,
                    nextAvailableDate = nextAvailableDate?.ToString("dd-MMM-yyyy"),
                    existingChildType = existingChildType
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "System Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult SearchPatients(string term)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_Query", $@"
                    SELECT TOP 20 
                        PatientFid,
                        FirstName + ' ' + ISNULL(LName, '') as Name, 
                        ContactNumber as Mobile,
                        Email,
                        Gender,
                        Age,
                        CityName,
                        Address
                    FROM SWG001.dbo.Patient 
                    WHERE FirstName LIKE '%{term}%' 
                       OR LName LIKE '%{term}%' 
                       OR ContactNumber LIKE '%{term}%'");

                var patients = BVGWellnessCenterDapparORM.DynamicList("sp_ExecuteQuery", param);

                var result = new List<object>();
                foreach (var p in patients)
                {
                    result.Add(new
                    {
                        label = p.Name + " - " + p.Mobile,
                        value = p.Name,
                        mobile = p.Mobile,
                        patientId = p.PatientFid,
                        email = p.Email,
                        gender = p.Gender,
                        age = p.Age,
                        city = p.CityName,
                        address = p.Address
                    });
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        // POST: IPD/IPD/SaveRoomBooking
        [HttpPost]
        public ActionResult SaveRoomBooking(StayBookingModel model)
        {
            SqlConnection conn = null;
            SqlTransaction trans = null;
            try
            {
                // Load Dropdown list for model reload in case of failure
                var paramTypes = new DynamicParameters();
                paramTypes.Add("@p_Query", "SELECT RoomTypeId, RoomName, MaxOccupancy, BasePrice FROM RoomTypes WHERE IsActive = 1");
                var roomDetails = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramTypes).ToList();
                ViewBag.RoomTypes = roomDetails.Select(r => new DropdownItem { Id = r.RoomTypeId, Name = r.RoomName + " (₹" + r.BasePrice + "/day)" }).ToList();
                ViewBag.RoomDetailsJson = Newtonsoft.Json.JsonConvert.SerializeObject(roomDetails);

                // Load Settings for reload
                var paramSettings = new DynamicParameters();
                paramSettings.Add("@p_Query", "SELECT SettingValue FROM MAS_IPDSettings WHERE SettingKey = 'PetChargePercentage'");
                var petPctStr = BVGWellnessCenterDapparORM.ReturnList<string>("sp_ExecuteQuery", paramSettings).FirstOrDefault();
                ViewBag.PetChargePercentage = string.IsNullOrEmpty(petPctStr) ? "25.00" : petPctStr;

                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Please fill all required fields correctly.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }

                // 1. Validation: Dates
                if (model.CheckInDate.Date >= model.CheckOutDate.Date)
                {
                    TempData["Message"] = "Check-In Date must be less than Check-Out Date.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }

                // 2. Validation: Occupancy Capacity
                // Option A: Max 3 Adults (when no children)
                // Option B: Max 2 Adults + 2 Children
                if (model.NoOfPersons > 3)
                {
                    TempData["Message"] = "Occupancy rule violation: Maximum of 3 Adults allowed.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }
                if (model.NoOfChildren > 2)
                {
                    TempData["Message"] = "Occupancy rule violation: Maximum of 2 Children allowed.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }
                if (model.NoOfPersons > 2 && model.NoOfChildren > 1)
                {
                    TempData["Message"] = "Occupancy rule violation: When there are 3 Adults, only 1 child is allowed.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }

                // 3. Validation: Child Ages (Original Logic)
                if (model.NoOfChildren > 0 && !string.IsNullOrEmpty(model.ChildAgeDetails))
                {
                    var ages = model.ChildAgeDetails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ages.Length != model.NoOfChildren)
                    {
                        TempData["Message"] = "Please enter age for all children.";
                        TempData["Icon"] = "warning";
                        return View("RoomBooking", model);
                    }
                    int ageVal;
                    foreach (var ageStr in ages)
                    {
                        if (int.TryParse(ageStr, out ageVal))
                        {
                            if (ageVal < 0 || ageVal > 12)
                            {
                                TempData["Message"] = "Child age validation failed: Children must be between 0 and 12 years.";
                                TempData["Icon"] = "warning";
                                return View("RoomBooking", model);
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Invalid child age value.";
                            TempData["Icon"] = "warning";
                            return View("RoomBooking", model);
                        }
                    }
                    
                    int cOld = 0;
                    foreach (var a in ages)
                    {
                        int av;
                        if (int.TryParse(a, out av) && av >= 6 && av <= 12) cOld++;
                    }
                    if (model.NoOfChildren == 2 && cOld > 1)
                    {
                        TempData["Message"] = "Occupancy rule violation: If there are 2 children, they must be (both 0-5 yrs) OR (one 0-5 yrs and one 6-12 yrs). Two 6-12 yrs children are not allowed.";
                        TempData["Icon"] = "warning";
                        return View("RoomBooking", model);
                    }
                    if (model.NoOfPersons == 3 && model.NoOfChildren == 1 && cOld > 0)
                    {
                        TempData["Message"] = "Occupancy rule violation: With 3 Adults, the 1 child must be 0-5 years old.";
                        TempData["Icon"] = "warning";
                        return View("RoomBooking", model);
                    }
                }

                // 3a. Validation: Family Members (New Logic)
                if (model.FamilyMember && model.FamilyMembers != null)
                {
                    int cOldFamily = 0;
                    foreach (var fm in model.FamilyMembers)
                    {
                        if (string.IsNullOrWhiteSpace(fm.FirstName) || string.IsNullOrWhiteSpace(fm.Gender) || fm.Age <= 0)
                        {
                            TempData["Message"] = "Please provide valid details (Name, Age, Gender) for all family members.";
                            TempData["Icon"] = "warning";
                            return View("RoomBooking", model);
                        }
                        if (fm.Age >= 6 && fm.Age <= 12) cOldFamily++;
                    }

                    if (model.NoOfChildren == 2 && cOldFamily > 1)
                    {
                        TempData["Message"] = "Occupancy rule violation: If there are 2 children, they must be (both 0-5 yrs) OR (one 0-5 yrs and one 6-12 yrs). Two 6-12 yrs children are not allowed.";
                        TempData["Icon"] = "warning";
                        return View("RoomBooking", model);
                    }
                    if (model.NoOfPersons == 3 && model.NoOfChildren == 1 && cOldFamily > 0)
                    {
                        TempData["Message"] = "Occupancy rule violation: With 3 Adults, the 1 child must be 0-5 years old.";
                        TempData["Icon"] = "warning";
                        return View("RoomBooking", model);
                    }
                }

                // Query Room details to verify capacity & charges
                var paramRoom = new DynamicParameters();
                paramRoom.Add("@p_Query", $"SELECT RoomTypeId, RoomName, MaxOccupancy, BasePrice FROM RoomTypes WHERE RoomTypeId = {model.RoomTypeId} AND IsActive = 1");
                var roomInfo = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", paramRoom).FirstOrDefault();
                if (roomInfo == null)
                {
                    TempData["Message"] = "Selected room type is invalid or inactive.";
                    TempData["Icon"] = "warning";
                    return View("RoomBooking", model);
                }

                // 4. Overlap & Gender Verification
                var checkJson = CheckAvailability(model.RoomTypeId, model.CheckInDate.ToString("yyyy-MM-dd"), model.CheckOutDate.ToString("yyyy-MM-dd"), model.TwinSharing, model.Gender, model.sameGender, model.BookingId);
                dynamic responseData = checkJson.Data;
                bool isRoomAvailable = responseData.success;
                if (!isRoomAvailable)
                {
                    TempData["Message"] = "Room Availability failed: " + responseData.message;
                    TempData["Icon"] = "error";
                    return View("RoomBooking", model);
                }

                string existingChildType = "None";
                try { existingChildType = responseData.existingChildType; } catch { }

                if (model.TwinSharing)
                {
                    model.StayType = "Double";
                    model.Own = false;
                    model.FamilyMember = false;

                    if (existingChildType == "Two_0_5")
                    {
                        if (model.NoOfChildren > 1) {
                            TempData["Message"] = "Cross-Booking Violation: Existing occupant has 2 children (0-5). You can only book 1 child (6-12).";
                            TempData["Icon"] = "warning";
                            return View("RoomBooking", model);
                        }
                        if (model.NoOfChildren == 1)
                        {
                            int age = 0;
                            if (!string.IsNullOrEmpty(model.ChildAgeDetails)) {
                                int.TryParse(model.ChildAgeDetails.Split(',')[0], out age);
                            }
                            if (age < 6 || age > 12) {
                                TempData["Message"] = "Cross-Booking Violation: Existing occupant has 2 children (0-5). Your child must be 6-12 years old.";
                                TempData["Icon"] = "warning";
                                return View("RoomBooking", model);
                            }
                        }
                    }
                    else if (existingChildType == "One_6_12")
                    {
                        if (model.NoOfChildren == 1) {
                            int age = 0;
                            if (!string.IsNullOrEmpty(model.ChildAgeDetails)) {
                                int.TryParse(model.ChildAgeDetails.Split(',')[0], out age);
                            }
                            if (age > 5) {
                                TempData["Message"] = "Cross-Booking Violation: Existing occupant has 1 child (6-12). Your child must be 0-5 years old.";
                                TempData["Icon"] = "warning";
                                return View("RoomBooking", model);
                            }
                        }
                    }
                }
                else
                {
                    model.StayType = "Seperate";
                    model.FamilyMember = !model.Own;
                }

                // Calculate Charges
                int nights = (model.CheckOutDate.Date - model.CheckInDate.Date).Days;
                if (nights <= 0) nights = 1;

                decimal roomBasePrice = roomInfo.BasePrice;

                // 1. Adults Charges
                decimal roomChargesPerDay = 0;
                if (!model.TwinSharing)
                {
                    // Private room: default to 2 persons charges
                    roomChargesPerDay = roomBasePrice * 2;
                    if (model.NoOfPersons >= 3) roomChargesPerDay += (roomBasePrice * 0.75m); // 3rd Adult Extra Bed
                }
                else
                {
                    if (model.NoOfPersons >= 1) roomChargesPerDay += roomBasePrice; // 1st Adult
                    if (model.NoOfPersons >= 2) roomChargesPerDay += roomBasePrice; // 2nd Adult
                    if (model.NoOfPersons >= 3) roomChargesPerDay += (roomBasePrice * 0.75m); // 3rd Adult
                }

                // 2. Child Charges
                decimal childChargesPerDay = 0;

                // Old Child Charges Logic
                if (model.TwinSharing && model.NoOfChildren > 0 && !string.IsNullOrEmpty(model.ChildAgeDetails))
                {
                    var ages = model.ChildAgeDetails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    int ageVal;
                    foreach (var ageStr in ages)
                    {
                        if (int.TryParse(ageStr, out ageVal))
                        {
                            if (ageVal >= 6 && ageVal <= 12)
                            {
                                // 6-12 years: 50% of BasePrice
                                childChargesPerDay += roomBasePrice * 0.50m;
                            }
                            // 0-5 years is Free (0%)
                        }
                    }
                }

                // New Family Members Child Charges Logic
                if (model.FamilyMember && model.FamilyMembers != null)
                {
                    foreach (var fm in model.FamilyMembers)
                    {
                        if (fm.Age >= 6 && fm.Age <= 12)
                        {
                            // 6-12 years: 50% of BasePrice
                            childChargesPerDay += roomBasePrice * 0.50m;
                        }
                    }
                }

                decimal totalPerDayBeforePet = roomChargesPerDay + childChargesPerDay;

                // 3. Pet Charges (25% Surcharge on Total)
                decimal petChargesPerDay = 0;
                if (model.IsPet)
                {
                    decimal petPercentage = 25.00m;
                    decimal dbPct;
                    if (decimal.TryParse(petPctStr, out dbPct))
                    {
                        petPercentage = dbPct;
                    }
                    petChargesPerDay = roomBasePrice * (petPercentage / 100m);
                }

                decimal dailyTotal = totalPerDayBeforePet + petChargesPerDay;
                decimal grandTotal = dailyTotal * nights;

                model.TotalAmount = grandTotal;

                // Save to database
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;
                if (Session["NewConnetionString"] != null)
                {
                    connStr = Session["NewConnetionString"].ToString();
                }

                conn = new SqlConnection(connStr);
                conn.Open();
                trans = conn.BeginTransaction();

                int currentUserId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 1;

                // Explicitly set BookingSource to Ayurmitra
                model.BookingSource = "Ayurmitra";
                model.BookingStatus = "Confirmed";
                int savedBookingId = model.BookingId;

                if (model.BookingId == 0)
                {
                    // INSERT
                    string insertBookingSql = @"
                        INSERT INTO StayBooking (
                            CreatedBy, PatientId, RoomTypeId, RoomAvailablityId, StayType, CheckInDate, CheckOutDate,
                            Status, NoOfPersons, NoOfChildren, IsPet, TwinSharing, sameGender, TotalAmount, GuestFullName,
                            EmailId, Mobile, City, BookingStatus, BookingSource, Own, FamilyMember
                        ) VALUES (
                            @CreatedBy, @PatientId, @RoomTypeId, @RoomAvailablityId, @StayType, @CheckInDate, @CheckOutDate,
                            @Status, @NoOfPersons, @NoOfChildren, @IsPet, @TwinSharing, @sameGender, @TotalAmount, @GuestFullName,
                            @EmailId, @Mobile, @City, @BookingStatus, @BookingSource, @Own, @FamilyMember
                        );
                        SELECT SCOPE_IDENTITY();";

                    int newBookingId = conn.QueryFirstOrDefault<int>(insertBookingSql, new
                    {
                        CreatedBy = currentUserId,
                        PatientId = model.PatientId,
                        RoomTypeId = model.RoomTypeId,
                        RoomAvailablityId = model.RoomAvailablityId,
                        StayType = model.StayType,
                        CheckInDate = model.CheckInDate,
                        CheckOutDate = model.CheckOutDate,
                        Status = model.Status,
                        NoOfPersons = model.NoOfPersons,
                        NoOfChildren = model.NoOfChildren,
                        IsPet = model.IsPet,
                        TwinSharing = model.TwinSharing,
                        sameGender = model.sameGender,
                        TotalAmount = model.TotalAmount,
                        GuestFullName = model.GuestFullName,
                        EmailId = model.EmailId,
                        Mobile = model.Mobile,
                        City = model.City,
                        BookingStatus = model.BookingStatus,
                        BookingSource = model.BookingSource,
                        Own = model.Own,
                        FamilyMember = model.FamilyMember
                    }, trans);

                    string insertDetailsSql = @"
                        INSERT INTO StayBookingDetails (
                            BookingId, Gender, Age, Address, RoomNo, ChildAgeDetails, PetCount, SpecialNotes, RefDoctor, EmergencyContact
                        ) VALUES (
                            @BookingId, @Gender, @Age, @Address, @RoomNo, @ChildAgeDetails, @PetCount, @SpecialNotes, @RefDoctor, @EmergencyContact
                        );";

                    conn.Execute(insertDetailsSql, new
                    {
                        BookingId = newBookingId,
                        Gender = model.Gender,
                        Age = model.Age,
                        Address = model.Address,
                        RoomNo = model.RoomNo,
                        ChildAgeDetails = model.ChildAgeDetails,
                        PetCount = model.PetCount,
                        SpecialNotes = model.SpecialNotes,
                        RefDoctor = model.RefDoctor,
                        EmergencyContact = model.EmergencyContact
                    }, trans);

                    TempData["Message"] = "Stay Booking Created Successfully!";
                    TempData["Icon"] = "success";
                    savedBookingId = newBookingId;
                }
                else
                {
                    // UPDATE
                    string updateBookingSql = @"
                        UPDATE StayBooking SET
                            RoomTypeId = @RoomTypeId,
                            CheckInDate = @CheckInDate,
                            CheckOutDate = @CheckOutDate,
                            NoOfPersons = @NoOfPersons,
                            NoOfChildren = @NoOfChildren,
                            IsPet = @IsPet,
                            TwinSharing = @TwinSharing,
                            sameGender = @sameGender,
                            TotalAmount = @TotalAmount,
                            GuestFullName = @GuestFullName,
                            EmailId = @EmailId,
                            Mobile = @Mobile,
                            City = @City,
                            Status = @Status,
                            BookingStatus = @BookingStatus,
                            Own = @Own,
                            FamilyMember = @FamilyMember
                        WHERE BookingId = @BookingId";

                    conn.Execute(updateBookingSql, new
                    {
                        RoomTypeId = model.RoomTypeId,
                        CheckInDate = model.CheckInDate,
                        CheckOutDate = model.CheckOutDate,
                        NoOfPersons = model.NoOfPersons,
                        NoOfChildren = model.NoOfChildren,
                        IsPet = model.IsPet,
                        TwinSharing = model.TwinSharing,
                        sameGender = model.sameGender,
                        TotalAmount = model.TotalAmount,
                        GuestFullName = model.GuestFullName,
                        EmailId = model.EmailId,
                        Mobile = model.Mobile,
                        City = model.City,
                        Status = model.Status,
                        BookingStatus = model.BookingStatus,
                        Own = model.Own,
                        FamilyMember = model.FamilyMember,
                        BookingId = model.BookingId
                    }, trans);

                    // Check if details exist
                    string checkDetailsSql = "SELECT COUNT(*) FROM StayBookingDetails WHERE BookingId = @BookingId";
                    int detailsCount = conn.QueryFirstOrDefault<int>(checkDetailsSql, new { BookingId = model.BookingId }, trans);

                    if (detailsCount > 0)
                    {
                        string updateDetailsSql = @"
                            UPDATE StayBookingDetails SET
                                Gender = @Gender,
                                Age = @Age,
                                Address = @Address,
                                RoomNo = @RoomNo,
                                ChildAgeDetails = @ChildAgeDetails,
                                PetCount = @PetCount,
                                SpecialNotes = @SpecialNotes,
                                RefDoctor = @RefDoctor,
                                EmergencyContact = @EmergencyContact
                            WHERE BookingId = @BookingId";

                        conn.Execute(updateDetailsSql, new
                        {
                            Gender = model.Gender,
                            Age = model.Age,
                            Address = model.Address,
                            RoomNo = model.RoomNo,
                            ChildAgeDetails = model.ChildAgeDetails,
                            PetCount = model.PetCount,
                            SpecialNotes = model.SpecialNotes,
                            RefDoctor = model.RefDoctor,
                            EmergencyContact = model.EmergencyContact,
                            BookingId = model.BookingId
                        }, trans);
                    }
                    else
                    {
                        string insertDetailsSql = @"
                            INSERT INTO StayBookingDetails (
                                BookingId, Gender, Age, Address, RoomNo, ChildAgeDetails, PetCount, SpecialNotes, RefDoctor, EmergencyContact
                            ) VALUES (
                                @BookingId, @Gender, @Age, @Address, @RoomNo, @ChildAgeDetails, @PetCount, @SpecialNotes, @RefDoctor, @EmergencyContact
                            );";

                        conn.Execute(insertDetailsSql, new
                        {
                            BookingId = model.BookingId,
                            Gender = model.Gender,
                            Age = model.Age,
                            Address = model.Address,
                            RoomNo = model.RoomNo,
                            ChildAgeDetails = model.ChildAgeDetails,
                            PetCount = model.PetCount,
                            SpecialNotes = model.SpecialNotes,
                            RefDoctor = model.RefDoctor,
                            EmergencyContact = model.EmergencyContact
                        }, trans);
                    }

                    TempData["Message"] = "Stay Booking Updated Successfully!";
                    TempData["Icon"] = "success";
                }

                // Save Family Members (for both INSERT and UPDATE)
                string deleteFamilySql = "DELETE FROM StayBookingFamilyMember WHERE BookingId = @BookingId";
                conn.Execute(deleteFamilySql, new { BookingId = savedBookingId }, trans);

                if (model.FamilyMember && model.FamilyMembers != null && model.FamilyMembers.Count > 0)
                {
                    string insertFamilySql = @"
                        INSERT INTO StayBookingFamilyMember (BookingId, FirstName, LastName, Age, Gender) 
                        VALUES (@BookingId, @FirstName, @LastName, @Age, @Gender)";

                    foreach (var fm in model.FamilyMembers)
                    {
                        conn.Execute(insertFamilySql, new
                        {
                            BookingId = savedBookingId,
                            FirstName = fm.FirstName,
                            LastName = fm.LastName ?? "",
                            Age = fm.Age,
                            Gender = fm.Gender
                        }, trans);
                    }
                }

                trans.Commit();

                return RedirectToAction("RoomBilling", new { bookingId = savedBookingId });
            }
            catch (Exception ex)
            {
                if (trans != null) trans.Rollback();
                TempData["Message"] = "Error saving booking: " + ex.Message;
                TempData["Icon"] = "error";
                return View("RoomBooking", model);
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // AJAX GET: IPD/IPD/GetBookedDates
        [HttpGet]
        public JsonResult GetBookedDates(int roomTypeId, bool isTwinSharing, string gender, bool sameGender, int? excludeBookingId)
        {
            try
            {
                // Scan the next 6 months day-by-day
                DateTime scanStart = DateTime.Today;
                DateTime scanEnd = scanStart.AddMonths(6);

                var scanParam = new DynamicParameters();
                scanParam.Add("@p_Query", $@"
                    SELECT sb.*, p.Gender
                    FROM StayBooking sb
                    LEFT JOIN Patient p ON sb.PatientId = p.PatientFid
                    WHERE sb.RoomTypeId = {roomTypeId}
                      AND sb.BookingStatus = 'Confirmed'
                      AND sb.CheckInDate <= '{scanEnd:yyyy-MM-dd}'
                      AND sb.CheckOutDate >= '{scanStart:yyyy-MM-dd}'
                      {(excludeBookingId.HasValue && excludeBookingId > 0 ? $"AND sb.BookingId != {excludeBookingId}" : "")}");

                var scanBookings = BVGWellnessCenterDapparORM.ReturnList<StayBookingModel>("sp_ExecuteQuery", scanParam).ToList();

                // Fetch MaxOccupancy for the room (for Outdoor Living Room occupancy 20)
                var rtParam = new DynamicParameters();
                rtParam.Add("@p_Query", $"SELECT MaxOccupancy FROM RoomTypes WHERE RoomTypeId = {roomTypeId}");
                int maxOcc = BVGWellnessCenterDapparORM.ReturnList<int>("sp_ExecuteQuery", rtParam).FirstOrDefault();
                int twinSharingLimit = (maxOcc > 3) ? maxOcc : 2;

                List<string> blockedDates = new List<string>();

                for (DateTime day = scanStart; day <= scanEnd; day = day.AddDays(1))
                {
                    var dayBookings = scanBookings.Where(b => day >= b.CheckInDate.Date && day < b.CheckOutDate.Date).ToList();

                    bool dayOk = true;
                    if (dayBookings.Count > 0)
                    {
                        bool entireBlocked = dayBookings.Any(b => !b.TwinSharing);
                        bool wantsPrivate = !isTwinSharing;
                        bool full = dayBookings.Count >= twinSharingLimit;

                        if (entireBlocked || wantsPrivate || full)
                        {
                            dayOk = false;
                        }
                        else if (sameGender)
                        {
                            // Twin Sharing check with sameGender option
                            foreach (var db in dayBookings)
                            {
                                if (!string.Equals(gender, db.Gender, StringComparison.OrdinalIgnoreCase))
                                {
                                    dayOk = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (!dayOk)
                    {
                        blockedDates.Add(day.ToString("yyyy-MM-dd"));
                    }
                }

                return Json(blockedDates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: IPD/IPD/UpdateBookingStatus?id=X&status=Y
        public ActionResult UpdateBookingStatus(int id, string status)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                var param = new DynamicParameters();
                param.Add("@p_Query", $"UPDATE StayBooking SET BookingStatus = '{status}', Status = '{status}' WHERE BookingId = {id}");
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", param);

                TempData["Message"] = $"Booking status updated to {status} successfully.";
                TempData["Icon"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error updating status: " + ex.Message;
                TempData["Icon"] = "error";
            }

            return RedirectToAction("Index");
        }

        // GET: IPD/IPD/DeleteBooking?id=X
        public ActionResult DeleteBooking(int id)
        {
            try
            {
                if (Session["UserID"] == null)
                    return RedirectToAction("Login", "Home", new { area = "" });

                var param = new DynamicParameters();
                param.Add("@p_Query", $"UPDATE StayBooking SET BookingStatus = 'Cancelled', Status = 'Cancelled' WHERE BookingId = {id}");
                BVGWellnessCenterDapparORM.ExecuteWithoutReturn("sp_ExecuteQuery", param);

                TempData["Message"] = "Booking cancelled successfully.";
                TempData["Icon"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error cancelling booking: " + ex.Message;
                TempData["Icon"] = "error";
            }

            return RedirectToAction("RoomBookingList");
        }

        #region Room Billing Methods
        [HttpGet]
        public ActionResult RoomBilling(int bookingId)
        {
            var model = new RoomBillingModel();
            try
            {
                string query = "SELECT BookingId, GuestFullName, Mobile, TotalAmount, PatientId FROM StayBooking WHERE BookingId = @BookingId";
                var param = new DynamicParameters();
                param.Add("@p_Query", query.Replace("@BookingId", bookingId.ToString()));
                var booking = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
                var b = booking.FirstOrDefault();
                if (b != null)
                {
                    model.BookingId = b.BookingId;
                    model.GuestFullName = b.GuestFullName;
                    model.Mobile = b.Mobile;
                    model.TotalAmount = b.TotalAmount;
                    model.AmountPaying = b.TotalAmount;
                    model.PatientId = b.PatientId != null ? Convert.ToInt32(b.PatientId) : 0;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading booking details: " + ex.Message;
                TempData["Icon"] = "error";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoomBilling(RoomBillingModel model)
        {
            try
            {
                string insertPaymentSql = @"
                    INSERT INTO SWG001.dbo.PaymentTransactions (
                        Fdate, BookingId, PaymentFor, amount, PaymentStatus, patientId, 
                        PaymentMode, BankName, BankTxnId
                    ) VALUES (
                        GETDATE(), @BookingId, 'StayBooking', @Amount, @PaymentStatus, @PatientId, 
                        @PaymentMode, @BankName, @BankTxnId
                    );
                    SELECT SCOPE_IDENTITY();";

                var param = new DynamicParameters();
                param.Add("@BookingId", model.BookingId);
                param.Add("@Amount", model.AmountPaying);
                param.Add("@PaymentStatus", model.PaymentStatus);
                param.Add("@PatientId", model.PatientId);
                param.Add("@PaymentMode", model.PaymentMode);
                param.Add("@BankName", model.BankName);
                param.Add("@BankTxnId", model.BankTxnId);

                using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString))
                {
                    conn.Open();
                    int paymentId = conn.QueryFirstOrDefault<int>(insertPaymentSql, param);

                    TempData["Message"] = "Billing record saved successfully!";
                    TempData["Icon"] = "success";
                    return RedirectToAction("RoomBillingList");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error saving billing: " + ex.Message;
                TempData["Icon"] = "error";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult RoomBillingList()
        {
            try
            {
                string query = @"
                    SELECT p.Id, p.Fdate, p.BookingId, p.amount, p.PaymentStatus, p.PaymentMode, 
                           sb.GuestFullName, sb.Mobile 
                    FROM SWG001.dbo.PaymentTransactions p
                    LEFT JOIN StayBooking sb ON p.BookingId = sb.BookingId
                    WHERE p.PaymentFor IN ('StayBooking', 'RoomBooking')
                    ORDER BY p.Id DESC";

                var param = new DynamicParameters();
                param.Add("@p_Query", query);
                var list = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
                return View(list);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error fetching billing list: " + ex.Message;
                TempData["Icon"] = "error";
                return View(new List<dynamic>());
            }
        }

        [HttpGet]
        public ActionResult PrintRoomBill(int paymentId)
        {
            try
            {
                string query = @"
                    SELECT p.Id, p.Fdate, p.BookingId, p.amount, p.PaymentStatus, p.PaymentMode, 
                           p.BankName, p.BankTxnId, sb.GuestFullName, sb.Mobile, sb.TotalAmount, 
                           sb.CheckInDate, sb.CheckOutDate, sb.StayType
                    FROM SWG001.dbo.PaymentTransactions p
                    LEFT JOIN StayBooking sb ON p.BookingId = sb.BookingId
                    WHERE p.Id = @PaymentId";

                var param = new DynamicParameters();
                param.Add("@p_Query", query.Replace("@PaymentId", paymentId.ToString()));

                var detailsList = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_ExecuteQuery", param);
                var details = detailsList.FirstOrDefault();
                return View(details);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading print view: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("RoomBillingList");
            }
        }
        #endregion
    }
}