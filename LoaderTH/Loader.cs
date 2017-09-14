/////////////////////////////////////////////////////////////////////////////
//  Loader.cs -   Loads appropriate drivers and test code modules          //
//                into the appdomain                                       //
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
 *   The Loader receives the XML test request in the form of a string variant.
 *   It parses this string variant back to its orginal type.i.e.XDocument.
 *   The loader loads appropriate drivers and test code assemblies into the childAppdomain and 
 *   initiates testing.At every stage the loader uses the general and test log objects to save execution 
 *   states into log files that may be referred to after testing.
 *   
 *  
 *   Public Interface
 *   ----------------
 *   Loader ldr=new Loader();
 *   void loadTests(string xmlDoc, Logger testlogs, Logger generalLogs);
 *   void loadPath(string path);
 *   void setCallBack(ICallBack cb);
 *   TestResults test(TestRequest req, TestLogger testlogs, string author);
 *   ITest loadDriver(TestElement ele, TestLogger testLogs);
 *   bool run(ITest testDriver, string nameOfDriver, TestLogger testLogs);  
 *   TestResults parseResultCheck(TestRequest req, TestLogger testlogs, string author);
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  ITests.cs,Logger.cs,THMessage.cs
 *   - Compiler command: csc Logger.cs,csc THMessage.cs, 
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 21st November 2016
 *     - first release
 * 
 */
//

using HRTimer;
using ITests;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestHarnessMessages;

namespace LoaderTH
{
    public class Loader:MarshalByRefObject,ILoadAndTest
    {
        string loadPath_;
        private ICallback cb_ = null;
        HiResTimer hrtim = new HiResTimer();
        public void loadPath(string path)
        {
            loadPath_ = path;

            Console.Write("\n  loadpath = {0}", loadPath_);
        }

        public void setCallback(ICallback cb)
        {
            cb_ = cb;
        }

        public TestResults test(TestRequest req, TestLogger testlogs, string author)
        {

            TestResults tRes = null;
            testlogs.nameOfFile = loadPath_+"\\"+author + "_"+req.TestRequestName+"_" + System.Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
            Console.WriteLine("REQUIREMENT 8");
            Console.WriteLine("Log file name "+testlogs.nameOfFile);
            //testlogs.log("Test drivers and test code repository path is :" + loadPath_);
            try
            {
                if (req == null)
                {
                    testlogs.log("The test request is empty");
                }
                else
                {
                    tRes = parseResultCheck(req, testlogs, author);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return tRes;
        }

        public TestResults parseResultCheck(TestRequest req, TestLogger testlogs, string author)
        {
            
            TestResults tResults = new TestResults();
            //Check if list is empty.If not,loop through each test and recover driver and test code details
            if (req != null)
            {
                //iterating over each of the tests
                foreach (TestElement ele in req.tests)
                {
                    bool testResult = false;
                    TestResult tRes = new TestResult();
                    tRes.testName = ele.testName;
                    tRes.log = author + "_"+req.TestRequestName+"_" + System.Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt";
                    testlogs.log("Test Name : " + ele.testName);
                    testlogs.log("Test Time : " + DateTime.Now.ToString());
                    //loading tests and returning handle to driver object
                    ITest driver = loadDriver(ele, testlogs);
                    if (driver != null)
                    {
                        testResult = run(driver, ele.testDriver, testlogs);
                        tRes.passed = testResult;
                        string logFromTestImpl = driver.getLog();
                        if (logFromTestImpl != null)
                        {
                            testlogs.log(logFromTestImpl);
                        }
                        else
                        {
                            testlogs.log("No logs were generated from the test");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Driver does not exist");
                        testlogs.log("Driver does not exist");
                    }
                    tResults.timeStamp = DateTime.Now;
                    tResults.author = author;
                    tResults.results.Add(tRes);
                }
            }
            else
            {
                Console.WriteLine("There are no tests at the moment");
            }
            return tResults;
        }
        public ITest loadDriver(TestElement ele, TestLogger testLogs)
        {
            ITest testDriver = null;
            try
            {
                Assembly assem = null;
                Type[] types = null;
                List<string> testCode = ele.testCodes;
                //iterates over each libraries required for the test case
                foreach (string tCode in testCode)
                {
                    try
                    {
                        // loads test code assembly
                        assem = Assembly.LoadFrom(loadPath_ + "/" + tCode);
                        testLogs.log(tCode + " " + "loaded");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Test code could not be found in directory",ex);
                    }
                }
                try
                {
                    // loads the test driver asssembly
                    assem = Assembly.LoadFrom(loadPath_ + "/" + ele.testDriver);
                    testLogs.log(ele.testDriver + " " + "loaded");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to load driver from directory",ex);
                }
                types = assem.GetExportedTypes();
                //Iterates through each of the types until driver is encountered
                foreach (Type t in types)
                {
                    // checks if the type derives from ITest.ITest_Int
                    if (t.IsClass && typeof(ITest).IsAssignableFrom(t))
                        // create instance of test driver
                        testDriver = (ITest)Activator.CreateInstance(t);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Loader",ex);
                testLogs.log("Exception in loader");
                testDriver = null;
            }
            return testDriver;
        }
        public bool run(ITest testDriver, string nameOfDriver, TestLogger testLogs)
        {
            bool testRes = false;
            try
            {
                Console.Write(" Executing test code invoked by test driver   {0}   in Domain   {1} ", nameOfDriver, AppDomain.CurrentDomain.FriendlyName);
                testLogs.log("Executing test code invoked by test driver" + nameOfDriver + "in" + AppDomain.CurrentDomain.FriendlyName);
                hrtim.Start();
                if (testDriver.test() == true)
                {
                    Console.WriteLine("\n  test passed");
                    testLogs.log("test passed");
                    Console.WriteLine("REQUIREMENT 12: Test Execution time");
                    hrtim.Stop();
                    Console.WriteLine("Time taken for test is "+hrtim.ElapsedMicroseconds);
                    testRes = true;
                }
                else
                {
                    Console.WriteLine("\n  test failed");
                    testLogs.log("test failed");
                    testRes = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n",ex);
                Console.WriteLine("Exception occured in Driver {0}", nameOfDriver);
                testLogs.log("Exception occured in " + nameOfDriver);
                testRes = false;
            }
            return testRes;
        }
#if (TEST_LOADER)
        public static void Main(string[] args) {
        Console.WriteLine("Initiating loader operation");
            XDocument sampleDoc = new XDocument(new XComment("This is a comment"),
                               new XElement("RootNode", new XElement("ChildNode1", "Value1"),
                               new XElement("ChildNode2", "Value2"), new XElement("ChildNode3", "Value"),
                               new XElement("ChildNode4", "Value4")));
            string doc_ = sampleDoc.ToString();
            Loader ldr = new Loader();
            Logger genLogs = new Logger("../../../ LogResults / GeneralLogs / thLogs_" + DateTime.Now.ToString().Replace(" / ", " - ").Replace(":", " - ") + ".txt");
            Logger testExecLog = new Logger();
            ldr.loadTests(doc_, genLogs, testExecLog);
        }
#endif
    }
}
