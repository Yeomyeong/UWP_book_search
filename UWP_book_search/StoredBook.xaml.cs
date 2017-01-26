using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using Windows.UI.Xaml;
using Windows.UI.Notifications;

namespace UWP_book_search
{
    public sealed partial class StoredBook
    {
        public SQLiteConnection SQLCon;

        public StoredBook()
        {
            this.InitializeComponent();

            SQLCon = App.SQLCon;
            DataLoading();
        }

        private void DataLoading()
        {
            using (var statement = SQLCon.Prepare("SELECT Id, Title, Category, ImageUrl, description FROM Books"))
            {
                List<BookItem> books = new List<BookItem>();

                while (statement.Step() == SQLiteResult.ROW)
                {
                    BookItem book = new BookItem();
                    book.Id = (Int64)statement[0];
                    book.Title = (string)statement[1];
                    book.Category = (string)statement[2];
                    book.ImageUrl = (string)statement[3];
                    book.Description = (string)statement[4];

                    books.Add(book);
                }
                lstBooks.ItemsSource = books;

                if (lstBooks.Items.Count != 0)
                {
                    lstBooks.SelectedIndex = 0;
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (lstBooks.SelectedItem == null)
                return;

            var bookItem = (BookItem)lstBooks.SelectedItem;
            Int64 id = bookItem.Id;

            var statement = SQLCon.Prepare("DELETE FROM Books WHERE Id=?");
            statement.Bind(1, id);
            statement.Step();
            DataLoading();

            //삭제 알림 토스트
            var notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var toastElement = notificationXml.GetElementsByTagName("text");
            toastElement[0].AppendChild(notificationXml.CreateTextNode(bookItem.Title + "이(가) 삭제되었습니다."));
            var toastNotification = new ToastNotification(notificationXml);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }
    }
}
