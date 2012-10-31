using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole
{
    public class LessTransform : System.Web.Optimization.IBundleTransform
    {
        public void Process(System.Web.Optimization.BundleContext context, System.Web.Optimization.BundleResponse bundle)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }

            context.HttpContext.Response.Cache.SetLastModifiedFromFileDependencies();

            var lessParser = new dotless.Core.Parser.Parser();
            dotless.Core.ILessEngine lessEngine = CreateLessEngine(lessParser);

            var content = new System.Text.StringBuilder(bundle.Content.Length);

            var bundleFiles = new List<System.IO.FileInfo>();

            foreach (var bundleFile in bundle.Files)
            {
                bundleFiles.Add(bundleFile);

                SetCurrentFilePath(lessParser, bundleFile.FullName);
                string source = System.IO.File.ReadAllText(bundleFile.FullName);
                content.Append(lessEngine.TransformToCss(source, bundleFile.FullName));
                content.AppendLine();

                bundleFiles.AddRange(GetFileDependencies(lessParser));
            }

            if (System.Web.Optimization.BundleTable.EnableOptimizations)
            {
                // include imports in bundle files to register cache dependencies
                bundle.Files = bundleFiles.Distinct();
            }

            bundle.ContentType = "text/css";
            bundle.Content = content.ToString();
        }

        /// <summary>
        /// Creates an instance of LESS engine.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        private dotless.Core.ILessEngine CreateLessEngine(dotless.Core.Parser.Parser lessParser)
        {
            var logger = new dotless.Core.Loggers.AspNetTraceLogger(dotless.Core.Loggers.LogLevel.Debug, new dotless.Core.Abstractions.Http());
            return new dotless.Core.LessEngine(lessParser, logger, true, false);
        }

        /// <summary>
        /// Gets the file dependencies (@imports) of the LESS file being parsed.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        /// <returns>An array of file references to the dependent file references.</returns>
        private IEnumerable<System.IO.FileInfo> GetFileDependencies(dotless.Core.Parser.Parser lessParser)
        {
            dotless.Core.Input.IPathResolver pathResolver = GetPathResolver(lessParser);

            foreach (var importPath in lessParser.Importer.Imports)
            {
                yield return new System.IO.FileInfo(pathResolver.GetFullPath(importPath));
            }

            lessParser.Importer.Imports.Clear();
        }

        /// <summary>
        /// Returns an <see cref="IPathResolver"/> instance used by the specified LESS lessParser.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        private dotless.Core.Input.IPathResolver GetPathResolver(dotless.Core.Parser.Parser lessParser)
        {
            var importer = lessParser.Importer as dotless.Core.Importers.Importer;
            var fileReader = importer.FileReader as dotless.Core.Input.FileReader;

            return fileReader.PathResolver;
        }

        /// <summary>
        /// Informs the LESS parser about the path to the currently processed file. 
        /// This is done by using a custom <see cref="IPathResolver"/> implementation.
        /// </summary>
        /// <param name="lessParser">The LESS parser.</param>
        /// <param name="currentFilePath">The path to the currently processed file.</param>
        private void SetCurrentFilePath(dotless.Core.Parser.Parser lessParser, string currentFilePath)
        {
            var importer = lessParser.Importer as dotless.Core.Importers.Importer;

            if (importer == null)
                throw new InvalidOperationException("Unexpected dotless importer type.");

            var fileReader = importer.FileReader as dotless.Core.Input.FileReader;

            if (fileReader == null || !(fileReader.PathResolver is ImportedFilePathResolver))
            {
                fileReader = new dotless.Core.Input.FileReader(new ImportedFilePathResolver(currentFilePath));
                importer.FileReader = fileReader;
            }
        }
    }

    public class ImportedFilePathResolver : dotless.Core.Input.IPathResolver
    {
        private string currentFileDirectory;
        private string currentFilePath;

        public ImportedFilePathResolver(string currentFilePath)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                throw new ArgumentNullException("currentFilePath");
            }

            CurrentFilePath = currentFilePath;
        }

        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        public string CurrentFilePath
        {
            get { return currentFilePath; }
            set
            {
                currentFilePath = value;
                currentFileDirectory = System.IO.Path.GetDirectoryName(value);
            }
        }

        /// <summary>
        /// Returns the absolute path for the specified improted file path.
        /// </summary>
        /// <param name="filePath">The imported file path.</param>
        public string GetFullPath(string filePath)
        {
            if (filePath.StartsWith("~"))
            {
                filePath = VirtualPathUtility.ToAbsolute(filePath);
            }

            if (filePath.StartsWith("/"))
            {
                filePath = System.Web.Hosting.HostingEnvironment.MapPath(filePath);
            }
            else if (!System.IO.Path.IsPathRooted(filePath))
            {
                filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentFileDirectory, filePath));
            }

            return filePath;
        }
    }
}