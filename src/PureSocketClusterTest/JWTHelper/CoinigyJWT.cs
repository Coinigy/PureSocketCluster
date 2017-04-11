using System.Collections.Generic;
using Coinigy.SocketCluster.JWTHelper.JWT;

namespace Coinigy.SocketCluster.JWTHelper
{
    // ReSharper disable once InconsistentNaming
    public static class CoinigyJWT
    {
        public static string Decode(string token)
        {
            try
            {
                return JsonWebToken.Decode(token, "e72463241A3D438f94F17bF79u668mC6");
            }
            catch (SignatureVerificationException)
            {
                return "invalid";
            }
        }

        public static string ServerCredentials()
        {
            var jwtProperties = new Dictionary<string, string>
            {
                {"user_id", "0"},
                {"subscription_status", "coinigy_server"}
            };

            return JsonWebToken.Encode(jwtProperties, "e72463241A3D438f94F17bF79u668mC6", JwtHashAlgorithm.HS256);
        }
    }
}
