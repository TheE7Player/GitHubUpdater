package TheE7Player;

import sun.rmi.runtime.Log;

import java.io.File;
import java.io.FileReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

import static TheE7Player.GitHubUpdater.*;

public class ProductComparer
{
    private static String currentOnlineVersion;

    public static String GetLatestVersion()
    {
        return (currentOnlineVersion != null) ? currentOnlineVersion : null;
    }

    //Compares the objects to see if current running version is the most resent
    public static boolean CompareVersionEqual(String Username, String currentVersion, Repo Repository)
    {
        if(!GitHubUpdater.IsConnected()) return false;
        if(GitHubUpdater.IsSafeToSearch()) {
            //Compares Repository

            //Github API for latest Release https://api.github.com/repos/ USERNAME / PROJECT NAME /releases/latest
            String url = String.format("https://api.github.com/repos/%s/%s/releases/latest", Username, Repository.getProject_Name());

            //https://api.github.com/repos/ UN / PN /releases/latest
            String[] responce = GitHubUpdater.GetJsonURL(url);
            //tag_name <- Version
            String latestVersion = null;

            for (String line : responce) {
                if (line.startsWith(GetKey("tag_name"))) {
                    latestVersion = GetValue(line);
                    break;
                }
            }
            if (latestVersion == null) {
                Log("Failed to compare versions!", GitHubUpdater.LogType.Error);
                return false;
            } else {
                if (latestVersion.equals(currentVersion)) {
                    Log(String.format("%s is that latest version available!", currentVersion), GitHubUpdater.LogType.Status);

                    return true;
                } else {
                    Log(String.format("%s is outdated, the newest one is %s!", currentVersion, latestVersion), GitHubUpdater.LogType.Status);
                    currentOnlineVersion = latestVersion;
                    return false;
                }
            }
        }
        return false;
    }

