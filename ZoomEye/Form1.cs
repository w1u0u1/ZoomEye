using MihaZupan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ZoomEye
{
    public partial class Form1 : Form
    {
        private bool stop = false;
        private string token = null;

        public Form1()
        {
            InitializeComponent();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) => { return true; };
        }

        private void SetStatusText(string text)
        {
            toolStripStatusLabel1.Text = text;
        }

        private void EnableWindow(bool enable)
        {
            txtUser.Enabled = enable;
            txtPass.Enabled = enable;
            txtProxy.Enabled = enable;
            cboSearchPage.Enabled = enable;
            txtKey.Enabled = enable;
            cboSearchPage.Enabled = enable;
            txtStartPage.Enabled = enable;
            btnResInfo.Enabled = enable;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboType.SelectedIndex = 0;
            cboSearchPage.SelectedIndex = 9;
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowPass.Checked)
                txtPass.PasswordChar = '\0';
            else
                txtPass.PasswordChar = '*';
        }

        private void tsmiDetails_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvResult.SelectedItems)
            {
                JToken jToken = item.Tag as JToken;
                JSONViewerForm jSONViewerForm = new JSONViewerForm(jToken.ToString());
                jSONViewerForm.Show();
            }
        }

        private void tsmiExport_Click(object sender, EventArgs e)
        {
            if (lvResult.Items.Count > 0)
                lvResult.Export();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                stop = false;

                if (btnSearch.Text == "Stop")
                {
                    stop = true;
                    btnSearch.Text = "Search";
                    return;
                }

                int startPage = 1;

                if (txtUser.Text.Length == 0)
                {
                    txtUser.Focus();
                    return;
                }

                if (txtPass.Text.Length == 0)
                {
                    txtPass.Focus();
                    return;
                }

                if (txtProxy.Text.Length > 0)
                {
                    if (!txtProxy.Text.ToUpper().StartsWith("HTTP://") && !txtProxy.Text.ToUpper().StartsWith("SOCKS5://"))
                    {
                        txtProxy.Focus();
                        return;
                    }

                    var proxy = txtProxy.Text.ToUpper();
                    var webProxy = new WebProxy(proxy);
                    if (proxy.StartsWith("SOCKS5://"))
                        CONFIG.Proxy = new HttpToSocks5Proxy(webProxy.Address.Host, webProxy.Address.Port);
                    else
                        CONFIG.Proxy = webProxy;
                }

                if (txtKey.Text.Length == 0)
                {
                    txtKey.Focus();
                    return;
                }

                if (cboSearchPage.Text.Length == 0)
                {
                    cboSearchPage.Focus();
                    return;
                }

                if (txtStartPage.Text.Length > 0)
                {
                    startPage = int.Parse(txtStartPage.Text);
                    if (startPage <= 0)
                    {
                        txtStartPage.Focus();
                        return;
                    }
                }

                EnableWindow(false);

                lvResult.Items.Clear();
                lvLog.Items.Clear();

                Thread thread = new Thread(() =>
                {
                    try
                    {
                        btnSearch.Text = "Stop";

                        int page = int.Parse(cboSearchPage.Text);
                        string key = txtKey.Text;

                        SetStatusText("Getting access token ...");

                        if (token == null)
                            token = libZoomEye.GetAccessToken(txtUser.Text, txtPass.Text);

                        for (int i = startPage; !stop && i <= startPage + page; i++)
                        {
                            ListViewItem item = new ListViewItem(lvLog.Items.Count.ToString());
                            try
                            {
                                string url = "";
                                if (cboType.SelectedIndex == 0)
                                    url = "https://api.zoomeye.org/host/search?query=";
                                else
                                    url = "https://api.zoomeye.org/web/search?query=";

                                url += key + "&page=" + i;

                                item.SubItems.AddRange(new string[] { url, "" });
                                lvLog.Items.Add(item);

                                item.SubItems[2].Text = "Watting ...";
                                SetStatusText(string.Format("Watting for {0} ... {1}/{2}", url, i, page));

                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                                request.Headers.Add("Authorization", "JWT " + token);
                                request.Proxy = CONFIG.Proxy;

                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                Stream s = response.GetResponseStream();

                                string encoding = response.ContentEncoding;
                                if (encoding == null || encoding.Length < 1)
                                    encoding = "UTF-8";

                                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                                string retString = reader.ReadToEnd();
                                JObject newObj1 = (JObject)JsonConvert.DeserializeObject(retString);
                                JArray jar = JArray.Parse(newObj1["matches"].ToString());
                                if (jar.Count != 0)
                                {
                                    foreach (var data in jar)
                                    {
                                        ListViewItem item1 = new ListViewItem(lvResult.Items.Count.ToString());
                                        item1.SubItems.AddRange(new string[] {libZoomEye.GetJsonIP(data), libZoomEye.GetJsonCountry(data),
                                    libZoomEye.GetJsonTitle(data), libZoomEye.GetJsonOS(data), libZoomEye.GetJsonSite(data), libZoomEye.GetJsonServer(data) }); ;
                                        item1.Tag = data;
                                        lvResult.Items.Add(item1);
                                    }
                                }

                                var totalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(newObj1["total"].ToString()) / 20));
                                if (totalPage < page)
                                    page = totalPage;

                                toolStripStatusLabel2.Text = "TotalPage: " + totalPage.ToString();

                                item.SubItems[2].Text = "OK";
                            }
                            catch (Exception ex)
                            {
                                item.SubItems[2].Text = ex.Message;
                            }
                        }

                        SetStatusText("Finish");
                    }
                    catch (Exception ex)
                    {
                        SetStatusText(ex.Message);
                    }

                    btnSearch.Text = "Search";
                    EnableWindow(true);
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                SetStatusText(ex.Message);
            }
        }

        private void btnResInfo_Click(object sender, EventArgs e)
        {
            if (txtUser.Text.Trim().Length == 0)
            {
                txtUser.Focus();
                return;
            }

            if (txtPass.Text.Trim().Length == 0)
            {
                txtPass.Focus();
                return;
            }

            Thread thread = new Thread(() =>
            {
                try
                {
                    SetStatusText("Getting access token ...");
                    string token = libZoomEye.GetAccessToken(txtUser.Text, txtPass.Text);

                    SetStatusText("Getting resources info ...");
                    string info = libZoomEye.GetResourcesInfo(token);

                    SetStatusText("Finish");
                    MessageBox.Show(info);
                }
                catch (Exception ex)
                {
                    SetStatusText(ex.Message);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void lvResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && lvResult.SelectedItems.Count > 0)
                tsmiDetails_Click(sender, e);
        }
    }
}