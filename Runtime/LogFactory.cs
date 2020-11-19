using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Log4Unity{




    public static class LogFactory
    {

        private static Dictionary<string,ULogger> _cachedLoggers = new Dictionary<string,ULogger>();
        private static Dictionary<ULogger,ConditionalLogger> _cachedConditionalLogger = new Dictionary<ULogger, ConditionalLogger>();
        static LogFactory(){
        }

        public static bool ConfigurateAtResources(string file){
            return Configurator.TryLoadFromResources(file);
        }

        public static bool Configurate(string configJSONText){
            return Configurator.TryLoadFromText(configJSONText);
        }

        public static ULogger GetLogger(){
            return GetLogger(Configurator.DEFAULT_CATAGORY);
        }

        public static ULogger GetLogger(string catagoryName){
            ULogger logger = null;
            if(_cachedLoggers.TryGetValue(catagoryName,out logger)){
                return logger;
            }
            logger = new ULoggerDefault(catagoryName);
            return logger;
        }

        public static ULogger GetLogger(System.Type type){
            return GetLogger(type.FullName);
        }

        public static ULogger GetLogger<T>(){
            return GetLogger(typeof(T));
        }

        public static ConditionalLogger GetConditionalLogger(){
            return GetLogger().Conditional();
        }

        public static ConditionalLogger GetConditionalLogger(string catagory){
            return GetLogger(catagory).Conditional();
        }

        public static ConditionalLogger GetConditionalLogger(System.Type type){
            return GetLogger(type).Conditional();
        }

        public static ConditionalLogger GetConditionalLogger<T>(){
            return GetLogger<T>().Conditional();
        }



        public static ConditionalLogger Conditional(this ULogger logger){
            ConditionalLogger result = null;
            if(_cachedConditionalLogger.TryGetValue(logger,out result)){
                return result;
            }else{
                result = new ConditionalLogger(logger);
                _cachedConditionalLogger.Add(logger,result);
                return result;
            }
        }


    }
}
