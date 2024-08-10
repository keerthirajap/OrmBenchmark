using System;
using System.Collections.Generic;
using System.Text;

namespace OrmBenchmark.Core
{
    public class BenchmarkResult
    {
        public string TestName { get; set; }

        public string ORMName { get; set; }

        public long FirstItemExecTime { get; set; }

        public long ExecTime { get; set; }
    }
}