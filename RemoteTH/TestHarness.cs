/////////////////////////////////////////////////////////////////////
// TestHarness.cs - Automates testing procedure                    //
//                                                                 //
// Rahul Vijaydev, CSE681 - Software Modeling and Analysis,        //
//Fall 2016                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * -------------------
 * The Test Harness is the testinf engine in a federal software system.It initiates the creation 
 * of a child Appdomain for the given test request.The test harness injects the loader and ITest 
 * interfaces into the appDomain and creates a proxy loader object to which the request
 * is passed.Every test runs on a separate child thread spawned by the test executive.Once the
 * results are generated,they are sent back from the thread using callback operation.
 * 
 * Public Interface:
 * -----------------
 * Message proccessTestRequest(Message msg, ICommunicator channel)
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
 * ver 1.0 : 21st November 2016
 * - first release
 */
using Communication;
using HRTimer;
using ITests;
using Logger;
using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;

namespace RemoteTestHarness
{
    public class CallBack : MarshalByRefObject, ICallback
    {
        public void sendMessage(Message msg)
        {
            Console.WriteLine("\n received message from childAppdomain :\"" + msg.body + "\"");
        }
    }

    class TestHarness
    {
        CallBack cb_;
        HiResTimer hrt;
        int BlockSize = 1024;
        byte[] block;
        public TestHarness()
        {
            cb_ = new CallBack();
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
        }
  
        public Message proccessTestRequest(Message msg, ICommunicator channel)
        {
            TestRequest tr = msg.body.FromXml<TestRequest>();
            TestLogger testLogger = new TestLogger();
            TestResults tResults = null;
            string filePath = null;
            Message resMsg = makeResMessage(msg);
            if (msg.body == "quit")
            {
                Console.WriteLine("Test Request is empty");
            }
            else
            {
                if (tr != null)
                {
                    Console.WriteLine( "\n  {0}\n  received message from:  {1}\n{2}\n  deserialized body:\n{3}",msg.to, msg.from, msg.body.shift(), tr.showThis());
                    filePath = loadAndProcessFiles(tr, msg.author, channel);
                    if (filePath == null)
                    {
                        Console.WriteLine("REQUIREMENT 3:Since filePath is null an ERROR message is returned");
                        resMsg.body = "ERROR:DLL files could not be found in the repository";
                        return resMsg;
                    }
                        string resFileName = filePath + "\\" + msg.author + "_" + tr.TestRequestName + "_" + "TestResult" + "_" + System.Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
                        AppDomain ad = createChildAppDomain();
                        ILoadAndTest iLdTest = installLoader(ad, filePath);
                        tResults = iLdTest.test(tr, testLogger, msg.author);
                        resMsg.body = tResults.ToXml();
                        writeTestResult(resMsg, resFileName, channel);
                        //sending logs to repository
                        Console.WriteLine("REQUIREMENT 7:Sending logs to repository");
                        fileUploader(testLogger.nameOfFile.Substring(testLogger.nameOfFile.LastIndexOf("\\") + 1), testLogger.nameOfFile, channel);
                        Console.WriteLine("REQUIREMENT 7:Unloading child appdomain");
                        AppDomain.Unload(ad);
                        try
                        {
                            System.IO.Directory.Delete(filePath, true);
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + filePath);
                            Console.WriteLine(ex.Message);
                        }
                }
            }
            return resMsg;
        }

        public Message makeResMessage(Message msg)
        {
            Message resMsg = new Message();
            resMsg.author = msg.author;
            resMsg.time = DateTime.Now;
            resMsg.type = "TestResult";
            resMsg.to = msg.from;
            resMsg.from = msg.to;
            return resMsg;
        }

