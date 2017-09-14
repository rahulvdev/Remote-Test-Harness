/////////////////////////////////////////////////////////////////////////////
//  CalculatorTc2.cs-   Performs arithmetic calculations and records the   //
//                      flow of execution in a StringBuilder object        //
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
 *   This is a simple calculator module that returns a StringBuilder object representing the 
 *   flow of execution of the test code.
 *  
 *  
 *   Public Interface
 *   ----------------
 *   CalculatorTc2 cal2=new CalculatorTc2();
 *   StringBuilder calculationFlow(char x, int op1, int op2)
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  CalculatorTc2.cs
 *   - Compiler command: csc Calculator Tc2.cs
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

namespace TestCode2
{
    public class CalculatorTc2

    {
        public StringBuilder calculationFlow(char x, int op1, int op2)
        {
            StringBuilder builder = new StringBuilder("Code 2 being tested");
            int res;
            builder.Append(Environment.NewLine);
            builder.Append("This code performs simple arithmetic operations on the given data");
            builder.Append(Environment.NewLine);
            builder.Append("In this code, a divide by zero exception is handled by a simple condition checker");
            builder.Append("Executing code..");
            builder.Append(Environment.NewLine);
            switch (x)
            {
                case '+':
                    builder.Append("performing addition operation");
                    res = op1 + op2;
                    break;
                case '-':
                    builder.Append("performing subtraction operation");
                    if (op1 > op2)
                    {
                        res = op1 - op2;
                    }
                    else
                    {
                        res = op2 - op1;
                    }
                    break;
                case '*':
                    builder.Append("performing multiplication operation");
                    res = op1 * op2;
                    break;
                case '/':
                    builder.Append("performing division operation");
                    builder.Append(Environment.NewLine);
                    if (op2 == 0)
                    {
                        builder.Append("Denominator is zero");
                        builder.Append(Environment.NewLine);
                        builder.Append("Prospective divide by zero exception");
                    }
                    else
                    {
                        res = op1 / op2;
                        builder.Append("No divide by zero exception");
                        builder.Append(Environment.NewLine);
                        builder.Append("Division succesfully carried out");
                    }
                    break;
            }
            return builder;
        }
#if (TEST_CALCULATORTC2)
        public static void Main(string[] args)
        {
            Console.WriteLine("Begin calculation");
            CalculatorTc2 cal = new CalculatorTc2();
            cal.calculationFlow('/', 5, 2);
            cal.calculationFlow('/', 10, 0);
        }
#endif
    }
}
