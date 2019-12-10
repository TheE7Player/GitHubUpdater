package TheE7Player;

import java.io.IOException;
import java.util.List;

public class GitHubFunctions extends GitHubUpdater
{
    private String username;

    public GitHubFunctions(String DirectSearchText, LogTypeSettings LogToConsole, boolean IsDirectURL) throws IOException {
        super(DirectSearchText, LogToConsole, IsDirectURL);
    }

    public GitHubFunctions(String Username, LogTypeSettings LogToConsole) throws IOException {
        super(Username, LogToConsole);
        this.username = Username;
    }

    public String GetUserName()
    {
        return this.username;
    }

    public Repo GetRepository(int Index)
    {
        try
        {
            return Repos.get(Index);
        }
        catch(Exception e)
        {
            return null;
        }
    }

    public Repo GetRepository(String name)
    {
        if(Repos == null)
            return null;

        for(Repo ex : Repos)
        {
            if(ex.getProject_Name().equals(name))
                return ex;
        }

        return null;
    }

    public String[] GetRepos()
    {
        //Fetch the repos from GitHubUpdater.java
        List<Repo> lists = GitHubUpdater.Repos;

        //Test if not null
        if(lists == null)
            return null;

        String[] names = new String[lists.size()];

        //We can continue
        for (int i = 0; i < lists.size(); i++) {
            names[i] = lists.get(i).getProject_Name();
        }

        //Return list to array and force it into String and not Object (" new String[0] ")
        return names;
    }

    public static String GetResetLimitDate()
    {
        return GitHubUpdater.GetResetLimitDate();
    }

}
