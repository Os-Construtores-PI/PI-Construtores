using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private DataSystem dataSystem;
    private void Start()
    {
        if (!TryGetComponent(out dataSystem)) return;
        dataSystem.Load();
    }

    public void ShutdownWorld()
    {

    }
}
