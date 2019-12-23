# Assets.py - Handles the releases files information

# Module dependencies
from enum import Enum


class Asset:
    class DisplaySize(Enum):
        Kilobytes = 2
        Megabytes = 3
        Gigabytes = 4

    # Python constructor
    def __init__(self, ItemName: str, ItemDownloadUrl: str, ItemSize: float):
        self.__item_name = ItemName
        self.__item_download_url = ItemDownloadUrl
        self.__item_size = ItemSize

    def item_size(self):
        """Returns the files size in bytes (Use get_item_size to convert to KB, MB or GB)"""
        return self.__item_size;

    def item_name(self):
        """Returns the items file name"""
        return self.__item_name;

    def item_url(self):
        """Returns the downloadable url link of the file"""
        return self.__item_download_url;

    def get_item_size(self, size_type: DisplaySize = DisplaySize.Megabytes) -> str:
        """
        Gets item's file size in certain file type format (KB, MB or GB)
        :param size_type: Size to return back as (Default = MB)
        """
        # Type checking
        if not isinstance(size_type, self.DisplaySize):
            raise TypeError('size_type must be an instance of Assets.DisplaySize')

        # Now we do our evaluation
        DivideByAmount = size_type.value  # Gets value based on what "size_type" is
        initialSize = self.__item_size

        for i in range(1, DivideByAmount):
            initialSize /= 1024

        SizeExtension = "KB" if size_type == self.DisplaySize.Kilobytes \
            else "MB" if size_type == self.DisplaySize.Megabytes \
            else "GB"

        # Return back with "initialSize" in 1dp (Decimal Place) with file type extension (KB, MB or GB)
        return f"{initialSize:.1f}{SizeExtension}"

''' CLASS TESTING GOES HERE:
# Testing file
test_obj = Asset('Item_1', 'URL HAHAHA', 2951456)

# Testing Getters
print(f"File name: {test_obj.item_name()}")
print(f"File size (bytes): {test_obj.item_size()}")
print(f"File url: {test_obj.item_url()}")

# Printing out 3 file types
print(f"File in KB: {test_obj.get_item_size(Assets.DisplaySize.Kilobytes)}")
print(f"File in MB: {test_obj.get_item_size()}")  # Default param = MB
print(f"File in GB: {test_obj.get_item_size(Assets.DisplaySize.Gigabytes)}")
'''