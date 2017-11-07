using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace ConsumerApp
{
    public class EventLog
    {
       static string strPath = HttpContext.Current.Server.MapPath("~/MessageLog/");
       static string strErrorPath = HttpContext.Current.Server.MapPath("~/ErrorLog/");
       static string strFileName = string.Empty;
       static string strLogTime = string.Empty;
        

        public static void LogData(string Data, bool append)
        {
            //string filename = @"D:\Business\JainH2O 23-jan-2015 12AM\JainH2O\JainH2OUI\Log\Sequence.txt";
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                strLogTime = String.Format("{0:dd-MM-yyyy} {1}", DateTime.Now, DateTime.Now.ToLongTimeString());
                strFileName = String.Format("{0}{1:yyyyMMdd}.txt", strPath + "HttpRequest", DateTime.Now);

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(strFileName, append))
                {
                    writer.WriteLine(Data);
                    writer.Close();
                }
            }

            catch (Exception exp)
            {

                Console.WriteLine(exp.Message);

            }
            finally
            {

            }
        }

        public static void LogErrorData(string Data, bool append)
        {
            //string filename = @"D:\Business\JainH2O 23-jan-2015 12AM\JainH2O\JainH2OUI\Log\Sequence.txt";
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                strLogTime = String.Format("{0:dd-MM-yyyy} {1}", DateTime.Now, DateTime.Now.ToLongTimeString());
                strFileName = String.Format("{0}{1:yyyyMMdd}.txt", strErrorPath + "ErrorLog", DateTime.Now);

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(strFileName, append))
                {
                    writer.WriteLine(strLogTime);
                    writer.WriteLine(Data);
                    writer.Close();
                }
            }

            catch (Exception exp)
            {

                Console.WriteLine(exp.Message);

            }
            finally
            {

            }
        }
    }
}