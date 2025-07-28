using System;
using System.Collections.Generic;
using UnityEngine;

public class WeightedTable<T>
{
    [Serializable]
    public struct Entry
    {
        public float weight;
        public T thing;

        public Entry(float peso, T coisa)
        {
            weight = peso;
            thing = coisa;
        }
    }

    private List<Entry> items = new();

    public void AddEntry(T objeto, float peso)
    {
        foreach (Entry entry in items)
        {
            if (EqualityComparer<T>.Default.Equals(entry.thing, objeto))
            {
                Debug.Log("Já existe um objeto igual");
                return;
            }
        }

        items.Add(new Entry(peso, objeto));
    }

    public void ModifyEntry(T objeto, float novo_peso)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(items[i].thing, objeto))
            {
                items[i] = new Entry(novo_peso, objeto);
                return;
            }
        }
    }

    public void RemoveEntry(T objeto)
    {
        items.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.thing, objeto));
    }

    public T PickEntry()
    {
        float totalWeight = 0f;
        foreach (var entry in items)
        {
            totalWeight += entry.weight;
        }

        float r = UnityEngine.Random.Range(0f, totalWeight);
        float sum = 0f;

        foreach (var entry in items)
        {
            sum += entry.weight;
            if (r <= sum)
            {
                return entry.thing;
            }
        }

        return default;
    }
}
