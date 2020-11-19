using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Log4Unity{
    public class UnityLogAppender : LayoutAppender
    {

        public override bool HandleLogEvent(ref LogEvent logEvent){
            if(!base.HandleLogEvent(ref logEvent)){
                return false;
            }
            var type = logEvent.logType;
            var message = logEvent.message;
            switch(type){
                case LogType.Debug:
                case LogType.Info:
                Debug.Log(message);
                break;
                case LogType.Warn:
                Debug.LogWarning(message);
                break;
                case LogType.Error:
                Debug.LogError(message);
                break;
                case LogType.Fatal:
                Debug.LogError(message);
                break;
            }
            return true;
        }
    }
}
