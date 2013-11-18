using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarball
{
    /// <summary>
    /// Tarball header structure
    /// </summary>
    struct TarballHeader
    {
        public string FileName;
        public uint FileMode;
        public uint OwnerId, GroupId;
        public long Size;
        public DateTime LastModified;
        public uint Checksum;
        public byte LinkIndicator;
        public string LinkedFile;
    }
}
