namespace UraClient.Model
{
    /// <summary>
    /// API Base Parameter
    /// </summary>
    public class APIBaseParameter
    {
        /// <summary>
        /// Key of parameter
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Object value of parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// String value of parameter
        /// </summary>
        public string StringValue { get; set; }
    }
}
