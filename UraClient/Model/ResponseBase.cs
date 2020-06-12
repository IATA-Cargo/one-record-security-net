namespace UraClient.Model
{
    /// <summary>
    /// Base response data, common data of all request
    /// </summary>
    public class ResponseBase
    {
        /// <summary>
        /// the code of server process
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// The time of server process
        /// </summary>
        public string createdat { get; set; }
    }

}
