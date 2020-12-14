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
            var basicLayout = new JsonData();
            basicLayout.SetJsonType(JsonType.Object);
            basicLayout["type"] = "basic";

            var basicTimeLayout = new JsonData();
            basicTimeLayout.SetJsonType(JsonType.Object);
            basicTimeLayout["type"] = "basic-time";
            
            var consoleConfigs = new JsonData();
            consoleConfigs.SetJsonType(JsonType.Object);
            consoleConfigs["layout"] = basicLayout;
            

            var fileConfigs = new JsonData();
            fileConfigs.SetJsonType(JsonType.Object);
            fileConfigs["rollType"] = "Session";
            fileConfigs["env"] = Env.EditorPlayer.ToString();
            fileConfigs["layout"]= basicTimeLayout;

            var c = new Configuration(){
                appenders = new Dictionary<string,Appender>(){
                    {"console",new Appender(){
                        type = "UnityLogAppender",
                        configs = consoleConfigs,
                    }},
                    {
                        "file",new Appender(){
                            type = "FileAppender",
                            configs = fileConfigs,
                        }
                    }
                },
                catagories = new Dictionary<string,Catagory>(){
                    {"default",new Catagory(){
                        appenders = new string[]{"console","file"},
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
