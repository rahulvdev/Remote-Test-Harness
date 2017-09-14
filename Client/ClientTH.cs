/////////////////////////////////////////////////////////////////////
// Client.cs - Demonstrate application use of channel              //
// Ver 1.0                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Client package defines one class, Client, that uses the Comm<Client>
 * class to pass messages to a remote endpoint.
 * 
 * Required Files:
 * ---------------
 * - Client.cs
 * - ICommunicator.cs, Communicator.cs
 * - Messages.cs, MessageTest, Serialization
 *
 * Maintenance History:
 * --------------------
 * Ver 1.0 : 21st Nov 2016
 * - first release 
 *  
 */
using Communication;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessMessages;
using Utilities;

namespace Client
{
     public class ClientTH
    {
        public Comm<ClientTH> comm { get; set; } = new Comm<ClientTH>();

        public string endPoint { get; } = Comm<ClientTH>.makeEndPoint("http://localhost", 8085);

        private Thread rcvThread = null;

        //----< initialize receiver >------------------------------------

        public ClientTH()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }
        //----< join receive thread >------------------------------------

        public void wait()
        {
            rcvThread.Join();
        }
        //----< construct a basic message >------------------------------

        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }
        //----< use private service method to receive a message >--------

        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();
                if (msg.body == "quit")
                    break;
            }
        }
        public static string makeTestRequest()
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

        public static void Main(string[] args)
        {
            Console.Write("\n  Testing Client Demo");
            Console.Write("\n =====================\n");

            ClientTH client = new ClientTH();

            Message msg = client.makeMessage("Rahul Vijaydev", client.endPoint, client.endPoint);
            client.comm.sndr.PostMessage(msg);

            msg = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
            msg.body = makeTestRequest();
            client.comm.sndr.PostMessage(msg);

            string remoteEndPoint = Comm<ClientTH>.makeEndPoint("http://localhost", 8080);
            msg = msg.copy();
            msg.to = remoteEndPoint;
            client.comm.sndr.PostMessage(msg);

            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
            msg.to = client.endPoint;
            msg.body = "quit";
            client.comm.sndr.PostMessage(msg);
            client.wait();
            Console.Write("\n\n");
        }
        //----< run client demo >----------------------------------------

#if (TEST_CLIENTTH)
        public static void Main(string[] args)
        {
            Console.Write("\n  Testing Client Demo");
            Console.Write("\n =====================\n");

            ClientTH client = new ClientTH();

            Message msg = client.makeMessage("Rahul Vijaydev", client.endPoint, client.endPoint);
            client.comm.sndr.PostMessage(msg);

            msg = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
            msg.body = makeTestRequest();
            client.comm.sndr.PostMessage(msg);

            string remoteEndPoint = Comm<ClientTH>.makeEndPoint("http://localhost", 8080);
            msg = msg.copy();
            msg.to = remoteEndPoint;
            client.comm.sndr.PostMessage(msg);

            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
            msg.to = client.endPoint;
            msg.body = "quit";
            client.comm.sndr.PostMessage(msg);
            client.wait();
            Console.Write("\n\n");
        }
#endif

    }
}
