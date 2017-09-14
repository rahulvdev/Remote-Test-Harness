/////////////////////////////////////////////////////////////////////
// Server.cs - Demonstrate application use of channel              //
// Ver 1.0                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Server package defines one class, Server, that uses the Comm<Server>
 * class to receive messages from a remote endpoint.
 * 
 * Required Files:
 * ---------------
 * - Server.cs
 * - ICommunicator.cs, CommServices.cs
 * - Messages.cs, MessageTest, Serialization
 *
 * Maintenance History:
 * --------------------
 * Ver 1.0 : 21st November 2016
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

namespace Server
{
    public class ServerTH
    {
        public Comm<ServerTH> comm { get; set; } = new Comm<ServerTH>();

        public string endPoint { get; } = Comm<ServerTH>.makeEndPoint("http://localhost", 8080);

        private Thread rcvThread = null;

        public ServerTH()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }

        public void wait()
        {
            rcvThread.Join();
        }
        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

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

        public static void Main(string[] args)
        {
            Console.Write("\n  Testing Server Demo");
            Console.Write("\n =====================\n");

            ServerTH Server = new ServerTH();

            Message msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);

            ///////////////////////////////////////////////////////////////
            // uncomment lines below to enable sending messages to Client

            //Server.comm.sndr.PostMessage(msg);

            //msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);
            //msg.body = MessageTest.makeTestRequest();
            //Server.comm.sndr.PostMessage(msg);

            //string remoteEndPoint = Comm<Server>.makeEndPoint("http://localhost", 8081);
            //msg = msg.copy();
            //msg.to = remoteEndPoint;
            //Server.comm.sndr.PostMessage(msg);

            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
            msg.to = Server.endPoint;
            msg.body = "quit";
            Server.comm.sndr.PostMessage(msg);
            Server.wait();
            Console.Write("\n\n");
        }
#if (TEST_SERVERTH)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Server Demo");
            Console.Write("\n =====================\n");

            ServerTH Server = new ServerTH();

            Message msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);

            ///////////////////////////////////////////////////////////////
            // uncomment lines below to enable sending messages to Client

            //Server.comm.sndr.PostMessage(msg);

            //msg = Server.makeMessage("Fawcett", Server.endPoint, Server.endPoint);
            //msg.body = MessageTest.makeTestRequest();
            //Server.comm.sndr.PostMessage(msg);

            //string remoteEndPoint = Comm<Server>.makeEndPoint("http://localhost", 8081);
            //msg = msg.copy();
            //msg.to = remoteEndPoint;
            //Server.comm.sndr.PostMessage(msg);

            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
            msg.to = Server.endPoint;
            msg.body = "quit";
            Server.comm.sndr.PostMessage(msg);
            Server.wait();
            Console.Write("\n\n");
        }
#endif
    }
}
