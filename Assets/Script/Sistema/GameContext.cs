using UnityEngine;

public static class GameContext
{
    public static int currentSlot;
    public static GameMode gameMode = GameMode.SINGLEPLAYER;
    public static bool loadFromSave = false;
}
