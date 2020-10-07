using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class CleanUpOperator : ExposedReceiveActor
    {
        public CleanUpOperator(IMongoCollection<CleanUpTime> cleanUp, IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucket)
        {
            Receive<StartCleanUp>(_ =>
            {
                try
                {
                    var data = cleanUp.AsQueryable().First();
                    if (data.Last + data.Interval >= DateTime.Now) return;

                        List<FilterDefinition<ToDeleteRevision>> deleted = new List<FilterDefinition<ToDeleteRevision>>();

                        foreach (var revision in revisions.AsQueryable())
                        {
                            bucket.Delete(ObjectId.Parse(revision.BuckedId));

                            deleted.Add(Builders<ToDeleteRevision>.Filter.Eq(r => r.BuckedId == revision.BuckedId, true));
                        }

                        if (deleted.Count != 0)
                        {
                            if (!revisions.DeleteMany(Builders<ToDeleteRevision>.Filter.And(deleted)).IsAcknowledged) 
                                Log.Warning("Delete Revisions not Deleted");
                        }

                        if (!cleanUp.UpdateOne(Builders<CleanUpTime>.Filter.Empty, Builders<CleanUpTime>.Update.Set(c => c.Last, DateTime.Now)).IsAcknowledged) 
                            Log.Warning("Cleanup Interval not updated");

                }
                catch (Exception e)
                {
                    Log.Error(e, "Error on Clean up Database");
                }
                finally
                {
                    Context.Stop(Self);
                }
            });
        }


    }
}