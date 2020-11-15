using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
namespace MS.Log4Unity{


    public enum LogType{
        Fatal = 1,
        Error = 2,
        Warn = 4,
        Info = 8,
        Debug = 16,
    }

    [System.Flags]
    public enum LogLevel{
        Off = 0,
        Fatal = 1,
        Error = 2,
        Warn = 4,
        Info = 8,
        Debug = 16,
        All = 31,
    }

    public interface ILogger{

        void Append(IAppender appender);

        void ClearAppenders();

        void Debug(string message);

        void Info(string message);

        void Warn(string message);

        void Error(string message);

        void Fatal(string message);

        LogLevel level{
            get;set;
        }

        string catagory{
            get;
        }
    }

    public interface IAppLogger:ILogger{

    }

    public class DefaultLogger : IAppLogger
    {
        private List<IAppender> _appenders = new List<IAppender>();

        private LogLevel _logLevel = LogLevel.All;

        private string _catagory;

        public DefaultLogger(string catagory){
            _catagory = catagory;
            Configurator.ConfigurateLogger(this,this.catagory);
        }

        public void Append(IAppender appender)
        {
            _appenders.Add(appender);
        }

        public string catagory{
            get{
                return _catagory;
            }
        }

        public LogLevel level{
            set{
                _logLevel = value;
            }get{
                return _logLevel;
            }
        }

        public bool IsOn(LogType type){
            var lv = (LogLevel)type;
            return lv <= _logLevel;
        }

        private void HandleMessage(LogType type,string message){
            foreach(var ap in _appenders){
                var logEvent = new LogEvent(){
                    logger = this,
                    logType = type,
                    message = message,
                };
                ap.HandleLogEvent(ref logEvent);
            }
        }

        public void Debug(string message)
        {
            if(!IsOn(LogType.Debug)){
                return;
            }
            HandleMessage(LogType.Debug,message);
        }
        public void Info(string message)
        {
            if(!IsOn(LogType.Info)){
                return;
            }
            HandleMessage(LogType.Info,message);
        }

        public void Warn(string message)
        {
            if(!IsOn(LogType.Warn)){
                return;
            }
            HandleMessage(LogType.Warn,message);
        }

        public void Error(string message)
        {
            if(!IsOn(LogType.Error)){
                return;
            }
            HandleMessage(LogType.Error,message);
        }

        public void Fatal(string message)
        {
            if(!IsOn(LogType.Fatal)){
                return;
            }
            HandleMessage(LogType.Fatal,message);
        }

        public void ClearAppenders()
        {
            _appenders.Clear();
        }
 
    }



    public class DelayConfiguratedLogger : IAppLogger
    {

        private DefaultLogger _logger;

        public DelayConfiguratedLogger(string catagoryName){
            this.catagory = catagoryName;
        }
        public string catagory{
            get;private set;
        }

        private IAppLogger innerLogger{
            get{
                if(_logger != null){
                    return _logger;
                }
                _logger = new DefaultLogger(this.catagory);
                return _logger;
            }
        }

        public LogLevel level{
            set{
                innerLogger.level = level;
            }get{
                return innerLogger.level;
            }
        }

        public void Append(IAppender appender)
        {
            innerLogger.Append(appender);
        }

        public void Debug(string message)
        {
            innerLogger.Debug(message);
        }

        public void Error(string message)
        {
            innerLogger.Error(message);
        }

        public void Fatal(string message)
        {
            innerLogger.Fatal(message);
        }

        public void Info(string message)
        {
            innerLogger.Info(message);
        }

        public void Warn(string message)
        {
            innerLogger.Warn(message);
        }

        public void ClearAppenders(){
            innerLogger.ClearAppenders();
        }


    }




}

