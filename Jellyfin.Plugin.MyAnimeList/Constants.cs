namespace Jellyfin.Plugin.MyAnimeList
{
    /// <summary>
    /// Class for Project Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The Name of the Plugin.
        /// </summary>
        public const string PluginName = "MyAnimeList";

        /// <summary>
        /// The GUID of the Plugin.
        /// </summary>
        public const string PluginGuid = "41b9d545-325d-4f3e-b712-085b3b7f1b0c";

        /// <summary>
        /// The Regex to identify the MyAnimeList ID in the title.
        /// </summary>
        public const string MalIdRegexPattern = @"\[mal-([0-9]+)\]$";
    }
}