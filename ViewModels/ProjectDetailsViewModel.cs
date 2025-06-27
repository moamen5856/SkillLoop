using SkillLoop2.Models;

namespace SkillLoop2.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public List<Review>? Reviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public float? UserRating { get; set; }
        public int? UserLike {  get; set; }
    }
}
