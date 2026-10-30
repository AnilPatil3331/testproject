using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Patient.Models
{
    public class Medicine
    {

        public int? MedicineId { get; set; }


        public string ItemName { get; set; }
            public string GenericName { get; set; }
            public string Frequency { get; set; }
            public int Count { get; set; }
            public string Duration { get; set; }
            public string Instruction { get; set; }
            public string Dosage { get; set; }
            public string Route { get; set; }
            public int Quantity { get; set; }
            public string Note { get; set; }





        public int Fid { get; set; }
        public string Fid_Encrypted { get; set; }
        public string MedicineName { get; set; }
    
        public int Bill_Fid { get; set; }
        public string Bill_Fid_Encrapted { get; set; }
        public string ServiceName { get; set; }
        public int ServiceFid { get; set; }
        public float Price { get; set; }
        //public int Quantity { get; set; }
        public float Total { get; set; }
        public int? EncounterFid { get; set; }
        public int? AppointmentFid { get; set; }
        public string Status { get; set; }
        public float finalAmount { get; set; }
        public float Discount { get; set; }
        public float FinalPaybalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int Service_Fid { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNumber { get; set; }
        public float Amount { get; set; }
        public int PatientFid { get; set; }
        public int FlageID { get; set; }
        public int PatientID { get; set; }
   
        public DateTime? DateOfBirth { get; set; }

        /* for outstanding*/

        public decimal RemainingAmount { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public string BillStatus { get; set; }

        public DateTime AppoinmentDate { get; set; }
     
        public string ServiceDetails { get; set; }


        public string FlageId { get; set; }
        public int Rate { get; set; }
        public int DoctorFid { get; set; }


        //public int ServiceFid { get; set; }
        //public string ServiceName { get; set; }
        //public decimal Rate { get; set; }
        //public int Quantity { get; set; }
        //public decimal Discount { get; set; }
        //public decimal Amount { get; set; }
        //public string Note { get; set; }
        //public int DoctorFid { get; set; }
        //public int PatientID { get; set; }

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
     
        public bool IsConfirm { get; set; }

        public int? DaysNumber { get; set; }

    }


    public class ManualPatientVM
    {
        public int? PatientFid { get; set; }   // 🔥 int → int?

        public string PatientName { get; set; }
        public string MobileNo { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public DateTime? VisitDate { get; set; }

        // OPTIONAL fields (later use)
        public DateTime? DateOfBirth { get; set; }
        public string ClinicName { get; set; }
        public string UHIDNO { get; set; }
    }



    public class PrescriptionServiceVM
    {
        public int ServiceFid { get; set; }
        public string ServiceName { get; set; }

        public decimal? Rate { get; set; }      // ✅ nullable
        public int? Quantity { get; set; }      // ✅ nullable
        public decimal? Discount { get; set; }  // ✅ nullable
        public decimal? Amount { get; set; }    // ✅ nullable

        public string Note { get; set; }
        public int? DoctorFid { get; set; }

        public int? PatientID { get; set; }
        public int? PatientFid { get; set; }




        // Add these for manual patient display
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public DateTime? VisitedDate { get; set; }
    }



    public class MedicineVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MedicinePacking { get; set; }
        public decimal Charges { get; set; }
    }

    public class MedicinePrescriptionVM
    {
        public int Id { get; set; }
        public int? MedicineFid { get; set; }
        public string MedicineName { get; set; }
        public string Packing { get; set; }
        public decimal Rate { get; set; }
        public int Quantity { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Route { get; set; }
        public string Duration { get; set; }
        public string Instruction { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public int? DoctorFid { get; set; }
        public string PatientID { get; set; }
        public int? PatientFid { get; set; }


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

        public bool IsConfirm { get; set; }

        public int? DaysNumber { get; set; }

        public int? ManualPatientId { get; set; }
    }

    public class SearchPatientVM
    {
        public int PatientFid { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string UHIDNO { get; set; }
    }

    public class SaveMedicineRequestVM
    {
        public List<MedicinePrescriptionVM> Medicines { get; set; }

        public int ManualPatientId { get; set; }
        public string PatientName { get; set; }
        public string MobileNo { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public DateTime? VisitDate { get; set; }
        public string FlageId { get; set; }
        public int? PatientFid { get; set; }
    }

}