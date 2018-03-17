using System.Collections.Generic;
using System.Linq;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Query
{
    public class SearchResult<TEntity> 
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public TEntity Entity { get; }
        public double? Score { get; }
        public byte[] Payload { get; }

        private SearchResult(TEntity entity, double? score, byte[] payload)
        {
            Entity = entity;
            Score = score;
            Payload = payload;
        }

        internal static IEnumerable<SearchResult<TEntity>> LoadMGetResults(IRedisearchSerializer serializer,
            RedisResult[] response)
        {
            var results = new List<SearchResult<TEntity>>();
            foreach (RedisValue[] entityAsFields in response.Where(r => !r.IsNull))
            {
                var entity = serializer.Deserialize<TEntity>(InitializeFieldsFrom(entityAsFields));

                results.Add(new SearchResult<TEntity>(
                    entity,
                    null,
                    null));
            }

            return results;
        }

        internal static IEnumerable<SearchResult<TEntity>> LoadSearchResults(IRedisearchSerializer serializer, RedisResult[] response,
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

                var fieldsArray = (RedisValue[])response[i + contentOffset];
                var entity = serializer.Deserialize<TEntity>(InitializeFieldsFrom(fieldsArray));
                
                results.Add(new SearchResult<TEntity>(
                    entity,
                    withScoresFlag ? (double?)score : null,
                    withPayloadsFlag ? payload : null));
            }

            return results;
        }

        private static Dictionary<string, RedisValue> InitializeFieldsFrom(RedisValue[] fields)
        {
            var fieldValues = new Dictionary<string, RedisValue>();
            if (fields == null) return fieldValues;
            for (int i = 0; i < fields.Length; i += 2)
            {
                fieldValues.Add(fields[i], fields[i + 1]);
            }

            return fieldValues;
        }
    }
}