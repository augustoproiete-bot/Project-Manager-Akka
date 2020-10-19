using System;
using Akka.Actor;
using CodingSeb.ExpressionEvaluator;
using ExpressionEvaluatorTestService.Api;
using Tauron.Akka;

namespace ExpressionEvaluatorTestService
{
    public sealed class ActualEvaluator : ExposedReceiveActor
    {
        private readonly ExpressionEvaluator _expressionEvaluator = new ExpressionEvaluator();

        public ActualEvaluator()
        {
            Receive<EvaluateExpression>(Evaluate);
        }

        private void Evaluate(EvaluateExpression request)
        {
            object result;

            try
            {
                result = request.Script ? _expressionEvaluator.ScriptEvaluate(request.Data) : _expressionEvaluator.Evaluate(request.Data);
            }
            catch (Exception e)
            {
                result = ErrorResult.From(e);
            }

            Sender.Tell(new EvaluationResult(result, result.GetType().IsPrimitive, false));
        }
    }
}