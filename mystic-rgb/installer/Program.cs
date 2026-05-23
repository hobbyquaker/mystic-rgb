using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

var sourceDir = AppContext.BaseDirectory;
var targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "mystic-rgb");

if (!IsElevated())
{
    RelaunchElevated();
    return;
}

try
{
    Directory.CreateDirectory(targetDir);

    CopyIfExists("mystic-rgb.exe");
    CopyIfExists("MysticLight_SDK.dll");
    CopyIfExists("README.md");
    CopyIfExists("LICENSE");

    var setupExe = Path.Combine(targetDir, "mystic-rgb.exe");
    if (File.Exists(setupExe))
    {
        using var p = Process.Start(new ProcessStartInfo(setupExe, "--setup")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
        p?.WaitForExit(15000);
    }

    MessageBox.Show($"Installation abgeschlossen.\n\nPfad:\n{targetDir}", "mystic-rgb Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
catch (Exception ex)
{
    MessageBox.Show($"Installation fehlgeschlagen:\n{ex.Message}", "mystic-rgb Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

void CopyIfExists(string fileName)
{
    var src = Path.Combine(sourceDir, fileName);
    if (!File.Exists(src)) return;
    var dst = Path.Combine(targetDir, fileName);
    File.Copy(src, dst, overwrite: true);
}

static bool IsElevated()
{
    using var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

static void RelaunchElevated()
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Environment.ProcessPath!,
            UseShellExecute = true,
            Verb = "runas"
        });
    }
    catch
    {
        MessageBox.Show("Administratorrechte sind erforderlich.", "mystic-rgb Installer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
