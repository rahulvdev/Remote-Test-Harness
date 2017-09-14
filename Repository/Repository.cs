/////////////////////////////////////////////////////////////////////
// Repository.cs - holds test code for TestHarness                 //
//                                                                 //
// Rahul Vijaydev, CSE681 - Software Modeling and Analysis,        //
//Fall 2016                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Repository holds the necessary library files that are fetched by the test harness for testing.
 * Files are fetched upon submisson of a download file request.The repository also supports queries
 * from the user through the GUI for test logs and Test Results independently.
 * 
 * Public Interface:
 * -----------------
 * void serviceInitator();
 * Stream downloadFile(string fName);
 * void uploadFile(FileTransferMessage msg);
 * void rcvThreadProc();
 * Message processLogQuery(Message msg);
 * Message getFiles(string logQuery,Message msg);
 * void processLogResult(Message msg);
 * 
 * Required Files:
 * - HRTimer.cs,Communication,Message.cs,Serialzation,THMessage.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 20 Oct 2016
 * - first release
 */
using Communication;
using HRTimer;
using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;

namespace RepositoryTH
{
    public class Repository<T>
    {
        public Comm<T> comm = new Comm<T>();

        string savePath = "..\\..\\..\\RepositoryFiles";
        string ToSendPath = "..\\..\\..\\RepositoryFiles";
        int BlockSize = 1024;
        byte[] block;
        HiResTimer hrt = null;
        public int repoPort = 8082;
        public int clientPort = 8085;
        public int THport = 8081;



        public Repository()
        {
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
            Console.Title = "REPOSITORY";
        }

        public void serviceInitator()
        {
            //string sndrEndPoint1 = Comm<InitRepo>.makeEndPoint("http://localhost", 8081);
            Receiver<T>.uFileDel = uploadFile;
            Receiver<T>.dFileDel = downloadFile;
            string rcvrEndPoint1 = Comm<T>.makeEndPoint("http://localhost", repoPort);
            comm.rcvr.CreateRecvChannel(rcvrEndPoint1);
            Thread rcvThread1 = comm.rcvr.start(rcvThreadProc);
            Console.Write("\n  rcvr thread id = {0}", rcvThread1.ManagedThreadId);
            Console.WriteLine();

        }
   
        public Stream downloadFile(string fName)
        {
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, fName);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                outStream = null;
            hrt.Stop();
            Console.Write("\n  Sent \"{0}\" in {1} microsec.", fName, hrt.ElapsedMicroseconds);
            return outStream;
        }

        public void uploadFile(FileTransferMessage msg)
        {

            int totalBytes = 0;
            hrt.Start();
            string filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt.ElapsedMicroseconds
            );

        }

        public void rcvThreadProc()
        {
            List<Task> taskList = new List<Task>();


            while (true)
            {
                Message msg = new Message();
                msg = comm.rcvr.GetMessage();
                Console.Write("\n  getting message on rcvThread {0}", Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine();
                if (msg.type == "LogRequest")
                {
                    Action<Message> LogResult = (message) => { processLogResult(message); };
                    Task t = Task<Message>.Factory.StartNew(() => processLogQuery(msg))
                        .ContinueWith(antecedent => LogResult(antecedent.Result));
                    taskList.Add(t);

                }
                else
                {
                    Console.Write("\n  {0}\n  received message from:  {1}\n{2}", msg.to, msg.from, msg.body.shift());
                    if (msg.body == "quit")
                        break;
                }
            }
            Task.WaitAll(taskList.ToArray());
            Console.Write("\n  receiver {0} shutting down\n");

        }
        public Message processLogQuery(Message msg)
        {
            Console.WriteLine("REQUIREMENT 9:");
            Console.WriteLine("Following Log Query sent from client");
            Console.WriteLine(msg.body.shift());
            Console.WriteLine("Processing on thread with thread id {0}", Thread.CurrentThread.ManagedThreadId);
            LogRequest lRes = msg.body.FromXml<LogRequest>();
            string author = lRes.author;
            string testReqName = lRes.TestRequestName;
            string logQuery = author + "_" + testReqName + "_";
            Message testResult = getFiles(logQuery,msg);
            return testResult;
        }

        public Message getFiles(string logQuery,Message msg)
        {
            StringBuilder logBuild = new StringBuilder();
            string logSender = msg.from;
            string logReceiver = msg.to;
            Message logRes = new Message();
            logRes.to = logSender;
            int noOfResFiles = 0;
            logRes.from = logReceiver;
            logRes.type = "LogResult";
            logRes.author = msg.author;
            logRes.time = DateTime.Now;
            List<string> logFiles = new List<string>(Directory.GetFiles(savePath, logQuery + "*"));
            foreach (string file in logFiles)
            {
                if (file != null)
                {
                    if (noOfResFiles <= 3)
                    {
                        if (File.Exists(file))
                        {
                            noOfResFiles++;
                            logBuild.AppendLine(Environment.NewLine);
                            string[] logContent = File.ReadAllLines(file);
                            foreach (string content in logContent)
                            {
                                logBuild.Append(content);
                            }
                            logBuild.AppendLine(Environment.NewLine);
                        }
                    }
                }
            }
            if (logBuild.Length < 1)
            {
                logBuild.Append("Requested logs were not available in the repository");
            }
            logRes.body = logBuild.ToString();
            //append stringBuilder content to msg body and return
            return logRes;
        }

        public void processLogResult(Message msg)
        {
            Console.WriteLine("The resulting message is {0}", msg);
            Console.WriteLine("Result pf thread with thread id :" + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
            Console.WriteLine(msg.body.shift());
            comm.sndr.PostMessage(msg);
        }
    }


    public class RepInit {
        public static void Main(string[] args)
        {
            Repository<InitRepo> rep = new Repository<InitRepo>();
            rep.serviceInitator();
        }

    }

    public class InitRepo {
        
    }
}
