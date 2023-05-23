using ATMService.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Net;

namespace ATMService.Services
{
    public class ISOHandler
    {
        int[] clear100 ={
            8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,42,43,44,45,46,47,50,52,53,55,56,57,58,59,60,61,62,63,64,65,66,67,68,
            69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,
            112,113,114,115,116,117,118,119,120,121,122,123,124,126,127,128
        };

        int[] clear200 = {
            8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,43,44,45,46,47,50,52,53,55,56,57,58,59,62,63,64,65,66,67,68,69,70,
            71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,
            112,113,114,115,116,117,118,119,120,121,122,123,124,126,127,128
        };

        int[] clear220 = {
            8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,44,45,46,47,50,52,53,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,
            71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,
            112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128
        };

        int[] clear420 = {
            8,9,10,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,44,45,46,47,50,51,52,53,55,56,57,58,59,61,62,63,64,65,66,67,68,69,70,
            71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,
            112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128
        };

        public const string MTI_ECHO = "0800";
        public const string MTI_SIGNON = "0810";
        public const string MTI_FINTRN = "0200";
        public const string MTI_LORO = "0220";
        public const string MTI_REVERSAL = "0420";
        public const string PCODE_BALANCEENQUIRY = "31";
        public const string PCODE_MINISTMT = "38";
        public const string PCODE_CASHWDL = "01";
        public const string PCODE_POS = "00";
        public const string PCODE_PURCHASE = "02";
        public const string RESP_ERROR = "06";
        public const string RESP_SUCCESS = "00";
        public const string RESP_FUNCTIONNOTSUPPORTED = "40";

        string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
        string UDL_PATH;
        public string SERVICE_IP;
        public int PORT;
        public bool CheckStartup()
        {
            UDL_PATH = APP_PATH + @"Udl\ATMISOService.udl";
            if (!File.Exists(UDL_PATH))
            {
                Log("UDL file ATMISOService.udl not found on UDL Folder.");
                return false;
            }
            return true;
        }

        public void Log(string message)
        {
            if (!Directory.Exists(APP_PATH + "Logs"))
            {
                Directory.CreateDirectory(APP_PATH + "Logs");
            }

            string fileName = "ATMLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(APP_PATH + @"Logs\" + fileName, true))
            {
                file.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ": " + message);
            }
        }


        public void DumpRequest(string message, string fileName)
        {
            if (!Directory.Exists(APP_PATH + "Dump"))
            {
                Directory.CreateDirectory(APP_PATH + "Dump");
            }

            //string fileName = "ATMLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(APP_PATH + @"Dump\" + fileName, true))
            {
                file.WriteLine(message);
            }
        }

