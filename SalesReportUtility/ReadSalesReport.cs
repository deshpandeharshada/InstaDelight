using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SalesReportUtility
{
    public class ReadSalesReport
    {
        IniFile MyIni;
        string userid = "";
        string DSRFolder = "";
        DateTime dateconfigured = DateTime.Now;
        DateTime dateLastLog = DateTime.Now;
        Authenticate obj = new Authenticate();
        public void ReadINIFile()
        {
            try
            {
                EventLog.LogData("ReadINIFile", true);
                MyIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
                EventLog.LogData("Started Sales Report Utility at " + DateTime.Now, true);

                userid = MyIni.Read("UserId", "Authenticate");
                DSRFolder = MyIni.Read("DSRFolderPath", "Authenticate");
                dateconfigured = Convert.ToDateTime(MyIni.Read("DateConfigured", "ConfigurationDetails"));
                if (MyIni.KeyExists("LastReadTime", "ReadLogDetails"))
                {
                    dateLastLog = Convert.ToDateTime(MyIni.Read("LastReadTime", "ReadLogDetails"));
                }
                else
                {
                    dateLastLog = dateconfigured;
                }

                if (userid != "" || DSRFolder != "")
                {
                    FileInfo[] todaysFiles = new DirectoryInfo(DSRFolder)
                         .EnumerateFiles()
                         .Select(x =>
                         {
                             x.Refresh();
                             return x;
                         })
                         .Where(x => (x.CreationTime.Date >= dateLastLog || x.LastWriteTime >= dateLastLog || x.LastAccessTime >= dateLastLog) && x.Extension == ".csv")
                         .OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.LastWriteTime)
                         .ToArray();

                    //foreach (string file in Directory.EnumerateFiles(DSRFolder, "*.csv"))
                    //foreach (FileInfo file in todaysFiles)
                    //{
                    if (todaysFiles.Length > 0)
                    {
                        FileInfo file = todaysFiles[0];
                        EventLog.LogData("Reading file " + file, true);

                        string mobileno = "";
                        string billamount = "";
                        using (var reader = new StreamReader(file.FullName))
                        {
                            int lineno = 0;
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                var values = line.Split(',');
                                lineno += 1;
                                mobileno = "";
                                billamount = "";

                                for (int i = 0; i < values.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(values[i]) == false)
                                    {
                                        DateTime dt;
                                        bool isDate = DateTime.TryParse(values[i], out dt);
                                        if (isDate)
                                        {
                                            if (dt < dateconfigured)
                                            {
                                                EventLog.LogData("Ignoring line " + lineno.ToString() + " as date is less than the utility startup date", true);
                                                break;
                                            }
                                        }
                                        double n;
                                        bool isNumeric = double.TryParse(values[i], out n);
                                        if (isNumeric)
                                        {
                                            if (values[i].Length == 10)
                                            {
                                                mobileno = values[i].ToString();
                                                //This is a mobile number
                                            }
                                            else if (values[i].Contains("."))
                                            {
                                                billamount = values[i].ToString();
                                                //this is amount
                                            }
                                        }
                                    }
                                }
                                if (billamount == "")
                                    billamount = "0";

                                if (mobileno != "")
                                {
                                    //Send DEC to this mobile no
                                    EventLog.LogData("Adding mobile no " + mobileno + " at " + DateTime.Now, true);
                                    string result = obj.AddNewDECConsumer(userid, mobileno, billamount);
                                    EventLog.LogData(result, true);
                                }
                            }
                        }
                        //Log this file in sales report table
                        EventLog.LogData("Adding sales report log.", true);
                        string result1 = obj.AddSalesReportLog(userid, file.FullName);
                        EventLog.LogData(result1, true);
                        WriteINIFile(file.FullName);
                    }
                    EventLog.LogData("Sales Report Utility finished at " + DateTime.Now, true);
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData(ex.Message, true);
            }
        }

        public void WriteINIFile(string filename)
        {
            MyIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
            MyIni.DeleteKey("LastReadTime");
            MyIni.DeleteKey("LastReadFile");
            MyIni.Write("LastReadTime", DateTime.Now.ToString(), "ReadLogDetails");
            MyIni.Write("LastReadFile", filename, "ReadLogDetails");

        }

    }
}
