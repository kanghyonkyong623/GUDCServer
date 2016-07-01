using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GUDC.Models
{
    public class TeamMemberEntry
    {
        public int id { get; set; }

        [Display(Name = "TEAM NAME")]
        public string TEAMCODE { get; set; } // id

        [Required]
        [Display(Name = "TEAM MEMBER NAME")]
        public string TEAMMEMBERNAME { get; set; }

        
        [Display(Name = "TEAM MEMBER CODE")]
        public string TEAMMEMBERCODE { get; set; }

        [Display(Name = "TEAM MEMBER TEL. NO.")]
        [DataType(DataType.PhoneNumber)]
        public string TEAMMEMBERNO { get; set; }

        public string DESCRIPTION { get; set; }

        [Display(Name = "TEAM MEMBER ROLE")]
        public string TEAMMEMBERROLE { get; set; }
    }
}