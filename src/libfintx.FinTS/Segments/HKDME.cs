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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using libfintx.FinTS.Data;
using libfintx.FinTS.Message;
using libfintx.FinTS.Segments;
using libfintx.Sepa;
using Microsoft.Extensions.Logging;

namespace libfintx.FinTS
{
    public static class HKDME
    {
        /// <summary>
        /// Collective collect
        /// </summary>
        public static async Task<String> Init_HKDME(FinTsClient client, DateTime SettlementDate,
            List<Pain00800202CcData> PainData, string NumberofTransactions, decimal TotalAmount)
        {
            client.Logger.LogInformation("Starting job HKDME: Collective collect money");

            client.SEGNUM = Convert.ToInt16(SEG_NUM.Seg3);

            var TotalAmount_ = TotalAmount.ToString().Replace(",", ".");

            var connectionDetails = client.ConnectionDetails;
            // TODO: Anscheinend wird totalAmount nur in der Version 2 unterstützt, diese hat jedoch nicht jede Bank implementiert.
            //string segments = "HKDME:" + client.SEGNUM + ":2+" + connectionDetails.Iban + ":" + connectionDetails.Bic + "+" + TotalAmount_ + ":EUR++" + "+urn?:iso?:std?:iso?:20022?:tech?:xsd?:pain.008.002.02+@@";
            SEG sEG = new SEG();
            StringBuilder sb = new StringBuilder();
            sb.Append(connectionDetails.Iban);
            sb.Append(DEG.Separator);
            sb.Append(connectionDetails.Bic);
            sb.Append(sEG.Delimiter);
            sb.Append(sEG.Delimiter);
            sb.Append("+urn?:iso?:std?:iso?:20022?:tech?:xsd?:pain.008.002.02+@@");
            string segments = sEG.toSEG(new SEG_DATA
            {
                Header = "HKDME",
                Num = client.SEGNUM,
                Version = 1,
                RefNum = 0,
                RawData = sb.ToString()
            });
            //string segments = "HKDME:" + client.SEGNUM + ":1+" + connectionDetails.Iban + ":" + connectionDetails.Bic + "++" + "+urn?:iso?:std?:iso?:20022?:tech?:xsd?:pain.008.002.02+@@";

            var message = pain00800202.Create(connectionDetails.AccountHolder, connectionDetails.Iban,
                connectionDetails.Bic, SettlementDate, PainData, NumberofTransactions, TotalAmount);

            segments = segments.Replace("@@", "@" + (message.Length - 1) + "@") + message;

            if (client.BPD.IsTANRequired("HKDME"))
            {
                client.SEGNUM = Convert.ToInt16(SEG_NUM.Seg4);
                segments = HKTAN.Init_HKTAN(client, segments, "HKDME");
            }

            var TAN = await FinTSMessage.Send(client, FinTSMessage.Create(client, client.HNHBS, client.HNHBK,
                segments, client.HIRMS));

            client.Parse_Message(TAN);

            return TAN;
        }
    }
}
