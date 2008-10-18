﻿//
// Copyright (c) 2008, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;

namespace DiscUtils.Iso9660
{
    /// <summary>
    /// Represents a file that will be built into the ISO image.
    /// </summary>
    public class BuildFileInfo : BuildDirectoryMember
    {
        private BuildDirectoryInfo _parent;
        private byte[] _contentData;
        private string _contentPath;
        private Stream _contentStream;
        private uint _extentStart;

        internal BuildFileInfo(string name, BuildDirectoryInfo parent, byte[] content)
            : base(IsoUtilities.NormalizeFileName(name), MakeShortFileName(name, parent))
        {
            _parent = parent;
            _contentData = content;
        }

        internal BuildFileInfo(string name, BuildDirectoryInfo parent, string content)
            : base(IsoUtilities.NormalizeFileName(name), MakeShortFileName(name, parent))
        {
            _parent = parent;
            _contentPath = content;
        }

        internal BuildFileInfo(string name, BuildDirectoryInfo parent, Stream source)
            : base(IsoUtilities.NormalizeFileName(name), MakeShortFileName(name, parent))
        {
            _parent = parent;
            _contentStream = source;
        }

        /// <summary>
        /// The parent directory, or <c>null</c> if none.
        /// </summary>
        public override BuildDirectoryInfo Parent
        {
            get { return _parent; }
        }

        internal uint ExtentStart
        {
            get { return _extentStart; }
            set { _extentStart = value; }
        }

        internal override long GetDataSize(Encoding enc)
        {
            if (_contentData != null)
            {
                return _contentData.LongLength;
            }
            else if (_contentPath != null)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(_contentPath);
                return fi.Length;
            }
            else
            {
                return _contentStream.Length;
            }
        }

        internal Stream OpenStream()
        {
            if (_contentData != null)
            {
                return new MemoryStream(_contentData, false);
            }
            else if (_contentPath != null)
            {
                return new FileStream(_contentPath, FileMode.Open, FileAccess.Read);
            }
            else
            {
                return _contentStream;
            }
        }

        internal void CloseStream(Stream s)
        {
            // Close and dispose the stream, unless it's one we were given to stream in
            // from (we might need it again).
            if (_contentStream != s)
            {
                s.Close();
                s.Dispose();
            }
        }

        private static string MakeShortFileName(string longName, BuildDirectoryInfo dir)
        {
            if (IsoUtilities.isValidFileName(longName))
            {
                return longName;
            }

            char[] shortNameChars = longName.ToUpper().ToCharArray();
            for (int i = 0; i < shortNameChars.Length; ++i)
            {
                if (!IsoUtilities.isValidDChar(shortNameChars[i]) && shortNameChars[i] != '.' && shortNameChars[i] != ';')
                {
                    shortNameChars[i] = '_';
                }
            }

            string[] parts = IsoUtilities.SplitFileName(new string(shortNameChars));

            if (parts[0].Length + parts[1].Length > 30)
            {
                parts[1] = parts[1].Substring(0, Math.Min(parts[1].Length, 3));
            }

            if (parts[0].Length + parts[1].Length > 30)
            {
                parts[0] = parts[0].Substring(0, 30 - parts[1].Length);
            }

            string candidate = parts[0] + '.' + parts[1] + ';' + parts[2];

            // TODO: Make unique

            return candidate;
        }
    }
}