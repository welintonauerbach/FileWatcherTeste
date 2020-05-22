using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcherTeste
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Monitorando Arquivos:");
            var dw = new DataWatcherService();

            if (args.Length == 0)
            {
                dw.WatchData2(@"C:\Vault_Idugel\");
            }
            else
            {
                dw.WatchData2(args[0]);
            }

            Console.ReadKey();

            var data = new StringBuilder();
            foreach (var file in dw.CreatedFiles)
            {
                data.AppendLine($"\n{file}");
            }

            foreach (var file in dw.DeletedFiles)
            {
                data.AppendLine($"\n{file}");
            }

            foreach (var file in dw.ChangedFiles)
            {
                data.AppendLine($"\n{file}");
            }

            foreach (var file in dw.RenamedFiles)
            {
                data.AppendLine($"\n{file}");
            }

            WriteLog.Write(data.ToString());
        }
    }


    public class DataWatcherService
    {
        private string _folderName = @"F:\Welinton\VSProjects\TecniCAD\TecniCAD ERP CONNECTOR\TConn\TCAD\TcConnectorClient\";
        private string _fileName = @"erpData.csv";

        public List<string> DeletedFiles;
        public List<string> CreatedFiles;
        public List<string> RenamedFiles;
        public List<string> ChangedFiles;

        public DataWatcherService()
        {
            DeletedFiles = new List<string>();
            CreatedFiles = new List<string>();
            RenamedFiles = new List<string>();
            ChangedFiles = new List<string>();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void WatchData()
        {
            var watcher = new FileSystemWatcher();

            watcher.Path = _folderName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = _fileName;
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Arquivo: {e.Name} - {e.ChangeType}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var lista = ConvertCsv.ReadFile($"{_folderName}\\{_fileName}");
            int num = 0;
            for (var index = 0; index < lista.Count; index++)
            {
                var material = lista[index];
                Console.WriteLine(
                    $"{num++} -> {material.Code}\t{material.Description}\t{material.Family}\t{material.Unity}");
            }

            stopWatch.Stop();
            Console.WriteLine($"Tempo Total: {stopWatch.ElapsedMilliseconds}");
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void WatchData2(string path)
        {
            var watcher = new FileSystemWatcher();

            //var path = @"C:\Vault_Idugel\";
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite 
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.FileName;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += OnChangedFile;
            watcher.Created += OnCreatedFile;
            watcher.Deleted += OnDeletedFile;
            watcher.Renamed += OnRenamedFile;

            watcher.EnableRaisingEvents = true;
        }

        private void OnCreatedFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Arquivo Criado: {DateTime.Now} -> {e.Name}");
            CreatedFiles.Add($"Arquivo Criado: {DateTime.Now} -> {e.Name}");
            //WriteLog.Write($"Criado: {e.Name}");

        }

        private void OnRenamedFile(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Arquivo Renomeado: {DateTime.Now} -> De: {e.OldName} -> Para: {e.Name}");
            RenamedFiles.Add($"Arquivo Renomeado: {DateTime.Now} -> De: {e.OldName} -> Para: {e.Name}");
            //WriteLog.Write($"Renomeado: {e.Name}");

        }

        private void OnDeletedFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Arquivo Deletado: {DateTime.Now} -> {e.Name}");
            DeletedFiles.Add($"Arquivo Deletado: {DateTime.Now} -> {e.Name}");
            //WriteLog.Write($"Deletado: {e.Name}");

        }

        private void OnChangedFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Arquivo Modificado: {DateTime.Now} -> {e.Name}");
            ChangedFiles.Add($"Arquivo Modificado: {DateTime.Now} -> {e.Name}");
            //WriteLog.Write($"Modificado: {e.Name}");
        }
    }

    public class Material
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Family { get; set; }
        public string Unity { get; set; }
    }

    public static class ConvertCsv
    {
        public static List<Material> ReadFile(string file)
        {
            var list = new List<Material>();
            var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = string.Empty;
            using (var str = new StreamReader(fs, Encoding.Default))
            {
                data = str.ReadToEnd();
            }
            fs.Close();

            string[] lines = data.Split('\n');

            if (lines.Length == 0)
            {
                return null;
            }
            foreach (var line in lines)
            {
                var splitedLine = line.Split(';');
                if (splitedLine.Length == 4)
                {
                    if (splitedLine[0] == "\n") continue;
                    
                    var material = new Material();
                    material.Code = splitedLine[0] ?? string.Empty;
                    material.Description = splitedLine[1] ?? string.Empty;
                    material.Family = splitedLine[2] ?? string.Empty;
                    material.Unity = splitedLine[3] ?? string.Empty;
                    list.Add(material);
                }
            }

            Console.WriteLine($"Linhas: {lines.Length}");
            return list;
        }
    }

    public static class WriteLog
    {

        public static void Write(string text)
        {
            using (var stream = new StreamWriter("LogFiles.txt",true))
            {
                stream.WriteLine($"Log -> {text}");
            }
        }
    }


}
