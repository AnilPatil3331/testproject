using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class CasePaperModel
    {


        // case paper model 
        public int? AppointmentFid { get; set; }
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

        //public int? DurationC { get; set; }
        public decimal? DurationC { get; set; }

        public int? DurationUnitId { get; set; }
        public string Onset { get; set; }
        public int? NatureOfIllnessId { get; set; }
        public int? ProgressionId { get; set; }
        public string[] AssociatedSymptomsId { get; set; }
        public string[] AggravatingFactorsId { get; set; }
        public string[] RelievingFactorsId { get; set; }
        public int? PreviousTreatment { get; set; }
        public int? ResponsePreviousTreatmentId { get; set; }


        // past history 


        public List<ChronicIllnessModel> ChronicIllnessList { get; set; }
        public List<PastSurgeryModel> PastSurgeryList { get; set; }
        public List<TraumaHistoryModel> TraumaHistoryList { get; set; }
        public List<AllergyModel> AllergyList { get; set; }
        public List<LongTermMedicationModel> LongTermMedicationList { get; set; }



        public class ChronicIllnessModel
        {
            public int? ChronicIllnessId { get; set; }
            public int? Duration { get; set; }
            public int? DurationUnit { get; set; }
        }
        public class PastSurgeryModel
        {
            public int? PastSurgeryId { get; set; }
            public int? PastSurgeryDuration { get; set; }
            public int? PastSurgeryDurationUnit { get; set; }
        }
        public class TraumaHistoryModel
        {
            public int? TraumaHistoryId { get; set; }
            public int? TraumaHistoryDuration { get; set; }
            public int? TraumaHistoryDurationUnit { get; set; }
        }
        public class AllergyModel
        {
            public int? AllergyId { get; set; }
            public int? AllergyHistoryDuration { get; set; }
            public int? AllergyHistoryDurationUnit { get; set; }
        }
        public class LongTermMedicationModel
        {
            public int? LongTermMedicationId { get; set; }
            public int? LongTermMedicationDuration { get; set; }
            public int? LongTermMedicationDurationUnit { get; set; }
        }
     


    
   
     
    
      

        //public List<int> FamilyHistoryIds { get; set; }



        //public string FamilyHistoryIds { get; set; }
        //public string LabTestsAdvisedIds { get; set; }
        //public string ImagingAdvisedIds { get; set; }




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
        public bool CycleRegular_Yes { get; set; }
        public bool CycleRegular_No { get; set; }
        public string CycleRegular { get; set; } // Yes / No
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
        public List<ComplaintVM> Complaints { get; set; }
        public List<int> FamilyHistoryIds { get; set; }
        public List<int> LabTestsAdvisedIds { get; set; }
        public List<int> ImagingAdvisedIds { get; set; }


        // File Upload
        public HttpPostedFileBase PreviousReports { get; set; }

        public class ComplaintVM
        {
            public int? ComplaintId { get; set; }
            public decimal? DurationC { get; set; }

            //public int? DurationC { get; set; }
            public int? DurationUnitId { get; set; }
            public string Onset { get; set; }
            public int? NatureOfIllnessId { get; set; }
            public int? ProgressionId { get; set; }
            public string[] AssociatedSymptomsId { get; set; }
            public string[] AggravatingFactorsId { get; set; }
            public string[] RelievingFactorsId { get; set; }
            public int? PreviousTreatment { get; set; }
            public int? ResponsePreviousTreatmentId { get; set; }
        }
    


    }
}