        public void fileUploader(string fName, string pathOfFile, ICommunicator channel) {
            try
            {
                hrt.Start();
                using (var inputStream = new FileStream(pathOfFile, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = fName;
                    msg.transferStream = inputStream;
                    channel.uploadFiles(msg);
                }
                hrt.Stop();
                Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", fName, hrt.ElapsedMicroseconds);
            }
            catch(Exception ex)
            {
                Console.Write("\n  can't find \"{0}\"", pathOfFile);
                Console.WriteLine(ex.Message);
            }

        }

        ILoadAndTest installLoader(AppDomain ad, string filePath)
        {
            ad.Load("LoaderTH");
            //showAssemblies(ad);
            //Console.WriteLine();

            // create proxy for LoadAndTest object in child AppDomain

            ObjectHandle oh
              = ad.CreateInstance("LoaderTH", "LoaderTH.Loader");
            object ob = oh.Unwrap();
            // unwrap creates proxy to ChildDomain
            // Console.Write("\n  {0}", ob);
            // set reference to LoadAndTest object in child

            ILoadAndTest landt = (ILoadAndTest)ob;

            // create Callback object in parent domain and pass reference
            // to LoadAndTest object in child

            landt.setCallback(cb_);
            landt.loadPath(filePath);  // send file path to LoadAndTest
            return landt;
        }
        string makeKey(string author)
        {
            DateTime now = DateTime.Now;
            string nowDateStr = now.Date.ToString("d");
            string[] dateParts = nowDateStr.Split('/');
            string key = "";
            foreach (string part in dateParts)
                key += part.Trim() + '_';
            string nowTimeStr = now.TimeOfDay.ToString();
            string[] timeParts = nowTimeStr.Split(':');
            for (int i = 0; i < timeParts.Count() - 1; ++i)
                key += timeParts[i].Trim() + '_';
            key += timeParts[timeParts.Count() - 1];
            key = author + "_" + key;
            return key;
        }

        public void writeTestResult(Message msg, string path,ICommunicator channel) {

            Console.WriteLine("REQUIREMENT 7:");
            Console.WriteLine("Sending Test Result to repository");
            if (path != null)
            {
                //System.IO.StreamWriter(string path,bool append) variant
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.WriteLine(DateTime.Now + " : " + msg.body);
                }
            }
            fileUploader(path.Substring(path.LastIndexOf("\\") + 1), path, channel);
            Console.WriteLine("REQUIREMENT 8:");
            Console.WriteLine("Test Result name: "+path.Substring(path.LastIndexOf("\\") + 1));
        }
        public string loadAndProcessFiles(TestRequest req, string author, ICommunicator channel)
        {
            string localDir = makeKey(author);
            string filePath = System.IO.Path.GetFullPath(localDir);
            Console.Write("\n  creating local test directory \"" + localDir + "\"");
            System.IO.Directory.CreateDirectory(localDir);
            List<string> testContent = retrieveTestContent(req);
            Console.WriteLine("REQUIREMENT 6:");
            Console.WriteLine("Downloading libraries from repository");
            foreach (string file in testContent)
            {
                try
                {
                    hrt.Start();
                    int totalBytes = 0;
                    Stream strm = channel.downloadFile(file);
                    if (strm == null) {
                        filePath = null;
                        return filePath;
                    }
                    string rfilename = Path.Combine(filePath, file);
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);
                    using (var outputStream = new FileStream(rfilename, FileMode.Create))
                    {
                        while (true)
                        {
                            int bytesRead = strm.Read(block, 0, BlockSize);
                            totalBytes += bytesRead;
                            if (bytesRead > 0)
                                outputStream.Write(block, 0, bytesRead);
                            else
                                break;
                        }
                    }
                    hrt.Stop();
                    ulong time = hrt.ElapsedMicroseconds;
                    Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", file, totalBytes, time);
                }
                catch (Exception ex)
                {
                    filePath = null;
                    Console.Write("\n  {0}", ex.Message);
                }
            }
            return filePath;
        }

        public AppDomain createChildAppDomain()
        {
            try { 
                Console.WriteLine("REQUIREMENT 4:");
                Console.Write("\n  creating child AppDomain ");

                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase
                  = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library

                //Create evidence for the new AppDomain from evidence of current

                Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                // Create Child AppDomain

                AppDomain ad
                  = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

                Console.Write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
                return ad;
            }
            catch (Exception except)
            {
                Console.Write("\n  " + except.Message + "\n\n");
            }
            return null;
        }

        public List<string> retrieveTestContent(TestRequest req)
        {
            List<TestElement> listOfElements = req.tests;
            List<string> testFiles = new List<string>();
            foreach (TestElement tEle in listOfElements)
            {
                testFiles.Add(tEle.testDriver);
                foreach (string tCode in tEle.testCodes)
                {
                    testFiles.Add(tCode);
                }
            }
            return testFiles;
        }

#if (TEST_TESTHARNESS)
        public static void Main(string[] args) {
         string address = Comm<ServerTH>.makeEndPoint("http://localhost", 8082);
        Message msg=new Message();
        msg.author="Rahul";
        msg.time=DateTime.Now;
        msg.body="quit";
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            ICommunicator channel = factory.CreateChannel();
            TestHarness th=new TestHarness();
            th.processTestRequest(msg,channel);
        }
#endif
    }
}
