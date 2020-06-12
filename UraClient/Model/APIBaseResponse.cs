namespace UraClient.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Response base object
    /// </summary>
    public class APIBaseResponse
    {
        /// <summary>
        /// Response status of the api request
        /// </summary>
        public int ResponseStatus { get; set; }

        /// <summary>
        /// Response headers of the api request
        /// </summary>
        public List<APIBaseParameter> ResponseHeaders { get; set; }

        /// <summary>
        /// Response content of the api request
        /// </summary>
        public string ResponseString { get; set; }

        /// <summary>
        /// Error of the api request
        /// </summary>
        public Exception Error { get; set; }
    }
}
