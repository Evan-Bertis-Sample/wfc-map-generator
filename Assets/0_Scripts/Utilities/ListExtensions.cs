using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class ListExtensions
{

    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = UnityEngine.Random.Range(0, n);
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static bool HaveSameElements<T>(this List<T> list1, List<T> list2)
    {
        if (list1 == null || list2 == null)
        {
            return false;
        }

        if (list1.Count != list2.Count)
        {
            return false;
        }

        var frequency1 = GetFrequency(list1);
        var frequency2 = GetFrequency(list2);

        foreach (var kvp in frequency1)
        {
            if (!frequency2.ContainsKey(kvp.Key) || frequency2[kvp.Key] != kvp.Value)
            {
                return false;
            }
        }

        return true;
    }

    public static void RemoveItemsNotInList<T>(this List<T> list, List<T> validItems)
    {
        int numThreads = Environment.ProcessorCount;
        int batchsize = list.Count / numThreads;

        List<Task> tasks = new List<Task>();
        List<T> filteredList = new List<T>();
        object lockObject = new object();

        for (int i = 0; i < numThreads; i++)
        {
            int startIndex = i * batchsize;
            int endIndex = (i == numThreads - 1) ? list.Count : (i + 1) * batchsize;
            Task task = Task.Run(() =>
            {
                for(int j = endIndex - 1; j >= startIndex; j--)
                {
                    if (validItems.Contains(list[j]))
                    {
                        lock (lockObject)
                        {
                            filteredList.Add(list[j]);
                        }
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());

        lock (lockObject)
        {
            list = filteredList;
        }
    }


    private static Dictionary<T, int> GetFrequency<T>(List<T> list)
    {
        var frequency = new Dictionary<T, int>();

        foreach (var item in list)
        {
            if (!frequency.ContainsKey(item))
            {
                frequency[item] = 0;
            }

            frequency[item]++;
        }

        return frequency;
    }
}
