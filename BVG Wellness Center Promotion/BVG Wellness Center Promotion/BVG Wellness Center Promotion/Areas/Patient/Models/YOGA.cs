using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class YOGA
    {
        // User details आणि Reports binding साठी
        public int MUid { get; set; }
        public List<UserReportModel> UserReports { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string PatientName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime AppoinmentDate { get; set; } = DateTime.Now;
        public string DoctorName { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
      public string AppointmentFid_Encrypted { get; set; }
        public string ContactNumber { get; set; }
        public string UHIDNO { get; set; }


        public string DescriptionUnAware { get; set; }

      
        public string DescriptionEgoism { get; set; }

        public string DescriptionDesire { get; set; }

        public string DescriptionHatred { get; set; }

        public string Fearofdeath { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int ExamID { get; set; }
        public int AppointmentFid { get; set; }
        public int PatientFid { get; set; }
        public int YogaDiagnosisID { get; set; }
        public int? DaysNumber { get; set; }

        public string ServiceFid { get; set; }
        public string ServiceName { get; set; }
        public string Duration { get; set; }
        public string DurationUnit { get; set; }
        public string Repetition { get; set; }
        public string RepetitionUnit { get; set; }

        // case paper model 

        public string TempUnitCP { get; set; }
        public int? TemperatureCasepaper { get; set; }
        public decimal? Height { get; set; }     // cm
        public decimal? Weight { get; set; }     // kg
        public decimal? BMI { get; set; }
        public int? Pulse { get; set; }               // bpm
        public string BloodPressure { get; set; }     // Sys/Dia
        public decimal? Temperaturee { get; set; }     // °F or °C
        public int? RespiratoryRate { get; set; }     // /min
        public int? SpO2 { get; set; }

        public int ComplaintId { get; set; }
        public int? DurationC { get; set; }
        public int? DurationUnitId { get; set; }
        public string Onset { get; set; }
        public int? NatureOfIllnessId { get; set; }
        public int? ProgressionId { get; set; }
        public string[] AssociatedSymptomsId { get; set; }
        public string[] AggravatingFactorsId { get; set; }
        public string[] RelievingFactorsId { get; set; }
        public int? PreviousTreatment { get; set; }
        public int? ResponsePreviousTreatmentId { get; set; }
        public string Relation { get; set; }

        // past history 
        public int? ChronicIllnessId { get; set; }
        public int? PastSurgeryId { get; set; }
        public int? TraumaHistoryId { get; set; }
        public int? AllergyId { get; set; }
        public int? LongTermMedicationId { get; set; }
        public int? ChronicIllnessDuration { get; set; }
        public int? ChronicIllnessDurationUnit { get; set; }
        public int? PastSurgeryDuration { get; set; }
        public int? PastSurgeryDurationUnit { get; set; }
        public int? TraumaHistoryDuration { get; set; }
        public int? TraumaHistoryDurationUnit { get; set; }
        public int? AllergyHistoryDuration { get; set; }
        public int? AllergyHistoryDurationUnit { get; set; }
        public int? LongTermMedicationDuration { get; set; }
        public int? LongTermMedicationDurationUnit { get; set; }

        //public List<int> FamilyHistoryIds { get; set; }

        public string InstructionTherapist { get; set; }
        public string InstructionPatinet { get; set; }

        //public string FamilyHistoryIds { get; set; }
        //public string LabTestsAdvisedIds { get; set; }
        //public string ImagingAdvisedIds { get; set; }
        // past history 
        public List<ChronicIllnessModel> ChronicIllnessList { get; set; }
        public List<PastSurgeryModel> PastSurgeryList { get; set; }
        public List<TraumaHistoryModel> TraumaHistoryList { get; set; }
        public List<AllergyModel> AllergyList { get; set; }
        public List<LongTermMedicationModel> LongTermMedicationList { get; set; }

        public class ChronicIllnessModel
        {
            public int? Id { get; set; }  // ← Add this for update/delete
            public int? ChronicIllnessId { get; set; }
            public decimal? Duration { get; set; }
            public int? DurationUnit { get; set; }
        }

        public class PastSurgeryModel
        {
            public int? Id { get; set; }  // ← Add this
            public int? PastSurgeryId { get; set; }
            public decimal? PastSurgeryDuration { get; set; }
            public int? PastSurgeryDurationUnit { get; set; }
        }

        public class TraumaHistoryModel
        {
            public int? Id { get; set; }  // ← Add this
            public int? TraumaHistoryId { get; set; }
            public decimal? TraumaHistoryDuration { get; set; }
            public int? TraumaHistoryDurationUnit { get; set; }
        }

        public class AllergyModel
        {
            public int? Id { get; set; }  // ← Add this
            public int? AllergyId { get; set; }
            public decimal? AllergyHistoryDuration { get; set; }
            public int? AllergyHistoryDurationUnit { get; set; }
        }

        public class LongTermMedicationModel
        {
            public int? Id { get; set; }  // ← Add this
            public int? LongTermMedicationId { get; set; }
            public decimal? LongTermMedicationDuration { get; set; }
            public int? LongTermMedicationDurationUnit { get; set; }
        }

        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
        public string Frequency { get; set; }
      
        public string Instruction { get; set; }
        public int Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }
      
        public List<int> ServiceFids { get; set; }
        public float Price { get; set; }
        public int? Quantity { get; set; }
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


        /* =======================
           7. PERSONAL HISTORY
           ======================= */

        // Diet
        //public bool Diet_Veg { get; set; }
        //public bool Diet_Mixed { get; set; }
        //public bool Diet_NonVeg { get; set; }

        //// Appetite
        //public bool Appetite_Good { get; set; }
        //public bool Appetite_Poor { get; set; }
        //public bool Appetite_Variable { get; set; }

        //// Bowel Habit
        //public bool Bowel_Regular { get; set; }
        //public bool Bowel_Constipated { get; set; }
        //public bool Bowel_Loose { get; set; }

        //// Urination
        //public bool Urination_Normal { get; set; }
        //public bool Urination_Burning { get; set; }
        //public bool Urination_Frequent { get; set; }

        //// Sleep
        //public bool Sleep_Sound { get; set; }
        //public bool Sleep_Disturbed { get; set; }
        //public bool Sleep_Insomnia { get; set; }

        //// Addiction
        //public bool Addiction_Tea { get; set; }
        //public bool Addiction_Coffee { get; set; }
        //public bool Addiction_Tobacco { get; set; }
        //public bool Addiction_Alcohol { get; set; }
        //public bool Addiction_None { get; set; }

        //// Exercise
        //public bool Exercise_Yes { get; set; }
        //public bool Exercise_No { get; set; }
        // end pending 

        public int? MenarcheAge { get; set; }
        public DateTime? LMP { get; set; }
        public string CycleRegular { get; set; }
        public bool CycleRegular_Yes { get; set; }
        public bool CycleRegular_No { get; set; }
        public string MenstrualPain { get; set; } // No / Mild / Moderate / Severe
        public string ObstetricHistory { get; set; }
        public string AmountOfFlow { get; set; }
        public int? MENSTRUALDurationUnit { get; set; }
        public int? MENSTRUALDurationC { get; set; }


        //public string LabTestsAdvised { get; set; }
        //public string ImagingAdvised { get; set; }
        //public List<int> LabTestsAdvisedIds { get; set; }
        //public List<int> ImagingAdvisedIds { get; set; }
        public DateTime? FollowUpDate { get; set; }
        // Expected Outcome (Radio)
        public string ExpectedOutcome { get; set; }

        // Red Flag Advice (Checkboxes)
        public bool RF_FeverIncrease { get; set; }
        public bool RF_SeverePain { get; set; }
        public bool RF_SuddenWorsening { get; set; }
        public bool RF_VomitingBleeding { get; set; }
        public bool RF_DizzinessFainting { get; set; }
        public bool RF_NoImprovement { get; set; }

        public string RF_Other { get; set; }
        public List<ComplaintVMYOGA> Complaints { get; set; } = new List<ComplaintVMYOGA>();
        //public List<int> FamilyHistoryIds { get; set; } = new List<int>();
        //public List<int> LabTestsAdvisedIds { get; set; } = new List<int>();
        //public List<int> ImagingAdvisedIds { get; set; } = new List<int>();
        public string FamilyHistoryIds { get; set; }
        public string LabTestsAdvisedIds { get; set; }
        public string ImagingAdvisedIds { get; set; }


        // File Upload
        public HttpPostedFileBase PreviousReports { get; set; }

        [NotMapped]
        public List<int> FamilyHistoryIdsList { get; set; } = new List<int>();
        [NotMapped]
        public List<int> LabTestsAdvisedIdsList { get; set; } = new List<int>();
        [NotMapped]
        public List<int> ImagingAdvisedIdsList { get; set; } = new List<int>();

        public List<FamilyHistoryModel> FamilyHistoryList { get; set; }

        public class FamilyHistoryModel
        {
            public int? Id { get; set; }
            public int? FamilyHistoryId { get; set; }
            public string Relation { get; set; }
            public List<int> FamilyHistoryIds { get; set; }
        }



        [NotMapped]
        public List<int> DiseaseIDList { get; set; } = new List<int>();
        public string DiseaseID { get; set; }

        public int CasePaperId { get; set; }
        //public CasePaperModel CasePaper { get; set; }



    }

    public class ComplaintVMYOGA
    {
        public int? ComplaintId { get; set; }
        public int? DurationC { get; set; }
        public int? DurationUnitId { get; set; }
        public string Onset { get; set; }
        public int? NatureOfIllnessId { get; set; }
        public int? ProgressionId { get; set; }
        public string[] AssociatedSymptomsId { get; set; }
        public string[] AggravatingFactorsId { get; set; }
        public string[] RelievingFactorsId { get; set; }
        public int? PreviousTreatment { get; set; }
        public string PreviousTreatmentOther { get; set; }
        public int? ResponsePreviousTreatmentId { get; set; }
    }
}
