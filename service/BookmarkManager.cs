
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Windows;

namespace Surfario.service
{
    public class BookmarkManager
    {
        public static void SaveBookmarksToFile(List<Bookmark> bookmarks)
        {
            string json = JsonConvert.SerializeObject(bookmarks);


            File.WriteAllText("bookmarks.json", json);
        }

        public static List<Bookmark> LoadBookmarksFromFile()
        {
            List<Bookmark> bookmarks = new List<Bookmark>();
            try
            {
                string json = File.ReadAllText("bookmarks.json");

                bookmarks = JsonConvert.DeserializeObject<List<Bookmark>>(json);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return bookmarks;
        }
    }
}

