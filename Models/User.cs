using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillLoop2.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string image { get; set; } = "~/images/profile photo.webp";

        [Required]
        [Range(18, 100)]
        public int Age { get; set; }



        [Required]
        public string Gander { get; set; } = "NotSpecified"; // Default value


        [Required]
        public DateTime JoinDate { get; set; } = DateTime.Now; // Default value

        public bool? IsPhotoDeleted { get; set; }

        [ValidateNever]
        public ICollection<UserProject> UserProjects { get; set; }

        [ValidateNever]
        public ICollection<Review> Reviews { get; set; }
    }
}
