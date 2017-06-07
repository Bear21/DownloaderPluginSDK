using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace downloader
{
   class DownloaderPlugin : IPlugin
   {
      private Func<Func<object, object>, object, object> GuiCallback;

      public Func<Func<object, object>, object, object> GuiThreadCallback
      {
         set
         {
            GuiCallback = value;
         }
      }

      private static Object OpenFileBrowser(Object fileName)
      {
         string extention = Path.GetExtension(fileName as string).TrimStart('.');
         SaveFileDialog sfd = new SaveFileDialog();
         sfd.Title = "Download file";
         sfd.Filter = $"{extention}|*.{extention}|All files (*.*)|*.*";
         sfd.FileName = fileName as string;
         if (sfd.ShowDialog() != DialogResult.OK)
            return null;
         return sfd.FileName;
      }

      // Returns the file path to save the file to. returns null if unknown.
      public string SelectFilePath(HttpWebResponse webResponse)
      {
         string path = null;
         string fileName = webResponse.ResponseUri.LocalPath.Substring(webResponse.ResponseUri.LocalPath.LastIndexOf('/') + 1);
         
         path = (string)GuiCallback(OpenFileBrowser, fileName);
         return path;
      }



      // Offers a webrequest to be modified before being sent.
      public void OnRequest(HttpWebRequest wr)
      {
         // wr.ServicePoint.BindIPEndPointDelegate = GetLocalEndPoint;
      }

      /*
       * Distributing the load over multiple connections:
      double connectionSpeedRatio = 3.5;
      double connectionCounter = connectionSpeedRatio;
      IPAddress[] localInterfaces = new[] { IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2") };

      IPEndPoint GetLocalEndPoint(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
      {
         connectionCounter -= 1;
         int interfaceIndex = 0;
         if (connectionCounter <= 0)
         {
            connectionCounter += connectionSpeedRatio;
            interfaceIndex = 1;
         }
         return new IPEndPoint(localInterfaces[interfaceIndex], 0);
      }*/
   }
}