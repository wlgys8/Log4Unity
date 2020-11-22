using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Log4Unity{

    public class AppTrack
    {

        public static event System.Action<AppEvent> handleAppEvent;

        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void OnEditorLaunch(){
            UnityEditor.EditorApplication.playModeStateChanged += (state)=>{
                if(state == UnityEditor.PlayModeStateChange.ExitingEditMode){
                    var appEvent = new AppEvent(){
                        eventType = AppEventType.ExitingEditMode,
                    };
                    DispatchAppEvent(appEvent);
                }
            };
        }

        #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnSessionLaunch(){
            Application.quitting += OnSessionQuit;
            var appEvent = new AppEvent(){
                eventType = AppEventType.Launch,
            };
            DispatchAppEvent(appEvent);
        }

        private static void OnSessionQuit(){
            var appEvent = new AppEvent(){
                eventType = AppEventType.Quit,
            };
            DispatchAppEvent(appEvent);
        }

        private static void DispatchAppEvent(AppEvent evt){
            if(handleAppEvent != null){
                handleAppEvent(evt);
            }
        }
    }
}
