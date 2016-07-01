using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace GUDC.Models
{
    public class GUDCContext : DbContext
    {
        public GUDCContext()
            : base("GUDC")
        {

        }

        public DbSet<TeamEntry> TeamEntries { get; set; }
        public DbSet<TeamMemberEntry> TeamMemberEntries { get; set; }
        public DbSet<TaskEntry> TaskEntries { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<TeamItem> TeamItems { get; set; }
        public DbSet<PointsTable> PointArrayTable { get; set; }
        public DbSet<TrackInfoTable> TrackInfoTables { get; set; }
    }
}