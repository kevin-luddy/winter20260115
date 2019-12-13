using System;
using System.Configuration;
using System.Transactions;
using Elmah;
using Microsoft.Practices.EnterpriseLibrary.Logging;


namespace IES.Common
{
    /// <summary>
    /// Represents the type of log entries made in the system.
    /// </summary>
    /// <remarks>
    /// All logging levels >= the value set will be logged
    /// </remarks>
    public enum LoggingLevels : int
    {
        /// <summary>
        /// Indicates a debug log type.
        /// </summary>
        DEBUG = 1,
        /// <summary>
        /// Indicates a info log type.
        /// </summary>
        INFO = 2,
        /// <summary>
        /// Indicates a warning log type.
        /// </summary>
        WARN = 3,
        /// <summary>
        /// Indicates an error log type.
        /// </summary>
        ERROR = 4,
        /// <summary>
        /// Indicates that logging is disabled.
        /// </summary>
        NONE = 5
        
    }

    /// <summary>
    /// Wrap the logging API for ease of use
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Log type to be used when determining whether or not to log
        /// </summary>
        private static LoggingLevels _logType = LoggingLevels.NONE;

        /// <summary>
        /// The category to use, default is to use the type name
        /// of the caller.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogType")]
        static Logger()
        {
            if (!Enum.TryParse<LoggingLevels>(ConfigurationManager.AppSettings["LogType"], true, out Logger._logType))
            {
                _logType = LoggingLevels.ERROR;

                Error elmahError = new Error();
                elmahError.Type = typeof(Logger).ToString();
                elmahError.User = "IIS";
                elmahError.Message = "LogType was not correctly specified in web.config. Logging in Error mode.";
                elmahError.Time = DateTime.Now;
                elmahError.HostName = Environment.MachineName;
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    Elmah.ErrorLog.GetDefault(null).Log(elmahError);
                }
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Logger"/> class from being created.
        /// </summary>
        private Logger()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inIdentifier">The identifier to use to identify the module of code that is logging</param>
        public Logger(string inIdentifier) : this()
        {
            Identifier = inIdentifier;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inType">The type of the code which will be used as an identifier for the module of code that is logging</param>
        public Logger(Type inType) : this((inType!=null)?inType.FullName:string.Empty)
        {
        }

        /// <summary>
        /// Write an entry to our performance log which is a .csv file to be easily examined by excel
        /// </summary>
        /// <param name="inMessage">the message to log</param>
        /// <param name="inTimeTaken">milliseconds taken for method</param>
        /// <param name="traceStatement">If true, set severity to verbose.</param>
        public void Performance(string inMessage, long inTimeTaken, bool traceStatement = false)
        {
            Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry entry = new LogEntry();
            entry.Categories.Clear();
            entry.Categories.Add("Performance");
            entry.Message = ActiveUser + "," + inMessage + "," + inTimeTaken.ToString();

            // Set the severity to match the priority
            if (!traceStatement)
            {
                entry.Severity = System.Diagnostics.TraceEventType.Information;
            }
            else
            {
                entry.Severity = System.Diagnostics.TraceEventType.Verbose;
            }

            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(entry);
        }

        #region Debug

        /// <summary>
        /// Gets a value indicating whether debug is enabled.
        /// </summary>
        public bool DebugEnabled
        {
            get
            {
                return _logType == LoggingLevels.DEBUG;
            }
        }

        /// <summary>
        /// Logs an exception as the debug log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public void Debug(Exception exception)
        {
            Debug(exception, null);
        }

        /// <summary>
        /// Logs a message formatted with additional args as the debug log type
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public void Debug(string inMessage)
        {
            Debug(null, inMessage);
        }

        /// <summary>
        /// Logs an exception and message formatted with additional args as the debug log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="inMessage">Message to log</param>
        public void Debug(Exception exception, string inMessage)
        {
            WriteEntry(inMessage, exception, LoggingLevels.DEBUG);
        }

        #endregion

        #region Info

        /// <summary>
        /// Logs an exception as the info log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public void Info(Exception exception)
        {
            Info(exception, null);
        }

        /// <summary>
        /// Logs a message formatted with additional args as the info log type
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public void Info(string inMessage)
        {
            Info(null, inMessage);
        }

        /// <summary>
        /// Logs an exception and message formatted with additional args as the info log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="inMessage">Message to log</param>
        public void Info(Exception exception, string inMessage)
        {
            WriteEntry(inMessage, exception, LoggingLevels.INFO);
        }

        #endregion

        #region Warn

        /// <summary>
        /// Logs an exception as the warn log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public void Warn(Exception exception)
        {
            Warn(exception, null);
        }

        /// <summary>
        /// Logs a message formatted with additional args as the warn log type
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public void Warn(string inMessage)
        {
            Warn(null, inMessage);
        }

        /// <summary>
        /// Logs an exception and message formatted with additional args as the warn log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="inMessage">Message to log</param>
        public void Warn(Exception exception, string inMessage)
        {
            WriteEntry(inMessage, exception, LoggingLevels.WARN);
        }

        #endregion

        #region Error

        /// <summary>
        /// Logs an exception as the error log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public void Error(Exception exception)
        {
            Error(exception, null);
        }

        /// <summary>
        /// Logs a message formatted with additional args as the error log type
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public void Error(string inMessage)
        {
            Error(null, inMessage);
        }

        /// <summary>
        /// Logs an exception and message formatted with additional args as the error log type
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="inMessage">Message to log</param>
        public void Error(Exception exception, string inMessage)
        {
            WriteEntry(inMessage, exception, LoggingLevels.ERROR);
        }

        #endregion
        
        /// <summary>
        /// Create the logging entry to be written to the logging block
        /// </summary>
        /// <param name="inMessage">the message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="inLevel">the enum level to log</param>
        private void WriteEntry(string inMessage, Exception exception, LoggingLevels inLevel)
        {
            //only write the entry if its at the configured level
            if (inLevel >= _logType)
            {
                Error elmahError;
                if (exception != null)
                {
                    elmahError = new Error(exception);

                    if (string.IsNullOrWhiteSpace(inMessage))
                    {
                        // re-use the exception message if no message is pushed in.
                        inMessage = elmahError.Message;
                    }

                    // let elmah figure out the user and type. We'll tack on our version to the message
                    inMessage = "User: " + ActiveUser + " Type: " + Identifier + " Message: " + inMessage;
                }
                else
                {
                    elmahError = new Error();
                    // set the type and user explicitly
                    elmahError.Type = Identifier;
                    elmahError.User = ActiveUser;
                }

                if (!string.IsNullOrWhiteSpace(inMessage))
                {
                    elmahError.Message = inMessage;
                }

                elmahError.Time = DateTime.Now;
                elmahError.HostName = Environment.MachineName;
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    Elmah.ErrorLog.GetDefault(null).Log(elmahError);
                }
            }
        }

        /// <summary>
        /// Gets the active user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private string ActiveUser
        {
            get
            {
                string activeUserWithDomain;

                try
                {
                    // NOTE This method is not ideal .. it's a duplicate of what's in
                    //      SecurityInformation.  However, I did not want to 
                    //      use a ServiceLocator to load the SecurityInformation class
                    //      and instead opted to duplicate this code in this location.

                    // split the ntid and just pass in the ntid
                    activeUserWithDomain = System.Threading.Thread.CurrentPrincipal.Identity.Name;

                    if (string.IsNullOrEmpty(activeUserWithDomain))
                    {
                        activeUserWithDomain = "ANONYMOUS"; // no username
                    }
                }
                catch  // for background-thread processing
                {
                    activeUserWithDomain = "APPUSER";
                }

                return activeUserWithDomain;
            }
        }
    }
}
