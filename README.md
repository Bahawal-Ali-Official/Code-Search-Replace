# Code Search Replace - Real-Time Memory Editor

![Project Status](https://img.shields.io/badge/Status-Active-brightgreen)
![Language](https://img.shields.io/badge/Language-C%23-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6)

**Code Search Replace** is a powerful Windows Forms application built with C# (.NET Framework) that allows users to scan, filter, and modify memory values of running processes in real-time. Designed for educational purposes, it demonstrates how to interact with low-level system memory using the Win32 API.

## 📸 Interface

![Application Interface](image_54fcbc.jpg)
*The main interface allows for process selection, value scanning, and memory modification.*

## 🚀 Features

* **Process Selection:** Easily attach to any running process on your Windows system.
* **Smart Value Detection:** Automatically detects data types (Integer, String, or Hex codes) during scans.
* **First Scan & Next Scan:** Implements classic memory filtering techniques. Start with an initial scan and narrow down results with subsequent scans as values change in-game.
* **Real-time Modification:** Once the memory address is isolated, modify values (e.g., scores, currency) instantly.
* **PID Support:** Manual entry for Process ID if needed.

## 🛠️ Technology Stack

This project works by bridging high-level C# code with low-level system functions via **P/Invoke**.

* **Language:** C# (.NET Framework)
* **GUI:** Windows Forms (WinForms)
* **Core Logic:** Interacts with `kernel32.dll`.
* **Key Win32 API Functions:**
    * `OpenProcess`: To gain access to the target application.
    * `ReadProcessMemory`: To scan for specific values.
    * `WriteProcessMemory`: To inject new values into the address.

## ⚙️ How It Works

1.  **Select Process:** The user selects a target game or application.
2.  **First Scan:** The user searches for a known value (e.g., `100` coins). The tool scans the process's memory space.
3.  **Find Next:** After the value changes in the game (e.g., coins drop to `90`), the user scans again. The tool compares the new value against the previous addresses to filter the list.
4.  **Modify:** When the exact address is found, the user writes a new value to manipulate the game state.

## 📦 Installation & Usage

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/Bahawal-Ali-Official/Code-Search-Replace.git
    ```
2.  **Open the Project:**
    Open `Login.sln` in **Visual Studio**.
3.  **Restore Packages:**
    Ensure all NuGet packages (like Fody/Costura if used) are restored.
4.  **Build & Run:**
    Click `Start` to launch the application.

## ⚠️ Disclaimer

**Educational Purpose Only.**
This tool is developed strictly for educational purposes to understand memory management, pointers, and the Windows API. The developer is not responsible for any misuse of this tool in online games or software violations.

## 👨‍💻 Author

**Developed by: Bahawal Ali**

---
*If you find this project helpful, please give it a ⭐ star!*
