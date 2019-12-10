using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

namespace TheE7Player
{
    //[Note]: "partial" means this class can be expanded from multiple different files

    //Look at GitHubUpdater/GHU-Enums,... to see how pratical and nice this keyword is...

    /// <summary>
    /// Root class to handle the information/functionality
    /// </summary>
    public partial class GitHubUpdater
    {
        #region Notes
        /*  https://api.github.com/rate_limit <- GitHub

           "resources": {
           "core": {
           "limit": 60, <- Max fetches within an Hour?
           "remaining": 59, <- Current fetches left
           "reset": 1575308603 <- Assuming reset timescale (How long to wait till resets back to 60)?
        },
        */
        #endregion

        #region Variables/Fields
        private static bool _LogVersion = true;
        private static bool LogVersionShown = false;
        private static DateTime NextWindowReset;
        protected static string Version = "0.1";
        protected static bool DirectSearch = false; //<- Important: Encase they use a feature which requires repo to be initialized. For error tracing, good to know what the last API status was.
        protected static APIStatus lastAPIStatus = APIStatus.None;
        private static LogTypeSettings printStatus = LogTypeSettings.LogWithError;
        private static bool messageShown = false;
        private static bool Connected = false;
        private string username;
        protected List<Repo> Repos;
        #endregion

        #region Constructors
        public GitHubUpdater(string Username, LogTypeSettings LogToConsole)
        {
            LogVersion(); printStatus = LogToConsole; Start(Username);
        }
        public GitHubUpdater(string DirectSearchText, LogTypeSettings LogToConsole, bool IsDirectURL) { LogVersion(); DirectSearch = IsDirectURL; printStatus = LogToConsole; Start(DirectSearchText); }
        #endregion

        #region Key Functions

        private List<Repo> SubSectionJson(string[] lines)
        {
            List<Repo> f = new List<Repo>();
            Repo r = new Repo();
           
            string result;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("["))
                    lines[i] = lines[i].Replace('[', ' ').Trim();

