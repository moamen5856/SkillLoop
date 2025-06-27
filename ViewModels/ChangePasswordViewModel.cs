using System.ComponentModel.DataAnnotations;

namespace SkillLoop2.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pasword is Required.")]
        [StringLength(40, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Paswword")]

        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password Does Not Match")]
        [Display(Name ="Confirm New Paswword")]
        public string ConfirmNewPassword { get; set; }
    }
}
