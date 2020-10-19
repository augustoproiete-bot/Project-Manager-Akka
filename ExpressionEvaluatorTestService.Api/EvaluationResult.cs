namespace ExpressionEvaluatorTestService.Api
{
    public sealed class EvaluationResult 
    {
        public object Result { get; }

        public bool Primitive { get; }

        public bool Error { get; }

        public EvaluationResult(object result, bool primitive, bool error)
        {
            Result = result;
            Primitive = primitive;
            Error = error;
        }
    }
}