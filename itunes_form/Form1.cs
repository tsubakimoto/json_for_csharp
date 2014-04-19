using json_for_csharp.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private readonly string URL = "http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/wa/wsSearch?term={0}&media=music&limit=20";

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            var term = HttpUtility.UrlEncode(txtArtist.Text, Encoding.UTF8);
            var json = await GetJson(string.Format(URL, term));
            //txtResult.Text = json;
            var root = Deserialize<RootObject>(json);
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
    }
}
