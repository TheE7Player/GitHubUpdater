package TheE7Player;

public class Assets
{
    //[NOTE] API displays the file size in bytes!
    //[NOTE] Divide by 1024 twice for MB and triple for GB

    /*
        Example: Find 97318010 byte into MB or GB
        1. 97318010 (bytes) / 1024 = 95,037.119... (Kilobytes)
        2. 95,037.119... (Kilobytes) / 1024 = (92.8)... (Megabytes) <--
        3. 92.8... (Megabytes) / 1024 = (0.09)0... (Gigabytes) <--
     */

    public enum DisplaySize
    {
        //https://stackoverflow.com/a/1067371 <- Java enum with numbers assigned

        Kilobytes(2),
        Megabytes(3),
        Gigabytes(4);

        private final int id;
        DisplaySize(int id) { this.id = id; }
        public int getValue() { return id; } // <- Use this to retrieve amount of times to divide by 1024
    }

    private String itemName; //<- Holds items name
    private String itemDownloadUrl; //<- Holds the items 'individual name'
    private double itemSize; //<- Holds the items size

    //Constructor to assign the private fields
    public Assets(String Name, String URL, double Size)
    {
        this.itemName = Name; this.itemDownloadUrl = URL; this.itemSize = Size;
    }

    //Getters to fetch the information
    public String getItemName() {
        return itemName;
    }
    public String getItemURL() {
        return itemDownloadUrl;
    }
    public String getItemSize(DisplaySize SizeType)
    {
        //Fetch amount of times to divide by from the SizeType declared
        int IterationAmount = SizeType.getValue();
        double initialSize = itemSize;

        for (int i = 1; i < IterationAmount; i++)
            initialSize /= 1024;

        String SizeExtension = (SizeType == DisplaySize.Kilobytes) ? "KB" :  (SizeType == DisplaySize.Megabytes) ? "MB" : "GB";

        return String.format("%.1f%s", initialSize, SizeExtension);
    }
}
