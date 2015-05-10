using EvaluateFizzBuzz.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluateFizzBuzz
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Requires two arguments: the file path to test and the file path for the results");
                WaitForEnterToExit();
                return;
            }

            var fnfoSource = new FileInfo(args[0]);
            if (!fnfoSource.Exists)
            {
                Console.Error.WriteLine("The specified source file could not be found");
                WaitForEnterToExit();
                return;
            }

            var fnfoTarget = new FileInfo(args[1]);
            if (fnfoSource.Exists)
            {
                try
                {
                    fnfoTarget.Delete();
                }
                catch (Exception exc)
                {
                    Console.Error.WriteLine("The targt file already exists and could not be deleted. Here is the exception:");
                    Console.Error.WriteLine(exc.ToString());
                    WaitForEnterToExit();
                    return;
                }
            }

            var originalSource = File.ReadAllText(args[0]);

            var evaluator = new Evaluator("FizzBuzzSample", "FizzBuzzClass", new List<TestDefinition>(){
                new TestDefinition("Standard number is returned as string", new object[]{ 1 }, "1"),
                new TestDefinition("3 is returned as 'Fizz'", new object[]{ 3 }, "Fizz"),
                new TestDefinition("5 is returned as 'Buzz'", new object[]{ 5 }, "Buzz"),
                new TestDefinition("15 is returned as 'FizzBuzz'", new object[]{ 15 },"FizzBuzz"),
                new TestDefinition("9 is returned as 'Fizz'", new object[]{ 9 }, "Fizz"),
                new TestDefinition("20 is returned as 'Buzz'", new object[]{ 20 }, "Buzz"),
                new TestDefinition("30 is returned as 'FizzBuzz'", new object[]{ 30 }, "FizzBuzz")
            });
            var normalizedSource = evaluator.NormalizeCode(originalSource);
            var results = evaluator.Evaluate(normalizedSource);

            var sb = new StringBuilder();
            sb.AppendFormat("Evaluated File: {0}\r\nEvaluation Time: {1}\r\nFinal Result: {2}\r\n\r\n",
                            fnfoSource.Name,
                            DateTime.Now,
                            results.Summary);

            sb.AppendFormat("Individual Results:\r\n{0}\r",
                            String.Join("\r\n", results.Tests.Select(t =>
                            {
                                if (t.IsError)
                                {
                                    return String.Format("\tError\t- {0}: {1}",
                                                            t.TestName,
                                                            t.ActualResult);
                                }
                                else if (!t.IsPass)
                                {
                                    return String.Format("\tFail\t- {0}: Expected '{1}', Actual was '{2}'",
                                                            t.TestName,
                                                            t.ExpectedResult.ToString(),
                                                            t.ActualResult.ToString());
                                }
                                else
                                {
                                    return String.Format("\tPass\t- {0}", t.TestName);
                                }
                            })));

            sb.AppendFormat("\r\n\r\n-------------- Original Code ----------------\r\n\r\n{0}", originalSource);
            sb.AppendFormat("\r\n\r\n------------------ Normalized Code ---------------\r\n{0}\r\n", normalizedSource);

            File.WriteAllText(args[1], sb.ToString());

            Console.WriteLine(String.Format("Evaluation Complete, results saved to {0}", args[1]));
            WaitForEnterToExit();
        }

        private static void WaitForEnterToExit()
        {
            Console.WriteLine("Hit enter to exit...");
            Console.Read();
        }
    }
}
