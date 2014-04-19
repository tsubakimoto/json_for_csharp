using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using json_for_csharp.common;
using Newtonsoft.Json;

namespace itunes_form
{
    public partial class Form1 : Form
    {
        private readonly string URL = "http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/wa/wsSearch?term=Perfume%20Game&media=music&limit=20";

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            var json = await GetJson();
            textBox1.Text = json;

            var root = Deserialize<RootObject>(json);
        }

        private async Task<string> GetJson()
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            return await client.DownloadStringTaskAsync(URL);
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
