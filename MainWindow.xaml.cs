using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Validator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinaryFormatter bf = new BinaryFormatter();

        ObservableCollection<VerifiedPage> Pages = new ObservableCollection<VerifiedPage>();

        public MainWindow()
        {
            InitializeComponent();

            LoadData();
            lstPages.ItemsSource = Pages;
        }

        void LoadData()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var exists = store.FileExists("pages.bin");
                if (exists)
                {
                    try
                    {
                        using (var file = store.OpenFile("pages.bin", System.IO.FileMode.Open))
                        {
                            var pages = bf.Deserialize(file) as VerifiedPage[];
                            Pages.AddRange(pages);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        void SaveData()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = store.OpenFile("pages.bin", System.IO.FileMode.Create))
                {
                    bf.Serialize(file, Pages.ToArray());
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtSitemap.Text.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                XNamespace ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var sitemap = XDocument.Load(txtSitemap.Text);
                var urls = sitemap.Element(ns + "urlset").Elements(ns + "url").Select(a => a.Element(ns + "loc").Value);
                foreach (var item in urls)
                {
                    if (!Pages.Any(a => a.Url.Equals(item, StringComparison.OrdinalIgnoreCase)))
                    {
                        Pages.Add(new VerifiedPage()
                        {
                            Url = item,
                            LastChecked = DateTime.Now.AddYears(-1)
                        });
                    }
                }
            }
            else
            {
                Pages.Add(new VerifiedPage()
                {
                    Url = txtSitemap.Text,
                    LastChecked = DateTime.Now.AddYears(-1)
                });
            }
            SaveData();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string server = ((cmbServer.SelectedItem) as ComboBoxItem).Tag.ToString();
            ThreadPool.QueueUserWorkItem((a) =>
                {
                    try
                    {
                        foreach (var item in Pages)
                        {
                            var req = HttpWebRequest.Create(item.Url) as HttpWebRequest;
                            req.IfModifiedSince = item.LastChecked;
                            req.Method = "HEAD";
                            //req.Headers[HttpRequestHeader.IfNoneMatch] = item.ETag;
                            try
                            {
                                var resp = req.GetResponse() as HttpWebResponse;
                                resp.GetResponseStream().Dispose();
                                if (!resp.Headers["ETag"].Equals(item.ETag))
                                {
                                    item.ETag = resp.Headers["ETag"];
                                    var validatorRequest = HttpWebRequest.Create(server + "check/check?uri=" + item.Url) as HttpWebRequest;
                                    validatorRequest.Pipelined = true;
                                    var validatorResponse = validatorRequest.GetResponse();
                                    item.Result = validatorResponse.Headers["x-w3c-validator-status"];
                                    validatorResponse.GetResponseStream().Dispose();
                                }
                                else
                                {
                                    //Cached
                                }
                            }
                            catch (WebException ex)
                            {
                                if ((ex.Response == null) || ((ex.Response as HttpWebResponse).StatusCode != HttpStatusCode.NotModified))
                                    throw;
                            }
                            item.LastChecked = DateTime.Now;
                        }
                        MessageBox.Show("All Checked");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    SaveData();

                });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button snd = sender as Button;
            string url = snd.Tag.ToString();
            Pages.Remove(Pages.Where(a => a.Url.Equals(url, StringComparison.OrdinalIgnoreCase)).First());
            SaveData();
        }
    }
}
