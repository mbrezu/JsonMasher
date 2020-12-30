using System.Collections.Generic;
using JsonMasher.Mashers;

namespace JsonMasher.Compiler
{
    public record ProgramPosition(int StartPosition, int EndPosition);
    public class SourceInformation
    {
        private Dictionary<ObjectKey, ProgramPosition> _astToPosition = new ();

        public void SetProgramPosition(IJsonMasherOperator ast, ProgramPosition position)
            => _astToPosition[new ObjectKey(ast)] = position;

        public ProgramPosition GetProgramPosition(IJsonMasherOperator ast)
        {
            var key = new ObjectKey(ast);
            if (_astToPosition.ContainsKey(key))
            {
                return _astToPosition[key];
            }
            else
            {
                return null;
            }
        }
    }
}
