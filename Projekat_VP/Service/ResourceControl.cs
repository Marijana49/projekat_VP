using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ResourceControl
    {
        struct WordInformation
        {
            public string word { get; }
            public int count { get; }
            public WordInformation(string word, int count)
            {
                this.word = word;
                this.count = count;
            }

            public override string ToString()
            {
                return "Word : " + word + " Count : " + count;
            }
        }
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
                        tm.AddTextToFile("Prvi red...");
                        // simulacija prekida / greške usred prenosa
                        throw new Exception("Simulirani prekid veze!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Došlo je do greške: " + ex.Message);
                }
                using (var reader = new StreamReader(testFile))
                {
                    Console.WriteLine("Fajl dostupan nakon greške. Sadržaj:");
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }
    }
}
