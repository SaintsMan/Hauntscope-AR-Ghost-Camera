namespace Hauntscope.Gameplay.Promo
{
    // The server's error codes (LiveOps/Promo/CLIENT.md) plus the two the game finds on its own.
    public static class PromoErrors
    {
        public const string NotFound = "not_found";
        public const string OtherGame = "game";
        public const string Used = "used";
        public const string Inactive = "inactive";
        public const string Expired = "expired";
        public const string NotStarted = "not_started";
        public const string Limit = "limit";
        public const string Rate = "rate";
        public const string BadRequest = "bad_request";
        public const string Server = "server";
        // Found by the game: no network at all, or the request failed or timed out.
        public const string Offline = "offline";
        public const string Failed = "failed";
    }
}
