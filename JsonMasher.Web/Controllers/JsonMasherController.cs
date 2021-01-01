using System;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using JsonMasher.Web.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonMasher.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JsonMasherController : ControllerBase
    {
        private readonly ILogger<JsonMasherController> _logger;

        public JsonMasherController(ILogger<JsonMasherController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<string> Post(ProgramInputDto dto)
        {
            try
            {
                var sb = new StringBuilder();
                var (filter, sourceInformation) = new Parser().Parse(dto.Program);
                IMashStack stack = new DebugMashStack();
                var inputs = dto.Input.AsMultipleJson();
                if (dto.Slurp)
                {
                    inputs = Json.Array(inputs).AsEnumerable();
                }
                var (results, context) = new Mashers.JsonMasher().Mash(
                    inputs, filter, stack, sourceInformation, 100000);
                foreach (var result in results)
                {
                    sb.AppendLine(result.ToString());
                }
                foreach (var log in context.Log)
                {
                    sb.AppendLine(log.ToString());
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
