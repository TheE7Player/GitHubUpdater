using System;
using System.Collections.Generic;
using System.Text;

namespace TheE7Player
{
    /// <summary>
    /// Class that holds the information for repositorys
    /// </summary>
    public class Repo
    {
        #region Notes
        /*
        Name of project -> "name":
        Full project name -> "full_name":
        Link (URL) -> "html_url"
        API URL -> "url"
        Project description -> "description"
        Projects language -> "language"
        Creation date -> "created_at"
        Latest update -> "updated_at"
        Latest push -> "pushed_at"

            WHAT NON-EXISTING USER JSON RESPONSES BACK:
            {
                "message": "Not Found",
                "documentation_url": "https://developer.github.com/v3/repos/#get"
            }
        
        [NOTE] Setters are set to protected so they don't show when using the library (Used for GitHubUpdater function "SubSectionJson")
        
        [CHANGES] -> Date data types are replaced for String, for simplisticity (DateTime is more powerful than Java's Date) 
        */
        #endregion

        //Parameterless constructor
        public Repo() {}

        #region Variables/Fields

        #region String
            private string project_name;
            private string project_full_name;
            private string project_url;
            private string project_apiurl;
            private string project_description;
            private string project_language;

             /// <summary>
            /// The Projects name
            /// </summary>
            public string Project_Name
            {
                get { return project_name; }
            internal set { project_name = value; }
            }
            /// <summary>
            /// The Projeccts full name ([User]/[Project])
            /// </summary>
            public string Project_Full_Name
            {
                get { return project_full_name; }
                internal set { project_full_name = value; }
            }
            /// <summary>
            /// The accessable URL to the project
            /// </summary>
            public string Project_URL
            {
                get { return project_url; }
            internal set { project_url = value; }
            }
            /// <summary>
            /// GitHub's API URL to the project
            /// </summary>
            public string Project_APIURL
            {
                get { return project_apiurl; }
            internal set { project_apiurl = value; }
            }
            /// <summary>
            /// The Projects description (If any)
            /// </summary>
            public string Project_Description
            {
                get { return project_description; }
            internal set { project_description = value; }
            }
            /// <summary>
            /// The Projects developement language
            /// </summary>
            public string Project_Language
            {
                get { return project_language; }
            internal set { project_language = value; }
            }
        #endregion

        #region Date
            private string projects_creation_date;
            private string projects_latest_update_date;
            private string projects_latest_push_date;

            /// <summary>
            /// Holds the Projects creation date (When the project went first public)
            /// </summary>
            public string Projects_Creation_Date
            {
                get { return projects_creation_date; }
            internal set { projects_creation_date = ParseDate(value); }
            }
            /// <summary>
            /// Holds the Projects latest update date on the project (Changes to readme etc)
            /// </summary>
            public string Projects_Latest_Update_Date
            {
                get { return projects_latest_update_date; }
                internal set { projects_latest_update_date = ParseDate(value); }
            }
            /// <summary>
            /// Holds the Projects latest push date (Commit push)
            /// </summary>
            public string Projects_Latest_Push_Date
            {
                get { return projects_latest_push_date; }
            internal set { projects_latest_push_date = ParseDate(value); }
            }
        #endregion

        #endregion

        #region Functions
        /// <summary>
        /// Tests all the variables to see if any are null, Returns true if all are not null
        /// </summary>
        /// <returns>True if all fields are assigned, False if one of the many isn't</returns>
        public bool SuccessfulFind()
        {
            String[] variables = new String[]
            {
                Project_Name,
                Project_Full_Name,
                Project_URL,
                Project_APIURL,
                Project_Description,
                Project_Language,
                Projects_Creation_Date,
                Projects_Latest_Push_Date,
                Projects_Latest_Update_Date
            };

            foreach (string i in variables)
            {
                if (i == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the string into a valid date format
        /// </summary>
        private string ParseDate(string input)
        {
            //This function can return null (DateTime?), so please test it when validating!
            //https://stackoverflow.com/a/3556188 <- Solution for .Net

            try { DateTime d2 = DateTime.Parse(input, null, System.Globalization.DateTimeStyles.RoundtripKind); return d2.ToShortDateString(); }
            catch (Exception) { return null; }
        }

        #endregion

    }
}
