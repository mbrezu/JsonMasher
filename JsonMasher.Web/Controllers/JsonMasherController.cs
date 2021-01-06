using System;
using System.Collections.Generic;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
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
        public ActionResult<MashingResultDto> Post(ProgramInputDto dto)
        {
            try
            {
                var (filter, sourceInformation) = new Parser().Parse(dto.Program, new SequenceGenerator());
                IMashStack stack = new DebugMashStack();
                var inputs = dto.Input.AsMultipleJson();
                if (dto.Slurp)
                {
                    inputs = Json.Array(inputs).AsEnumerable();
                }
                var (results, context) = new Mashers.JsonMasher().Mash(
                    inputs, filter, stack, sourceInformation, 100000);
                return new MashingResultDto {
                    StdOut = CollectStdOut(results),
                    StdErr = CollectStdErr(context)
                };
            }
            catch (Exception ex)
            {
                return new MashingResultDto {
                    StdOut = ex.ToString()
                };
            }
        }

        private string CollectStdOut(IEnumerable<Json> results)
        {
            var sb = new StringBuilder();
            foreach (var result in results)
            {
                sb.AppendLine(result.ToString());
            }
            return sb.ToString();
        }

        private string CollectStdErr(IMashContext context)
        {
            var sb = new StringBuilder();
            foreach (var log in context.Log)
            {
                sb.AppendLine(log.ToString());
            }
            return sb.ToString();
        }
    }
}