    public static boolean CompareVersionLess(String Username, String currentVersion, Repo Repository)
    {
        //Same as CompareVersionEqual but has option to ignore updates that are lower than the latest (current) version

        if(!GitHubUpdater.IsConnected()) return false;

        if(GitHubUpdater.IsSafeToSearch()) {
        //Compares Repository
        String url;
        String[] responce = null;
        try
        {
            //Github API for latest Release https://api.github.com/repos/ USERNAME / PROJECT NAME /releases/latest
            url = String.format("https://api.github.com/repos/%s/%s/releases/latest", Username, Repository.getProject_Name());

            //https://api.github.com/repos/ UN / PN /releases/latest
            responce = GitHubUpdater.GetJsonURL(url);
        }
        catch(NullPointerException e)
        {
            Log("ERROR with fetching", GitHubUpdater.LogType.Error);
            return false;
        }

        //tag_name <- Version
        String latestVersion = null;

            for (String line : responce) {
                if (line.startsWith(GetKey("tag_name"))) {
                    latestVersion = GetValue(line);
                    break;
                }
            }
            if (latestVersion == null) {
                Log("Failed to compare versions!", GitHubUpdater.LogType.Error);
                return false;
            } else {
                double latestN, currentN;
                latestN = Double.parseDouble(FixVersion(latestVersion));
                currentN = Double.parseDouble(FixVersion(currentVersion));

                if (currentN == latestN) {
                    Log(String.format("%s is that latest version available!", currentVersion), GitHubUpdater.LogType.Status);
                    return true;
                } else {
                    if (currentN < latestN) {
                        Log(String.format("%s is outdated, the newest one is %s!", currentVersion, latestVersion), GitHubUpdater.LogType.Status);
                        currentOnlineVersion = latestVersion;
                        return false;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private static String FixVersion(String input)
    {
        String VersionAfterDot = input.substring(input.indexOf(".")).replace(".", "");
        String VersionBeforeDot = String.format("%s.", input.substring(0, input.indexOf(".")));

        return String.format("%s%s", VersionBeforeDot, VersionAfterDot);
    }

    public static Release[] GetUpdates(Repo Repository, boolean FindReleaseFiles, int MaxReleaseSearch)
    {
        if(MaxReleaseSearch <= 1)
        {
            Log("MaxReleaseSearch is set to an unreasonable size, has to be more than 1", LogType.Error);
            return null;
        }

        if(Repository == null)
        {

            Log("Unknown or wrong Repos connection! (Doesn't exist or wrong account lookup) ", LogType.Error);
            return null;
        }


        Log("Starting process of fetching releases", LogType.Information);
        List<Release> list = new ArrayList<Release>(2);
        List<Assets> dl_Files = new ArrayList<Assets>(1);

        if(GitHubUpdater.IsSafeToSearch())
        {
            //https://api.github.com/repos/%s/%s/releases/latest
            //https://api.github.com/users/thee7player

            /*Local testing (Reading from file)
                String[] response;
                try
                {
                    response = Files.readAllLines(Paths.get("C:\\...\\release_Info.txt")).toArray(new String[0]);
                }
                {
                    response = null;
                }
            */

            String[] response = GetJsonURL(String.format("%s/releases", Repository.getProject_APIURL()));
            if(response == null) {return null;}

            String[] body = new String[7];
            Log(String.format("Getting list of information on releases in project: %s", Repository.getProject_Name()), LogType.Information);
            for(int i = 1; i < response.length;i++)
            {
                if (CanSafelyJumpIndex((i + 1), response.length))
                {
                    //Check if end of JSON body
                    if(ArrayFull(body))
                    {
                        list.add(new Release(body)); //Add the new response

                        //Add the asset file (if any)
                        if(dl_Files != null && FindReleaseFiles) //Check if not null
                            if(dl_Files.size() > 0 && FindReleaseFiles) //Then check if we have any objects in the list
                                for(int x = 0; x < dl_Files.size(); x++)
                                    list.get(list.size() - 1).AddAssest(dl_Files.get(x));

                        if(!list.get(list.size() - 1).parsedCorrectly)
                        {
                            String error = String.format("ERROR: Release failed in parsing! -> {%s}", String.join(", ", body));
                            Log(error, LogType.Error);
                            return null;
                        }
                        body = new String[7]; //Reset array for next release information
                        dl_Files.clear();

                        //We've hit our max releases to find!
                        if(list.size() == MaxReleaseSearch) break;

                        continue;
                    }

                    response[i] = response[i].trim();
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
                    if(response[i].startsWith(GetKey("assets")) && FindReleaseFiles)
                    {
                        i++; //Jump one
                        // itemName = "name"
                        // itemURL = "browser_download_url"
                        // itemSize = "size" (in Bytes)
                        String itemName = null, itemURL= null, itemSize = null;
                        while (i < response.length)
                        {
                            response[i] = response[i].trim();
                            if(response[i].startsWith(GetKey("name"))) {itemName = GetValue(response[i]);}
                            if(response[i].startsWith(GetKey("size"))) {itemSize = GetValue(response[i]);}
                            if(response[i].startsWith(GetKey("browser_download_url")))
                            {
                                //We're near/are at the end of an asset object!
                                if(response[i].contains("}"))
                                    response[i] = response[i].replace("}", "").trim();

                                if(response[i].contains("]"))
                                    response[i] = response[i].replace("]", "").trim();

                                itemURL = GetValue(response[i]);
                                dl_Files.add(new Assets(itemName, itemURL, Double.parseDouble(itemSize)));

                                itemName = null; itemURL = null; itemSize = null;

                                //Asset block tends to end with the next parent being "tarball_url", using this as a get out clause (break the inner-loop)
                                if (CanSafelyJumpIndex((i + 1), response.length))
                                    if(response[i + 1].startsWith(GetKey("tarball_url")))
                                        break;

                            }

                            //Check if it is safe to jump index by one
                            if (CanSafelyJumpIndex((i + 1), response.length))
                                i++;
                            else
                                break;
                        }
                        continue;
                    }

                    if(body[0] == null) if(response[i].startsWith(GetKey("name"))) { body[0] = GetValue(response[i]); continue;}
                    if(body[1] == null) if(response[i].startsWith(GetKey("tag_name"))) { body[1] = GetValue(response[i]); continue;}
                    if(body[2] == null) if(response[i].startsWith(GetKey("html_url"))) { body[2] = GetValue(response[i]);  continue;}
                    if(body[3] == null) if(response[i].startsWith(GetKey("body"))) { body[3] = GetValue(response[i]);  continue;}
                    if(body[4] == null) if(response[i].startsWith(GetKey("created_at"))) { body[4] = GetValue(response[i]); continue;}
                    if(body[5] == null) if(response[i].startsWith(GetKey("published_at"))) { body[5] = GetValue(response[i]); continue;}
                    if(body[6] == null) if(response[i].startsWith(GetKey("prerelease"))) { body[6] = GetValue(response[i]); continue;}

                }
            }
        }
        Log("Finished process of fetching releases", LogType.Information);
        //Return the list as an array
        return list.toArray(new Release[list.size()]);
    }

    private static boolean ArrayFull(String[] arr)
    {
        for(String i : arr)
        {
            if(i == null) {return false;}
        }

        return true;
    }


}
