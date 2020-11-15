using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace MS.Log4Unity{

    public interface IFilter{

        bool OnFilter(ILogger logger,IAppender appender);
    }



    public class CatagoryFilter:IFilter{

        private Regex _catagoryRegex;

        public CatagoryFilter(string catagoryRegex){
            _catagoryRegex = new Regex(catagoryRegex);
        }

        public bool OnFilter(ILogger logger, IAppender appender)
        {
            return _catagoryRegex.IsMatch(logger.catagory);
        }
    }
}
