/////////////////////////////////////////////////////////////////////////////
//  TestExecutive.cs -   Initiates test harness operation,childAppDomain   //
//                       creation and assembly loading                     //
//                                                                         //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Platform:     Windows 10,                                              //
//  Application:  Test Harness App                                         //
//  Author:       Rahul Vijaydev                                           //
//                                                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   The Test Executive hosts the WCF service that other modules client and repsoitory can subscribe
 *   to. It continuously monitors the receiver queue for incoming messages from the client and spawns
 *   separate threads for each of the messages.Message body is parsed into a Testrequest object and 
 *   sent to the test harness for testing.
 *   
 *   
 *  
 *   Public Interface
 *   ----------------
 *   TestExecutive<t> testEx=new TestExecutive<>();
 *   void serviceInitator();
 *   ICommunicator createChannelToRep();
 *   void rcvThreadProc();
 *   Message initiateTesting(Message msg);
 *   void processTestResult(Message msg);
 *   string makeTestRequest();
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  Client.cs,Communinicaton,HRTimer,Itests,Loader.cs,TestLogger.cs,
 *     Messages.Message.cs,Serialization,Server,THMessages.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 21st November 2016
 *     - first release
 * 
 */
//
using Client;
using Communication;
using Messages;
using RemoteTestHarness;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;

namespace RemoteTestHarness
{
    public class TestExecutive<T>

    {
        public Comm<T> comm { get; set; } = new Comm<T>();
        public ICommunicator channel = null;
        public int repPort = 8082;
        public int clientPort = 8085;
        public int THport = 8081;


        public TestExecutive() {
            Console.Title = "TEST HARNESS";
        }
        public void serviceInitator()
        {
            //string sndrEndPoint1 = Comm<T>.makeEndPoint("http://localhost", 8080);
            string rcvrEndPoint1 = Comm<T>.makeEndPoint("http://localhost", THport);
            comm.rcvr.CreateRecvChannel(rcvrEndPoint1);
            Thread rcvThread1 = comm.rcvr.start(rcvThreadProc);
            Console.Write("\n  rcvr thread id = {0}", rcvThread1.ManagedThreadId);
            Console.WriteLine();

        }

        public ICommunicator createChannelToRep()
        {

            string address = Comm<ServerTH>.makeEndPoint("http://localhost", repPort);
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            ICommunicator channel = factory.CreateChannel();
            return channel;

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
                if (msg.type == "TestRequest")
                {
                    Action<Message> processResult = (message) => { processTestResult(message); };
                    Task t = Task<Message>.Factory.StartNew(() => initiateTesting(msg))
                        .ContinueWith(antecedent => processResult(antecedent.Result));
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

  
        public Message initiateTesting(Message msg)
        {
            Console.WriteLine("REQUIREMENT 4:");
            Console.WriteLine("Processing message from author "+ msg.author+" on thread with thread id {0}", Thread.CurrentThread.ManagedThreadId);
            TestHarness tHar = new TestHarness();
            Console.WriteLine("REQUIREMENT 2:");
            Console.WriteLine("Message with following content has been sent from the client UI\n");
            Console.WriteLine(msg.body);
            channel = (channel == null) ? createChannelToRep() : channel;
            Message testResult = tHar.proccessTestRequest(msg, channel);
            return testResult;
        }

        public void processTestResult(Message msg)
        {
            Console.WriteLine("REQUIREMENTS 6 and 7:");
            Console.WriteLine("The following message is the test result that is being sent to the client");
            Console.WriteLine("The resulting message is {0}", msg);
            Console.WriteLine("Result of thread with thread id :" + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
            Console.WriteLine(msg.body.shift());
            comm.sndr.PostMessage(msg);
        }

        public string makeTestRequest()
        {
            TestElement te1 = new TestElement("test1");
            te1.addDriver("TestDriver.dll");
            te1.addCode("TestCode1.dll");
            te1.addCode("TestCode2.dll");
            TestElement te2 = new TestElement("test2");
            te2.addDriver("TestDriver3.dll");
            te2.addCode("TestCode4.dll");
            //te2.addCode("tc4.dll");
            TestRequest tr = new TestRequest();
            tr.author = "Rahul Vijaydev";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            return tr.ToXml();
        }
    }

    class EntryPoint
    {
        public static void Main(string[] args)
        {
            TestExecutive<ServerTH> tex1 = new TestExecutive<ServerTH>();
            //TestExecutive<ClientTH> tex2 = new TestExecutive<ClientTH>();
            tex1.serviceInitator();
           /* string sndrEndPoint1 = Comm<ClientTH>.makeEndPoint("http://localhost", 8080);
            string rcvrEndPoint1 = Comm<ServerTH>.makeEndPoint("http://localhost", 8080);

            Message msg = null;
            string rcvrEndPoint;

            for (int i = 0; i < 1; ++i)
            {
                msg = new Message(tex1.makeTestRequest());
                msg.type = "TestRequest";
                msg.from = sndrEndPoint1;
                msg.to = rcvrEndPoint = rcvrEndPoint1;
                msg.author = "Rahul Vijaydev";
                msg.time = DateTime.Now;

                tex1.comm.sndr.PostMessage(msg);

                Console.WriteLine("\n  {0}\n  posting message with body:\n{1}", msg.from, msg.body.shift());
                //Thread.Sleep(20);
            }*/
        }
    }
}

