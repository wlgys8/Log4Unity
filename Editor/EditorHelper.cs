using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
namespace MS.Log4Unity.Editor{
    using Configurations;
    internal class EditorHelper
    {
        public const string DEFAULT_CONFIG_FILE_PATH =  "Assets/Resources/log4unity.json";

        [UnityEditor.MenuItem("Edit/Log4Unity/CreateConfigFile")]
        public static void CreateDefaultConfigFileAtResources(){

            if(File.Exists(DEFAULT_CONFIG_FILE_PATH)){
                Debug.LogWarning("already exists file at:" + DEFAULT_CONFIG_FILE_PATH);
                return;
            }
            
            var consoleConfigs = new JsonData();
            consoleConfigs.SetJsonType(JsonType.Object);
            var layout = new JsonData();
            consoleConfigs["layout"] = layout;
            layout.SetJsonType(JsonType.Object);
            layout["type"] = "basic";
            Debug.Log(consoleConfigs.ToJson());

            var c = new Configuration(){
                appenders = new Dictionary<string,Appender>(){
                    {"console",new Appender(){
                        type = "MS.Log4Unity.UnityLogAppender",
                        configs = consoleConfigs,
                    }}
                },
                catagories = new Dictionary<string,Catagory>(){
                    {"default",new Catagory(){
                        appenders = new string[]{"console"},
                        level = "all",
                    }}
                },
            };
            var writter = new JsonWriter();
            writter.PrettyPrint = true;
            JsonMapper.ToJson(c,writter);
            var dir = Path.GetDirectoryName(DEFAULT_CONFIG_FILE_PATH);
            if(!Directory.Exists(dir)){
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(DEFAULT_CONFIG_FILE_PATH,writter.ToString());
            UnityEditor.AssetDatabase.ImportAsset(DEFAULT_CONFIG_FILE_PATH);
        }
    }
}
