using System;
using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public record ZipStage(
        Func<IEnumerable<Json>, Json> ZipUp, 
        IEnumerable<Json> Parts);
    public interface IJsonZipper
    {
        ZipStage ZipDown(Json json, IMashContext context, IMashStack stack);
    }
}
