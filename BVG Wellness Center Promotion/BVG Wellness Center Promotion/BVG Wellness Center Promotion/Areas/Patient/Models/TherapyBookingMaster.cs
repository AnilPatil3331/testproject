
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class TherapyBookingMaster
    {
        //public string CommunityType { get; set; } // CSV of selected checkboxes


        // Rename Self to match table column or map explicitly
        public string SelfName { get; set; } // if table uses SelfName

        // Physical Fitness / Investigations (checkboxes)
        public bool ECGStatus { get; set; }
        public bool BloodTestStatus { get; set; }
        public bool XRayStatus { get; set; }
        public bool OtherTestsStatus { get; set; }

        public int ServiceGroupID { get; set; }
        //public int Fid { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }

        public int TherapyBookingId { get; set; }
        //-----------------------add patient ------------------------
        public int? PatientId { get; set; }
        public int PatientFid { get; set; }
        public int? AppointmentFid { get; set; }
        public string DoctorName { get; set; }
        public string Department { get; set; }

        public int sloBookingId { get; set; }

        // for new patient
        public string PatientName { get; set; }
        public string MobileNo { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }

        public bool IsChargeable { get; set; }  // For medicines - checkbox array


        public int? Duration { get; set; }
        public string[] MedicineName { get; set; }
        public int[] MedicineFid { get; set; }
        public string[] Doses { get; set; }
        public string[] Frequncy { get; set; }
        public int?[] DurationM { get; set; }
        public int?[] Quantity { get; set; }
        public bool[] IsChargeableArray { get; set; }
        public string[] DurationType { get; set; }  // For medicines - checkbox array
        // --------------------- 1. Basic Details ---------------------
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public int ServiceFid { get; set; }
        public int ServiceCategoryFid { get; set; }
        public int ServiceRoomID { get; set; }
        public string ServiceName { get; set; }
        public string LocalName { get; set; }
        public string Slogan { get; set; }
        public string Uses { get; set; }
        public string Description { get; set; }

        // --------------------- 2. Authority & Referral ---------------------
        public string AdvisedBy { get; set; }
        public int DoctorFid { get; set; }
        //public int PhysicianId { get; set; }
        public string AttendantName { get; set; }
        public string Self { get; set; }
        //public int PatientId { get; set; }
        public string OPDType { get; set; }
        public string OtherOPDType { get; set; }
        public string[] CommunityType { get; set; }


        // --------------------- 3. Facility / Room ---------------------
        public string FacilityType { get; set; }
        public string IPDCategory { get; set; }
        public string RoomType { get; set; }
        public string RoomFrequency { get; set; }
        public int? NumberOfSessions { get; set; }


        // --------------------- 4. Therapy Execution ---------------------
        public int TherapistId { get; set; }
        public int NoOfDays { get; set; }
        public int? CustomNoOfDays { get; set; } // For custom days input
        public string TimeSlot { get; set; }
        //public string Duration { get; set; }

        public int? CustomDuration { get; set; } // For custom duration input
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // --------------------- 5. Billing ---------------------

        public int Charges { get; set; }
        //public decimal TherapyValue { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal BalanceAmount { get; set; }

        // --------------------- 6. Medical Fitness & Investigations ---------------------
        public string Investigations { get; set; }  
        public string InvestigationsOther { get; set; } 
        public string LMPDate { get; set; }
        public bool ConsentTaken { get; set; }

        // --------------------- 7. Room / Place ---------------------
        public string TherapyPlaces { get; set; }  
        public string TherapyPlacesOther { get; set; } // 

        // --------------------- 8. Checklist & Procedure ---------------------
        public bool PatientPrepared { get; set; }
        public bool TherapyPrepared { get; set; }
        public string Instruments { get; set; }
        public string PrerequisiteProcedure { get; set; }
        public string MainProcedure { get; set; }
        public string PostProcedureCare { get; set; }

        // Medicines - repeating fields serialized
        public string Medicines { get; set; }

        // --------------------- 9. Instructions & Safety ---------------------
        public string PatientInstructions { get; set; }
        public string RelativeInstructions { get; set; }
        public string TherapistInstructions { get; set; }
        public string Precautions { get; set; }
        public string ContraIndications { get; set; }
        public string RelatedDisorders { get; set; }

       

        public string TherapistName { get; set; }
        //public decimal TotalAmoount { get; set; }

        public decimal TotalAmount { get; set; }
        /// New property for multi-slot booking
        public List<TherapySlotVM> SelectedSlots { get; set; } = new List<TherapySlotVM>();



        // Billing 

       

       
     
        public int? Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }

        
        public float Price { get; set; }

        public float Total { get; set; }
    
        public float finalAmount { get; set; }
        public float Discount { get; set; }
        public float FinalPaybalAmount { get; set; }
        public string PaymentStatus { get; set; }
  
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }
        public float Amount { get; set; }
        public string FlagId { get; set; }

        //public int? ServicesSession { get; set; }

    }

    public class TherapySlotVM
    {
        public string date { get; set; }        // ✅ small 'd'
        public string startTime { get; set; }   // ✅ camelCase
        public string endTime { get; set; }
        public int ServiceRoomID { get; set; }
        public int IsBooked { get; set; }
    }
   

    public class TherapyBookingListVM
    {
        public decimal TotalPaid { get; set; }
        public decimal FinalPaybalAmount { get; set; }
        public string FlagId { get; set; }
        public bool IsPaid { get; set; }
        public bool IsPartiallyPaid => TotalPaid > 0 && TotalPaid < (FinalPaybalAmount > 0 ? FinalPaybalAmount : TotalAmount);
        public bool IsFullyPaid => TotalPaid > 0 && TotalPaid >= (FinalPaybalAmount > 0 ? FinalPaybalAmount : TotalAmount);
        public int SlotBookingId { get; set; }
       public int? PatientFid { get; set; }
        public int TherapyBookingId { get; set; }
        public string PatientName { get; set; }
        public string ServiceName { get; set; }
        public string RoomName { get; set; }
        public string DoctorName { get; set; }
        public string TherapistName { get; set; }
        public string BookingSource { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal BalanceAmount { get; set; }

        //public DateTime SlotDate { get; set; }
        //public DateTime SlotStart { get; set; }
        //public DateTime SlotEnd { get; set; }

        public DateTime? SlotDate { get; set; }
        public DateTime? SlotStart { get; set; }
        public DateTime? SlotEnd { get; set; }

        public string SlotsDisplay { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public List<TherapySlotDisplayVM> Slots { get; set; } = new List<TherapySlotDisplayVM>();
        public List<GroupedSlotVM> GroupedSlots { get; set; }
    }

    public class GroupedSlotVM
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DisplayText => $"{Date.ToString("dd MMM yyyy")}\n{StartTime} to {EndTime}";
    }

    public class AdvisedTherapyVMs
    {
        public int ServiceFid { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategories { get; set; }
        public string DaysNumber { get; set; }
        public string Frequency { get; set; }
        public string InstructionPatinet { get; set; }
        public string InstructionTherapist { get; set; }
        public string Consent { get; set; }
       
    }

    public class TherapySlotDisplayVM
    {
        public DateTime SlotDate { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
    }

    public class TherapySlotConfigVM
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsBreak { get; set; }
        public string BreakLabel { get; set; }
    }
}
