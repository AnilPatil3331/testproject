using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class PlusExam
    {

        public int Fid { get; set; }
        public int PatientFid { get; set; }
        public string Condition { get; set; }
        public int AppointmentFid { get; set; }
        public int PulseRate { get; set; }
        public int BloodPressureDia { get; set; }
        public int BloodPressureSys { get; set; }
        public decimal? Weight { get; set; }



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



    }
}