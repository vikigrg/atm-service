using ATMService.Models;
using Dapper;
using Npgsql;
using System;
using System.Data;

namespace ATMService.Services
{
    public class CBSHandler
    {
        public CBSResponseData Postgres(CBSRequestData cbsRequestData)
        {
            CBSResponseData cbsResponseData = new CBSResponseData();
            string _connectionString = "User ID = postgres; Password = 123; Host = localhost; Port = 5432; Database = ATMTest; Pooling = true; Min Pool Size = 0; Max Pool Size = 100; Connection Lifetime = 0;";
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("messageType", cbsRequestData.messageType);
                    parameters.Add("merchantType", cbsRequestData.merchantType);
                    parameters.Add("tranType", cbsRequestData.tranType);
                    parameters.Add("pan", cbsRequestData.pan);
                    parameters.Add("accountnumber", cbsRequestData.accountNumber);
                    parameters.Add("dateTime", cbsRequestData.dateTime);
                    parameters.Add("amount", cbsRequestData.amount);
                    parameters.Add("trace", cbsRequestData.trace);
                    parameters.Add("acquirerId", cbsRequestData.acquirerId);
                    parameters.Add("issuerId", cbsRequestData.issuerId);
                    parameters.Add("terminalId", cbsRequestData.terminalId);
                    parameters.Add("postingType", cbsRequestData.postingType);
                    parameters.Add("tranCharge", cbsRequestData.trancharge);
                    parameters.Add("respCode", dbType: DbType.String, direction: ParameterDirection.Output);
                    parameters.Add("ledgerBal", dbType: DbType.VarNumeric, direction: ParameterDirection.Output);
                    parameters.Add("respPan", dbType: DbType.String, direction: ParameterDirection.Output);
                    parameters.Add("respAccount", dbType: DbType.String, direction: ParameterDirection.Output);
                    parameters.Add("oldTrace", cbsRequestData.oldTrace);
                    parameters.Add("oldDateTime", cbsRequestData.oldDateTime);
                    parameters.Add("oldAcquirerid", cbsRequestData.oldAcquirerId);
                    parameters.Add("currencyCode", cbsRequestData.currencyCode);
                    parameters.Add("AuthId", dbType: DbType.String, direction: ParameterDirection.Output);
                    parameters.Add("retreivalRefNo", dbType: DbType.String, direction: ParameterDirection.Output);
                    parameters.Add("data", cbsRequestData);
                    connection.Query("sp_iso_inout_cbs", parameters, commandType: CommandType.StoredProcedure);

                    cbsResponseData.respCode = parameters.Get<string>("respCode");
                    cbsResponseData.ledgerBal = parameters.Get<double>("ledgerBal");
                    cbsResponseData.respPan = parameters.Get<string>("respPan");
                    cbsResponseData.respAccount = parameters.Get<string>("respAccount");
                    cbsResponseData.authId = parameters.Get<string>("AuthId");
                    //cbsResponseData. = parameters.Get<string>("respCode");
                }
            }
            catch (Exception ex)
            {
                cbsResponseData.respCode = ISOHandler.RESP_ERROR;
                new ISOHandler().Log("Error|ProcessAtCBS" + ex.Message);
            }
            return cbsResponseData;
        }
    }
}
