using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace TheE7Player
{
    public class GitHubFunctions : GitHubUpdater
    {
        private string _username;
        public string Username { get =>_username; set => _username = value; }
        public GitHubFunctions(string Username, GitHubUpdater.LogTypeSettings LogToConsole) : base(Username, LogToConsole)
        { this._username = Username;  } 
        public GitHubFunctions(string DirectSearchText, GitHubUpdater.LogTypeSettings LogToConsole, bool IsDirectURL) : base(DirectSearchText, LogToConsole, IsDirectURL) {}

        public Repo GetRepository(int Index)
        {
            try
            {
                if (Repos is null) return null;

                Repo result = Repos[Index]; return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Repo GetRepository(String name)
        {
            if (Repos is null)
                return null;

            foreach (Repo ex in Repos)
            {
                if (ex.Project_Name.Equals(name))
                    return ex;
            }

            return null;
        }

        public string[] GetRepos()
        {
            //Fetch the repos from GitHubUpdater.java
            List<Repo> lists = Repos;

            //Test if not null
            if (lists == null)
                return null;

            string[] names = new String[lists.Count];

            //We can continue
            for (int i = 0; i < lists.Count; i++)
            {
                names[i] = lists[i].Project_Name;
            }

            //Return list to array and force it into String and not Object (" new String[0] ")
            return names;
        }

        public int getAPIFetchCount()
        {
            string[] responce = GetJsonResponse("https://api.github.com/rate_limit");

            if (responce == null) return -1;

            foreach (string line in responce)
            {
                if (line.Contains("\"remaining\":"))
                {
                    try
                    {
                        String Amount = line.Substring(line.IndexOf(":") + 1);
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

        public new static string GetResetLimitDate()
        {
            return GitHubUpdater.GetResetLimitDate();
        }
    }
}
