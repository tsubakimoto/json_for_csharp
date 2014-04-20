using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using json_for_csharp.common;
using System.Net;
using Newtonsoft.Json;

namespace itunes_wpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string URL = "http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/wa/wsSearch?term={0}&country=jp&media=music&entity=album&limit=20&lang=ja_jp";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            listBox.ItemsSource = null;

            if (string.IsNullOrEmpty(txtArtist.Text.Trim())) return;

            var term = HttpUtility.UrlEncode(txtArtist.Text, Encoding.UTF8);
            var root = new RootObject();

            try
            {
                var json = await GetJson(string.Format(URL, term));
                root = JsonConvert.DeserializeObject<RootObject>(json);
            }
            catch (WebException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            ListSong(root);
        }

        private async Task<string> GetJson(string url)
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            return await client.DownloadStringTaskAsync(url);
        }

        private void ListSong(RootObject root)
        {
            if (root == null) return;

            var source = new List<ImageListItem>();
            foreach (var result in root.Results)
            {
                source.Add(CreateSource(result));
            }
            listBox.ItemsSource = source;
        }

        private ImageListItem CreateSource(Result result)
        {
            var item = new ImageListItem()
            {
                ArtistName = result.ArtistName,
                CollectionName = result.CollectionName,
                ArtworkUrl100 = result.ArtworkUrl100
            };
            return item;
        }
    }
}
