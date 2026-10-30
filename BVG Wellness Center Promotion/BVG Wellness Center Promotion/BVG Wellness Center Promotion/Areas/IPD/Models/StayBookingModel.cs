using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BVG_Wellness_Center_Promotion.Areas.IPD.Models
{
    public class StayBookingModel
    {
        // Fields matching SWG001.dbo.StayBooking
        public int BookingId { get; set; }
        public int? CreatedBy { get; set; }
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Room Type is required")]
        public int RoomTypeId { get; set; }

        public int? RoomAvailablityId { get; set; }
        public string StayType { get; set; } = "Single";

        [Required(ErrorMessage = "Check-In Date is required")]
        public DateTime CheckInDate { get; set; }

        public TimeSpan? CheckInTime { get; set; }

        [Required(ErrorMessage = "Check-Out Date is required")]
        public DateTime CheckOutDate { get; set; }

        public TimeSpan? CheckOutTime { get; set; }
        public DateTime? UserCheckInDateTime { get; set; }
        public DateTime? UserCheckOutDateTime { get; set; }
        public string Status { get; set; } = "Booked";

        [Required(ErrorMessage = "Adult count is required")]
        [Range(1, 3, ErrorMessage = "Adult count must be between 1 and 3")]
        public int NoOfPersons { get; set; } // Adults

        [Range(0, 2, ErrorMessage = "Child count cannot exceed 2")]
        public int NoOfChildren { get; set; }

        public bool IsPet { get; set; }
        public bool TwinSharing { get; set; }
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Patient Name is required")]
        public string GuestFullName { get; set; }

        public string EmailId { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string Mobile { get; set; }

        public string City { get; set; }
        public string BookingStatus { get; set; } = "Booked";
        public string Images { get; set; }

        // Billing properties
        public string BillStatus { get; set; }
        public int? PaymentId { get; set; }
        public string BookingSource { get; set; } = "IPD Module";

        // Extra details matching SWG001.dbo.StayBookingDetails
        public int DetailsId { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Please enter a valid age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        public string RoomNo { get; set; }

        public string ChildAgeDetails { get; set; }

        public int? PetCount { get; set; } = 0;

        public string SpecialNotes { get; set; }

        public string RefDoctor { get; set; }

        public string EmergencyContact { get; set; }

        public bool sameGender { get; set; }

        // Additional read-only properties for UI display and calculation
        public string RoomName { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }

        // Family Sharing Properties
        public bool Own { get; set; } = true;
        public bool FamilyMember { get; set; }

        public List<StayBookingFamilyMemberModel> FamilyMembers { get; set; } = new List<StayBookingFamilyMemberModel>();
    }

    public class RoomBillingModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string GuestFullName { get; set; }
        public string Mobile { get; set; }
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Please enter amount to pay")]
        public decimal AmountPaying { get; set; }

        public string PaymentFor { get; set; } = "StayBooking";

        [Required(ErrorMessage = "Please select payment mode")]
        public string PaymentMode { get; set; }

        public string BankName { get; set; }
        public string BankTxnId { get; set; }
        public string PaymentStatus { get; set; } = "Success";

        public DateTime Fdate { get; set; }
        public int PatientId { get; set; }
    }

    public class StayBookingFamilyMemberModel
    {
        public int FamilyMemberId { get; set; }
        public int BookingId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required(ErrorMessage = "Age is required")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
    }
}
