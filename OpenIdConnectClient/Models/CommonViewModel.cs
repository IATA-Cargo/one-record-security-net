namespace WebOpenIdConnectClient.Models
{

    public class CommonViewModel
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string VerifyMessage { get; set; }
        public bool VerifyResult { get; set; }
        public string ApiUri { get; set; }
        public string ApiRequestHeader { get; set; }
        public string ApiRequestRespose { get; set; }
        public string AuthenticatedSubject { get; set; }
        public string ResultData { get; set; }
    }

}
