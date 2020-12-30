using Cinchoo.PGP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PGPEncrypt
{
    class Program
    {
        static void Main(string[] args)
        {

            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create("ftp://islandroutes.exavault.com");
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            Console.WriteLine("Connecting to FTP Server.....");
            NetworkCredential creds = new NetworkCredential("encrypt", "7mmT@XAy");
            listRequest.Credentials = creds;
            List<string> files = new List<string>();

            using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (StreamReader listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                  files.Add(listReader.ReadLine());
                }
            }

            foreach (string file in files)
            {
                string[] tokens = file.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                if (tokens[8].Length > 3)
                    downloadFTP(tokens[8],creds);
            }
            Console.Read();
        }

        static void downloadFTP(string filepath, NetworkCredential creds)
        {
            string[] s = filepath.Split(new[] { '.' });
            string ftpfilepath = "ftp://islandroutes.exavault.com/" + filepath;

            Console.WriteLine("Retrieving file: " + filepath);


            string inputfilepath = @"C:\ftp_down\" + filepath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = creds;
                byte[] fileData = request.DownloadData(ftpfilepath);

                using (FileStream file = File.Create(inputfilepath))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
               // Console.Write(" Completed");
              //  Console.WriteLine("");
                encrypt_file(inputfilepath,filepath);
            }
        }

        static void encrypt_file(string localpath,string filename)
        {
            Console.WriteLine("Encrypting file " + filename);

            try
              {
                using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
                {
                    pgp.EncryptFile(localpath, @"c:\ftp_down\" + filename + ".PGP", @"c:\ftp_down\pkey.asc", true, false);
                }

                Console.WriteLine("File encrypted and available.....");
                File.Delete(@"c:\ftp_down\" + filename);
              }
              catch(Exception ex)
              {
                Console.WriteLine(ex.Message);
              }
        }
    }
}
