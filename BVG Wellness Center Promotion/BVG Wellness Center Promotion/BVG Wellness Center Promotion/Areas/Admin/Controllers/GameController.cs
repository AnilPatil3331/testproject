using BVG_Wellness_Center_Promotion.Areas.Admin.Models;
using BVG_Wellness_Center_Promotion.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Controllers
{
    public class GameController : Controller
    {
        // GET: Admin/Game
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddQuestion()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Query", "select LevelId as Id, LevelName  as Name from Mas_Game_Level");
                var Level = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param);
                ViewBag.Level = Level;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Query", "select AgeCategoryId as Id, AgeCategoryName as Name from Mas_AgeCategory");
                var AgeCategory = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param1);
                ViewBag.AgeCategory = AgeCategory;


                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Query", "select ProfessionId as Id, ProfessionName as Name from Mas_Game_Profession");
                var ProfessionList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param2);
                ViewBag.ProfessionList = ProfessionList;


                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_Query", "select DiseaseId as Id, DiseaseName as Name from Mas_Game_Disease");
                var DiseaseList = BVGWellnessCenterDapparORM.ReturnList<DropdownItem>("sp_ExecuteQuery", param3);
                ViewBag.DiseaseList = DiseaseList;


                return View();
            }
            catch(Exception Ex)
            {
                return View();
            }
           
        }

        public ActionResult QuestionList()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }
                return View();
            }
            catch (Exception Ex)
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult SaveQuestion(Game model, int? GameId, string[] SelectedProfessions, string[] SelectedDisease)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();

                // Multi-select comma join
                string professions = SelectedProfessions != null ? string.Join(",", SelectedProfessions) : "";
                string diseases = SelectedDisease != null ? string.Join(",", SelectedDisease) : "";
                string gender = model.Gender != null ? string.Join(",", model.Gender) : "";
                string marital = model.MaritalStatus != null ? string.Join(",", model.MaritalStatus) : "";
                string ageCat = model.AgeCategoryID != null ? string.Join(",", model.AgeCategoryID) : "";

                param.Add("@GameId", GameId);
                param.Add("@Gender", gender);
                param.Add("@MartialStatus", marital);
                param.Add("@LevelId", model.LevelId);
                param.Add("@QuestionType", model.QuestionType);
                param.Add("@Question", model.Question);

                param.Add("@OptionA", model.OptionA);
                param.Add("@OptionB", model.OptionB);
                param.Add("@OptionC", model.OptionC);
                param.Add("@OptionD", model.OptionD);

                param.Add("@SuggestionOptionA", model.SuggestionOptionA);
                param.Add("@SuggestionOptionB", model.SuggestionOptionB);
                param.Add("@SuggestionOptionC", model.SuggestionOptionC);
                param.Add("@SuggestionOptionD", model.SuggestionOptionD);

                param.Add("@ProfessionId", professions);
                param.Add("@DiseaseId", diseases);
                param.Add("@AgeCategoryId", ageCat);

                param.Add("@CreatedId", Convert.ToInt32(Session["UserID"]));
                param.Add("@IsUpdate", GameId == null ? 0 : 1);

                var result = BVGWellnessCenterDapparORM.ReturnList<dynamic>("sp_GameQuestion_InsertUpdate", param);

                TempData["Message"] = "Question Saved Successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("QuestionList");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Something went wrong!";
                TempData["Icon"] = "error";
                return RedirectToAction("AddQuestion");
            }
        }


    }
}