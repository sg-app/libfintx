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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using libfintx.FinTS.Data;
using libfintx.Sepa;

namespace libfintx.FinTS
{
    public partial class FinTsClient
    {
        /// <summary>
        /// Transfer money - General method
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="receiverName">Name of the recipient</param>
        /// <param name="receiverIBAN">IBAN of the recipient</param>
        /// <param name="receiverBIC">BIC of the recipient</param>
        /// <param name="amount">Amount to transfer</param>
        /// <param name="purpose">Short description of the transfer (dt. Verwendungszweck)</param>      
        /// <param name="hirms">Numerical SecurityMode; e.g. 911 for "Sparkasse chipTan optisch"</param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> Transfer(TANDialog tanDialog, string receiverName, string receiverIBAN, string receiverBIC,
            decimal amount, string purpose, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCCS(this, receiverName, receiverIBAN, receiverBIC, amount, purpose);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }

        /// <summary>
        /// Transfer money at a certain time - General method
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="receiverName">Name of the recipient</param>
        /// <param name="receiverIBAN">IBAN of the recipient</param>
        /// <param name="receiverBIC">BIC of the recipient</param>
        /// <param name="amount">Amount to transfer</param>
        /// <param name="purpose">Short description of the transfer (dt. Verwendungszweck)</param>      
        /// <param name="executionDay"></param>
        /// <param name="hirms">Numerical SecurityMode; e.g. 911 for "Sparkasse chipTan optisch"</param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> Transfer_Terminated(TANDialog tanDialog, string receiverName, string receiverIBAN, string receiverBIC,
            decimal amount, string purpose, DateTime executionDay, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCSE(this, receiverName, receiverIBAN, receiverBIC, amount, purpose, executionDay);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }


        /// <summary>
        /// Collective transfer money - General method
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="painData"></param>
        /// <param name="numberOfTransactions"></param>
        /// <param name="totalAmount"></param>
        /// <param name="hirms">Numerical SecurityMode; e.g. 911 for "Sparkasse chipTan optisch"</param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> CollectiveTransfer(TANDialog tanDialog, List<Pain00100203CtData> painData,
            string numberOfTransactions, decimal totalAmount, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCCM(this, painData, numberOfTransactions, totalAmount);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }

        /// <summary>
        /// Collective transfer money terminated - General method
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="painData"></param>
        /// <param name="numberOfTransactions"></param>
        /// <param name="totalAmount"></param>
        /// <param name="executionDay"></param>
        /// <param name="hirms">Numerical SecurityMode; e.g. 911 for "Sparkasse chipTan optisch"</param>
        /// <returns>
        /// Bank return codes
        /// </returns>
        public async Task<HBCIDialogResult> CollectiveTransfer_Terminated(TANDialog tanDialog, List<Pain00100203CtData> painData,
            string numberOfTransactions, decimal totalAmount, DateTime executionDay, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCME(this, painData, numberOfTransactions, totalAmount, executionDay);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }

        /// <summary>
        /// Get terminated transfers
        /// </summary>
        /// <returns>
        /// Banker's orders
        /// </returns>
        public async Task<HBCIDialogResult<List<TerminatedTransfer>>> GetTerminatedTransfers(TANDialog tanDialog)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result.TypedResult<List<TerminatedTransfer>>();

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result.TypedResult<List<TerminatedTransfer>>();

            // Success
            string BankCode = await Transaction.HKCSB(this);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result.TypedResult<List<TerminatedTransfer>>();

            result = await ProcessSCA(result, tanDialog);

            if (!result.IsSuccess)
                return result.TypedResult<List<TerminatedTransfer>>();

            BankCode = result.RawData;
            int startIdx = BankCode.IndexOf("HICSB");
            if (startIdx < 0)
                return result.TypedResult<List<TerminatedTransfer>>();

            var data = new List<TerminatedTransfer>();

            string BankCode_ = BankCode.Substring(startIdx);
            for (; ; )
            {
                var match = Regex.Match(BankCode_, @"HICSB.+?(?<xml><\?xml.+?</Document>)\+(?<orderid>.*?)(\+(?<deleteable>j|n))?(\+(?<modifiable>j|n))?'", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string xml = match.Groups["xml"].Value;
                    // xml ist UTF-8
                    xml = Converter.ConvertEncoding(xml, Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8);

                    string orderId = match.Groups["orderid"].Value;
                    bool? deleteable = null;
                    if (match.Groups["deleteable"].Success)
                        deleteable = match.Groups["deleteable"].Value.Equals("j", StringComparison.OrdinalIgnoreCase) ? true : false;

                    bool? modifiable = null;
                    if (match.Groups["modifiable"].Success)
                        modifiable = match.Groups["modifiable"].Value.Equals("j", StringComparison.OrdinalIgnoreCase) ? true : false;

                    var painData = Pain00100103CtData.Create(xml);

                    var item = new TerminatedTransfer(orderId, deleteable, modifiable, painData);
                    data.Add(item);
                }

                int endIdx = BankCode_.IndexOf("'");
                if (BankCode_.Length <= endIdx + 1)
                    break;

                BankCode_ = BankCode_.Substring(endIdx + 1);
                startIdx = BankCode_.IndexOf("HICSB");
                if (startIdx < 0)
                    break;
            }

            // Success
            return result.TypedResult(data);
        }

        public async Task<HBCIDialogResult> ModifyTerminatedTransfer(TANDialog tanDialog, string orderId, string receiverName, string receiverIBAN,
                string receiverBIC, decimal amount, string usage, DateTime executionDay, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCSA(this, orderId, receiverName, receiverIBAN, receiverBIC, amount, usage, executionDay);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }

        public async Task<HBCIDialogResult> DeleteTerminatedTransfer(TANDialog tanDialog, string orderId, string receiverName, string receiverIBAN,
                string receiverBIC, decimal amount, string usage, DateTime executionDay, string hirms)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog, true);
            if (!result.IsSuccess)
                return result;

            TransactionConsole.Output = string.Empty;

            if (!string.IsNullOrEmpty(hirms))
                HIRMS = hirms;

            string BankCode = await Transaction.HKCSL(this, orderId, receiverName, receiverIBAN, receiverBIC, amount, usage, executionDay);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result;

            result = await ProcessSCA(result, tanDialog);

            return result;
        }
    }
}
