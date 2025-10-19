using System;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static CollectibleManager Instance { get; private set; }

    public Text collectableCountText; // Assign your UI Text element in the Inspector
    private int currentCollectables = 0;

    public event Action<int> OnColletableCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    public void AddColletable(int amount = 1)
    {
        currentCollectables += amount;
        UpdateCollectableText();
        OnColletableCountChanged?.Invoke(currentCollectables);
    }

    private void UpdateCollectableText()
    {
        if (collectableCountText != null)
            collectableCountText.text = currentCollectables.ToString("00");
    }

    public int GetCurrentColletables()
    {
        return currentCollectables;
    }

    public void ResetColletables()
    {
        currentCollectables = 0;
        UpdateCollectableText();
        OnColletableCountChanged?.Invoke(currentCollectables);
    }

    // Initialize the text when the game starts
    private void Start()
    {
        UpdateCollectableText();
    }
}
