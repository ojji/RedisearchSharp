using System;
using System.Linq;

namespace RediSearchSharp.Query
{
    public interface ITermNormalizer
    {
        string NormalizeTerm(string value);
    }

    internal class TermNormalizer : ITermNormalizer
    {
        public string NormalizeTerm(string value)
        {
            return new String(value.Select(c => Char.IsLetterOrDigit(c) ? c : ' ').ToArray());
        }
    }

    /// <summary>
    /// Encapsulates a textual search term.
    /// </summary>
    public struct Term
    {
        public string Value { get; }
        public bool IsExact { get; }
        public bool IsDefault { get; }
        public ITermNormalizer TermNormalizer { get; }
        private static readonly ITermNormalizer DefaultTermNormalizer = new TermNormalizer();

        internal Term(string value, bool isDefault, bool isExact, ITermNormalizer termNormalizer)
        {
            Value = value;
            IsDefault = isDefault;
            IsExact = isExact;
            TermNormalizer = termNormalizer;
        }

        /// <summary>
        /// Creates an expanded text search term.
        /// </summary>
        /// <param name="value">The text value of the term.</param>
        /// <returns>An expanded text term to be used in a query.</returns>
        public static Term Create(string value)
        {
            return new Term(value, false, false, DefaultTermNormalizer);
        }

        /// <summary>
        /// Creates an exact text search term.
        /// </summary>
        /// <param name="value">The text value of the term.</param>
        /// <returns>An exact text term to be used in a query.</returns>
        public static Term CreateExact(string value)
        {
            return new Term(value, false, true, DefaultTermNormalizer);
        }

        internal static Term CreateDefault(string value)
        {
            return new Term(value, true, false, DefaultTermNormalizer);
        }
        
        internal string GetValue(TermResolvingStrategy termResolvingStrategy)
        {
            // if the term is not default we ignore the strategy param
            // if the term is default we use the strategy param value
            var useExact = (!IsDefault && IsExact) ||
                           (IsDefault && termResolvingStrategy == TermResolvingStrategy.Exact);

            if (useExact)
            {
                return $"\"{TermNormalizer.NormalizeTerm(Value)}\"";
            }

            return TermNormalizer.NormalizeTerm(Value);
        }
    }
}