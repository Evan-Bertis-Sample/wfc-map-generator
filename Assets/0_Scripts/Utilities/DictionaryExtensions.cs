using UnityEngine;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void Print<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            Debug.Log(pair.Key + ": " + pair.Value);
        }
    }
}