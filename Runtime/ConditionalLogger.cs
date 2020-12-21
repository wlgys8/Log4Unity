using System.Diagnostics;
namespace MS.Log4Unity{


    public class ConditionalLogger
    {

        private ULogger _logger;

        public ConditionalLogger(ULogger logger){
            _logger = logger;
        }

        public ULogger innerLogger{
            get{
                return _logger;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public void EditorDebug(object message){
            innerLogger.Debug(message);
        }

        [Conditional("UNITY_EDITOR")]
        public void EditorInfo(object message){
            innerLogger.Info(message);
        }

        [Conditional("UNITY_EDITOR")]
        public void EditorWarn(object message){
            innerLogger.Warn(message);
        }

        [Conditional("UNITY_EDITOR")]
        public void EditorError(object message){
            innerLogger.Error(message);
        }

        [Conditional("UNITY_EDITOR")]
        public void EditorFatal(object message){
            innerLogger.Fatal(message);
        }

        [Conditional("LOG4UNITY_DEBUG")]
        public void Debug(object message)
        {
            innerLogger.Debug(message);
        }

        [Conditional("LOG4UNITY_INFO")]
        public void Info(object message)
        {
            innerLogger.Info(message);
        }

        [Conditional("LOG4UNITY_WARN")]
        public void Warn(object message)
        {
            innerLogger.Warn(message);
        }
        public void Error(object message)
        {
            innerLogger.Error(message);
        }
        public void Fatal(object message)
        {
            innerLogger.Fatal(message);
        }

        public static bool isDebugDefined{
            get{
                #if LOG4UNITY_DEBUG
                return true;
                #else
                return false;
                #endif
            }
        }

        public static bool isInfoDefined{
            get{
                #if LOG4UNITY_INFO
                return true;
                #else
                return false;
                #endif
            }
        }

        public static bool isWarnDefined{
            get{
                #if LOG4UNITY_WARN
                return true;
                #else
                return false;
                #endif
            }
        }


    }
}
