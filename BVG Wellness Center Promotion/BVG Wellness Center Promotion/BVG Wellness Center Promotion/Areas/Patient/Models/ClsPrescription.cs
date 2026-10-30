using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class ClsPrescription
    {
        public int Fid { get; set; }
        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Instruction { get; set; }
        public int Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }
        public string ServiceName { get; set; }
        public int ServiceFid { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public float Total { get; set; }
        public int? EncounterFid { get; set; }
        public int? AppointmentFid { get; set; }
        public string Status { get; set; }
        public float finalAmount { get; set; }
        public float Discount { get; set; }
        public float FinalPaybalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int Service_Fid { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }     
        public float Amount { get; set; }
        public int PatientFid { get; set; }
        public int FlageID { get; set; }
        public int PatientID { get; set; }

        public string BillFlageID { get; set; }
        public string FlagId { get; set; }

        /* for outstanding*/
        public int Id { get; set; }
        public decimal RemainingAmount { get; set; }
      
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
       public string BillStatus { get; set; }

        public DateTime AppoinmentDate { get; set; }
        public string SloteStartTime { get; set; }
        public string SloteEndTime { get; set; }
        public string ServiceDetails { get; set; }

    
        public string FirstName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
    }
}