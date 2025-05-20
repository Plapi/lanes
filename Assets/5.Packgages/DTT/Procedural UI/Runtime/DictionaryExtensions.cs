using System;
using System.Collections.Generic;

namespace DTT.UI.ProceduralUI
{
    /// <summary>
    /// Extension class for the dictionary.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the object in the dictionary if the given object is equal to an object in the dictionary 
        /// following the comparer as check.
        /// </summary>
        /// <typeparam name="TKey"> The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue"> The value type of the dictionary.</typeparam>
        /// <param name="dictionary"> The dictionary.</param>
        /// <param name="originalKey"> The original key.</param>
        /// <param name="comparer"> The function that checks if there equal.</param>
        /// <returns>The object found to be equal.</returns>
        public static TKey GetComparableKey<TKey,TValue>(
            this Dictionary<TKey,TValue> dictionary, 
            TKey originalKey, 
            Func<TKey,TKey,bool> comparer )
        {
            foreach(TKey key in dictionary.Keys)
            {
                if(comparer(key, originalKey))
                {
                    return key;
                }
            }
            return default;
        }
    }
}