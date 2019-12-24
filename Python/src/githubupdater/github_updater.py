# github_updater.py - Main logic behind the library

import re  # Import pythons ReGex library (Pattern matching)

# Python Library Dependencies
from datetime import datetime  # Import time module
from enum import Enum
from typing import List

# Import the enums into the library
from Classes.Repo import Repo


# APIStatus - Enum for API events (Response feedback)
class APIStatus(Enum):
    none = 1
    working = 2
    max_fetch = 3
    response_null = 4
    match_fail = 5


# LogTypeSettings - Enum which changes the log behaviour of the program
class LogTypeSettings(Enum):
    log_all = 1
    log_with_error = 2
    log_status_only = 3
    log_error_only = 4
    log_information_only = 5


# LogType - Enum which goes along side LogTypeSettings, describes the status of the log
class LogType(Enum):
    error = 1
    status = 2
    information = 3


_connected = False  # Assign it
_messageShown = False  # Used in CallMaxFetchMessage()
_log_version_shown = False
_version = '0.1'  # Holds library version
_next_window_reset: str = None
_last_api_status = APIStatus.none
_print_status = LogTypeSettings.log_with_error


def __parse_date(self, _input: str):
    return datetime.fromtimestamp(_input).strftime("%Y-%m-%d %I:%M:%S")


def parse_date(_input: str):
    # https://stackabuse.com/converting-strings-to-datetime-in-python/
    date_time_obj = datetime.strptime(_input, "%Y-%m-%dT%H:%M:%SZ")
    return f"{date_time_obj.date()} {date_time_obj.time()}"


def _log_version():
    """Logs to the console the current version of the library"""
    if not _log_version_shown:
        print(f"RUNNING GitHubUpdater (Python) version: {_version}")


def log(message: str, _type: LogType):
    """Prints out a message with a category type"""

    if _print_status == LogTypeSettings.log_all:
        print(message)
        pass

    if _print_status == LogTypeSettings.log_with_error and (_type == LogType.error or _type == LogType.status):
        print(message)
        pass

    if _print_status == LogTypeSettings.log_status_only and _type == LogType.status:
        print(message)
        pass

    if _print_status == LogTypeSettings.log_error_only and _type == LogType.error:
        print(message)
        pass

    if _print_status == LogTypeSettings.log_information_only and _type == LogType.information:
        print(message)
        pass


def _call_max_fetch_message(result: APIStatus):
    global _last_api_status
    global _messageShown

    _last_api_status = result

    if not _messageShown:
        if result == APIStatus.max_fetch:
            new_time = _next_window_reset.__str__()
            log(f"Cannot call API as max fetches are used (60 max per Hour) Rate limit resets back at: {new_time}",
                LogType.status)
        elif result == APIStatus.response_null:
            log("Issue with feedback from response to API, connection may be down or problem with link", LogType.error)
        elif result == APIStatus.match_fail:
            log("Cannot find any matches with the given feedback from API (Failure in finding correct keys)",
                LogType.error)
        _messageShown = True

# TODO: Change to "_last_api_status == APIStatus.working"
def _safe_to_search() -> bool:
    return APIStatus.working


def get_json_url(url: str) -> str:
    global _last_api_status

    try:
        result = _can_do_search()
        if not result == APIStatus.working:
            _call_max_fetch_message(result)
            raise Exception(f"MAX API COUNT HIT (RESET AT {_next_window_reset.__str__()})")
        _last_api_status = result
        site = valid_url(url)

        if not site:
            return None

        return get_json_response(url)
    except (ValueError, Exception) as e:
        print(f"[ERROR] {e}")
        return None


def subsection_json(lines: List[str]) -> List[str]:
    new_list: List[Repo] = []
    result: str = None
    # Using different approach from Java/C# (Going to use dictionary to assign key variables)
    n_repo = {"Name": None, "Full Name": None, "URL": None, "API": None, "Info": None, "Lang": None,
              "Creation_Date": None, "Update_Date": None, "Push_Date": None}

    for i in range(0, len(lines)):
        if lines[i].startswith('['):
            lines[i].replace('[', ' ').strip()

        if lines[i].startswith("{\"id\""):
            i += 1
            while i < len(lines):
                if can_safely_jump((i + 1), len(lines)):
                    if lines[i].endswith('}') and lines[i].startswith("{\"id\""):
                        break
                result = _get_value(lines[i])
                if lines[i].startswith("\"owner\""):
                    i += 18
                    continue
                if can_safely_jump((i + 1), len(lines)):
                    if result is None and lines[i].startswith("{\"id\""):
                        break

                if n_repo["Name"] is None and lines[i].startswith(_get_key("name")):
                    n_repo["Name"] = result
                    i += 1
                    continue

                if n_repo["Full Name"] is None and lines[i].startswith(_get_key("full_name")):
                    n_repo["Full Name"] = result
                    i += 1
                    continue

                if n_repo["URL"] is None and lines[i].startswith(_get_key("html_url")):
                    n_repo["URL"] = result
                    i += 1
                    continue

                if n_repo["API"] is None and lines[i].startswith(_get_key("url")):
                    n_repo["API"] = result
                    i += 1
                    continue

                if n_repo["Info"] is None and lines[i].startswith(_get_key("description")):
                    n_repo["Info"] = result
                    i += 1
                    continue

                if n_repo["Lang"] is None and lines[i].startswith(_get_key("language")):
                    n_repo["Lang"] = result
                    i += 1
                    continue

                if n_repo["Creation_Date"] is None and lines[i].startswith(_get_key("created_at")):
                    n_repo["Creation_Date"] = result
                    i += 1
                    continue

                if n_repo["Update_Date"] is None and lines[i].startswith(_get_key("updated_at")):
                    n_repo["Update_Date"] = result
                    i += 1
                    continue

                if n_repo["Push_Date"] is None and lines[i].startswith(_get_key("pushed_at")):
                    n_repo["Push_Date"] = result
                    i += 1
                    continue

                i += 1

            # Now we port it into a repo
            new_repo = Repo(n_repo["Name"], n_repo["Full Name"], n_repo["URL"], n_repo["API"], n_repo["Info"],
                            n_repo["Lang"], n_repo["Creation_Date"], n_repo["Update_Date"], n_repo["Push_Date"])

            if not new_repo.successful_find():
                log("ERROR - One or more fields failed to be assigned!", LogType.error)
                break
            else:
                log(f"Success! \"{new_repo.full_name}\" has been found and assigned to!", LogType.information)
                new_list.append(new_repo)
                del new_repo


