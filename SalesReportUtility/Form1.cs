using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ASquare.WindowsTaskScheduler;
using ASquare.WindowsTaskScheduler.Models;
using ASquare.WindowsTaskScheduler.Interface;

namespace SalesReportUtility
{
    public partial class Form1 : Form
    {
        Authenticate obj = new Authenticate();

        IniFile MyIni;
        string mobilenumber, password, userid;

        public void ReadINIFile()
        {
            try
            {
                EventLog.LogErrorData("ReadINIFile", true);
                MyIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
                mobilenumber = MyIni.Read("Username", "Authenticate");
                password = MyIni.Read("Pwd", "Authenticate");
                userid = MyIni.Read("UserId", "Authenticate");
                lblFolderPath.Text = MyIni.Read("DSRFolderPath", "Authenticate");
                txtUserName.Text = mobilenumber;
                txtPassword.Text = password;

                if (mobilenumber != "" || password != "" || userid != "")
                {
                    string status = obj.ValidateMerchant(mobilenumber, password);
                    if (status != userid)
                    {
                        MessageBox.Show("User credentials are changed. Please configure exe again");

                        MyIni.DeleteKey("username");
                        MyIni.DeleteKey("Pwd");
                        MyIni.DeleteKey("UserId");
                        MyIni.DeleteKey("DSRFolderPath");
                        MyIni.DeleteSection("Authenticate");
                    }
                    else if (Directory.Exists(lblFolderPath.Text) == false)
                    {
                        MessageBox.Show("DSR folder has been moved or renamed. Please configure exe again");

                        MyIni.DeleteKey("username");
                        MyIni.DeleteKey("Pwd");
                        MyIni.DeleteKey("UserId");
                        MyIni.DeleteKey("DSRFolderPath");
                        MyIni.DeleteSection("Authenticate");
                    }
                    else
                    {
                        ReadSalesReport rpt = new ReadSalesReport();
                        rpt.ReadINIFile();


                        this.Close();
                        this.Dispose();
                    }
                }
                else
                {
                    // MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + "SalesReportUtility.exe");
                    //First time execution. Schedule exe.

                }
            }
            catch (Exception ex)
            {
                //  MessageBox.Show("Could not read ini file");
                EventLog.LogErrorData(ex.Message, true);
                this.Close();
                this.Dispose();
            }
        }

        public void WriteINIFile()
        {
            try
            {
                MyIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
                MyIni.DeleteKey("username");
                MyIni.DeleteKey("Pwd");
                MyIni.DeleteKey("UserId");
                MyIni.DeleteKey("DSRFolderPath");

                MyIni.Write("Username", mobilenumber, "Authenticate");
                MyIni.Write("Pwd", password, "Authenticate");
                MyIni.Write("UserId", userid, "Authenticate");
                MyIni.Write("DSRFolderPath", lblFolderPath.Text, "Authenticate");

                if (MyIni.KeyExists("DateConfigured") == false)
                {
                    MyIni.Write("DateConfigured", DateTime.Now.ToString(), "ConfigurationDetails");
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show("Could not read ini file");
                EventLog.LogErrorData(ex.Message, true);
                this.Close();
                this.Dispose();
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReadINIFile();
        }

        private void cmdConfigure_Click(object sender, EventArgs e)
        {

            if (txtUserName.Text == "")
            {
                MessageBox.Show("Please enter user name");
                return;
            }

            if (txtPassword.Text == "")
            {
                MessageBox.Show("Please enter password");
                return;
            }
            if (lblFolderPath.Text == "")
            {
                MessageBox.Show("Please select path of DSR folder");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            string status = obj.ValidateMerchant(txtUserName.Text, txtPassword.Text);
            if (status.Contains("Error") == false)
            {
                mobilenumber = txtUserName.Text;
                password = txtPassword.Text;
                userid = status;

                obj.ConfigureMerchant(userid);
                //User authenticated
                WriteINIFile();
                EventLog.LogData("Schedule exe " + AppDomain.CurrentDomain.BaseDirectory + "SalesReportUtility.exe", true);
                SchedulerResponse response = WindowTaskScheduler
   .Configure()
   .CreateTask("SalesReportReader", "\"" + AppDomain.CurrentDomain.BaseDirectory + "SalesReportUtility.exe" + "\"")
   .RunDaily()
   .RunEveryXMinutes(15)
   .RunDurationFor(new TimeSpan(23, 59, 59))
   .SetStartDate(DateTime.Now)
   .SetStartTime(new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 10, 0))
   .Execute();

                EventLog.LogData(response.IsSuccess.ToString(), true);
                EventLog.LogData(response.ErrorMessage, true);
                ReadINIFile();
            }
            else
            {
                EventLog.LogErrorData(status, true);
                MessageBox.Show(status);
            }
            Cursor.Current = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                lblFolderPath.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }


    }
}
