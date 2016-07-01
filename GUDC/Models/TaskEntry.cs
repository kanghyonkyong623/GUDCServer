using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace GUDC.Models
{
    public class TaskEntry
    {
        public int id { get; set; } //id

        public string TASK { get; set; }

        public string DETAILS { get; set; }

        public string STATUS { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime START { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime END { get; set; }

        [Required]
        public string LOCATION { get; set; }

        public string COORDINATE { get; set; }

        [Required]
        [Display(Name = "TEAM NAME")]
        public string TEAMNAME { get; set; }

        [Display(Name = "DIGGING LENGTH")]
        public string DIGLEN { get; set; }

        [Display(Name = "CLIENT NAME")]
        public string CLIENTNAME { get; set; }

        [JsonIgnore]
        [DataType(DataType.MultilineText)]
        public string route { get; set; }

    }
}