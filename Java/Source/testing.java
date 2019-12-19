import TheE7Player.*;


import java.io.IOException;

import java.util.Date;
import java.util.List;

public class testing {

    //Testing goes here!
    public static void main(String[] args) throws Exception
    {
        System.out.println("GitHubUpdater Testing modules (Testing user: TheE7Player):");
        System.out.println("Next limit rate reset is: " + GitHubFunctions.GetResetLimitDate());
        System.out.println("Test 1: Test_GETREPOS()     ~       Testing to see if we can fetch a certain users repos");
        System.out.println("Test 2: Test_COMPAREVERISON()  ~    Testing ProductComparer class with Older, Current and Newest (Direct Comparing)");
        System.out.println("Test 3: Test_COMPARELESS()  ~       Testing ProductComparer class with Older, Current and Newest (Lower Comparing, Ignore higher versions (If any) )");
        System.out.println("Test 4: Test_GetRelease()   ~       Testing to see if we can fetch current repos releases and it's information from the API");
        System.out.println("Test 5: Test_DirectSearch()   ~     Testing DirectSearch feature in Constructor for taking less rates (1 Direct fetch, ignoring 2 fetches)");
        boolean[] tests;
        tests = new boolean[]{Test_GETREPOS(), Test_COMPAREVERISON(), Test_COMPARELESS() , Test_GetRelease(), Test_DirectSearch()};
        for (int i = 0; i < tests.length; i++) {
            System.out.println(String.format("Test %d -> %s", i + 1, tests[i]));
        }

    }

    private static boolean Test_GETREPOS() {
        try {
            GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);

            String[] Repos = udp.GetRepos();

            for (String i : Repos)
                System.out.println(String.format("%s Repos -> %s", udp.GetUserName(), i));

            return true;
        } catch (IOException e) {
            return false;
        }
    }

    private static boolean Test_COMPAREVERISON() {
        boolean OutOfDate, SameVersion, HigherVersion;

        try {
            GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);

            //Target Repo with name
            Repo target = udp.GetRepository("CSGO-Event-Viewer");
            String Update1 = "0.3";
            String Update2 = "0.4.2"; //Same version as of 10/12/19
            String Update3 = "0.7";

            //Project_APIURL <-

            OutOfDate = ProductComparer.CompareVersionEqual("thee7player", Update1, target);
            SameVersion = ProductComparer.CompareVersionEqual("thee7player", Update2, target);
            HigherVersion = ProductComparer.CompareVersionEqual("thee7player", Update3, target);

            /*EXPECTED Results:
             | OutOfDate - Should return 'false' as 0.3 < 0.4.1
             | SameVersion - Should return 'true' as 0.4.1 equals 0.4.1
             | HigherVersion - should return 'false' as 0.7 > 0.4.1
            */
            return (!OutOfDate && SameVersion && !HigherVersion);
        } catch (IOException e) {
            return false;
        }
    }

    private static boolean Test_COMPARELESS()
    {
        try
        {
            GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);
            System.out.println("FETCH COUNT IS CURRENTLY: " + udp.getAPIFetchCount());
            //Target Repo with name
            Repo target = udp.GetRepository("CSGO-Event-Viewer");
            String Update1 = "0.3";
            String Update2 = "0.4.2"; //Same version as of 30/11/19
            String Update3 = "0.7";

            //Project_APIURL <-

            boolean a = ProductComparer.CompareVersionLess("thee7player", Update1, target);
            boolean b = ProductComparer.CompareVersionLess("thee7player", Update2, target);
            boolean c = ProductComparer.CompareVersionLess("thee7player", Update3, target);
            System.out.println("FETCH COUNT AFTER !!!!!!!!!!!!!!!!! : " + udp.getAPIFetchCount());
            return (!a && b && c);
        }
        catch(IOException e)
        {
            return false;
        }
    }

    private static boolean Test_GetRelease()
    {
        try {
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

            boolean findFiles = true;
            Release[] r = ProductComparer.GetUpdates(udp.GetRepository("CSGO-Event-Viewer"), findFiles, 2);
            List<Assets> _files = null;
            System.out.println(" -- INFORMATION FROM RELEASES -- ");
            for (Release a : r)
            {
                _files = a.getAssets(); //<- Get files from each release

                System.out.println(String.format("Release: %s",a.getName()));
                System.out.println(String.format("└─ Tag: %s",a.getTag()));
                System.out.println(String.format("└─ Url: %s",a.getURL()));
                System.out.println(String.format("└─ Prerelease: %s",a.getIsPreRelease()));

                System.out.println("└─ Dates:");
                System.out.println(String.format("     └─ Created Date (When drafted) : %s",a.getCreatedDate()));
                System.out.println(String.format("     └─ Published Date (When visible to public): %s",a.getPublishedDate()));

                System.out.println(String.format("└─ Downloadable files (%d): ", _files.size()));
                for(Assets x : _files)
                    System.out.println(String.format("     └─ File %s : %s (%s) -> %s", x.getItemName(), x.getItemSize(Assets.DisplaySize.Megabytes), x.getItemSize(Assets.DisplaySize.Kilobytes), x.getItemURL()));
            }
            System.out.println(" -- INFORMATION FROM RELEASES -- ");

            return (findFiles) ? r != null && _files != null : r != null;
        }
        catch(Exception e)
        {
            return false;
        }
    }

    private static boolean Test_DirectSearch()
    {
        try
        {
            GitHubFunctions udp = new GitHubFunctions("repos/TheE7Player/CSGO-Event-Viewer", GitHubUpdater.LogTypeSettings.LogWithError, true);

            //Since DirectSearch targets one project, use .GetRepostiory(0);
            Repo Repos = udp.GetRepository(0);

            if(Repos != null) //Ensure it isn't null before fetching information
            {
                String project_APILink = Repos.getProject_APIURL();
                String project_Name = Repos.getProject_Name();
                Date project_LastUpdate = Repos.getProjects_Latest_Update_Date(); //Requires: import java.util.Date;

                System.out.println(String.format("Repo Name: %s\r\nRepo API Link: %s\r\nRepo Date (Updated): %s", project_Name, project_APILink, project_LastUpdate.toString()));
            }
            return Repos != null;
        } catch (IOException e) {
            return false;
        }
    }

}
