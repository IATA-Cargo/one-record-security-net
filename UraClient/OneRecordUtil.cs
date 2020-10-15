using System;
using System.Collections.Generic;
using System.Text;

namespace UraClient
{
    public class OneRecordDummyResponse
    {
        public string message { get; set; }
        public string details { get; set; }
        public OneRecordDummyData result { get; set; }
        public OneRecordTLSID subcriberID { get; set; }
        public long timestamp { get; set; }

    }

    public class OneRecordDummyData
    {
        public int productID { get; set; } = 1;
        public int price { get; set; } = 1000;
        public int quantity { get; set; } = 10;
    }

    public class OneRecordTLSID
    {
        public string desc { get; set; }
        public string subjectDN { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
        public string issuerDN { get; set; }
        public string lastAuthenticatedAt { get; set; }
        public IList<string> oneRecordIDList { set; get; }

    }

    public class Constants
    {
        public const string TYPE_USER_CERTIFICATE = "IATA Subcriber Certificate";
        public const string VERIFY_OK = "OK";
    }
}
