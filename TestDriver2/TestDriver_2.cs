/////////////////////////////////////////////////////////////////////////////
//  TestDriver_2.cs-   Tests aspects of TestCode1 and TestCode2            //
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
 *  The TestDriver2 class implements the ITest_Int interface and defines the test() and getLog() methods.
 *  The test() method creates an object of TestCode3 that it intends to run a test on.It checks the number of
 *  a given type of character in a string and returns the value for it to be logged.
 *  
 *   Public Interface
 *   ----------------
 *   Driver2 dr2=new Driver2();
 *   void test();
 *   string getLog();
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  TestDriver_2.cs,TestCode3.cs
 *   - Compiler command: csc TestDriver2.cs,TestCode3.cs
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

namespace TestDriver2
{
    public class TestDriver_2:ITest
    {
        StringBuilder builder = new StringBuilder("Running test code");
        string characterCount;

        public string getLog()
        {
            builder.Append(Environment.NewLine);
            builder.Append("The number of specified characters in the given string is" + " " + characterCount);
            return builder.ToString();
        }

        public bool test()
        {
            Console.WriteLine("REQUIREMENT 5:");
            Console.WriteLine("TestDriver2.TestDriver_2 deriving from ITest");
            StringOperation strOp = new StringOperation();
            int charCount = strOp.getCharacterCount("malayalam", 'm');
            characterCount = charCount.ToString();
            return false;
        }

#if (TEST_TESTDRIVER_2)
        public static void Main(string[] args)
        {
            Console.WriteLine("Invoking test method from the driver");
            TestDriver_2 d2 = new TestDriver_2();
            d2.test();
            Console.WriteLine("Invoking getLog() method from the driver");
            string log = d2.getLog();
            Console.WriteLine("The log results are as follows: ");
            Console.WriteLine(log);
        }
#endif
    }
}
