using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Name is Required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pasword is Required.")]
        [StringLength(40,MinimumLength =8)]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password Does Not Match")]

        [Display(Name = "Paswword")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Paswword")]

        public string ConfirmPassword { get; set; }

    }
}
