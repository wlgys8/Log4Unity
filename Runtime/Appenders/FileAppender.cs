using System.Collections;
using System.Collections.Generic;
using MS.Log4Unity.Configurations;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace MS.Log4Unity{
    using FileRoller;
    public class FileAppender : LayoutAppender
    {   
        private const string FILE_NAME = "Logs/app.log";

        private static string DEFAULT_FILE_NAME{
            get{
                #if UNITY_EDITOR
                return FILE_NAME;
                #else
                return Application.persistentDataPath + "/" + FILE_NAME;
                #endif
            }
        }

        private static byte[] _NEW_LINE;

        private static byte[] NEW_LINE{
            get{
                if(_NEW_LINE == null){
                    _NEW_LINE = System.Text.Encoding.UTF8.GetBytes("\n");
                }
                return _NEW_LINE;
            }
        }


        private string _filePath;
        private FileStream _fileStream;

        private bool _isFileStreamInitialized = false;

        private int _maxFileCount = 3;
        private int _maxFileSize = 1024;

        private int _flushIntervalMillSeconds = 1000;

        private System.DateTime _lastFlushTime;
        

        private List<object> _messageQueueToFile = new List<object>();
        private bool _isFileWriting = false;
        private BaseRollingFileStream _rollingFS;

        public override void OnInitialize(ConfigsReader configs)
        {
            base.OnInitialize(configs);
            string fileName = configs.GetString("fileName",null);
            if(fileName == null){
                fileName = DEFAULT_FILE_NAME;
            }
            this._filePath = fileName;
            this._maxFileCount = Mathf.Max(1,configs.GetInt("maxBackups",3));
            this._maxFileSize = Mathf.Max(10 * 1024,configs.GetInt("maxFileSize",1024 * 1024));
            _flushIntervalMillSeconds = Mathf.Max(0,configs.GetInt("flushInterval",1000));
            RollType rollType = RollType.Size;
            var rollTypeStr = configs.GetString("rollType",null);
            if(rollTypeStr != null){
                if(!System.Enum.TryParse<RollType>(rollTypeStr,out rollType)){
                    UnityEngine.Debug.LogWarning($"bad rollType:{rollTypeStr}");
                    rollType = RollType.Size;
                }
            }
            AppTrack.handleAppEvent += HandleAppEvent;
            if(rollType == RollType.Session){
                _rollingFS = new RollingFileStream(_filePath,new RollingFileStream.Options(){
                    maxFileSize = _maxFileSize,
                    numToKeep = _maxFileCount,
                });
                _rollingFS.Roll();
            }else if(rollType == RollType.Date){
                _rollingFS = new DateRollingFileStream(_filePath,new DateRollingFileStream.Options(){
                    maxBackups = _maxFileCount,
                    maxFileSize = _maxFileSize
                });
            }else if(rollType == RollType.Size){
                _rollingFS = new RollingFileStream(_filePath,new RollingFileStream.Options(){
                    maxFileSize = _maxFileSize,
                    numToKeep = _maxFileCount,
                });
            }
        }
     
        public override bool HandleLogEvent(ref LogEvent logEvent){
            if(!base.HandleLogEvent(ref logEvent)){
                return false;
            }
            if(_rollingFS == null){
                return false;
            }
            QueueMessage(logEvent.message);
            return true;
        }

        private void QueueMessage(object message){
            var empty = _messageQueueToFile.Count == 0;
            _messageQueueToFile.Add(message);
            if(!_isFileWriting){
                WriteMessageToFileAsync();
            }   
        }
        
        
        private async void WriteMessageToFileAsync(){
            _isFileWriting = true;
            while(_messageQueueToFile.Count > 0 ){
                var message = _messageQueueToFile[0];
                _messageQueueToFile.RemoveAt(0);
                if(message == null){
                    message = "";
                }
                var bytes = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                await _rollingFS.WriteAsync(bytes,0,bytes.Length);
                await _rollingFS.WriteAsync(NEW_LINE,0,NEW_LINE.Length);
            }
            _isFileWriting = false;
            var deltaTime = System.DateTime.Now - _lastFlushTime;
            if(deltaTime.Milliseconds < _flushIntervalMillSeconds){
                await Task.Delay((int)(_flushIntervalMillSeconds - deltaTime.Milliseconds));
            }
            _lastFlushTime = System.DateTime.Now;
            await _rollingFS.FlushAsync();
        }

        private void HandleAppEvent(AppEvent appEvent)
        {
            if(appEvent.eventType == AppEventType.Quit || 
            appEvent.eventType == AppEventType.ExitingEditMode){
                _messageQueueToFile.Clear();
                _rollingFS.Close();
            }
        }


        public enum RollType{
            Size,
            Session,
            Date,
        }

    }
}
