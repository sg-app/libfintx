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

namespace libfintx.Swift;

/// <summary>
/// A single SWIFT field.
/// </summary>
public class SwiftLine
{
    /// <summary>
    /// The SWIFT field number. Can be alphanumeric.
    /// </summary>
    public string SwiftTag { get; set; }

    /// <summary>
    /// The data of the SWIFT field.
    /// </summary>
    public string SwiftData { get; set; }

    public SwiftLine(string swiftTag, string swiftData)
    {
        SwiftTag = swiftTag;
        SwiftData = swiftData;
    }
}
