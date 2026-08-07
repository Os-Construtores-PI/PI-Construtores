using UnityEngine;

public class FPSDirector : MonoBehaviour
{
  [Header("Configurações")]
  [SerializeField, Tooltip("FPS alvo. Use -1 para ilimitado, 0 para padrão da plataforma.")]
  private int targetFPS = 60;

  [SerializeField, Tooltip("Ativar VSync. Em desktop, VSync sobrescreve targetFPS.")]
  private bool useVSync = true;

  [
    SerializeField,
    Tooltip("Contagem de VSync (1 = a cada refresh, 2 = a cada 2 refreshes, etc). Máx: 4.")
  ]
  [Range(0, 4)]
  private int vSyncCount = 1;

  [Header("Opções Adicionais")]
  [SerializeField, Tooltip("Limitar FPS quando a janela perde o foco.")]
  private bool limitFPSOnBackground = true;

  [SerializeField, Tooltip("FPS quando a janela está em segundo plano.")]
  private int backgroundFPS = 30;

  [SerializeField, Tooltip("Exibir FPS atual no console (a cada segundo).")]
  private bool showFPSInConsole = false;

  public int CurrentTargetFPS => Application.targetFrameRate;
  public bool IsVSyncEnabled => QualitySettings.vSyncCount > 0;
  public int CurrentVSyncCount => QualitySettings.vSyncCount;
  public bool IsLimitingBackground => limitFPSOnBackground;

  private float _fpsTimer;
  private int _frameCount;
  private int _lastFPS;

  void Start()
  {
    ApplySettings();
  }

  void Update()
  {
    if (showFPSInConsole)
    {
      _frameCount++;
      _fpsTimer += Time.unscaledDeltaTime;

      if (_fpsTimer >= 1f)
      {
        _lastFPS = _frameCount;
        Debug.Log($"[FPSDirector] FPS Atual: {_lastFPS}");
        _frameCount = 0;
        _fpsTimer = 0f;
      }
    }
  }

  void OnApplicationFocus(bool hasFocus)
  {
    if (!limitFPSOnBackground)
      return;

    if (hasFocus)
    {
      ApplySettings();
    }
    else
    {
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = backgroundFPS;
      Debug.Log($"[FPSDirector] Janela em segundo plano. FPS limitado a {backgroundFPS}");
    }
  }

  // ============================================
  // FUNÇÕES PÚBLICAS — Ligue em botões da UI
  // ============================================

  public void SetTargetFPS(int fps)
  {
    targetFPS = fps;
    ApplySettings();
    Debug.Log($"[FPSDirector] Target FPS definido: {fps}");
  }

  public void SetVSync(bool enabled)
  {
    useVSync = enabled;
    ApplySettings();
    Debug.Log($"[FPSDirector] VSync: {(enabled ? "ON" : "OFF")}");
  }

  public void SetVSyncCount(int count)
  {
    vSyncCount = Mathf.Clamp(count, 0, 4);
    if (useVSync)
    {
      ApplySettings();
    }
    Debug.Log($"[FPSDirector] VSync Count: {vSyncCount}");
  }

  public void ToggleVSync()
  {
    SetVSync(!useVSync);
  }

  public void SetBackgroundLimit(bool enabled)
  {
    limitFPSOnBackground = enabled;
    Debug.Log($"[FPSDirector] Limite em background: {(enabled ? "ON" : "OFF")}");
  }

  public void SetBackgroundFPS(int fps)
  {
    backgroundFPS = Mathf.Max(1, fps);
    Debug.Log($"[FPSDirector] Background FPS: {backgroundFPS}");
  }

  public void SetShowFPS(bool show)
  {
    showFPSInConsole = show;
    if (!show)
    {
      _frameCount = 0;
      _fpsTimer = 0f;
    }
  }

  public int GetCurrentMeasuredFPS()
  {
    return _lastFPS;
  }

  public void SetPresetUnlimited() => SetTargetFPS(-1);

  public void SetPreset30FPS() => SetTargetFPS(30);

  public void SetPreset60FPS() => SetTargetFPS(60);

  public void SetPreset120FPS() => SetTargetFPS(120);

  public void SetPreset144FPS() => SetTargetFPS(144);

  public void ApplySettings()
  {
    if (useVSync)
    {
      QualitySettings.vSyncCount = Mathf.Clamp(vSyncCount, 1, 4);
      Application.targetFrameRate = -1;
    }
    else
    {
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = targetFPS;
    }
  }

  public void ResetToDefaults()
  {
    targetFPS = 60;
    useVSync = true;
    vSyncCount = 1;
    limitFPSOnBackground = true;
    backgroundFPS = 30;
    showFPSInConsole = false;
    ApplySettings();
    Debug.Log("[FPSDirector] Configurações resetadas para padrão.");
  }
}
