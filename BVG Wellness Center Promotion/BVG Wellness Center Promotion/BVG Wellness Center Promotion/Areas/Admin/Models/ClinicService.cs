using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class ClinicService
    {
        public int ServiceFid { get; set; }
        public string ServiceId_Encrypted { get; set; }
        public int FDate { get; set; }
        public string Servicecategory { get; set; }
        public string ServiceName { get; set; }
        public int Charges { get; set; }
        public bool telemed_service { get; set; }
        public int ClinicFid { get; set; }
        public int DoctorId { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public string ServiceImageFile { get; set; }
        public string ServiceGroup { get; set; }
        public string MedicinePacking { get; set; }
        public string MedicineIndication { get; set; }
        public string Doses { get; set; }
        public string Instructions { get; set; }
        public string Group { get; set; }
        public string Frequncy { get; set; }

        public int Fid { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
        //-----------------------add patient ------------------------
        public int? PatientId { get; set; }
        public int PatientFid { get; set; }
        public int? AppointmentFid { get; set; }
        public string DoctorName { get; set; }
        public string Department { get; set; }
        // for new patient
        public string PatientName { get; set; }
        public string MobileNo { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        // --------------------- 1. Basic Details ---------------------
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public int? ServiceRoomID { get; set; }
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
        public string CommunityTypes { get; set; } // CSV of selected checkboxes
                                                   // --------------------- 3. Facility / Room ---------------------
        public string FacilityType { get; set; }
        public string IPDCategory { get; set; }
        public string RoomType { get; set; }

        // --------------------- 4. Therapy Execution ---------------------
        public int TherapistId { get; set; }
        public int NoOfDays { get; set; }
        public int? CustomNoOfDays { get; set; } // For custom days input
        public string TimeSlot { get; set; }
        //public int Duration { get; set; }
        public int? CustomDuration { get; set; } // For custom duration input
        public DateTime? FromDate { get; set; }
        public DateTime ToDate { get; set; }
        // --------------------- 5. Billing ---------------------
        public decimal TherapyValue { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal TotalAmoount { get; set; }
        // --------------------- 6. Medical Fitness & Investigations ---------------------
        public string Investigations { get; set; }  // CSV of selected investigations
        public string InvestigationsOther { get; set; } // Other investigation
        public string LMPDate { get; set; }
        public bool ConsentTaken { get; set; }
        // --------------------- 7. Room / Place ---------------------
        public string TherapyPlaces { get; set; }  // CSV of selected places
        public string TherapyPlacesOther { get; set; } // Other place
                                                       // --------------------- 8. Checklist & Procedure ---------------------
        public bool PatientPrepared { get; set; }
        public bool TherapyPrepared { get; set; }

        public int IsDouble { get; set; }
        public int IsTherapy { get; set; }
        public string PrerequisiteProcedure { get; set; }
        public string MainProcedure { get; set; }
        public string PostProcedureCare { get; set; }



        public String Quantity { get; set; }


        // --------------------- 9. Instructions & Safety ---------------------
        public string PatientInstructions { get; set; }
        public string RelativeInstructions { get; set; }
        public string TherapistInstructions { get; set; }
        public string Precautions { get; set; }
        public string ContraIndications { get; set; }
        public string RelatedDisorders { get; set; }



        public string Instruments { get; set; }




        public string MedicineName { get; set; }        // CSV from DB
        public List<string> MedicineNamesList { get; set; } = new List<string>(); // for multi-select


        public string ServiceRoomIds { get; set; }

        public List<int> SelectedRoomIds { get; set; } = new List<int>();

          public int?  ServiceGroupID {get; set;}



    }



}






