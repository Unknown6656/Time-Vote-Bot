using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System;

using mshtml;

namespace Time_Vote_Bot
{
    // http://time.com/5215736/time-100-2018-reader-poll/?utm_campaign=apester

    public static class Program_TEST1
    {
        private static readonly object _lock = new object();
        private static bool _ready;


        [STAThread]
        public static void Main(string[] args)
        {
            SetFeatureBrowserEmulation();

            using (Form f = new Form
            {
                Width = 400,
                Height = 800,
            })
            using (WebBrowser wb = new WebBrowser
            {
                Dock = DockStyle.Fill,
            })
            {
                bool loaded = false;
                bool closed = false;

                f.Controls.Add(wb);
                f.Shown += (s, e) => loaded = true;
                f.Closing += (s, e) => loaded = false;
                f.FormClosed += (s, e) => closed = true;
                f.Show();

                while (loaded)
                    Application.DoEvents();

                MainThread(wb);

                while (!closed)
                    Application.DoEvents();
            }
        }

        private static void SetFeatureBrowserEmulation()
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", appName, 10000, RegistryValueKind.DWord);
        }

        private static void BlockingNavigate(this WebBrowser wb, string url)
        {
            lock (_lock)
            {
                void cmplt(object s, WebBrowserDocumentCompletedEventArgs e)
                {
                    if (e.Url == wb.Document.Url)
                        _ready = true;
                }


                _ready = false;

                wb.DocumentCompleted += cmplt;
                wb.Navigate(url);

                do
                    Application.DoEvents();
                while (!_ready);

                wb.DocumentCompleted -= cmplt;
            }
        }

        public static void Sleep(int ms)
        {
            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();

            while (sw.ElapsedMilliseconds < ms)
                Application.DoEvents();

            sw.Stop();
        }

        private static void MainThread(WebBrowser wb)
        {
            using (WebBrowserControlXPathQueriesProcessor proc = new WebBrowserControlXPathQueriesProcessor(wb))
            {
                wb.BlockingNavigate("http://time.com/5215736/time-100-2018-reader-poll/?utm_campaign=apester");

                Sleep(1000);

                HTMLIFrameClass iframe = proc.GetHtmlElement("iframe.tempo-iframe-embed") as HTMLIFrameClass;
                HTMLDocument doc = wb.Document.DomDocument as HTMLDocument;

                iframe.scrollIntoView(true);

                if ((iframe.id ?? "").Trim().Length == 0)
                    iframe.id = $"tmp_id_{Guid.NewGuid():N}";


                return;

                object ndx = 0;

                var iframedom = doc.frames.item(ref ndx) as HTMLWindow2Class;
                var innerdoc = iframedom.document as HTMLDocument;


                var ihtml = innerdoc.documentElement.innerHTML;

                // inner.innerHTML = "<b>kek</b>";
            }
        }
    }

    public class WebBrowserControlXPathQueriesProcessor
        : IDisposable
    {
        private readonly WebBrowser _wb;


        public WebBrowserControlXPathQueriesProcessor(WebBrowser webBrowser)
        {
            _wb = webBrowser;
            _wb.DocumentCompleted += Inject;

            Inject(_wb, null);
        }

        private void Inject(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_wb.Document is HtmlDocument d)
            {
                HtmlElement head = d.GetElementsByTagName("head")[0];
                HtmlElement script1 = d.CreateElement("script");
                HtmlElement script2 = d.CreateElement("script");
                HtmlElement script3 = d.CreateElement("script");

                (script1.DomElement as IHTMLScriptElement).src = "http://blog.jquery.com/wp-content/themes/jquery/js/modernizr.custom.2.6.2.min.js";
                (script2.DomElement as IHTMLScriptElement).src = "http://svn.coderepos.org/share/lang/javascript/javascript-xpath/trunk/release/javascript-xpath-latest-cmp.js";
                (script3.DomElement as IHTMLScriptElement).text = @"
function GetElements(XPath)
{
    var elems = $(XPath);
    var arr = [];

    for (var i = 0; i < elems.length; ++i)
        arr.push(elems[i]);

    return arr;

    //var xPathRes = document.evaluate(XPath, document, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);              
    //var nextElement = xPathRes.iterateNext();
    //var elements = new Object();
    //var elementIndex = 1;

    //while (nextElement)
    //{
    //    elements[elementIndex++] = nextElement;
    //    nextElement = xPathRes.iterateNext();
    //}

    //elements.length = elementIndex -1;

    //return elements;
};
";

                head.AppendChild(script1);
                head.AppendChild(script2);
                head.AppendChild(script3);
            }
        }

        public IHTMLElement GetHtmlElement(string xPathQuery)
        {
            object res = _wb.Document.InvokeScript("eval", new object[] { $"$('{xPathQuery}')[0];" });

            return res as IHTMLElement;
        }

        public IEnumerable<IHTMLElement> GetHtmlElements(string xPathQuery)
        {
            object COM = _wb.Document.InvokeScript("eval", new[] { $"GetElements('{xPathQuery}')" });
            Type type = COM.GetType();

            int length = (int)type.InvokeMember("length", BindingFlags.GetProperty, null, COM, null);

            for (int i = 1; i <= length; i++)
                yield return type.InvokeMember(i.ToString(), BindingFlags.GetProperty, null, COM, null) as IHTMLElement;
        }

        public void Dispose() => _wb.DocumentCompleted -= Inject;
    }
}

/*

var jq=document.createElement('script');jq.src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js";document.getElementsByTagName('head')[0].appendChild(jq);$(document).ready(function(){jQuery.noConflict();});


*/
