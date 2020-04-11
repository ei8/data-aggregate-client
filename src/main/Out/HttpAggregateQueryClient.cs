using NLog;
using org.neurul.Common.Http;
using Polly;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.Data.Aggregate.Common;

namespace works.ei8.Data.Aggregate.Client.Out
{
    public class HttpAggregateQueryClient : IAggregateQueryClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpAggregateQueryClient.logger.Error(ex, "Error occurred while communicating with ei8 Aggregate. " + ex.InnerException?.Message)
            );
        private static readonly string GetAggregatesPathTemplate = "data/aggregates";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpAggregateQueryClient()
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<ItemData> GetItemById(string avatarUrl, string id, CancellationToken token = default(CancellationToken)) =>
           await HttpAggregateQueryClient.exponentialRetryPolicy.ExecuteAsync(
               async () => await this.GetItemByIdInternal(avatarUrl, id, token).ConfigureAwait(false));
        
        private async Task<ItemData> GetItemByIdInternal(string avatarUrl, string id, CancellationToken token = default)
        {
            return await requestProvider.GetAsync<ItemData>(
                           $"{avatarUrl}{HttpAggregateQueryClient.GetAggregatesPathTemplate}/{id}",
                           token: token
                           );
        }
    }
}
