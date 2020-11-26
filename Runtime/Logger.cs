

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

    public interface ULogger{
 
        void Debug(object message);

        void Info(object message);

        void Warn(object message);

        void Error(object message);

        void Fatal(object message);  

        void ClearAppenders();

        string catagory{
            get;
        }

        bool IsOn(LogType type);

        LogLevel level{
            get;set;
        }

        void Append(IAppender appender);
    }
 
    public class ULoggerDefault:ULogger
    {
        private List<IAppender> _appenders = new List<IAppender>();

        private LogLevel _logLevel = LogLevel.All;

        private string _catagory;

        private bool _isConfigurated = false;

        public ULoggerDefault(string catagory){
            _catagory = catagory;
        }

        private void ConfigurateIfNot(){
            if(_isConfigurated){
                return;
            }
            _isConfigurated = true;
             Configurator.ConfigurateLogger(this,this.catagory);
        }

        public void Append(IAppender appender)
        {
            ConfigurateIfNot();
            _appenders.Add(appender);
        }

        public string catagory{
            get{
                return _catagory;
            }
        }

        public LogLevel level{
            set{
                ConfigurateIfNot();
                _logLevel = value;
            }get{
                 ConfigurateIfNot();
                return _logLevel;
            }
        }

        public bool IsOn(LogType type){
            ConfigurateIfNot();
            return Configurator.CheckLogOn(type,_logLevel);
        }

        private void HandleMessage(LogType type,object message){
            foreach(var ap in _appenders){
                var logEvent = new LogEvent(){
                    logger = this,
                    logType = type,
                    message = message,
                };
                ap.HandleLogEvent(ref logEvent);
            }
        }

        public void Debug(object message)
        {
            if(!IsOn(LogType.Debug)){
                return;
            }
            HandleMessage(LogType.Debug,message);
        }

        public void Info(object message)
        {
            if(!IsOn(LogType.Info)){
                return;
            }
            HandleMessage(LogType.Info,message);
        }

        public void Warn(object message)
        {
            if(!IsOn(LogType.Warn)){
                return;
            }
            HandleMessage(LogType.Warn,message);
        }

        public void Error(object message)
        {
            if(!IsOn(LogType.Error)){
                return;
            }
            HandleMessage(LogType.Error,message);
        }

        public void Fatal(object message)
        {
            if(!IsOn(LogType.Fatal)){
                return;
            }
            HandleMessage(LogType.Fatal,message);
        }

        public void ClearAppenders()
        {
            ConfigurateIfNot();
            _appenders.Clear();
        }
 
    }



}

