using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SaveSystem
{
    public void SaveInt(string chave, int valor)
    {
        PlayerPrefs.SetInt(chave, valor);
    }
    public int LoadInt(string chave)
    {
        return PlayerPrefs.GetInt(chave);
    }
}