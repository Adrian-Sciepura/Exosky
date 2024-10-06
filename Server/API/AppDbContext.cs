using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StarData> Stars { get; set; }
        public DbSet<ExoplanetData> Exoplanets { get; set; }
    }


    public class StarData
    {
        [Key] [StringLength(19)] public string GAIA_id { get; set; }
        public double parallax { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }

    public class ExoplanetData
    {
        [Key] [StringLength(24)] public string name { get; set; }
        public double parallax { set; get; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
}
