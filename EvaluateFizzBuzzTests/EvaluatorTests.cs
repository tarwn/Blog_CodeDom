using EvaluateFizzBuzz.Evaluation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluateFizzBuzzTests
{

    [TestFixture]
    public class EvaluatorTests
    {
        static string ConcatenationNamespace = "Concatenation";
        static string ConcatenationClassName = "Concatenator";
        static string ConcatenationCode = @"

            using System;
            namespace Concatenation {
                public class Concatenator {
                    public static string DoSomething(string input){ return input + ""prime""; }
                }
            }";
        static string ExceptionCode = @"

            using System;
            namespace Concatenation {
                public class Concatenator {
                    public static string DoSomething(string input){ return (1/(input.Length - input.Length)).ToString(); }
                }
            }";

        [Test]
        public void NormalizeCode_MethodWithNoNamespaceOrClass_IsWrappedInBoth()
        {
            string code = "public string DoSomething(string input){ return input + \"prime\"; }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("using System;"), "Does not contain the using statements");
            Assert.IsTrue(actual.Contains("namespace MyNamespace"), "Does not contain the namespace");
            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
        }

        [Test]
        public void NormalizeCode_StaticClassWithNoNamespace_IsWrappedInNamespace()
        {
            string code = "public static class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("using System;"), "Does not contain the using statements");
            Assert.IsTrue(actual.Contains("namespace MyNamespace"), "Does not contain the namespace");
            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
        }

        [Test]
        public void NormalizeCode_StaticClassWithNoNamespace_ClassIsRenamed()
        {
            string code = "public static class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
            Assert.IsFalse(actual.Contains("MyOriginalClassName"), "Contains the original classname");
        }

        [Test]
        public void NormalizeCode_ClassWithNoNamespace_IsWrappedInNamespace()
        {
            string code = "public class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("using System;"), "Does not contain the using statements");
            Assert.IsTrue(actual.Contains("namespace MyNamespace"), "Does not contain the namespace");
            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
        }

        [Test]
        public void NormalizeCode_ClassWithNoNamespace_ClassIsRenamed()
        {
            string code = "public class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
            Assert.IsFalse(actual.Contains("MyOriginalClassName"), "Contains the original classname");
        }

        [Test]
        public void NormalizeCode_NamespacedCode_AddsUsingStatements()
        {
            string code = "namespace OriginalNamespace { public static class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("using System;"), "Does not contain the using statements");
            Assert.IsTrue(actual.Contains("namespace MyNamespace"), "Does not contain the namespace");
            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
        }

        [Test]
        public void NormalizeCode_NamespacedCode_RenamesNamespace()
        {
            string code = "namespace OriginalNamespace { public static class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("namespace MyNamespace"), "Does not contain the namespace");
            Assert.IsFalse(actual.Contains("namespace OriginalNamespace"), "Contains the original namespace");
        }

        [Test]
        public void NormalizeCode_RunOnCodeLine_ReplacesOriginalClassAndNamespace()
        {
            string code = "namespace OriginalNamespace { public static class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } } }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsFalse(actual.Contains("MyOriginalClassName"), "Contains the original classname");
            Assert.IsFalse(actual.Contains("namespace OriginalNamespace"), "Contains the original namespace");
        }

        [Test]
        public void NormalizeCode_TwoClasses_OnlyFirstClassIsRenamed()
        {
            string code = "public class MyOriginalClassName { public string DoSomething(string input){ return input + \"prime\"; } }";
            code += "\npublic class MyOtherClass { public void ImportantFunction() {} }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("public class MyClassName"), "Does not contain the classname");
            Assert.IsFalse(actual.Contains("MyOriginalClassName"), "Contains the original classname");
            Assert.IsTrue(actual.Contains("public class MyOtherClass"), "Does not contain the second classname");
        }

        [Test]
        public void NormalizeCode_StaticMethod_IsConvertedToInstance()
        {
            string code = "public static string DoSomething(string input){ return input + \"prime\"; }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("public string DoSomething(string input)"), "Does not contain the method without static");
        }

        [Test]
        public void NormalizeCode_MultipleStaticMethods_AllAreConvertedToInstance()
        {
            string code = @"public static string DoSomething(string input){ return input + ""prime""; }
                            public static void DoSomething2(int input){ return input.ToString() + ""prime""; }";

            var evaluator = new Evaluator("MyNamespace", "MyClassName", new List<TestDefinition>());
            var actual = evaluator.NormalizeCode(code);

            Assert.IsTrue(actual.Contains("public string DoSomething(string input)"), "Does not contain the first method without static");
            Assert.IsTrue(actual.Contains("public void DoSomething2(int input)"), "Does not contain the second method without static");
        }

        [Test]
        public void Evaluate_GoodCode_PassesWithOneSuccessfulResult()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "MyInputprime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ConcatenationCode);

            Assert.AreEqual(1, actual.Tests.Count);
            Assert.IsTrue(actual.Tests[0].IsPass);
            Assert.IsFalse(actual.Tests[0].IsError);
        }

        [Test]
        public void Evaluate_BadCode_FailsWithOneFailedResult()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "MyInput2prime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ConcatenationCode);

            Assert.AreEqual(1, actual.Tests.Count);
            Assert.IsFalse(actual.Tests[0].IsPass);
            Assert.IsFalse(actual.Tests[0].IsError);
        }

        [Test]
        public void Evaluate_NonBuildableCodeExample_FailsWithCompileError()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "MyInput2prime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ConcatenationCode + " broken stuff at the end");

            Assert.IsTrue(actual.CouldNotExecute);
            Assert.AreEqual(1, actual.Tests.Count);
            Assert.IsFalse(actual.Tests[0].IsPass);
            Assert.IsTrue(actual.Tests[0].IsError);
        }

        [Test]
        public void Evaluate_CodeWithExceptionExample_FailsTestWithError()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "MyInputprime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ExceptionCode);

            Assert.IsFalse(actual.CouldNotExecute);
            Assert.AreEqual(1, actual.Tests.Count);
            Assert.IsFalse(actual.Tests[0].IsPass);
            Assert.IsTrue(actual.Tests[0].IsError);
        }

        [Test]
        public void Evaluate_GoodCodeWithMultipleTests_PassesWithMatchingSuccessfulResults()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "MyInputprime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput2" }, "MyInput2prime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput3" }, "MyInput3prime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ConcatenationCode);

            Assert.AreEqual(3, actual.Tests.Count);
            foreach (var test in tests)
            {
                var matchingResult = actual.Tests.Where(r => r.TestName == test.Name).First();
                Assert.IsTrue(matchingResult.IsPass);
                Assert.IsFalse(matchingResult.IsError);
            }
        }

        [Test]
        public void Evaluate_BadCodeWithMultipleTests_FailsWithMatchingFailResults()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInputprime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInput2prime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInput3prime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ConcatenationCode);

            Assert.AreEqual(3, actual.Tests.Count);
            foreach (var test in tests)
            {
                var matchingResult = actual.Tests.Where(r => r.TestName == test.Name).First();
                Assert.IsFalse(matchingResult.IsPass);
                Assert.IsFalse(matchingResult.IsError);
            }
        }

        [Test]
        public void Evaluate_ExceptionGenerationCodeWithMultipleTests_FailsWithMatchingErrorResults()
        {
            var tests = new List<TestDefinition>() { 
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInputprime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInput2prime"),
                new TestDefinition("basic concatentation test", new object[]{ "MyInput" }, "ZZZMyInput3prime")
            };

            var evaluator = new Evaluator(ConcatenationNamespace, ConcatenationClassName, tests);
            var actual = evaluator.Evaluate(ExceptionCode);

            Assert.AreEqual(3, actual.Tests.Count);
            foreach (var test in tests)
            {
                var matchingResult = actual.Tests.Where(r => r.TestName == test.Name).First();
                Assert.IsFalse(matchingResult.IsPass);
                Assert.IsTrue(matchingResult.IsError);
            }
        }
    }
}
