namespace RediSearchSharp.Query
{
    public enum TermResolvingStrategies
    {
        Expanded,
        Exact
    }

    public class QueryOptions
    {
        public bool Verbatim { get; set; }
        public bool WithScores { get; set; }
        public bool WithScoreKeys { get; set; }
        public bool WithPayloads { get; set; }
        public bool DisableStopwordFiltering { get; set; }
        public TermResolvingStrategies DefaultTermResolvingStrategy { get; set; }
        public bool InOrder { get; set; }
        
        public static readonly QueryOptions DefaultOptions = new QueryOptions
        {
            Verbatim = false,
            WithScores = false,
            WithScoreKeys = false,
            WithPayloads = false,
            DisableStopwordFiltering = false,
            DefaultTermResolvingStrategy = TermResolvingStrategies.Exact,
            InOrder = false
        };
    }
}