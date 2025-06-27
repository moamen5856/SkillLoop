using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.Models
{
    public class ProjectTechnology
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ValidateNever]
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string TechologyName { get; set; }

        public Project Project { get; set; }
        public string ProjectId { get; set; }

    }
}
