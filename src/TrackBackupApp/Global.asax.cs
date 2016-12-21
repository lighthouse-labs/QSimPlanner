﻿using System;
using System.Web;
using System.IO;
using System.Net;
using System.Web.Caching;
using QSP.RouteFinding.Tracks.Nats;

namespace TrackBackupApp
{
    public class Global : HttpApplication
    {
        private const string DummyPageUrl = "http://qsimplan.somee.com/DummyPage.aspx";
        private const string DummyCacheItemKey = "dummyKey";

        private bool RegisterCacheEntry()
        {
            if (HttpContext.Current.Cache[DummyCacheItemKey] != null)
            {
                return false;
            }

            var onRemove = new CacheItemRemovedCallback(CacheItemRemovedCallback);

            HttpContext.Current.Cache.Add(DummyCacheItemKey,
                                          "Test",
                                          null,
                                          DateTime.Now.AddMinutes(15),
                                          Cache.NoSlidingExpiration,
                                          CacheItemPriority.Normal,
                                          onRemove);

            WriteToLog(3);
            return true;
        }

        public void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            HitPage();
            WriteToLog(" Cache item callback, Reason: " + reason.ToString());
            DoWork();
        }

        private void HitPage()
        {
            WebClient client = new WebClient();
            client.DownloadData(DummyPageUrl);
        }

        private void DoWork()
        {
            try
            {
                SaveNats();
            }
            catch (Exception ex)
            {
                WriteToLog(ex.ToString());
            }
        }

        private void WriteToLog(int para)
        {
            var msgs = new[]
            {
                " Application is starting.",
                " Saving the NATs.",
                " Cache item callback.",
                " Cache Added."
            };
            
            WriteToLog(msgs[para]);
        }


        public void WriteToLog(string msg)
        {
            using (var wr = File.AppendText(System.Web.Hosting.HostingEnvironment.MapPath("~/log.txt")))
            {
                wr.WriteLine(DateTime.Now.ToString() + " " + msg);
            }
        }
        
        private void SaveNats()
        {
            var result = new NatsDownloader().DownloadFromNotam();

            foreach (var i in result)
            {
                string s = i.Direction == NatsDirection.East
                         ? "Eastbound"
                         : "Westbound";
                
                string filepath = "~/nats/" + s + ".xml";

                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/nats"));

                File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(filepath),
                                  i.ConvertToXml().ToString());
            }
            WriteToLog(1);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Url.ToString() == DummyPageUrl)
            {
                // Add the item in cache And when succesful, do the work.
                RegisterCacheEntry();
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Fires when the application is started
            SaveNats();
            RegisterCacheEntry();
            WriteToLog(0);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}