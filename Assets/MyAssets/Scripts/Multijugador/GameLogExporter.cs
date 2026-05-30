using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Se inicializa automáticamente al arrancar el juego (editor o build).
/// Captura todos los logs que tengan los prefijos del proyecto y los guarda
/// en un fichero .txt en la carpeta persistente de la app.
///
/// Ruta en editor:   %AppData%\..\LocalLow\<Company>\<Product>\GameLog_<fecha>.txt
/// Ruta en build PC: %AppData%\..\LocalLow\<Company>\<Product>\GameLog_<fecha>.txt
/// Ruta en build Android: /storage/emulated/0/Android/data/<package>/files/GameLog_<fecha>.txt
/// </summary>
public static class GameLogExporter
{
    // ── Prefijos que se van a capturar ──────────────────────────────────────
    private static readonly string[] Prefijos =
    {
        "[NP2]", "[HGM]", "[SpawnManager]", "[TagGame]",
        "[Host]", "[Servidor]", "NetworkPlayer2", "XRRig"
    };

    private static string _filePath;
    private static bool _inicializado;

    // [RuntimeInitializeOnLoadMethod] hace que Unity llame a esto automáticamente
    // antes de que se cargue la primera escena — sin necesidad de añadir nada a la jerarquía.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_inicializado) return;
        _inicializado = true;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _filePath = Path.Combine(Application.persistentDataPath, $"GameLog_{timestamp}.txt");

        // Cabecera del fichero
        File.WriteAllText(_filePath,
            $"=== GameLog — {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n" +
            $"Plataforma : {Application.platform}\n" +
            $"Versión    : {Application.version}\n" +
            $"Fichero    : {_filePath}\n" +
            "========================================================\n\n");

        Application.logMessageReceivedThreaded += OnLog;

        // Imprime la ruta en consola para que sea fácil de encontrar
        Debug.Log($"[GameLogExporter] Guardando logs filtrados en:\n{_filePath}");
    }

    private static void OnLog(string mensaje, string stackTrace, LogType tipo)
    {
        // Filtrar solo los mensajes de nuestro proyecto
        bool capturar = false;
        foreach (var prefijo in Prefijos)
        {
            if (mensaje.Contains(prefijo))
            {
                capturar = true;
                break;
            }
        }
        if (!capturar) return;

        // Icono según tipo de log
        string icono = tipo switch
        {
            LogType.Warning   => "⚠",
            LogType.Error     => "✖",
            LogType.Exception => "💥",
            _                 => "·"
        };

        string linea = $"[{DateTime.Now:HH:mm:ss.fff}] {icono} {mensaje}";

        // Añadir stack trace solo en errores/excepciones para no llenar el fichero
        if ((tipo == LogType.Error || tipo == LogType.Exception) &&
            !string.IsNullOrEmpty(stackTrace))
        {
            linea += "\n    " + stackTrace.Replace("\n", "\n    ");
        }

        try
        {
            File.AppendAllText(_filePath, linea + "\n");
        }
        catch
        {
            // Silencioso — no queremos un bucle infinito de logs de error
        }
    }
}
