using System.Collections.Generic;
using StackExchange.Redis;

namespace RediSearchSharp.Internal
{
    public class Document
    {
        public string Id { get; set; }
        public double Score { get; set; }
        public byte[] Payload { get; set; }
        public Dictionary<string, RedisValue> Fields { get; set; }

        public static Document Load(string id, double score, byte[] payload, RedisValue[] fields)
        {
            return new Document
            {
                Id = id,
                Score = score,
                Payload = payload,
                Fields = InitializeFieldsFrom(fields)
            };
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