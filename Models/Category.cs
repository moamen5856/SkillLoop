using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillLoop2.Models
{
    public class Category
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ValidateNever]
        public string CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; }
        [ValidateNever]
        public ICollection<Project> Projects { get; set; }
    }
}
