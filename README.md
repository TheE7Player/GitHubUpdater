# GitHubUpdater 
**GitHubUpdater** is a library to allow coders to implement an updater for their projects using GitHub, easy.

*Library is open to contributors to implement into their own programming language, please inform me so this project can expand!*

(Java only at the moment, will expand in a bit)

Here are the simplest ways of achieving them (v0.1):

# Information (Important!)
For each API call you make, takes one

# Testing
Testing should be done for each update:
## Java
Passed all tests (5/5) [10/12/19] (Can be found in testing.java)

|Test Number|Test Function|PASSED/FAILED|
|:---:|:---:|:---:|
|1|Test_GETREPOS|✔|
|2|Test_COMPAREVERISON|✔|
|3|Test_COMPARELESS|✔|
|4|Test_GetRelease|✔|
|5|Test_DirectSearch|✔|


# Retrieve Repo(s) from Username

## Java
```java
try   
{  
	//Create an instance of GitHubFunctions from user "TheE7Player"
	GitHubFunctions udp = new GitHubFunctions("thee7player", GitHubUpdater.LogTypeSettings.LogStatusOnly);
    
    //Fetch "public" repos under the username "TheE7Player"
	String[] Repos = udp.GetRepos();  
  
	if(Repos != null) //Check if Repos doesn't return empty
		for (String i : Repos) //Iterate through "Repos" and store it into "i"
		    System.out.println(String.format("%s Repos -> %s", udp.GetUserName(), i));
	
	/*
		Output:
		thee7player Repos -> CSGO-Event-Viewer
		thee7player Repos -> CSGO_Event_Parser
		thee7player Repos -> Old_Projects
	*/
} 
catch (IOException e) 
{  
     //Problem! Do something!
     //TODO: Do something
}  
```
```java
//Using DirectSearch feature (Takes 1 API call instead of 2)
//DirectSearch ignores the initial check on the user name before going into the repository

try   
{  
	//Create an instance of GitHubFunctions from user "TheE7Player"
	//[Note]: Must include "repos/[username]/[project_name]
	//[Note]: Uses boolean for DirectSearch, setting it to "false" will ignore the DirectSearch
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
	
	/*
		Output:
		Repo Name: CSGO-Event-Viewer
		Repo API Link: https://api.github.com/repos/TheE7Player/CSGO-Event-Viewer
		Repo Date (Updated): Sun Dec 08 15:39:29 GMT 2019
	*/
} 
catch (IOException e) 
{  
     //Problem! Do something!
     //TODO: Do something
}
```
# Comparing Updates
## Java
There are two methods available to compare versions (*uses ProductComparer class*) :

    ProductComparer.CompareVersionEqual
This function returns a boolean if the **current release version** is the same as the **current running version**

    ProductComparer.CompareVersionLess
This functions returns a boolean if the **current running version** is *less than* the **current release version**.

*Note: A repo variable (which is initialized with data) is required in order to function!* (udp is assumed declared):
```java
Repo target = udp.GetRepository("CSGO-Event-Viewer"); //Targetting "CSGO-Event-Viewer" Repository
```
```java
//This demonstration shows how you can use the ProductComparer class
//[NOTE] : Wrap this code in a try catch with IOException!
/// Assuming "target" has tag version of "0.4.1"

//Returns false: "0.3" isn't equal to "0.4.1"
boolean Compare_Equal1 = ProductComparer.CompareVersionEqual("thee7player", "0.3", target);

//Returns true: "0.4.1" is equal to "0.4.1"
boolean Compare_Equal2 = ProductComparer.CompareVersionEqual("thee7player", "0.4.1", target);

//Returns false: "0.3" is less than "0.4.1"
boolean Compare_Less1 = ProductComparer.CompareVersionLess("thee7player", "0.3", target);

//Returns false: "0.7" isn't less than "0.4.1"
boolean Compare_Less2 = ProductComparer.CompareVersionLess("thee7player", "0.7", target);
```
# Fetching Release Files
This library will allow you to target releases with additional information on the files from the release
# Java
*Note: Assuming udp is already initialized*
```java
//Param 1 (Type: Repo): Target Repository (The repo you want to search/fetch from!)
//Param 2 (Type: Boolean): If you want to include release file information (Set to false if you want to just see the release information)
//Param 3 (Type: Int) : The max releases you want to find (Keep low, can take along while!)

//Look at the release information in "CSGO-Event-Viewer", grab the release files information and only return back 2 of the latest releases...
Release[] r = ProductComparer.GetUpdates(udp.GetRepository("CSGO-Event-Viewer"), "true", 2);

//List to store information on the files from "r" (Requires: import java.util.List)
List<Assets> _files = null;

//Check if "r" isn't null before iterating through
if(r != null)
{
	//Loop through "r" and store it into "a"
	for (Release a : r)  
	{
		_files = a.getAssests(); //<- Get files from each release
		
		System.out.println(String.format("Release: %s",a.getName()));  
		System.out.println(String.format("└─ Tag: %s",a.getTag()));  
		System.out.println(String.format("└─ Url: %s",a.getURL()));  
		System.out.println(String.format("└─ Prerelease: %s",a.getIsPreRelease()));  
  
		System.out.println("└─ Dates:");  
		System.out.println(String.format(" └─ Created Date (When drafted) : 		%s",a.getCreatedDate()));  
		System.out.println(String.format(" └─ Published Date (When visible to public): %s",a.getPublishedDate()));  
  
		if(_files != null) //Loop through "_files" if it isn't null (empty variable)
		{
			System.out.println(String.format("└─ Downloadable files (%d): ", _files.size()));  
			for(Assets x : _files)  
			    System.out.println(String.format(" └─ File %s : %s (%s) -> %s", x.getItemName(), x.getItemSize(Assets.DisplaySize.Megabytes), x.getItemSize(Assets.DisplaySize.Kilobytes), x.getItemURL()));
		}
	}
}
```
```
OUTPUT:

Release: Beta Build 0.4.2
└─ Tag: 0.4.2
└─ Url: https://github.com/TheE7Player/CSGO-Event-Viewer/releases/tag/0.4.2
└─ Prerelease: false
└─ Dates:
     └─ Created Date (When drafted) : Sun Dec 08 15:39:26 GMT 2019
     └─ Published Date (When visible to public): Sun Dec 08 15:44:45 GMT 2019
└─ Downloadable files (1): 
     └─ File CSGO_Event_Finder.0.4.2.zip : 2.8MB (2882.3KB) -> https://github.com/TheE7Player/CSGO-Event-Viewer/releases/download/0.4.2/CSGO_Event_Finder.0.4.2.zip

Release: Beta Build 0.4.1
└─ Tag: 0.4.1
└─ Url: https://github.com/TheE7Player/CSGO-Event-Viewer/releases/tag/0.4.1
└─ Prerelease: false
└─ Dates:
     └─ Created Date (When drafted) : Wed Nov 20 00:04:50 GMT 2019
     └─ Published Date (When visible to public): Wed Nov 20 00:07:50 GMT 2019
└─ Downloadable files (1): 
     └─ File CSGO_Event_Finder.zip : 2.5MB (2552.2KB) -> https://github.com/TheE7Player/CSGO-Event-Viewer/releases/download/0.4.1/CSGO_Event_Finder.zip
```
