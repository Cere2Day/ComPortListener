using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using PDR;
using System.Collections.Generic;

namespace PDRForms
{
    public partial class Form1 : Form
    {
        private static List<string> receivedDataList = new List<string>(); // Liste, um die letzten 20 Werte zu speichern
        private const int MaxLines = 20; // Maximale Anzahl der Zeilen vor dem Leeren

        
        public static int port;
        public static int baud;
        public static int databit;
        public static int filename;
        public static int arguments;
        private bool allowVisible;

        public static string fileName = Path.Combine(Environment.CurrentDirectory, "Serial.ini");
        public static string[] lines = File.ReadAllLines(fileName);

        public static string subport = "";
        public static string subBaud = "";
        public static string subDataBit = "";
        public static string subFilename = "";
        public static string subArguments = "";
        private static string logFilePath = Path.Combine(Environment.CurrentDirectory, "PDR.log");

        private NotifyIcon systemTrayIcon;
        private readonly Timer timer;

        private ContextMenuStrip contextMenuStrip;

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = true;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private async Task CallmyfunctionAsync(string data, string filename, string arguments)
        {
            try
            {
                // Überprüfe, ob die PortalDL.exe läuft
                Process[] processes = Process.GetProcessesByName("PortalDL");
                foreach (Process p in processes)
                {
                    p.Kill();
                }

                string fileNameOnly = Path.GetFileName(filename);

                if (!fileNameOnly.Equals("PortalDL.exe", StringComparison.OrdinalIgnoreCase))
                {
                    arguments = arguments + " " + data;
                }
                else
                {
                    arguments = arguments + data;
                }

                // Kombiniere filename und arguments in einer einzigen Nachricht
                string logMessage = $"{filename} {arguments}";
                Logger.LogToFile($"Attempting to start process with {logMessage}"); // Vor dem Start protokollieren

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false
                };
                process.StartInfo = startInfo;
                process.Start();

                Logger.LogToFile($"Process started successfully with {logMessage}"); // Nach dem Start protokollieren
            }
            catch (UnauthorizedAccessException uae)
            {
                Logger.LogToFile($"UnauthorizedAccessException: {uae.Message} Filename: {filename} Arguments: {arguments}"); // Detaillierte Fehlermeldung protokollieren
            }
            catch (FileNotFoundException fnfe)
            {
                Logger.LogToFile($"FileNotFoundException: {fnfe.Message} Filename: {filename} Arguments: {arguments}"); // Detaillierte Fehlermeldung protokollieren
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"General Exception: {ex.Message} Filename: {filename} Arguments: {arguments}"); // Allgemeine Fehlermeldung protokollieren
            }
        }

        private static void LogToFile(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeSystemTray();
            //InitializeRichTextBox(); // Initialisiert die RichTextBox
            timer = new Timer
            {
                Interval = 5000
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            this.UseWaitCursor = false;
        }

        private void InitializeSystemTray()
        {
            systemTrayIcon = new NotifyIcon
            {
                Icon = this.Icon,
                Visible = true,
                Text = "PortDataReceiver"
            };
            systemTrayIcon.MouseDoubleClick += new MouseEventHandler(OnSystemTrayIconDoubleClick);

            contextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem option0MenuItem = new ToolStripMenuItem("Maximieren");
            ToolStripMenuItem option1MenuItem = new ToolStripMenuItem("INI öffnen");
            ToolStripMenuItem option2MenuItem = new ToolStripMenuItem("Anwendung beenden");

            option0MenuItem.Click += Option0MenuItem_Click;
            option1MenuItem.Click += Option1MenuItem_Click;
            option2MenuItem.Click += Option2MenuItem_Click;

            contextMenuStrip.Items.Add(option0MenuItem);
            contextMenuStrip.Items.Add(option1MenuItem);
            contextMenuStrip.Items.Add(option2MenuItem);

            systemTrayIcon.ContextMenuStrip = contextMenuStrip;

            this.Hide();
        }

        private void InitializeRichTextBox()
        {
            richTextBox1 = new RichTextBox
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(richTextBox1);
        }

        private void OnSystemTrayIconDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                if (lines[i].Contains("PORT="))
                {
                    port = i;
                }
                if (lines[i].Contains("BAUD="))
                {
                    baud = i;
                }
                if (lines[i].Contains("FILENAME="))
                {
                    filename = i;
                }
                if (lines[i].Contains("DATABIT="))
                {
                    databit = i;
                }
                if (lines[i].Contains("ARGUMENTS="))
                {
                    arguments = i;
                }
            }
            subport = lines[port].Substring(lines[port].IndexOf('=') + 1);
            subBaud = lines[baud].Substring(lines[baud].IndexOf('=') + 1);
            subDataBit = lines[databit].Substring(lines[databit].IndexOf('=') + 1);
            subFilename = lines[filename].Substring(lines[filename].IndexOf('=') + 1);
            subArguments = lines[arguments].Substring(lines[arguments].IndexOf('=') + 1);

            SerialPort mySerialPort = new SerialPort(subport)
            {
                BaudRate = int.Parse(subBaud),
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = int.Parse(subDataBit),
                Handshake = Handshake.None,
                RtsEnable = true
            };

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);

            try
            {
                mySerialPort.Open();
            }
            catch (Exception)
            {
                COM_PORT_ERROR com_err = new COM_PORT_ERROR();
                com_err.ShowDialog();
            }
            label1.Text = "Listening on " + subport;

            Console.Read();
        }


        private async void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();

                try
                {
                    // Verarbeite die Daten asynchron
                    CallmyfunctionAsync(indata, subFilename, subArguments).Wait();

                    // Aktualisiere die RichTextBox
                    UpdateRichTextBox(indata);
                }
                catch (Exception ex)
                {
                    Logger.LogToFile("Error processing data: " + ex.Message);
                }
            });
        }

        private void UpdateRichTextBox(string data)
        {
            if (receivedDataList.Count >= 20)
            {
                receivedDataList.RemoveAt(0);
            }
            receivedDataList.Add(data);

            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke((MethodInvoker)delegate
                {
                    richTextBox1.Lines = receivedDataList.ToArray();
                });
            }
            else
            {
                richTextBox1.Lines = receivedDataList.ToArray();
            }
        }

        private void ProcessData(string data)
        {
            try
            {
                string cleanData = data.Trim();
                if (Uri.IsWellFormedUriString(cleanData, UriKind.Absolute))
                {
                    // Verarbeite URL
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile("Error in ProcessData: " + ex.Message);
            }
        }

        private void NotifyIcon1_DoubleClick_1(object sender, EventArgs e)
        {
            allowVisible = true;
            Show();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            timer.Stop();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void Option0MenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void Option1MenuItem_Click(object sender, EventArgs e)
        {
            // Nutze den Pfad aus der INI-Datei
            Process.Start(subFilename, fileName);
        }

        private void Option2MenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Öffne die INI-Datei in Notepad
                Process.Start("notepad.exe", fileName);
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung, falls etwas schiefgeht
                MessageBox.Show("Fehler beim Öffnen der INI-Datei: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            // Event-Handler für TextChanged
        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                // Öffne die Log-Datei in Notepad
                Process.Start("notepad.exe", logFilePath);
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung, falls etwas schiefgeht
                MessageBox.Show("Fehler beim Öffnen der Log-Datei: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
