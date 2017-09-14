/////////////////////////////////////////////////////////////////////////////
//  CalculatorTc1.cs-   Checks for possible divide by zero exception          //
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
 *   This is a simple calculator module that returns  a test pass or test fail command in the form of 
 *   a boolean value back to the driver that ran the code.A divide by zero exception is thrown if the 
 *   denominator in division operation is 0.In this case the tests fails and returns the boolean value false.
 *   If not, true is returned.
 *  
 *  
 *   Public Interface
 *   ----------------
 *   CalculatorTc1 cal1=new CalculatorTc1();
 *   bool exceptionMonitor(char x, int op1, int op2);
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  CalculatorTc1.cs
 *   - Compiler command: csc CalculatorTc1.cs 
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 21st November 2016
 *     - first release
 * 
 */
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCode

{
    public class CalculatorTc1
    {
        public bool exceptionMonitor(char x, int op1, int op2)
        {
            int res;
            bool testResult = false;
            //Console.WriteLine("You have submitted {0} and {1} as arguments to perform {2} operation",op1,op2,x);
            switch (x)
            {
                case '+':
                    res = op1 + op2;
                    testResult = true;
                    break;
                case '-':
                    if (op1 > op2)
                    {
                        res = op1 - op2;
                        testResult = true;
                    }
                    else
                    {
                        res = op2 - op1;
                        testResult = true;
                    }
                    break;
                case '*':
                    res = op1 * op2;
                    testResult = true;
                    break;
                case '/':
                    if (op2 == 0)
                    {
                        //Console.WriteLine("Prospective Divide By zero Exception");
                        testResult = false;
                    }
                    else
                    {
                        res = op1 / op2;
                        testResult = true;
                    }
                    break;
            }
            return testResult;
        }

#if (TEST_CALCULATORTC1)
        public static void Main(string[] args)
        {
            Console.WriteLine("Begin calculation");
            CalculatorTc1 cal = new CalculatorTc1();
            cal.exceptionMonitor('/', 5, 2);
            cal.exceptionMonitor('/', 10, 0);
        }
#endif
    }
}
