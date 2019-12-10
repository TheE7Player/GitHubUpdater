using System;
using System.Collections.Generic;
using System.Text;

namespace TheE7Player
{
    /// <summary>
    /// Class which holds the file information from Releases
    /// </summary>
    public class Assets
    {
        /// <summary>
        /// Enum to display file size to a specific size (Used to convert)
        /// </summary>
        public enum DisplaySize { Kilobytes = 2, Megabytes = 3, Gigabytes = 4};

        #region Variables/Fields
        private string itemname, itemdownloadurl;
        private double itemsize;

        public Assets(string Name, string URL, double Size) { this.itemName = Name; this.itemDownloadUrl = URL; this.itemSize = Size; }

        /// <summary>
        /// Holds the items name
        /// </summary>
        public string itemName
        {
            get { return itemname; }
            private set { itemname = value; }
        }

        /// <summary>
        /// Holds the items downloadable url link
        /// </summary>
        public string itemDownloadUrl
        {
            get { return itemdownloadurl; }
            private set { itemdownloadurl = value; }
        }

        /// <summary>
        /// Holds the item size
        /// </summary>
        public double itemSize
        {
            get { return itemsize; }
            private set { itemsize = value; }
        }
        #endregion

        #region Functions
        public string getItemSize(DisplaySize SizeType)
        {
            //Fetch amount of times to divide by from the SizeType declared
            int IterationAmount = (int)SizeType; //Fetch the enum value by casting it into an int (To receive it's assigned number)
            double initialSize = itemSize;

            for (int i = 1; i < IterationAmount; i++)
                initialSize /= 1024;

            string SizeExtension = (SizeType == DisplaySize.Kilobytes) ? "KB" : (SizeType == DisplaySize.Megabytes) ? "MB" : "GB";

            //Example return: 92.8MB
            return $"{initialSize.ToString("0.0")}{SizeExtension}";
        }
        #endregion
    }
}
