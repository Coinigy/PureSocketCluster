namespace PureSocketCluster
{
    internal class Parser
    {
        internal enum ParseResult
        {
            ISAUTHENTICATED,
            PUBLISH,
            REMOVETOKEN,
            SETTOKEN,
            EVENT,
            ACKRECEIVE
        }

        internal static ParseResult Parse(long? rid, string strEvent)
        {
            if (string.IsNullOrEmpty(strEvent)) return rid == 1 ? ParseResult.ISAUTHENTICATED : ParseResult.ACKRECEIVE;
            switch (strEvent)
            {
                case "#publish":
                    return ParseResult.PUBLISH;
                case "#removeAuthToken":
                    return ParseResult.REMOVETOKEN;
                case "#setAuthToken":
                    return ParseResult.SETTOKEN;
                default:
                    return ParseResult.EVENT;
            }
        }
    }
}