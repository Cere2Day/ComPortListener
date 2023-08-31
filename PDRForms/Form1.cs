using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using PDR;

namespace PDRForms
{


    public partial class Form1 : Form
    {
        public static int port;                                                             // Variable from Ini for Com-Port Number
        public static int baud;                                                             // Variable from Ini for Baud
        public static int databit;                                                          // Variable from Ini for Databit
        public static int filename;                                                         // Variable from Ini for ininame
        public static int arguments;                                                        // Variable from Ini for params
        private bool allowVisible;                                                          // Bool-Variable for double click
        

        public static string fileName = Path.Combine(Environment.CurrentDirectory, "Serial.ini");       //Read ini-file

        public static string[] lines = File.ReadAllLines(fileName);                                     //Put ini-input in array

        public static String subport = "";                                                  //Substitute-String for Port
        public static String subBaud = "";                                                  //Substitute-String for Baud
        public static String subDataBit = "";                                               //Substitute-String for Databit
        public static String subFilename = "";                                              //Substitute-String for Filename
        public static String subArguments = "";                                             //Substitute-String for Arguments
        public string newLine = Environment.NewLine;

        private NotifyIcon systemTrayIcon;
        private readonly Timer timer;

        private ContextMenuStrip contextMenuStrip;




        protected override void SetVisibleCore(bool value)                                  //Show app
        {
            if (!allowVisible)
            {
                value = true;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }
        private static void Callmyfunction(string data, string filename, string arguments)      //Function with hidden cmd and arguments from ini file
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = filename,
                Arguments = arguments + " " + data
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        public Form1()                                                                           
        {
            InitializeComponent();
            InitializeSystemTray();
            timer = new Timer
            {
                Interval = 5000
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            
        }

        private void InitializeSystemTray()
        {
            systemTrayIcon = new NotifyIcon
            {
                Icon = this.Icon, // Setze das Icon des NotifyIcon-Steuerelements auf das Icon deiner Windows Forms App.
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

            this.Hide(); // Verstecke das Hauptformular.
        }

        private void OnSystemTrayIconDoubleClick(object sender, MouseEventArgs e)
        {
            // Zeige das Hauptformular wieder an, wenn das System Tray Icon doppelt angeklickt wird.
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }






        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < lines.Length; i++)                                  //for-Schleife to put ini-content in substitude variables
            {                                                                       //with contains
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
            subport = lines[port].Substring(lines[port].IndexOf('=') + 1);                      //fill the stubstitude strings with the right positions
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
            };                                  //create com port via .net







            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);      //event

            try
            {
                mySerialPort.Open();                                                                            //open 
            }catch (Exception)
            {
                COM_PORT_ERROR com_err = new COM_PORT_ERROR();
                com_err.ShowDialog();
            }
            label1.Text = "Listening on " + subport + ":";                                                  //show com port on label
            textBox2.Text = "Config located in: " + fileName;                                               //show in textbox where config is



            Console.Read();
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)     //receive data event
        {
            SerialPort sp = (SerialPort)sender;                                                                                                                    
            string indata = sp.ReadExisting();                                                                  //write data in string
            
            Callmyfunction(indata, subFilename, subArguments);                                                  //callmyfunction
            this.Invoke((MethodInvoker)delegate                                                                 //write received data in textbox
            {
                textBox1.Text = textBox1.Text + indata +newLine;
                
            });
        }        
        private void NotifyIcon1_DoubleClick_1(object sender, EventArgs e)                        //double click systray
        {
            allowVisible = true;
            Show();
        }        
        private void Button1_Click_1(object sender, EventArgs e)                                   //Close on exit button
        {
            Environment.Exit(1);
        }



        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Timer abgelaufen, minimieren Sie die Anwendung in den System-Tray
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            // Stoppen Sie den Timer, da er nicht mehr benötigt wird
            timer.Stop();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;// Aktion beim Minimieren des Fensters
            }
        }

        private void Option0MenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }
        private void Option1MenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", fileName);
        }

        private void Option2MenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", fileName);
        }
    }


}
