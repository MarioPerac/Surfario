using CefSharp.Wpf;
using CefSharp;
using Surfario.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Surfario
{
    /// <summary>
    /// Interaction logic for BookmarkWindow.xaml
    /// </summary>
    public partial class BookmarkWindow : Window
    {
        private Bookmark bookmark;
        private List<Bookmark> bookmarks;
        public BookmarkWindow(Bookmark bookmark, List<Bookmark> bookmarks)
        {
            InitializeComponent();
            this.bookmark = bookmark;
            this.bookmarks = bookmarks;
            BookmarkName.Text = bookmark.Name;
        }

        private void AddBookmark(object sender, RoutedEventArgs e)
        {
            bookmark.Name = BookmarkName.Text;

            if (bookmarks.Any(b => b.Equals(bookmark)))
            {
                MessageBox.Show("Bookmark ALREADY EXIST.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                bookmarks.Add(bookmark);
                BookmarkManager.SaveBookmarksToFile(bookmarks);
                MessageBox.Show("Bookmark ADDED SUCCESSFULLY!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Close();
        }

        private void RemoveBookmark(object sender, RoutedEventArgs e)
        {
            int countRemoved = bookmarks.RemoveAll(b => b.Equals(bookmark));
            if (countRemoved == 1)
            {
                BookmarkManager.SaveBookmarksToFile(bookmarks);
                MessageBox.Show("Bookmark REMOVED SUCCESSFULLY!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Bookmark DOES NOT EXIST.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Close();
        }
    }
}
