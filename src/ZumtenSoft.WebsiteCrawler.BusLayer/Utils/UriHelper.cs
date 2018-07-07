using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class UriHelper
    {
        public static Uri ToAbsolute(this Uri relativeOrAbsoluteUri, Uri baseUri)
        {
            if (relativeOrAbsoluteUri.IsAbsoluteUri)
                return relativeOrAbsoluteUri;

            Uri result;
            Uri.TryCreate(baseUri, relativeOrAbsoluteUri, out result);
            return result;
        }

        public static Uri WithoutSession(this Uri originalUri)
        {
            return originalUri == null ? null : originalUri.RemoveParameter("SID");
        }

        public static Uri RemoveParameter(this Uri originalUri, string paramName)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(originalUri.Query);
            query.Remove(paramName);
            string newPath = originalUri.AbsoluteUri.Split('?').First();
            if (query.Count > 0)
                newPath += "?" + query;

            string pathWithoutLineBreak = Regex.Replace(newPath, "(%0[0-9A-Fa-f])|[\r\n\t]", "").TrimEnd('&');
            return new Uri(pathWithoutLineBreak);
        }
    }
}
