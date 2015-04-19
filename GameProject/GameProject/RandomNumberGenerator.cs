using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameProject
{
    /// <summary>
    /// Provides a single generator for random numbers throughout the game
    /// </summary>
    public static class RandomNumberGenerator
    {
        #region Fields

        static Random rand;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the random number generator
        /// </summary>
        public static void Initialize()
        {
            rand = new Random();
        }

        /// <summary>
        /// Returns a nonnegative random number
        /// </summary>
        /// <returns>the random number</returns>
        //public static int Next()
        //{
        //    return rand.Next();
        //}

        /// <summary>
        /// Returns a nonnegative random number less than maxValue (exclusive)
        /// </summary>
        /// <param name="maxValue">the exclusive max value</param>
        /// <returns>the random number</returns>
        public static int Next(int maxValue)
        {
            return rand.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number between minValue (inclusive) and maxValue (exclusive)
        /// </summary>
        /// <param name="minValue">inclusive min value</param>
        /// <param name="maxValue">exclusive max value</param>
        /// <returns>the random number</returns>
        //public static int Next(int minValue, int maxValue)
        //{
        //    return rand.Next(minValue, maxValue);
        //}

        /// <summary>
        /// Returns a nonnegative random number less than maxValue (exclusive)
        /// </summary>
        /// <param name="maxValue">the exclusive max value</param>
        /// <returns>the random number</returns>
        public static float NextFloat(float maxValue)
        {
            return (float)rand.NextDouble() * maxValue;
        }

        /// <summary>
        /// Returns a random number between minValue (inclusive) and maxValue (inclusive)
        /// </summary>
        /// <param name="minValue">inclusive min value</param>
        /// <param name="maxValue">inclusive max value</param>
        /// <returns>the random number</returns>
        //public static float NextFloat(float minValue, float maxValue)
        //{
        //    return minValue + (float)rand.NextDouble() * (maxValue - minValue);
        //}

        /// <summary>
        /// Returns a random number between 0.0 and 1.0
        /// </summary>
        /// <returns>the random number</returns>
        public static double NextDouble()
        {
            return rand.NextDouble();
        }

        #endregion
    }
}
