using System.Collections;
using System.Collections.Generic;
using MS.Log4Unity.Configurations;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace MS.Log4Unity{
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


        private string _fileName;
        private FileStream _fileStream;

        private bool _isFileStreamInitialized = false;

        private int _maxFileCount = 3;
        private int _maxFileSize = 1024;

        private int _flushIntervalMillSeconds = 1000;

        private System.DateTime _lastFlushTime;
        

        private List<string> _messageQueueToFile = new List<string>();
        private bool _isFileWriting = false;

        public override void OnInitialize(ConfigsReader configs)
        {
            base.OnInitialize(configs);
            string fileName = configs.GetString("fileName",null);
            if(fileName == null){
                fileName = DEFAULT_FILE_NAME;
            }
            this._fileName = fileName;

            this._maxFileCount = Mathf.Max(1,configs.GetInt("maxFileCount",3));
            this._maxFileSize = Mathf.Max(10 * 1024,configs.GetInt("maxFileSize",1024 * 1024));
            _flushIntervalMillSeconds = Mathf.Max(0,configs.GetInt("flushInterval",1000));
            AppTrack.handleAppEvent  += HandleAppEvent;
        }

        private FileStream fileStream{
            get{
                if(!_isFileStreamInitialized){
                    _isFileStreamInitialized = true;
                    this.AllocateNewFileStream();
                }
                return _fileStream;
            }
        }

        private void AllocateNewFileStream(){
            if(!File.Exists(this._fileName)){
                var dirName = Path.GetDirectoryName(this._fileName);
                if(!Directory.Exists(dirName)){
                    Directory.CreateDirectory(dirName);
                }
            }
            _fileStream = new FileStream(this._fileName,FileMode.Append);
        }

        private void CloseFileStream(){
            if(_fileStream == null){
                return;
            }
            _fileStream.Close();
            _fileStream = null;
        }

        public override bool HandleLogEvent(ref LogEvent logEvent){
            if(!base.HandleLogEvent(ref logEvent)){
                return false;
            }
            if(fileStream == null){
                return false;
            }
            QueueMessage(logEvent.message);
            // WriteMessageToFileAsync(logEvent.message);
            return true;
        }

        private void QueueMessage(string message){
            var empty = _messageQueueToFile.Count == 0;
            _messageQueueToFile.Add(message);
            if(!_isFileWriting){
                WriteMessageToFileAsync();
            }   
        }
        
        
        private async void WriteMessageToFileAsync(){
            _isFileWriting = true;
            while(_messageQueueToFile.Count > 0 ){
                string message = _messageQueueToFile[0];
                _messageQueueToFile.RemoveAt(0);
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                await _fileStream?.WriteAsync(bytes,0,bytes.Length);
                await _fileStream?.WriteAsync(NEW_LINE,0,NEW_LINE.Length);
                this.RollIfNeed();
            }
            _isFileWriting = false;
            var deltaTime = System.DateTime.Now - _lastFlushTime;
            if(deltaTime.Milliseconds < _flushIntervalMillSeconds){
                await Task.Delay((int)(_flushIntervalMillSeconds - deltaTime.Milliseconds));
            }
            _lastFlushTime = System.DateTime.Now;
            await _fileStream?.FlushAsync();
        }

        private void HandleAppEvent(AppEvent appEvent)
        {
            if(appEvent.eventType == AppEventType.Quit || appEvent.eventType == AppEventType.ExitingEditMode){
                _messageQueueToFile.Clear();
                this.CloseFileStream();
            }
        }


        private bool RollIfNeed(){
            if(_fileStream == null){
                return false;
            }
            if(_fileStream.Length < _maxFileSize){
                return false;
            }
            _fileStream.Close();
            var index = _maxFileCount - 1;

            //the last backup file
            var targetFileName = $"{_fileName}.{index}";
            if(File.Exists(targetFileName)){
                File.Delete(targetFileName);
            }

            index --;
            while(index > 0){
                targetFileName = $"{_fileName}.{index}";
                if(File.Exists(targetFileName)){
                    File.Move(targetFileName,$"{_fileName}.{index + 1}");
                }
                index --;
            }
            File.Move(_fileName,$"{_fileName}.1");
            this.AllocateNewFileStream();
            return true;
        }

    }
}
