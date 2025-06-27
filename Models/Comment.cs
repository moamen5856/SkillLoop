using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.Models
{
    public class Comment
    {
        public string CommentId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 500 characters.")]
        public string CommentContent { get; set; }

        public DateTime CommentDate { get; set; }

        public Review Review { get; set; }

        public string ReviewId { get; set; }
    }
}
