﻿/*	
 * 	
 *  This file is part of libfintx.
 *  
 *  Copyright (C) 2016 - 2022 Torsten Klinger
 * 	E-Mail: torsten.klinger@googlemail.com
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 3 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software Foundation,
 *  Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 * 	
 */

// #define WINDOWS
#nullable enable

using libfintx.Logger.Log;
#if USE_LIB_SixLabors_ImageSharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
#endif
using System;
using System.IO;
using System.Text;

namespace libfintx.FinTS
{
    public class MatrixCode
    {
#if USE_LIB_SixLabors_ImageSharp
        private Image<Rgba32>? _codeImage;

        public Image<Rgba32> CodeImage
        {
            get
            {
                if (_codeImage == null)
                {
                    var ms = new MemoryStream(ImageData);
                    _codeImage = (Image<Rgba32>) Image.Load(ms);
                }

                return _codeImage;
            }
        }
#endif

        public string ImageMimeType { get; private set; }

        public byte[] ImageData { get; private set; }

        /// <summary>
        /// photoTAN matrix code
        /// </summary>
        /// <param name="photoTanString"></param>
        public MatrixCode(string photoTanString)
        {
            try
            {
                var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(photoTanString);
                int offset = 0;

                //Read mimetype            
                byte[] b = new byte[2];
                Array.Copy(data, offset, b, 0, 2);

                int mimeTypeLen = int.Parse(Decode(b));
                b = new byte[mimeTypeLen];
                offset += 2;

                Array.Copy(data, offset, b, 0, mimeTypeLen);
                ImageMimeType = Encoding.Default.GetString(b);
                offset += mimeTypeLen;

                //Read image data            
                offset += 2;
                int len = data.Length - offset;
                b = new byte[len];
                Array.Copy(data, offset, b, 0, len);
                ImageData = b;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Invalid photoTan image returned. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Internal decode picture format
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string Decode(byte[] bytes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; ++i)
            {
                sb.Append(Convert.ToString(bytes[i], 10));
            }
            return sb.ToString();
        }

        public void Render(object pictureBox)
        {
            if (pictureBox == null)
                return;
        }
    }
}
