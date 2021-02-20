using System;

//Basic Driver Code
//C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe .\main_test.cs .\CommandEvaluator.cs
namespace Main_Testing
{
    class DriverClass {
        static void Main(string[] args) {
            Console.WriteLine("In Main");

            CommandEvaluator ce = new CommandEvaluator();
            ce.eval("helloWorld");
            ce.eval("helloWorld", null);

            int[] test_ints = {1,2,3,4,5};
            ce.eval("printList", new object[] {test_ints});

            Console.WriteLine("Press Any Key To Close.");
            Console.ReadKey();
        }
    }
}