using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace ExpressionEvaluatorTestService.Api
{
    public sealed class EvaluateExpression
    {
        public string Data { get; }

        public bool Script { get; }

        public EvaluateExpression(string data, bool script)
        {
            Data = data;
            Script = script;
        }
    }
}