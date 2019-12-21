# Release.py - Handles the release(s) information

import datetime  # Import time module for "__parse_date"
from Assets import Asset  # Import Assets.py into Release.py


class Release:

    def __parse_date(self, _input: str):
        # https://stackabuse.com/converting-strings-to-datetime-in-python/
        date_time_obj = datetime.datetime.strptime(_input, "%Y-%m-%dT%H:%M:%SZ")
        return f"{date_time_obj.date()} {date_time_obj.time()}"

    def __init__(self, result: list):

        # First check if array size is 7
        if not len(result) == 7:
            raise Exception(
                f" \"result\" in Release.__init__ should be of size \"7\", but got {len(result)} instead...")

        '''
            Array should be 7 / 8 in length
            Result[0] = Name("name")
            Result[1] = Tag("tag_name")
            Result[2] = URL("html_url")
            Result[3] = INFO("body")
            Result[4] = Create Date("created_at")
            Result[5] = Published Date("published_at")
            Result[6] = PreRelease ? true / false("prerelease")
        '''

        try:
            self._Name = result[0]
            self._Tag = result[1]
            self._URL = result[2]
            self._Info = result[3]
            self._Created = self.__parse_date(result[4])
            self._Published = self.__parse_date(result[5])
            self._isPreRelease = True if result[6] == "true" or result[6] == "true" else False
            self._parsedCorrectly = True
        except (ValueError, Exception):  # <- PyCharm throws "too broad exception" if it doesn't include this
            self._parsedCorrectly = False
        self._Assets = []  # Create new list (empty for now)

    def add_asset(self, assets: Asset):
        if self._Assets.__contains__(assets):
            print(f"[INFO] Ignoring {assets.item_name()} as it is already included into the list")
        else:
            self._Assets.append(assets)

    def get_assets(self):
        return self._Assets

    def get_name(self):
        return self._Name

    def get_tag(self):
        return self._Tag

    def get_url(self):
        return self._URL

    def get_description(self):
        return self._Info

    def get_created_date(self):
        return self._Created

    def get_published_date(self):
        return self._Published

    def get_is_prerelease(self) -> bool:
        return self._isPreRelease

    def get_is_parsedcorrectly(self) -> bool:
        return self._parsedCorrectly


''' Testing goes here

test_data = [
    "Beta Build 0.4.2",
    "0.4.2",
    "https://github.com/TheE7Player/CSGO-Event-Viewer/releases/tag/0.4.2",
    "# Beta build 0.4.2 ... ...",
    "2019-12-08T15:39:26Z",
    "2019-12-08T15:44:45Z",
    "false"
]

e = Release(test_data)

print(f"STATUS: {e.get_is_parsedcorrectly()}")
print(f"Name: {e.get_name()}")
print(f"Tag: {e.get_tag()}")
print(f"Url: {e.get_url()}")
print(f"Info: {e.get_description()}")
print(f"Date Created: {e.get_created_date()}")
print(f"Date Public: {e.get_published_date()}")
print(f"Prerelease: {e.get_is_prerelease()}")

i1 = Asset('Item_1', 'URL HAHAHA', 2951456)
i2 = Asset('Item_2', 'URL HAHAHA', 2951456)
e.add_asset(i1)
e.add_asset(i2)
nList = e.get_assets()
'''
