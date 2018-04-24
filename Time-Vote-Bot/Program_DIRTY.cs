using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System;

namespace Time_Vote_Bot.Properties
{
    using static Native;


    public unsafe static class Program_DIRTY
    {
        public const string URL = "http://time.com/5215736/time-100-2018-reader-poll/?utm_campaign=apester";
        public static readonly Regex TITLE_REGEX = new Regex($@"vote.*{DateTime.Now.Year}.*time.*\|.*time", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly string[] OK_NAMES =
        {
            // ALL OTHER NAMES ARE BEING IMPLICITLY DECLINED

            "bill.*gates", // ex microsoft ceo
            "dara.*hosrowshahi", // uber ceo
            "tim.*cook", // apple ceo
            "pony.*ma", // tencent ceo
            "jos.*andr.*s", // josé andrés, chef
            "jared.*kushner", // trump sa
            "pope.*francis", // satan's embassador
            "shinzo.*abe", // nippon
            "ivanka.*trump", // angel
            "mohamm.*d.*bin.*salam", // saudi arabia
            "rodrigo.*duterte", // philippines
            "scott.*pruitt", // head of epa
            "chloe.*kim", // hot
            "elon.*musk", // tesla spacex ceo
            "catherine.*duchess.*cambridge", // hot
            "jeff.*sessions", // ag
            "recep.*tayyip.*erdo.*an", // turks
            "ayatullah.*khamenei", // iran
            "vladimir.*putin", // /ourboi/
            "xi.*jinping", // based gook
            "paul.*ryan", // why not?
            "prince.*william", // royal
            "donald.*trump", // god emperor
            "kim.*jong.*un", // rocket man
            "ester.*ledeck.*", // maybe hot - maybe not
            "stephen.*king", // good author
        };
        public static readonly Regex OK_REGEX = new Regex($"({string.Join("|", OK_NAMES)})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        public static long _vcount = 0;


        public static void Main()
        {
start:
            bool success = false;
            voidptr pid, hwnd = null;
            string tmpdir = new FileInfo(typeof(Program_DIRTY).Assembly.Location).Directory.FullName + "/.tmp";

            if (!Directory.Exists(tmpdir))
                Directory.CreateDirectory(tmpdir);

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Progra~2\Google\Chrome\Application\chrome.exe",
                    Arguments = $"--app=\"{URL}\" --incognito --disable-web-security --user-data-dir=\"{tmpdir}\"",
                }
            })
            {
                process.Start();
                process.WaitForInputIdle();

                Thread.Sleep(3000);

                pid = process.Handle;
            }

            if (pid)
                hwnd = Array.Find(GetAllWindows(), x => TITLE_REGEX.IsMatch(x.title)).hwnd;
            else
            {
                Console.WriteLine("No process could be created.");

                return;
            }

