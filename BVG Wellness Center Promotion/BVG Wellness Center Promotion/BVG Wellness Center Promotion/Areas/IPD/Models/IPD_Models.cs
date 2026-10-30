using System;
using System.Collections.Generic;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Models
{
    // =============================================
    // FLOOR MASTER
    // =============================================
    public class IPD_FloorModel
    {
        public int FloorId { get; set; }
        public string FloorName { get; set; }
        public int FloorNumber { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // =============================================
    // WARD MASTER
    // =============================================
    public class IPD_WardModel
    {
        public int WardId { get; set; }
        public int? FloorId { get; set; }
        public string WardName { get; set; }
        public string WardType { get; set; } = "General";
        public bool IsActive { get; set; } = true;
    }

    // =============================================
    // CATEGORY MASTER
    // =============================================
    public class IPD_CategoryModel
    {
        public int CategoryId { get; set; }
        public int? FloorId { get; set; }
        public int? WardId { get; set; }
        public string CategoryName { get; set; }
        public string FoodOption { get; set; } = "None";  // None/Optional/Mandatory
        public bool IsActive { get; set; } = true;
    }

    // =============================================
    // ROOM CHARGES MASTER
    // =============================================
    public class IPD_RoomChargesModel
    {
        public int ChargeId { get; set; }
        public string ChargeName { get; set; }
        public string ChargeType { get; set; } = "ServiceCharge";  // RoomCharge/FoodCharge/ServiceCharge
        public decimal DefaultAmount { get; set; }
        public bool IsActive { get; set; } = true;

        // For mapping (used in Room form)
        public int MappingId { get; set; }
        public int RoomId { get; set; }
        public decimal ChargeAmount { get; set; }   // room-specific override
        public bool IsCompulsory { get; set; }      // true=locked, false=optional
        public bool IsSelected { get; set; }        // used in booking form
    }

    // =============================================
    // ROOM MASTER
    // =============================================
    public class IPD_RoomModel
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public int? FloorId { get; set; }
        public int? WardId { get; set; }
        public int? CategoryId { get; set; }
        public decimal RoomChargePerDay { get; set; }
        public bool IsActive { get; set; } = true;

        // For display (joined from masters)
        public string FloorName { get; set; }
        public string WardName { get; set; }
        public string CategoryName { get; set; }

        // For Room form — mapped charges
        public List<IPD_RoomChargesModel> MappedCharges { get; set; } = new List<IPD_RoomChargesModel>();
    }

    // =============================================
    // BED MASTER
    // =============================================
    public class IPD_BedModel
    {
        public int BedId { get; set; }
        public int RoomId { get; set; }
        public string BedNumber { get; set; }
        public string BedStatus { get; set; } = "Available";  // Available/Occupied/Cleaning/Maintenance/Reserved
        public bool IsActive { get; set; } = true;

        // For display
        public string RoomNumber { get; set; }
        public string FloorName { get; set; }
        public string WardName { get; set; }
        public string CategoryName { get; set; }
    }

    // =============================================
    // FOOD PLAN MASTER
    // =============================================
    public class IPD_FoodPlanModel
    {
        public int FoodPlanId { get; set; }
        public string PlanName { get; set; }
        public decimal DailyCharge { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // =============================================
    // HOSPITAL SETTINGS
    // =============================================
    public class IPD_HospitalSettingModel
    {
        public int SettingId { get; set; }
        public bool UseFloor { get; set; }
        public bool UseWard { get; set; }
        public bool UseCategory { get; set; } = true;
        public bool FoodModuleEnabled { get; set; }
        public bool FoodMandatory { get; set; }
        public bool AdvanceRequired { get; set; }
        public bool BedTransferAllowed { get; set; } = true;
        public bool PackageBillingEnabled { get; set; }
        public bool AttendantAllowed { get; set; }
        public bool AutoReleaseBedOnDischarge { get; set; } = true;
        public bool GSTEnabled { get; set; }
        public decimal GSTRate { get; set; } = 5.00m;
        public bool EnableHierarchicalMapping { get; set; }
    }

    // =============================================
    // STAY BOOKING
    // =============================================
    public class IPD_StayBookingModel
    {
        // Header
        public int BookingId { get; set; }
        public string BookingNo { get; set; }
        public int PatientFid { get; set; }
        public int? DoctorId { get; set; }
        public string ReferredBy { get; set; }
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateTime ExpectedDischarge { get; set; } = DateTime.Now.AddDays(1);
        public DateTime? ActualDischarge { get; set; }
        public string AdmissionType { get; set; } = "Planned";  // Emergency/Planned/DayCare
        public string Status { get; set; } = "Active";          // Active/Discharged/Cancelled
        public string Remarks { get; set; }
        public decimal FinalBillAmount { get; set; }

        // Location
        public int? FloorId { get; set; }
        public int? WardId { get; set; }
        public int? CategoryId { get; set; }
        public int RoomId { get; set; }
        public int BedId { get; set; }
        public decimal RoomChargeSnapshot { get; set; }

        // Food
        public int? FoodPlanId { get; set; }
        public decimal FoodDailyCharge { get; set; }

        // Advance
        public decimal AdvanceAmount { get; set; }
        public string AdvancePayMode { get; set; } = "Cash";

        // For display (joined)
        public string RoomNumber { get; set; }
        public string BedNumber { get; set; }
        public string FloorName { get; set; }
        public string WardName { get; set; }
        public string CategoryName { get; set; }
        public int DaysCount { get; set; }

        // Charges selected in admission form
        public List<int> SelectedOptionalChargeIds { get; set; } = new List<int>();
    }

    // =============================================
    // BILL MODEL
    // =============================================
    public class IPD_BillLineModel
    {
        public string BillSection { get; set; }      // RoomRent/CompulsoryCharge/OptionalCharge/FoodCharge
        public string Description { get; set; }
        public decimal RatePerDay { get; set; }
        public int Days { get; set; }
        public decimal Amount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class IPD_BillSummaryModel
    {
        public int BookingId { get; set; }
        public string BookingNo { get; set; }
        public List<IPD_BillLineModel> Lines { get; set; } = new List<IPD_BillLineModel>();
        public decimal RoomRentTotal { get; set; }
        public decimal CompulsoryTotal { get; set; }
        public decimal OptionalTotal { get; set; }
        public decimal FoodTotal { get; set; }
        public decimal Subtotal { get; set; }
        
        public string DiscountType { get; set; } = "None";
        public decimal DiscountValue { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public string DiscountReason { get; set; }
        
        public bool GSTEnabled { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal NetPayable { get; set; }
    }

    // =============================================
    // BILLING
    // =============================================
    public class IPD_BillingModel
    {
        public int BillId { get; set; }
        public string BillNo { get; set; }
        public int BookingId { get; set; }
        public int PatientFid { get; set; }
        public decimal RoomTotal { get; set; }
        public decimal CompulsoryTotal { get; set; }
        public decimal OptionalTotal { get; set; }
        public decimal FoodTotal { get; set; }
        public decimal Subtotal { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal NetPayable { get; set; }
        public string DiscountType { get; set; } = "None";
        public decimal DiscountValue { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public string DiscountReason { get; set; }
        public DateTime BillDate { get; set; }
        public int? GeneratedBy { get; set; }
    }

    // =============================================
    // BED DASHBOARD
    // =============================================
    public class IPD_BedDashboardModel
    {
        public int BedId { get; set; }
        public string BedNumber { get; set; }
        public string BedStatus { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public decimal RoomChargePerDay { get; set; }
        public int? FloorId { get; set; }
        public string FloorName { get; set; }
        public int FloorNumber { get; set; }
        public int? WardId { get; set; }
        public string WardName { get; set; }
        public string WardType { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    // =============================================
    // TRANSFER HISTORY
    // =============================================
    public class IPD_TransferHistoryModel
    {
        public int TransferId { get; set; }
        public DateTime TransferDate { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }
        public string OldRoom { get; set; }
        public string OldBed { get; set; }
        public string NewRoom { get; set; }
        public string NewBed { get; set; }
    }
}
