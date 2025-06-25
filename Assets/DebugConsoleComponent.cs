using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Console de debug com suporte a múltiplos ComponentBehaviour.
/// </summary>
public class DebugConsoleComponent : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private GameObject consoleUI;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI logText;

    [Header("Objeto Alvo")]
    [SerializeField] private GameObject targetObject;

    private bool isConsoleOpen = false;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (targetObject.TryGetComponent(out PlayerInput input))
            playerInput = input;
    }

    private void Update()
    {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            ToggleConsole();

        if (isConsoleOpen && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                ExecuteCommand(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }

        if (isConsoleOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseConsole();
    }

    private void ToggleConsole()
    {
        if (isConsoleOpen) CloseConsole();
        else OpenConsole();
    }

    private void OpenConsole()
    {
        isConsoleOpen = true;
        consoleUI.SetActive(true);
        inputField.ActivateInputField();
        if (playerInput) playerInput.enabled = false;
    }

    private void CloseConsole()
    {
        isConsoleOpen = false;
        consoleUI.SetActive(false);
        if (playerInput) playerInput.enabled = true;
    }

    private void ExecuteCommand(string command)
    {
        string[] args = command.Trim().Split(' ');

        if (args.Length == 0) return;

        switch (args[0].ToLower())
        {
            case "/set":
                if (args.Length < 3) { AppendLog("Uso: /set [atributo] [valor]"); return; }

                string setAttr = args[1];
                string rawValue = args[2];
                var components = targetObject.GetComponents<ComponentBehaviour>();
                bool found = false;

                foreach (var cb in components)
                {
                    if (cb.TryGetAttribute<object>(setAttr, out _))
                    {
                        if (float.TryParse(rawValue, out float f))
                        {
                            cb.SetAttribute(setAttr, f);
                            AppendLog($"[float] {setAttr} = {f}");
                        }
                        else if (bool.TryParse(rawValue.ToLower(), out bool b))
                        {
                            cb.SetAttribute(setAttr, b);
                            AppendLog($"[bool] {setAttr} = {b}");
                        }
                        else
                        {
                            cb.SetAttribute(setAttr, rawValue);
                            AppendLog($"[string] {setAttr} = {rawValue}");
                        }
                        found = true;
                        break;
                    }
                }

                // Caso não exista ainda, cria no primeiro componente
                if (!found && components.Length > 0)
                {
                    var cb = components[0];
                    if (float.TryParse(rawValue, out float f))
                        cb.SetAttribute(setAttr, f);
                    else if (bool.TryParse(rawValue.ToLower(), out bool b))
                        cb.SetAttribute(setAttr, b);
                    else
                        cb.SetAttribute(setAttr, rawValue);

                    AppendLog($"Atributo '{setAttr}' criado no primeiro componente.");
                }

                break;

            case "/get":
                if (args.Length < 2) { AppendLog("Uso: /get [atributo]"); return; }

                string getAttr = args[1];
                var comps = targetObject.GetComponents<ComponentBehaviour>();
                foreach (var cb in comps)
                {
                    if (cb.TryGetAttribute<float>(getAttr, out float f))
                    {
                        AppendLog($"[float] {getAttr} = {f}");
                        return;
                    }
                    if (cb.TryGetAttribute<bool>(getAttr, out bool b))
                    {
                        AppendLog($"[bool] {getAttr} = {b}");
                        return;
                    }
                    if (cb.TryGetAttribute<string>(getAttr, out string s))
                    {
                        AppendLog($"[string] {getAttr} = {s}");
                        return;
                    }
                }
                AppendLog($"Atributo '{getAttr}' não encontrado.");
                break;

            case "/help":
                AppendLog("Comandos:\n/set [attr] [valor]\n/get [attr]\n/applystat [StatType] [QualityTier] [PERMANENT|TEMPORARY] [duração] [cooldown]");
                break;

            case "/applystat":
                if (args.Length < 4)
                {
                    AppendLog("Uso: /applystat [StatType] [QualityTier] [PERMANENT|TEMPORARY] [duração] [cooldown]");
                    return;
                }

                if (!System.Enum.TryParse(args[1], true, out StatType stat))
                {
                    AppendLog("StatType inválido.");
                    return;
                }

                if (!System.Enum.TryParse(args[2], true, out QualityTier tier))
                {
                    AppendLog("QualityTier inválido.");
                    return;
                }

                if (!System.Enum.TryParse(args[3], true, out StatComponent.StatTime statTime))
                {
                    AppendLog("StatTime inválido. Use PERMANENT ou TEMPORARY.");
                    return;
                }

                float duration = args.Length > 4 && float.TryParse(args[4], out float d) ? d : 0;
                float cooldown = args.Length > 5 && float.TryParse(args[5], out float c) ? c : 0;

                if (targetObject.TryGetComponent<StatComponent>(out var statComp))
                {
                    statComp.ApplyStat(stat, tier, statComp.gameObject, statTime, duration, cooldown);
                    AppendLog($"ApplyStat aplicado: {stat} {tier} {statTime}");
                }
                else
                {
                    AppendLog("StatComponent não encontrado no alvo.");
                }
                break;
            default:
                AppendLog("Comando inválido. Use /help.");
                break;
        }
    }

    private void AppendLog(string msg)
    {
        logText.text += $"\n> {msg}";
        Debug.Log("[Console] " + msg);
    }
}
