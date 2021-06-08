using System;
using System.Collections.Generic;
using System.Linq;
using Material.Icons;

namespace Material.Files.Resolvers
{
    public class MimeTypesDatabase
    {
        private static List<MimeType> _pool;
        private static MimeType _defaultTextMime;

        private static MimeType _defaultMime;
        public static MimeType OctetStreamMime => _defaultMime; 
        
        static MimeTypesDatabase()
        {
            if (_pool != null)
                throw new OperationCanceledException("MIME type database has initialized already.");
            
            _defaultMime = new MimeType("application/octet-stream", new[] {"bin"}, "Binary file", MaterialIconKind.File);
            _defaultTextMime = new MimeType("text/plain", new[] {"txt","text","conf","def","list","log","in", "ini"}, "Text file", MaterialIconKind.File);
            _pool = new List<MimeType>();
            
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
            // Integrated common mime types
            
            // Text section
            _pool.Add(_defaultTextMime);
            _pool.Add(new MimeType("text/xml", new []{"xml"}, "XML file"));
            _pool.Add(new MimeType("text/html", new []{"html", "htm", "shtml"}, "HTML file"));
            _pool.Add(new MimeType("text/css", new []{"css"}, "CSS file"));
            _pool.Add(new MimeType("text/csv", new []{"csv"}, "Comma-separated values file"));
            _pool.Add(new MimeType("text/javascript", new []{"js"}, "JavaScript file"));
            _pool.Add(new MimeType("text/javascript", new []{"mjs"}, "JavaScript module file"));
            _pool.Add(new MimeType("text/markdown", new []{"md"}, "Markdown file"));
            
            // Application section
            _pool.Add(new MimeType("application/x-httpd-php", new []{"php"}, "Hypertext Preprocessor file"));
            
            _pool.Add(new MimeType("application/json", new []{"json"}, "JSON file"));
            
            _pool.Add(new MimeType("application/rtf", new []{"rtf"}, "Rich Text Format document"));
            _pool.Add(new MimeType("application/pdf", new []{"pdf"}, "PDF document"));
            
            _pool.Add(new MimeType("application/msword", new []{"doc"}, "Microsoft Word document", MaterialIconKind.FileWordOutline));
            _pool.Add(new MimeType("application/vnd.ms-excel", new []{"xls"}, "Microsoft Excel file", MaterialIconKind.FileExcelOutline));
            _pool.Add(new MimeType("application/vnd.ms-powerpoint", new []{"ppt"}, "Microsoft PowerPoint file", MaterialIconKind.FilePowerpointOutline));
            
            _pool.Add(new MimeType("application/vnd.openxmlformats-officedocument.wordprocessingml.document", new []{"docx"}, "Microsoft Word (OpenXml) document", MaterialIconKind.FileWordOutline));
            _pool.Add(new MimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new []{"xlsx"}, "Microsoft Excel (OpenXml) file", MaterialIconKind.FileExcelOutline));
            _pool.Add(new MimeType("application/vnd.openxmlformats-officedocument.presentationml.presentation", new []{"pptx"}, "Microsoft PowerPoint (OpenXml) file", MaterialIconKind.FilePowerpointOutline));
            
            _pool.Add(new MimeType("application/zip", new []{"zip"}, "Zip archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/x-7z-compressed", new []{"7z"}, "7-Zip archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/vnd.rar", new []{"rar"}, "Rar archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/x-bzip", new []{"bz"}, "BZip archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/x-bzip2", new []{"bz2"}, "BZip2 archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/x-freearc", new []{"arc"}, "Archive document file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/gzip", new []{"gz"}, "GZip compressed archive file", MaterialIconKind.ZipBox));
            _pool.Add(new MimeType("application/x-tar", new []{"tar"}, "Tarball archive file", MaterialIconKind.ZipBox));
            