                //Check if start of json (starts with first attribute "id")
                if (lines[i].StartsWith("{\"id\""))
                {
                    //Jump one
                    i++;

                    //Iterate within
                    while (i < lines.Length)
                    {
                        if (CanSafelyJumpIndex((i + 1), lines.Length))
                            if (lines[i].EndsWith("}") && lines[i + 1].StartsWith("{\"id\"")) { break; } //Jump out of loop, reached end

                        result = GetValue(lines[i]);

                        if (lines[i].StartsWith("\"owner\""))
                        {
                            i += 18;
                            continue;
                        }

                        if (CanSafelyJumpIndex((i + 1), lines.Length))
                            if (result == null && lines[i + 1].StartsWith("{\"id\""))
                            {
                                break;
                            }

                        if (r.Project_Name == null) if (lines[i].StartsWith(GetKey(("name")))) { r.Project_Name = result; i++; continue; } //If Json key is "name" set it's value

                        if (r.Project_Full_Name == null) if (lines[i].StartsWith(GetKey("full_name"))) { r.Project_Full_Name = result; i++; continue; }

                        if (r.Project_URL == null) if (lines[i].StartsWith(GetKey("html_url"))) { r.Project_URL = result; i++; continue; }

                        if (r.Project_APIURL == null) if (lines[i].StartsWith(GetKey("url"))) { r.Project_APIURL = result; i++; continue; }

                        if (r.Project_Description == null) if (lines[i].StartsWith(GetKey("description"))) { r.Project_Description = result; i++; continue; }

                        if (r.Project_Language == null) if (lines[i].StartsWith(GetKey("language"))) { r.Project_Language = result; i++; continue; }

                        if (r.Projects_Creation_Date == null) if (lines[i].StartsWith(GetKey("created_at"))) { r.Projects_Creation_Date = result; i++; continue; }

                        if (r.Projects_Latest_Update_Date == null) if (lines[i].StartsWith(GetKey("updated_at"))) { r.Projects_Latest_Update_Date = result; i++; continue; }

                        if (r.Projects_Latest_Push_Date == null) if (lines[i].StartsWith(GetKey("pushed_at"))) { r.Projects_Latest_Push_Date = result; i++; continue; }

                        i++;
                    }

                    if (!r.SuccessfulFind())
                    {
                        Log("ERROR - One or more fields failed to be assigned!", LogType.Error);
                        break;
                    }
                    else
                    {
                        Log($"Success! \"{r.Project_Full_Name}\" has been found and assigned to!", LogType.Information);
                        f.Add(r);
                    }

                    //Reset for next repo
                    r = new Repo();
                }
            }
            return f;
        }

        internal static string[] GetJsonResponse(string url)
        {
            string[] result = null;
            WebClient webClient = null;
            try
            {
                //Put the responce from the API into result
                using (webClient = new WebClient())
                {
                    try
                    {
                        webClient.Headers.Add("User-Agent: Other");
                        string responce = webClient.DownloadString(url);
                        result = responce.Split(',');
                        return result;

                    }
                    catch (WebException ex)
                    {
                        Log($"ERROR: Unknown link to API -> {ex.Message}", LogType.Error);
                    }
                }
            }          
            catch(Exception e)
            {
                if (e.Message.Contains("HTTP response code: 403 for URL"))
                {
                    Log("FAULT: Max connections made, Please try again later!", LogType.Error);
                }
                else
                {
                    Log($"ERROR: Problem with handling -> {e.Message}", LogType.Error);
                }
            }
            finally
            {
                //Clean up webClient after usage (GOOD PRACTICE)
                if (!Object.Equals(webClient, null))
                {
                    //This means that webClient hasn't be cleared, so we'll manually tell it to
                    webClient.Dispose();
                }
            }              
            return null;
        }

        internal static string[] GetJsonURL(string url)
        {
            try
            {
                APIStatus result = CanDoSearch();
                if (result != APIStatus.Working)
                {
                    CallMaxFetchMessage(result);
                    throw new Exception("MAX API COUNT HIT");
                }

                lastAPIStatus = result;

                bool site = ValidURL(url);

                if (!site)
                    return null;

                return GetJsonResponse(url);
            }
            catch (Exception ee)
            {
                if (ee.Message != "MAX API COUNT HIT")
                    Console.WriteLine("[ERROR]: " + ee.Message);
            }
            return null;
        }

        internal static bool ValidURL(string url)
        {
            try
            {
                Log("STEP 1: ANALYZE INTERNET SIGNAL", LogType.Information);

                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent: Other");
                wc.DownloadString(url);
                Log("SUCCESS, PROCEEDING TO CONNECT TO API", LogType.Information);
                Connected = true;
                return true;
            }
            catch (Exception e)
            {
                Log("ERROR - NO CONNECTION", LogType.Error);
                Connected = false;
                return false;
            }
        }

        #endregion

        #region Methods
        ///// <summary>
        ///// Logs the librarys version on first time launch
        ///// </summary>
        //private static void _LogVersion()
        //{
        //    if (_LogVersion && !LogVersionShown)
        //    {
        //        Console.WriteLine($"RUNNING GitHubUpdater (JAVA) version: %{Version}");
        //        LogVersionShown = true;
        //    }
        //}

        /// <summary>
        /// For any errors processing the json
        /// </summary>
        /// <param name="result"></param>
        private static void CallMaxFetchMessage(APIStatus result)
        {
            lastAPIStatus = result;
            if (!messageShown)
            {
                if (result == APIStatus.Max_Fetch)
                {
                    string newTime = NextWindowReset.ToString();
                    Log($"Cannot call API as max fetches are used (60 max per Hour) Rate limit resets back at: {newTime}", LogType.Status);
                }
                else if (result == APIStatus.Responce_Null)
                {
                    Log("Issue with feedback from response to API, connection may be down or problem with link", LogType.Error);
                }
                else if (result == APIStatus.Match_Fail)
                {
                    Log("Cannot find any matches with the given feedback from API (Failure in finding correct keys)", LogType.Error);
                }

                messageShown = true;
            }
        }

        private static void LogVersion()
        {
            if (_LogVersion && !LogVersionShown)
            {
                Console.WriteLine($"RUNNING GitHubUpdater (C#) version: {Version}");
                LogVersionShown = true;
            }

        }

        internal static bool IsSafeToSearch() { return lastAPIStatus == APIStatus.Working; }

        /// <summary>
        /// Logs messages from the classes and allows prioritisation for messages
        /// </summary>
        /// <param name="message">The message to dispaly to console</param>
        /// <param name="mType">Message type (Is it an error, information etc?)</param>
        internal static void Log(String message, LogType mType)
        {
            //Print the message right away, no restrictions
            if (printStatus == LogTypeSettings.LogAll)
            { Console.WriteLine(message); return; }

            //Check if printStatus is "LogWithError" and check if the message type is "Error" or "Status"
            if (printStatus == LogTypeSettings.LogWithError && mType == LogType.Error || mType == LogType.Status)
            { Console.WriteLine(message); return; }


            //Check if printStatus is "LogStatusOnly" and check if the message type is "Status"
            if (printStatus == LogTypeSettings.LogStatusOnly && mType == LogType.Status)
            { Console.WriteLine(message); return; }

            //Check if printStatus is "LogErrorOnly" and check if the message type is "Error"
            if (printStatus == LogTypeSettings.LogErrorOnly && mType == LogType.Error)
            { Console.WriteLine(message); return; }

            //Check if printStatus is "LogInformationOnly" and check if the message type is "Information"
            if (printStatus == LogTypeSettings.LogInformationOnly && mType == LogType.Information)
            { Console.WriteLine(message); return; }
        }

        private void Start(string Username)
        {
            this.username = (!DirectSearch) ? Username : Username.Substring(0, Username.IndexOf('/'));

            try
            {
                if (DirectSearch)
                    Direct_Init(Username);
                else
                    Init();
            }
            catch (Exception unH)
            {
                Log($"ERROR - NO INTERNET CONNECTION OR HOST IS DOWN -> {unH.Message}", LogType.Error);
            }
        }

        private void Init()
        {
            string user = GetAPIReposLink();

            Log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information);

            string[] Lines = GetJsonURL(user);

            Log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information);

            if (Lines is null)
            {
                Log("Error parsing or getting URL for parsing", LogType.Error);
                return;
            }

            //Get all the data and fling it into Repos
            Repos = SubSectionJson(Lines);
        }

        private void Direct_Init(string Username)
        {
            //https://docs.oracle.com/javase/tutorial/networking/urls/readingURL.html
            //String user = GetAPIReposLink();
            string directLink = String.Join(String.Empty, new object[] { "https://api.github.com/", Username });

            Log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information);

            string[] Lines = GetJsonURL(directLink);

            Log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information);

            if (Lines is null)
            {
                Log("Error parsing or getting URL for parsing", LogType.Error);
                return;
            }

            //Get all the data and fling it into Repos
            Repos = SubSectionJson(Lines);
        }
        #endregion

        #region Functions
      
        internal static string GetValue(string line)
        {
            //  [  \"\w+\":(\"?.+\"?)  ] <- Regex
            Regex pattern = new Regex("\"\\w+\":(\"?.+\"?)");
            Match matcher = pattern.Match(line);

            if (!matcher.Success) return String.Empty;
            else
            {
                string repond = matcher.Groups[1].Value;

                if (repond.Contains("\""))
                    repond = repond.Replace("\"", "").Trim();

                if (repond.Contains(","))
                    repond = repond.Replace(",", "").Trim();

                return repond;
            }
        }

        internal static bool CanSafelyJumpIndex(int NewIndex, int ArraySize)
        {
            return NewIndex < ArraySize;
        }

        internal static string GetKey(string key)
        {
            return $"\"{key}\"";
        }

        internal string GetAPIReposLink()
        {
            return String.Join("", new object[] { "https://api.github.com/users/", this.username, "/repos" });
        }

        internal static bool IsConnected() { return Connected; }

        internal static APIStatus CanDoSearch()
        {
            string[] responce = GetJsonResponse("https://api.github.com/rate_limit");

            if (responce is null) return APIStatus.Responce_Null;

            foreach (string line in responce)
            {
                if (line.Contains("\"remaining\":"))
                {
                    try
                    {
                        string Amount = line.Substring(line.IndexOf(":") + 1);
                        int n_Amount = Convert.ToInt32(Amount);
                        int n_AmountNew = ((n_Amount - 1) > 0) ? (n_Amount - 1) : 0;
                        string resetDate = responce[2].Substring(responce[2].IndexOf(":") + 1).Replace("}", "");

                        //Converting UTC to current time: https://stackoverflow.com/a/7844741
                        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        NextWindowReset = epoch.AddSeconds(long.Parse(resetDate));

                        Log($"API COUNT FROM {n_Amount} to {n_AmountNew}", LogType.Information);
                        if ((n_Amount - 1) > 0)
                            return APIStatus.Working;
                        else
                            return APIStatus.Max_Fetch;


                    }
                    catch (Exception SIE)
                    {
                        return APIStatus.Match_Fail;
                    }
                }
            }
            return APIStatus.Match_Fail;
        }

        internal static string GetResetLimitDate()
        {
            if (NextWindowReset == null)
                CanDoSearch();

            return NextWindowReset.ToString();
        }

        #endregion
    }
}