def get_json_response(url: str) -> List[str]:
    import requests
    from requests.exceptions import MissingSchema

    result: List[str] = []
    try:
        # Get the url response, and convert the byte stream to a string (__str__())
        response = requests.get(url).content.__str__()

        # Now we get rid of b'' starting
        response = response[2:]

        # Now we split the string into sub-sections from where ',' is in each line
        response_arr = response.split(',')

        # Now we iterate it through into result
        for line in response_arr:
            result.append(line)

        return result
    except (ValueError, Exception, MissingSchema) as e:
        log(f"ERROR - NO CONNECTION: {e}", LogType.error)
        return None


def valid_url(url: str) -> bool:
    # Depended on lib "requests" TODO: ALERT ON WIKI ON LIBRARY USED
    global _connected

    import requests
    from requests.exceptions import MissingSchema

    try:
        log("STEP 1: ANALYZE INTERNET SIGNAL", LogType.information)
        request = requests.get(url)
        log("SUCCESS, PROCEEDING TO CONNECT TO API", LogType.information)
        _connected = True
        return True
    except MissingSchema:
        log("ERROR - NO CONNECTION", LogType.error)
        _connected = False
        return False


# TODO: Change true to "_connected" after testing is done!
def is_connected() -> bool:
    """Returns back if the client is connected to the API or internet"""
    return True


def _get_value(line: str) -> str:
    pattern = re.compile(r"\"\w+\":(\"?.+\"?)")
    if not pattern.match(line):
        return None

    respond = pattern.search(line).group(1)  # Target the first group found (The value in the json pair)

    if "\"" in respond:
        respond = respond.replace("\"", "").strip()  # Remove the " from the string and trim any spaces (if any) (strip)

    if "," in respond:
        respond = respond.replace(",", "").strip()  # Remove the , from the string and trim any spaces (if any) (strip)

    return respond


def _get_key(key: str) -> str:
    return f"\"{key}\""


def get_key(key: str) -> str: return _get_key(key)


def get_value(line: str) -> str: return _get_value(line)


def _can_do_search() -> APIStatus:
    global _next_window_reset

    response = get_json_response("https://api.github.com/rate_limit")
    if response is None:
        return APIStatus.response_null

    for line in response:
        if "\"remaining\":" in line:
            try:
                amount = line[line.index(":") + 1:]
                int_amount = int(amount)
                int_amount_new = (int_amount - 1) if (int_amount - 1) > 0 else 0
                _next_window_reset = response[2][response[2].index(":") + 1:].replace("}", "")
                log(f"API COUNT FROM {int_amount} to {int_amount_new}", LogType.information)
                return APIStatus.working if (int_amount - 1) > 0 else APIStatus.max_fetch
            except(ValueError, Exception):
                return APIStatus.match_fail


def _get_reset_limit_date() -> str:
    global _next_window_reset

    if _next_window_reset is None:
        _can_do_search()

    return _next_window_reset


def can_safely_jump(index: int, arr_size: int) -> bool:
    return index < arr_size


class GitHubUpdater:
    # Yet to be declared variables
    _username: str = None
    _direct_search = False
    _repos: List[Repo] = []

    # Constructors
    def __init__(self, username: str, log_to_console: LogTypeSettings):
        _log_version()
        global _print_status
        _print_status = log_to_console
        self._start(username)

    def __init__(self, direct_search: str, log_to_console: LogTypeSettings, direct_url: bool):
        _log_version()
        global _print_status
        _print_status = log_to_console
        self._direct_search = direct_url
        self._start(direct_search)

    def _start(self, username: str):
        self._username = username if self._direct_search else username[:username.index('/')]

        try:
            if self._direct_search:
                self._direct_search(username)
            else:
                self._init()
        except (ValueError, Exception) as error:
            log(f"ERROR - NO INTERNET CONNECTION OR HOST IS DOWN -> {error}", LogType.error)

    def _init(self):
        user = f"https://api.github.com//users//{self._username}//repos"
        log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information)

        Lines = get_json_url(user)

        log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information)

        if Lines is None:
            log("Error parsing or getting URL for parsing", LogType.Error)
            pass

        self._repos = subsection_json(Lines)

    def _direct_init(self, link: str):
        direct_link = f"https://api.github.com/{link}"

        log("LINK SETUP COMPLETE, ATTEMPTING TO CONNECT TO GITHUB API", LogType.Information)

        Lines = get_json_url(direct_link)

        log("DECOMPOSING JSON INTO SUITABLE GROUPS", LogType.Information)

        if Lines is None:
            log("Error parsing or getting URL for parsing", LogType.Error)
            pass

        self._repos = subsection_json(Lines)
