using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesReportUtility.SalesReportWebService;

namespace SalesReportUtility
{
    public class Authenticate
    {
        SalesReportWebService.SalesReportWebService obj = new SalesReportWebService.SalesReportWebService();

        public string ValidateMerchant(string username,string password)
        {
            return obj.Authenticate(username, password);
        }

        public string ConfigureMerchant(string userid)
        {
            return obj.MerchantConfigured(userid);
        }

        public string AddNewDECConsumer(string userid,string mobileno,string billamt)
        {
            return obj.AddNewDECConsumer(userid, mobileno, billamt);
        }

        public string AddSalesReportLog(string userid,string filename)
        {
            return obj.AddSalesReportLog(userid, filename);
        }
    }
}