            if (hwnd)
            {
                Console.WriteLine("Main window: " + hwnd);

                SetWindowPos(hwnd, null, 2000, 10, 420, 800, 0x0040);
                SendKeysTo(hwnd, "+^J");

                Thread.Sleep(5000);

                voidptr devtools = Array.Find(GetAllWindows(), x => x.title.Contains(URL) && x.title.Contains("Developer Tools")).hwnd;

                if (devtools)
                {
                    Console.WriteLine("Dev tools window: " + devtools);

                    DoStuff(pid, hwnd, devtools);
                    SendMessage(devtools, 0x10, null, null);
                    SendKeysTo(devtools, "^W");
                    DestroyWindow(devtools);

                    success = true;
                }
                else
                    Console.WriteLine("Dev tools window not found.");

                SendMessage(hwnd, 0x10, null, null);
                SendKeysTo(hwnd, "^W");
                DestroyWindow(hwnd);
            }
            else
                Console.WriteLine("Main window not found.");

            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
            else if (success)
                goto start;
            else if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit ...");
                Console.ReadKey(true);
            }
        }

        private static void DoStuff(voidptr pid, voidptr main, voidptr devtools)
        {
            const string add_jquery = "let sc=document.createElement(\"script\");sc.setAttribute('src','//code.jquery.com/jquery-3.3.1.min.js');sc.addEventListener('load',function(){let sc=document.createElement('script');document.body.appendChild(sc);},false);document.body.appendChild(sc);\n";
            const string iframe_jq_sel = "$('iframe.tempo-iframe-embed')";

            string tmpID = $"__tmpid_{Guid.NewGuid():N}";
            string iframe_jq_isel = $"$($('#{tmpID}')[0].contentDocument)";

            SetWindowPos(devtools, null, 0, 10, 1000, 800, 0x0040);

            Thread.Sleep(2000);

            ShowWindow(devtools, 1);

            Thread.Sleep(500);

            ShowWindow(devtools, 3);

            Thread.Sleep(1000);

            devtools.Focus();

            SendKeysTo(devtools, "^l");
            SendKeysTo(devtools, "^u");
            SendTextTo(devtools, add_jquery);

            Thread.Sleep(300);

            SendTextTo(devtools, $"{iframe_jq_sel}[0].scrollIntoView(true);\n");
            SendTextTo(devtools, $"{iframe_jq_sel}.attr('id','{tmpID}');\n");

            //SendTextTo(devtools, $"inspect({iframe_jq_sel}[0]);\n");
            //SendKeysTo(devtools, "^ö");

            devtools.Focus();

            Thread.Sleep(500);

            LeftMouseClick(100, 90);
            SendKeysTo(devtools, "T");
            SendKeysTo(devtools, "{ENTER}");

            for (int i = 0; i < 2; ++i)
            {
                Thread.Sleep(500);
                LeftMouseClick(100, 90);
                SendKeysTo(devtools, "T");

                for (int j = 0; j < 2; ++j)
                {
                    Thread.Sleep(100);

                    SendKeysTo(devtools, "{DOWN}");
                }

                Thread.Sleep(100);

                SendKeysTo(devtools, "{ENTER}");
            }

            Thread.Sleep(500);

            LeftMouseClick(100, 800);

            SendKeysTo(devtools, "^l");
            SendTextTo(devtools, "$('title');\n");
            SendTextTo(devtools, "// We are inside iframe nao!\n");

            const string start_jq_sel = "$('body>div>div.main.animated-view.ng-scope>div>div>div.story-slide.canvas.animate.ng-scope>div.slide-pager.ng-isolate-scope>span.nav-button.forward.first-slide-arrows>div>i.ic.icon-arrow-right.ng-scope')";

            SendTextTo(devtools, add_jquery);
            SendTextTo(devtools, $"{start_jq_sel}.click();\n");
            SendTextTo(devtools, "// started dem shitty poll ...\n");

            Thread.Sleep(500);

            main.Focus();

            Thread.Sleep(1000);

            SetWindowPos(main, null, 1400, 100, 420, 700, 0x0040);

            devtools.Focus();

            const string s_acc = "__accept";
            const string s_dec = "__decline";
            const string s_text = "__getname";

            SendCodeTo(devtools, $@"
var shutdown;
function clickresponse(index)
{{
    var c = [];
    $('div.render-image__item.ng-isolate-scope').each(function() {{
        var e = $(this);
        if ((e.width() == 60) && (e.height() == 60))
            c.push(e);
    }});
    var s = c[index];
    if (s === undefined) shutdown();
    else s.click();
}}
function {s_text}()
{{
    return $('.render__item.render-text.ape-rich-text.ape-rich-text--renderer.ape-rich-text--renderer-story.ng-binding.ng-scope.ng-isolate-scope.active-text-animations').text();
}}
var {s_acc} = function() {{ clickresponse(0) }};
var {s_dec} = function() {{ clickresponse(1) }};
");

            DoDecisions(pid, main, devtools, s_text, s_acc, s_dec);

            Console.WriteLine("Finished Voting!");
        }

        private static void SendCodeTo(voidptr devtools, string js) => SendTextTo(devtools, js.Replace("\r", "").Replace("    ", "\t").Replace("\t", "").Trim() + '\n');

        private static void DoDecisions(voidptr pid, voidptr main, voidptr devtools, string s_text, string s_acc, string s_dec)
        {
            HttpListener listener = new HttpListener();
            const int port = 1488;
            bool open = true;

            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            SendCodeTo(devtools, $@"
shutdown = function() {{
    var xhr = new XMLHttpRequest();
    xhr.open('GET', 'http://localhost:{port}?shutdown=0', true);
    xhr.send();
}}
function requestresp() {{
    var xhr = new XMLHttpRequest();
    try {{
        var name = encodeURI({s_text}());
        xhr.open('GET', 'http://localhost:{port}?name=' + name, true);
        xhr.onload = function() {{
            var responseText = xhr.responseText;
            console.log(""voting for '"" + {s_text}() + ""' with '"" + responseText + ""'"");
            if (responseText === 'YES') {s_acc}();
            else {s_dec}();
        }};
        xhr.send();
    }} catch (e) {{
        shutdown();
    }}
}}
");

            do
            {
                ++_vcount;

                SendTextTo(devtools, "requestresp();\n");

                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string name = request.QueryString["name"];
                bool accpt = OK_REGEX.IsMatch(name ?? "");

                Console.WriteLine($"Vode no. {_vcount,20}:   Voting on '{name}' with {(accpt ? "YES" : "NO")}.");

                open = request.QueryString["shutdown"] is null;

                byte[] buffer = Encoding.UTF8.GetBytes(open ? accpt ? "YES" : "NOPE" : "Commiting Sudoku ...");

                response.StatusCode = open ? 200 : 300;
                response.StatusDescription = open ? "OK." : "Shutting down.";
                response.ContentLength64 = buffer.Length;
                
                using (Stream os = response.OutputStream)
                    os.Write(buffer, 0, buffer.Length);

                Thread.Sleep(2000);
            }
            while (open);

            listener.Stop();
        }
    }
}


/*
 
function requestresp()
{
    var name = encodeURI('{jq_name}');
    var xhr = new XMLHttpRequest();

    xhr.open('GET', 'http://localhost:{port}?name=' + name, true);
    xhr.onload = function() {
        var responseText = xhr.responseText;

        if (responseText === 'YES')
            '{jq_acc}';
        else
            '{jq_dec}';
    };
    xhr.onerror = function() {
        console.log("there was an error!");
    };
    xhr.send();
}



 */
