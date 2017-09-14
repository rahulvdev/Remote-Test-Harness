/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - WPF User Interface for WCF Communicator       //
// ver 2.2                                                         //
// Rahul Vjaydev, CSE681 - Software Modeling & Analysis,           //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation
 * -----------------
 * The MaiWindow.xaml.cs file contains all event handlers for GUI components.
 * 
 * Public Interface
 * ----------------
 * void rcvThreadProc();
 * void OnNewMessageHandler(Messages.Message msg);
 * void hostClient();
 * void FileBrowserButton_Click(object sender, RoutedEventArgs e);
 * void UploadDLL_Click(object sender, RoutedEventArgs e);
 * void SubmitTestRequestButton_Click(object sender, RoutedEventArgs e);
 * void queryLogButton_Click(object sender, RoutedEventArgs e);
 * void Window_Unloaded(object sender, RoutedEventArgs e);
 * void createRepoChannel();
 * void uploadLibFiles(string filePath, string filename);
 * void testExecutive();
 * void requestInvoke_testExec();    
 * void logInvoke_testExec();
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 21st November 2016
 * - first release
 */

using Client;
using Communication;
using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using TestHarnessMessages;
using Utilities;

namespace UserInterface
{
    public partial class MainWindow : Window
    {
        public Comm<ClientTH> comm = new Comm<ClientTH>();

