using System;
using System.IO;
using System.Windows.Forms;

namespace Maneubo
{
  static class Program
  {
    public static string GetDataDirectory()
    {
      string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Maneubo");
      if(!Directory.Exists(directory))
      {
        try { Directory.CreateDirectory(directory); }
        catch { directory = null; }
      }
      return directory;
    }

    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  }
}
