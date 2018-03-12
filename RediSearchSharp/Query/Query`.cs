using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using RediSearchSharp.Utils;
using StackExchange.Redis;

namespace RediSearchSharp.Query
{
    public interface IMatching<TEntity> where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IMatchingContinuation<TEntity> MustMatch(Term term);
        IMatchingContinuation<TEntity> MustMatch(Term[] terms);
        IMatchingContinuation<TEntity> MustMatch(string term);
        IMatchingContinuation<TEntity> MustMatch(string[] terms);

        IMatchingContinuation<TEntity> MustNotMatch(string term);
        IMatchingContinuation<TEntity> MustNotMatch(string[] terms);
        IMatchingContinuation<TEntity> MustNotMatch(Term term);
        IMatchingContinuation<TEntity> MustNotMatch(Term[] terms);

        IMatchingContinuation<TEntity> ShouldMatch(string term);
        IMatchingContinuation<TEntity> ShouldMatch(string[] terms);
        IMatchingContinuation<TEntity> ShouldMatch(Term term);
        IMatchingContinuation<TEntity> ShouldMatch(Term[] terms);
        
        IMatchingContinuation<TEntity> MustMatch(NumericTerm term);
        IMatchingContinuation<TEntity> MustMatch(NumericTerm[] terms);
        IMatchingContinuation<TEntity> MustNotMatch(NumericTerm term);
        IMatchingContinuation<TEntity> MustNotMatch(NumericTerm[] terms);
        IMatchingContinuation<TEntity> ShouldMatch(NumericTerm term);
        IMatchingContinuation<TEntity> ShouldMatch(NumericTerm[] terms);

        IMatchingContinuation<TEntity> MustMatch(GeoTerm term);
        IMatchingContinuation<TEntity> MustMatch(GeoTerm[] terms);
        IMatchingContinuation<TEntity> MustNotMatch(GeoTerm term);
        IMatchingContinuation<TEntity> MustNotMatch(GeoTerm[] terms);
        IMatchingContinuation<TEntity> ShouldMatch(GeoTerm term);
        IMatchingContinuation<TEntity> ShouldMatch(GeoTerm[] terms);
    }

    public enum SortingOrder
    {
        Ascending,
        Descending
    }

    public interface IQueryOptions<TEntity> where TEntity : RedisearchSerializable<TEntity>, new()
    {   
        IQueryOptions<TEntity> WithId<TProperty>(TProperty id);
        IQueryOptions<TEntity> WithId<TProperty>(TProperty[] ids);
        IQueryOptions<TEntity> WithSlop(int slop);
        IQueryOptions<TEntity> SortBy<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, SortingOrder order = SortingOrder.Ascending);
        IQueryOptions<TEntity> Limit(int offset, int count);
        Query<TEntity> Build();
        Query<TEntity> Build(Action<QueryOptions> optionsBuilder);
    }

    public interface IMatchingContinuation<TEntity> : IMatching<TEntity>, IQueryOptions<TEntity> where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IMatching<TEntity> And();
        IMatching<TEntity> And<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors);
    }
    /// <summary>
    /// Represents a search query to execute and retrieve results from the redisearch engine.
    /// </summary>
    public class Query<TEntity> : IMatchingContinuation<TEntity>
        where TEntity : RedisearchSerializable<TEntity>, new()
    {
        public QueryOptions Options { get; }

        private string _currentFieldKey;
        private readonly StringBuilder _queryBuilder;
        private readonly Dictionary<string, List<Filter>> _filters;
        private int _slop;
        private RedisValue[] _ids;
        private string _sortingBy;
        private SortingOrder _sortingOrder;
        private Paging _paging;

        public Query()
        {
            Options = QueryOptions.DefaultOptions;
            _filters = new Dictionary<string, List<Filter>>();
            _queryBuilder = new StringBuilder();
            _slop = -1;
            _paging = new Paging(0, 10);
        }

        public IMatching<TEntity> Where()
        {
            CreateOrChangeFieldKey<object>(null);
            return this;
        }

        public IMatching<TEntity> Where<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors)
        {
            CreateOrChangeFieldKey(propertySelectors);
            return this;
        }

        IMatching<TEntity> IMatchingContinuation<TEntity>.And()
        {
            CreateOrChangeFieldKey<object>(null);
            return this;
        }
        
        IMatching<TEntity> IMatchingContinuation<TEntity>.And<TProperty>(Expression<Func<TEntity, TProperty>>[] propertySelectors)
        {
            CreateOrChangeFieldKey(propertySelectors);
            return this;
        }

        private void CreateOrChangeFieldKey<TProperty>(params Expression<Func<TEntity, TProperty>>[] propertySelectors)
        {
            _currentFieldKey = propertySelectors == null ?
                               string.Empty :
                               BuildFieldKeyFromPropertySelectors(propertySelectors);
            
            if (_filters.ContainsKey(_currentFieldKey))
            {
                return;
            }
            _filters.Add(_currentFieldKey, new List<Filter>());
        }

        private string BuildFieldKeyFromPropertySelectors<TProperty>(IEnumerable<Expression<Func<TEntity, TProperty>>> propertySelectors)
        {
            var fieldKeyBuilder = new StringBuilder();
            foreach (var propertySelector in propertySelectors)
            {
                fieldKeyBuilder.AppendFormat($"{propertySelector.GetMemberName()}|");
            }

            return fieldKeyBuilder.ToString(0, fieldKeyBuilder.Length - 1);
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(string term)
        {
            return ((IMatching<TEntity>)this).MustMatch(term.AsDefaultTerm());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(string[] terms)
        {
            var termsConverted = terms.Select(t => t.AsDefaultTerm());
            return ((IMatching<TEntity>)this).MustMatch(termsConverted.ToArray());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(Term term)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.Must, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(Term[] terms)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.Must, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(string term)
        {
            return ((IMatching<TEntity>)this).MustNotMatch(term.AsDefaultTerm());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(string[] terms)
        {
            var termsConverted = terms.Select(t => t.AsDefaultTerm());
            return ((IMatching<TEntity>)this).MustNotMatch(termsConverted.ToArray());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(Term term)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.MustNot, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(Term[] terms)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.MustNot, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(string term)
        {
            return ((IMatching<TEntity>)this).ShouldMatch(term.AsDefaultTerm());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(string[] terms)
        {
            var termsConverted = terms.Select(t => t.AsDefaultTerm());
            return ((IMatching<TEntity>)this).ShouldMatch(termsConverted.ToArray());
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(Term term)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.Should, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(Term[] terms)
        {
            _filters[_currentFieldKey].Add(new TextFilter(_currentFieldKey, Filter.FilterTypes.Should, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(NumericTerm term)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.Must, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(NumericTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.Must, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(NumericTerm term)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.MustNot, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(NumericTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.MustNot, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(NumericTerm term)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.Should, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(NumericTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new NumericFilter(_currentFieldKey, Filter.FilterTypes.Should, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(GeoTerm term)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.Must, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustMatch(GeoTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.Must, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(GeoTerm term)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.MustNot, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.MustNotMatch(GeoTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.MustNot, terms));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(GeoTerm term)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.Should, term));
            return this;
        }

        IMatchingContinuation<TEntity> IMatching<TEntity>.ShouldMatch(GeoTerm[] terms)
        {
            _filters[_currentFieldKey].Add(new GeoFilter(_currentFieldKey, Filter.FilterTypes.Should, terms));
            return this;
        }
        
        IQueryOptions<TEntity> IQueryOptions<TEntity>.WithSlop(int slop)
        {
            if (slop < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slop), "Slop value must be at least 0.");
            }

            _slop = slop;
            return this;
        }

        IQueryOptions<TEntity> IQueryOptions<TEntity>.WithId<TProperty>(TProperty id)
        {
            if (id == null)
            {
                throw new ArgumentException("The id must not be null.");
            }

            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            _ids = new [] { schemaMetadata.GetPrimaryKeySelectorFromProperty<TProperty>()(id) };
            return this;
        }

        IQueryOptions<TEntity> IQueryOptions<TEntity>.WithId<TProperty>(TProperty[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentException("The keys array must not be null or empty.");
            }

            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            _ids = ids.Select(id => schemaMetadata.GetPrimaryKeySelectorFromProperty<TProperty>()(id)).ToArray();
            return this;
        }

        IQueryOptions<TEntity> IQueryOptions<TEntity>.SortBy<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, SortingOrder order)
        {   
            _sortingBy = propertySelector.GetMemberName();
            _sortingOrder = order;
            return this;
        }

        IQueryOptions<TEntity> IQueryOptions<TEntity>.Limit(int offset, int count)
        {
            _paging = new Paging(offset, count);
            return this;
        }

        Query<TEntity> IQueryOptions<TEntity>.Build()
        {
            return this;
        }

        Query<TEntity> IQueryOptions<TEntity>.Build(Action<QueryOptions> optionsBuilder)
        {
            optionsBuilder(Options);
            return this;
        }
        
        public void SerializeRedisArgs(List<object> args)
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();

            args.Add(BuildQueryString());

            if (Options.Verbatim)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("VERBATIM"));
            }

            if (Options.DisableStopwordFiltering)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("NOSTOPWORDS"));
            }

            if (Options.WithScores)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("WITHSCORES"));
            }

            if (Options.WithPayloads)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("WITHPAYLOADS"));
            }

            if (Options.WithScoreKeys)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("WITHSCOREKEYS"));
            }

            if (_ids != null && _ids.Length != 0)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("INKEYS"));
                args.Add(_ids.Length);

                foreach (var key in _ids.Select(id => string.Concat(schemaMetadata.DocumentIdPrefix, id)))
                {
                    args.Add(key);
                }
            }
            
            if (_slop > -1)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("SLOP"));
                args.Add(_slop);
            }

            if (Options.InOrder)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("INORDER"));
            }

            args.Add(RedisearchIndexCache.GetBoxedLiteral("LANGUAGE"));
            args.Add(string.IsNullOrEmpty(Options.Language)
                ? schemaMetadata.Language
                : RedisearchIndexCache.GetBoxedLiteral(Options.Language));

            if (_sortingBy != null)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("SORTBY"));
                args.Add(_sortingBy);

                args.Add(_sortingOrder == SortingOrder.Ascending ? RedisearchIndexCache.GetBoxedLiteral("ASC") : RedisearchIndexCache.GetBoxedLiteral(
                    "DESC"));
            }

            if (_paging.Offset != 0 || _paging.Count != 10)
            {
                args.Add(RedisearchIndexCache.GetBoxedLiteral("LIMIT"));
                args.Add(_paging.Offset);
                args.Add(_paging.Count);
            }
        }

        private string BuildQueryString()
        {
            foreach (var fields in _filters)
            {
                foreach (var filter in fields.Value)
                {
                    filter.SerializeQuery(_queryBuilder, Options);
                }
            }
            return _queryBuilder.ToString();
        }

        private abstract class Filter
        {
            public enum FilterTypes
            {
                Must,
                MustNot,
                Should
            };

            protected string FieldName { get; }
            
            protected FilterTypes FilterType { get; }

            protected Filter(string fieldName, FilterTypes filterType)

            {
                FieldName = fieldName;
                FilterType = filterType;
            }

            public abstract void SerializeQuery(StringBuilder queryBuilder, QueryOptions queryOptions);
        }

        private class TextFilter : Filter
        {
            private Term[] Terms { get; }
            public TextFilter(string fieldName, FilterTypes filterType, params Term[] terms) : base(fieldName, filterType)
            {
                Terms = terms;
            }

            public override void SerializeQuery(StringBuilder queryBuilder, QueryOptions queryOptions)
            {
                queryBuilder.Append("(");
                if (FilterType == FilterTypes.MustNot)
                {
                    queryBuilder.Append("-");
                }

                if (FilterType == FilterTypes.Should)
                {
                    queryBuilder.Append("~");
                }

                if (!string.IsNullOrEmpty(FieldName))
                {
                    queryBuilder.Append($"@{FieldName}:");
                }
                
                queryBuilder.Append(string.Join("|", Terms.Select(t => t.GetValue(queryOptions.DefaultTermResolvingStrategy))));

                queryBuilder.Append(") ");
            }
        }

        private class NumericFilter : Filter
        {
            private NumericTerm[] Terms { get; }

            public NumericFilter(string fieldName, FilterTypes filterType, params NumericTerm[] terms) : base(fieldName, filterType)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    throw new InvalidOperationException("The field name cannot be empty in numeric queries");
                }
                Terms = terms;
            }

            private void AddNumberToQuery(StringBuilder queryBuilder, double number, bool exclusive)
            {
                if (exclusive)
                {
                    queryBuilder.Append("(");
                }
                if (double.IsNegativeInfinity(number))
                {
                    queryBuilder.Append("-inf");
                }
                else if (double.IsPositiveInfinity(number))
                {
                    queryBuilder.Append("inf");
                }
                else
                {
                    queryBuilder.Append(number.ToString("G17", CultureInfo.InvariantCulture));
                }
            }
            
            private void AddTermToQuery(StringBuilder queryBuilder, NumericTerm term)
            {
                queryBuilder.Append($"@{FieldName}:");
                queryBuilder.Append("[");
                AddNumberToQuery(queryBuilder, term.Min, term.ExclusiveMin);
                queryBuilder.Append(" ");
                AddNumberToQuery(queryBuilder, term.Max, term.ExclusiveMax);
                queryBuilder.Append("] | ");
            }
            
            public override void SerializeQuery(StringBuilder queryBuilder, QueryOptions queryOptions)
            {
                queryBuilder.Append("(");
                if (FilterType == FilterTypes.MustNot)
                {
                    queryBuilder.Append("-");
                }

                if (FilterType == FilterTypes.Should)
                {
                    queryBuilder.Append("~");
                }

                foreach (var numericTerm in Terms)
                {
                    AddTermToQuery(queryBuilder, numericTerm);
                }

                queryBuilder.Length -= 2;
                queryBuilder.Append(") ");
            }
        }

        private class GeoFilter : Filter
        {
            private GeoTerm[] Terms { get; }
            public GeoFilter(string fieldName, FilterTypes filterType, params GeoTerm[] terms) : base(fieldName, filterType)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    throw new InvalidOperationException("The field name cannot be empty in geoqueries");
                }
                Terms = terms;
            }

            public override void SerializeQuery(StringBuilder queryBuilder, QueryOptions queryOptions)
            {
                queryBuilder.Append("(");
                if (FilterType == FilterTypes.MustNot)
                {
                    queryBuilder.Append("-");
                }

                if (FilterType == FilterTypes.Should)
                {
                    queryBuilder.Append("~");
                }

                foreach (var geoTerm in Terms)
                {
                    AddTermToQuery(queryBuilder, geoTerm);
                }

                queryBuilder.Length -= 2;
                queryBuilder.Append(") ");
            }

            private void AddTermToQuery(StringBuilder queryBuilder, GeoTerm term)
            {
                queryBuilder.Append($"@{FieldName}:");
                queryBuilder.Append(term.GetValue());
                queryBuilder.Append(" | ");
            }
        }

        private struct Paging
        {
            public int Offset { get; }
            public int Count { get; }

            public Paging(int offset, int count)
            {
                Offset = offset;
                Count = count;
            }
        }
    }
}