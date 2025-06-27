using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.Models
{
    public class ProjectImage
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ValidateNever]
        public string Id { get; set; } 
        public string ImageUrl { get; set; }


        [ForeignKey("ProjectId")]
        [ValidateNever]
        
        public Project Project { get; set; }
        public string ProjectId { get; set; }

    }
}
