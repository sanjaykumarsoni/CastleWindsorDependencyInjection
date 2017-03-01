using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;

namespace CommonLayer
{
    [Flags]
    [DataContract]
    public enum LogPolicy
    {
        /// <summary>
        /// Will not be logged to any Log Sources
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// Fatal error or application crash
        /// </summary>
        [EnumMember]
        Critical = 2,

        /// <summary>
        /// Will be logged to the Log Sources and Exception will be shown as Error message to the users.
        /// </summary>
        /// <remarks>If ShowUnHandledErrors is set to true, Actual exception information is shown to user. Else, the Generic Exception message like "Please contact Administrator is shown to the user"</remarks>
        [EnumMember]
        Error = 4,

        /// <summary>
        /// Will be logged to the Log Sources and Exception will be shown as Alert message to the users.
        /// </summary>
        [EnumMember]
        Warning = 8,

        /// <summary>
        /// Will be logged to the Log Sources but Exception will not be shown to the users
        /// </summary>
        [EnumMember]
        Information = 16,

        /// <summary>
        /// All log policy Information will be logged to the loggers.
        /// </summary>
        [EnumMember]
        All = 32,
    }

    [Flags]
    [DataContract]
    public enum LogSource
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        EventViewer = 1,
        [EnumMember]
        FileSystem = 2,
        [EnumMember]
        Database = 4,
        [EnumMember]
        Email = 16,
        [EnumMember]
        Debuger = 32,
        [EnumMember]
        TraceListner = 64
    }

    public interface ILogContext : IDisposable
    {
        void Log(Exception exception);
        void Log(String message);
        void Log(String message, LogPolicy logPolicy);
        void Log(String message, LogPolicy logPolicy, Int32 eventId, short category);
    }

    public class DefaultLogContext : ILogContext
    {
        private static DefaultLogContext _logger;
        private readonly IList<LogPolicy> _logPolicies = new List<LogPolicy>();
        private readonly IList<ILogContext> _loggers = new List<ILogContext>();
        public Boolean IsAsync { get; set; }

        public static DefaultLogContext LogContext
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new DefaultLogContext();
                    _logger.Initialize();
                  
                }
                return _logger;
            }
        }

        #region ILogContext Members

        public virtual void Dispose()
        {
            foreach (ILogContext logger in _loggers)
            {
                logger.Dispose();
            }
            _logPolicies.Clear();
            _loggers.Clear();
        }

        public virtual void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            if (_logPolicies.Contains(logPolicy))
            {
                LogToLoggers(message, logPolicy, eventId, category);
            }
        }

        public virtual void Log(String message)
        {
            Log(message, LogPolicy.All);
        }

        //[ServiceContract]
        public interface ICustomException
        {
            //[DataMember] 
            String Code { get; set; }
            //[DataMember]
            IList<String> BusinessErrors { get; set; }
            //[DataMember]
            LogPolicy Policy { get; set; }
        }
        public virtual void Log(Exception exception)
        {
            var customExcption = exception as  ICustomException;
            var logPolicy = LogPolicy.Error;
            if (customExcption != null){logPolicy = customExcption.Policy;}
            if (_logPolicies.Contains(logPolicy))
            {
                LogToLoggers(exception.Message, logPolicy, 1, 1);
                //For Health Monitoring
                //var exceptionEvent = new ExceptionEvent(exception);
                //exceptionEvent.Raise();
            }
        }

        public virtual void Log(String message, LogPolicy logPolicy)
        {
                LogToLoggers(message, logPolicy, 1, 1);
        }

        #endregion

        internal void Initialize()
        {
            IsAsync = false;
            String logAsync = ConfigurationManager.AppSettings["LogAsync"];
            IsAsync = String.IsNullOrEmpty(logAsync) != true &&
                      Convert.ToBoolean(logAsync, CultureInfo.InvariantCulture);
            var logSources = (LogSource) Convert.ToInt32(ConfigurationManager.AppSettings["LogSource"]);
            if ((logSources & LogSource.EventViewer) == LogSource.EventViewer)
            {
                _loggers.Add(new EventViewerDefaultLogContext(ConfigurationManager.AppSettings["EventLogSource"],
                                                   ConfigurationManager.AppSettings["EventViewerName"]));
            }

            if ((logSources & LogSource.FileSystem) == LogSource.FileSystem)
            {
                _loggers.Add(new FileDefaultLogContext(Path.Combine(ConfigurationManager.AppSettings["LogDirectory"], "Log.txt")));
            }

            if ((logSources & LogSource.Email) == LogSource.Email)
            {
                _loggers.Add(new EmailDefaultLogContext(ConfigurationManager.AppSettings["LogEmailFromAddress"],
                                             ConfigurationManager.AppSettings["LogEmailToAddress"],
                                             ConfigurationManager.AppSettings["LogEmailSubject"],
                                             ConfigurationManager.AppSettings["LogEmailBody"]));
            }

            if ((logSources & LogSource.Debuger) == LogSource.Debuger)
            {
                _loggers.Add(new DebugDefaultLogContext());
            }

            if ((logSources & LogSource.TraceListner) == LogSource.TraceListner)
            {
                _loggers.Add(new TraceDefaultLogContext());
            }

            if ((logSources & LogSource.Database) == LogSource.Database)
            {
                _loggers.Add(new DatabaseDefaultLogContext(ConfigurationManager.AppSettings["LogDbConnectionStringName"]));
            }

            var logPolicies = (LogPolicy) Convert.ToInt32(ConfigurationManager.AppSettings["LogPolicy"]);
            if ((logPolicies & LogPolicy.Critical) == LogPolicy.Critical)
            {
                _logPolicies.Add(LogPolicy.Critical);
            }
            if ((logPolicies & LogPolicy.Error) == LogPolicy.Error)
            {
                _logPolicies.Add(LogPolicy.Error);
            }
            if ((logPolicies & LogPolicy.Information) == LogPolicy.Information)
            {
                _logPolicies.Add(LogPolicy.Information);
            }

            if ((logPolicies & LogPolicy.Warning) == LogPolicy.Warning)
            {
                _logPolicies.Add(LogPolicy.Warning);
            }
        }

        protected virtual void LogToLoggers(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            foreach (ILogContext logger in _loggers)
            {
                //if log implements ILogAsync then invoke it on a separate thread
                if (IsAsync)
                {
                    AsyncMethodCaller caller = logger.Log;
                    caller.BeginInvoke(message, logPolicy, eventId, category, WriteCallbackMethod, null);
                }
                else
                {
                    logger.Log(message, logPolicy, eventId, category);
                }
            }
        }

        private static void WriteCallbackMethod(IAsyncResult ar)
        {
            // Retrieve the delegate.
            var result = (AsyncResult) ar;
            var caller = (AsyncMethodCaller) result.AsyncDelegate;

            // Call EndInvoke to retrieve the results.
            caller.EndInvoke(ar);
        }

        #region Nested type: AsyncMethodCaller

        private delegate void AsyncMethodCaller(String message, LogPolicy logPolicy, Int32 eventId, short category);

        #endregion
    }
}
