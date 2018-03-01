using System.Collections.Generic;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Query
{
    public class SearchResult<TEntity> 
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public IEnumerable<TEntity> Results { get; }

        public SearchResult(IRedisearchSerializer serializer, RedisResult[] response, bool withScoresFlag, bool withPayloadsFlag)
        {
            int step = 2;
            int scoreOffset = 0;
            int contentOffset = 1;
            int payloadOffset = 0;
            if (withScoresFlag)
            {
                step += 1;
                scoreOffset = 1;
                contentOffset += 1;
            }

            if (withPayloadsFlag)
            {
                payloadOffset = scoreOffset + 1;
                step += 1;
                contentOffset += 1;
            }
        
            // the first item is the total number of the response
            var searchResults = new List<TEntity>((int)response[0]);

            for (int i = 1; i < response.Length; i += step)
            {
                var id = (string)response[i];
                double score = 1.0;
                byte[] payload = null;
                if (withScoresFlag)
                {
                    score = (double)response[i + scoreOffset];
                }
                if (withPayloadsFlag)
                {
                    payload = (byte[])response[i + payloadOffset];
                }

                var fields = (RedisValue[])response[i + contentOffset];
                
                searchResults.Add(serializer.Deserialize<TEntity>(Document.Load(id, score, payload, fields)));
            }
            Results = searchResults;
        }
    }
}