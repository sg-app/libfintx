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

using System;
using System.Linq;
using System.Net; // Perhaps using int -> SecurityProtocolType conversion better than binding a lib to this class.

namespace libfintx.FinTS.Data;

public class ConnectionDetails
{
    /// <summary>
    /// Url of the HBCI/FinTS endpoint. Can be retrieved from: https://www.hbci-zka.de/institute/institut_auswahl.htm
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// HBCI version number. E.g. '300' for FinTS 3.0.
    ///
    /// Obsolete, please uses <see cref="FinTSVersion"/> instead.
    /// </summary>
    [Obsolete($"Please use property {nameof(FinTSVersion)} instead")]
    public int HbciVersion
    {
        get => (int) FinTSVersion;
        set
        {
            var versionValues = Enum.GetValues<FinTsVersion>().Select(v => (int) v);

            if (!Enum.GetValues<FinTsVersion>().Select(v => (int) v).Contains(value))
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid HbciVersion value. Allowed values are {string.Join(", ", versionValues)}");
            }

            FinTSVersion = (FinTsVersion) value;
        }
    }

    /// <summary>
    /// The FinTS version to be used.
    ///
    /// Default is FinTS 3.0.
    /// </summary>
    public FinTsVersion FinTSVersion { get; set; } = FinTsVersion.v300;

    /// <summary>
    /// Logon ID/username for the bank account
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// UserId with HBCI special characters escaped.
    /// </summary>
    public string UserIdEscaped => Helper.EscapeHbciString(UserId);

    /// <summary>
    /// Logon-Pin for the bank account
    /// </summary>
    public string Pin { get; set; }

    /// <summary>
    /// Name of the Accountholder
    /// </summary>
    public string AccountHolder { get; set; }

    /// <summary>
    /// Accountnumber of the bank account
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// Subaccountfeature of the bank account
    /// </summary>
    public string SubAccount { get; set; }

    /// <summary>
    /// Bankcode of the bank account
    /// </summary>
    public int Blz { get; set; }

    /// <summary>
    /// Bankcode of the bank's headquarter (e.g. to be used in Hypovereinsbank)
    /// </summary>
    public int? BlzHeadquarter { get; set; }

    /// <summary>
    /// BLZ needed for message header (HNVSK, HNSHK) - either BlzHeaderquarter or Blz
    /// </summary>
    public int BlzPrimary => BlzHeadquarter ?? Blz;

    /// <summary>
    /// IBAN of the bank account
    /// </summary>
    public string Iban { get; set; }

    /// <summary>
    /// BIC of the bank account
    /// </summary>
    public string Bic { get; set; }

    /// <summary>
    /// System ID (Kundensystem-ID)
    /// </summary>
    public string CustomerSystemId { get; set; }

    // Security
    public SecurityProtocolType SecurityProtocol { get; set; } = SecurityProtocolType.Tls12;
}
