using System.ComponentModel.DataAnnotations;

namespace JsonMasher.Web.Model
{
    public class ProgramInputDto
    {
        [Required]
        public string Program { get; set; }

        [Required]
        public string Input { get; set; }
        
        public bool Slurp { get; set; }
    }
}
