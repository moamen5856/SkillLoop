using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
