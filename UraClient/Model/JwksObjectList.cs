
namespace UraClient.Model
{
    using System.Collections.Generic;


    public class JwksObjectList
    {
        public List<JwksObject> keys { get; set; }
    }

    public class JwksObject
    {
        public string kty { get; set; }
        public string use { get; set; }
        public string kid { get; set; }
        public string xSt { get; set; }
        public string e { get; set; }
        public string n { get; set; }
        public List<string> x5c { get; set; }
        public string alg { get; set; }

    }
}
