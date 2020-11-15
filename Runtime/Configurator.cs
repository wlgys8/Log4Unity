using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

namespace MS.Log4Unity{

    using Configurations;


    internal class Configurator
    {
        public const string DEFAULT_CATAGORY = "default";

        private static Dictionary<string,Catagory> _catagories = new Dictionary<string,Catagory>();
        private static Configuration _configuration;

        public static bool TryLoad(string configFile){
            if(!File.Exists(configFile)){
                return false;
            }
            var text = File.ReadAllText(configFile);
            return TryLoadFromText(text);
        }

        public static void Load(string configFile){
            if(!TryLoad(configFile)){
                throw new System.Exception($"configFile not found at {configFile}");
            }
        }

        public static bool TryLoadFromResources(string configFile){
            var textAsset = Resources.Load<TextAsset>(configFile);
            if(!textAsset){
                return false;
            }
            return TryLoadFromText(textAsset.text);
        }

        public static void LoadFromResources(string configFile){
            if(!TryLoadFromResources(configFile)){
                throw new System.Exception($"configFile not found at Resources/{configFile}");
            }
        }

        public static void LoadFromText(string configText){
            TryLoadFromText(configText);
            if(_configuration == null){
                _configuration = new Configuration();
            }
                  
        }

        public static bool TryLoadFromText(string configText){
            try{
                _configuration = JsonMapper.ToObject<Configuration>(configText);
                return true;
            }catch(System.Exception e){
                Debug.LogException(e);
                return false;
            }
        }
        
        private static Catagory GetCatagory(string name){
            if(_configuration == null){
                return null;
            }
            if(_configuration.catagories == null){
                return null;
            }
            if(!_configuration.catagories.ContainsKey(name)){
                return null;
            }
            return _configuration.catagories[name];
        }

        public static Appender GetAppender(string name){
            if(!CheckConfigFileLoaded()){
                return null;
            }
            Appender ret = null;
            if(_configuration.appenders.TryGetValue(name,out ret)){
                return ret;
            }else{
                Debug.LogWarning("can not find appender by name = " + name);
            }
            return null;
        }

        private static bool IsConfigurationLoaded(){
            return _configuration != null;
        }

        private static bool _notConfiguratedWarningShowed = false;
        private static bool _hasTriedLoadDefaultConfigFile = false;

        private static bool CheckConfigFileLoaded(){
            if(!IsConfigurationLoaded()){
                if(!_notConfiguratedWarningShowed){
                    Debug.LogWarning("config file must be loaded first before you call any api of logger!");
                    _notConfiguratedWarningShowed = true;
                }
                return false;
            }
            return true;
        }

        private static bool TryLoadDefaultConfigIfNot(){
            if(!IsConfigurationLoaded()){
                if(!_hasTriedLoadDefaultConfigFile){
                    _hasTriedLoadDefaultConfigFile = true;
                     if(TryLoadFromResources("log4unity")){
                         return true;
                     }
                }
                if(!_notConfiguratedWarningShowed){
                    Debug.LogWarning("config file must be loaded first before you call any api of logger!");
                    _notConfiguratedWarningShowed = true;
                }
                return false;
            }
            return true;
        }

        public static void ConfigurateLogger(ILogger logger,string catagoryName){
            if(!TryLoadDefaultConfigIfNot()){
                return;
            }
            var catagory = Configurator.GetCatagory(catagoryName);
            if(catagory == null){
                catagory = Configurator.GetCatagory(DEFAULT_CATAGORY);
            }
            logger.ClearAppenders();
            var level = ParseLogLevel(catagory.level);
            logger.level = level;
            if(catagory.appenders == null || catagory.appenders.Length == 0){
                //default
            }else{
                foreach(var ap in catagory.appenders){
                    var appender = AppendersManager.GetAppender(ap);
                    if(appender != null){
                        logger.Append(appender);
                    }
                }
            }
        }

        private static LogLevel ParseLogLevel(string level){
            if(level == null){
                return LogLevel.All;
            }
            LogLevel ret;
            if(System.Enum.TryParse<LogLevel>(level,true,out ret)){
                return ret;
            }else{
                Debug.LogWarning($"failed to parse {level} to LogLevel");
                return LogLevel.All;
            }
        }
    }

    [System.Flags]
    public enum Env{
        EditorPlayer = 1,
        BuiltPlayer = 2,
        All = 3,
    }

    public struct AppEvent{
        public AppEventType eventType;
    }

    public enum AppEventType{
        Quit,

        /// <summary>
        /// this event only happen in UnityEditor
        /// </summary>
        ExitingEditMode,
    }


    namespace Configurations{

        public class Configuration{

            public Dictionary<string,Appender> appenders;
            public Dictionary<string,Catagory> catagories; 
        }


        public class Layout{
            public string type;
            public string pattern;

            public Layout(){
                this.type = "basic";
            }

            public Layout(ConfigsReader configs){
                this.type = configs.GetString("type","basic");
                this.pattern = configs.GetString("pattern",null);
            }
        }


        public class Appender{
            public string type;
            public JsonData configs;

            private ConfigsReader _configsReader;

            public ConfigsReader GetConfigsReader(){
                if(_configsReader == null){
                    _configsReader = new ConfigsReader(configs);
                }
                return _configsReader;
            }

      

        }

        public class Catagory{
            public string[] appenders;
            public string level;
        }


        public class ConfigsReader{

            private JsonData _configs;

            internal ConfigsReader(JsonData configs){
                _configs = configs;
            }

            public bool Has(string key){
                return _configs.ContainsKey(key);
            }

            public int GetInt(string key,int defaultValue){
                if(!Has(key)){
                    return defaultValue;
                }
                var value = _configs[key];
                return (int)value;
            }

            public string GetString(string key,string defaultValue){
                if(!Has(key)){
                    return defaultValue;
                }
                var value = _configs[key];
                return (string)value;
            }

            public bool GetBool(string key,bool defaultValue){
                if(!Has(key)){
                    return defaultValue;
                }
                var value = _configs[key];
                return (bool)value;
            }

            public ConfigsReader GetConfigs(string key){
                if(!Has(key)){
                    return null;
                }
                var value = _configs[key];
                return new ConfigsReader(value);
                
            }

            
        }
    }
}




