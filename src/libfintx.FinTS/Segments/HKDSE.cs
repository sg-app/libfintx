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
using System.Text;
using System.Threading.Tasks;
using libfintx.FinTS.Data;
using libfintx.FinTS.Message;
using libfintx.FinTS.Segments;
using libfintx.Sepa;
using Microsoft.Extensions.Logging;

namespace libfintx.FinTS
{
    public static class HKDSE
    {
        /// <summary>
        /// Collect
        /// </summary>
        public static async Task<String> Init_HKDSE(FinTsClient client, string Payer, string PayerIBAN,
            string PayerBIC, decimal Amount, string Usage, DateTime SettlementDate, string MandateNumber,
            DateTime MandateDate, string CreditorIDNumber)
        {
            client.Logger.LogInformation("Starting job HKDSE: Collect money");

            client.SEGNUM = Convert.ToInt16(SEG_NUM.Seg4);

            var connectionDetails = client.ConnectionDetails;
            SEG sEG = new SEG();
            StringBuilder sb = new StringBuilder();
            sb.Append(connectionDetails.Iban);
            sb.Append(DEG.Separator);
            sb.Append(connectionDetails.Bic);
            sb.Append("+urn?:iso?:std?:iso?:20022?:tech?:xsd?:pain.008.002.02+@@");
            string segments = sEG.toSEG(new SEG_DATA
            {
                Header = "HKDSE",
                Num = client.SEGNUM,
                Version = 1,
                RefNum = 0,
                RawData = sb.ToString()
            });
            //string segments = "HKDSE:" + client.SEGNUM + ":1+" + connectionDetails.Iban + ":" + connectionDetails.Bic + "+urn?:iso?:std?:iso?:20022?:tech?:xsd?:pain.008.002.02+@@";

            var message = pain00800202.Create(connectionDetails.AccountHolder, connectionDetails.Iban,
                connectionDetails.Bic, Payer, PayerIBAN, PayerBIC, Amount, Usage, SettlementDate, MandateNumber,
                MandateDate, CreditorIDNumber);

            segments = segments.Replace("@@", "@" + (message.Length - 1) + "@") + message;

            if (client.BPD.IsTANRequired("HKDSE"))
            {
                client.SEGNUM = Convert.ToInt16(SEG_NUM.Seg4);
                segments = HKTAN.Init_HKTAN(client, segments, "HKDSE");
            }

            var response = await FinTSMessage.Send(client, FinTSMessage.Create(client, client.HNHBS, client.HNHBK,
                segments, client.HIRMS));

            client.Parse_Message(response);

            return response;
        }
    }
}
