using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Log4Unity{




    public class LogFactory
    {

        private static Dictionary<string,IAppLogger> _cachedLoggers = new Dictionary<string,IAppLogger>();

        static LogFactory(){
        }

        public static bool ConfigurateAtResources(string file){
            return Configurator.TryLoadFromResources(file);
        }

        public static bool Configurate(string configJSONText){
            return Configurator.TryLoadFromText(configJSONText);
        }

        public static ILogger GetLogger(){
            return GetLogger(Configurator.DEFAULT_CATAGORY);
        }

        public static ILogger GetLogger(string catagoryName){
            IAppLogger logger = null;
            if(_cachedLoggers.TryGetValue(catagoryName,out logger)){
                return logger;
            }
            logger = new DelayConfiguratedLogger(catagoryName);
            return logger;
        }

        public static ILogger GetLogger(System.Type type){
            return GetLogger(type.FullName);
        }

    }
}
