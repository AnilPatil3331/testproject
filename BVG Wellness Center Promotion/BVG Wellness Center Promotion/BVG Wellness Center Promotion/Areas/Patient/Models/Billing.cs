using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class Billing
    {

        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
        public string Frequency { get; set; }
        public int? Duration { get; set; }
        public string Instruction { get; set; }
        public int? Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }
        public string ServiceName { get; set; }
        public string ServiceFid { get; set; }
        public List<int> ServiceFids { get; set; }
        public float Price { get; set; }
        public int? Quantity { get; set; }
        public float Total { get; set; }
        public int? EncounterFid { get; set; }
        public float finalAmount { get; set; }
        public float Discount { get; set; }
        public float FinalPaybalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int? Service_Fid { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }
        public float Amount { get; set; }
        public string  FlagId { get; set; }
        public int? PatientID { get; set; }
        public int? AppointmentFid { get; set; }
      


    }
}