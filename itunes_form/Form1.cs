using json_for_csharp.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace itunes_form
{
    public partial class Form1 : Form
    {
        private readonly string URL = "http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/wa/wsSearch?term={0}&country=jp&media=music&entity=album&limit=20&lang=ja_jp";

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtArtist.Text.Trim())) return;

            var term = HttpUtility.UrlEncode(txtArtist.Text, Encoding.UTF8);
            var root = new RootObject();

            try
            {
                var json = await GetJson(string.Format(URL, term));
                root = Deserialize<RootObject>(json);
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

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        private void ListSong(RootObject root)
        {
            if (root == null) return;

            foreach (var result in root.Results)
            {
                pictureBox1.ImageLocation = result.ArtworkUrl100;
                break;
            }
        }
    }
}
