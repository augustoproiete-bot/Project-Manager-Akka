using System;
using MongoDB.Bson;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class CleanUpTime
    {
        public ObjectId Id { get; set; }

        public TimeSpan Interval { get; set; }

        public DateTime Last { get; set; }
    }
}