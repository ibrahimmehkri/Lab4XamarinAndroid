using System;
namespace Scratchpad
{
    public class DBContract
    {
        public static readonly string ID = "_id";

        public DBContract()
        {
        }

        public static class Entry
        {
            public static readonly string TABLE_NAME = "entry";
            public static readonly string COLUMN_NAME_NAME = "name";
            public static readonly string COLUMN_NAME_FILEPATH = "filePath";
            public static readonly string COLUMN_NAME_DATE = "date";
            public static readonly string COLUMN_NAME_AMOUNT = "amount"; 
        }
    }
}
