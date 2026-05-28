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
        if (msg.Contains("NetworkPlayer2") || msg.Contains("SpawnManager") || msg.Contains("XRRig"))
        {
            log += "\n" + msg;
            if (texto != null) texto.text = log;
        }
    }
}