using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Areas.Admin.Models
{
    public class Game
    {
        public int LevelId { get; set; }
        public List<string> Gender { get; set; }
        public List<string> MaritalStatus { get; set; }


        public List<int> SelectedProfessions { get; set; }

        public List<int> SelectedDisease { get; set; }
        public string QuestionType { get; set;}

            public string Question { get; set; }
            public string OptionA { get; set; }
            public string OptionB { get; set; }
            public string OptionC { get; set; }
            public string OptionD { get; set; }
            public string SuggestionOptionA { get; set; }
            public string SuggestionOptionB { get; set; }
            public string SuggestionOptionC { get; set; }
            public string SuggestionOptionD { get; set; }
            public bool IsActive { get; set; }
        public List<string> AgeCategoryID { get; set; }
    }
}