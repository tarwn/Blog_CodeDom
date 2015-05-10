using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvaluateFizzBuzz.Evaluation
{
    public class EvaluationItem
    {
        public string TestName { get; set; }
        public string ExpectedResult { get; set; }
        public string ActualResult { get; set; }
        public bool IsPass { get; set; }
        public bool IsError { get; set; }
    }
}
