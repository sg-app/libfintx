﻿/*	
 * 	
 *  This file is part of libfintx.
 *  
 *  Copyright (C) 2016 - 2021 Torsten Klinger
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using libfintx.FinTS.Camt;
using libfintx.FinTS.Camt.Camt052;
using libfintx.FinTS.Camt.Camt053;
using libfintx.Globals;
using libfintx.Swift;

namespace libfintx.FinTS
{
    public partial class FinTsClient
    {
        /// <summary>
        /// Account transactions in SWIFT-format
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="saveMt940File"></param>
        /// <returns>
        /// Transactions
        /// </returns>
        public async Task<HBCIDialogResult<List<SwiftStatement>>> Transactions(TANDialog tanDialog, DateTime? startDate = null, DateTime? endDate = null, bool saveMt940File = false)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result.TypedResult<List<SwiftStatement>>();

            result = await ProcessSCA(result, tanDialog);
            if (result.HasError)
                return result.TypedResult<List<SwiftStatement>>();

            string startDateStr = startDate?.ToString("yyyyMMdd");
            string endDateStr = endDate?.ToString("yyyyMMdd");

            // Success
            string BankCode = await Transaction.HKKAZ(this, startDateStr, endDateStr, null);
            result = new HBCIDialogResult(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result.TypedResult<List<SwiftStatement>>();

            result = await ProcessSCA(result, tanDialog);
            if (result.HasError)
                return result.TypedResult<List<SwiftStatement>>();

            BankCode = result.RawData;
            var TransactionsMt940 = new StringBuilder();
            var TransactionsMt942 = new StringBuilder();

            Regex regex = new Regex(@"HIKAZ:.+?@\d+@(?<mt940>.+?)(\+@\d+@(?<mt942>.+?))?('{1,2}H[A-Z]{4}:\d+:\d+)", RegexOptions.Singleline);
            Match match = regex.Match(BankCode);
            if (match.Success)
            {
                TransactionsMt940.Append(match.Groups["mt940"].Value);
                TransactionsMt942.Append(match.Groups["mt942"].Value);
            }

            string BankCode_ = BankCode;
            while (BankCode_.Contains("+3040::"))
            {
                Parse_Message(BankCode_);

                string Startpoint = Parse_Transactions_Startpoint(BankCode_);

                BankCode_ = await Transaction.HKKAZ(this, startDateStr, endDateStr, Startpoint);
                result = new HBCIDialogResult(Parse_BankCode(BankCode_), BankCode_);
                if (result.HasError)
                    return result.TypedResult<List<SwiftStatement>>();

                result = await ProcessSCA(result, tanDialog);
                if (result.HasError)
                    return result.TypedResult<List<SwiftStatement>>();

                BankCode_ = result.RawData;
                match = regex.Match(BankCode_);
                if (match.Success)
                {
                    TransactionsMt940.Append(match.Groups["mt940"].Value);
                    TransactionsMt942.Append(match.Groups["mt942"].Value);
                }
            }

            var swiftStatements = new List<SwiftStatement>();

            swiftStatements.AddRange(MT940.Deserialize(TransactionsMt940.ToString(), loggerFactory: _loggerFactory));
            if (saveMt940File)
            {
                WriteMT940ToFile(TransactionsMt940.ToString(), ConnectionDetails.Account);
            }
            swiftStatements.AddRange(MT940.Deserialize(TransactionsMt942.ToString(), pending: true, _loggerFactory));
            if (saveMt940File)
            {
                WriteMT940ToFile(TransactionsMt942.ToString(), ConnectionDetails.Account);
            }

            return result.TypedResult(swiftStatements);
        }

        /// <summary>
        /// Writes a MT940 statement into the library own folder for further use.
        /// </summary>
        /// <param name="sta">
        /// The raw text of the MT940 statement.
        /// </param>
        /// <param name="account">
        /// The account name.
        /// </param>
        // ReSharper disable once InconsistentNaming
        private static void WriteMT940ToFile(string sta, string account)
        {
            string dir = FinTsGlobals.ProgramBaseDir;

            dir = Path.Combine(dir, "STA");

            string filename = Path.Combine(dir, Helper.MakeFilenameValid(account + "_" + DateTime.Now + ".STA"));

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // STA
            if (!File.Exists(filename))
            {
                using (File.Create(filename))
                { }

                File.AppendAllText(filename, sta);
            }
            else
                File.AppendAllText(filename, sta);
        }

        /// <summary>
        /// Account transactions in camt format
        /// </summary>
        /// <param name="camtVers"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="tanDialog"></param>
        /// <param name="saveCamtFile"></param>
        /// <returns>
        /// Transactions
        /// </returns>
        public async Task<HBCIDialogResult<List<CamtStatement>>> Transactions_camt(TANDialog tanDialog, CamtVersion camtVers,
            DateTime? startDate = null, DateTime? endDate = null, bool saveCamtFile = false)
        {
            var result = await InitializeConnection();
            if (result.HasError)
                return result.TypedResult<List<CamtStatement>>();

            result = await ProcessSCA(result, tanDialog);
            if (result.HasError)
                return result.TypedResult<List<CamtStatement>>();

            // Plain camt message
            string camt = string.Empty;

            string startDateStr = startDate?.ToString("yyyyMMdd");
            string endDateStr = endDate?.ToString("yyyyMMdd");

            // Success
            string BankCode = await Transaction.HKCAZ(this, startDateStr, endDateStr, null, camtVers);
            result = new HBCIDialogResult<List<CamtStatement>>(Parse_BankCode(BankCode), BankCode);
            if (result.HasError)
                return result.TypedResult<List<CamtStatement>>();

            result = await ProcessSCA(result, tanDialog);
            if (result.HasError)
                return result.TypedResult<List<CamtStatement>>();

            BankCode = result.RawData;
            var statements = new List<CamtStatement>();

            Camt052Parser camt052Parser = null;
            Camt053Parser camt053Parser = null;
            var encoding = Encoding.GetEncoding("ISO-8859-1");

            string BankCode_ = BankCode;

            // Es kann sein, dass in der payload mehrere Dokumente enthalten sind
            int xmlStartIdx = BankCode_.IndexOf("<?xml version=");
            if (xmlStartIdx < 0) // Manche Banken liefern das XML ohne Header (Hypo Vereinsbank)
                xmlStartIdx = BankCode_.IndexOf("<Document");
            int xmlEndIdx = BankCode_.IndexOf("</Document>") + "</Document>".Length;
            while (xmlStartIdx >= 0)
            {
                if (xmlStartIdx > xmlEndIdx)
                    break;

                camt = BankCode_.Substring(xmlStartIdx, xmlEndIdx - xmlStartIdx);
                if (!camt.StartsWith("<?xml"))
                    camt = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + camt;

                switch (camtVers)
                {
                    case CamtVersion.Camt052:
                        if (camt052Parser == null)
                            camt052Parser = new Camt052Parser(_loggerFactory);

                        if (saveCamtFile)
                        {
                            // Save camt052 statement to file
                            Camt052File.Save(ConnectionDetails.Account, camt, encoding);
                        }
                        camt052Parser.ProcessDocument(camt, encoding);

                        statements.AddRange(camt052Parser.statements);
                        break;
                    case CamtVersion.Camt053:
                        if (camt053Parser == null)
                            camt053Parser = new Camt053Parser(_loggerFactory);

                        if (saveCamtFile)
                        {
                            // Save camt053 statement to file
                            Camt053File.Save(ConnectionDetails.Account, camt, encoding);
                        }
                        camt053Parser.ProcessDocument(camt, encoding);

                        statements.AddRange(camt053Parser.statements);
                        break;
                }

                BankCode_ = BankCode_.Substring(xmlEndIdx);
                xmlStartIdx = BankCode_.IndexOf("<?xml version");
                if (xmlStartIdx < 0) // Manche Banken liefern das XML ohne Header (Hypo Vereinsbank)
                    xmlStartIdx = BankCode_.IndexOf("<Document");
                xmlEndIdx = BankCode_.IndexOf("</Document>") + "</Document>".Length;
            }

            BankCode_ = BankCode;

            while (BankCode_.Contains("+3040::"))
            {
                Parse_Message(BankCode_);

                string Startpoint = Parse_Transactions_Startpoint(BankCode_);

                BankCode_ = await Transaction.HKCAZ(this, startDateStr, endDateStr, Startpoint, camtVers);
                result = new HBCIDialogResult<List<CamtStatement>>(Parse_BankCode(BankCode_), BankCode_);
                if (result.HasError)
                    return result.TypedResult<List<CamtStatement>>();

                result = await ProcessSCA(result, tanDialog);
                if (result.HasError)
                    return result.TypedResult<List<CamtStatement>>();

                BankCode_ = result.RawData;

                // Es kann sein, dass in der payload mehrere Dokumente enthalten sind
                xmlStartIdx = BankCode_.IndexOf("<?xml version=");
                if (xmlStartIdx < 0) // Manche Banken liefern das XML ohne Header (Hypo Vereinsbank)
                    xmlStartIdx = BankCode_.IndexOf("<Document");
                xmlEndIdx = BankCode_.IndexOf("</Document>") + "</Document>".Length;

                while (xmlStartIdx >= 0)
                {
                    if (xmlStartIdx > xmlEndIdx)
                        break;

                    camt = BankCode_.Substring(xmlStartIdx, xmlEndIdx - xmlStartIdx);
                    if (!camt.StartsWith("<?xml"))
                        camt = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + camt;

                    switch (camtVers)
                    {
                        case CamtVersion.Camt052:
                            if (camt052Parser == null)
                                camt052Parser = new Camt052Parser();

                            if (saveCamtFile)
                            {
                                // Save camt052 statement to file
                                Camt052File.Save(ConnectionDetails.Account, camt, encoding);
                            }
                            camt052Parser.ProcessDocument(camt, encoding);

                            statements.AddRange(camt052Parser.statements);
                            break;
                        case CamtVersion.Camt053:
                            if (camt053Parser == null)
                                camt053Parser = new Camt053Parser();

                            if (saveCamtFile)
                            {
                                // Save camt053 statement to file
                                Camt053File.Save(ConnectionDetails.Account, camt, encoding);
                            }
                            camt053Parser.ProcessDocument(camt, encoding);

                            statements.AddRange(camt053Parser.statements);
                            break;
                    }

                    BankCode_ = BankCode_.Substring(xmlEndIdx);
                    xmlStartIdx = BankCode_.IndexOf("<?xml version");
                    if (xmlStartIdx < 0) // Manche Banken liefern das XML ohne Header (Hypo Vereinsbank)
                        xmlStartIdx = BankCode_.IndexOf("<Document");
                    xmlEndIdx = BankCode_.IndexOf("</Document>") + "</Document>".Length;
                }
            }

            return result.TypedResult(statements);
        }

        /// <summary>
        /// Account transactions in simplified libfintx-format
        /// </summary>
        /// <param name="tanDialog"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>
        /// Transactions
        /// </returns>
        public async Task<HBCIDialogResult<List<AccountTransaction>>> TransactionsSimple(TANDialog tanDialog, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await Transactions(tanDialog, startDate, endDate);
            if (!result.IsSuccess)
                return result.TypedResult<List<AccountTransaction>>();

            var transactionList = new List<AccountTransaction>();
            foreach (var swiftStatement in result.Data)
            {
                foreach (var swiftTransaction in swiftStatement.SwiftTransactions)
                {
                    transactionList.Add(new AccountTransaction()
                    {
                        OwnerAccount = swiftStatement.AccountCode,
                        OwnerBankCode = swiftStatement.BankCode,
                        PartnerBic = swiftTransaction.BankCode,
                        PartnerIban = swiftTransaction.AccountCode,
                        PartnerName = swiftTransaction.PartnerName,
                        RemittanceText = swiftTransaction.Description,
                        TransactionType = swiftTransaction.Text,
                        TransactionDate = swiftTransaction.InputDate,
                        ValueDate = swiftTransaction.ValueDate
                    });
                }
            }

            return result.TypedResult(transactionList);
        }
    }
}
