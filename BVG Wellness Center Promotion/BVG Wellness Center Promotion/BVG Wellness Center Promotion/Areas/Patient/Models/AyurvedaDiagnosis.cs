using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class AyurvedaDiagnosis
    {


        public int Fid { get; set; }
        public int PatientFid { get; set; }
       
        public int AppointmentFid { get; set; }
        public string StatusCheck { get; set; }
        public int AyurvedaDiagnosisID { get; set; }
        public string Fever { get; set; }

        [NotMapped]
        public List<int> DiseaseIDList { get; set; } = new List<int>();
        public string DiseaseID { get; set; }

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
        public bool isActive { get; set; }

    }
}