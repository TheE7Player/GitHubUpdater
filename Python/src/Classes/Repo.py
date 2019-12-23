# Repo.py - Handles users Repositories that are public

import datetime  # Import time module for "__parse_date"


class Repo:
    _parsedFailed = False
    name: str = None
    full_name: str = None

    def __parse_date(self, _input: str):
        # https://stackabuse.com/converting-strings-to-datetime-in-python/
        date_time_obj = datetime.datetime.strptime(_input, "%Y-%m-%dT%H:%M:%SZ")
        return f"{date_time_obj.date()} {date_time_obj.time()}"

    def __init__(self, Name: str, Full_Name: str, URL: str, API_URL: str, Description: str, Language: str,
                 Creation_Date: str, Latest_Date: str, Latest_Push: str):
        self.name = Name
        self.full_name = Full_Name
        self.__project_url = URL
        self.__project_apiurl = API_URL
        self.__project_description = Description
        self.__project_language = Language
        try:
            self.__projects_creation_date = self.__parse_date(Creation_Date)
            self.__projects_latest_update_date = self.__parse_date(Latest_Date)
            self.__projects_latest_push_date = self.__parse_date(Latest_Push)
            _parsedFailed = False
        except (ValueError, Exception):
            _parsedFailed = True

    def successful_find(self) -> bool:
        return not self._parsedFailed


''' TESTING GOES HERE
e = Repo("CSGO-Event-Viewer", "TheE7Player/CSGO-Event-Viewer", "https://github.com/TheE7Player/CSGO-Event-Viewer",
         "https://api.github.com/repos/TheE7Player/CSGO-Event-Viewer",
         "A Java project to allow users to see what events you can use with logic_eventlistener", "Java",
         "2019-11-16T19:00:17Z", "2019-11-30T16:23:40Z", "2019-11-30T16:23:38Z")
print(e)
'''
