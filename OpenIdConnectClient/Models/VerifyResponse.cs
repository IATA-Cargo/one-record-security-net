namespace WebOpenIdConnectClient.Models
{
    using System.Collections.Generic;

    public class VerifyResponse
    {
        public string message { get; set; }

        public string details { get; set; }

        public VerifyResponseData result { get; set; }
        public AuthenticatedSubject authenticatedSubject { get; set; }

        public long timestamp { get; set; }

    }

    public class VerifyResponseMutual
    {
        public string message { get; set; }

        public string details { get; set; }

        public VerifyResponseData result { get; set; }
        public AuthenticatedSubjectMutual authenticatedSubject { get; set; }

        public long timestamp { get; set; }

    }

    public class VerifyResponseData
    {
        public int productID { get; set; } = 1;
        public int price { get; set; } = 1000;
        public int quantity { get; set; } = 10;
    }

    public class AuthenticatedSubject
    {

        public string type { get; set; }
        public string subjectDN { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
        public string issuerDN { get; set; }
        public string lastAuthenticatedAt { get; set; }
        public string validationService { get; set; } = "IATA Trust Platform";
    }

    public class AuthenticatedSubjectMutual : AuthenticatedSubject
    {
        public IList<string> sans { get; set; }
    }
}
