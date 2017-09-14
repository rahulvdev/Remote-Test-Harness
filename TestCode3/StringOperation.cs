/////////////////////////////////////////////////////////////////////////////
//  StringOperation.cs-   Counts the number of characters of a given type  //
//                        and returns the value back to the driver module  //
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
 *   This module performs a simple operation on a string object.It counts the number of characters of 
 *   a given type and returns the value back to the driver.
 *  
 *  
 *   Public Interface
 *   ----------------
 *   StringOperation sOp=new StringOperation();
 *   int getCharacterCount(string msg, char c);
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  StringOperation.cs
 *   - Compiler command: csc StringOperation.cs 
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
    public class StringOperation

    {
        public int getCharacterCount(string msg, char c)
        {
            //Console.WriteLine("The given string is "+msg);
            //Console.WriteLine("We need to find out the number of {0}'s in the given string "+msg);
            int count = 0;
            int charArrLength;
            if (msg.Length != 0)
            {
                char[] arr = msg.ToCharArray();
                charArrLength = arr.Length;
                for (int i = 0; i < charArrLength; i++)
                {
                    if (arr[i] == c)
                        count++;
                }
            }
            else
            {
                count = 0;
            }
            return count;
        }
#if (TEST_STRINGOPERATION)
        public static void Main(string[] args)
        {
            string userInput = Console.ReadLine();
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            char c = keyInfo.KeyChar;
            Console.WriteLine("Begin string operation");
            StringOperation strOp = new StringOperation();
            int charCount = strOp.getCharacterCount(userInput, c);
            Console.WriteLine("The number of" + c.ToString() + " " + "'s are");
        }
#endif
    }
}
