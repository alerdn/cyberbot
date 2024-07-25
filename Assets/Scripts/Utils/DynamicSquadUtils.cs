using System.Collections.Generic;
using UnityEngine;

public static class DynamicSquadUtils
{

    public static T GetRandom<T>(this List<T> list, int initialIndex = 0)
    {
        return list[Random.Range(initialIndex, list.Count)];
    }

    public static int GetRandomIndex<T>(this List<T> list, int initialIndex = 0)
    {
        return Random.Range(initialIndex, list.Count);
    }
}