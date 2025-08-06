using UnityEngine;
using UnityEngine.UI;

public class CollectibleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static CollectibleManager Instance { get; private set; }

    public Text collectableCountText; // Assign your UI Text element in the Inspector
    private int currentCollectables = 0;

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

    public void AddCollectable()
    {
        currentCollectables++;
        UpdateCollectableText();
    }

    private void UpdateCollectableText()
    {
        if (collectableCountText != null)
        {
            collectableCountText.text = "Collectables: " + currentCollectables.ToString();
        }
    }

    // Initialize the text when the game starts
    private void Start()
    {
        UpdateCollectableText();
    }
}
