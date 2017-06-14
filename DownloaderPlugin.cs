using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace downloader
{
    class DownloaderPlugin : IGuiPlugin
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
            sfd.Filter = string.Format("{0}|*.{0}|All files (*.*)|*.*", extention);
            sfd.FileName = fileName as string;
            if (sfd.ShowDialog() != DialogResult.OK)
                return null;
            return sfd.FileName;
        }

        private Dictionary<string, string> paths = getPaths();
        private static string PATHS_FILE = "paths.txt";

        private void addPath(string pathName, string path)
        {
            // Add the new path
            paths.Add(pathName, path);
            // Save paths to disk
            File.WriteAllLines(PATHS_FILE, paths.Select(s => String.Format("{0}|{1}", s.Key, s.Value)));
        }

        private static Dictionary<string, string> getPaths()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (File.Exists(PATHS_FILE))
            {
                // Load from disk, populate and return Dictionary
                StreamReader myFileStream = File.OpenText(PATHS_FILE);
                string[] line;
                while (myFileStream.Peek() > -1)
                {
                    line = myFileStream.ReadLine().Split('|');
                    dictionary.Add(line[0], line[1]);
                }
                myFileStream.Close();
            }
            return dictionary;
        }

        // Returns the file path to save the file to. returns null if unknown.
        public string SelectFilePath(HttpWebResponse webResponse)
        {
            string path = null;
            string fileName = webResponse.ResponseUri.LocalPath.Substring(webResponse.ResponseUri.LocalPath.LastIndexOf('/') + 1);
            string remoteDirectory;
            string localDirectory = null;

            // Uncomment the following line if you want file names to be more like scene release scheme
            //fileName = fileName.Replace(" - ", "-").Replace(' ', '.').Replace("+", "");

            // Get the part before the file name to store and compare
            remoteDirectory = webResponse.ResponseUri.LocalPath.Substring(0, webResponse.ResponseUri.LocalPath.LastIndexOf('/'));

            // if the remoteDirectory (key) hasn't been seen before prompt user for save path (value) and store that
            if (!paths.TryGetValue(remoteDirectory, out localDirectory))
            {
                // If user presses cancel another plugin can try to intercept it.
                path = (string)GuiCallback(OpenFileBrowser, fileName);
                if (path != null)
                {
                    // User may have chosen a new fileName
                    fileName = path.Substring(path.LastIndexOf('\\') + 1);
                    localDirectory = path.Substring(0, path.LastIndexOf('\\'));
                    addPath(remoteDirectory, localDirectory);
                }
            }
            if (localDirectory != null)
            {
                path = string.Format("{0}\\{1}", localDirectory, fileName);
            }
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