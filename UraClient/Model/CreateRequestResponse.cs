namespace UraClient.Model
{

    /// <summary>
    /// Response data when create the certificate request
    /// </summary>
    public class CreateRequestResponse : ResponseBase
    {
        /// <summary>
        /// The id of requests
        /// </summary>
        public int requestid { get; set; }

        /// <summary>
        /// The status of the certificate request
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// The id of certificate
        /// </summary>
        public int certificateid { get; set; }

        /// <summary>
        /// The id of user
        /// </summary>
        public int userid { get; set; }

        /// <summary>
        /// The id of template
        /// </summary>
        public int templateid { get; set; }

        /// <summary>
        /// Certificate content
        /// </summary>
        public string certificate { get; set; }

        /// <summary>
        /// Certificate content with pkcs 12 format
        /// </summary>
        public string certificate_pkcs12 { get; set; }

        /// <summary>
        /// The passphrase of pkcs 12
        /// </summary>
        public string passPhrase { get; set; }

        /// <summary>
        /// The issuer of certificate
        /// </summary>
        public string issuer { get; set; }

        /// <summary>
        /// The serial number of certificate
        /// </summary>
        public string serialNumber { get; set; }

        /// <summary>
        /// The start time valid of certificate
        /// </summary>
        public string validFrom { get; set; }

        /// <summary>
        /// The end time valid of certificate
        /// </summary>
        public string validUntil { get; set; }

        /// <summary>
        /// The issued DN of certificate
        /// </summary>
        public string issuedDN { get; set; }
    }
}
