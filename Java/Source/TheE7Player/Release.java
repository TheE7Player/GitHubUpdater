package TheE7Player;

import java.time.Instant;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import static TheE7Player.GitHubUpdater.Log;

public class Release
{

    public Boolean parsedCorrectly = false; //If parsing the string works

    //COMMENT PATTERN -> // [Field description] ("[Json parent name]")
    private String r_Name; //Holds the releases title ("name")
    private String r_Tag; //Holds the releases tag ("tag_name")
    private String r_URL; //Holds url of this release ("html_url")
    private String r_Info; //Holds the body text (eg. Fixed bug with errors etc) ("body")

    private Date r_Created; //Holds the date where it was drafted ("created_at")
    private Date r_Published; //Holds the date where it was open to the public ("published_at")

    private Boolean r_isPreRelease; //Holds if the release is a prerelease ("prerelease")

    private List<Assets> r_Files;

    public Release(String[] Result)
    {
        /*Array should be 7/8 in length
            Result[0] = Name ("name")
            Result[1] = Tag ("tag_name")
            Result[2] = URL ("html_url")
            Result[3] = INFO ("body")
            Result[4] = Create Date ("created_at")
            Result[5] = Published Date ("published_at")
            Result[6] = PreRelease ? true/false ("prerelease")
        */

        if(Result.length != 7)
            return;

        try
        {
            this.r_Name = Result[0];
            this.r_Tag = Result[1];
            this.r_URL = Result[2];
            this.r_Info = Result[3];

            this.r_Created = ParseDate(Result[4]);
            this.r_Published = ParseDate(Result[5]);

            this.r_isPreRelease = (Result[6] == "true" || Result[6] == "True") ? true : false;

            Object[] test = {r_Name, r_Tag, r_URL, r_Info, r_Created, r_Published, r_isPreRelease};
            for(Object t : test)
            {
                if(t == null)
                {
                    parsedCorrectly = false;
                    return;
                }
            }
            parsedCorrectly = true;
        }
        catch(Exception e)
        {
            String error = String.format("PARSING FAILED",e.getMessage());
            Log(error, GitHubUpdater.LogType.Error);
            parsedCorrectly = false;
        }
    }

    private Date ParseDate(String input)
    {
        //https://stackoverflow.com/questions/2201925/converting-iso-8601-compliant-string-to-java-util-date
        try
        {
            //ISO 8601 DATE EXAMPLE: "2019-11-20T00:07:50Z"
            return Date.from( Instant.parse(input));
        }
        catch(Exception e)
        {
            return null;
        }
    }

    public void AddAsset(Assets assets)
    {
        if(r_Files == null)
            r_Files = new ArrayList<Assets>(1);

        if(r_Files.contains(assets))
        {Log(String.format("[INFO] Ignoring %s as it is already included into the list", assets.getItemName()), GitHubUpdater.LogType.Information); return;}
        else
            r_Files.add(assets);
    }

    public List<Assets> getAssets()
    {
        return (r_Files != null ) ? r_Files : null;
    }

    public String getName() { return (parsedCorrectly) ? r_Name :  null; }

    public String getTag() {
        return (parsedCorrectly) ? r_Tag :  null;
    }

    public String getURL() {
        return (parsedCorrectly) ? r_URL :  null;
    }

    public String getInfo() {
        return (parsedCorrectly) ? r_Info :  null;
    }

    public Date getCreatedDate() {
        return (parsedCorrectly) ? r_Created :  null;
    }

    public Date getPublishedDate() {
        return (parsedCorrectly) ? r_Published :  null;
    }

    public Boolean getIsPreRelease() {
        return (parsedCorrectly) ? r_isPreRelease :  null;
    }
}
