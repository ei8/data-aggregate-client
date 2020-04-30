using System.Threading;
using System.Threading.Tasks;
using ei8.Data.Aggregate.Common;

namespace ei8.Data.Aggregate.Client.Out
{
    public interface IAggregateQueryClient
    {
        Task<ItemData> GetItemById(string avatarId, string id, CancellationToken token = default(CancellationToken)); 
    }
}
