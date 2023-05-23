namespace ATMService.Models
{
    public class ISOModel
    {
        public string MTI { get; set; }
        public string PCode { get; set; }
        public string TxnDateTime { get; set; }
        public string AccountNo { get; set; }
        public string Trace { get; set; }
        public string Acquirer { get; set; }
        public string MerchantId { get; set; }
        public string TerminalID { get; set; }
        public string TerminalName { get; set; }
        public string TxnAmount { get; set; }
        public string LocalDate { get; set; }
        public string LocalTime { get; set; }
        public string PAN { get; set; }
        public string CurrencyCode { get; set; }
        public string ReversalData { get; set; }
        public string TrnFee { get; set; }
        public string RetreivalRefNo { get; set; }
        public string ExpiryDate { get; set; } //14
        public string AcquiringInstitution { get; set; } //19

    }


    public class CBSResponseData
    {
        public string respCode, respPan, respAccount, authId, data;
        public double ledgerBal;
        //public string ministatement;
    }


    public class CBSRequestData
    {
        public string messageType, merchantType, tranType, pan, accountNumber, dateTime, trace, acquirerId, issuerId, postingType, oldTrace, oldDateTime, oldAcquirerId, currencyCode, retreivalRefNo, terminalId;
        public double amount, trancharge;

        public CBSRequestData()
        {
            oldTrace = "";
            oldDateTime = "";
            oldAcquirerId = "";

        }
    }
}
