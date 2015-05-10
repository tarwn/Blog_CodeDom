using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvaluateFizzBuzz.Evaluation
{
    public class TestDefinition
    {
        public string Name { get; private set; }
        public object[] InputParameters { get; private set; }
        public Func<object, LocalEvaluationResult> EvaluateResult { get; private set; }
        public string DescriptionOfExpectation { get; private set; }
        public Action Setup { get; private set; }

        public TestDefinition(string name, object[] inputParameters, object expectedOutput)
        {
            Name = name;
            InputParameters = inputParameters;
            DescriptionOfExpectation = String.Format(expectedOutput.ToString());
            EvaluateResult = (o) => new LocalEvaluationResult(expectedOutput.Equals(o), o != null ? o.ToString() : "");
        }

        public TestDefinition(string name, object[] inputParameters, Func<object, LocalEvaluationResult> evaluateResult, string descriptionOfExpectation, Action setup = null)
        {
            Name = name;
            InputParameters = inputParameters;
            DescriptionOfExpectation = descriptionOfExpectation;
            EvaluateResult = evaluateResult;
            Setup = setup;
        }

        public void PerformSetup()
        {
            if (Setup != null)
                Setup();
        }
    }
}
