using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutorunZchaff
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "E:\\CNF\\";
            string output = "";
            string OutputFileName = "ZchaffOut.txt";            

            FileStream fs;

            DirectoryInfo d = new DirectoryInfo(path);

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            string fileName;

            foreach (FileInfo file in d.GetFiles())
            {
                fileName = file.Name.Split('.')[0];
                OutputFileName = fileName + ".txt";
                if (File.Exists(OutputFileName))
                {
                    File.Delete(OutputFileName);
                }
                fs = File.Create("E:\\ZchaffOut\\" + OutputFileName);

                p.Start();
                p.StandardInput.WriteLine("E:\\zchaff64\\zchaff.exe E:\\CNF\\" + file.Name + "&exit");
                p.StandardInput.AutoFlush = true;
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
                AddText(fs, output);
                AddText(fs, "\n\n\n\n");
                AddText(fs, file.Name.Split('.')[1].Replace("_"," "));
                AddText(fs, ".\n");
            }

            Console.WriteLine(output);
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }
    }
}
