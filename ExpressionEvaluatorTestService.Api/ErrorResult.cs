using System;

namespace ExpressionEvaluatorTestService.Api
{
    public sealed class ErrorResult
    {
        public string Stack { get; }

        public string Message { get; }

        public string TypeName { get; }

        public ErrorResult(string stack, string message, string typeName)
        {
            Stack = stack;
            Message = message;
            TypeName = typeName;
        }

        public static ErrorResult From(Exception e) 
            => new ErrorResult(e.StackTrace, e.Message, e.GetType().Name);
    }
}