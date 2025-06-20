using System;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public abstract class ComponentBehaviour : MonoBehaviour
{
    public event Action<string, object> OnAttributeChanged;
    public void SubscribeToAttribute(string attributeName, Action<object> callback)
    {
        OnAttributeChanged += (name, value) =>
        {
            if (name == attributeName) callback(value);
        };
    }

    protected Dictionary<string, object> attributes = new();
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







    [Serializable]
    public struct Entities
    {
        public EntityType TipoEntidade;
        public Sub_EnemyType TipoInimigo;
    }


    public enum EntityType
    {
        PLAYER, ENEMY, ENTITY
    }
    public enum Sub_EnemyType
    {
        NONE, SIMPLE, RANGED, FLYING, TANK
    }
    public enum ErrorType
    {
        SUCCESS,ATTRIBUTE_ERROR,COMPONENT_ERROR
    }
}
