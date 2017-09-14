/////////////////////////////////////////////////////////////////////////////
//  StringReversal.cs-   Checks if given string and string after reversal  // 
//                        are same.                                        //
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
 *   This module checks if given string and its version after reversal are the same.
 *  
 *  
 *   Public Interface
 *   ----------------
 *   StringReversal rev=new StringReversal();
 *   public char[] reverseString(string message);
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  StringReversal.cs
 *   - Compiler command: csc StringReversal.cs
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
    public class StringReversal
    {
        public char[] reverseString(string message)
        {
            char[] brev = message.ToCharArray();
            int arrLength = brev.Length;
            char[] arev = new char[arrLength];
            Array.Copy(brev, arev, arrLength);
            int index;
            char temp;
            int indexOfLastElement = arev.Length - 1;
            index = (arev.Length) / 2;
            for (int i = 0; i < index; i++)
            {
                temp = arev[i];
                arev[i] = arev[indexOfLastElement];
                arev[indexOfLastElement] = temp;
                indexOfLastElement--;
            }

            return arev;
        }


        //stub for string reversal
#if (TEST_STRINGREVERSAL)
        public static void Main(string[] args)
        {
            Console.WriteLine("Begin reverse operation");
            StringReversal rev = new StringReversal();
            string msg = "hello";
            char[] arr = rev.reverseString(msg);
            string revMsg = new string(arr);
            if (msg == revMsg)
                Console.WriteLine("Test passed");
            else
                Console.WriteLine("Test failed");
        }
#endif

    }
}
