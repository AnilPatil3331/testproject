using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class EHR
    {

        public string DepartmentName { get; set; }
        public string UHIDNO { get; set; }
        public int PatientFid { get; set; }
        public int MUid { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public int AppointmentFid { get; set; }
        public string BloodGroup { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LName { get; set; }
        //public int Age { get; set; }
        public string Age { get; set; }
        public string ContactNumber { get; set; }
        public string AppointmentFid_Encrypted { get; set; }
        public string Status { get; set; }
        public DateTime AppoinmentDate { get; set; } = DateTime.Now;

        public string Name { get; set; }

        public int? DaysNumber { get; set; }

        public string ClinicName { get; set; }

        public string  DoctorName { get; set; }

        public int PulseExamID { get; set; }

        public List<PrescriptionModel> Prescriptions { get; set; }

            public int OPDNO { get; set; }
            public int Fid { get; set; }
            public int AyurvedaDiagnosisID { get; set; }
            public int OPDID { get; set; }
            public string Fever { get; set; }
            public string Condition { get; set; }




        public string InstructionTherapist { get; set; }
        public string InstructionPatinet { get; set; }




        public string Fevers { get; set; }
        public string CoughCold { get; set; }
        public string BreathingProblem { get; set; }
        public string ChestPain { get; set; }
        public string StomachPain { get; set; }
        public string VomitingLooseMotion { get; set; }
        public string HeadacheDizziness { get; set; }
        public string BodyPain { get; set; }
        public string UrinaryProblem { get; set; }
        public string Swelling { get; set; }


        public int ServiceGroupID { get; set; }



        public int PulseRate { get; set; }
            public int BloodPressureDia { get; set; }
            public int BloodPressureSys { get; set; }



            public bool Vat { get; set; }
            public bool Vataf { get; set; }
            public bool Vatpit { get; set; }
            public bool VarKafPit { get; set; }
            public bool VatPitKaf { get; set; }

            // Jaloka / Snake
            public bool Leech { get; set; }
            public bool Snake { get; set; }

            // Pitta
            public bool Pitta { get; set; }
            public bool PittaKaf { get; set; }
            public bool PittaVat { get; set; }
            public bool PottaKafVat { get; set; }
            public bool PittaVatKaf { get; set; }

            // Animals
            public bool Sparrow { get; set; }
            public bool Crow { get; set; }
            public bool Monkey { get; set; }
            public bool Frog { get; set; }

            // Kapha
            public bool kaf { get; set; }
            public bool KaffPitta { get; set; }
            public bool Kaffvtta { get; set; }
            public bool KafPittaVat { get; set; }
            public bool KVP { get; set; }

            // Birds
            public bool Swan { get; set; }
            public bool Pigeon { get; set; }
            public bool Peacock { get; set; }

            // Other
            public bool SaniPat { get; set; }
            public bool BustardQuail { get; set; }
            public bool GreyBird { get; set; }
            public bool ButtonQuai { get; set; }

            public string GatiMovement1 { get; set; } // First row: सरल / तिर्यक् / वक्र
            public string GatiMovement2 { get; set; } // Second row: दीर्घ / ऱ्हस्व
            public string GatiMovement3 { get; set; } // Third row: सम / विषम


            public string SamataUndigested { get; set; }   // सामताUndigested
            public string GatiMovement { get; set; }       // गतिMovement
            public string Speed { get; set; }        // वेगSpeed
            public string Rhythm { get; set; }       // लयRhythm
            public string Strength { get; set; }     // बलStrength
            public string Volume { get; set; }       // घनफलVolume
            public string Temperature { get; set; }  // तापमानTemperature





            public string UrinePrakritasama { get; set; }

            public string UrineVata { get; set; }

            public string UrinePitta { get; set; }

            public string UrineRakta { get; set; }

            public string UrineKapha { get; set; }

            public string UrineTridosha { get; set; }



            public string UrinePrakritasamaOtherText { get; set; }

            public string UrineTridoshaOtherText { get; set; }



            public string NaturalState { get; set; }
            public string NaturalStateOtherText { get; set; }


           public string FlagId { get; set; }
  
            // Kapha
            public string KaphaType { get; set; }
            public string KaphaOtherText { get; set; }

            // Pitta
            public string PittaType { get; set; }
            public string PittaOtherText { get; set; }

            // Vata-Kapha
            public string VataKaphaType { get; set; }

            // Vata-Pitta
            public string VataPittaType { get; set; }

            // Kapha-Pitta
            public string KaphaPittaType { get; set; }

            // Tridosha
            public string TridoshaType { get; set; }



            public bool Krishna { get; set; }
            public bool Drudha { get; set; }
            public bool Shushka { get; set; }
            public bool Trutita { get; set; }
            public bool Phena { get; set; }
            public bool Ruksha { get; set; }
            public bool Dhumra { get; set; }
            public bool Pravahana { get; set; }
            public bool Adhmana { get; set; }




            public string NaturalStateTongue { get; set; }
            public string NaturalStateOther { get; set; }

            // Ruksha-Snigdha
            public string RukshaSnigdha { get; set; }

            // Sama / Nirama / Alpasama
            public string SamaNiramaAlpasama { get; set; }

            // Vata
            public string VataCharacteristics { get; set; }

            // Pitta
            public string PittaCharacteristics { get; set; }
            public string PittaOther { get; set; }

            // Kapha
            public string KaphaCharacteristics { get; set; }

            // Tridosha
            public string TridoshaCharacteristics { get; set; }

            public string NaturalStateVoiceExam { get; set; }     // प्राकृतसमS
            public string KaphaVoiceExam { get; set; }                       // कफK
            public string PittaVoiceExam { get; set; }            // पित्तP
            public string VataVoiceExam { get; set; }                // वातV


            public string EyeNaturalState { get; set; }   // प्राकृतसम
            public string EyeVata { get; set; }           // वात
            public string EyePitta { get; set; }          // पित्त
            public string EyeKapha { get; set; }          // कफ

            // Appearance Exam
            public string AppearanceNaturalState { get; set; } // प्राकृतसम
            public string AppearanceVata { get; set; }         // वात
            public string AppearancePitta { get; set; }        // पित्त
            public string AppearanceKapha { get; set; }


            public string NaturalStateTouch { get; set; }   // प्राकृतसम
            public string PittaTouch { get; set; }          // पित्त
            public string VataTouch { get; set; }           // वात
            public string KaphaTouch { get; set; }          // कफ


            // Natural State
            public string StoolNaturalState { get; set; }
            public string StoolNaturalOtherText { get; set; }

            // Vata Checkboxes
            public bool StoolVataKrishna { get; set; }
            public bool StoolVataDrudha { get; set; }
            public bool StoolVataShushka { get; set; }
            public bool StoolVataTrutita { get; set; }
            public bool StoolVataPhena { get; set; }
            public bool StoolVataRuksha { get; set; }
            public bool StoolVataDhumra { get; set; }
            public bool StoolVataPravahana { get; set; }
            public bool StoolVataAdhmana { get; set; }
            public bool StoolVataUdarshul { get; set; }
            public bool StoolVataKatishul { get; set; }

            // Kapha
            public string StoolKapha { get; set; }
            public string StoolKaphaOtherText { get; set; }

            // Pitta
            public string StoolPitta { get; set; }
            public string StoolPittaOtherText { get; set; }

            // Vata-Kapha
            public string StoolVataKapha { get; set; }

            // Vata-Pitta
            public string StoolVataPitta { get; set; }

            // Kapha-Pitta
            public string StoolKaphaPitta { get; set; }

            // Tridosha
            public string StoolTridosha { get; set; }

            public bool isActive { get; set; }
            public string ItemName { get; set; }
            public string ItemCode { get; set; }
            public string GenericName { get; set; }

        public string Relation { get; set; }


        // RESULT PAGE DATA 
        public string StatusCheck { get; set; }

            public DateTime MedicineDate { get; set; }

            // Row 1
            public bool Sams { get; set; }

            // Row 2
            public bool VatavFunction { get; set; }
            public bool VatakafFunction { get; set; }
            public bool VatpittaFunction { get; set; }
            public bool VatakafpittaFunction { get; set; }
            public bool VatapittakafFunction { get; set; }

            // Row 3
            public bool PittaFunction { get; set; }
            public bool PittaKafFunction { get; set; }
            public bool PittaVatFunction { get; set; }
            public bool PittaKafVatFunction { get; set; }
            public bool PittaVatKafFunction { get; set; }

            // Row 4
            public bool KafFunction { get; set; }
            public bool KafPittaFunction { get; set; }
            public bool KafVatFunction { get; set; }
            public bool KafPittaVatFunction { get; set; }
            public bool KafVatPittaFunction { get; set; }






            public bool JatharagniAcids { get; set; }
            public bool DhatvagniEnzymes { get; set; }
            public bool MahabhutagniHormones { get; set; }

            public bool Vatavishamagni { get; set; }
            public bool PittaTikshnagni { get; set; }
            public bool KafMandagni { get; set; }

            public bool Samagni { get; set; }


            public bool RasaSentiment { get; set; }
            public bool RaktaBlood { get; set; }
            public bool MansaMuscles { get; set; }
            public bool MedaFat { get; set; }
            public bool AsthiBones { get; set; }
            public bool MajjaNerves { get; set; }
            public bool ShukraReproductive { get; set; }



            public bool StanyaBreastMilk { get; set; }
            public bool RajaMenstrualBlood { get; set; }
            public bool SnayuLigaments { get; set; }
            public bool TwakSkin { get; set; }
            public bool Danta { get; set; }
            public bool Oja { get; set; }



            public bool KaphaUpmala { get; set; }
            public bool PittaUpmala { get; set; }
            public bool RomaHairsUpmala { get; set; }
            public bool NakhaNails { get; set; }
            public bool KarnaMal { get; set; }
            public bool NetraVit { get; set; }
            public bool MukhikMal { get; set; }
            public bool RomKupMal { get; set; }
            public bool PrajananAngMal { get; set; }
            public bool CharmSnehMal { get; set; }


            public string Koshtha { get; set; }



            public bool Purisha { get; set; }   // Stool
            public bool Mutra { get; set; }     // Urine
            public bool Sweda { get; set; }     // Sweat





            //public class PrescriptionModel


            public int? PrescriptionID { get; set; }   // PK
            public int FormID { get; set; }
            public int FormulaID { get; set; }
            public int PackageID { get; set; }
            public int QuantityID { get; set; }
            public string FormName { get; set; }
            public string PackageName { get; set; }
            public string QuantityName { get; set; }
            public string FormulaName { get; set; }
            public string TimeName { get; set; }
            public int TimeID { get; set; }
            public int NumberID { get; set; }
            public int Id { get; set; }
            public int NumberValue { get; set; }
            public string  NumberQuantity { get; set; }
            public int? FluidVehicleID { get; set; }
            public int? VehicleID { get; set; }
            public string VehicleName { get; set; }

            public int QuantityIDDose { get; set; }
            /*  public int Id { get; set; }   */    // MAS_NumberList चं PK



            public int? FluidNumberID { get; set; }
            public int? FluidQuantityID { get; set; }


            public int ProcessingUnit { get; set; }
            public int ProcessingQty { get; set; }
        public int ProcessingNumber { get; set; }

        public int NumberLeftID { get; set; }
            public int NumberRightID { get; set; }
            public int DoseNumberID { get; set; }


            // For dropdown binding (optional if you use ViewBag)
            public IEnumerable<SelectListItem> Forms { get; set; }
            public IEnumerable<SelectListItem> Formulas { get; set; }
            public IEnumerable<SelectListItem> Packages { get; set; }
            public IEnumerable<SelectListItem> Quantities { get; set; }

            public int SuchnaID { get; set; }
            public string Suchna { get; set; }
            public string SuchnaName { get; set; }
            public string SuchnaText { get; set; }
            public string InstructionName { get; set; }
            public string Description { get; set; }


            public int NightSuchnaID { get; set; }
            public int EveningAfterMealSuchnaID { get; set; }
            public int EveningSuchnaID { get; set; }
            public int MorningAfterMealSuchnaID { get; set; }
            public int MorningSuchnaID { get; set; }





            public string txtSuchana { get; set; }
            public string txtMorningSuchana { get; set; }
            public string txtMorningAfterMealSuchana { get; set; }
            public string txtEveningSuchana { get; set; }

            public string txtEveningAfterMealSuchana { get; set; }
            public string txtNightSuchana { get; set; }


            public string Suchnapahate { get; set; }
     
        public string MorningSuchana { get; set; }












        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Instruction { get; set; }
        public int Bill_Fid { get; set; }
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
        public int Service_Fid { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }
        public float Amount { get; set; }




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
        public decimal DurationC { get; set; }
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

        //public int? ChronicIllnessId { get; set; }
        //public int? PastSurgeryId { get; set; }
        //public int? TraumaHistoryId { get; set; }
        //public int? AllergyId { get; set; }
        //public int? LongTermMedicationId { get; set; }
        //public int? ChronicIllnessDuration { get; set; }
        //public int? ChronicIllnessDurationUnit { get; set; }
        //public int? PastSurgeryDuration { get; set; }
        //public int? PastSurgeryDurationUnit { get; set; }
        //public int? TraumaHistoryDuration { get; set; }
        //public int? TraumaHistoryDurationUnit { get; set; }
        //public int? AllergyHistoryDuration { get; set; }
        //public int? AllergyHistoryDurationUnit { get; set; }
        //public int? LongTermMedicationDuration { get; set; }
        //public int? LongTermMedicationDurationUnit { get; set; }

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
        public List<ComplaintVM> Complaints { get; set; } = new List<ComplaintVM>();
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








        public int ManualPatientId { get; set; }

        // ✅ USER REPORTS LIST (ADD THIS)
        public List<UserReportModel> UserReports { get; set; }

        // Yoga Diagnosis Properties
        public string DescriptionUnAware { get; set; }
        public string DescriptionEgoism { get; set; }
        public string DescriptionDesire { get; set; }
        public string DescriptionHatred { get; set; }
        public string Fearofdeath { get; set; }

        // Homeopathy Diagnosis Properties
        public string DescriptionCheerful { get; set; }
        public string DescriptionAffected { get; set; }
    }



    public class MedicineDispensingModel
    {
        public int AppointmentFid { get; set; }
        public int PatientFid { get; set; }
        public int? PrescriptionID { get; set; }
        public List<MedicineInstructionModel> MedicineInstructions { get; set; }
        public List<int> GlobalPathyaIds { get; set; }
    }

    public class MedicineInstructionModel
    {
        public string Description { get; set; }
        public string MorningSuchana { get; set; }
        public int? PrescriptionID { get; set; }
        public int? PathyaId { get; set; }
        public string PathyaInstruction { get; set; } // हे आधीच असेल

    }

    public class ComplaintVM
    {
        public int? ComplaintId { get; set; }
        public decimal? DurationC { get; set; }
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
    public class UserReportModel
    {
        public int Fid { get; set; }
        public int UserId { get; set; }
        public string ReportType { get; set; }
        public string ReportFile1 { get; set; }
        public string ReportFile2 { get; set; }
        public string ReportFile3 { get; set; }
        public string ReportFile4 { get; set; }
        public string ReportFile5 { get; set; }
        public string ReportTitle { get; set; }
        public DateTime Date { get; set; }
        public int Deactivate { get; set; }
    }
    public class PatientMobileReportModel
    {
        public int Fid { get; set; }
        public int UserId { get; set; }
        public string ReportType { get; set; }
        public string ReportFile1 { get; set; }
        public string ReportFile2 { get; set; }
        public string ReportFile3 { get; set; }
        public string ReportFile4 { get; set; }
        public string ReportFile5 { get; set; }
        public string ReportTitle { get; set; }
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public bool Deactivate { get; set; }
    }
}
