using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class FileHandlingService : IFileHandling
    {
        [OperationBehavior(AutoDisposeParameters = true)]
        public FileManipulationResults GetFiles(FileManipulationOptions options)
        {
            var results = new FileManipulationResults();
            try
            {
                var path = ConfigurationManager.AppSettings["path"];
                if (path == null)
                {
                    results.ResultMessage = "Path not defined";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                if (!Directory.Exists(path))
                {
                    results.ResultType = ResultType.Warning;
                    results.ResultMessage = "Folder doesn't exist";
                    return results;
                }

                string[] files = Directory.GetFiles(path);
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.StartsWith(options.KeyWord, StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            MemoryStream ms = new MemoryStream();
                            fileStream.CopyTo(ms);
                            results.MemoryStreamCollection.Add(fileName, ms);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                results.ResultType = ResultType.Failed;
                results.ResultMessage = e.Message;
            }
            results.ResultMessage = "Files sent succesfully";
            results.ResultType = ResultType.Success;
            return results;
        }
        [OperationBehavior(AutoDisposeParameters = true)]
        public FileManipulationResults SendFile(FileManipulationOptions options)
        {
            FileManipulationResults results = new FileManipulationResults();
            try
            {
                var path = ConfigurationManager.AppSettings["path"];
                if (path == null)
                {
                    results.ResultMessage = "Path not defined";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                if (!Directory.Exists(path))
                {
                    results.ResultMessage = "Folder doesn't exist";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                if (options.MemoryStream == null || options.MemoryStream.Length == 0)
                {
                    results.ResultMessage = "File is empty";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                var fullPath = Path.Combine(path, options.KeyWord);
                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    options.MemoryStream.WriteTo(fs);

                }
                results.ResultType = ResultType.Success;
                results.ResultMessage = "Filesaved succesfully";
                return results;
            }
            catch (Exception ex)
            {
                results.ResultMessage = ex.Message;
                results.ResultType = ResultType.Failed;
                return results;
            }
        }
    }
}
