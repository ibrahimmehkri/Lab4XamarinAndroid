using System;
using Android.Database.Sqlite;
using Android.Content;
using Android.Util;
using Android.Database; 

namespace Scratchpad
{
    public class DBHelper : SQLiteOpenHelper
    {
        private static readonly string TAG = "DBHelperLogs";
        public static readonly string DATABASE_NAME = "EntryDatabase.db";
        public static int DATABASE_VERSION = 1;
        public static readonly string SQL_ENTRY_TABLE = $"CREATE TABLE {DBContract.Entry.TABLE_NAME} ({DBContract.ID} INTEGER PRIMARY KEY, {DBContract.Entry.COLUMN_NAME_NAME} TEXT, {DBContract.Entry.COLUMN_NAME_FILEPATH} TEXT, {DBContract.Entry.COLUMN_NAME_DATE} TEXT, {DBContract.Entry.COLUMN_NAME_AMOUNT} REAL)";
        public static readonly string SQL_DELETE_ENTRY_TABLE = $"DROP TABLE IF EXISTS {DBContract.Entry.TABLE_NAME}";

        public DBHelper(Context context) : base(context, DATABASE_NAME, null, DATABASE_VERSION)
        {
        }

        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL(SQL_ENTRY_TABLE);
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            db.ExecSQL(SQL_DELETE_ENTRY_TABLE);
            DATABASE_VERSION = newVersion; 
            OnCreate(db); 
        }

        public int GetEntryTableRowCount()
        {
            SQLiteDatabase db = this.ReadableDatabase;
            if (db != null)
            {
                string[] columns = { DBContract.ID, DBContract.Entry.COLUMN_NAME_NAME, DBContract.Entry.COLUMN_NAME_FILEPATH };
                Android.Database.ICursor cursor = db.Query(DBContract.Entry.TABLE_NAME, columns, null, null, null, null, null);
                return cursor.Count;
            }
            else
            {
                return -1;
            }
        }

        public string GetTaxReport()
        {
            SQLiteDatabase db = this.ReadableDatabase;
            string[] columnsRequired = { DBContract.Entry.COLUMN_NAME_NAME, DBContract.Entry.COLUMN_NAME_AMOUNT, DBContract.Entry.COLUMN_NAME_DATE };
            if(db != null)
            {
                string taxReport = "";
                double totalTaxPayable = 0; 
                ICursor cursor = db.Query(DBContract.Entry.TABLE_NAME, columnsRequired, null, null, null, null, null);
                cursor.MoveToFirst(); 
                for(int i = 0; i < cursor.Count; i++)
                {
                    string name = cursor.GetString(cursor.GetColumnIndex(DBContract.Entry.COLUMN_NAME_NAME));
                    double amount = cursor.GetDouble(cursor.GetColumnIndex(DBContract.Entry.COLUMN_NAME_AMOUNT));
                    string dateString = cursor.GetString(cursor.GetColumnIndex(DBContract.Entry.COLUMN_NAME_DATE));

                    taxReport += $"Date: {dateString}, Desc: {name}, Amount: {amount}\n\n";

                    if(amount < 0)
                    {
                        totalTaxPayable += (amount * -1) * 0.25;
                    }
                    cursor.MoveToNext(); 
                }

                taxReport += $"Total payable Vat: {totalTaxPayable}";
                return taxReport; 
            }
            else
            {
                return null; 
            }
        }

        public void EmptyEntryTable()
        {
            this.OnUpgrade(this.WritableDatabase, DBHelper.DATABASE_VERSION, DBHelper.DATABASE_VERSION++);
        }
    }
}
