using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GrimmsVenture.Core;

/// <summary>
/// Logger do jogo: escreve em Logs/log-*.txt (ao lado do executavel).
/// Mantem no maximo 3 arquivos — ao abrir um novo, apaga os mais antigos.
/// Pasta Logs/ fica fora do GitHub (.gitignore). Nunca derruba o jogo.
/// </summary>
public static class GameLogger
{
    static readonly object _lock = new();
    static StreamWriter? _writer;

    static GameLogger()
    {
        try
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "Logs");
            Directory.CreateDirectory(dir);

            // rotacao: deixa no maximo 2 antigos (+ o novo que vai abrir = 3)
            foreach (string f in Directory.GetFiles(dir, "log-*.txt")
                                       .OrderByDescending(f => f)
                                       .Skip(2))
            {
                try { File.Delete(f); } catch { }
            }

            _writer = new StreamWriter(
                Path.Combine(dir, "log-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt"),
                append: false, Encoding.UTF8)
            { AutoFlush = true };

            Info("=== Grimms Venture — log aberto ===");

            // captura crashes nao tratados (jogo derrubado por excecao)
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Error("CRASH NAO TRATADO: " + e.ExceptionObject);
        }
        catch { /* o logger nunca pode derrubar o jogo */ }
    }

    public static void Info(string msg)  => Write("INFO ", msg);
    public static void Warn(string msg)  => Write("WARN ", msg);
    public static void Error(string msg) => Write("ERROR", msg);

    static void Write(string level, string msg)
    {
        lock (_lock)
        {
            try
            {
                _writer?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{level}] {msg}");
            }
            catch { }
        }
    }
}
