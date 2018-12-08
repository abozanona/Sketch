using Sketch.Models;
using SQLite;
using System;
using System.IO;

namespace Sketch.Classes
{
    public class AppDatabase
    {

        public static SQLiteConnection database;
        //specify DB File name here
        public static string DBFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");

        public AppDatabase()
        {
            SQLitePCL.Batteries.Init();
            database = new SQLiteConnection(DBFile);
            //create DB Models here
            //database.CreateTable<Customer>();
        }
    }
}
