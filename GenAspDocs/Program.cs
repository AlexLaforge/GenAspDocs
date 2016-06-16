using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GenAspDocs
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("GenAspDocs - Generate html documentation from markdown comments in asp-files.");
            Console.WriteLine();
            if (args.Length != 2)
            {
                Console.WriteLine(@"Usage: genaspdocs ""inputfolder"" ""outputfolder""\n");
                Console.WriteLine();
                return -1;
            }

            var inputDirectory = new DirectoryInfo(args[0]);
            var outputDirectory = Directory.CreateDirectory(args[1]);

            var sb = new StringBuilder();
            sb.AppendLine($"Processed folder `{inputDirectory.FullName}` on {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine("# Index");
            sb.AppendLine();

            FileInfo prevFile = null;

            foreach (var inputFileInfo in inputDirectory.GetFiles("*.asp", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Processing {inputFileInfo.FullName}");
                using (var reader = inputFileInfo.OpenText())
                {
                    var docTitle = inputFileInfo.FullName.Replace(inputDirectory.FullName, "");
                    var doc = ExtractDocs(reader, docTitle);
                    if (doc.Length > 0)
                    {
                        var outputFileInfo = GetOutputFileInfo(inputFileInfo, inputDirectory, outputDirectory);
                        SaveMdContentAsMdAndHtml(outputFileInfo.FullName, doc);

                        if (prevFile == null || prevFile.DirectoryName != outputFileInfo.DirectoryName)
                        {
                            var relativeName = outputFileInfo.DirectoryName.Replace(outputDirectory.FullName, "");
                            sb.AppendLine(System.Environment.NewLine + $"## {relativeName}" + System.Environment.NewLine);
                        }

                        var relativePath = outputFileInfo
                                                .FullName
                                                .Replace(outputDirectory.FullName, "")
                                                .Substring(1)
                                                .Replace(Path.DirectorySeparatorChar,'/');
                        sb.AppendLine($"- [{relativePath}]({relativePath}.html)");
                    }
                }
                var indexFileName = Path.Combine(outputDirectory.FullName, "index");
                SaveMdContentAsMdAndHtml(indexFileName, sb.ToString());
            }

            return 0;
        }

        static Markdown mark = new Markdown();


        static void SaveMdContentAsMdAndHtml(string htmlFilenameWithoutExtension, string markdownContent)
        {
            var fi = new FileInfo(htmlFilenameWithoutExtension);
            var htmlContent = mark.Transform(markdownContent);
            fi.Directory.Create();
            File.WriteAllText(fi.FullName + ".md", markdownContent);
            File.WriteAllText(fi.FullName + ".html", htmlContent);
        }

        static FileInfo GetOutputFileInfo(FileInfo InputFile, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory)
        {
            var inputDirUrl = inputDirectory.FullName;
            var outputDirUrl = outputDirectory.FullName;
            var inputFileUrl = InputFile.FullName;
            if (!inputFileUrl.StartsWith(inputDirUrl))
            {
                throw new InvalidOperationException($"WTF Path stuff: \nInputFile:{inputFileUrl}\nInputDir{inputDirUrl}");
            }
            var outputFileUrl = outputDirUrl + inputFileUrl.Substring(inputDirUrl.Length);
            var extension = InputFile.Extension;
            outputFileUrl = Regex.Replace(outputFileUrl, $"\\{extension}$", "");
            return new FileInfo(outputFileUrl);
        }

        static string ExtractDocs(StreamReader reader, string docTitle)
        {
            var RegexOpenScope = new Regex("^\\s*((Public|Private)\\s+)?(Default\\s+)?(Class|Function|Sub|Property\\s+(Get|Let|Set))", RegexOptions.IgnoreCase);
            var RegexCloseScope = new Regex("^\\s*End\\s+(Function|Sub|Class|Property).*", RegexOptions.IgnoreCase);

            var sb = new StringBuilder();
            Stack<string> DocScope = new Stack<string>();
            DocScope.Push($"File :{docTitle}");
            var wasPreviousLineDocumenting = false;
            while (!reader.EndOfStream)
            {
                var s = reader.ReadLine().Trim();
                var isLineEmpty = string.IsNullOrWhiteSpace(s);
                var isLineAComment = !isLineEmpty && s.TrimStart().StartsWith("'");
                var isLineADocComment = isLineAComment && s.TrimStart().StartsWith("'''");
                var isLineACodeLine = !isLineEmpty && !isLineAComment;
                var isLineAnOpeningScope = isLineACodeLine && RegexOpenScope.IsMatch(s);
                var isLineAClosingScope = isLineACodeLine && !isLineAnOpeningScope && RegexCloseScope.IsMatch(s);

                if (isLineADocComment)
                {
                    if (!wasPreviousLineDocumenting && DocScope.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append("".PadLeft(DocScope.Count, '#'));
                        sb.AppendLine(" `" + DocScope.Peek().Trim() + "`" + System.Environment.NewLine);
                    }

                    var docLine = s.Substring(3);
                    var isHeaderLine = docLine.TrimStart().StartsWith("#");
                    if (isHeaderLine)
                    {
                        docLine = "".PadLeft(DocScope.Count, '#') + docLine.TrimStart();
                    }
                    sb.AppendLine(docLine);
                    wasPreviousLineDocumenting = true;
                }
                else
                {
                    wasPreviousLineDocumenting = false;
                    if (isLineAnOpeningScope)
                    {
                        DocScope.Push(s);
                    }
                    else if (isLineAClosingScope)
                    {
                        DocScope.Pop();
                    }
                }
            }
            return sb.ToString();
        }
    }

}
