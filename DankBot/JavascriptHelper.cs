using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint;

namespace DankBot
{
    class JavascriptHelper
    {
        private Engine engine;
        private string OutputBuffer;


        public JavascriptHelper()
        {
            engine = new Engine();
            engine.SetValue("log", new Action<object>(Log));

            OutputBuffer = "";

        }

        private void Log(object obj)
        {
            OutputBuffer += obj.ToString() + "\n";
        }

        public void RunSource(string source)
        {
            OutputBuffer = "";
            engine = new Engine();
            engine.SetValue("log", new Action<object>(Log));
            engine.Execute(source);
        }

        public string TryExecuteJs(string source)
        {
            var cc = new CancellationToken();
            var task = Task.Run(() => RunSource(source),cc);
            if (task.Wait(TimeSpan.FromSeconds(5)))
            {
                return OutputBuffer;
            }
            throw new Exception("Timeout ...");
        }
    }
}
