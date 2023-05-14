using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class NetworkLogger
    {
        private static readonly NetworkLogger instance = new NetworkLogger();

        private Action<string> _printLog = null;
        private string _pendingLog = string.Empty;
        private object _latch = new object();
        private int _pendingTick = 0;
        private int _intervalTick = 0;
        private string _outputPath = null;
        private bool _isOpen = false;
        private bool _isWriteBinary = false;

        public static void Open(Action<string> printLog, bool isWriteBinary = true, int intervalTick = 100)
        {
            instance.Setup(printLog, isWriteBinary, intervalTick);
        }

        public static void Write(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            instance.PushLog(message, file, line);
        }

        public static void WriteDebug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
#if DEBUG
            instance.PushLog(message, file, line);
#endif
        }

        public static void WriteException(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            instance.PushLog(message, file, line);
            instance.ProcessLog(true);

            throw new ArgumentException(message);
        }

        public static void Flush()
        {
            instance.ProcessLog();
        }

        public static void Flush_Short()
        {
            instance.ProcessLog_Short();
        }

        public static void ConsoleLog(string message)
        {
            Console.Write(message);
        }

        private void Setup(Action<string> printLog, bool isWriteBinary, int intervalTick)
        {
            _isWriteBinary = isWriteBinary;

            if (true == _isWriteBinary)
            {
                var directory = Directory.GetCurrentDirectory() + @"\Logs";
                var date = DateTime.Now;

                if (false == Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                _outputPath = $"{directory}{@"\"}{date.Year}{date.ToString("MM")}{date.ToString("dd")}.log";
            }

            _printLog       = printLog;
            _intervalTick   = intervalTick;
            _pendingTick    = Environment.TickCount + intervalTick;
            _isOpen         = true;
        }

        private string Tag(string file, int line)
        {
            var date = DateTime.Now;
            return $"[{date.ToString("MM")}-{date.ToString("dd")} {date.ToString("hh")}:{date.ToString("mm")}:{date.ToString("ss")}][{Path.GetFileName(file)}:{line}]";
        }

        private void PushLog(string message, string file = "", int line = 0)
        {
            if (false == _isOpen)
                return;

            lock (_latch)
                _pendingLog += $"{Tag(file, line)} {message}" + Environment.NewLine;
        }

        private void ProcessLog(bool isForce = false)
        {
            var nowTick = Environment.TickCount;
            lock (_latch)
            {
                if (false == isForce)
                {
                    if (nowTick < _pendingTick)
                        return;
                    _pendingTick = nowTick + _intervalTick;
                }

                if (string.Empty == _pendingLog)
                    return;

                try
                {
                    if (true == _isWriteBinary)
                    {
                        using (var writer = File.AppendText(_outputPath))
                            writer.Write(_pendingLog);
                    }
                }
                catch (Exception exception)
                {
                    _printLog.Invoke(exception.ToString());
                    _printLog.Invoke("Log Binary File Is Pending");
                    return;
                }

                _printLog.Invoke(_pendingLog);
                _pendingLog = string.Empty;
            }
        }

        private void ProcessLog_Short(bool isForce = false)
        {
            var nowTick = Environment.TickCount;
            lock (_latch)
            {
                if (false == isForce)
                {
                    if (nowTick < _pendingTick)
                        return;
                    _pendingTick = nowTick + _intervalTick;
                }

                if (string.Empty == _pendingLog)
                    return;

                _printLog.Invoke(_pendingLog);
                _pendingLog = string.Empty;
            }
        }
    }
}
