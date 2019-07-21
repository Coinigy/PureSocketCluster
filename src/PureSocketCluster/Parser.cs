namespace PureSocketCluster
{
    internal class Parser
    {
        internal enum ParseResult
        {
            IsAuthenticated,
            Publish,
            RemoveToken,
            SetToken,
            Event,
            AckReceive
        }

        internal static ParseResult Parse(long? rid, string strEvent)
        {
            if (string.IsNullOrEmpty(strEvent))
            {
                return rid == 1 ? ParseResult.IsAuthenticated : ParseResult.AckReceive;
            }

            switch (strEvent)
            {
                case "#publish":
                    return ParseResult.Publish;
                case "#removeAuthToken":
                    return ParseResult.RemoveToken;
                case "#setAuthToken":
                    return ParseResult.SetToken;
                default:
                    return ParseResult.Event;
            }
        }
    }
}