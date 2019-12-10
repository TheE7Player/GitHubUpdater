package TheE7Player;

import java.io.*;
import java.net.*;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class GitHubUpdater
{
    /*  https://api.github.com/rate_limit <- GitHub

       "resources": {
       "core": {
       "limit": 60, <- Max fetches within an Hour?
       "remaining": 59, <- Current fetches left
       "reset": 1575308603 <- Assuming reset timescale (How long to wait till resets back to 60)?
    },
    */
    private static boolean LogVersion = true;
    private static boolean LogVersionShown = false;
    private static Date NextWindowReset = null;

    protected static String Version = "0.1";

    private static void LogVersion()
    {
        if(LogVersion && !LogVersionShown)
        {
            System.out.println(String.format("RUNNING GitHubUpdater (JAVA) version: %s", Version));
            LogVersionShown = true;
        }

    }

    protected static boolean DirectSearch = false; //<- Important: Encase they use a feature which requires repo to be initialized
    //For error tracing, good to know what the last API status was
    protected static APIStatus lastAPIStatus = APIStatus.None;
    protected static boolean IsSafeToSearch()
    {
        return lastAPIStatus == APIStatus.Working;
    }

    protected static String GetResetLimitDate()
    {
        if(NextWindowReset == null)
            CanDoSearch();

        return NextWindowReset.toString();
    }

    public enum APIStatus
    {
        None,
        Working,
        Max_Fetch,
        Responce_Null,
        Match_Fail
    }

    protected static APIStatus CanDoSearch()
    {
        String[] responce = GetJsonResponse("https://api.github.com/rate_limit");

        if(responce == null) return APIStatus.Responce_Null;

        for(String line : responce)
        {
            if(line.contains("\"remaining\":"))
            {
                try
                {
                    String Amount = line.substring(line.indexOf(":")+1);
                    int n_Amount = Integer.parseInt(Amount);
                    int n_AmountNew = ( (n_Amount - 1 ) > 0) ? (n_Amount - 1 ) : 0;
                    String resetDate = responce[2].substring(responce[2].indexOf(":")+1).replace("}", "");
                    NextWindowReset = new Date(Long.parseLong(resetDate) * 1000 );

                    Log("API COUNT FROM " + n_Amount + " to " + n_AmountNew, LogType.Information);
                    if((n_Amount - 1) > 0)
                        return APIStatus.Working;
                    else
                        return APIStatus.Max_Fetch;


                }
                catch(StringIndexOutOfBoundsException SIE)
                {
                    return APIStatus.Match_Fail;
                }
            }
        }
        return APIStatus.Match_Fail;
    }

    public int getAPIFetchCount()
    {
        String[] responce = GetJsonResponse("https://api.github.com/rate_limit");

        if(responce == null) return -1;

        for(String line : responce)
        {
            if(line.contains("\"remaining\":"))
            {
                try
                {
                    String Amount = line.substring(line.indexOf(":") + 1);
                    int n_Amount = Integer.parseInt(Amount);
                    return ( (n_Amount - 1 ) > 0) ? (n_Amount - 1 ) : -1;
                }
                catch(Exception e)
                {
                    return -1;
                }
            }
        }
        return -1;
    }

    private static boolean messageShown = false;
    private static void CallMaxFetchMessage(APIStatus result)
    {
        lastAPIStatus = result;
        if(!messageShown)
        {
            if(result == APIStatus.Max_Fetch)
            {
                String newTime = NextWindowReset.toString();
                Log("Cannot call API as max fetches are used (60 max per Hour) Rate limit resets back at: " + newTime, LogType.Status);
            }
            else if(result == APIStatus.Responce_Null)
            {
                Log("Issue with feedback from response to API, connection may be down or problem with link", LogType.Error);
            }
            else if(result == APIStatus.Match_Fail)
            {
                Log("Cannot find any matches with the given feedback from API (Failure in finding correct keys)", LogType.Error);
            }

            messageShown = true;
        }
    }

    private static boolean Connected = false;
    public static boolean IsConnected() {return Connected;}

    private String username;

    protected static List<Repo> Repos;

    protected static String[] GetJsonResponse(String url)
    {
        try
        {
            BufferedReader in = new BufferedReader(new InputStreamReader(new URL(url).openStream()));

            String inputLine;
            String[] Lines = null;
            Log("API FOUND, ATTEMPTING TO DECODE", LogType.Information);
            while ((inputLine = in.readLine()) != null)
            {
                Lines = inputLine.split(",");
                break;
            }
            in.close();
            return Lines;
        }
        catch(MalformedURLException | FileNotFoundException e)
        {
            Log("ERROR: Unknown link to API -> " + e.getMessage(), LogType.Error);
        }
        catch(IOException io)
        {
            if(io.getMessage().contains("HTTP response code: 403 for URL"))
            {
                Log("FAULT: Max connections made, Please try again later!", LogType.Error);
            }
            else
            {
                Log("ERROR: Problem with IO handling -> " + io.getMessage(), LogType.Error);
            }
        }
        return null;
    }

    protected static String[] GetJsonURL(String url)
    {
        try
        {
            APIStatus result = CanDoSearch();
            if(result != APIStatus.Working)
            {
                CallMaxFetchMessage(result);
                throw new Exception("MAX API COUNT HIT");
            }

            lastAPIStatus = result;

            URL site = ValidURL(url);

            if(site == null)
                return null;

            return GetJsonResponse(url);
        }
        catch (Exception ee)
        {
            if(ee.getMessage() != "MAX API COUNT HIT")
                System.out.println("[ERROR]: " + ee.getMessage());
        }
        return null;
    }

    protected static URL ValidURL(String url)
    {
        try
        {
            URL site = new URL(url);
            Log("CHECKING IF CLIENT HAS INTERNET ACCESS", LogType.Information);

            URLConnection signal = site.openConnection();

            Log("STEP 1: ANALYZE INTERNET SIGNAL", LogType.Information);
            signal.connect();
            Log("SUCCESS, PROCEEDING TO CONNECT TO API", LogType.Information);
            Connected = true;
            return site;
        }
        catch (IOException e)
        {
            Log("ERROR - NO CONNECTION", LogType.Error);
            Connected = false;
            return null;
        }
    }

    private static LogTypeSettings printStatus = LogTypeSettings.LogWithError;

    public enum LogTypeSettings
    {
        LogAll,
        LogWithError,
        LogStatusOnly,
        LogErrorOnly,
        LogInformationOnly
    }

    public enum LogType
    {
        Error,
        Status,
        Information
    }

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

    protected static void Log(String message, LogType mType)
    {
        //Print the message right away, no restrictions
        if(printStatus ==  LogTypeSettings.LogAll)
        {System.out.println(message);return;}

        //Check if printStatus is "LogWithError" and check if the message type is "Error" or "Status"
        if(printStatus == LogTypeSettings.LogWithError && mType == LogType.Error || mType == LogType.Status)
        {System.out.println(message);return;}


        //Check if printStatus is "LogStatusOnly" and check if the message type is "Status"
        if(printStatus ==  LogTypeSettings.LogStatusOnly && mType == LogType.Status)
        {System.out.println(message);return;}

        //Check if printStatus is "LogErrorOnly" and check if the message type is "Error"
        if(printStatus ==  LogTypeSettings.LogErrorOnly && mType == LogType.Error)
        {System.out.println(message);return;}

        //Check if printStatus is "LogInformationOnly" and check if the message type is "Information"
        if(printStatus ==  LogTypeSettings.LogInformationOnly && mType == LogType.Information)
        {System.out.println(message);return;}

    }

    public GitHubUpdater(String Username, LogTypeSettings LogToConsole) throws IOException { LogVersion(); printStatus = LogToConsole; Start(Username);}

    public GitHubUpdater(String DirectSearchText, LogTypeSettings LogToConsole, boolean IsDirectURL) throws IOException
    {
        LogVersion();
        DirectSearch = IsDirectURL;
        printStatus = LogToConsole;
        Start(DirectSearchText);
    }

    private void Start(String Username) throws IOException {
        this.username = (!DirectSearch) ? Username : Username.substring(0, Username.indexOf('/'));

        try {
            if(DirectSearch)
                Direct_Init(Username);
            else
                Init();
        }
        catch(UnknownHostException unH)
        {
            Log("ERROR - NO INTERNET CONNECTION OR HOST IS DOWN -> " + unH.getMessage(), LogType.Error);
        }
    }

    private void Init() throws IOException
    {
        //https://docs.oracle.com/javase/tutorial/networking/urls/readingURL.html
        String user = GetAPIReposLink();

        Log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information);

        String[] Lines = GetJsonURL(user);

        Log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information);

        if(Lines == null)
        {
            Log("Error parsing or getting URL for parsing", LogType.Error);
            return;
        }

        //Get all the data and fling it into Repos
        Repos = SubSectionJson(Lines);
    }

    private void Direct_Init(String Username) throws IOException
    {
        //https://docs.oracle.com/javase/tutorial/networking/urls/readingURL.html
        //String user = GetAPIReposLink();
        String directLink = String.format("https://api.github.com/%s", Username );

        Log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information);

        String[] Lines = GetJsonURL(directLink);

        Log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information);

        if(Lines == null)
        {
            Log("Error parsing or getting URL for parsing", LogType.Error);
            return;
        }

        //Get all the data and fling it into Repos
        Repos = SubSectionJson(Lines);
    }

    private String GetAPIReposLink()
    {
        return String.format("https://api.github.com/users/%s/repos", this.username );
    }

    private List<Repo> SubSectionJson(String[] lines)
    {
        List<Repo> f = new ArrayList<Repo>();
        Repo r = new Repo();

        String result;

        for(int i = 0; i < lines.length; i++)
        {
            if(lines[i].startsWith("["))
                lines[i] = lines[i].replace('[',' ').trim();

            //Check if start of json (starts with first attribute "id")
            if(lines[i].startsWith("{\"id\""))
            {
                //Jump one
                i++;

                //Iterate within
                while(i < lines.length)
                {
                    if(CanSafelyJumpIndex((i + 1), lines.length))
                        if(lines[i].endsWith("}") && lines[i + 1].startsWith("{\"id\"")) { break; } //Jump out of loop, reached end

                    result = GetValue(lines[i]);

                    if(lines[i].startsWith("\"owner\""))
                    {
                        i+=18;
                        continue;
                    }

                    if(CanSafelyJumpIndex((i + 1), lines.length))
                        if(result == null && lines[i + 1].startsWith("{\"id\""))
                        {
                            break;
                        }

                    if(r.getProject_Name() == null) if(lines[i].startsWith(GetKey("name"))) { r.setProject_Name(result); i++; continue;} //If Json key is "name" set it's value

                    if(r.getProject_Full_Name() == null) if(lines[i].startsWith(GetKey("full_name"))) {r.setProject_Full_Name(result); i++; continue;}

                    if(r.getProject_URL() == null) if(lines[i].startsWith(GetKey("html_url"))) {r.setProject_URL(result); i++; continue;}

                    if(r.getProject_APIURL() == null) if(lines[i].startsWith(GetKey("url"))) {r.setProject_APIURL(result); i++; continue;}

                    if(r.getProject_Description() == null) if(lines[i].startsWith(GetKey("description"))) {r.setProject_Description(result); i++; continue;}

                    if(r.getProject_Language() == null) if(lines[i].startsWith(GetKey("language"))) {r.setProject_Language(result); i++; continue;}

                    if(r.getProjects_Creation_Date() == null) if(lines[i].startsWith(GetKey("created_at"))) {r.setProjects_Creation_Date(result); i++; continue;}

                    if(r.getProjects_Latest_Update_Date() == null) if(lines[i].startsWith(GetKey("updated_at"))) {r.setProjects_Latest_Update_Date(result); i++; continue;}

                    if(r.getProjects_Latest_Push_Date() == null) if(lines[i].startsWith(GetKey("pushed_at"))) {r.setProjects_Latest_Push_Date(result); i++; continue;}
                    i++;
                }

                if(!r.SuccessfulFind())
                {
                    Log("ERROR - One or more fields failed to be assigned!", LogType.Error);
                    break;
                }
                else
                {
                    Log(String.format("Success! \"%s\" has been found and assigned to!", r.getProject_Full_Name()), LogType.Information);
                    f.add(r);
                }

                //Reset for next repo
                r = new Repo();
            }
        }
        return f;
    }

    protected static String GetValue(String line)
    {
        //  [  \"\w+\":(\"?.+\"?)  ] <- Regex
        Pattern pattern = Pattern.compile("\\\"\\w+\\\":(\\\"?.+\\\"?)");
        Matcher matcher = pattern.matcher(line);
        while (matcher.find())
        {
            String repond = matcher.group(1);

            if(repond.contains("\""))
                repond = repond.replace("\"", "").trim();

            if(repond.contains(","))
                repond = repond.replace(",", "").trim();

            return repond;
        }

        return null;
    }

    protected static boolean CanSafelyJumpIndex(int NewIndex, int ArraySize) {
        return NewIndex < ArraySize;
    }

    protected static String GetKey(String key)
    {
        return String.format("\"%s\"",key);
    }
}
