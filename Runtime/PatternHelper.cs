using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MS.Log4Unity{
    using Configurations;

    public struct PatternFieldResolveContext{
        public ULogger logger;
        public LogType logType;
        public string message;
        public string parameter;
    }

    public delegate string PatternFieldResolve(PatternFieldResolveContext context);


    public class PatternHelper
    {

        private static Dictionary<char,PatternFieldResolve> _resolverMap = new Dictionary<char, PatternFieldResolve>();
        private static PatternFormatter _formatter = new PatternFormatter();
        private static Dictionary<string,Layout> _layouts = new Dictionary<string,Layout>();
        private static Dictionary<LogType,string> _logTypeToColor = new Dictionary<LogType, string>(){
            {LogType.Debug,"blue"},
            {LogType.Info,null},
            {LogType.Warn,"yellow"},
            {LogType.Error,"red"},
            {LogType.Fatal,"magenta"},
        };

        static PatternHelper(){

            //register specific field
            RegisterPatternFieldResolver('c',(ctx)=>{
                return ctx.logger.catagory;
            });
            RegisterPatternFieldResolver('r',(ctx)=>{
                return System.DateTime.Now.ToLocalTime().ToLongTimeString();
            });
            RegisterPatternFieldResolver('p',(ctx)=>{
                return ctx.logType.ToString();
            });
            RegisterPatternFieldResolver('m',(ctx)=>{
                return ctx.message;
            });
            RegisterPatternFieldResolver('d',(ctx)=>{
                return System.DateTime.Now.ToString(ctx.parameter);
            });
            RegisterPatternFieldResolver('n',(ctx)=>{
                return "\n";
            });
            //start color block
            RegisterPatternFieldResolver('[',(ctx)=>{
                string c = TryGetColor(ctx.logType);
                if(c == null){
                    return null;
                }
                return string.Format("<color=\"{0}\">",c);
            });
            //end color block
            RegisterPatternFieldResolver(']',(ctx)=>{
                string c = TryGetColor(ctx.logType);
                if(c == null){
                    return null;
                }
                return "</color>";
            });


            //register builtin layout

            RegisterLayout(new Layout(){
                type = "basic",
                pattern = "[%d] [%p] %c - %m"
            });
             RegisterLayout(new Layout(){
                type = "coloured",
                pattern = "%[[%d] [%p] %c%] - %m"
            });
            RegisterLayout(new Layout(){
                type = "messagePassThrough",
                pattern = null,
            });
        }

        public static string TryGetColor(LogType logType){
            string color = null;
            if(_logTypeToColor.TryGetValue(logType,out color)){
            }
            return color;
        }

        public static void RegisterPatternFieldResolver(char specificChar,PatternFieldResolve resolver){
            _resolverMap.Add(specificChar,resolver);
        }

        public static void RegisterLayout(Layout layout){
            _layouts.Add(layout.type,layout);
        }

        private static PatternFieldResolve TryGetResolver(char specificChar){
            PatternFieldResolve v = null;
            if(_resolverMap.TryGetValue(specificChar,out v)){
                return v;
            }
            return null;
        }
        
        public static string Format(string pattern, ULogger logger, LogType type, object message){
            return _formatter.Format(pattern,logger,type,message);
        }

        public static string FormatWithLayout(Layout layout, ULogger logger, LogType type, object message){
            var messageStr = message == null?"":message.ToString();
            Layout actualLayout = null;
            if(layout.type == "pattern"){
                actualLayout = layout;
            }else{
                if(!_layouts.TryGetValue(layout.type,out actualLayout)){
                    return messageStr;
                }
            }
            if(actualLayout == null){
                return messageStr;
            }
            return Format(actualLayout.pattern,logger,type,message);
        }



        private class PatternFormatter{

            private const char ESCAPE_CHAR = '%';
            private const char PARAMETER_BEGIN_CHAR = '{';
            private const char PARAMETER_END_CHAR = '}';
            
            private int _index;
            private string _pattern;

            private PatternFieldResolve _fieldResolver;
            private StringBuilder _messageBuilder = null;
            private StringBuilder _fieldParameterBuilder;


            public PatternFormatter(){

            }

            private bool MoveNext(){
                _index ++;
                return _index < _pattern.Length;
            }

            public char charValue{
                get{
                    return _pattern[_index];
                }
            }


            private void BeginParseField(ref PatternFieldResolveContext ctx){
                ctx.parameter = null;
                var vName = this.charValue;
                _fieldResolver = TryGetResolver(vName);
            }
 
            private bool IsParsingField(){
                return _fieldResolver != null;
            }

           
            private void EndParseField(ref PatternFieldResolveContext ctx){
                var value = _fieldResolver(ctx);
                if(value != null){
                    _messageBuilder.Append(value);
                }
                _fieldResolver = null;
            }

            private void BeginParseFieldParameter(){
                _fieldParameterBuilder = new StringBuilder();
            }

            private bool IsParsingFieldParameter(){
                return _fieldParameterBuilder != null;
            }

            private string EndParseFieldParameter(){
                var s = _fieldParameterBuilder.ToString();
                _fieldParameterBuilder = null;
                return s;
            }


            private bool ReadCharValueIfNotParsingField(ref PatternFieldResolveContext context){
                var charValue = this.charValue;
                if(charValue == ESCAPE_CHAR){
                    if(!MoveNext()){
                        return false;
                    }
                    this.BeginParseField(ref context);
                }else{
                    _messageBuilder.Append(charValue);
                }
                return true;
            }

            public string Format(string pattern, ULogger logger, LogType type, object message){
                var messageStr = message == null? "":message.ToString();
                if(pattern == null){
                    return messageStr;
                }
                _index = -1;
                _pattern = pattern;
                _messageBuilder = new StringBuilder();
                _fieldParameterBuilder = null;
                _fieldResolver = null;
                var context = new PatternFieldResolveContext(){
                    logger = logger,
                    logType = type,
                    message = messageStr
                };

                while (true)
                {
                    if(!MoveNext()){
                        break;
                    }
                    var charValue = this.charValue;
                    if(this.IsParsingField()){
                        if(this.IsParsingFieldParameter()){
                            if(charValue == PARAMETER_END_CHAR ){
                                var p = this.EndParseFieldParameter();
                                context.parameter = p;
                            }else{
                                _fieldParameterBuilder.Append(charValue);
                            }
                        }else{
                            if(charValue == PARAMETER_BEGIN_CHAR){
                                BeginParseFieldParameter();
                            }else{
                                EndParseField(ref context);
                                if(!ReadCharValueIfNotParsingField(ref context)){
                                    break;
                                }
                            }
                        }
                    }else{
                        if(!ReadCharValueIfNotParsingField(ref context)){
                            break;
                        }
                    }
                }
                if(IsParsingField()){
                    EndParseField(ref context);
                }
                return _messageBuilder.ToString();                      
            }
        }
    }
}
