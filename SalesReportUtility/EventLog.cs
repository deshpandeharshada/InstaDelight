using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SalesReportUtility
{
    public class EventLog
    {
        static string strPath = AppDomain.CurrentDomain.BaseDirectory + "MessageLog";

        static string strErrorPath = AppDomain.CurrentDomain.BaseDirectory + "ErrorLog";
        static string strFileName = string.Empty;
        static string strLogTime = string.Empty;


        public static void LogData(string Data, bool append)
        {            
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                strLogTime = String.Format("{0:dd-MM-yyyy} {1}", DateTime.Now, DateTime.Now.ToLongTimeString());
                strFileName = String.Format("{0}{1:yyyyMMdd}.txt", strPath + "/EventLog", DateTime.Now);

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
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                strLogTime = String.Format("{0:dd-MM-yyyy} {1}", DateTime.Now, DateTime.Now.ToLongTimeString());
                strFileName = String.Format("{0}{1:yyyyMMdd}.txt", strErrorPath + "/ErrorLog", DateTime.Now);

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
