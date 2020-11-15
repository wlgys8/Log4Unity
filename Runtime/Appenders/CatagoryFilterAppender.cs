using System.Text.RegularExpressions;

namespace MS.Log4Unity{
    using Configurations;
    public class CatagoryFilterAppender : BaseAppender
    {
        private IAppender _redirectAppender;
        private Regex _catagoryRegex;

        public override void OnInitialize(ConfigsReader configs)
        {
            base.OnInitialize(configs);
            var appenderName = configs.GetString("appender",null);
            if(appenderName != null){
                _redirectAppender = AppendersManager.GetAppender(appenderName);
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
                if(_redirectAppender != null){
                    _redirectAppender.HandleLogEvent(ref logEvent);
                }
                return true;
            }
            return false;
        }
    }
}
