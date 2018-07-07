using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;

namespace ZumtenSoft.WebsiteCrawler.Utils.Helpers
{
    public static class DescriptionExtractor
    {
        private static readonly Dictionary<object, string> EnumValues = new Dictionary<object, string>
        {
            { HttpStatusCode.Continue, "100 Continue" },
            { HttpStatusCode.SwitchingProtocols, "101 SwitchingProtocols" },
            { HttpStatusCode.OK, "200 OK" },
            { HttpStatusCode.Created, "201 Created" },
            { HttpStatusCode.Accepted, "202 Accepted" },
            { HttpStatusCode.NonAuthoritativeInformation, "203 NonAuthoritativeInformation" },
            { HttpStatusCode.NoContent, "204 NoContent" },
            { HttpStatusCode.ResetContent, "205 ResetContent" },
            { HttpStatusCode.PartialContent, "206 PartialContent" },
            { HttpStatusCode.Ambiguous, "300 Ambiguous" },
            { HttpStatusCode.MovedPermanently, "301 MovedPermanently" },
            { HttpStatusCode.Redirect, "302 Redirect" },
            { HttpStatusCode.SeeOther, "303 SeeOther" },
            { HttpStatusCode.NotModified, "304 NotModified" },
            { HttpStatusCode.UseProxy, "305 UseProxy" },
            { HttpStatusCode.Unused, "306 Unused" },
            { HttpStatusCode.TemporaryRedirect, "307 TemporaryRedirect" },
            { HttpStatusCode.BadRequest, "400 BadRequest" },
            { HttpStatusCode.Unauthorized, "401 Unauthorized" },
            { HttpStatusCode.PaymentRequired, "402 PaymentRequired" },
            { HttpStatusCode.Forbidden, "403 Forbidden" },
            { HttpStatusCode.NotFound, "404 NotFound" },
            { HttpStatusCode.MethodNotAllowed, "405 MethodNotAllowed" },
            { HttpStatusCode.NotAcceptable, "406 NotAcceptable" },
            { HttpStatusCode.ProxyAuthenticationRequired, "407 ProxyAuthenticationRequired" },
            { HttpStatusCode.RequestTimeout, "408 RequestTimeout" },
            { HttpStatusCode.Conflict, "409 Conflict" },
            { HttpStatusCode.Gone, "410 Gone" },
            { HttpStatusCode.LengthRequired, "411 LengthRequired" },
            { HttpStatusCode.PreconditionFailed, "412 PreconditionFailed" },
            { HttpStatusCode.RequestEntityTooLarge, "413 RequestEntityTooLarge" },
            { HttpStatusCode.RequestUriTooLong, "414 RequestUriTooLong" },
            { HttpStatusCode.UnsupportedMediaType, "415 UnsupportedMediaType" },
            { HttpStatusCode.RequestedRangeNotSatisfiable, "416 RequestedRangeNotSatisfiable" },
            { HttpStatusCode.ExpectationFailed, "417 ExpectationFailed" },
            { HttpStatusCode.InternalServerError, "500 InternalServerError" },
            { HttpStatusCode.NotImplemented, "501 NotImplemented" }, 
            { HttpStatusCode.BadGateway, "502 BadGateway" },
            { HttpStatusCode.ServiceUnavailable, "503 ServiceUnavailable" },
            { HttpStatusCode.GatewayTimeout, "504 GatewayTimeout" },
            { HttpStatusCode.HttpVersionNotSupported, "505 HttpVersionNotSupported" },
        };

        private static readonly Dictionary<MemberInfo, string> ValuesByMember = new Dictionary<MemberInfo, string>();

        public static string GetDescription(object value)
        {
            string description;
            if (!EnumValues.TryGetValue(value, out description))
            {
                FieldInfo field = value.GetType().GetField(value.ToString());
                description = field != null ? GetDescription(field) : null;
                EnumValues.Add(value, description);
            }
            return description;
        }

        public static string GetDescription(MemberInfo info)
        {
            string result;
            if (!ValuesByMember.TryGetValue(info, out result))
            {
                var descriptionAttribute = info
                  .GetCustomAttributes(typeof(DescriptionAttribute), false)
                  .FirstOrDefault() as DescriptionAttribute;

                result = descriptionAttribute != null
                  ? descriptionAttribute.Description
                  : info.Name;

                ValuesByMember.Add(info, result);
            }

            return result;
        }
    }
}
