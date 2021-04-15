namespace nowplaying_for_tidal.Data
{
    public class ConfigModel
    {
        public string DiscordAppId { get; set; } = Constants.DiscordRpcDefaultAppId;
        public string DiscordMfaToken { get; set; } = string.Empty;
    }
}
