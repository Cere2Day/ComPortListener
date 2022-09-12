using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading.Tasks;


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

        protected override void SetVisibleCore(bool value)                                  //Show app
        {
            if (!allowVisible)
            {
                value = true;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }
        private static void callmyfunction(string data, string filename, string arguments)      //Function with hidden cmd and arguments from ini file
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = filename;
            startInfo.Arguments = arguments + " " + data;
            process.StartInfo = startInfo;
            process.Start();
        }

        public Form1()                                                                           
        {
            InitializeComponent();
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




            SerialPort mySerialPort = new SerialPort(subport);                                  //create com port via .net
            mySerialPort.BaudRate = int.Parse(subBaud);
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = int.Parse(subDataBit);
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.RtsEnable = true;






            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);      //event

            mySerialPort.Open();                                                                            //open 
                        label1.Text = "Listening on " + subport + ":";                                      //show com port on label
            textBox2.Text = "Config located in: " + fileName;                                               //show in textbox where config is



            Console.Read();
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)     //receive data event
        {
            SerialPort sp = (SerialPort)sender;                                                                                                                    
            string indata = sp.ReadExisting();                                                                  //write data in string
            
            callmyfunction(indata, subFilename, subArguments);                                                  //callmyfunction
            this.Invoke((MethodInvoker)delegate                                                                 //write received data in textbox
            {
                textBox1.Text = textBox1.Text + indata +newLine;
                
            });
        }        
        private void notifyIcon1_DoubleClick_1(object sender, EventArgs e)                        //double click systray
        {
            allowVisible = true;
            Show();
        }        
        private void button1_Click_1(object sender, EventArgs e)                                   //Close on exit button
        {
            Environment.Exit(1);
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }


}
