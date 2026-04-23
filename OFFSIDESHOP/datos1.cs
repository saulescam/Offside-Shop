using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public class datos1
    {
        private static string _valorGlobal = string.Empty;
        public static string valorGlobal
        {
            get { return _valorGlobal; }
            set { _valorGlobal = value; }
        }
    }
}