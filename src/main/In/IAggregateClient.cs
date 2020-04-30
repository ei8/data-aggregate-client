using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Data.Aggregate.Client.In
{
    public interface IAggregateClient
    {
        Task ChangeAggregate(string avatarUrl, string id, string newAggregate, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken));
    }
}
