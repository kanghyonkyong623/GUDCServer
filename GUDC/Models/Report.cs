using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GUDC.Models
{
    //public class Report
    //{
    //    public string Task { get; set; }
    //    public string Status { get; set; }
    //    public DateTime Start { get; set; }
    //    public DateTime End { get; set; }
    //    public string Location { get; set; }
    //    public string District { get; set; }
    //}
    public class TrackInfoTable
    {
        public int id { get; set; } 
        public string taskName { get; set; }
        public string teamName { get; set; }
        public DateTime date { get; set; }
    }
    public class PointsTable
    {
        public int id { get; set; } 
        public string taskName { get; set; }
        //public string teamName { get; set; }        
        public int index { get; set; }
        //public DateTime date { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
    }
}