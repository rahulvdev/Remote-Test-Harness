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

namespace Client2
{
    public class ClientTH_2
    {
        public Comm<ClientTH_2> comm { get; set; } = new Comm<ClientTH_2>();

        public string endPoint { get; } = Comm<ClientTH_2>.makeEndPoint("http://localhost", 8085);

        private Thread rcvThread = null;

        //----< initialize receiver >------------------------------------

        public ClientTH_2()
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
        //----< run client demo >----------------------------------------

#if (TEST_CLIENTTH_2)
        public static void Main(string[] args)
        {
            Console.Write("\n  Testing Client Demo");
            Console.Write("\n =====================\n");

            ClientTH_2 client = new ClientTH_2();

            Message msg = client.makeMessage("Rahul Vijaydev", client.endPoint, client.endPoint);
            client.comm.sndr.PostMessage(msg);

            msg = client.makeMessage("Fawcett", client.endPoint, client.endPoint);
            msg.body = makeTestRequest();
            client.comm.sndr.PostMessage(msg);

            string remoteEndPoint = Comm<ClientTH_2>.makeEndPoint("http://localhost", 8080);
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
