# githubupdater.py - Main logic behind the library

# Python Library Dependencies
from datetime import datetime  # Import time module
import re  # Import pythons ReGex library (Pattern matching)
from typing import List
import requests

# Import the enums into the library
from githubupdater.githubupdater_enum import APIStatus, LogTypeSettings, LogType
from Classes.Repo import Repo

_connected = False  # Assign it
_messageShown = False  # Used in CallMaxFetchMessage()
_log_version_shown = False
_version = '0.1'  # Holds library version
_next_window_reset: str = None
_last_api_status = APIStatus.none
_print_status = LogTypeSettings.log_with_error


# TODO: Key Functions


def __parse_date(self, _input: str):
    return datetime.fromtimestamp(_input).strftime("%Y-%m-%d %I:%M:%S")


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
            new_time = None  # TODO: Return NewWindowReset to string here!
            log(f"Cannot call API as max fetches are used (60 max per Hour) Rate limit resets back at: {new_time}",
                LogType.status)
        elif result == APIStatus.response_null:
            log("Issue with feedback from response to API, connection may be down or problem with link", LogType.error)
        elif result == APIStatus.match_fail:
            log("Cannot find any matches with the given feedback from API (Failure in finding correct keys)",
                LogType.error)
        _messageShown = True


def _safe_to_search() -> bool:
    return _last_api_status == APIStatus.working


def get_json_url(url: str) -> str:

    global _last_api_status

    try:
        result = _can_do_search()
        if not result == APIStatus.working:
            _call_max_fetch_message(result)
            raise Exception("MAX API COUNT HIT")
        _last_api_status = result
        site = valid_url(url)

        if not site:
            return None

        return get_json_response(url)
    except (ValueError, Exception) as e:
        print(f"[ERROR] {e}")
        return None


# TODO: Implement subsection_json
def subsection_json(lines: List[Repo]) -> None:
    return None


# TODO: Implement get_json_response
def get_json_response(url: str) -> List[str]:
    pass


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


def is_connected() -> bool:
    """Returns back if the client is connected to the API or internet"""
    return _connected


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


def _can_safely_jump(index: int, arr_size: int) -> bool:
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
