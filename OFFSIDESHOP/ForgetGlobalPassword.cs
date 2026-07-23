using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public class ForgetGlobalPassword
    {
        private static string _valorGlobal = string.Empty;

        public static string ValorGlobal
        {
            get { return _valorGlobal; }
            set { _valorGlobal = value; }
        }
    }
}