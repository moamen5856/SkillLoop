using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SkillLoop2.Models
{
    public class ProjectFile
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProjectFilesId { get; set; }

        [Required, StringLength(50, MinimumLength = 2)]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string FileType { get; set; }

        [Required]
        public string FileExtension { get; set; }

        [Required]
        public long FileSize { get; set; } = 0; // القيمة الافتراضية

        [ForeignKey("ProjectId")]
        [ValidateNever]
        public Project Project { get; set; }

        public string ProjectId { get; set; }
    }
}