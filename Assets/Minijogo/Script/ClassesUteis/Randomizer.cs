using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Randomizer
{
    /// <summary>
    /// Recebe um dicionário int - cor e retorna uma lista aleatória
    /// </summary>
    /// <param name="nameColors"></param>
    /// <returns></returns>
    public List<int> ListRandomizer(Dictionary<int, string> nameColors)
    {
        List<int> list = nameColors.Keys.ToList();
        System.Random rng = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }
    public int NumbRandomizer(int lastNumb, int range)
    {
        int sortNumb = Random.Range(0, range);
        while (sortNumb == lastNumb)
        {
            sortNumb = Random.Range(0, range);
        }
        ;
        return sortNumb;
    }

}
