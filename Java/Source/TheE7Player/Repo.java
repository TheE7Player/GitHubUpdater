package TheE7Player;

import java.time.Instant;
import java.util.Date;

public class Repo {
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
    */

    private String Project_Name;
    private String Project_Full_Name;
    private String Project_URL;
    private String Project_APIURL;
    private String Project_Description;
    private String Project_Language;
    private Date Projects_Creation_Date;
    private Date Projects_Latest_Update_Date;
    private Date Projects_Latest_Push_Date;

    public Repo() { }

    public String getProject_Name() {
        return Project_Name;
    }

    public void setProject_Name(String project_Name) {
        Project_Name = project_Name;
    }

    public String getProject_Full_Name() {
        return Project_Full_Name;
    }

    public void setProject_Full_Name(String project_Full_Name) {
        Project_Full_Name = project_Full_Name;
    }

    public String getProject_URL() {
        return Project_URL;
    }

    public void setProject_URL(String project_URL) {
        Project_URL = project_URL;
    }

    public String getProject_APIURL() {
        return Project_APIURL;
    }

    public void setProject_APIURL(String project_APIURL) {
        Project_APIURL = project_APIURL;
    }

    public String getProject_Description() {
        return Project_Description;
    }

    public void setProject_Description(String project_Description) {
        Project_Description = project_Description;
    }

    public String getProject_Language() {
        return Project_Language;
    }

    public void setProject_Language(String project_Language) {
        Project_Language = project_Language;
    }

    public Date getProjects_Creation_Date() {
        return Projects_Creation_Date;
    }

    public void setProjects_Creation_Date(String projects_Creation_Date) {
        Projects_Creation_Date =  ParseDate(projects_Creation_Date);
    }

    public Date getProjects_Latest_Update_Date() {
        return Projects_Latest_Update_Date;
    }

    public void setProjects_Latest_Update_Date(String projects_Latest_Update_Date) {
        Projects_Latest_Update_Date =  ParseDate(projects_Latest_Update_Date);
    }

    public Date getProjects_Latest_Push_Date() {
        return Projects_Latest_Push_Date;
    }

    public void setProjects_Latest_Push_Date(String projects_Latest_Push_Date) {
        Projects_Latest_Push_Date = ParseDate(projects_Latest_Push_Date);
    }

    public boolean SuccessfulFind()
    {
        String[] variables = new String[]
        {
            Project_Name,
            Project_Full_Name,
            Project_URL,
            Project_APIURL,
            Project_Description,
            Project_Language
        };

        Date[] dates = new Date[]
        {
            Projects_Creation_Date,
            Projects_Latest_Push_Date,
            Projects_Latest_Update_Date
        };

        for(String i : variables)
        {
            if(i == null)
                return false;
        }

        for (Date d : dates)
        {
            if(d ==null)
                return false;
        }

        return true;
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
}


