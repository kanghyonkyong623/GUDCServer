using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GUDC.Models
{
    public class TeamItem 
    {
        public int id { get; set; }
        
        public string TEAMCODE { get; set; }

        
        [Required]
        [Display(Name = "TEAM NAME")]
        [Remote("CheckTeamNameExist", "Teams", ErrorMessage = "The Team Name Exists")]
        public string TEAMNAME { get; set; }

        public string DISTRICT { get; set; }
        public int MEMBERCOUNT { get; set; }

        [Display(Name = "TEAM STATUS")]
        public string TEAMSTATUS { get; set; }

        public string TEAMLOCATION { get; set; }

    }
}