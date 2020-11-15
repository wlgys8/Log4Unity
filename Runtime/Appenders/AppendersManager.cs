using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Log4Unity{

    internal class AppendersManager
    {
        private static Dictionary<string,IAppender> _appenders = new Dictionary<string,IAppender>();

        public static IAppender GetAppender(string appenderName){
            if(_appenders.ContainsKey(appenderName)){
                return _appenders[appenderName];
            }
            var appenderCfg = Configurator.GetAppender(appenderName);
            if(appenderCfg == null){
                Debug.LogWarning("unknown appender:" + appenderName);
                return null;
            }
            var tp = System.Type.GetType(appenderCfg.type);
            if(tp == null){
                Debug.LogWarning("unknown appender:" + appenderCfg.type);
                return null;
            }
            var appender = System.Activator.CreateInstance(tp) as IAppender;
            appender.OnInitialize(appenderCfg.GetConfigsReader());
            _appenders.Add(appenderName,appender);
            return appender;
        }
    }
}
