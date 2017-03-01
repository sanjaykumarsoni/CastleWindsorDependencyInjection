using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace CommonLayer
{
    public class DebugDefaultLogContext : DefaultLogContext
    {
        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            String logLine = String.Format("{0}: {1:G}: {2},{3}: {4}.", logPolicy, DateTime.Now, eventId, category,
                                           message);
            Debug.WriteLine(logLine);
        }
    }

    public class TraceDefaultLogContext : DefaultLogContext
    {
        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            String logLine = String.Format("{0}: {1:G}: {2},{3}: {4}.", logPolicy, DateTime.Now, eventId, category,
                                           message);
            Trace.WriteLine(logLine);
        }
    }

    public class EventViewerDefaultLogContext : DefaultLogContext
    {
        private const String MachineName = ".";
        private readonly EventLog _eventLog;
        private readonly String _eventLogName;
        private readonly String _eventLogSource;

        public EventViewerDefaultLogContext(String eventLogName, String eventLogSource)
        {
            _eventLogName = eventLogName;
            _eventLogSource = eventLogSource;
            if (!EventLog.SourceExists(_eventLogName, MachineName))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(_eventLogSource, _eventLogName));
            }
            _eventLog = new EventLog { Source = _eventLogSource };
        }

        public override void Dispose()
        {
            if (_eventLog != null)
            {
                _eventLog.Close();
            }
            base.Dispose();
        }


        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            switch (logPolicy)
            {
                case LogPolicy.Critical:
                case LogPolicy.Error:
                    _eventLog.WriteEntry(message, EventLogEntryType.Error, eventId, category);
                    break;
                case LogPolicy.Warning:
                    _eventLog.WriteEntry(message, EventLogEntryType.Warning, eventId, category);
                    break;
                case LogPolicy.Information:
                    _eventLog.WriteEntry(message, EventLogEntryType.Information, eventId, category);
                    break;
                default:
                    _eventLog.WriteEntry(message, EventLogEntryType.Error, eventId, category);
                    break;
            }
        }
    }

    public class FileDefaultLogContext : DefaultLogContext
    {
        private readonly String _filePath;
        private readonly StreamWriter _streamWriter;

        public FileDefaultLogContext(String filePath)
        {
            _filePath = filePath;
            _streamWriter = File.Exists(_filePath) ? File.AppendText(_filePath) : File.CreateText(_filePath);
            _streamWriter.AutoFlush = true;
        }

        public override void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Close();
            }
            base.Dispose();
        }

        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            String logLine = String.Format("{0}: {1:G}: {2},{3}: {4}.", logPolicy, DateTime.Now, eventId, category,
                                           message);
            _streamWriter.WriteLine(logLine);
            //_streamWriter.WriteLine(_streamWriter.NewLine);
        }
    }

    public class EmailDefaultLogContext : DefaultLogContext
    {
        private readonly String _fromAddress;
        private readonly String _subject;
        private readonly String _toAddress;
        private String _bodyTemplate;

        public EmailDefaultLogContext(String fromAddress, String toAddress,
                           String subject, String bodyTemplate)
        {
            _fromAddress = fromAddress;
            _toAddress = toAddress;
            _subject = subject;
            _bodyTemplate = bodyTemplate;
        }

        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            _bodyTemplate = String.Format(_bodyTemplate, message);
            //Emailer.Emailer.SendMail(_fromAddress, _toAddress, _subject, _bodyTemplate);
        }
    }

    public class DatabaseDefaultLogContext : DefaultLogContext
    {
        private readonly DbCommand _command;
        private readonly DbConnection _connection;
        private readonly DbProviderFactory _factory;

        public DatabaseDefaultLogContext(String logDbConnectionStringName)
        {
            String logDbConnectionString =
                ConfigurationManager.ConnectionStrings[logDbConnectionStringName].ConnectionString;
            String dbProviderType = ConfigurationManager.ConnectionStrings[logDbConnectionStringName].ProviderName;
            if (String.IsNullOrWhiteSpace(dbProviderType))
            {
                dbProviderType = "SYSTEM.DATA.SQLCLIENT";
            }

            switch (dbProviderType.ToUpperInvariant())
            {
                case "SYSTEM.DATA.SQLCLIENT":
                    _factory = SqlClientFactory.Instance;
                    break;
                case "SYSTEM.DATA.ORACLECLIENT":
                    //factory = OracleClientFactory.Instance;
                    break;
                case "SYSTEM.DATA.OLEDB":
                  //  _factory = OleDbFactory.Instance;
                    break;
                case "SYSTEM.DATA.ODBC":
                  //  _factory = OdbcFactory.Instance;
                    break;
                case "NPGSQL":
                    //factory = NpgsqlFactory.Instance;
                    break;
                default:
                    _factory = SqlClientFactory.Instance;
                    break;
            }
            _connection = _factory.CreateConnection();
            _command = _factory.CreateCommand();
            if (_connection != null)
            {
                _connection.ConnectionString = logDbConnectionString;
                if (_command != null) _command.Connection = _connection;
            }
        }

        public override void Log(String message, LogPolicy logPolicy, Int32 eventId, short category)
        {
            String logLine = String.Format("{0}: {1:G}: {2},{3}: {4}.", logPolicy, DateTime.Now, eventId, category,
                                           message);
            const String sqlQuery =
                "Insert Into Logger (Message, LogPolicy, EventId, Category, AddedDate) Values (@Message, @LogPolicy, @EventId, @Category, @AddedDate)";
            _command.CommandText = sqlQuery;
            _command.CommandType = CommandType.Text;
            AddParameter("@Message", message);
            AddParameter("@LogPolicy", logPolicy);
            AddParameter("@EventId", eventId);
            AddParameter("@Category", category);
            AddParameter("@AddedDate", DateTime.Now);
            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
                _command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                var debugLogger = new DebugDefaultLogContext();
                debugLogger.Log(exception.Message);
                var traceLogger = new TraceDefaultLogContext();
                traceLogger.Log(exception.Message);
            }
            finally
            {
                _command.Parameters.Clear();
                //_connection.Close();
            }

            Debug.WriteLine(logLine);
        }

        private void AddParameter(String name, object value)
        {
            DbParameter p = _factory.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            _command.Parameters.Add(p);
        }
    }
}
    