using IPA.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TestLogger : Logger
    {
        private TestContext context;

        public TestLogger(TestContext ctx)
        {
            context = ctx;
        }

        public override void Log(Level level, string message)
        {
            context.WriteLine($"LOG -> [{level}] {message}");
        }
    }
}
