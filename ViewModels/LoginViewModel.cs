using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Please Enter An Email Address.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Your Account Password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="Remember me?")]
        public bool RememberMe { get; set; }
    }
}
