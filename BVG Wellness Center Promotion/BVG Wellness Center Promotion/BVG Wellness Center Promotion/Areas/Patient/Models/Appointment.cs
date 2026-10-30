using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class Appointment
    {
        public int AppointmentFid { get; set; }
        public string AppointmentFid_Encrypted { get; set; }

        public int VisitFid { get; set; }
        public string VisitFid_Encrypted { get; set; }

        public int ClinicFid { get; set; }
        public int DoctorFid { get; set; }
         //public List<int> ServiceFid { get; set; }   
        public string ServiceFidString { get; set; }       
        public DateTime AppoinmentDate { get; set; }= DateTime.Now;
        public int? PatientFid { get; set; }
        public string Status { get; set; }
        public string ReportFile { get; set; }
        //public HttpPostedFileBase ReportFile { get; set; }
        public string SloteStartTime { get; set; }
        public string SloteEndTime { get; set; }
        public string ServiceDetails { get; set; }
        public double TaxDetails { get; set; }
        public int Weight { get; set; }
        public string Palse { get; set; }
        public string BloodPressure { get; set; }
        public string SpO2 { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string FDate { get; set; }   // SQL मधून हे येईल

        public string UHIDNO { get; set; }

        //public int ServiceFids { get; set; }

        //public string ServiceFids { get; set; }
        public string ServiceFid { get; set; }





        // billing  model
        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Instruction { get; set; }
        public int Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }
        public string ServiceName { get; set; }
       // public string ServiceFid { get; set; }
        public List<int> ServiceFids { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public float Total { get; set; }
        public int? EncounterFid { get; set; }
        public float finalAmount { get; set; }
        public float Discount { get; set; }
        public float FinalPaybalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int Service_Fid { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }
        public float Amount { get; set; }


        public int TotalSlot { get; set; }      // एकूण क्षमता (उदा. 3)
        public int BookedSlot { get; set; }     // किती बुक झाले (उदा. 1)
        public int AvailableSlot { get; set; }
        public string BookingSource { get; set; }

    }
}