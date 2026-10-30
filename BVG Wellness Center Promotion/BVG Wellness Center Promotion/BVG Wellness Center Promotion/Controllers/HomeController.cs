using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Data;

namespace BVG_Wellness_Center_Promotion.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        SqlConnection sqlcons = new SqlConnection(BVGWellnessCenterDapparORM.connectionStrings);

        public ActionResult Login()
        {
            try
            {
               
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }

        [HttpGet]
        public ActionResult Logout()
        {
            // सेशन पूर्णपणे नष्ट करा
            Session.Clear();
            Session.Abandon();

            // ब्राउझरला पेज सेव्ह न करण्यास सांगा
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoServerCaching();

            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public ActionResult Login(string UserID, string password , string CliniCode)
        {
            dynamic GetConnectionString = null;
            try
            {
                //Set Connection For use Dapper Class
                //   BVGWellnessCenterDapparORM.SetConnection();
                if (UserID==null)
                {
                    TempData["Message"] = "Before connectionstring";
                    return RedirectToAction("Login", "Home", new { area = "" });

                }
                var Con_Server = "";
                var Con_UserId = "";
                var Con_Password = "";
                var Con_Database = "";
                var Con_hospitalCode = "";

               

                try
                {
                    // query = @"Select Con_Server, Con_UserId, Con_Password, 
                    //         Con_Database, Con_hospitalCode 
                    //  from HospitalRegistration 
                    //  where Con_hospitalCode = @CliniCode 
                    //  and Deactivate = 0 
                    //  and Isactive = 1";

                    //// The issue: sqlcons needs to be initialized with correct connection string
                    //// Make sure connectionStrings is set correctly
                    //var GetConnectionString = sqlcons.Query(query,
                    //    new { CliniCode = CliniCode }).FirstOrDefault();

                    
                    
                    //var GetConnectionString = sqlcons.Query(@"Select Con_Server,Con_UserId,Con_Password,Con_Database,Con_hospitalCode from  HospitalRegistration where Con_hospitalCode='" + CliniCode + "' and Deactivate=0 and Isactive=1").FirstOrDefault();

                     GetConnectionString = sqlcons.Query(
   @"SELECT HospitalName, Con_Server,Con_UserId,Con_Password,Con_Database,Con_hospitalCode
     FROM HospitalRegistration
     WHERE Con_hospitalCode = @CliniCode
       AND Deactivate = 0
       AND Isactive = 1",
   new { CliniCode }
).FirstOrDefault();

                    Con_Server = GetConnectionString.Con_Server;
                    Con_UserId = GetConnectionString.Con_UserId;
                    Con_Password = GetConnectionString.Con_Password;
                    Con_Database = GetConnectionString.Con_Database;
                    Con_hospitalCode = GetConnectionString.Con_hospitalCode;
                    if (GetConnectionString == null)
                    {
                        var data = "Please enter valid customer code";
                        TempData["Message"] = "Failed to Login";
                        return RedirectToAction("Login", "Home", new { area = "" });
                        // return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    if (GetConnectionString.Con_hospitalCode == CliniCode)
                    {
                        try
                        {
                            BVGWellnessCenterDapparORM.SetConnection(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = ex.Message + "Failed to Login :Step-2";
                            return RedirectToAction("Login", "Home", new { area = "" });
                        }
                    }
                    else
                    {
                        var data = "Customer code is invalid";
                        TempData["Message"] = "Failed to Login";
                        return RedirectToAction("Login", "Home", new { area = "" });
                        // return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = ex.Message + "Failed to Login :Step-3" + CliniCode + " | " + Con_Server + "|" + Con_UserId + " | " + Con_Password + " | " + Con_Database + " | " + Con_hospitalCode;
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
               


               



                //Session["UserID"] = 1;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_LoginId", UserID);
                param.Add("@p_LoginPassword", password);
                var UserResult = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_Login", param).FirstOrDefault();
                if (UserResult != null)
                {
                    Session["UserID"] = UserResult.UserId;
                    Session["UserName"] = UserResult.FirstName + " " + UserResult.LName;
                    // 1. इथे नवीन सेशन्स ॲड केले आहेत (Gender आणि ProfileImage साठी)
                    Session["Gender"] = UserResult.Gender;
                    Session["ProfileImage"] = UserResult.ProfileImage;
                    Session["UserClinicCode"] = CliniCode?.ToString().Trim().ToUpper();
                    Session["ClinicFid"] = UserResult.ClinicFid;
                    if (GetConnectionString != null && GetConnectionString.HospitalName != null)
                    {
                        Session["ClinicName"] = GetConnectionString.HospitalName;
                    }
                    else
                    {
                        Session["ClinicName"] = UserResult.ClinicName ?? "Our Clinic";
                    }
                    //Session["ClinicName"] = UserResult.ClinicName ?? "Our Clinic";
                    // Greeting Logic add here
                    string greeting = DateTime.Now.Hour < 12 ? "Good Morning" :
                                      DateTime.Now.Hour < 17 ? "Good Afternoon" : "Good Evening";

                    Session["Greeting"] = greeting;

                    if (UserResult.IsAdmin == true)
                    {
                        Session["UserRole"] = "IsAdmin";
                        Session["RoleName"] = "Administrator";
                        
                    }
                    else if (UserResult.IsDoctor == true)
                    {
                        Session["UserRole"] = "IsDoctor";
                        Session["RoleName"] = "Doctor";
                    }
                    else if (UserResult.IsReceptionist == true)
                    {
                        Session["UserRole"] = "IsReceptionist";
                        Session["RoleName"] = "Receptionist";
                    }
                    else if (UserResult.IsTherapist == true)
                    {
                        Session["UserRole"] = "IsTherapist";
                        Session["RoleName"] = "Therapist";
                    }

                    //CreateQRCode(UserResult.ClinicFid);



                    return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
                    //if (UserResult.IsAdmin==true)
                    //{
                    //    Session["UserDesignation"] = "Admin";
                    //    return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
                    //    //return RedirectToAction("AdminDashboard", "AdminDashboard", new { area = "Admin" });
                    //}
                    //else if(UserResult.IsDoctor== true)
                    //{
                    //    Session["UserDesignation"] = "Doctor";
                    //    return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
                    //}else if(UserResult.IsReceptionist== true)
                    //{
                    //    Session["UserDesignation"] = "Receptionist";
                    //    return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });
                    //    //return RedirectToAction("AdminDashboard", "AdminDashboard", new { area = "Admin" });
                    //}
                }
                else
                {
                    TempData["Message"] = "Failed to Login";
                    TempData["icon"] = "error";
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                //return RedirectToAction("Dashboard", "Dashboard", new { area = "Patient" });

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                //return RedirectToAction("ErrorPage", "Login");
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }
        public ActionResult ErrorPage()
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetDoctorImage(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return File(Server.MapPath("~/assets/images/male-doctor.png"), "image/png");
                }

                // १. डेटाबेसमधील डबल स्लॅश (\\) चा प्रॉब्लेम क्लीन करा
                string cleanPath = filePath.Replace("\\\\", "\\");

                // २. फाईल खरोखर त्या जागी अस्तित्वात आहे का ते तपासा
                if (!System.IO.File.Exists(cleanPath))
                {
                    // जर फाईल सिस्टीममध्ये मिळाली नाही, तर ही एरर TempData मध्ये टाका जेणेकरून आपल्याला समजेल
                    TempData["DebugImageError"] = "File not found at path: " + cleanPath;
                    return File(Server.MapPath("~/assets/images/male-doctor.png"), "image/png");
                }

                // ३. फाईलचे एक्सटेंशन तपासा
                string contentType = "image/png";
                if (cleanPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    cleanPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/jpeg";
                }

                // ४. फाईलचे सर्व बाइट्स मेमरीमध्ये रीड करून पाठवा (परमिशन प्रॉब्लेम टाळण्यासाठी)
                byte[] fileBytes = System.IO.File.ReadAllBytes(cleanPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                // जर परमिशनचा किंवा दुसरा काही एरर आला तर तो TempData मध्ये सेव्ह करा
                TempData["DebugImageError"] = "Catch Error: " + ex.Message;
                return File(Server.MapPath("~/assets/images/male-doctor.png"), "image/png");
            }
        }
        public void CreateQRCode(int ClicnicFid)
        {
            try
            {
                string Url = "http://welness.bvglife.com/Patient/QRScanePatient/AddPatientQRScane";
               
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(Url, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode1 = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode1.GetGraphic(60);

                var movelocation = Server.MapPath("~/QRCodes/" + ClicnicFid + "/");

                if (!Directory.Exists(movelocation))
                {
                    Directory.CreateDirectory(movelocation);
                }
                string FileNameNew = "QRCode-" + ClicnicFid;
                qrCodeImage.Save(movelocation + FileNameNew);

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProcessType", "Save");
                param.Add("@p_QRCodeImage", movelocation + FileNameNew);
                param.Add("@p_ClinicFid", ClicnicFid);
                var result = BVGWellnessCenterDapparORM.ExcuteSp("sp_SUD_QRCode", param);

            }
            catch (Exception)
            {

                throw;
            }

        }



// 1. Forgot Password Page (GET)
public ActionResult ForgotPassword() { return View(); }

        [HttpPost]
        public ActionResult SendOTP(string Email, string ClinicCode, string LoginId)
        {
            try
            {
                // युझरने टाकलेली माहिती ViewBag मध्ये स्टोअर करा जेणेकरून ती फॉर्मवर तशीच राहील
                ViewBag.ClinicCode = ClinicCode;
                ViewBag.LoginId = LoginId;
                ViewBag.Email = Email;

                // 1. Basic Validation
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(ClinicCode))
                {
                    TempData["Message"] = "Please enter both Clinic Code and Email.";
                    TempData["Icon"] = "error";
                    return View("ForgotPassword");
                }

                // 2. Main Registry (sqlcons) open kara
                if (sqlcons.State == ConnectionState.Closed) sqlcons.Open();

                // 3. HospitalRegistration madhun connection details gha
                var clinicInfo = sqlcons.Query(@"SELECT Con_Server, Con_UserId, Con_Password, Con_Database 
                                        FROM HospitalRegistration 
                                        WHERE LTRIM(RTRIM(Con_hospitalCode)) = @ClinicCode 
                                        AND Deactivate = 0",
                                                new { ClinicCode = ClinicCode.Trim() }).FirstOrDefault();

                if (clinicInfo == null)
                {
                    TempData["Message"] = "Invalid Clinic Code or Clinic is inactive!";
                    TempData["Icon"] = "error";
                    return View("ForgotPassword");
                }

                // 4. Connection String
                string dynamicConnString = $"Data Source={clinicInfo.Con_Server};Initial Catalog={clinicInfo.Con_Database};User ID={clinicInfo.Con_UserId};Password={clinicInfo.Con_Password};";

                string status = "error";
                string dbMessage = "";

                // 5. Direct ADO.NET vapra
                using (SqlConnection conn = new SqlConnection(dynamicConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_SUD_UserMaster", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ProcessType", "CheckEmail");
                        cmd.Parameters.AddWithValue("@p_Email", Email.Trim());
                        cmd.Parameters.AddWithValue("@p_LoginId", LoginId.Trim());

                        SqlParameter msgParam = new SqlParameter("@p_Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                        SqlParameter iconParam = new SqlParameter("@p_Icon", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

                        cmd.Parameters.Add(msgParam);
                        cmd.Parameters.Add(iconParam);

                        conn.Open();
                        SqlConnection.ClearPool(conn);

                        cmd.ExecuteNonQuery();

                        dbMessage = msgParam.Value != DBNull.Value ? msgParam.Value.ToString() : "Email check failed";
                        status = iconParam.Value != DBNull.Value ? iconParam.Value.ToString() : "error";
                    }
                }

                // 6. Result Check kara
                if (status != "success")
                {
                    // कोणत्याही ईमेल किंवा लॉगिन विसंगतीच्या मेसेजला बदलण्यासाठी
                    if (dbMessage.Contains("Login ID") || dbMessage.Contains("Email") || dbMessage.Contains("match"))
                    {
                        TempData["Message"] = "Incorrect Email ID! Please enter your registered Email ID.";
                    }
                    else
                    {
                        TempData["Message"] = dbMessage;
                    }
                    TempData["Icon"] = "error";
                    return View("ForgotPassword");
                }

                // 7. OTP Generate ani Session store
                string otp = new Random().Next(1000, 9999).ToString();
                Session["ResetOTP"] = otp;
                Session["ResetEmail"] = Email.Trim();
                Session["ResetClinicCode"] = ClinicCode.Trim();
                Session["ResetLoginId"] = LoginId.Trim();

                // 8. Mail Pathvanyacha Logic (SMTP)
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("apiprobus@gmail.com", "Wellness Center");
                mail.To.Add(Email.Trim());
                mail.Subject = "Password Reset OTP";
                mail.Body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;'>
                <h2 style='color: #5881ed;'>Password Reset Request</h2>
               <p>Hello <b>{LoginId.Trim()}</b>,</p>
                <p>Your OTP for resetting the password for Clinic <b>{ClinicCode}</b> is:</p>
                <h1 style='letter-spacing: 5px; color: #333;'>{otp}</h1>
                <p>This OTP is valid for a limited time. Please do not share it with anyone.</p>
                <br/>
                <p>Regards,<br/>Team Wellness</p>
            </div>";
                mail.IsBodyHtml = true;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("apiprobus@gmail.com", "nibd kmnr fbdb pguf");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                TempData["Message"] = "OTP has been sent to your registered email!";

                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "System Error: " + ex.Message;
                TempData["Icon"] = "error";
                return View("ForgotPassword");
            }
        }

        public ActionResult VerifyOTP() { return View(); }

    [HttpPost]
    public ActionResult VerifyOTP(string UserOTP)
    {
        if (Session["ResetOTP"] != null && Session["ResetOTP"].ToString() == UserOTP)
        {
            return RedirectToAction("ResetPassword");
        }
        TempData["Message"] = "Invalid OTP!";
        TempData["Icon"] = "error";
        return View();
    }


        [HttpGet]
        public ActionResult ResendOTP()
        {
            try
            {
                // सेशन्समधून डेटा मिळवा
                string email = Session["ResetEmail"]?.ToString();
                string clinicCode = Session["ResetClinicCode"]?.ToString();
                string loginId = Session["ResetLoginId"]?.ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(clinicCode) || string.IsNullOrEmpty(loginId))
                {
                    TempData["Message"] = "Session expired. Please request OTP again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ForgotPassword");
                }

                // नवीन OTP जनरेट करा
                string otp = new Random().Next(1000, 9999).ToString();
                Session["ResetOTP"] = otp;

                // SMTP द्वारे पुन्हा ईमेल पाठवा
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("apiprobus@gmail.com", "Wellness Center");
                mail.To.Add(email);
                mail.Subject = "Password Reset OTP (Resent)";
                mail.Body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;'>
                <h2 style='color: #5881ed;'>Password Reset Request - Resent OTP</h2>
                <p>Hello <b>{loginId}</b>,</p>
                <p>Your new OTP for resetting the password for Clinic <b>{clinicCode}</b> is:</p>
                <h1 style='letter-spacing: 5px; color: #333;'>{otp}</h1>
                <p>This OTP is valid for a limited time. Please do not share it with anyone.</p>
                <br/>
                <p>Regards,<br/>Team Wellness</p>
            </div>";
                mail.IsBodyHtml = true;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("apiprobus@gmail.com", "nibd kmnr fbdb pguf");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                TempData["Message"] = "OTP has been resent to your registered email!";
                TempData["Icon"] = "success";

                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "System Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("VerifyOTP");
            }
        }

        // 4. Reset Password Page (GET)
        public ActionResult ResetPassword() { return View(); }

        [HttpPost]
        public ActionResult UpdateForgottenPassword(string NewPassword)
        {
            try
            {
                // 1. Session मधून आधी सेव्ह केलेल्या व्हॅल्यूज घ्या
                string clinicCode = Session["ResetClinicCode"]?.ToString();
                string resetEmail = Session["ResetEmail"]?.ToString();
                string resetLoginId = Session["ResetLoginId"]?.ToString(); // SendOTP मधून आलेला अचूक आयडी

                if (string.IsNullOrEmpty(clinicCode) || string.IsNullOrEmpty(resetEmail) || string.IsNullOrEmpty(resetLoginId))
                {
                    TempData["Message"] = "Session expired. Please try again from the first step.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ForgotPassword");
                }

                // 2. Main Registry (JYAMDB) मधून कनेक्शनचे डिटेल्स घ्या
                if (sqlcons.State == ConnectionState.Closed) sqlcons.Open();
                var clinicInfo = sqlcons.Query(@"SELECT Con_Server, Con_UserId, Con_Password, Con_Database 
                                        FROM HospitalRegistration 
                                        WHERE LTRIM(RTRIM(Con_hospitalCode)) = @clinicCode 
                                        AND Deactivate = 0",
                                                new { clinicCode }).FirstOrDefault();

                if (clinicInfo == null) { throw new Exception("Clinic dynamic connection configurations not found."); }

                // 3. नवीन फ्रेश कनेक्शन स्ट्रिंग बनवा
                string dynamicConnString = $"Data Source={clinicInfo.Con_Server};Initial Catalog={clinicInfo.Con_Database};User ID={clinicInfo.Con_UserId};Password={clinicInfo.Con_Password};";

                string status = "error";
                string dbMessage = "";

                // 4. डायरेक्ट ADO.NET वापरून डेटाबेसला हिट करा (Dapper चा पूल इथे नको)
                using (SqlConnection conn = new SqlConnection(dynamicConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_SUD_UserMaster", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_ProcessType", "ResetPassword");
                        cmd.Parameters.AddWithValue("@p_Email", resetEmail.Trim());
                        cmd.Parameters.AddWithValue("@p_LoginId", resetLoginId.Trim()); // हा आयडी आता वरती केलेल्या SP च्या बदलाशी मॅच होईल
                        cmd.Parameters.AddWithValue("@p_LoginPassword", NewPassword);

                        // Output Parameters
                        SqlParameter msgParam = new SqlParameter("@p_Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                        SqlParameter iconParam = new SqlParameter("@p_Icon", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

                        cmd.Parameters.Add(msgParam);
                        cmd.Parameters.Add(iconParam);

                        conn.Open();
                        SqlConnection.ClearPool(conn); // जुना कोणताही कॅश डेटा क्लिअर करा

                        cmd.ExecuteNonQuery();

                        dbMessage = msgParam.Value != DBNull.Value ? msgParam.Value.ToString() : "Database execution failed";
                        status = iconParam.Value != DBNull.Value ? iconParam.Value.ToString() : "error";
                    }
                }

                // 5. रिझल्ट नुसार व्ह्यू दाखवा
                if (status == "success")
                {
                    // सिक्युरिटीसाठी सर्व सेशन्स नष्ट करा
                    Session.Clear();
                    Session.Abandon();

                    TempData["Message"] = "Password updated successfully! Redirecting to login...";
                    TempData["Icon"] = "success";
                    return View("ResetPassword");
                }
                else
                {
                    // फेल झालं तर नक्की का फेल झालं आणि कोणत्या DB मध्ये फेल झालं ते दाखवेल
                    TempData["Message"] = dbMessage + " (Checked in: " + clinicInfo.Con_Database + ")";
                    TempData["Icon"] = "error";
                    return View("ResetPassword");
                }
            }
            catch (Exception ex)
            {
                Session.Clear();
                TempData["Message"] = "System Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("Login");
            }
        }
    }
}