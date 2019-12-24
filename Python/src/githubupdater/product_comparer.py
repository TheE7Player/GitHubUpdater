# product_comparer.py - Handles version checking

import re
from typing import List

from Classes.Repo import Repo
from Classes.Release import Release
from Classes.Assets import Asset
from githubupdater import get_json_response, LogType, log, is_connected, get_json_url, can_safely_jump, get_key, \
    get_value
from githubupdater.github_updater import _safe_to_search

_current_online_version: str = None


def current_online_version():
    return _current_online_version;


def _array_full(arr: list) -> bool:
    for item in arr:
        if item is None:
            return False
    return True


def _fix_version(_input: str):
    new_input = re.sub(r"[^\\d.]", "", _input).strip()
    after_dot = f"{new_input[new_input.index('.'):].replace('.', '').strip()}"
    before_dot = f"{new_input[:new_input.index('.')]}."
    return f"{before_dot}{after_dot}"


def get_api_count():
    response = get_json_response("https://api.github.com/rate_limit")
    if response is None:
        return -1
    for line in response:
        if "\"remaining\":" in line:
            try:
                amount = line[line.index(":") + 1:]
                int_amount = int(amount)
                int_amount_new = (int_amount - 1) if (int_amount - 1) > 0 else 0
                return int_amount_new
            except(ValueError, Exception):
                return -1


def get_updates(repos: Repo, find_release_files: bool, max_search: int):
    if max_search <= 1:
        log("MaxReleaseSearch is set to an unreasonable size, has to be more than 1", LogType.error)
        return None

    if repos is None:
        log("Unknown or wrong Repos connection! (Doesn't exist or wrong account lookup)", LogType.error)
        return None

    log("Starting process of fetching releases", LogType.information)
    list_of_releases: List[Release] = []
    list_of_assets: List[Asset] = []

    body = [None] * 7  # Fixed size array of 7
    if is_connected() and _safe_to_search():
        response = get_json_url(f"{repos.api_link}/releases")
        if response is None:
            return None
        log(f"Getting list of information on releases in project: {repos.full_name}", LogType.information)
        for i in range(0, len(response)):
            if can_safely_jump((i + 1), len(response)):
                if _array_full(body):
                    list_of_releases.append(Release(body))
                    if list_of_assets is not None and find_release_files:
                        if len(list_of_assets) > 0 and find_release_files:
                            for each_file in list_of_assets:
                                list_of_releases[len(list_of_releases) - 1].add_asset(each_file)

                    if not list_of_releases[len(list_of_releases) - 1].get_is_parsedcorrectly():
                        log(f"ERROR: Release failed in parsing! -> {', '.join(body)}", LogType.error)
                        return None
                    body = [None] * 7  # Reset the array, CANNOT use .clear() (Deletes the fixed size!)
                    list_of_assets.clear()

                    # Now check if we've reached our max limit
                    if len(list_of_releases) == max_search:
                        return list_of_releases

            _response = response[i].strip()
            if response[i].startswith(get_key("assets")) and find_release_files:
                i += 1
                i_name = None
                i_size = None
                i_url = None
                while i < len(response):
                    _response = response[i].strip()
                    if _response.startswith(get_key("name")):
                        i_name = get_value(_response)
                    if _response.startswith(get_key("size")):
                        i_size = get_value(_response)
                    if _response.startswith(get_key("browser_download_url")):
                        if "}" in _response:
                            _response = _response.replace("}", "").strip()
                        if "]" in _response:
                            _response = _response.replace("]", "").strip()
                        i_url = _response
                        list_of_assets.append(Asset(i_name, i_url, float(i_size)))
                        i_name = None
                        i_url = None
                        i_size = None
                        if can_safely_jump(i, len(response)):
                            if response[i + 1].startswith(get_key("tarball_url")):
                                break
                    if can_safely_jump(i, len(response)):
                        i += 1
                    else:
                        break
                    continue

            if body[0] is None and response[i].startswith(get_key("name")):
                body[0] = get_value(response[i])

            if body[1] is None and response[i].startswith(get_key("tag_name")):
                body[1] = get_value(response[i])

            if body[2] is None and response[i].startswith(get_key("html_url")):
                body[2] = get_value(response[i])

            if body[3] is None and response[i].startswith(get_key("body")):
                body[3] = get_value(response[i])

            if body[4] is None and response[i].startswith(get_key("created_at")):
                body[4] = get_value(response[i])

            if body[5] is None and response[i].startswith(get_key("published_at")):
                body[5] = get_value(response[i])

            if body[6] is None and response[i].startswith(get_key("prerelease")):
                body[6] = get_value(response[i])

    log("Finished process of fetching releases", LogType.information)


def compare_less(username: str, current_version: str, repos: Repo):
    if not is_connected():
        return False

    if not _safe_to_search():
        return False

    response = get_json_url(f"https://api.github.com/repos/{repos.full_name}/releases")

    latest_version = None

    for each_line in response:
        if each_line.startswith(get_key("tag_name")):
            latest_version = get_value(each_line)

    if latest_version is None:
        log("Failed to compare versions!", LogType.error)
        return False
    else:
        latest_ver = float(_fix_version(latest_version))
        current_version = float(_fix_version(latest_version))

        if current_version == latest_ver:
            log(f"{current_version} is that latest version available!", LogType.status)
            return True
        else:
            if current_version < latest_version:
                log(f"{current_version} is outdated, the newest one is {latest_version}!", LogType.status)
                return False


def compare_equal(username: str, current_version: str, repos: Repo):
    if not is_connected():
        return False

    if not _safe_to_search():
        return False

    response = get_json_url(f"https://api.github.com/repos/{repos.full_name}/releases")

    latest_version = None

    for each_line in response:
        if each_line.startswith(get_key("tag_name")):
            latest_version = get_value(each_line)

    if latest_version is None:
        log("Failed to compare versions!", LogType.error)
        return False
    else:
        latest_ver = float(_fix_version(latest_version))
        current_version = float(_fix_version(latest_version))

        if current_version == latest_ver:
            log(f"{current_version} is that latest version available!", LogType.status)
            return True
        else:
            log(f"{current_version} is outdated, the newest one is {latest_version}!", LogType.status)
            return False
