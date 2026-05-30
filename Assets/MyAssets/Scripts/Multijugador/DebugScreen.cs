using UnityEngine;
using Unity.Netcode;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI texto;
    private string log = "";

    void OnEnable()
    {
        Application.logMessageReceived += OnLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= OnLog;
    }

    void OnLog(string msg, string stack, LogType type)
    {
        if (msg.Contains("[NP2]") || msg.Contains("[HGM]") || msg.Contains("[SpawnManager]") ||
            msg.Contains("[TagGame]") || msg.Contains("[Host]") || msg.Contains("[Servidor]") ||
            msg.Contains("NetworkPlayer2") || msg.Contains("XRRig"))
        {
            log += "\n" + msg;
            if (log.Length > 6000)
                log = log.Substring(log.Length - 6000);
            if (texto != null) texto.text = log;
        }
    }
}