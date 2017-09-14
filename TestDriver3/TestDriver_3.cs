/////////////////////////////////////////////////////////////////////////////
//  TestDriver_3.cs-   Tests aspects of TestCode4                          //
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
 *  This module runs a test on TestCode4 i.e StringRevarsal and compares if the given string and
 *  string generated after reversal are the same.
 *  
 *   Public Interface
 *   ----------------
 *   Driver3 dr3=new Driver3();
 *   void test();
 *   string getLog();
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  TestDriver3.cs,StringReversal.cs
 *   - Compiler command: csc TestDriver3.cs,TestCode4.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 21st November 2016
 *     - first release
 * 
 */
//
using ITests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCode;

namespace TestDriver3
{
    public class TestDriver_3:ITest
    {
        private StringBuilder logResult = new StringBuilder();
        public string getLog()
        {
            string log;
            if (logResult == null)
                log = logResult.Append("Test not executed yet").ToString();
            else
                log = logResult.ToString();
            return log;
        }

        public bool test()
        {
            Console.WriteLine("REQUIREMENT 5:");
            Console.WriteLine("TestDriver3.TestDriver_3 deriving from ITest");
            bool comp = false;
            string msg = "malayalam";
            logResult.Append("The given string is:" + msg);
            char[] bRev = msg.ToCharArray();
            StringReversal rev = new StringReversal();
            char[] aRev = rev.reverseString("malayalam");
            logResult.Append("Before reversal the string is: " + msg);
            logResult.Append(Environment.NewLine);
            //Console.WriteLine("\n");
            string afterRev = new string(aRev);
            logResult.Append("After reversal the new string is: " + afterRev);
            logResult.Append(Environment.NewLine);
            //Console.WriteLine("\n");
            if (msg == afterRev)
            {
                logResult.Append("Test passed");
                comp = true;
            }
            else
            {
                logResult.Append("Test failed");
            }
            return comp;
        }

#if (TEST_TESTDRIVER_3)
        public static void Main(string[] args)
        {
            TestDriver_3 d3 = new TestDriver_3();
            bool testRes = d3.test();
            string log = d3.getLog();
            if (testRes)
                Console.WriteLine("Test passed");
            else
                Console.WriteLine("Test failed");
        }
#endif
    }
}
