using System;
using System.Collections.Generic;
using UnityEngine;

// Classe base abstrata que estende MonoBehaviour para criar componentes
// que armazenam atributos dinâmicos e notificam mudanças via eventos
public abstract class ComponentBehaviour : MonoBehaviour
{
    // Evento disparado quando um atributo é alterado, enviando nome e novo valor
    public event Action<string, object> OnAttributeChanged;

    // Método para se inscrever em mudanças de um atributo específico,
    // recebendo callback com o novo valor quando o atributo mudar
    public void SubscribeToAttribute(string attributeName, Action<object> callback)
    {
        OnAttributeChanged += (name, value) =>
        {
            if (name == attributeName) callback(value);
        };
    }

    // Dicionário interno que armazena pares atributo-nome / valor genérico
    protected Dictionary<string, object> attributes = new();

    // Método seguro para tentar obter um atributo de tipo genérico T
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

    // Método para obter um atributo já tipado, lança exceção se não encontrar
    public T GetAttribute<T>(string attributeName)
    {
        if (attributes.TryGetValue(attributeName, out object value))
        {
            return (T)value;
        }
        throw new Exception($"Atributo {attributeName} não encontrado");
    }

    // Define ou atualiza o valor de um atributo e dispara o evento de mudança
    public void SetAttribute(string attributeName, object value)
    {
        if (string.IsNullOrEmpty(attributeName))
        {
            throw new ArgumentException("Nome do atributo não pode ser vazio");
        }

        attributes[attributeName] = value;

        // Notifica os inscritos que o atributo mudou
        OnAttributeChanged?.Invoke(attributeName, value);
    }
}
