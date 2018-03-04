using System.Collections.Generic;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Query
{
    public class SearchResult<TEntity> 
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public TEntity Entity { get; private set; }
        public double? Score { get; private set; }
        public byte[] Payload { get; private set; }

        public static IEnumerable<SearchResult<TEntity>> LoadResults(IRedisearchSerializer serializer, RedisResult[] response,
            bool withScoresFlag, bool withPayloadsFlag)
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
            var results = new List<SearchResult<TEntity>>((int)response[0]);

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
                Document doc = Document.Load(id, score, payload, fields);

                results.Add(new SearchResult<TEntity>(serializer.Deserialize<TEntity>(doc), doc.Score, doc.Payload));
            }

            return results;
        }

        public SearchResult(TEntity entity, double score, byte[] payload)
        {
            Entity = entity;
            Score = score;
            Payload = payload;
        }
    }
}