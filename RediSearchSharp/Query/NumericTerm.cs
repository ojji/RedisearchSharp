namespace RediSearchSharp.Query
{
    /// <summary>
    /// Encapsulates a numeric search term.
    /// </summary>
    public struct NumericTerm
    {
        public double Min { get; }
        public double Max { get; }
        public bool ExclusiveMin { get; }
        public bool ExclusiveMax { get; }
        
        private NumericTerm(double min, double max, bool exclusiveMin, bool exclusiveMax)
        {
            Min = min;
            Max = max;
            ExclusiveMin = exclusiveMin;
            ExclusiveMax = exclusiveMax;
        }

        /// <summary>
        /// Creates a numeric search term.
        /// </summary>
        /// <param name="min">The minimum value of the numeric term (inclusive).</param>
        /// <param name="max">The maximum value of the numeric term (inclusive).</param>
        /// <returns>A numeric search term matching the supplied range.</returns>
        public static NumericTerm Between(double min, double max)
        {
            return new NumericTerm(min, max, false, false);
        }

        /// <summary>
        /// Creates a numeric search term.
        /// </summary>
        /// <param name="min">The minimum value of the numeric term.</param>
        /// <param name="max">The maximum value of the numeric term.</param>
        /// <param name="exclusiveMin">If set to true, the minimum value will be excluded from the range.</param>
        /// <param name="exclusiveMax">If set to true, the maximum value will be excluded from the range.</param>
        /// <returns>A numeric search term matching the supplied range.</returns>
        public static NumericTerm Between(double min, double max, bool exclusiveMin, bool exclusiveMax)
        {
            return new NumericTerm(min, max, exclusiveMin, exclusiveMax);
        }
    }
}