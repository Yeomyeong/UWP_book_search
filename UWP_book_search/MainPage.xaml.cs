using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Net.Http;
using System.Diagnostics;
using System.Xml.Linq;
using SQLitePCL;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace UWP_book_search
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string BOOK_SEARCH_URL = "https://apis.daum.net/search/book?apikey={0}&q={1}&output={2}";
        private string API_KEY = ""; //다음 검색 API KEY - https://developers.daum.net 에서 발급
        private string OUTPUT = "xml";

        public SQLiteConnection SQLCon;

        public MainPage()
        {
            this.InitializeComponent();

            SQLCon = App.SQLCon;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private async void txtSearch_Click(object sender, RoutedEventArgs e)
        {
            string temp = string.Format(BOOK_SEARCH_URL, API_KEY, txtKeyword.Text, OUTPUT);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(temp);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            //Debug.WriteLine(responseBody);

            XDocument xDoc = XDocument.Parse(responseBody);

            var books = from BookItem in xDoc.Descendants("item")
                        select new BookItem
                        {
                            Title = (string)BookItem.Element("title"),
                            Category = (string)BookItem.Element("category"),
                            ImageUrl = (string)BookItem.Element("cover_l_url"),
                            Description = (string)BookItem.Element("description")
                        };
            var booklist = books.ToList(); //이 시점에 쿼리를 수행
            lstBooks.ItemsSource = booklist;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (lstBooks.SelectedItem == null)
                return;

            var bookItem = (BookItem)lstBooks.SelectedItem;

            try
            {
                using (var book = SQLCon.Prepare("INSERT INTO Books(Title, Category, ImageUrl, Description) VALUES(?,?,?,?)"))
                {
                    book.Bind(1, bookItem.Title);
                    book.Bind(2, bookItem.Category);
                    book.Bind(3, bookItem.ImageUrl);
                    book.Bind(4, bookItem.Description);
                    book.Step();
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DB 에러" + ex.StackTrace);
            }
        }

        private void txtStoredBooks_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StoredBook), null);
        }
    }
}
