using NLog;
using neurUL.Common.Http;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Data.Aggregate.Client.In
{
    public class HttpAggregateClient : IAggregateClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpAggregateClient.logger.Error(ex, "Error occurred while communicating with ei8 Aggregate. " + ex.InnerException?.Message)
            );

        private static readonly string aggregatesPath = "data/aggregates/";
        private static readonly string aggregatesPathTemplate = aggregatesPath + "{0}";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpAggregateClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task ChangeAggregate(string avatarUrl, string id, string newAggregate, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken)) =>
            await HttpAggregateClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.ChangeAggregateInternal(avatarUrl, id, newAggregate, expectedVersion, authorId, token).ConfigureAwait(false));

        private async Task ChangeAggregateInternal(string avatarUrl, string id, string newAggregate, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                Aggregate = newAggregate,
                AuthorId = authorId
            };

            await this.requestProvider.PutAsync(
               $"{avatarUrl}{string.Format(HttpAggregateClient.aggregatesPathTemplate, id)}",
               data,
               token: token,
               headers: new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }
    }
}
