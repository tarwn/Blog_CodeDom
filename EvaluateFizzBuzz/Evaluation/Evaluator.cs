using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvaluateFizzBuzz.Evaluation
{
    public class Evaluator
    {
        public static string USING_STATEMENTS = @"
using System;
using System.Collections.Generic;
using System.Linq;
";

        public Evaluator(string intendedNamespace, string intendedClassname, List<TestDefinition> tests)
        {
            IntendedNamespace = intendedNamespace;
            IntendedClassName = intendedClassname;
            Tests = tests;
        }

        public string IntendedNamespace { get; private set; }

        public string IntendedClassName { get; private set; }

        public List<TestDefinition> Tests { get; private set; }

        public string NormalizeCode(string code)
        {
            var normalizedSource = code;

            // normalize class name
            var classnameRegex = new Regex("(public|private) (static )?class [^{]+{");
            if (classnameRegex.IsMatch(normalizedSource))
            {
                normalizedSource = classnameRegex.Replace(normalizedSource, "public class " + IntendedClassName + "\n{", 1);
            }
            else
            {
                normalizedSource = String.Format("public class {0}\r\n{{\r\n{1}\r\n\r\n}}", IntendedClassName, normalizedSource);
            }

            // instance-ize functions
            var methodnameRegex = new Regex("(public|private) static (?!class)([^{]+)+{");
            if (methodnameRegex.IsMatch(normalizedSource))
            {
                normalizedSource = methodnameRegex.Replace(normalizedSource, "$1 $2\n{");
            }

            // normalize namespace
            var namespaceRegex = new Regex("namespace [^\\n{ ]+[^{]+{");
            if (namespaceRegex.IsMatch(normalizedSource))
            {
                normalizedSource = namespaceRegex.Replace(normalizedSource, "namespace " + IntendedNamespace + "\n{");
            }
            else
            {
                normalizedSource = String.Format("{0}\r\n\nnamespace {1}\r\n{{\r\n{2}\r\n}}", USING_STATEMENTS, IntendedNamespace, normalizedSource);
            }

            // add using statements if not present
            if (!normalizedSource.Contains("using System"))
            {
                normalizedSource = USING_STATEMENTS + "\r\n" + normalizedSource;
            }

            return normalizedSource;
        }

        public EvaluationResult Evaluate(string code)
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.GenerateInMemory = true;

            var results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                var errorList = results.Errors.OfType<CompilerError>()
                                              .Select(ce => String.Format("Line {0}: {1}", ce.Line, ce.ErrorText));
                return EvaluationResult.DoesNotBuild(errorList, Tests);
            }

            var assembly = results.CompiledAssembly;
            var type = assembly.GetType(IntendedNamespace + "." + IntendedClassName);
            var method = type.GetMethods()[0];

            var result = new EvaluationResult();

            foreach (var test in Tests)
            {
                var testObject = type.GetConstructor(new Type[] { }).Invoke(null);
                try
                {
                    test.PerformSetup();
                    // expecting static function, so instance arg is null - null ref means we got it wrong
                    var output = method.Invoke(testObject, test.InputParameters);
                    var evalResult = test.EvaluateResult(output);
                    if (evalResult.IsPass)
                    {
                        result.Tests.Add(new EvaluationItem()
                        {
                            TestName = test.Name,
                            IsPass = true,
                            IsError = false,
                            ExpectedResult = test.DescriptionOfExpectation,
                            ActualResult = evalResult.Output
                        });
                    }
                    else
                    {
                        result.Tests.Add(new EvaluationItem()
                        {
                            TestName = test.Name,
                            IsPass = false,
                            IsError = false,
                            ExpectedResult = test.DescriptionOfExpectation,
                            ActualResult = evalResult.Output
                        });
                    }

                }
                catch (TargetInvocationException exc)
                {
                    result.Tests.Add(new EvaluationItem()
                    {
                        TestName = test.Name,
                        IsPass = false,
                        IsError = true,
                        ExpectedResult = test.DescriptionOfExpectation,
                        ActualResult = exc.InnerException.ToString()
                    });
                }
                catch (Exception exc)
                {
                    // don't expect this to happen
                    result.Tests.Add(new EvaluationItem()
                    {
                        TestName = test.Name,
                        IsPass = false,
                        IsError = true,
                        ExpectedResult = test.DescriptionOfExpectation,
                        ActualResult = exc.ToString()
                    });
                }
                finally
                {
                    testObject = null;
                    GC.Collect();
                }
            }

            result.Summary = String.Format("{0}/{1} tests passed.",
                                            result.Tests.Where(t => t.IsPass).Count(),
                                            result.Tests.Count);
            result.Score = Evaluator.TallyScore(result.Tests);

            return result;
        }

        public static double TallyScore(List<EvaluationItem> results)
        {
            if (results.Any(r => r.IsError))
                return 0;
            else
                return 1;
        }
    }
}
