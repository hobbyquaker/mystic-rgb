// ============================================================================
// mystic-rgb — MSI Mystic Light RGB LED Controller
//
// Controls RGB LEDs on MSI hardware via the official MysticLight SDK.
// 100% vibe-coded with Claude Sonnet 4.6 🤖
//
// Architecture (UAC-free elevation via Task Scheduler):
//   The MysticLight SDK requires administrator privileges to communicate with
//   the MSI Center service. To avoid a UAC prompt on every invocation, a
//   Windows Scheduled Task is registered once (--setup). Non-elevated calls
//   then trigger this task and receive the output via temp files.
// ============================================================================

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

const string TaskName = "MysticRGB_DeviceList";
const int MaxRetries = 5;
const int RetryDelayMs = 1000;
const int TaskTimeout = 15_000;

string outputFile = Path.Combine(Path.GetTempPath(), "mystic-rgb-output.txt");
string doneFile = Path.Combine(Path.GetTempPath(), "mystic-rgb-done.flag");
string cacheFile = Path.Combine(Path.GetTempPath(), "mystic-rgb-styles-cache.txt");
string actionFile = Path.Combine(Path.GetTempPath(), "mystic-rgb-action.txt");

bool isTaskMode = args.Contains("--task-mode");
bool isSetupMode = args.Contains("--setup");
bool isElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                       .IsInRole(WindowsBuiltInRole.Administrator);

int effectIdx = Array.IndexOf(args, "--effect");
int colorIdx = Array.IndexOf(args, "--color");
string? effect = effectIdx >= 0 && effectIdx + 1 < args.Length ? args[effectIdx + 1] : null;
string? color = colorIdx >= 0 && colorIdx + 1 < args.Length ? args[colorIdx + 1] : null;

if (isSetupMode)
{
    if (!isElevated) return;

    string exe = Environment.ProcessPath!;
    string xmlPath = Path.Combine(Path.GetTempPath(), "mystic-rgb-task.xml");
    File.WriteAllText(xmlPath, BuildTaskXml(exe), Encoding.Unicode);
    try
    {
        RunCmd("schtasks", $"/Create /TN \"{TaskName}\" /XML \"{xmlPath}\" /F");
    }
    finally
    {
        File.Delete(xmlPath);
    }

    return;
}

if (isTaskMode)
{
    var stored = (File.Exists(actionFile) ? File.ReadAllText(actionFile).Trim() : "Off").Split(' ', 2);
    string te = stored[0];
    string? tc = stored.Length > 1 ? stored[1] : null;

    var sb = new StringBuilder();
    using var writer = new StringWriter(sb);
    Console.SetOut(writer);
    Console.SetError(writer);

    RunDeviceList(te, tc);

    writer.Flush();
    File.WriteAllText(outputFile, sb.ToString());
    File.WriteAllText(doneFile, "done");
    return;
}

if (effect is null) return;

if (isElevated)
{
    RunDeviceList(effect, color);
    return;
}

if (!TaskExists()) return;

File.WriteAllText(actionFile, color != null ? $"{effect} {color}" : effect!);
File.Delete(outputFile);
File.Delete(doneFile);
RunCmd("schtasks", $"/Run /TN \"{TaskName}\"");

int waited = 0;
while (!File.Exists(doneFile) && waited < TaskTimeout)
{
    Thread.Sleep(200);
    waited += 200;
}



void RunDeviceList(string effectName, string? colorValue)
{
    int status = NativeMethods.MLAPI_Initialize();
    if (status != 0) return;

    Thread.Sleep(500);
    try
    {
        string[] devTypes = [], ledCounts = [];

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            status = NativeMethods.MLAPI_GetDeviceInfo(out devTypes, out ledCounts);
            if (status == 0) break;

            if (attempt < MaxRetries) Thread.Sleep(RetryDelayMs);
        }

        if (status != 0) return;

        var (R, G, B) = ParseColor(colorValue);
        var styleCache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < devTypes.Length; i++)
        {
            try
            {
                int ledCount = int.Parse(ledCounts[i]);
                for (int area = 0; area < ledCount; area++)
                {
                    int infoRet = NativeMethods.MLAPI_GetLedInfo(devTypes[i], area, out _, out string[]? styles);
                    if (infoRet != 0) continue;

                    if (styles != null)
                        foreach (var s in styles) styleCache.Add(s);

                    string styleToUse =
                        styles?.FirstOrDefault(s => s.Equals(effectName, StringComparison.OrdinalIgnoreCase)) ??
                        styles?.FirstOrDefault() ??
                        effectName;

                    NativeMethods.MLAPI_SetLedStyle(devTypes[i], area, styleToUse);
                    NativeMethods.MLAPI_SetLedColor(devTypes[i], area, R, G, B);
                }
            }
            catch { }
        }

        if (styleCache.Count > 0)
            try { File.WriteAllLines(cacheFile, styleCache.OrderBy(s => s)); } catch { }
    }
    finally
    {
        NativeMethods.MLAPI_Release();
    }
}

