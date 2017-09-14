/////////////////////////////////////////////////////////////////////
// ICommunicator.cs - Defines service and message contracts        //
// ver 2.0                                                         //
// Jim Fawcett, CSE681 - Software Modeling & Analysis, Summer 2011 //
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 2.0 : 10 Oct 11
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 *   Included [Message Contract] of type FileTransferMessage for file downloading
 *   and uploading puroposes
 * ver 1.0 : 21st November 16
 * - first release
 */

using Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    [ServiceContract]
    public interface ICommunicator
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(Message msg);
        [OperationContract(IsOneWay = true)]
        void uploadFiles(FileTransferMessage msg);
        [OperationContract]
        Stream downloadFile(string filename);
        Message GetMessage();
    }
    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand =true)]
        public string filename { get; set; }
        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
