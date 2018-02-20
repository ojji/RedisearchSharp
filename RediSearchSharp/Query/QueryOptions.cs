using System;

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
        public virtual bool WithoutContent { get; set; }
        public bool WithScores { get; set; }
        public bool WithScoreKeys { get; set; }
        public bool WithPayloads { get; set; }
        public bool DisableStopwordFiltering { get; set; }
        public TermResolvingStrategies DefaultTermResolvingStrategy { get; set; }
        public bool InOrder { get; set; }
        public string Language { get; set; }

        public static readonly QueryOptions DefaultOptions = new QueryOptions
        {
            Verbatim = false,
            WithoutContent = false,
            WithScores = false,
            WithScoreKeys = false,
            WithPayloads = false,
            DisableStopwordFiltering = false,
            DefaultTermResolvingStrategy = TermResolvingStrategies.Exact,
            InOrder = false,
            Language = Languages.Default
        };
    }

    public class TypedQueryOptions : QueryOptions
    {
        public override bool WithoutContent
        {
            get => false;
            set => throw new InvalidOperationException("You cannot set NOCONTENT on a typed query.");
        }

        public new static readonly TypedQueryOptions DefaultOptions = new TypedQueryOptions
        {
            Verbatim = false,
            WithScores = false,
            WithScoreKeys = false,
            WithPayloads = false,
            DisableStopwordFiltering = false,
            DefaultTermResolvingStrategy = TermResolvingStrategies.Exact,
            InOrder = false,
            Language = Languages.Default
        };
    }
}