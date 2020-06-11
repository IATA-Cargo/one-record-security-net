namespace UraClient.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Input data when create the certificate request
    /// </summary>
    public class CreateRequestInput
    {
        /// <summary>
        /// The csr
        /// </summary>
        public string csr { get; set; }

        /// <summary>
        /// The id of user
        /// </summary>
        public int userid { get; set; }

        /// <summary>
        /// The id of template
        /// </summary>
        public int templateid { get; set; }

        /// <summary>
        /// The subjects of certificate
        /// </summary>
        public Dictionary<string, string> subject { get; set; }

        /// <summary>
        /// The sans of certificate
        /// </summary>
        public SanInput san { get; set; }

        /// <summary>
        /// Time to validity for certificate (unit day)
        /// </summary>
        public int validity_period { get; set; }

        /// <summary>
        /// The key type of certificate
        /// </summary>
        public string key_type { get; set; }

        /// <summary>
        /// The length type of certificate
        /// </summary>
        public int key_length { get; set; }

        /// <summary>
        /// The key name of certificate
        /// </summary>
        public string key_name { get; set; }
    }

    /// <summary>
    /// The sans information add to certificate
    /// </summary>
    public class SanInput
    {
        /// <summary>
        /// List dns in certificate
        /// </summary>
        public List<string> dns { get; set; }

        /// <summary>
        /// List email in certificate
        /// </summary>
        public List<string> email { get; set; }
    }
}
