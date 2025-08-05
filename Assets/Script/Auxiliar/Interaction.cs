using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionKeyMap
{
    private Dictionary<string, Type> keyToAction = new();
    private Dictionary<Type, string> typeToKey = new();

    // Tenta adicionar um mapeamento único
    public bool BindKey(string key, Type action)
    {
        // Garante que nem a tecla nem a ação já foram usadas
        if (keyToAction.ContainsKey(key) || typeToKey.ContainsKey(action))
            return false;

        keyToAction[key] = action;
        typeToKey[action] = key;
        return true;
    }

    // Altera a tecla de uma ação existente
    public bool RebindKey(Type typeobj, string newKey)
    {
        if (!typeToKey.TryGetValue(typeobj, out var oldKey))
            return false;

        if (keyToAction.ContainsKey(newKey))
            return false; // nova tecla já usada

        // Remove mapeamentos antigos
        keyToAction.Remove(oldKey);
        typeToKey.Remove(typeobj);

        // Adiciona novos
        return BindKey(newKey, typeobj);
    }

    // Consulta
    public bool TryGetAction(string key, out Type type) => keyToAction.TryGetValue(key, out type);
    public bool TryGetKey(Type type, out string key) => typeToKey.TryGetValue(type, out key);

    public void PrintMappings()
    {
        foreach (var pair in keyToAction)
        {
            Debug.Log($"{pair.Key} => {pair.Value}");
        }
    }
}
