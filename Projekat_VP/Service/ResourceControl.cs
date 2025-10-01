using System;
using System.IO;

namespace Service
{
    public class ResourceControl
    {
        public class TextManipulation : IDisposable
        {
            private TextWriter textWriter;
            private TextReader textReader;
            private bool disposed = false;
            string path = "";
            public string Path { get => path; }

            public TextManipulation(string path)
            {
                this.path = path;
            }

            ~TextManipulation()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (textWriter != null)
                        {
                            textWriter.Dispose();
                        }
                        if (textReader != null)
                        {
                            textReader.Dispose();
                        }
                    }
                    disposed = true;
                }
            }

            public void AddTextToFile(string text)
            {
                textWriter = File.AppendText(path);
                textWriter.WriteLine(text);
                textWriter.Close();
            }

            public string ReadAllText()
            {
                textReader = File.OpenText(path);
                string text = textReader.ReadToEnd();
                textReader.Close();
                return text;
            }

            public void DeleteAllText()
            {
                File.WriteAllText(path, string.Empty);
            }

            public static void TestDisposeProof()
            {
                string testFile = "test.csv";

                try
                {
                    using (var tm = new TextManipulation(testFile))
                    {
                        tm.AddTextToFile("First line...");
                        // simulacija prekida / greške usred prenosa
                        throw new Exception("Disconnection simulation started!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                using (var reader = new StreamReader(testFile))
                {
                    Console.WriteLine("File available after error. Content:");
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }
    }
}
