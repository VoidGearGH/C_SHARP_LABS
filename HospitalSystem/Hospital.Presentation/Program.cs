using System;
using System.Windows.Forms;

namespace Hospital.Presentation
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}