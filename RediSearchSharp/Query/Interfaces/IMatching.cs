using RediSearchSharp.Serialization;

namespace RediSearchSharp.Query.Interfaces
{
    public interface IMatching<TEntity> where TEntity : RedisearchSerializable<TEntity>, new()
    {
        IAndOrOptions<TEntity> MustMatch(Term term);
        IAndOrOptions<TEntity> MustMatch(Term[] terms);
        IAndOrOptions<TEntity> MustMatch(string term);
        IAndOrOptions<TEntity> MustMatch(string[] terms);

        IAndOrOptions<TEntity> MustNotMatch(string term);
        IAndOrOptions<TEntity> MustNotMatch(string[] terms);
        IAndOrOptions<TEntity> MustNotMatch(Term term);
        IAndOrOptions<TEntity> MustNotMatch(Term[] terms);

        IAndOrOptions<TEntity> ShouldMatch(string term);
        IAndOrOptions<TEntity> ShouldMatch(string[] terms);
        IAndOrOptions<TEntity> ShouldMatch(Term term);
        IAndOrOptions<TEntity> ShouldMatch(Term[] terms);
        
        IAndOrOptions<TEntity> MustMatch(NumericTerm term);
        IAndOrOptions<TEntity> MustMatch(NumericTerm[] terms);
        IAndOrOptions<TEntity> MustNotMatch(NumericTerm term);
        IAndOrOptions<TEntity> MustNotMatch(NumericTerm[] terms);
        IAndOrOptions<TEntity> ShouldMatch(NumericTerm term);
        IAndOrOptions<TEntity> ShouldMatch(NumericTerm[] terms);

        IAndOrOptions<TEntity> MustMatch(GeoTerm term);
        IAndOrOptions<TEntity> MustMatch(GeoTerm[] terms);
        IAndOrOptions<TEntity> MustNotMatch(GeoTerm term);
        IAndOrOptions<TEntity> MustNotMatch(GeoTerm[] terms);
        IAndOrOptions<TEntity> ShouldMatch(GeoTerm term);
        IAndOrOptions<TEntity> ShouldMatch(GeoTerm[] terms);
    }
}