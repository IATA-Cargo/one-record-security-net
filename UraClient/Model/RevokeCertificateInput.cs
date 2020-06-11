namespace UraClient.Model
{
    /// <summary>
    /// Input data when revoke a certificate
    /// </summary>
    public class RevokeCertificateInput
    {
        /// <summary>
        /// The id of certificate
        /// </summary>
        public int certificateId { get; set; }

        /// <summary>
        /// The id of revocation reason
        /// </summary>
        public int revocationReasonId { get; set; }

        /// <summary>
        /// The decription for revocation
        /// </summary>
        public string revocationComment { get; set; }
    }
}
