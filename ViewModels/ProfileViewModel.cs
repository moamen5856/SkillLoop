using SkillLoop2.Models;
using System.ComponentModel.DataAnnotations;

public class ProfileViewModel : User
{
    public string UserID { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Range(18, 100)]
    public int Age { get; set; }

    [Required]
    public string Gander { get; set; }

    [Required]
    public bool IsPhotoDeleted { get; set; }



    public IFormFile imageFile { get; set; }
}