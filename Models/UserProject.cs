using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillLoop2.Models
{
    public class UserProject
    {
        [Key, Required]
        public string UserProjectID { get; set; }

        public User User { get; set; }
        public string UserId { get; set; }

        public Project Project { get; set; }
        public string ProjectId { get; set; }
    }
}
