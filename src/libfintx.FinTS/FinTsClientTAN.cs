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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libfintx.FinTS
{
    public partial class FinTsClient
    {
        /// <summary>
        /// Confirm order with TAN
        /// </summary>
        /// <param name="TAN"></param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> TAN(string TAN)
        {
            string bankCode = await Transaction.TAN(this, TAN);
            var result = new HBCIDialogResult(Parse_BankCode(bankCode), bankCode);

            return result;
        }

        /// <summary>
        /// Confirm order with TAN
        /// </summary>
        /// <param name="TAN"></param>
        /// <param name="MediumName"></param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> TAN4(string TAN, string MediumName)
        {
            string bankCode = await Transaction.TAN4(this, TAN, MediumName);
            var result = new HBCIDialogResult(Parse_BankCode(bankCode), bankCode);

            return result;
        }

        /// <summary>
        /// Request tan medium name
        /// </summary>
        /// <returns>
        /// TAN Medium Name
        /// </returns>
        public async Task<HBCIDialogResult<List<string>>> RequestTANMediumName()
        {
            HBCIDialogResult result = await InitializeConnection("HKTAB");
            if (!result.IsSuccess)
                return result.TypedResult<List<string>>();

            // Should not be needed when processing HKTAB
            //result = ProcessSCA(connectionDetails, result, tanDialog);
            //if (!result.IsSuccess)
            //    return result.TypedResult<List<string>>();

            string BankCode = await Transaction.HKTAB(this);
            result = new HBCIDialogResult<List<string>>(Parse_BankCode(BankCode), BankCode);
            if (!result.IsSuccess)
                return result.TypedResult<List<string>>();

            // Should not be needed when processing HKTAB
            //result = ProcessSCA(connectionDetails, result, tanDialog);
            //if (!result.IsSuccess)
            //    return result.TypedResult<List<string>>();

            BankCode = result.RawData;
            string BankCode_ = "HITAB" + Helper.Parse_String(BankCode, "'HITAB", "'");
            return result.TypedResult(Parse_TANMedium(BankCode_).ToList());
        }
    }
}
