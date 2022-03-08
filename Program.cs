using System;
using System.Windows.Forms;

namespace FastGraphics
{
  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      try
      {
        Form1 form = new Form1();
        Application.Idle += (s, e) =>
        {
          form.OnIdle();
          //form.Invalidate();
          //form.UpdateRenderTimeText();
        };

        Application.Run(form);
      }
      catch (Exception ex)
      {
        ex = ex;
      }
    }
  }
}
