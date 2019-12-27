using System;
using System.Collections.Generic;
using System.Text;

namespace TheE7Player
{
    public static class ProductComparer
    {
        private static string currentonlineversion;
        public static string currentOnlineVersion { get => currentOnlineVersion; private set => currentOnlineVersion = value; }

        #region Functions
        private static bool ArrayFull(string[] arr)
        {
            foreach (string i in arr) { if (i is null) { return false; } }
            return true;
        }
        private static string FixVersion(string input)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "[^\\d.]", "");

            string VersionAfterDot = input.Substring(input.IndexOf(".")).Replace(".", "");
            string VersionBeforeDot = String.Format("{0}.", input.Substring(0, input.IndexOf(".")));
            return String.Format("{0}{1}", VersionBeforeDot, VersionAfterDot);
        }

        public static int getAPIFetchCount()
        {
            string[] response = GitHubUpdater.GetJsonResponse("https://api.github.com/rate_limit");

            if (response == null) return -1;

            foreach (string line in response)
            {
                if (line.Contains("\"remaining\":"))
                {
                    try
                    {
                        string Amount = line.Substring(line.IndexOf(":") + 1);
                        int n_Amount = Convert.ToInt32(Amount);
                        return ((n_Amount - 1) > 0) ? (n_Amount - 1) : -1;
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }
            }
            return -1;
        }

        #endregion

        #region Key Functions

        public static Release[] GetUpdates(Repo Repository, bool FindReleaseFiles, int MaxReleaseSearch)
        {
            if (MaxReleaseSearch <= 1)
            {
                
                GitHubUpdater.Log("MaxReleaseSearch is set to an unreasonable size, has to be more than 1", GitHubUpdater.LogType.Error);
                return null;
            }

            if (Repository == null)
            {

                GitHubUpdater.Log("Unknown or wrong Repos connection! (Doesn't exist or wrong account lookup) ", GitHubUpdater.LogType.Error);
                return null;
            }


            GitHubUpdater.Log("Starting process of fetching releases", GitHubUpdater.LogType.Information);
            List<Release> list = new List<Release>(2);
            List<Assets> dl_Files = new List<Assets>(1);

            if (GitHubUpdater.IsSafeToSearch())
            {
                //https://api.github.com/repos/%s/%s/releases/latest
                //https://api.github.com/users/thee7player

                string[] response = GitHubUpdater.GetJsonURL(String.Format("{0}/releases", Repository.Project_APIURL));
                if (response == null) { return null; }

                string[] body = new String[7];
                GitHubUpdater.Log(String.Format("Getting list of information on releases in project: {0}", Repository.Project_Name), GitHubUpdater.LogType.Information);
                for (int i = 1; i < response.Length; i++)
                {
                    if (GitHubUpdater.CanSafelyJumpIndex((i + 1), response.Length))
                    {
                        //Check if end of JSON body
                        if (ArrayFull(body))
                        {
                            list.Add(new Release(body)); //Add the new response

                            //Add the asset file (if any)
                            if (dl_Files != null && FindReleaseFiles) //Check if not null
                                if (dl_Files.Count > 0 && FindReleaseFiles) //Then check if we have any objects in the list
                                    for (int x = 0; x < dl_Files.Count; x++)
                                        list[list.Count - 1].AddAsset(dl_Files[x]);

                            if (!list[list.Count - 1].parsedCorrectly)
                            {
                                GitHubUpdater.Log($"ERROR: Release failed in parsing! -> {String.Join(", ", body)}", GitHubUpdater.LogType.Error);
                                return null;
                            }
                            body = new String[7]; //Reset array for next release information
                            dl_Files.Clear();

                            //We've hit our max releases to find!
                            if (list.Count == MaxReleaseSearch) break;

                            continue;
                        }

                        response[i] = response[i].Trim();
                        //String result = GetValue(response[i].trim());

                        /*Array should be 7/8 in length
                           Result[0] = Name ("name")
                           Result[1] = Tag ("tag_name")
                           Result[2] = URL ("html_url")
                           Result[3] = INFO ("body")
                           Result[4] = Create Date ("created_at")
                           Result[5] = Published Date ("published_at")
                           Result[6] = PreRelease ? true/false ("prerelease")
                       */

                        //Check if line is "assets" and start to build up information
                        if (response[i].StartsWith(GitHubUpdater.GetKey("assets")) && FindReleaseFiles)
                        {
                            i++; //Jump one
                                 // itemName = "name"
                                 // itemURL = "browser_download_url"
                                 // itemSize = "size" (in Bytes)
                            string itemName = null, itemURL = null, itemSize = null;
                            while (i < response.Length)
                            {
                                response[i] = response[i].Trim();
                                if (response[i].StartsWith(GitHubUpdater.GetKey("name"))) { itemName = GitHubUpdater.GetValue(response[i]); }
                                if (response[i].StartsWith(GitHubUpdater.GetKey("size"))) { itemSize = GitHubUpdater.GetValue(response[i]); }
                                if (response[i].StartsWith(GitHubUpdater.GetKey("browser_download_url")))
                                {
                                    //We're near/are at the end of an asset object!
                                    if (response[i].Contains("}"))
                                        response[i] = response[i].Replace("}", "").Trim();

                                    if (response[i].Contains("]"))
                                        response[i] = response[i].Replace("]", "").Trim();

                                    itemURL = GitHubUpdater.GetValue(response[i]);
                                    dl_Files.Add(new Assets(itemName, itemURL, Double.Parse(itemSize)));

                                    itemName = null; itemURL = null; itemSize = null;

                                    //Asset block tends to end with the next parent being "tarball_url", using this as a get out clause (break the inner-loop)
                                    if (GitHubUpdater.CanSafelyJumpIndex((i + 1), response.Length))
                                        if (response[i + 1].StartsWith(GitHubUpdater.GetKey("tarball_url")))
                                            break;

                                }

                                //Check if it is safe to jump index by one
                                if (GitHubUpdater.CanSafelyJumpIndex((i + 1), response.Length))
                                    i++;
                                else
                                    break;
                            }
                            continue;
                        }

                        if (body[0] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("name"))) { body[0] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[1] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("tag_name"))) { body[1] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[2] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("html_url"))) { body[2] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[3] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("body"))) { body[3] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[4] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("created_at"))) { body[4] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[5] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("published_at"))) { body[5] = GitHubUpdater.GetValue(response[i]); continue; }
                        if (body[6] == null) if (response[i].StartsWith(GitHubUpdater.GetKey("prerelease"))) { body[6] = GitHubUpdater.GetValue(response[i]); continue; }

                    }
                }
            }
            GitHubUpdater.Log("Finished process of fetching releases", GitHubUpdater.LogType.Information);
            //Return the list as an array
            return list.ToArray();
        }

        public static bool CompareVersionLess(string Username, string currentVersion, Repo Repository)
        {
            //Same as CompareVersionEqual but has option to ignore updates that are lower than the latest (current) version

            if (!GitHubUpdater.IsConnected()) return false;

            if (GitHubUpdater.IsSafeToSearch())
            {
                //Compares Repository
                string url;
                string[] responce = null;
                try
                {
                    //Github API for latest Release https://api.github.com/repos/ USERNAME / PROJECT NAME /releases/latest
                    url = String.Join(String.Empty, new object[] { "https://api.github.com/repos/", Username, "/", Repository.Project_Name, "/releases/latest" });

                    //https://api.github.com/repos/ UN / PN /releases/latest
                    responce = GitHubUpdater.GetJsonURL(url);
                }
                catch (Exception)
                {
                    GitHubUpdater.Log("ERROR with fetching", GitHubUpdater.LogType.Error);
                    return false;
                }

                //tag_name <- Version
                string latestVersion = null;

                foreach (string line in responce)
                {
                    if (line.StartsWith(GitHubUpdater.GetKey("tag_name")))
                    {
                        latestVersion = GitHubUpdater.GetValue(line);
                        break;
                    }
                }

                if (latestVersion == null)
                {
                    GitHubUpdater.Log("Failed to compare versions!", GitHubUpdater.LogType.Error);
                    return false;
                }
                else
                {
                    double latestN, currentN;
                    latestN = Double.Parse(FixVersion(latestVersion));
                    currentN = Double.Parse(FixVersion(currentVersion));

                    if (currentN == latestN)
                    {
                        GitHubUpdater.Log($"{currentVersion} is that latest version available!", GitHubUpdater.LogType.Status);
                        return true;
                    }
                    else
                    {
                        if (currentN < latestN)
                        {
                            GitHubUpdater.Log($"{currentVersion} is outdated, the newest one is {latestVersion}!", GitHubUpdater.LogType.Status);
                            currentonlineversion = latestVersion;
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CompareVersionEqual(string Username, string currentVersion, Repo Repository)
        {
            if (!GitHubUpdater.IsConnected()) return false;
            if (GitHubUpdater.IsSafeToSearch())
            {
                //Compares Repository

                //Github API for latest Release https://api.github.com/repos/ USERNAME / PROJECT NAME /releases/latest
                string url = String.Join(String.Empty, new object[] { "https://api.github.com/repos/", Username, "/", Repository.Project_Name, "/releases/latest" });

                //https://api.github.com/repos/ UN / PN /releases/latest
                string[] responce = GitHubUpdater.GetJsonURL(url);
                //tag_name <- Version
                string latestVersion = null;

                foreach (string line in responce)
                {
                    if (line.StartsWith(GitHubUpdater.GetKey("tag_name")))
                    {
                        latestVersion = GitHubUpdater.GetValue(line);
                        break;
                    }
                }

                if (latestVersion == null)
                {
                    GitHubUpdater.Log("Failed to compare versions!", GitHubUpdater.LogType.Error);
                    return false;
                }
                else
                {
                    double latestN, currentN;
                    latestN = Double.Parse(FixVersion(latestVersion));
                    currentN = Double.Parse(FixVersion(currentVersion));

                    if (currentN == latestN)
                    {
                        GitHubUpdater.Log($"{currentVersion} is that latest version available!", GitHubUpdater.LogType.Status);
                        return true;
                    }
                    else
                    {

                        GitHubUpdater.Log($"{currentVersion} is outdated, the newest one is {latestVersion}!", GitHubUpdater.LogType.Status);
                        currentonlineversion = latestVersion;
                        return false;
                    }
                }
            }
            return false;
        }

        #endregion

    }
}
