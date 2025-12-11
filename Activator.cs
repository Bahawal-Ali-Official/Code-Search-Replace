using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Beyondmem;
using Guna.UI2.WinForms;

namespace Login
{
    public partial class Activator : Form
    {
        private string selectedPidString;
        private IEnumerable<long> foundAddresses;

        public static Escape MemLib = new Escape();

        public Activator()
        {
            InitializeComponent();
        }

        private void Activator_Load(object sender, EventArgs e)
        {
            status.Text = "Please select a process...";
            status.ForeColor = Color.Black;
        }

        private byte[] GetBytesFromInput(string input)
        {
            try
            {
                if (input.Contains("."))
                {
                    float floatValue = float.Parse(input);
                    return BitConverter.GetBytes(floatValue);
                }
                else
                {
                    int intValue = int.Parse(input);
                    return BitConverter.GetBytes(intValue);
                }
            }
            catch (FormatException)
            {
                return Encoding.ASCII.GetBytes(input);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int procId = ShowProcessSelectionDialog();
            if (procId != 0)
            {
                selectedPidString = procId.ToString();
                PID_Value.Text = selectedPidString;
                status.Text = $"Selected Process ID: {selectedPidString}";
                status.ForeColor = Color.Green;
            }
            else
            {
                status.Text = "No process selected.";
                status.ForeColor = Color.Red;
            }
        }

        private async void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PID_Value.Text))
            {
                status.Text = "Please enter or select a Process ID.";
                status.ForeColor = Color.Red;
                MessageBox.Show("Process ID field is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                status.Text = "Please enter a value to search.";
                status.ForeColor = Color.Red;
                MessageBox.Show("Search value field is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int pid = int.Parse(PID_Value.Text);
                string searchInput = guna2TextBox1.Text;

                status.Text = "Searching...";
                status.ForeColor = Color.Blue;

                if (!MemLib.OpenProcess(pid))
                {
                    status.Text = "Failed To Open Process....";
                    status.ForeColor = Color.DarkRed;
                    MessageBox.Show("Failed to open process. Please ensure you have selected a valid process and are running as administrator.", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                byte[] searchBytes = GetBytesFromInput(searchInput);
                string aobSearchString = BitConverter.ToString(searchBytes).Replace("-", " ");

                foundAddresses = await MemLib.AoBScan(0L, long.MaxValue, aobSearchString, true, true, false);

                if (foundAddresses == null || !foundAddresses.Any())
                {
                    status.Text = "Value not found. 😔";
                    status.ForeColor = Color.Red;
                    guna2CircleButton2.Enabled = false;
                    MessageBox.Show("Search completed. No matching values were found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    status.Text = $"Found {foundAddresses.Count()} occurrences! ✅";
                    status.ForeColor = Color.Green;
                    guna2CircleButton2.Enabled = true;
                    MessageBox.Show($"Search completed. Found {foundAddresses.Count()} values.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                status.Text = $"An error occurred: {ex.Message}";
                status.ForeColor = Color.Red;
                guna2CircleButton2.Enabled = false;
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            if (foundAddresses == null || !foundAddresses.Any())
            {
                MessageBox.Show("Please perform a search first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox2.Text))
            {
                MessageBox.Show("Please enter a value to replace with.");
                return;
            }

            byte[] searchBytes = GetBytesFromInput(guna2TextBox1.Text);
            byte[] replaceBytes = GetBytesFromInput(guna2TextBox2.Text);

            if (searchBytes.Length != replaceBytes.Length)
            {
                MessageBox.Show("🛑 WARNING: Search and Replace values must have the SAME byte length! Replacing with different sizes can crash the target application.", "Critical Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            status.Text = "Replacing values...";
            status.ForeColor = Color.Blue;

            try
            {
                await Task.Run(() =>
                {
                    int pid = int.Parse(PID_Value.Text);
                    if (!MemLib.OpenProcess(pid))
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            status.Text = "Failed to open process for replacement.";
                            status.ForeColor = Color.Red;
                        });
                        return;
                    }

                    int replacedCount = 0;
                    Parallel.ForEach(foundAddresses, address =>
                    {
                        // Yahan arguments ki tarteeb (order) theek kar di gayi hai
                        bool writeSuccess = MemLib.WriteMemory(address.ToString("X"), "bytes", BitConverter.ToString(replaceBytes).Replace("-", " "));

                        if (writeSuccess)
                        {
                            replacedCount++;
                        }
                    });

                    this.Invoke((MethodInvoker)delegate
                    {
                        status.Text = $"Replaced {replacedCount} out of {foundAddresses.Count()} occurrences. 🎉";
                        status.ForeColor = Color.DarkGreen;
                    });
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    status.Text = $"An error occurred during replacement: {ex.Message}";
                    status.ForeColor = Color.Red;
                });
            }
        }

        public static int ShowProcessSelectionDialog()
        {
            Form dialog = new Form
            {
                Text = "Select Process",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent
            };

            ListBox listBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (Process p in Process.GetProcesses())
            {
                listBox.Items.Add($"{p.ProcessName} ({p.Id})");
            }

            int selectedPid = 0;

            listBox.DoubleClick += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    string selected = listBox.SelectedItem.ToString();
                    selectedPid = int.Parse(selected.Split('(')[1].Trim(')'));
                    dialog.Close();
                }
            };

            dialog.Controls.Add(listBox);
            dialog.ShowDialog();

            return selectedPid;
        }

        [DllImport("user32.dll")]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);
        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);
        [DllImport("KERNEL32.DLL")]
        public static extern int Process32First(IntPtr handle, ref ProcessEntry32 pe);
        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
          IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("KERNEL32.DLL")]
        public static extern int Process32Next(IntPtr handle, ref ProcessEntry32 pe);

        public struct ProcessEntry32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHealabel1;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        private async void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            if (foundAddresses == null || !foundAddresses.Any())
            {
                MessageBox.Show("Please perform a search and a replace operation first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("Please enter the new value to verify.");
                return;
            }

            status.Text = "Verifying replaced values...";
            status.ForeColor = Color.Blue;

            try
            {
                await Task.Run(() =>
                {
                    int pid = int.Parse(PID_Value.Text);
                    if (!MemLib.OpenProcess(pid))
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            status.Text = "Failed to open process for verification.";
                            status.ForeColor = Color.Red;
                        });
                        return;
                    }

                    byte[] newSearchBytes = GetBytesFromInput(guna2TextBox1.Text);

                    List<long> verifiedAddresses = new List<long>();

                    Parallel.ForEach(foundAddresses, address =>
                    {
                        byte[] currentBytes = MemLib.FuckedReader(address.ToString("X"), newSearchBytes.Length);

                        if (currentBytes != null && currentBytes.SequenceEqual(newSearchBytes))
                        {
                            verifiedAddresses.Add(address);
                        }
                    });

                    this.Invoke((MethodInvoker)delegate
                    {
                        if (verifiedAddresses.Count > 0)
                        {
                            status.Text = $"Found {verifiedAddresses.Count} locations with the new value. 🎉";
                            status.ForeColor = Color.DarkGreen;
                            foundAddresses = verifiedAddresses;
                        }
                        else
                        {
                            status.Text = "No addresses found with the new value. 😔";
                            status.ForeColor = Color.Red;
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    status.Text = $"An error occurred during verification: {ex.Message}";
                    status.ForeColor = Color.Red;
                });
            }
        }

        private void guna2PictureBox11_Click(object sender, EventArgs e)
        {
            string phoneNumber = "923294675295"; 

            try
            {
                
                Process.Start($"whatsapp://send?phone={phoneNumber}");
            }
            catch (Exception ex)
            {
          
                MessageBox.Show($"Could not open WhatsApp. Please ensure it is installed.\nError: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}