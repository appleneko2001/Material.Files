// Information about MIME types are from WIKIPEDIA: https://en.wikipedia.org/wiki/Media_type
using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Files.Resolvers
{
    public class MimeType
    {
        private string _type; // Left part of Mime-Type
        private string _subtype; // Right part of Mime-Type
        private string _suffix; // Suffix part of Mime-Type

        private string[] _extensions; // Extensions name database
        private string _friendlyName;

        private bool _isStandardTree;
        private bool _isVendorTree;
        private bool _isPersonalTree;
        private bool _isUnregisteredTree;

        private object _icon;
        
        /// <summary>
        /// Create an <see cref="MimeType"/> instance.
        /// </summary>
        /// <param name="mime">MIME type string, should be lower case. For example, "text/plain". MIME type string with suffix are supported too.</param>
        /// <param name="exts">Extensions name that related with this MIME type, should be lower case. For example, "text/plain" is related with "txt"</param>
        public MimeType(string mime, string[] exts, string friendlyName = null, object icon = null)
        {
            var mimePart = mime.Split('/');
            if (mimePart.Length != 2)
                throw new ArgumentException($"The MIME type string \"{mime}\" is not valid.");

            _type = mimePart[0];
            var rightPartMimeStr = mimePart[1].Split('+');
            if (rightPartMimeStr.Length >= 2)
            {
                _subtype = rightPartMimeStr[0];
                _suffix = rightPartMimeStr[1];
            }
            else
            {
                _subtype = mimePart[1];
            }

            _extensions = exts;

            if (friendlyName == null)
            {
                _friendlyName = $"{_subtype} file";
            }
            else
            {
                _friendlyName = friendlyName;
            }

            _icon = icon;
            
            // Determine this MIME is Vendor Tree
            if (mimePart[1].StartsWith("vnd."))
                _isVendorTree = true;
            
            // Determine this MIME is Personal Tree
            else if (mimePart[1].StartsWith("prs."))
                _isPersonalTree = true;
            
            // Determine this MIME is Unregistered Tree
            else if (mimePart[1].StartsWith("x."))
                _isPersonalTree = true;
            
            // Maybe this MIME is not those trees, give it Standard Tree in default case.
            else
                _isStandardTree = true;
        }

        public string Type => _type;
        public string SubType => _subtype;
        public string Suffix => _suffix;
        public IReadOnlyCollection<string> Extensions => _extensions;
        public string Name => _friendlyName;

        public bool IsStandardTree => _isStandardTree;
        public bool IsVendorTree => _isVendorTree;
        public bool IsPersonalTree => _isPersonalTree;
        public bool IsUnregisteredTree => _isUnregisteredTree;

        public object Icon => _icon;

        public override string ToString()
        {
            var builder = new StringBuilder($"{_type}/{_subtype}");
            if (_suffix != null)
                builder.Append($"+{_suffix}");
            
            return builder.ToString();
        }
    }
}