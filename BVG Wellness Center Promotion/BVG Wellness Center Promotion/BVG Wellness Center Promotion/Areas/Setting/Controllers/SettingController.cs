using BVG_Wellness_Center_Promotion.Areas.Setting.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace BVG_Wellness_Center_Promotion.Areas.Setting.Controllers
{
    public class SettingController : Controller
    {
        // GET: Setting/Setting
        #region Setting
        public ActionResult Setting()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }

        }
        #endregion

        #region PartialView SettingSidebar
        public ActionResult SettingSidebar()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region Holidays
        public ActionResult Holidays()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }




     
        #endregion

        #region configuration
        public ActionResult Configuration()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region AppConfigurations
        public ActionResult AppConfigurations()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }

        #endregion

        #region EmailTemplate
        public ActionResult EmailTemplate()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region WatsUpTemplate
        public ActionResult SMSWatsUpTemplate()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region webhooks
        public ActionResult Webhooks()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region CreateWebhook
        public ActionResult CreateWebhook()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region custom-notification
        public ActionResult CustomNotification()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region CustomForm
        public ActionResult CustomForm()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion
        #region CustomFormCreate
        public ActionResult CustomFormCreate()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion

        #region ChangeThemsSetting
        public ActionResult ProSetting()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion


        #region Location
        public ActionResult Location()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

            DynamicParameters param = new DynamicParameters();
            var list = BVGWellnessCenterDapparORM.ReturnList<LocationS>("Sp_GetSavedLocationList", param).ToList();
            return View(list);
    
              //  return View();
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }




        [HttpGet]
        public JsonResult SearchLocation(string searchTerm)
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@searchTerm", searchTerm);
            //param.Add("@HospitalID", Session["HospitalID"]);
            var result = BVGWellnessCenterDapparORM.ReturnList<LocationS>("Sp_searchAreabyPincode", param).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult SaveLocation(int? FId, LocationS L)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@PincodeFId", FId);
                //BVGWellnessCenterDapparORM.Execute("Sp_SaveLocation", param);
                var Result = BVGWellnessCenterDapparORM.ReturnList<LocationS>("Sp_SaveLocation", param);

                return RedirectToAction("Location");
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }



        public ActionResult DeleteLocation(int FId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@FId", FId);

            var Result = BVGWellnessCenterDapparORM.ReturnList<LocationS>("Sp_DeleteSavedLocation", param);
            
            return RedirectToAction("Location");
        }
        #endregion

        #region ChangePassword

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string OldPassword, string NewPassword)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "ChangePassword");
                param.Add("@p_UserID", Session["UserID"]);
                param.Add("@p_LoginPassword", OldPassword); // Current Password match honyasathi
                param.Add("@p_UserId_Encrypted", NewPassword); // Navin Password update honyasathi

                param.Add("@p_Message", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);

                // SP Execute kara
                BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_UserMaster", param);

                // Database madhun yenara message ani icon set kara
                string icon = param.Get<string>("@p_Icon");
                TempData["Message"] = param.Get<string>("@p_Message");
                TempData["Icon"] = icon;
                // ✅ Jar password successfully change jhala asel tar Session clear kara
                if (icon == "success")
                {
                    Session.Clear();
                    Session.Abandon();
                    // User la message dakhvun logout karnyathi redirect return View karel
                }

                return View();
                // *** MAHATVACHA BADAL ITHE AAHE ***
                // Redirect naka karu, tyach view la return kara mhanje popup disel
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        #endregion



// 1. Forgot Password cha Page dakvne
[HttpGet]
    public ActionResult ForgotPassword()
    {
        return View();
    }

    // 2. Email check karun OTP pathvne
    [HttpPost]
    public ActionResult SendOTP(string Email)
    {
        // Database madhe email check kara
        DynamicParameters param = new DynamicParameters();
        param.Add("@p_Email", Email);
        param.Add("@p_ProcessType", "CheckEmail");
        // Email asel tarach pudhe ja...

        // OTP Generate kara
        string otp = new Random().Next(1000, 9999).ToString();
        Session["ResetOTP"] = otp;
        Session["ResetEmail"] = Email;

        try
        {
            // Email pathvnyacha logic
            var senderEmail = new MailAddress("apiprobus@gmail.com", "BVG Wellness Center");
            var receiverEmail = new MailAddress(Email);
            var password = "nibd kmnr fbdb pguf"; // Gmail App Password

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };

            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = "Password Reset OTP",
                Body = "Your OTP for password reset is: " + otp
            })
            {
                smtp.Send(mess);
            }

            TempData["Message"] = "OTP sent to your email!";
            return RedirectToAction("VerifyOTP");
        }
        catch (Exception)
        {
            TempData["Message"] = "Email sending failed!";
            return View("ForgotPassword");
        }
    }
}
}