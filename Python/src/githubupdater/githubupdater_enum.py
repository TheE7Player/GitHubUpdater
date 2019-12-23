# githubupdater_enum.py - Extension to expand enums into the library
from enum import Enum


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
