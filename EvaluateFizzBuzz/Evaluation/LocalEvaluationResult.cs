using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvaluateFizzBuzz.Evaluation
{
    public class LocalEvaluationResult
    {
        public LocalEvaluationResult(bool isPass, string output)
        {
            IsPass = isPass;
            Output = output;
        }

        public bool IsPass { get; set; }
        public string Output { get; set; }

    }
}
