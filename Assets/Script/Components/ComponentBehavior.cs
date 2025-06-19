using System;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public abstract class ComponentBehaviour : MonoBehaviour
{

    protected Dictionary<string, object> attributes = new Dictionary<string, object>();
    public bool TryGetAttribute<T>(string attributeName, out T value)
    {
        if (attributes.TryGetValue(attributeName, out object objValue) && objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default;
        return false;
    }
    public T GetAttribute<T>(string attributeName)
    {
        if (attributes.TryGetValue(attributeName, out object value))
        {
            return (T)value;
        }
        throw new Exception($"Atributo {attributeName} não encontrado");
    }
    public void SetAttribute(string attributeName, object value)
    {
        if (string.IsNullOrEmpty(attributeName))
        {
            throw new ArgumentException("Nome do atributo não pode ser vazio");
        }

        attributes[attributeName] = value;
        OnAttributeChanged?.Invoke(attributeName, value);
    }



    public event Action<string, object> OnAttributeChanged;
    public void SubscribeToAttribute(string attributeName, Action<object> callback)
    {
        OnAttributeChanged += (name, value) =>
        {
            if (name == attributeName) callback(value);
        };
    }




    [Serializable]
    public struct Entities
    {
        public EntityType entityType;
        public Sub_EnemyType enemyType;
    }


    public enum EntityType
    {
        player, enemy, entity
    }
    public enum Sub_EnemyType
    {
        none,simple, ranged, flying, tank
    }
    public enum StatType
    {
        armor, attack, speed, jump
    }

}
