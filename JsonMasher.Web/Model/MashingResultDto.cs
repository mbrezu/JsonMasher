using System.ComponentModel.DataAnnotations;

namespace JsonMasher.Web.Model
{
    public class MashingResultDto
    {
        public string StdOut { get; set; }

        public string StdErr { get; set; }
    }
}