        public ICommunicator channelToRep = null;
        delegate void NewMessage(Messages.Message msg);
        public int repPort = 8082;
        public int clientPort = 8085;
        public int testHarPort = 8081;
        event NewMessage OnNewMessage;
        Thread rcvThread;

        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
            Console.Title = "CLIENT1";
            Title = "CLient 1";
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            hostClient();
            testExecutive();
            BrowseRequest.IsEnabled = false;
            submitTestRequest.IsEnabled = false;
        }
        void rcvThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty
                Messages.Message rcvdMsg = new Messages.Message();
                rcvdMsg = comm.rcvr.GetMessage();
                //parse the message 
                if (rcvdMsg.body == "quit")
                    break;
                // call window functions on UI thread
                this.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  OnNewMessage,
                  rcvdMsg);
            }
        }
        void OnNewMessageHandler(Messages.Message msg)
        {
            if (msg.type == "TestResult")
            {
                TestRes.Items.Insert(0, msg.body);

            }
            else if (msg.type == "LogResult") {
                if (msg.body == "Log Unavailable") {
                    LogRes.Items.Insert(0, "Log Unavailable");
                }
                LogRes.Items.Insert(0,msg.body);
            }
        }

        public void hostClient()
        {

            string rcvrEndPoint = Comm<ClientTH>.makeEndPoint("http://localhost", clientPort);
            try
            {


                comm.rcvr.CreateRecvChannel(rcvrEndPoint);
                rcvThread = comm.rcvr.start(rcvThreadProc);
                rcvThread.IsBackground = true;
                Console.Write("\n  rcvr thread id = {0}", rcvThread.ManagedThreadId);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(rcvrEndPoint.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }

        private void FileBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = sender as System.Windows.Controls.Button;
            string buttonName = b.Name;
            if (buttonName.Equals("BrowseLibFiles"))
            {
                var dialog = new FolderBrowserDialog();
                dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                dialog.ShowDialog();
                dllFiles.Text = dialog.SelectedPath;
            }
            else
            {
                System.Windows.Forms.FileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                TestReq.Text = fileDialog.FileName;
            }
        }

        private void UploadDLL_Click(object sender, RoutedEventArgs e)
        {
            BrowseRequest.IsEnabled = true;
            submitTestRequest.IsEnabled = true;

            List<string> listFiles = new List<string>(System.IO.Directory.GetFiles(dllFiles.Text+"\\", "*.dll"));
            if (listFiles.Count != 0)
            {
                if (channelToRep == null)
                {
                    createRepoChannel();
                }
                Console.WriteLine("REQUIREMENTS 2 and 6:");
                foreach (string file in listFiles)
                {
                    uploadLibFiles(file, file.Substring(file.LastIndexOf("\\") + 1));
                    Console.WriteLine("File" + file + "uploaded to repository");
                }
            }
            else {
                TestRes.Items.Insert(0, "DLL files not found");
            }
            Console.WriteLine("");
        }
        private void SubmitTestRequestButton_Click(object sender, RoutedEventArgs e)
        {
            Messages.Message msg = new Messages.Message();
            try
            {
                string testReqFile = TestReq.Text;
                XDocument tReQDoc = new XDocument();
                tReQDoc = XDocument.Load(testReqFile);
                TestRequest TRobj = tReQDoc.ToString().FromXml<TestRequest>();
                msg.body = tReQDoc.ToString();
                msg.type = "TestRequest";
                msg.from = Comm<ClientTH>.makeEndPoint("http://localhost", clientPort);
                msg.to = Comm<ClientTH>.makeEndPoint("http://localhost", testHarPort);
                msg.time = DateTime.Now;
                msg.author = TRobj.author;
                Console.WriteLine("REQUIREMENTS 2 and 6:");
                Console.WriteLine("The following test request has been submitted to the test harness");
                Console.WriteLine(msg.body);
                comm.sndr.PostMessage(msg);
                Console.WriteLine("");
            }
            catch (Exception ex) {
                TestRes.Items.Insert(0, "XML format not supported");
                Console.WriteLine("XML format not supported",ex.Message);
            }
        }
        private void queryLogButton_Click(object sender, RoutedEventArgs e)
        {
            Messages.Message logMsg = new Messages.Message();
            logMsg.author = authorName.Text;
            logMsg.to = Comm<ClientTH>.makeEndPoint("http://localhost", repPort);
            logMsg.from = Comm<ClientTH>.makeEndPoint("http://localhost", clientPort);
            logMsg.time = DateTime.Now;
            logMsg.type = "LogRequest";
            LogRequest lReq = new LogRequest();
            lReq.author= authorName.Text;
            lReq.TestRequestName = TestRequestQuery.Text;
            logMsg.body = lReq.ToXml();
            comm.sndr.PostMessage(logMsg);
            Console.WriteLine("");

        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //  objComm.sndr.PostMessage("quit");
            comm.sndr.Close();
            comm.rcvr.Close();
        }

        public void createRepoChannel()
        {
            string senderEndPoint1 = Comm<ClientTH>.makeEndPoint("http://localhost", repPort);
            EndpointAddress baseAddress = new EndpointAddress(senderEndPoint1);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, senderEndPoint1);
            channelToRep = factory.CreateChannel();


        }
        public void uploadLibFiles(string filePath, string filename)
        {
            try
            {
                //hrt.Start();
                using (var inputStream = new FileStream(filePath, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    channelToRep.uploadFiles(msg);
                }
                // hrt.Stop();
                //Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            }
            catch (Exception e)
            {
                Console.Write("\n  can't find \"{0}\"", filePath);
                Console.WriteLine(e.Message);
            }
        }

        public void testExecutive() {

            Console.WriteLine("TEST EXECUTIVE:Demonstratimg Requirement 13");
            Console.WriteLine("-----------------------------------------------");
            //uploading files
            string filesToRepopath = "..\\..\\..\\FilesToRepository ";
            List<string> listFiles = new List<string>(System.IO.Directory.GetFiles(filesToRepopath + "\\", "*.dll"));
            if (listFiles.Count != 0)
            {
                if (channelToRep == null)
                {
                    createRepoChannel();
                }
                foreach (string file in listFiles)
                {
                    uploadLibFiles(file, file.Substring(file.LastIndexOf("\\") + 1));
                    Console.WriteLine("File" + file + "uploaded to repository");
                }
            }
            else
            {
                TestRes.Items.Insert(0, "DLL files not found");
            }

            //Submitting test request
            requestInvoke_testExec();
            //log invoker
            logInvoke_testExec();
        }

        public void requestInvoke_testExec() {
            string reqPath = "..\\..\\..\\TestRequest//testRequest.xml";
             Messages.Message msg = new Messages.Message();
            try
            {
                XDocument tReQDoc = new XDocument();
                tReQDoc = XDocument.Load(reqPath);
                TestRequest TRobj = tReQDoc.ToString().FromXml<TestRequest>();
                msg.body = tReQDoc.ToString();
                msg.type = "TestRequest";
                msg.from = Comm<ClientTH>.makeEndPoint("http://localhost", clientPort);
                msg.to = Comm<ClientTH>.makeEndPoint("http://localhost", testHarPort);
                msg.time = DateTime.Now;
                msg.author = TRobj.author;
                Console.WriteLine("REQUIREMENTS 2 and 6:");
                Console.WriteLine("The following test request has been submitted to the test harness");
                Console.WriteLine(msg.body);
                comm.sndr.PostMessage(msg);
                Console.WriteLine("");
            }
            catch (Exception ex) {
                TestRes.Items.Insert(0, "XML format not supported");
                Console.WriteLine("XML format not supported",ex.Message);
            }
        }
        public void logInvoke_testExec() {
            Messages.Message logMsg = new Messages.Message();
            logMsg.author = "Rahul Vijaydev";
            logMsg.to = Comm<ClientTH>.makeEndPoint("http://localhost", repPort);
            logMsg.from = Comm<ClientTH>.makeEndPoint("http://localhost", clientPort);
            logMsg.time = DateTime.Now;
            logMsg.type = "LogRequest";
            LogRequest lReq = new LogRequest();
            lReq.author = "Rahul Vijaydev";
            lReq.TestRequestName = "FirstTestRequest";
            logMsg.body = lReq.ToXml();
            comm.sndr.PostMessage(logMsg);
        }
    }
}

