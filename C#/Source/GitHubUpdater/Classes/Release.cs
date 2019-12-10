using System;
using System.Collections.Generic;
using System.Text;

namespace TheE7Player
{
    public class Release
    {
        #region Variables/Fields
        private string r_name, r_tag, r_url, r_info, r_created, r_published = String.Empty;
        private bool parsedcorrectly, r_isprerelease = false;
        private List<Assets> r_Files;

        public string Name { get => r_name; private set => r_name = value; }
        public string Tag { get => r_tag; private set => r_tag = value; }
        public string URL { get => r_url; private set => r_url = value; }
        public string Info { get => r_info; private set => r_info = value; }
        public string Created { get => r_created; private set => r_created = value; }
        public string Published { get => r_published; private set => r_published = value; }
        public bool isPreRelease { get => r_isprerelease; private set => r_isprerelease = value; }
        public bool parsedCorrectly { get => parsedcorrectly; private set => parsedcorrectly = value; }
        public List<Assets> Assets { get => r_Files; private set => r_Files = value; }

        public Release(string[] Result)
        {
            /*
                Array should be 7/8 in length
                Result[0] = Name ("name")
                Result[1] = Tag ("tag_name")
                Result[2] = URL ("html_url")
                Result[3] = INFO ("body")
                Result[4] = Create Date ("created_at")
                Result[5] = Published Date ("published_at")
                Result[6] = PreRelease ? true/false ("prerelease")
            */

            if (Result.Length != 7)
                return;

            try
            {
                this.Name = Result[0];
                this.Tag = Result[1];
                this.URL = Result[2];
                this.Info = Result[3];

                this.Created = ParseDate(Result[4]);
                this.Published = ParseDate(Result[5]);

                this.isPreRelease = (Result[6] == "true" || Result[6] == "True") ? true : false;

                Object[] test = { Name, Tag, URL, Info, Created, Published, isPreRelease };
                foreach (Object t in test)
                {
                    if (t is null)
                    {
                        parsedCorrectly = false;
                        return;
                    }
                }
                parsedCorrectly = true;
            }
            catch (Exception e)
            {
                GitHubUpdater.Log($"PARSING FAILED: {e.Message}", GitHubUpdater.LogType.Error);
                parsedCorrectly = false;
            }
        }

        #endregion

        #region Functions
        private string ParseDate(string input)
        {
            //This function can return null (DateTime?), so please test it when validating!
            //https://stackoverflow.com/a/3556188 <- Solution for .Net

            try { DateTime d2 = DateTime.Parse(input, null, System.Globalization.DateTimeStyles.RoundtripKind); return d2.ToShortDateString(); }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Adds an Asset
        /// </summary>
        /// <param name="assets"></param>
        public void AddAsset(Assets assets)
        {
            if (r_Files == null)
                r_Files = new List<Assets>(1);

            if (r_Files.Contains(assets))
            { GitHubUpdater.Log($"[INFO] Ignoring {assets.itemName} as it is already included into the list", GitHubUpdater.LogType.Information); return; }
            else
                r_Files.Add(assets);
        }
        #endregion
    }
}
