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

        [Conditional("DEBUG")]
        public void Debug(object message)
        {
            innerLogger.Debug(message);
        }

        public void Info(object message)
        {
            innerLogger.Info(message);
        }

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

    }
}
