using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class PrescriptionModel
    {

        public int? PrescriptionID { get; set; }   // PK
        public int? FormID { get; set; }
        public int? FormulaID { get; set; }                   
        public int? PackageID { get; set; }
        public int? QuantityID { get; set; }
        public string FormName { get; set; }
        public string PackageName { get; set; }
        public string QuantityName { get; set; }
        public string FormulaName { get; set; }
        public string TimeName { get; set; }
        public int? TimeID { get; set; }
        public int? NumberID { get; set; }
        public int? Id { get; set; }
        public int? NumberValue { get; set; }
        public int? DoseID { get; set; }
        public int? FluidVehicleID { get; set; }
        public int? VehicleID { get; set; }
        public string VehicleName { get; set; }

        public string MixingDataJson { get; set; }

        public string MixingDataFluid { get; set; }

        public int? OPDNO { get; set; }

        public string NumberLeft { get; set; }
        public string DoseNumber { get; set; }
        public string Dose { get; set; }
        public string FluidVehicle { get; set; }
        public string MixingDetails { get; set; }
        public string MixingDetailsFluid { get; set; }
        /*  public int Id { get; set; }   */    // MAS_NumberList चं PK

        public int? ProcessingUnit { get; set; }
        public int? ProcessingQty { get; set; }

        public string PackageResult { get; set; }


        public string FormResult { get; set; }


        public string TimeResult { get; set; }
        public string Time { get; set; }

        public DateTime MedicineDate { get; set; }
        public string ProcessingNumber { get; set; }
        public string ProcessingUnitType { get; set; }


        public int? NumberLeftID { get; set; }
        public int? NumberRightID { get; set; }
        public int? DoseNumberID { get; set; }
        public int? QuantityIDDose { get; set; }

        public int? FluidNumberID { get; set; }
        public int? FluidQuantityID { get; set; }
        public string MorningSuchana { get; set; }

        // For dropdown binding (optional if you use ViewBag)
        public IEnumerable<SelectListItem> Forms { get; set; }
        public IEnumerable<SelectListItem> Formulas { get; set; }
        public IEnumerable<SelectListItem> Packages { get; set; }
        public IEnumerable<SelectListItem> Quantities { get; set; }
        public bool IsConfirm { get; set; }

        public int? DaysNumber { get; set; }
        public int? IsActive { get; set; }
    }

    //public class PrescriptionModel1
    //{
    //    [JsonProperty("FormName")]
    //    public string FormID { get; set; }

    //    [JsonProperty("PackageName")]
    //    public string PackageID { get; set; }

    //    [JsonProperty("NumberLeft")]
    //    public string NumberLeftID { get; set; }

    //    [JsonProperty("MixingDetails")]
    //    public string FormulaID { get; set; }

    //    [JsonProperty("DoseNumber")]
    //    public string DoseNumberID { get; set; }

    //    [JsonProperty("Dose")]
    //    public string QuantityIDDose { get; set; }

    //    [JsonProperty("TimeName")]
    //    public string TimeID { get; set; }

    //    [JsonProperty("FluidVehicle")]
    //    public string FluidVehicleID { get; set; }
    //}


    public class MixingDetail
    {
        public string NumberRightValue { get; set; }
        public string QuantityName { get; set; }
        public string FormulaName { get; set; }
        public int? NumberRightID { get; set; }
        public int? QuantityID { get; set; }
        public int? FormulaID { get; set; }

        public int? FluidNumberID { get; set; }
        public int? FluidQuantityID { get; set; }
        public int? FluidVehicleID { get; set; }

        public string VehicleName { get; set; }
    }

    public class AdvisedTherapyVM
    {
        public int ATId { get; set; }
        public int ServiceFid { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategories { get; set; }
        public string DaysNumber { get; set; }
        public string Frequency { get; set; }
        public string InstructionPatinet { get; set; }
        public string InstructionTherapist { get; set; }
        public string Consent { get; set; }
    }


    public class AdvisedTherapySubmitVM
    {
        public int AppointmentFid { get; set; }
        public int PatientFid { get; set; }
        public string AdvisedTherapyJson { get; set; }
    }

}