        public string ReadDump(string fileName)
        {
            FileStream fs = new FileStream(APP_PATH + @"Dump\" + fileName, FileMode.Open);
            int hexIn;
            String hex = "";
            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            {
                string singleHex = string.Format("{0:X}", hexIn);
                if (singleHex.Length > 1)
                {
                    singleHex = singleHex.Substring(1, 1);
                }
                hex += singleHex;
            }
            fs.Close();
            //File.Delete(APP_PATH + @"Dump\" + fileName);
            return hex;
        }

        public string BinToStr(byte[] data)
        {
            string retValue = "";
            for (int i = 0; i < data.Length; i++)
            {
                //string hexValue = data[i].ToString("X");
                string hexValue = string.Format("{0:X}", data[i]);
                string value = hexValue;
                if (true)
                {
                    uint hexInt = Convert.ToUInt32(hexValue, 16);
                    value = Convert.ToChar(hexInt).ToString();
                }
                retValue += value;
            }
            return retValue;
        }

        public string BinToHex(byte[] data)
        {
            string retValue = "";
            for (int i = 0; i < data.Length; i++)
            {
                retValue += data[i].ToString("X");
            }
            return retValue;
        }

        public string ProcessRequest(byte[] data)
        {
            /*
            try
            {
                int hexIn;
                String hex = "";
                for (int i = 0; (hexIn = data[i]) != -1; i++)
                {
                    string singleHex = string.Format("{0:X}", hexIn);
                    if (singleHex.Length > 1)
                    {
                        singleHex = singleHex.Substring(1, 1);
                    }
                    hex += singleHex;
                }
                Log("HEXTEXT:" + hex);
            }
            catch (Exception ex)
            {
                Log("HEXTEXT ERROR:" + ex.Message);
            }
            */



            /*SCT
            string requestData = BinToStr(data);
            */

            /*
            VISA NETWORK
            Failed to convert byte data to acceptable format when read directly.
            However, dumping to text file and reading it as byte, solves this issue.   
            */
            string requestData = BinToStr(data);
            try
            {
                string guid = Guid.NewGuid().ToString();  //Creating unique filename for each request.
                DumpRequest(requestData, guid); //Dumping the request data to text file
                requestData = ReadDump(guid); //Reading the data in acceptable format
                Log("Dump Value:" + requestData);
            }
            catch (Exception ex)
            {
                Log("Dump Exception:" + ex.Message);
            }

            Log("Request:" + requestData);
            string processData = requestData.Substring(4, requestData.Length - 4);
            Log("Process Data=" + processData);
            string binData = BinToHex(data);
            BIM_ISO8583.NET.ISO8583 isoBreaker = new BIM_ISO8583.NET.ISO8583();
            string[] message = null;
            string MTI = "0800";
            try
            {
                message = isoBreaker.Parse(processData);
            }
            catch (Exception e)
            {
                Log("PARSE ERROR:" + e.Message);
                message[39] = RESP_ERROR;
                goto EXITWITHFINISH;
            }

            Log("Request:" + BreakISO(message));


            MTI = message[129];
            if (MTI.Equals(MTI_ECHO) || MTI.Equals(MTI_SIGNON)) //ECHO and SIGN ON REQUEST
            {
                message[39] = RESP_SUCCESS;
                goto EXITWITHFINISH;
            }

            if (MTI.Equals(MTI_ECHO) || MTI.Equals(MTI_SIGNON) || MTI.Equals(MTI_FINTRN) || MTI.Equals(MTI_LORO) || MTI.Equals(MTI_REVERSAL))
            {
                ISOModel isoModel = MapModels(message);
                string _tranType = GetTranType(isoModel.PCode);
                CBSRequestData cbsRequestData = new CBSRequestData()
                {
                    messageType = GetMsgType(isoModel.MTI),
                    merchantType = GetMerchantType(isoModel.PCode),
                    tranType = _tranType,
                    pan = isoModel.PAN,
                    accountNumber = isoModel.AccountNo,
                    dateTime = isoModel.TxnDateTime,
                    trace = isoModel.Trace,
                    acquirerId = isoModel.Acquirer,
                    issuerId = isoModel.PAN.Substring(0, 6),
                    postingType = GetPostingType(isoModel.PCode),
                    amount = double.Parse(isoModel.TxnAmount),
                    terminalId = isoModel.TerminalID

                };


                //In reversal case, original data are packed in Field90
                if (MTI.Equals(MTI_REVERSAL))
                {
                    try
                    {
                        cbsRequestData.oldAcquirerId = isoModel.ReversalData.Substring(23, 8);
                        cbsRequestData.oldTrace = isoModel.ReversalData.Substring(4, 6);
                        cbsRequestData.oldDateTime = isoModel.ReversalData.Substring(10, 10);
                    }
                    catch (Exception ex)
                    {
                        Log("Error|Reversal Data:" + ex.Message);
                        message[39] = RESP_ERROR;
                        goto EXITWITHFINISH;
                    }
                }

                //CBSResponseData cbsResponseData = ProcessAtCBS(cbsRequestData);
                CBSResponseData cbsResponseData = new CBSHandler().Postgres(cbsRequestData);
                message[38] = cbsResponseData.authId;
                message[39] = cbsResponseData.respCode; //responseCode
                message[54] = BuildBalanceStr(cbsResponseData.ledgerBal, "01"); //Balance
                if (_tranType.Equals("MSTM"))
                    message[111] = cbsResponseData.data;
            }
            else
            {
                message[39] = RESP_FUNCTIONNOTSUPPORTED;
            }


        EXITWITHFINISH:
            Log("Response:" + BreakISO(message));
            string responseMessage = ""; ;
            try
            {
                responseMessage = isoBreaker.Build(message, GetResponseMTI(MTI));
            }
            catch (Exception e)
            {
                Log("Error|ProcessRequest:" + e.Message);
            }
            responseMessage = PrepareResponseMsg(responseMessage);
            Log("Response:" + responseMessage);

            return responseMessage;
        }


        public string GetMsgType(string MTI)
        {
            switch (MTI)
            {
                case MTI_FINTRN:
                    return "FIN";
                case MTI_LORO:
                    return "LORO";
                case MTI_REVERSAL:
                    return "REV";
                default:
                    return "";
            }
        }

        public string GetTranType(string PCODE)
        {
            switch (PCODE.Substring(0, 2))
            {
                case PCODE_BALANCEENQUIRY:
                    return "BAL";
                case PCODE_MINISTMT:
                    return "MSTM";
                case PCODE_CASHWDL:
                    return "CWD";
                case PCODE_PURCHASE:
                    return "PUR";
                case PCODE_POS:
                    return "PUR";
                default:
                    return "";
            }
        }

        public string GetMerchantType(string PCODE)
        {
            if (PCODE.Substring(0, 2).Equals(PCODE_POS) || PCODE.Substring(0, 2).Equals(PCODE_PURCHASE))
                return "POS";
            else
                return "ATM";
        }

        public string GetPostingType(string PCODE)
        {
            var pType = PCODE.Substring(0, 1);
            if (pType.Equals("0") || pType.Equals("1"))
                return "DR";
            if (pType.Equals("2"))
                return "CR";
            return "";
        }

        private string GetResponseMTI(string MTI)
        {
            switch (MTI)
            {
                case "0800":
                    return "0810";
                case "0200":
                    return "0210";
                case "0220":
                    return "0230";
                case "0420":
                    return "430";
                default:
                    return MTI;
            }
        }

        public string BuildBalanceStr(double balance, string balType, string accType = "10", string currencyCode = "524")
        {
            //accType 10: Saving, 20: Current
            //balType 01: Ledger Balance, 02: Available Balance
            string balStatus = "C";
            if (balance < 0)
                balStatus = "D";

            string balStr = (Convert.ToInt32(balance * 100)).ToString();
            balStr = balStr.PadLeft(12 - balStr.Length, '0');
            balStr = accType + balType + currencyCode + balStatus + balStr;
            return balStr;
        }


        public ISOModel MapModels(string[] message)
        {
            ISOModel iSOModel = new ISOModel
            {
                MTI = message[129],
                PCode = message[3],
                TxnDateTime = message[7],
                AccountNo = message[102],
                Trace = message[11],
                Acquirer = message[32],
                MerchantId = message[18],
                TerminalID = message[41],
                TerminalName = message[43],
                TxnAmount = message[4],
                LocalDate = message[13],
                LocalTime = message[12],
                PAN = message[2],
                CurrencyCode = message[49],
                ReversalData = message[90],
                TrnFee = message[30], //Not implenting now,  D Debit C Credit.
                RetreivalRefNo = message[37],
                ExpiryDate = message[14],
                AcquiringInstitution = message[19]
            };
            return iSOModel;
        }


        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }


