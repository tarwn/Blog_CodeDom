using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvaluateFizzBuzz.Evaluation
{
    public class EvaluationResult
    {
        public string Summary { get; set; }

        public List<EvaluationItem> Tests { get; set; }

        public bool CouldNotExecute { get; set; }

        public double Score { get; set; }

        public EvaluationResult()
        {
            Tests = new List<EvaluationItem>();
        }

        public static EvaluationResult DoesNotBuild(IEnumerable<string> errors, IEnumerable<TestDefinition> tests)
        {
            return new EvaluationResult()
            {
                CouldNotExecute = true,
                Summary = String.Format("Does not Build, {0} Errors: {1}", errors.Count(), String.Join(", ", errors)),
                Score = 0,
                Tests = tests.Select(t => new EvaluationItem()
                {
                    TestName = t.Name,
                    ExpectedResult = t.DescriptionOfExpectation,
                    ActualResult = "Does not build",
                    IsPass = false,
                    IsError = true
                }).ToList()
            };
        }

    }
}
