using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheE7Player;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GitHubUpdater Testing modules (Testing user: TheE7Player):");
            Console.WriteLine("Next limit rate reset is: " + GitHubFunctions.GetResetLimitDate());
            Console.WriteLine("Test 1: Test_GETREPOS()     ~       Testing to see if we can fetch a certain users repos");
            Console.WriteLine("Test 2: Test_COMPAREVERISON()  ~    Testing ProductComparer class with Older, Current and Newest (Direct Comparing)");
            Console.WriteLine("Test 3: Test_COMPARELESS()  ~       Testing ProductComparer class with Older, Current and Newest (Lower Comparing, Ignore higher versions (If any) )");
            Console.WriteLine("Test 4: Test_GetRelease()   ~       Testing to see if we can fetch current repos releases and it's information from the API");
            Console.WriteLine("Test 5: Test_DirectSearch()   ~     Testing DirectSearch feature in Constructor for taking less rates (1 Direct fetch, ignoring 2 fetches)");
            bool[] tests;
            tests = new bool[] { Test_GETREPOS(), Test_COMPAREVERISON(), Test_COMPARELESS(), Test_GetRelease(), Test_DirectSearch() };

            for (int i = 0; i < tests.Length; i++)
                Console.WriteLine($"Test {i + 1} -> {tests[i]}");

            Console.ReadLine();
        }

        #region Testing here
        public static bool Test_GETREPOS()
        {
            try
            {
                GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);

                string[] Repos = udp.GetRepos();

                if (Repos is null) return false;

                foreach (string i in Repos)
                    Console.WriteLine(String.Format("{0} Repos -> {1}", udp.Username, i));

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool Test_COMPAREVERISON()
        {
            bool OutOfDate, SameVersion, HigherVersion;
            try
            {
                GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);

                //Target Repo with name
                Repo target = udp.GetRepository("CSGO-Event-Viewer");

                if (target is null) return false;

                string Update1 = "0.3";
                string Update2 = "0.4.2"; //Same version as of 10/12/19
                string Update3 = "0.7";

                OutOfDate = ProductComparer.CompareVersionEqual("thee7player", Update1, target);
                SameVersion = ProductComparer.CompareVersionEqual("thee7player", Update2, target);
                HigherVersion = ProductComparer.CompareVersionEqual("thee7player", Update3, target);

                /*EXPECTED Results:
                 | OutOfDate - Should return 'false' as 0.3 < 0.4.1
                 | SameVersion - Should return 'true' as 0.4.1 equals 0.4.1
                 | HigherVersion - should return 'false' as 0.7 > 0.4.1
                */
                return (!OutOfDate && SameVersion && !HigherVersion);

            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_COMPARELESS()
        {
            try
            {
                GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);
                Console.WriteLine("FETCH COUNT IS CURRENTLY: " + udp.getAPIFetchCount());
                //Target Repo with name
                Repo target = udp.GetRepository("CSGO-Event-Viewer");
                string Update1 = "0.3";
                string Update2 = "0.4.2"; //Same version as of 30/11/19
                string Update3 = "0.7";
                if (target is null) return false;
                //Project_APIURL <-

                bool a = ProductComparer.CompareVersionLess("thee7player", Update1, target);
                bool b = ProductComparer.CompareVersionLess("thee7player", Update2, target);
                bool c = ProductComparer.CompareVersionLess("thee7player", Update3, target);
                Console.WriteLine("FETCH COUNT AFTER !!!!!!!!!!!!!!!!! : " + udp.getAPIFetchCount());
                return (!a && b && c);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool Test_GetRelease()
        {
            try
            {
                GitHubFunctions udp = new GitHubFunctions("TheE7Player", GitHubUpdater.LogTypeSettings.LogStatusOnly);

                /*Bypass the waiting process (FOR TESTING)
                Repo t = new Repo();
                t.setProject_Name("CSGO-Event-Viewer");
                t.setProject_Full_Name ( "TheE7Player/CSGO-Event-Viewer");
                t.setProject_URL ( "https://github.com/TheE7Player/CSGO-Event-Viewer");
                t.setProject_APIURL ( "https://api.github.com/repos/TheE7Player/CSGO-Event-Viewer");
                t.setProject_Description ("A Java project to allow users to see what events you can use with logic_eventlistener");
                t.setProject_Language ("Java");
                t.setProjects_Creation_Date ("2019-11-16T19:00:17Z");
                t.setProjects_Latest_Update_Date ("2019-11-30T16:23:40Z");
                t.setProjects_Latest_Push_Date ("2019-11-30T16:23:38Z"); */

                bool findFiles = true;
                Release[] r = ProductComparer.GetUpdates(udp.GetRepository("CSGO-Event-Viewer"), findFiles, 2);
                List<Assets> _files = null;
                Console.WriteLine(" -- INFORMATION FROM RELEASES -- ");

                if (r is null)
                    return false;

                foreach (Release a in r)
                {
                    _files = a.Assets; //<- Get files from each release

                    Console.WriteLine($"Release: {a.Name}");
                    Console.WriteLine($"└─ Tag: {a.Tag }");
                    Console.WriteLine($"└─ Url: {a.URL}");
                    Console.WriteLine($"└─ Prerelease: {a.isPreRelease }");

                    Console.WriteLine("└─ Dates:");
                    Console.WriteLine($"     └─ Created Date (When drafted) : { a.Created}");
                    Console.WriteLine($"     └─ Published Date (When visible to public): { a.Published }");

                    Console.WriteLine($"└─ Downloadable files {_files.Count}: ");
                    foreach (Assets x in _files)
                        Console.WriteLine($"     └─ File { x.itemName } : { x.getItemSize(Assets.DisplaySize.Megabytes) } ({x.getItemSize(Assets.DisplaySize.Kilobytes) }) -> { x.itemDownloadUrl}");
                }
                Console.WriteLine(" -- INFORMATION FROM RELEASES -- ");

                return (findFiles) ? r != null && _files != null : r != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_DirectSearch()
        {
            try
            {
                GitHubFunctions udp = new GitHubFunctions("repos/TheE7Player/CSGO-Event-Viewer", GitHubUpdater.LogTypeSettings.LogWithError, true);

                //Since DirectSearch targets one project, use .GetRepostiory(0);
                Repo Repos = udp.GetRepository(0);

                if (Repos != null) //Ensure it isn't null before fetching information
                {
                    string project_APILink = Repos.Project_APIURL;
                    string project_Name = Repos.Project_Name;
                    string project_LastUpdate = Repos.Projects_Latest_Update_Date; //Requires: import java.util.Date;

                    Console.WriteLine($"Repo Name: {project_Name}\r\nRepo API Link: {project_APILink}\r\nRepo Date (Updated): {project_LastUpdate}");
                }
                return Repos != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

    }
}
