using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UDCG.Application.Interface;

namespace UDCG.Application.Feature.Users.Services
{
    public class AppLoggerSevice : IAppLoggerSevice
    {
        public void LogMessage(string message)
        {
            try
            {
                var LogFileName = "\\LogFile - " + DateTime.Today.ToShortDateString() + ".txt";
                LogFileName = LogFileName.Replace("/", "-");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + LogFileName, true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
            }
        }
    }
}
