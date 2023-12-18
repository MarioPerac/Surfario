using CefSharp.Wpf;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Surfario.service;

namespace Surfario
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private string URIPrefix = "https://";
        private string defaultURI = "www.google.com/";
        private Dictionary<int, ChromiumWebBrowser> tabBrowserAssociations = new Dictionary<int, ChromiumWebBrowser>();
        private TabItem currentTab;
        private static ChromiumWebBrowser currentBrowser;
        private static int TabId = 0;
        private static List<Bookmark> bookmarks;


        public MainWindow()
        {
            Cef.Initialize(new CefSettings());
            InitializeComponent();
            bookmarks = BookmarkManager.LoadBookmarksFromFile();
            loadBookmarks();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                AddTab();

            } catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void loadBookmarks()
        {
            bookmarks = BookmarkManager.LoadBookmarksFromFile();
            BookmarksStackPanel.Children.Clear();
            foreach (Bookmark bookmark in bookmarks)
            {
                Button button = new Button { Content = bookmark.Name, Height=30, Width=100, Tag = bookmark.Address, Background=Brushes.White, BorderThickness= new Thickness(0,0,1,0) };
                void buttonClickHandler(object sender, RoutedEventArgs e)
                {
                    currentBrowser.Address = URIPrefix + bookmark.Address;
                }
                button.Click += buttonClickHandler;
                BookmarksStackPanel.Children.Add(button);
            }
        }


        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (currentBrowser.CanGoBack)
            {

                currentBrowser.Back();
            }
        }


        private void GoForward(object sender, RoutedEventArgs e)
        {
            if (currentBrowser.CanGoForward)
            {
                currentBrowser.Forward();

            }
        }

        private void HandleLoadStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            if (!args.IsLoading)
            {

                Dispatcher.Invoke(() =>
                {

                    if (!URITextBox.Text.Equals(currentBrowser.Address.Substring("https://".Length)))
                    {
                        URITextBox.Text = currentBrowser.Address.Substring("https://".Length);
                    }
                    if (URITextBox.Text.Contains(defaultURI))
                    {
                        BookmarksGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BookmarksGrid.Visibility = Visibility.Collapsed;
                    }
                    setBookmarkImage();

                    currentBrowser.EvaluateScriptAsync("document.title")
                        .ContinueWith(titleTask =>
                        {
                            if (!titleTask.IsFaulted)
                            {
                                var response = titleTask.Result;

                                if (response.Success && response.Result != null)
                                {
                                    string title = response.Result.ToString();
                                    Dispatcher.Invoke(() =>
                                    {
                                       
                                        UpdateTextBlockTitle(title);
                                        UpdateButtonStates();
                                        
                                    });
                                }
                            }
                        });
                });
            }
        }

        private void UpdateTextBlockTitle(string title)
        {
            if (currentTab.DataContext is TextBlock textBlock)
            {
                textBlock.Text = title;
            }
        }
        private void KeyDownSearchBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    currentBrowser.Address = URIPrefix + URITextBox.Text;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void CurrentBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                UpdateButtonStates();
            }
        }


        private void UpdateButtonStates()
        {
            Dispatcher.Invoke(() =>
            {
                leftArrowButton.IsEnabled = currentBrowser.CanGoBack;
                rightArrowButton.IsEnabled = currentBrowser.CanGoForward;
            });
        }


        private void ReloadPage(object sender, RoutedEventArgs e)
        {
            currentBrowser.Reload();
        }

        private void AddTab(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private TabItem InitializeTab()
        {
            TabItem tab = new TabItem
            {
                Tag = TabId,
            };

            TextBlock textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Padding = new Thickness(10, 0, 0, 0)
            };
            Button button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 22,
                Height = 22
            };

            textBlock.Text = "New Tab";
            tab.DataContext = textBlock;
            void CloseTabButton(object sender, RoutedEventArgs e)
            {
                TabControl.Items.Remove(tab);
                tabBrowserAssociations.Remove((int)tab.Tag);
                if (TabControl.Items.Count == 0)
                {
                    Close();
                }

            }

            button.Click += CloseTabButton;
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
            button.Content = new Image
            {
                Source = new BitmapImage(new Uri("images/close.png", UriKind.RelativeOrAbsolute)),
                Width = 20,
                Height = 20
            };

            Grid tabGrid = new Grid
            {
                Width = 220,
                Height = 40,
            };
            tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(button, 1);

            tabGrid.Children.Add(textBlock);
            tabGrid.Children.Add(button);
            tab.Header = tabGrid;

            return tab;
        }
        private void AddTab()
        {
            TabItem tab = InitializeTab();

            TabControl.Items.Add(tab);
            TabControl.SelectedItem = tab;

            leftArrowButton.IsEnabled = false;
            rightArrowButton.IsEnabled = false;

          

            var browser = new ChromiumWebBrowser();
            browser.Address = URIPrefix + defaultURI;
            browser.AddressChanged += Browser_AddressChanged;
            EventHandler<LoadingStateChangedEventArgs> loadingStateChangedHandler = HandleLoadStateChanged;
            browser.LoadingStateChanged += loadingStateChangedHandler;
            currentBrowser = browser;
            currentTab = tab;
            BrowserGrid.Children.Add(browser);
            URITextBox.Text = defaultURI;
            tabBrowserAssociations.Add((int)tab.Tag, browser);
            BookmarksGrid.Visibility = Visibility.Visible;
            BookmarkImage.Source = new BitmapImage(new Uri("images/bookmark.png", UriKind.RelativeOrAbsolute));
            TabId++;
        }
        private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            URITextBox.Text = currentBrowser.Address.Substring("https://".Length);

        }
        private void SwitchTab(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = TabControl.SelectedItem as TabItem;

            if (selectedTab != null)
            {
                int tag = (int)selectedTab.Tag;

                if (tabBrowserAssociations.ContainsKey(tag))
                {

                    currentTab = selectedTab;
                    currentBrowser = tabBrowserAssociations[tag];
                    URITextBox.Text = currentBrowser.Address.Substring("https://".Length);
                    if (URITextBox.Text.Contains(defaultURI))
                    {
                        BookmarksGrid.Visibility = Visibility.Visible;
                        
                    }
                    else
                    {
                        BookmarksGrid.Visibility = Visibility.Collapsed;
                    }
                    setBookmarkImage();
                    UpdateButtonStates();
                    foreach (ChromiumWebBrowser browser in BrowserGrid.Children)
                    {
                        if (browser == currentBrowser)
                        {
                            
                            browser.Visibility = Visibility.Visible;
                            
                        }
                        else
                        {
                            browser.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void BookmarkPage(object sender, RoutedEventArgs e)
        {
            string name = ((TextBlock)currentTab.DataContext).Text;
            string address = URITextBox.Text;

            foreach (Button button in BookmarksStackPanel.Children)
            {
                if(button.Tag.ToString().Equals(address))
                {
                    name = (string)button.Content;
                }
            }
            
            BookmarkWindow bookmarkWindow = new BookmarkWindow(new Bookmark(name, address), bookmarks);
            bookmarkWindow.ShowDialog();
            loadBookmarks();
        }

        private void setBookmarkImage()
        {
            Bookmark bookmark = new Bookmark("", URITextBox.Text);
            if (bookmarks.Contains(bookmark))
            {
                BookmarkImage.Source = new BitmapImage(new Uri("images/bookmarkSelected.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                BookmarkImage.Source = new BitmapImage(new Uri("images/bookmark.png", UriKind.RelativeOrAbsolute));
            }
            
        }
    }
}
