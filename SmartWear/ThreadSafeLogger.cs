using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace Litfal
{
    public class ThreadSafeLogger
    {
        UnityModManager.ModEntry.ModLogger _baseLogger;
        private object _writeLock = new object();

        public ThreadSafeLogger(UnityModManager.ModEntry.ModLogger baseLogger)
        {
            _baseLogger = baseLogger;
        }


        public virtual void Critical(string str)
        {
            lock (_writeLock) _baseLogger.Critical(str);
        }
        public virtual void Error(string str)
        {
            lock (_writeLock) _baseLogger.Error(str);
        }
        public virtual void Log(string str)
        {
            lock (_writeLock) _baseLogger.Log(str);
        }
        public virtual void Warning(string str)
        {
            lock (_writeLock) _baseLogger.Warning(str);
        }

        public virtual void Debug(string str)
        {
#if(DEBUG)
            lock (_writeLock) _baseLogger.Log(str);
#endif
        }
    }
}
