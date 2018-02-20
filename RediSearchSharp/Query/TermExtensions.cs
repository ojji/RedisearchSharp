namespace RediSearchSharp.Query
{
    public static class TermExtensions
    {
        /// <summary>
        /// Creates an expanded <see cref="Term"/> from the string value.
        /// </summary>
        /// <param name="termValue">The text value of the expanded term.</param>
        /// <returns>An expanded text term to be used in a query.</returns>
        public static Term AsExpandedTerm(this string termValue)
        {
            return Term.Create(termValue);
        }

        /// <summary>
        /// Creates an exact <see cref="Term"/> from the string value.
        /// </summary>
        /// <param name="termValue">The text value of the exact term.</param>
        /// <returns>An exact text term to be used in a query.</returns>
        public static Term AsExactTerm(this string termValue)
        {
            return Term.CreateExact(termValue);
        }

        internal static Term AsDefaultTerm(this string termValue)
        {
            return Term.CreateDefault(termValue);
        }
    }
}