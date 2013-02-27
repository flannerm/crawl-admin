using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.IO;
using Oracle.DataAccess.Client;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Crawl.DataAccess
{
    public class DbConnection
    {
        
        public static OracleConnection createConnectionSDR()
        {
            OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["SDRDbConn"].ConnectionString);

            cn.Open();

            return cn;
        }






    }

}

