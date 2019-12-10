//Class for Enums for GitHubUpdater

namespace TheE7Player
{
    public partial class GitHubUpdater
    {
        /*
            Github API: https://api.github.com/users/ [ USERNAME HERE ] /repos
            Get projects release API: https://api.github.com/repos/ [ USERNAME HERE ] / [ ENTER PROJECT NAME HERE ]/releases

            Get latest version: https://api.github.com/ [ USERNAME HERE ] / [ ENTER PROJECT NAME HERE ]/releases/latest

            Update tag -> "tag_name"
            Release Name -> "name"
            Draft (Boolean) -> "draft"

            Update text (IN MARKDOWN FORMAT!!!) -> "body"

            File name -> "name"
            Download file (URL) -> "browser_download_url"

            File created -> "created_at"
            File updated -> "updated_at"

        */

        /// <summary>
        /// Enum for API events (Responce feedback)
        /// </summary>
        public enum APIStatus { None, Working, Max_Fetch, Responce_Null, Match_Fail }

        /// <summary>
        /// Change the log behaviour of the program
        /// </summary>
        public enum LogTypeSettings { LogAll, LogWithError, LogStatusOnly, LogErrorOnly, LogInformationOnly }

        /// <summary>
        /// Goes along side LogTypeSettings, describes the status of the log
        /// </summary>
        public enum LogType { Error, Status, Information }
    }
}
