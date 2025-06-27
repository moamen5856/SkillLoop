using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.Models
{
    public class Project
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ValidateNever]
        public string ProjectId { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ProjectURL { get; set; }
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [ValidateNever]
        public Category Category { get; set; }
        public string CategoryId { get; set; }


        [ValidateNever]
        public ICollection<UserProject> UserProjects { get; set; }

        [ValidateNever]
        public ICollection<Review> Reviews { get; set; }

        [ValidateNever]
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        [ValidateNever]

        public ICollection<ProjectTechnology> ProjectTechnologies { get; set; }
        [ValidateNever]

        public ICollection<ProjectImage> ProjectImages { get; set; }
    }
}