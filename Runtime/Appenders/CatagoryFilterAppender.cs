using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MS.Log4Unity{
    using Configurations;
    public class CatagoryFilterAppender : BaseAppender
    {
        private List<IAppender> _redirectAppenders = new List<IAppender>();
        private Regex _catagoryRegex;

        public override void OnInitialize(ConfigsReader configs)
        {
            base.OnInitialize(configs);
            var appenderNames = configs.GetStringArray("appenders",null);
            if(appenderNames != null){
                foreach(var name in appenderNames){
                    var appender = AppendersManager.GetAppender(name);
                    if(appender != null){
                        _redirectAppenders.Add(appender);
                    }
                }
            }
            var catagory = configs.GetString("catagory",null);
            if(catagory != null){
                _catagoryRegex = new Regex(catagory);
            }
        }
        
        public override bool HandleLogEvent(ref LogEvent logEvent)
        {
            if(base.HandleLogEvent(ref logEvent)){
                if(_catagoryRegex != null && !_catagoryRegex.IsMatch(logEvent.logger.catagory)){
                    return false;
                }

                if(_redirectAppenders != null){
                    foreach(var appender in _redirectAppenders){
                        var e = logEvent;
                        appender.HandleLogEvent(ref e);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