(int R, int G, int B) ParseColor(string? c)
{
    if (c != null)
    {
        var hex = c.StartsWith('#') ? c[1..] : c;
        if (hex.Length == 3) hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        if (hex.Length == 6)
        {
            try
            {
                int v = Convert.ToInt32(hex, 16);
                return ((v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
            }
            catch { }
        }
    }

    return c?.ToLowerInvariant() switch
    {
        "red" => (255, 0, 0),
        "green" => (0, 255, 0),
        "blue" => (0, 0, 255),
        "cyan" => (0, 255, 255),
        "magenta" => (255, 0, 255),
        "yellow" => (255, 255, 0),
        "white" => (255, 255, 255),
        _ => (0, 0, 0),
    };
}

string[]? TryGetDeviceStyles()
{
    if (NativeMethods.MLAPI_Initialize() != 0) return null;
    try
    {
        if (NativeMethods.MLAPI_GetDeviceInfo(out var devTypes, out var ledCounts) != 0) return null;

        var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < devTypes.Length; i++)
        {
            int n = int.TryParse(ledCounts[i], out int x) ? x : 1;
            for (int area = 0; area < n; area++)
                if (NativeMethods.MLAPI_GetLedInfo(devTypes[i], area, out _, out string[]? styles) == 0 && styles != null)
                    foreach (var s in styles) all.Add(s);
        }

        var result = all.OrderBy(s => s).ToArray();
        try { File.WriteAllLines(cacheFile, result); } catch { }
        return result;
    }
    catch
    {
        return null;
    }
    finally
    {
        NativeMethods.MLAPI_Release();
    }
}

bool TaskExists()
{
    int rc = RunCmd("schtasks", $"/Query /TN \"{TaskName}\"", silent: true);
    return rc == 0;
}

int RunCmd(string exe, string arguments, bool silent = false)
{
    var psi = new ProcessStartInfo(exe, arguments)
    {
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = silent,
        RedirectStandardError = silent,
    };
    using var p = Process.Start(psi)!;
    if (silent) { p.StandardOutput.ReadToEnd(); p.StandardError.ReadToEnd(); }
    p.WaitForExit();
    return p.ExitCode;
}

string BuildTaskXml(string exePath)
{
    string safe = exePath
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");

    return $"""
        <?xml version="1.0" encoding="UTF-16"?>
        <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
          <Triggers/>
          <Principals>
            <Principal id="Author">
              <LogonType>InteractiveToken</LogonType>
              <RunLevel>HighestAvailable</RunLevel>
            </Principal>
          </Principals>
          <Settings>
            <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
            <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
            <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
            <ExecutionTimeLimit>PT5M</ExecutionTimeLimit>
            <Enabled>true</Enabled>
          </Settings>
          <Actions>
            <Exec>
              <Command>{safe}</Command>
              <Arguments>--task-mode</Arguments>
            </Exec>
          </Actions>
        </Task>
        """;
}

static class NativeMethods
{
    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_Initialize();

    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_Release();

    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_GetDeviceInfo(
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pDevType,
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedCount);

    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_GetLedInfo(
        [MarshalAs(UnmanagedType.BStr)] string type,
        int index,
        [MarshalAs(UnmanagedType.BStr)] out string pName,
        [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedStyles);

    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_SetLedStyle(
        [MarshalAs(UnmanagedType.BStr)] string type,
        int index,
        [MarshalAs(UnmanagedType.BStr)] string style);

    [DllImport("MysticLight_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MLAPI_SetLedColor(
        [MarshalAs(UnmanagedType.BStr)] string type,
        int index,
        int R, int G, int B);
}
