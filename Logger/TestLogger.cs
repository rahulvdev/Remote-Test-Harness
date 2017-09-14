/////////////////////////////////////////////////////////////////////////////
//  Logger.cs-   Writes log messages to text files                        //
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
 *   The main motivation of the logger is for writing test harness and test code execution details 
 *   into corresponding text files.
 *  
 *   Public Interface
 *   ----------------
 *   Logger logger=new Logger();
 *   void log(String text);
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  TestLogger.cs
 *   - Compiler command: csc TestLogger.cs
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class TestLogger:MarshalByRefObject
    {
        public string nameOfFile { get; set; }

        public TestLogger(String name)
        {
            nameOfFile = name;
        }
        //empty constructor
        //for test logs, file name will be available after parsing XML
        public TestLogger()
        {

        }

        // Appending text to the file during every call to the method
        public void log(String text)
        {
            if (nameOfFile != null)
            {
                //System.IO.StreamWriter(string path,bool append) variant
                using (StreamWriter file = new StreamWriter(nameOfFile, true))
                {
                    file.WriteLine(DateTime.Now + " : " + text);
                }
            }
        }
#if (TEST_TESTLOGGER)
        public static void Main(string[] args)
        {
            Console.WriteLine("Creating Logger instance");
            TestLogger lgr = new TestLogger("../../../ LogResults / GeneralLogs / thLogs_" + DateTime.Now.ToString().Replace(" / ", " - ").Replace(":", " - ") + ".txt");
            lgr.log("Testing logger module");
        }
#endif
    }
}
