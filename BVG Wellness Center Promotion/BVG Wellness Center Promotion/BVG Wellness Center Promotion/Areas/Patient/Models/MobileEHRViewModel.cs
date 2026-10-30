using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class MobileEHRViewModel
    {
        public int AppointmentFid { get; set; }
        public int PatientFid { get; set; }
        public string PatientName { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string DoctorName { get; set; }
        public string AppoinmentDate { get; set; }
        public string NextAppointmentDate { get; set; }

        // Lists for Prescriptions and Bills
        public List<dynamic> PrescriptionList { get; set; }
        public List<dynamic> HomeopathyPrescriptionList { get; set; }
        public List<dynamic> CounterResultView { get; set; }
        public List<dynamic> AdvisedTherapyResult { get; set; }
        public List<dynamic> GenerateBillList { get; set; }
        public decimal TotalBillAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal DiscountAmt { get; set; }
    }
}