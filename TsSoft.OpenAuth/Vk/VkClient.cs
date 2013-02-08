namespace TsSoft.OpenAuth.Vk
{
    using DotNetOpenAuth.AspNet.Clients;
    using DotNetOpenAuth.Messaging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Web;

    public class VkClient : OAuth2Client
    {
        public const string PROVIDER_NAME = "vk";
        private const string TokenEndpoint = "https://oauth.vk.com/access_token";
        private const string AuthorizationEndpoint = "https://oauth.vk.com/authorize";
        private const string ApiEndpoint = "https://api.vk.com/method/";
        private string appId;
        private string clientSecret;

        public VkClient(string appId, string clientSecret)
            : base(PROVIDER_NAME)
        {
            this.appId = appId;
            this.clientSecret = clientSecret;
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            builder.AppendQueryArgument("client_id", this.appId);
            builder.AppendQueryArgument("scope", "groups, wall");
            builder.AppendQueryArgument("redirect_uri", returnUrl.AbsoluteUri);
            builder.AppendQueryArgument("display", "page");
            return builder.Uri;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var url = ApiEndpoint + "users.get";
            var builder = new UriBuilder(url);
            builder.AppendQueryArgument("access_token", accessToken);
            var userId = Convert.ToString(HttpContext.Current.Session[accessToken]);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                builder.AppendQueryArgument("uids", userId);
            }
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string data = client.DownloadString(builder.Uri);
                if (string.IsNullOrEmpty(data))
                {
                    return null;
                }

                dynamic jsonResult = JObject.Parse(data);

                //TODO: Проверять успешность запроса
                var userData = new Dictionary<string, string>();
                userData.Add("id", Convert.ToString(jsonResult.response[0].uid));
                userData.Add("username", Convert.ToString(jsonResult.response[0].uid));
                userData.Add("name", string.Format("{0} {1}",
                    Convert.ToString(jsonResult.response[0].first_name),
                    Convert.ToString(jsonResult.response[0].last_name)));
                return userData;
            }
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var builder = new UriBuilder(TokenEndpoint);
            builder.AppendQueryArgument("client_id", this.appId);
            builder.AppendQueryArgument("client_secret", this.clientSecret);
            builder.AppendQueryArgument("code", authorizationCode);
            var redirectUri = NormalizeHexEncoding(returnUrl.AbsoluteUri);
            builder.AppendQueryArgument("redirect_uri", redirectUri);

            //builder.AppendQueryArgument("grant_type", "client_credentials");

            using (WebClient client = new WebClient())
            {
                string data = client.DownloadString(builder.Uri);
                if (string.IsNullOrEmpty(data))
                {
                    return null;
                }

                JObject dataObject = JObject.Parse(data);

                //TODO: Проверять успешность запроса
                var accessToken = dataObject["access_token"].ToString();
                var userId = (int)dataObject["user_id"];

                HttpContext.Current.Session[accessToken] = Convert.ToString(userId);
                return accessToken;
            }
        }

        protected string NormalizeHexEncoding(string url)
        {
            var chars = url.ToCharArray();
            for (int i = 0; i < chars.Length - 2; i++)
            {
                if (chars[i] == '%')
                {
                    chars[i + 1] = char.ToUpperInvariant(chars[i + 1]);
                    chars[i + 2] = char.ToUpperInvariant(chars[i + 2]);
                    i += 2;
                }
            }
            return new string(chars);
        }
    }
}