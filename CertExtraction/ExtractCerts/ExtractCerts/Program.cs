using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Diagnostics;

namespace ExtractCerts
{
    class Program
    {
        static void Main(string[] args)
        {
            //X509Certificate oldcert = new X509Certificate(string.Format(@"d:\src\dns.main\private\dev\Setup\ADM\Secret\{0}", args[0]));
            //X509Certificate newcert = new X509Certificate(string.Format(@"d:\src\dns.prod\private\dev\Setup\ADM\Secret\{0}", args[0]));
            X509Certificate oldcert = new X509Certificate(args[0]);
            X509Certificate newcert = new X509Certificate(args[1]);
            DirectoryInfo di = new DirectoryInfo(args[2]);

            string issuer;
            string subjectName;
            issuer = newcert.Issuer;
            subjectName = newcert.Subject;
            Console.WriteLine("New Cert:");
            Console.WriteLine("Issuer: {0}", issuer);
            Console.WriteLine("Subject name : {0}", subjectName);

            string oldissuer;
            string oldsubjectName;
            oldissuer = oldcert.Issuer;
            oldsubjectName = oldcert.Subject;
            Console.WriteLine("Old Cert to be replaced");
            Console.WriteLine("Issuer: {0}", oldissuer);
            Console.WriteLine("Subject name : {0}", oldsubjectName);

            //DirectoryInfo di = new DirectoryInfo(@"d:\src\dns.prod\private\dev\Setup\ADM\Environment\");
            
            bool isFound = false;
            List<string> filesToEdit = new List<string>();
            foreach (DirectoryInfo d in di.GetDirectories())
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    isFound = false;
                    
                    if (fi.FullName.Contains("DNSWebsvccredentials.xml"))
                    {
                        StreamReader sr = fi.OpenText();
                        Console.WriteLine(fi.FullName);
                        StringBuilder newContent = new StringBuilder();
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            
                            if (line.Contains(oldsubjectName) && !line.Contains(oldissuer))
                            {
                                isFound = true;
                                Console.WriteLine(line);
                                line = line.Replace(oldsubjectName, subjectName);
                                int i = line.IndexOf("issuer=");
                                int end = line.IndexOf("/>");
                                string sub = line.Substring(i + 8, line.Length - i - 10);
                                Console.WriteLine("Substring to be replaced is {0}", sub);
                                line = line.Replace(sub, issuer);
                            }

                            newContent.AppendLine(line);
                        }
                        sr.Close();

                        
                        if (isFound)
                        {
                            
                            filesToEdit.Add(fi.FullName.ToString());
                            FileAttributes fas = File.GetAttributes(fi.FullName);
                            File.SetAttributes(fi.FullName, fas ^ FileAttributes.ReadOnly);
                            StreamWriter sw = new StreamWriter(fi.OpenWrite());
                            sw.WriteLine(newContent.ToString());
                            sw.Close();
                            break;
                        }
                    }
                }
            }

            Console.WriteLine(string.Join(" ", filesToEdit));
            Console.WriteLine("You need to sd edit this file:\n sd edit {0}\n", string.Join(" ", filesToEdit));

            StreamWriter sWrite = new StreamWriter(Path.Combine(args[2], "edit.cmd"));
            sWrite.Write(string.Format("sd edit {0}", string.Join(" ", filesToEdit)));
            sWrite.Close();
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = string.Format(" /c {0}", Path.Combine(args[2], "edit.cmd"));
            p.StartInfo.UseShellExecute = true;

            p.Start();
        }
    }
}
