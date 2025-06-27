using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SkillLoop2.Models
{
    public class Review
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ReviewID { get; set; }

        [Required]
        public DateTime EvalDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public float Rating { get; set; } = 0.0f;

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0, 1, ErrorMessage = "Like must be 0 or 1.")]
        public int Like { get; set; } = 0;

        public Project Project { get; set; }
        public string ProjectId { get; set; }

        public User User { get; set; }
        public string UserId { get; set; }

        [ValidateNever]
        public ICollection<Comment> Comments { get; set; }
    }
}
