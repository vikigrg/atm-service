/*
Date:2023-05-23
Author:Bikram Gurung
Description:
ISO8583 ATM Service for Windows.
Keep your own logic inside sp_iso_inout_cbs procedure. 
Currently this only logs the request and response.
*/


If Not Exists(Select * from sys.sysdatabases where name='ISO8583')
Create database ISO8583

Go

USE [ISO8583]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


If Not Exists(Select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME='ISOLog')
Create Table ISOLog
(
	Recid BigInt Identity(1,1) not null,
	TrnDateTime DateTime not null default(GetDate()),
	 MessageType varchar(4),
	 MerchantType varchar(4),
	 TranType varchar(6),
	 PAN varchar(19),
	 AccountNumber varchar(20),
	 TxnDateTime varchar(10),
	 Amount numeric(18,6),
	 Trace varchar(6),
	 AcquirerId varchar(10),
	 IssuerId varchar(10),
	 TerminalId varchar(8),
	 PostingType varchar(2),
	 TranCharge numeric(18,6),
	 OldTrace varchar(6),
	 OldDateTime varchar(10),
	 OldAcquirerid varchar(10),
	 RetreivalRefNo Varchar(10),
	 CurrencyCode Varchar(10),
	 AuthId Varchar(6) 
)

Go

If Not Exists(Select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME='ISOLog')
Create table ISO8583Log
(
	LogId BigInt Identity(1,1) not null,
	RequestTimeStamp DateTime not null,
	RequestLog Nvarchar(1000) null,
	ResponseTimeStamp DateTime not null,
	ResponseLog Nvarchar(1000) null
)


Go

If Object_id('sp_iso_inout_cbs') is not null
Drop Procedure sp_iso_inout_cbs
Go
Create PROCEDURE [dbo].[sp_iso_inout_cbs]
@messageType varchar(4),
@merchantType varchar(4),
@tranType varchar(6),
@pan varchar(19),
@accountNumber varchar(20),
@dateTime varchar(10),
@amount numeric(18,6),
@trace varchar(6),
@acquirerId varchar(10),
@issuerId varchar(10),
@terminalId varchar(8),
@postingType varchar(2),
@tranCharge numeric(18,6),
@respCode varchar(2) out,
@ledgerBal numeric(18,6) out,
@respPan varchar(19) out,
@respAccount varchar(20) out,
@oldTrace varchar(6),
@oldDateTime varchar(10),
@oldAcquirerid varchar(10),
@retreivalRefNo Varchar(10)='NA',
@currencyCode Varchar(10)='524', --Default currency set to NPR
@AuthId Varchar(6) out, --Unique Id must be returned from CBS
@Data Varchar(350) out --ministatement

AS
/*
 Declare @messageType varchar(4)
 Declare @merchantType varchar(4)
 Declare @tranType varchar(6)
 Declare @pan varchar(19)='123456789123456789'
 Declare @accountNumber varchar(20)='123456789'
 Declare @dateTime varchar(10)
 Declare @amount numeric(18,6)
 Declare @trace varchar(6)
 Declare @acquirerId varchar(10)
 Declare @issuerId varchar(10)
 Declare @terminalId varchar(8)
 Declare @postingType varchar(2)
 Declare @tranCharge numeric(18,6)
 Declare @respCode varchar(2) 
 Declare @ledgerBal numeric(18,6) 
 Declare @respPan varchar(19)
 Declare @respAccount varchar(20)
 Declare @oldTrace varchar(6)
 Declare @oldDateTime varchar(10)
 Declare @oldAcquirerid varchar(10)
 Declare @currencyCode Varchar(10)
 Declare @AuthId Varchar(6) 
 Declare @retreivalRefNo Varchar(10)
 Declare @data varchar(350)
 Exec sp_iso_inout_cbs @messagetype,@merchantType,@tranType,@pan,@accountnumber,@datetime,@amount,@trace,@acquirerId,@issuerId,@terminalId,@postingtype,@trancharge,
                      @respCode out,@ledgerBal out,@respPan out,@respAccount out,@oldTrace,@oldDateTime,@oldAcquirerId,@retreivalRefNo,@currencyCode,@AuthId out,@data out

Select @respCode,@ledgerBal,@respPan,@respAccount,@AuthId,@data


Select * from ISOLog
*/
--Static variable Declaration Section---------------
 Declare @localDate VARCHAR(15)
 Declare @localTime VARCHAR(15)
Declare @ErrorNumber int
Declare @ErrorLine INT
Declare @ErrorMessage NVARCHAR(4000)
Declare @ErrorSeverity INT
Declare @ErrorState INT
Declare @procname VARCHAR(100)
Declare @query nvarchar(1000)
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN
			set @AuthId=Left(Replace(NewId(),'-',''),6)
			Insert into ISOLog(MessageType,MerchantType,TranType,PAN,AccountNumber,TxnDateTime,Amount,Trace,AcquirerId,IssuerId,TerminalId,PostingType,TranCharge,OldTrace,OldDateTime,OldAcquirerid,RetreivalRefNo,CurrencyCode,AuthId)
			Values(@messagetype,@merchantType,@tranType,@pan,@accountnumber,@datetime,@amount,@trace,@acquirerId,@issuerId,@terminalId,@postingtype,@trancharge,@oldTrace,
                   @oldDateTime,@oldAcquirerId,@retreivalRefNo,@currencyCode,@AuthId)
			
			--Dummy Data
			set @respCode='00';
			set @respPan=@pan
			set @respAccount=@accountNumber
			set @ledgerBal=1000 
			set @data='[08/19/2009 60507 DR 1000.00][08/19/2009 60507 DR 1000.00]' 
        END
    END TRY
    BEGIN CATCH
                set @ErrorNumber  = ERROR_NUMBER();
                set @ErrorLine  = ERROR_LINE();
                set @ErrorMessage  = ERROR_MESSAGE();
                set @ErrorSeverity = ERROR_SEVERITY();
                set @ErrorState  = ERROR_STATE();
                set @procname='sp_iso_inout_cbs';
                RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END