            _pool.Add(new MimeType("application/java-archive", new []{"jar"}, "Java application", MaterialIconKind.LanguageJava));
            _pool.Add(new MimeType("application/x-shockwave-flash", new []{"swf"}, "Shockwave flash file"));
            
            _pool.Add(new MimeType("application/x-redhat-package-manager", new []{"rpm"}, "RedHat package file", MaterialIconKind.Redhat));
            
            _pool.Add(new MimeType("application/xhtml+xml", new []{"xhtml"}, "XHTML file"));
            
            _pool.Add(new MimeType("application/x-msdownload", new []{"exe", "com"}, "Microsoft Windows application", MaterialIconKind.Cog));
            _pool.Add(new MimeType("application/x-msdownload", new []{"dll"}, "Microsoft Windows application library", MaterialIconKind.FileCog));
            
            _pool.Add(new MimeType("application/x-ms-shortcut", new []{"lnk"}, "Microsoft Windows shortcut file"));
            
            _pool.Add(new MimeType("application/x-sh", new []{"sh"}, "Bourne shell script", MaterialIconKind.ScriptText));
            _pool.Add(new MimeType("application/x-bat", new []{"bat", "cmd", "btm"}, "MS-DOS batch script", MaterialIconKind.ScriptText));
            
            // Audio section
            _pool.Add(new MimeType("audio/aac", new []{"aac"}, "AAC audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/midi", new []{"mid", "midi", "kar"}, "MIDI audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/mpeg", new []{"mp3"}, "MP3 audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/ogg", new []{"ogg", "oga"}, "OGG audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/opus", new []{"opus"}, "Opus audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/x-m4a", new []{"m4a"}, "M4A audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/wav", new []{"wav"}, "Waveform audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/webm", new []{"weba"}, "WEBM audio file", MaterialIconKind.FileMusic));
            _pool.Add(new MimeType("audio/3gpp", new []{"3gp"}, "3GPP container audio file", MaterialIconKind.FileMusic));
            
            // Video section
            _pool.Add(new MimeType("video/x-msvideo", new []{"avi"}, "AVI video file", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/mp4", new []{"mp4"}, "MP4 video file", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/mpeg", new []{"mpeg"}, "MPEG video file", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/mp2t", new []{"ts"}, "MPEG transport stream", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/ogg", new []{"ogv"}, "OGG video file", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/webm", new []{"webm"}, "WEBM video file", MaterialIconKind.FileVideo));
            _pool.Add(new MimeType("video/3gpp", new []{"3gp"}, "3GPP container video file", MaterialIconKind.FileVideo));
            
            // Image section
            _pool.Add(new MimeType("image/bmp", new []{"bmp"}, "Windows OS/2 Bitmap file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/gif", new []{"gif"}, "Graphics Interchange Format file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/vnd.microsoft.icon", new []{"ico"}, "Windows Icon file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/jpeg", new []{"jpeg", "jpg"}, "JPEG image file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/png", new []{"png"}, "Portable Network Graphics file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/svg+xml", new []{"svg"}, "Scalable Vector Graphics file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/tiff", new []{"tif", "tiff"}, "Tagged Image File Format file", MaterialIconKind.FileImage));
            _pool.Add(new MimeType("image/webp", new []{"webp"}, "WEBP image file", MaterialIconKind.FileImage));
            
            // Font section
            _pool.Add(new MimeType("font/otf", new []{"otf"}, "OpenType font file"));
            _pool.Add(new MimeType("font/ttf", new []{"ttf"}, "TrueType font file"));
            _pool.Add(new MimeType("font/woff", new []{"woff"}, "Web Open Font Format file"));
            _pool.Add(new MimeType("font/woff2", new []{"woff2"}, "Web Open Font Format file"));
        }

        public static MimeType GetMime(string ext)
        {
            var result = _pool.Where(delegate(MimeType type)
            {
                return type.Extensions.Contains(ext);
            });

            if (result.Any())
                return result.First();

            return _defaultMime;
        }
    }
}