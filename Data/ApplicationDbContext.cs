using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillLoop2.Models;

namespace SkillLoop2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProjectFile> ProjectFiles { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<SkillLoop2.Models.UserProject> UserProject { get; set; } = default!;
        public DbSet<SkillLoop2.Models.ProjectTechnology> ProjectTechnology { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; }



    }  
}
