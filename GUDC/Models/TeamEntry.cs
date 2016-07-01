using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Web;

namespace GUDC.Models
{
    public class TeamEntry
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string TEAMCODE { get; set; } // id

        public string USENAME { get; set; }

        public string ROLE { get; set; }

        public string NAME { get; set; }

        public string PHONE { get; set; }

        public string EMAIL { get; set; }        

    }

}
