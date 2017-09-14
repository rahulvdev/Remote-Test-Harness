/////////////////////////////////////////////////////////////////////////////
//  ITest_Int.cs- Provides an interface to various test drivers            //
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
 *   This module defines 3 interfaces, the ITest interface, ILoadAndTest interface 
 *   and ICallback interfaces.
 *   
 *   Public Interface
 *   ----------------
 *   abstract string getLog();
 *   abstract void test();
 *   abstract TestResults test(TestRequest req,TestLogger log,string author);
 *   abstract void setCallback(ICallback cb);
 *   abstract void loadPath(string path);
 *   abstract void sendMessage(Message msg);
 *   
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  ITest.cs,ILoadAndTest.cs,ICallback.cs
 *   - Compiler command: csc ITest.cs,csc ILoadAndTest.cs,csc ICallback.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 21st November 2016
 *     - first release
 * 
 */
//





using Logger;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarnessMessages;

namespace ITests
{
    public interface ITest
    {
        bool test();
        string getLog();
    }
    public interface ILoadAndTest
    {
        TestResults test(TestRequest req,TestLogger log,string author);
        void setCallback(ICallback cb);
        void loadPath(string path);
    }
    public interface ICallback
    {
        void sendMessage(Message msg);
    }
}
