using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Patient
{
    public class PatientAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Patient";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            // 👇 GetReport explicit route — dataTokens sathi
            context.MapRoute(
                name: "Patient_GetReport",
                url: "Patient/AayurvedEHR/GetReport",
                defaults: new
                {
                    controller = "AayurvedEHR",
                    action = "GetReport"
                },
                namespaces: new[] {
            "BVG_Wellness_Center_Promotion.Areas.Patient.Controllers"
                }
            );

            context.MapRoute(
                "Patient_default",
                "Patient/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}