        public string PrepareResponseMsg(string message)
        {
            try
            {
                string bitmapValues = message.Substring(4, 32);
                string messageLen = message.Length.ToString();
                messageLen = "0000".Substring(0, 4 - messageLen.Length) + messageLen;
                message = messageLen + message;
            }
            catch (Exception ex)
            {
                Log("Error|PrepareResponseMsg1:" + ex.Message);
            }
            return message;
        }
        public string GetServiceIp()
        {
            List<String> ips = new List<String>();
            String strHostName = Dns.GetHostName();
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                ips.Add(ipaddress.ToString());
            }

            return (ips[0]);
        }

        public string BreakISO(string[] message)
        {
            string breakedData = "";
            try
            {
                for (int i = 1; i < message.Length - 1; i++)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(message[i]))
                            breakedData += "[" + i.ToString() + "]: " + message[i] + ",";
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log("Error|BreakISO:" + ex.Message);
            }
            return breakedData;
        }



        public CBSResponseData ProcessAtCBS(CBSRequestData cbsRequestData)
        {
            CBSResponseData cbsResponseData = new CBSResponseData();
            OleDbConnection conn = new OleDbConnection("File Name=" + UDL_PATH);
            try
            {
                OleDbCommand command = new OleDbCommand("sp_iso_inout_cbs", conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@messageType", cbsRequestData.messageType);
                command.Parameters.AddWithValue("@merchantType", cbsRequestData.merchantType);
                command.Parameters.AddWithValue("@tranType", cbsRequestData.tranType);
                command.Parameters.AddWithValue("@pan", cbsRequestData.pan);
                command.Parameters.AddWithValue("@accountNumber", cbsRequestData.accountNumber);
                command.Parameters.AddWithValue("@dateTime", cbsRequestData.dateTime);
                command.Parameters.AddWithValue("@amount", cbsRequestData.amount);
                command.Parameters.AddWithValue("@trace", cbsRequestData.trace);
                command.Parameters.AddWithValue("@acquirerId", cbsRequestData.acquirerId);
                command.Parameters.AddWithValue("@issuerId", cbsRequestData.issuerId);
                command.Parameters.AddWithValue("@terminalId", cbsRequestData.terminalId);
                command.Parameters.AddWithValue("@postingType", cbsRequestData.postingType);
                command.Parameters.AddWithValue("@tranCharge", cbsRequestData.trancharge);


                OleDbParameter outputParamRespCode = new OleDbParameter("@respCode", OleDbType.VarChar, 2);
                outputParamRespCode.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamRespCode);

                OleDbParameter outputParamBalamt = new OleDbParameter("@ledgerBal", OleDbType.Numeric, 24);
                outputParamBalamt.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamBalamt);

                OleDbParameter outputParamPAN = new OleDbParameter("@respPan", OleDbType.VarChar, 19);
                outputParamPAN.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamPAN);

                OleDbParameter outputParamRespAccount = new OleDbParameter("@respAccount", OleDbType.VarChar, 20);
                outputParamRespAccount.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamRespAccount);


                command.Parameters.AddWithValue("@oldTrace", cbsRequestData.oldTrace);
                command.Parameters.AddWithValue("@oldDateTime", cbsRequestData.oldDateTime);
                command.Parameters.AddWithValue("@oldAcquirerid", cbsRequestData.oldAcquirerId);
                command.Parameters.AddWithValue("@retreivalRefNo", cbsRequestData.retreivalRefNo);
                command.Parameters.AddWithValue("@currencyCode", cbsRequestData.currencyCode);


                OleDbParameter outputParamAuthId = new OleDbParameter("@authId", OleDbType.VarChar, 6);
                outputParamAuthId.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamAuthId);

                OleDbParameter outputParamData = new OleDbParameter("@Ministmt", OleDbType.VarChar, 350);
                outputParamData.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(outputParamData);


                conn.Open();
                command.ExecuteNonQuery();
                cbsResponseData.respCode = outputParamRespCode.Value.ToString();
                cbsResponseData.ledgerBal = double.Parse(outputParamBalamt.Value.ToString());
                cbsResponseData.respAccount = outputParamPAN.Value.ToString();
                cbsResponseData.respPan = outputParamRespAccount.Value.ToString();
                cbsResponseData.authId = outputParamAuthId.Value.ToString();
                cbsResponseData.data = outputParamData.Value.ToString();
            }
            catch (Exception ex)
            {
                cbsResponseData.respCode = RESP_ERROR;
                Log("Error|ProcessAtCBS" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return cbsResponseData;
        }
    }
}

