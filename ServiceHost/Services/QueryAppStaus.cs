using System;

namespace ServiceHost.Services
{
    public sealed class QueryAppStaus
    {
        public Guid OperationId { get; }

        public QueryAppStaus(Guid operationId) => OperationId = operationId;
    }
}