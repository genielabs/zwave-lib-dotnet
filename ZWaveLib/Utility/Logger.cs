using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveLib
{
    public class Logger
    {
        public delegate void LogMessageEventHandler(object msg, params object[] parameters);

        public LogMessageEventHandler WarnMessageReceived;
        public LogMessageEventHandler TraceMessageReceived;
        public LogMessageEventHandler DebugMessageReceived;
        public LogMessageEventHandler ErrorMessageReceived;
        public LogMessageEventHandler InfoMessageReceived;

        public void Warn(object msg, params object[] parameters)
        {
            WarnMessageReceived?.Invoke(msg, parameters);
        }

        public void Trace(object msg, params object[] parameters)
        {
            TraceMessageReceived?.Invoke(msg, parameters);
        }

        public void Debug(object msg, params object[] parameters)
        {
            DebugMessageReceived?.Invoke(msg, parameters);
        }
        
        public void Error(object msg, params object[] parameters)
        {
            ErrorMessageReceived?.Invoke(msg, parameters);
        }

        public void Info(object msg, params object[] parameters)
        {
            InfoMessageReceived?.Invoke(msg, parameters);
        }
    }
}
