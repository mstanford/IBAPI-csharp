using System;
using System.Collections.Generic;
using System.Text;

namespace IBAPI
{
	public class TWSClient
	{
		public const int ClientVersion = 63;//API v. 9.71

		private readonly string _host;
		private readonly int _port;
		private readonly int _clientId;
		private readonly bool _extraAuth = false;
		private readonly string _account;
		private readonly Action<Dictionary<string, object>> _process;
		private int _serverVersion = -1;
		private int _nextOrderId = -1;
		private System.Net.Sockets.TcpClient _tcpClient;
		private System.IO.Stream _stream;
		private System.IO.BinaryWriter _writer;
		private System.IO.BinaryReader _reader;
		private System.IO.MemoryStream _tempStream = new System.IO.MemoryStream();

		private System.Threading.ManualResetEvent _stopEvent;
		private System.Threading.Thread _runner;

        public TWSClient(string host, int port, int clientId, string account, Action<Dictionary<string, object>> process)
        {
			_host = host;
			_port = port;
			_clientId = clientId;
			_account = account;
			_process = process;
        }

		public bool IsConnected = false;
		public string ConnectionTime;

		public void Connect()
		{
			if (IsConnected)
				throw new System.Exception();

			_nextOrderId = -1;

			_tcpClient = new System.Net.Sockets.TcpClient(_host, _port);
			_stream = _tcpClient.GetStream();
			_writer = new System.IO.BinaryWriter(_stream);
			_reader = new System.IO.BinaryReader(_stream);

			_writer.Write(UTF8Encoding.UTF8.GetBytes(ClientVersion.ToString()));
			_writer.Write((byte)0);

			_serverVersion = IncomingMessage.ReadInt(_reader);
			if (_serverVersion < 38)
				throw new System.Exception();

			ConnectionTime = IncomingMessage.ReadString(_reader);

			IsConnected = true;

			if (_serverVersion >= 3)
			{
				if (_serverVersion < LINKING)
				{
					_writer.Write(UTF8Encoding.UTF8.GetBytes(_clientId.ToString()));
					_writer.Write((byte)0);
				}
				else if (!_extraAuth)
				{
                    throw new System.Exception();

					//OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
                    //outgoingMessage.Write((int)OutgoingMessage.StartApi);
                    //outgoingMessage.Write(1);
                    //outgoingMessage.Write(_clientId);
                    //outgoingMessage.Flush();
                    //Send(_tempStream);
				}
			}

			_stopEvent = new System.Threading.ManualResetEvent(false);
			_runner = new System.Threading.Thread(ReadAndProcessMessages);
			_runner.Start();
			while (!_runner.IsAlive) { }

			// One good way of knowing if we can proceed is by monitoring the order's nextValidId reception which comes down automatically after connecting.
			while (_nextOrderId <= 0) { }
		}

		private void ReadAndProcessMessages()
		{
			try
			{
				while (!_stopEvent.WaitOne(0))
				{
					Dictionary<string, object> message = IncomingMessage.Read(_reader, _serverVersion);

					switch ((int)message["MessageTypeId"])
					{
						case IncomingMessage.ManagedAccounts:
							if (!_account.Equals((string)message["AccountsList"]))
								throw new System.Exception();
							break;
						case IncomingMessage.NextValidId:
							_nextOrderId = (int)message["OrderId"];
							break;
					}

                    if (message.ContainsKey("Account") && !_account.Equals(message["Account"]))
                    {
                        //TODO log this error.
                        ;
                    }
                    else
                    {
                        _process(message);
                    }
				}
			}
            catch (Exception exception)
			{
                if (exception.Message.Equals("Unable to read data from the transport connection: A blocking operation was interrupted by a call to WSACancelBlockingCall.") || 
                    exception.Message.Equals("Unable to read beyond the end of the stream."))
                {
                }
                else
                {
                    Dictionary<string, object> message = new Dictionary<string, object>();
                    message["MessageTypeId"] = IncomingMessage.Error;
                    message["MessageType"] = "Exception";
                    message["ErrorMessage"] = exception.Message;
                    message["ErrorStackTrace"] = exception.StackTrace;

                    _process(message);
                }
			}
			Disconnect();
		}

		public void Disconnect()
		{
			if (_writer == null)
				return;

			IsConnected = false;
			_serverVersion = -1;

			if (_writer != null)
			{
				_writer.Close();
				_writer = null;
				_stream.Close();
				_stream = null;
			}
		}



		// MinServerVersion
        public const int MIN_VERSION = 38;

        //shouldn't these all be deprecated?
        public const int HISTORICAL_DATA = 24;
        public const int CURRENT_TIME = 33;
        public const int REAL_TIME_BARS = 34;
        public const int SCALE_ORDERS = 35;
        public const int SNAPSHOT_MKT_DATA = 35;
        public const int SSHORT_COMBO_LEGS = 35;
        public const int WHAT_IF_ORDERS = 36;
        public const int CONTRACT_CONID = 37;

        public const int PTA_ORDERS = 39;
        public const int FUNDAMENTAL_DATA = 40;
        public const int UNDER_COMP = 40;
        public const int CONTRACT_DATA_CHAIN = 40;
        public const int SCALE_ORDERS2 = 40;
        public const int ALGO_ORDERS = 41;
        public const int EXECUTION_DATA_CHAIN = 42;
        public const int NOT_HELD = 44;
        public const int SEC_ID_TYPE = 45;
        public const int PLACE_ORDER_CONID = 46;
        public const int REQ_MKT_DATA_CONID = 47;
        public const int REQ_CALC_IMPLIED_VOLAT = 49;
        public const int REQ_CALC_OPTION_PRICE = 50;
        public const int CANCEL_CALC_IMPLIED_VOLAT = 50;
        public const int CANCEL_CALC_OPTION_PRICE = 50;
        public const int SSHORTX_OLD = 51;
        public const int SSHORTX = 52;
        public const int REQ_GLOBAL_CANCEL = 53;
        public const int HEDGE_ORDERS = 54;
        public const int REQ_MARKET_DATA_TYPE = 55;
        public const int OPT_OUT_SMART_ROUTING = 56;
        public const int SMART_COMBO_ROUTING_PARAMS = 57;
        public const int DELTA_NEUTRAL_CONID = 58;
        public const int SCALE_ORDERS3 = 60;
        public const int ORDER_COMBO_LEGS_PRICE = 61;
        public const int TRAILING_PERCENT = 62;
        public const int DELTA_NEUTRAL_OPEN_CLOSE = 66;
        public const int ACCT_SUMMARY = 67;
        public const int TRADING_CLASS = 68;
        public const int SCALE_TABLE = 69;
        public const int LINKING = 70;
        public const int ALGO_ID = 71;

		///**
		// * @brief Requests a specific account's summary.
		// * This method will subscribe to the account summary as presented in the TWS' Account Summary tab. The data is returned at EWrapper::accountSummary
		// * @param reqId the unique request idntifier.
		// * @param group set to "All" to return account summary data for all accounts, or set to a specific Advisor Account Group name that has already been created in TWS Global Configuration.
		// * @params tags a comma separated list with the desired tags:
		// *      - AccountType
		// *      - NetLiquidation,
		// *      - TotalCashValue — Total cash including futures pnl
		// *      - SettledCash — For cash accounts, this is the same as TotalCashValue
		// *      - AccruedCash — Net accrued interest
		// *      - BuyingPower — The maximum amount of marginable US stocks the account can buy
		// *      - EquityWithLoanValue — Cash + stocks + bonds + mutual funds
		// *      - PreviousEquityWithLoanValue,
		// *      - GrossPositionValue — The sum of the absolute value of all stock and equity option positions
		// *      - RegTEquity,
		// *      - RegTMargin,
		// *      - SMA — Special Memorandum Account
		// *      - InitMarginReq,
		// *      - MaintMarginReq,
		// *      - AvailableFunds,
		// *      - ExcessLiquidity,
		// *      - Cushion — Excess liquidity as a percentage of net liquidation value
		// *      - FullInitMarginReq,
		// *      - FullMaintMarginReq,
		// *      - FullAvailableFunds,
		// *      - FullExcessLiquidity,
		// *      - LookAheadNextChange — Time when look-ahead values take effect
		// *      - LookAheadInitMarginReq,
		// *      - LookAheadMaintMarginReq,
		// *      - LookAheadAvailableFunds,
		// *      - LookAheadExcessLiquidity,
		// *      - HighestSeverity — A measure of how close the account is to liquidation
		// *      - DayTradesRemaining — The Number of Open/Close trades a user could put on before Pattern Day Trading is detected. A value of "-1" means that the user can put on unlimited day trades.
		// *      - Leverage — GrossPositionValue / NetLiquidation
		// * @sa cancelAccountSummary, EWrapper::accountSummary, EWrapper::accountSummaryEnd
		// */
		public void RequestAccountSummary(int requestId, string group, string tags)
		{
			int VERSION = 1;
			//if (!CheckConnection())
			//    return;

			//if (!Check_serverVersion(reqId, MinServerVer.ACCT_SUMMARY,
			//    " It does not support account summary requests."))
			//    return;

			OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
			outgoingMessage.Write((int)OutgoingMessage.RequestAccountSummary);
			outgoingMessage.Write(VERSION);
			outgoingMessage.Write(requestId);
			outgoingMessage.Write(group);
			outgoingMessage.Write(tags);
			Send(_tempStream);
		}

		// Requests all positions from all accounts.  cancelPositions, EWrapper::position, EWrapper::positionEnd
		public void RequestPositions()
		{
			//if (!CheckConnection())
			//    return;
			//if (!Check_serverVersion(MinServerVer.ACCT_SUMMARY, " It does not support position requests."))
			//    return;

			int VERSION = 1;
			OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
			outgoingMessage.Write(OutgoingMessage.RequestPositions);
			outgoingMessage.Write(VERSION);
			Send(_tempStream);
		}

		///**
		// * @brief Requests contract information.
		// * This method will provide all the contracts matching the contract provided. It can also be used to retrieve complete options and futures chains. This information will be returned at EWrapper:contractDetails
		// * @param reqId the unique request identifier.
		// * @param contract the contract used as sample to query the available contracts. Typically, it will contain the Contract::Symbol, Contract::Currency, Contract::SecType, Contract::Exchange
		// * @sa EWrapper::contractDetails
		// */
		public void RequestContractDetails(
			int requestId,
			int contractId,
			string symbol,
			string securityType,
			string expiry,
			double strike,
			string right,
			string multiplier,
			string exchange,
			string currency,
			string localSymbol,
			string tradingClass,
			bool includeExpired,
			string securityIdType,
			string securityId)
		{
			//if (!CheckConnection())
			//    return;

			//if (!IsEmpty(contract.SecIdType) || !IsEmpty(contract.SecId))
			//{
			//    if (!Check_serverVersion(reqId, MinServerVer.SEC_ID_TYPE, " It does not support secIdType not secId attributes"))
			//        return;
			//}

			//if (!IsEmpty(contract.TradingClass))
			//{
			//    if (!Check_serverVersion(reqId, MinServerVer.TRADING_CLASS, " It does not support the TradingClass parameter when requesting contract details."))
			//        return;
			//}

			int VERSION = 7;

			OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
			outgoingMessage.Write(OutgoingMessage.RequestContractData);
			outgoingMessage.Write(VERSION);//version
			if (_serverVersion >= CONTRACT_DATA_CHAIN)
			{
				outgoingMessage.Write(requestId);
			}
			if (_serverVersion >= CONTRACT_CONID)
			{
				outgoingMessage.Write(contractId);
			}
			outgoingMessage.Write(symbol);
			outgoingMessage.Write(securityType);
			outgoingMessage.Write(expiry);
			outgoingMessage.Write(strike);
			outgoingMessage.Write(right);
			if (_serverVersion >= 15)
			{
				outgoingMessage.Write(multiplier);
			}
			outgoingMessage.Write(exchange);
			outgoingMessage.Write(currency);
			outgoingMessage.Write(localSymbol);
			if (_serverVersion >= TRADING_CLASS)
			{
				outgoingMessage.Write(tradingClass);
			}
			if (_serverVersion >= 31)
			{
				outgoingMessage.Write(includeExpired);
			}
			if (_serverVersion >= SEC_ID_TYPE)
			{
				outgoingMessage.Write(securityIdType);
				outgoingMessage.Write(securityId);
			}
			Send(_tempStream);
		}

		///**
		// * @brief Requests real time market data.
		// * This function will return the product's market data. It is important to notice that only real time data can be delivered via the API.
		// * @param tickerId the request's identifier
		// * @param contract the Contract for which the data is being requested
		// * @param genericTickList comma separated ids of the available generic ticks:
		// *      - 100 	Option Volume (currently for stocks)
		// *      - 101 	Option Open Interest (currently for stocks) 
		// *      - 104 	Historical Volatility (currently for stocks)
		// *      - 106 	Option Implied Volatility (currently for stocks)
		// *      - 162 	Index Future Premium 
		// *      - 165 	Miscellaneous Stats 
		// *      - 221 	Mark Price (used in TWS P&L computations) 
		// *      - 225 	Auction values (volume, price and imbalance) 
		// *      - 233 	RTVolume - contains the last trade price, last trade size, last trade time, total volume, VWAP, and single trade flag.
		// *      - 236 	Shortable
		// *      - 256 	Inventory 	 
		// *      - 258 	Fundamental Ratios 
		// *      - 411 	Realtime Historical Volatility 
		// *      - 456 	IBDividends
		// * @param snapshot when set to true, it will provide a single snapshot of the available data. Set to false if you want to receive continuous updates.
		// * @sa cancelMktData, EWrapper::tickPrice, EWrapper::tickSize, EWrapper::tickString, EWrapper::tickEFP, EWrapper::tickGeneric, EWrapper::tickOption, EWrapper::tickSnapshotEnd
		// */
		public void RequestMarketData(int tickerId, int contractId, string symbol, string securityType, string expiry, double strike, string right, string multiplier, string exchange, string primaryExchange, string currency, string localSymbol, string tradingClass, bool snapshot)
		{
			//if (!CheckConnection())
			//    return;

			//if (snapshot && !Check_serverVersion(tickerId, MinServerVer.SNAPSHOT_MKT_DATA,
			//    "It does not support snapshot market data requests."))
			//    return;

			//if (contract.UnderComp != null && !Check_serverVersion(tickerId, MinServerVer.UNDER_COMP,
			//    " It does not support delta-neutral orders"))
			//    return;


			//if (contract.ConId > 0 && !Check_serverVersion(tickerId, MinServerVer.CONTRACT_CONID,
			//    " It does not support ConId parameter"))
			//    return;

			//if (!Util.StringIsEmpty(contract.TradingClass) && !Check_serverVersion(tickerId, MinServerVer.TRADING_CLASS,
			//    " It does not support trading class parameter in reqMktData."))
			//    return;

			//contract.Symbol = Symbols[i];
			//contract.SecType = "STK";
			//contract.Currency = "USD";
			//contract.Exchange = "SMART";
			//_clientSocket.reqMktData(i, contract, "", true, mktDataOptions);

			int version = 11;
			OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
			outgoingMessage.Write(OutgoingMessage.RequestMarketData);
			outgoingMessage.Write(version);
			outgoingMessage.Write(tickerId);
			if (_serverVersion >= CONTRACT_CONID)
				outgoingMessage.Write(contractId);
			outgoingMessage.Write(symbol);
			outgoingMessage.Write(securityType);
			outgoingMessage.Write(expiry);
			outgoingMessage.Write(strike);
			outgoingMessage.Write(right);
			if (_serverVersion >= 15)
				outgoingMessage.Write(multiplier);
			outgoingMessage.Write(exchange);
			if (_serverVersion >= 14)
				outgoingMessage.Write(primaryExchange);
			outgoingMessage.Write(currency);
			if (_serverVersion >= 2)
				outgoingMessage.Write(localSymbol);
			if (_serverVersion >= TRADING_CLASS)
				outgoingMessage.Write(tradingClass);
			//if (_serverVersion >= 8 && Constants.BagSecType.Equals(contract.SecType))
			//{
			//    //if (contract.ComboLegs == null)
			//    //{
			//    outgoingMessage.Write(0);
			//    //}
			//    //else
			//    //{
			//    //    outgoingMessage.Write(contract.ComboLegs.Count);
			//    //    for (int i = 0; i < contract.ComboLegs.Count; i++)
			//    //    {
			//    //        ComboLeg leg = contract.ComboLegs[i];
			//    //        outgoingMessage.Write(leg.ConId);
			//    //        outgoingMessage.Write(leg.Ratio);
			//    //        outgoingMessage.Write(leg.Action);
			//    //        outgoingMessage.Write(leg.Exchange);
			//    //    }
			//    //}
			//}

			if (_serverVersion >= UNDER_COMP)
			{
				//if (contract.UnderComp != null)
				//{
				//    outgoingMessage.Write(true);
				//    outgoingMessage.Write(contract.UnderComp.ConId);
				//    outgoingMessage.Write(contract.UnderComp.Delta);
				//    outgoingMessage.Write(contract.UnderComp.Price);
				//}
				//else
				//{
				outgoingMessage.Write(false);
				//}
			}
			if (_serverVersion >= 31)
			{
				outgoingMessage.Write("");
			}
			if (_serverVersion >= SNAPSHOT_MKT_DATA)
			{
				outgoingMessage.Write(snapshot);
			}
			if (_serverVersion >= LINKING)
			{
				outgoingMessage.Write("");
				//outgoingMessage.Write(TagValueListToString(mktDataOptions));
			}
			Send(_tempStream);
		}

		// Places an order
		// id the order's unique identifier. Use a sequential id starting with the id received at the nextValidId method.
		// contract the order's contract
		// order the order
		// nextValidId, reqAllOpenOrders, reqAutoOpenOrders, reqOpenOrders, cancelOrder, reqGlobalCancel, EWrapper::openOrder, EWrapper::orderStatus, Order, Contract
		public int PlaceOrder(int contractId, string symbol, string secType, string expiry, double strike, string right, string multiplier, string exchange, string primaryExchange, string currency, string localSymbol, string tradingClass, string secIdType, string secId, Order order)
		{
			//if (!VerifyOrder(order, orderId, StringsAreEqual(Constants.BagSecType, contract.SecType)))
			//    return;
			//if (!VerifyOrderContract(contract, orderId))
			//    return;

			int orderId = _nextOrderId;
			_nextOrderId++;

			OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
			outgoingMessage.Write((int)OutgoingMessage.PlaceOrder);
			outgoingMessage.Write((_serverVersion < NOT_HELD) ? 27 : 43);
			outgoingMessage.Write(orderId);

			if (_serverVersion >= PLACE_ORDER_CONID)
				outgoingMessage.Write(contractId);
			outgoingMessage.Write(symbol);
			outgoingMessage.Write(secType);
			outgoingMessage.Write(expiry);
			outgoingMessage.Write(strike);
			outgoingMessage.Write(right);
			if (_serverVersion >= 15)
				outgoingMessage.Write(multiplier);
			outgoingMessage.Write(exchange);
			if (_serverVersion >= 14)
				outgoingMessage.Write(primaryExchange);
			outgoingMessage.Write(currency);
			if (_serverVersion >= 2)
				outgoingMessage.Write(localSymbol);
			if (_serverVersion >= TRADING_CLASS)
				outgoingMessage.Write(tradingClass);
			if (_serverVersion >= SEC_ID_TYPE)
			{
				outgoingMessage.Write(secIdType);
				outgoingMessage.Write(secId);
			}

			outgoingMessage.Write(order.Action);
			outgoingMessage.Write(order.TotalQuantity);
			outgoingMessage.Write(order.OrderType);
			if (_serverVersion < ORDER_COMBO_LEGS_PRICE)
			{
				outgoingMessage.Write(order.LimitPrice == Double.MaxValue ? 0 : order.LimitPrice);
			}
			else
			{
				outgoingMessage.WriteMax(order.LimitPrice);
			}
			if (_serverVersion < TRAILING_PERCENT)
			{
				outgoingMessage.Write(order.AuxPrice == Double.MaxValue ? 0 : order.AuxPrice);
			}
			else
			{
				outgoingMessage.WriteMax(order.AuxPrice);
			}

			// outgoingMessage.Write extended order fields
			outgoingMessage.Write(order.TimeInForce);
			outgoingMessage.Write(order.OneCancelsAllGroup);
			outgoingMessage.Write(order.Account);
			outgoingMessage.Write(order.OpenClose);
			outgoingMessage.Write(order.Origin);
			outgoingMessage.Write(order.OrderRef);
			outgoingMessage.Write(order.Transmit);
			if (_serverVersion >= 4)
			{
				outgoingMessage.Write(order.ParentId);
			}

			if (_serverVersion >= 5)
			{
				outgoingMessage.Write(order.BlockOrder);
				outgoingMessage.Write(order.SweepToFill);
				outgoingMessage.Write(order.DisplaySize);
				outgoingMessage.Write(order.TriggerMethod);
				if (_serverVersion < 38)
				{
					// will never happen
					outgoingMessage.Write(/* order.ignoreRth */ false);
				}
				else
				{
					outgoingMessage.Write(order.OutsideRth);
				}
			}

			if (_serverVersion >= 7)
			{
				outgoingMessage.Write(order.Hidden);
			}

			// outgoingMessage.Write combo legs for BAG requests
			bool isBag = false; //StringsAreEqual(Constants.BagSecType, contract.SecType);
			//if (_serverVersion >= 8 && isBag)
			//{
			//    //if (contract.ComboLegs == null)
			//    //{
			//    outgoingMessage.Write(0);
			//    //}
			//    //else
			//    //{
			//    //    outgoingMessage.Write(contract.ComboLegs.Count);

			//    //    ComboLeg comboLeg;
			//    //    for (int i = 0; i < contract.ComboLegs.Count; i++)
			//    //    {
			//    //        comboLeg = (ComboLeg)contract.ComboLegs[i];
			//    //        outgoingMessage.Write(comboLeg.ConId);
			//    //        outgoingMessage.Write(comboLeg.Ratio);
			//    //        outgoingMessage.Write(comboLeg.Action);
			//    //        outgoingMessage.Write(comboLeg.Exchange);
			//    //        outgoingMessage.Write(comboLeg.OpenClose);

			//    //        if (serverVersion >= MinServerVer.SSHORT_COMBO_LEGS)
			//    //        {
			//    //            outgoingMessage.Write(comboLeg.ShortSaleSlot);
			//    //            outgoingMessage.Write(comboLeg.DesignatedLocation);
			//    //        }
			//    //        if (serverVersion >= MinServerVer.SSHORTX_OLD)
			//    //        {
			//    //            outgoingMessage.Write(comboLeg.ExemptCode);
			//    //        }
			//    //    }
			//    //}
			//}

			//// add order combo legs for BAG requests
			//if (_serverVersion >= ORDER_COMBO_LEGS_PRICE && isBag)
			//{
			//    //if (order.OrderComboLegs == null)
			//    //{
			//    outgoingMessage.Write(0);
			//    //}
			//    //else
			//    //{
			//    //    outgoingMessage.Write(order.OrderComboLegs.Count);

			//    //    for (int i = 0; i < order.OrderComboLegs.Count; i++)
			//    //    {
			//    //        OrderComboLeg orderComboLeg = order.OrderComboLegs[i];
			//    //        outgoingMessage.WriteMax(orderComboLeg.Price);
			//    //    }
			//    //}
			//}

			//if (_serverVersion >= SMART_COMBO_ROUTING_PARAMS && isBag)
			//{
			//    List<KeyValuePair<string, string>> smartComboRoutingParams = order.SmartComboRoutingParams;
			//    int smartComboRoutingParamsCount = smartComboRoutingParams == null ? 0 : smartComboRoutingParams.Count;
			//    outgoingMessage.Write(smartComboRoutingParamsCount);
			//    if (smartComboRoutingParamsCount > 0)
			//    {
			//        for (int i = 0; i < smartComboRoutingParamsCount; ++i)
			//        {
			//            KeyValuePair<string, string> tagValue = smartComboRoutingParams[i];
			//            outgoingMessage.Write(tagValue.Key);
			//            outgoingMessage.Write(tagValue.Value);
			//        }
			//    }
			//}

			if (_serverVersion >= 9)
			{
				// outgoingMessage.Write deprecated sharesAllocation field
				outgoingMessage.Write("");
			}

			if (_serverVersion >= 10)
			{
				outgoingMessage.Write(order.DiscretionaryAmt);
			}

			if (_serverVersion >= 11)
			{
				outgoingMessage.Write(order.GoodAfterTime);
			}

			if (_serverVersion >= 12)
			{
				outgoingMessage.Write(order.GoodTillDate);
			}

			if (_serverVersion >= 13)
			{
				outgoingMessage.Write(order.FaGroup);
				outgoingMessage.Write(order.FaMethod);
				outgoingMessage.Write(order.FaPercentage);
				outgoingMessage.Write(order.FaProfile);
			}
			if (_serverVersion >= 18)
			{ // institutional short sale slot fields.
				outgoingMessage.Write(order.ShortSaleSlot);      // 0 only for retail, 1 or 2 only for institution.
				outgoingMessage.Write(order.DesignatedLocation); // only populate when order.shortSaleSlot = 2.
			}
			if (_serverVersion >= SSHORTX_OLD)
			{
				outgoingMessage.Write(order.ExemptCode);
			}
			if (_serverVersion >= 19)
			{
				outgoingMessage.Write(order.OneCancelsAllType);
				if (_serverVersion < 38)
				{
					// will never happen
					outgoingMessage.Write( /* order.rthOnly */ false);
				}
				outgoingMessage.Write(order.Rule80A);
				outgoingMessage.Write(order.SettlingFirm);
				outgoingMessage.Write(order.AllOrNone);
				outgoingMessage.WriteMax(order.MinQty);
				outgoingMessage.WriteMax(order.PercentOffset);
				outgoingMessage.Write(order.ETradeOnly);
				outgoingMessage.Write(order.FirmQuoteOnly);
				outgoingMessage.WriteMax(order.NbboPriceCap);
				outgoingMessage.WriteMax(order.AuctionStrategy);
				outgoingMessage.WriteMax(order.StartingPrice);
				outgoingMessage.WriteMax(order.StockRefPrice);
				outgoingMessage.WriteMax(order.Delta);
				// Volatility orders had specific watermark price attribs in server version 26
				double lower = (_serverVersion == 26 && order.OrderType.Equals("VOL"))
					 ? Double.MaxValue
					 : order.StockRangeLower;
				double upper = (_serverVersion == 26 && order.OrderType.Equals("VOL"))
					 ? Double.MaxValue
					 : order.StockRangeUpper;
				outgoingMessage.WriteMax(lower);
				outgoingMessage.WriteMax(upper);
			}

			if (_serverVersion >= 22)
			{
				outgoingMessage.Write(order.OverridePercentageConstraints);
			}

			if (_serverVersion >= 26)
			{ // Volatility orders
				outgoingMessage.WriteMax(order.Volatility);
				outgoingMessage.WriteMax(order.VolatilityType);
				if (_serverVersion < 28)
				{
					bool isDeltaNeutralTypeMKT = (String.Compare("MKT", order.DeltaNeutralOrderType, true) == 0);
					outgoingMessage.Write(isDeltaNeutralTypeMKT);
				}
				else
				{
					outgoingMessage.Write(order.DeltaNeutralOrderType);
					outgoingMessage.WriteMax(order.DeltaNeutralAuxPrice);

					if (_serverVersion >= DELTA_NEUTRAL_CONID && order.DeltaNeutralOrderType != null && order.DeltaNeutralOrderType.Length > 0)
					{
						outgoingMessage.Write(order.DeltaNeutralConId);
						outgoingMessage.Write(order.DeltaNeutralSettlingFirm);
						outgoingMessage.Write(order.DeltaNeutralClearingAccount);
						outgoingMessage.Write(order.DeltaNeutralClearingIntent);
					}

					if (_serverVersion >= DELTA_NEUTRAL_OPEN_CLOSE && order.DeltaNeutralOrderType != null && order.DeltaNeutralOrderType.Length > 0)
					{
						outgoingMessage.Write(order.DeltaNeutralOpenClose);
						outgoingMessage.Write(order.DeltaNeutralShortSale);
						outgoingMessage.Write(order.DeltaNeutralShortSaleSlot);
						outgoingMessage.Write(order.DeltaNeutralDesignatedLocation);
					}
				}
				outgoingMessage.Write(order.ContinuousUpdate);
				if (_serverVersion == 26)
				{
					// Volatility orders had specific watermark price attribs in server version 26
					double lower = order.OrderType.Equals("VOL") ? order.StockRangeLower : Double.MaxValue;
					double upper = order.OrderType.Equals("VOL") ? order.StockRangeUpper : Double.MaxValue;
					outgoingMessage.WriteMax(lower);
					outgoingMessage.WriteMax(upper);
				}
				outgoingMessage.WriteMax(order.ReferencePriceType);
			}

			if (_serverVersion >= 30)
			{ // TRAIL_STOP_LIMIT stop price
				outgoingMessage.WriteMax(order.TrailStopPrice);
			}

			if (_serverVersion >= TRAILING_PERCENT)
			{
				outgoingMessage.WriteMax(order.TrailingPercent);
			}

			if (_serverVersion >= SCALE_ORDERS)
			{
				if (_serverVersion >= SCALE_ORDERS2)
				{
					outgoingMessage.WriteMax(order.ScaleInitLevelSize);
					outgoingMessage.WriteMax(order.ScaleSubsLevelSize);
				}
				else
				{
					outgoingMessage.Write("");
					outgoingMessage.WriteMax(order.ScaleInitLevelSize);

				}
				outgoingMessage.WriteMax(order.ScalePriceIncrement);
			}

			if (_serverVersion >= SCALE_ORDERS3 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != Double.MaxValue)
			{
				outgoingMessage.WriteMax(order.ScalePriceAdjustValue);
				outgoingMessage.WriteMax(order.ScalePriceAdjustInterval);
				outgoingMessage.WriteMax(order.ScaleProfitOffset);
				outgoingMessage.Write(order.ScaleAutoReset);
				outgoingMessage.WriteMax(order.ScaleInitPosition);
				outgoingMessage.WriteMax(order.ScaleInitFillQty);
				outgoingMessage.Write(order.ScaleRandomPercent);
			}

			if (_serverVersion >= SCALE_TABLE)
			{
				outgoingMessage.Write(order.ScaleTable);
				outgoingMessage.Write(order.ActiveStartTime);
				outgoingMessage.Write(order.ActiveStopTime);
			}

			if (_serverVersion >= HEDGE_ORDERS)
			{
				outgoingMessage.Write(order.HedgeType);
				if (order.HedgeType != null && order.HedgeType.Length > 0)
					outgoingMessage.Write(order.HedgeParam);
			}

			if (_serverVersion >= OPT_OUT_SMART_ROUTING)
				outgoingMessage.Write(order.OptOutSmartRouting);

			if (_serverVersion >= PTA_ORDERS)
			{
				outgoingMessage.Write(order.ClearingAccount);
				outgoingMessage.Write(order.ClearingIntent);
			}

			if (_serverVersion >= NOT_HELD)
				outgoingMessage.Write(order.NotHeld);

			if (_serverVersion >= UNDER_COMP)
			{
				//if (contract.UnderComp != null)
				//{
				//    UnderComp underComp = contract.UnderComp;
				//    outgoingMessage.Write(true);
				//    outgoingMessage.Write(underComp.ConId);
				//    outgoingMessage.Write(underComp.Delta);
				//    outgoingMessage.Write(underComp.Price);
				//}
				//else
				//{
				outgoingMessage.Write(false);
				//}
			}

			if (_serverVersion >= ALGO_ORDERS)
			{
				outgoingMessage.Write(order.AlgoStrategy);
				if (order.AlgoStrategy != null && order.AlgoStrategy.Length > 0)
				{
					throw new System.Exception();

					//List<KeyValuePair<string, string>> algoParams = order.AlgoParams;
					//int algoParamsCount = algoParams == null ? 0 : algoParams.Count;
					//outgoingMessage.Write(algoParamsCount);
					//if (algoParamsCount > 0)
					//{
					//    for (int i = 0; i < algoParamsCount; ++i)
					//    {
					//        KeyValuePair<string, string> tagValue = (KeyValuePair<string, string>)algoParams[i];
					//        outgoingMessage.Write(tagValue.Key);
					//        outgoingMessage.Write(tagValue.Value);
					//    }
					//}
				}
			}

			if (_serverVersion >= ALGO_ID)
				outgoingMessage.Write(order.AlgoId);

			if (_serverVersion >= WHAT_IF_ORDERS)
				outgoingMessage.Write(order.WhatIf);

			if (_serverVersion >= LINKING)
			{
				//int orderOptionsCount = order.OrderMiscOptions == null ? 0 : order.OrderMiscOptions.Count;
				//outgoingMessage.Write(orderOptionsCount);
				outgoingMessage.Write(TagValueListToString(order.OrderMiscOptions));
			}

			Send(_tempStream);

			return orderId;
		}

        // Requests all open orders submitted by any API client as well as those directly placed in the TWS. The existing orders will be received via the openOrder and orderStatus events.
        public void RequestAllOpenOrders()
        {
            int VERSION = 1;
            OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
            outgoingMessage.Write(OutgoingMessage.RequestAllOpenOrders);
            outgoingMessage.Write(VERSION);
            Send(_tempStream);
        }

        // Requests the server's current time.
        public void RequestCurrentTime()
        {
            int VERSION = 1;
            OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
            outgoingMessage.Write(OutgoingMessage.RequestCurrentTime);
            outgoingMessage.Write(VERSION);//version
            Send(_tempStream);
        }

        // Cancels all the active orders.  This method will cancel ALL open orders included those placed directly via the TWS.
        public void RequestGlobalCancel()
        {
            const int VERSION = 1;
            OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
            outgoingMessage.Write(OutgoingMessage.RequestGlobalCancel);
            outgoingMessage.Write(VERSION);
            Send(_tempStream);
        }

        // Requests all the day's executions matching the filter.  Only the current day's executions can be retrieved. Along with the executions, the CommissionReport will also be returned. The execution details will arrive at EWrapper:execDetails
        public void RequestExecutions(int requestId, int clientId, string acctCode, string time, string symbol, string secType, string exchange, string side)
        {
            int VERSION = 3;

            OutgoingMessage outgoingMessage = new OutgoingMessage(_tempStream);
            outgoingMessage.Write(OutgoingMessage.RequestExecutions);
            outgoingMessage.Write(VERSION);//version

            if (_serverVersion >= EXECUTION_DATA_CHAIN)
            {
                outgoingMessage.Write(requestId);
            }

            //Send the execution rpt filter data
            if (_serverVersion >= 9)
            {
                outgoingMessage.Write(clientId);
                outgoingMessage.Write(acctCode);

                // Note that the valid format for time is "yyyymmdd-hh:mm:ss"
                outgoingMessage.Write(time);
                outgoingMessage.Write(symbol);
                outgoingMessage.Write(secType);
                outgoingMessage.Write(exchange);
                outgoingMessage.Write(side);
            }
            Send(_tempStream);
        }



























		///**
		//* @brief Cancels a historical data request.
		//* @param reqId the request's identifier.
		//* @sa reqHistoricalData
		//*/
		//public void cancelHistoricalData(int reqId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(24, " It does not support historical data cancelations."))
		//        return;
		//    const int VERSION = 1;
		//    //No server version validation takes place here since minimum is already higher
		//    SendCancelRequest(OutgoingMessages.CancelOptionPrice, VERSION, reqId, EClientErrors.FAIL_SEND_CANHISTDATA);
		//}

		///**
		// * @brief Calculate the volatility for an option.
		// * Request the calculation of the implied volatility based on hypothetical option and its underlying prices. The calculation will be return in EWrapper's tickOptionComputation callback.
		// * @param reqId unique identifier of the request.
		// * @param contract the option's contract for which the volatility wants to be calculated.
		// * @param optionPrice hypothetical option price.
		// * @param underPrice hypothetical option's underlying price.
		// * @sa EWrapper::tickOptionComputation, cancelCalculateImpliedVolatility, Contract
		// */
		//public void calculateImpliedVolatility(int reqId, Contract contract, double optionPrice, double underPrice, List<TagValue> impliedVolatilityOptions)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.REQ_CALC_IMPLIED_VOLAT, " It does not support calculate implied volatility."))
		//        return;
		//    if (contract.TradingClass != null && contract.TradingClass.Length != 0 && !Check_serverVersion(MinServerVer.TRADING_CLASS, ""))
		//        return;
		//    const int version = 3;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.ReqCalcImpliedVolat);
		//    outgoingMessage.Write(version);
		//    outgoingMessage.Write(reqId);
		//    outgoingMessage.Write(contract.ConId);
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    outgoingMessage.Write(contract.Multiplier);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.PrimaryExch);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//        outgoingMessage.Write(contract.TradingClass);
		//    outgoingMessage.Write(optionPrice);
		//    outgoingMessage.Write(underPrice);

		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        int tagValuesCount = impliedVolatilityOptions == null ? 0 : impliedVolatilityOptions.Count;
		//        outgoingMessage.Write(tagValuesCount);
		//        outgoingMessage.Write(TagValueListToString(impliedVolatilityOptions));
		//    }

		//    Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQCALCIMPLIEDVOLAT);
		//}

		///**
		// * @brief Calculates an option's price.
		// * Calculates an option's price based on the provided volatility and its underlying's price. The calculation will be return in EWrapper's tickOptionComputation callback.
		// * @param reqId request's unique identifier.
		// * @param contract the option's contract for which the price wants to be calculated.
		// * @param volatility hypothetical volatility.
		// * @param underPrice hypothetical underlying's price.
		// * @sa EWrapper::tickOptionComputation, cancelCalculateOptionPrice, Contract
		// */
		//public void calculateOptionPrice(int reqId, Contract contract, double volatility, double underPrice, List<TagValue> optionPriceOptions)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.REQ_CALC_OPTION_PRICE,
		//        " It does not support calculation price requests."))
		//        return;
		//    if (contract.TradingClass != null && contract.TradingClass.Length != 0 &&
		//        !Check_serverVersion(MinServerVer.REQ_CALC_OPTION_PRICE, " It does not support tradingClass parameter in calculateOptionPrice."))
		//        return;

		//    const int version = 3;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.ReqCalcOptionPrice);
		//    outgoingMessage.Write(version);
		//    outgoingMessage.Write(reqId);
		//    outgoingMessage.Write(contract.ConId);
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    outgoingMessage.Write(contract.Multiplier);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.PrimaryExch);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//        outgoingMessage.Write(contract.TradingClass);
		//    outgoingMessage.Write(volatility);
		//    outgoingMessage.Write(underPrice);

		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        int tagValuesCount = optionPriceOptions == null ? 0 : optionPriceOptions.Count;
		//        outgoingMessage.Write(tagValuesCount);
		//        outgoingMessage.Write(TagValueListToString(optionPriceOptions));
		//    }

		//    Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQCALCOPTIONPRICE);
		//}

		///**
		// * @brief Cancels the account's summary request.
		// * After requesting an account's summary, invoke this function to cancel it.
		// * @param reqId the identifier of the previously performed account request
		// * @sa reqAccountSummary
		// */
		//public void cancelAccountSummary(int reqId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.ACCT_SUMMARY,
		//        " It does not support account summary cancellation."))
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelAccountSummary, 1, reqId, EClientErrors.FAIL_SEND_CANACCOUNTDATA);
		//}

		///**
		// * @brief Cancels an option's implied volatility calculation request
		// * @param reqId the identifier of the implied volatility's calculation request.
		// * @sa calculateImpliedVolatility
		// */
		//public void cancelCalculateImpliedVolatility(int reqId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.CANCEL_CALC_IMPLIED_VOLAT,
		//        " It does not support calculate implied volatility cancellation."))
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelImpliedVolatility, 1, reqId, EClientErrors.FAIL_SEND_CANCALCIMPLIEDVOLAT);
		//}

		///**
		// * @brief Cancels an option's price calculation request
		// * @param reqId the identifier of the option's price's calculation request.
		// * @sa calculateOptionPrice
		// */
		//public void cancelCalculateOptionPrice(int reqId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.CANCEL_CALC_OPTION_PRICE,
		//        " It does not support calculate option price cancellation."))
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelOptionPrice, 1, reqId, EClientErrors.FAIL_SEND_CANCALCOPTIONPRICE);
		//}

		///**
		// * @brief Cancels Fundamental data request
		// * @param reqId the request's idenfier.
		// * @sa reqFundamentalData
		// */
		//public void cancelFundamentalData(int reqId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.FUNDAMENTAL_DATA,
		//        " It does not support fundamental data requests."))
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelFundamentalData, 1, reqId, EClientErrors.FAIL_SEND_CANFUNDDATA);
		//}

		///**
		// * @brief Cancels a RT Market Data request
		// * @param tickerId request's identifier
		// * @sa reqMktData
		// */
		//public void cancelMktData(int tickerId)
		//{
		//    if (!CheckConnection())
		//        return;

		//    SendCancelRequest(OutgoingMessages.CancelMarketData, 1, tickerId, EClientErrors.FAIL_SEND_CANMKT);
		//}

		///**
		// * @brief Cancel's market depth's request.
		// * @param tickerId request's identifier.
		// * @sa reqMarketDepth
		// */
		//public void cancelMktDepth(int tickerId)
		//{
		//    if (!CheckConnection())
		//        return;

		//    SendCancelRequest(OutgoingMessages.CancelMarketDepth, 1, tickerId,
		//        EClientErrors.FAIL_SEND_CANMKTDEPTH);
		//}

		///**
		// * @brief Cancels IB's news bulletin subscription
		// * @sa reqNewsBulletins
		// */
		//public void cancelNewsBulletin()
		//{
		//    if (!CheckConnection())
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelNewsBulletin, 1,
		//        EClientErrors.FAIL_SEND_CORDER);
		//}

		///**
		// * @brief Cancels an active order
		// * @param orderId the order's client id
		// * @sa placeOrder, reqGlobalCancel
		// */
		//public void cancelOrder(int orderId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    SendCancelRequest(OutgoingMessages.CancelOrder, 1, orderId,
		//        EClientErrors.FAIL_SEND_CORDER);
		//}

		///**
		// * @brief Cancels all account's positions request
		// * @sa reqPositions
		// */
		//public void cancelPositions()
		//{
		//    if (!CheckConnection())
		//        return;

		//    if (!Check_serverVersion(MinServerVer.ACCT_SUMMARY,
		//        " It does not support position cancellation."))
		//        return;

		//    SendCancelRequest(OutgoingMessages.CancelPositions, 1, EClientErrors.FAIL_SEND_CANPOSITIONS);
		//}

		///**
		// * @brief Cancels Real Time Bars' subscription
		// * @param tickerId the request's identifier.
		// * @sa reqRealTimeBars
		// */
		//public void cancelRealTimeBars(int tickerId)
		//{
		//    if (!CheckConnection())
		//        return;

		//    SendCancelRequest(OutgoingMessages.CancelRealTimeBars, 1, tickerId, EClientErrors.FAIL_SEND_CANRTBARS);
		//}

		///**
		// * @brief Cancels Scanner Subscription
		// * @param tickerId the subscription's unique identifier.
		// * @sa reqScannerSubscription, ScannerSubscription, reqScannerParameters
		// */
		//public void cancelScannerSubscription(int tickerId)
		//{
		//    if (!CheckConnection())
		//        return;

		//    SendCancelRequest(OutgoingMessages.CancelScannerSubscription, 1, tickerId, EClientErrors.FAIL_SEND_CANSCANNER);
		//}

		///**
		// * @brief Exercises your options
		// * @param tickerId exercise request's identifier
		// * @param contract the option Contract to be exercised.
		// * @param exerciseAction set to 1 to exercise the option, set to 2 to let the option lapse.
		// * @param exerciseQuantity number of contracts to be exercised
		// * @param account destination account
		// * @param ovrd Specifies whether your setting will override the system's natural action. For example, if your action is "exercise" and the option is not in-the-money, by natural action the option would not exercise. If you have override set to "yes" the natural action would be overridden and the out-of-the money option would be exercised. Set to 1 to override, set to 0 not to.
		// */
		//public void exerciseOptions(int tickerId, Contract contract, int exerciseAction, int exerciseQuantity, string account, int ovrd)
		//{
		//    //WARN needs to be tested!
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(21, " It does not support options exercise from the API."))
		//        return;
		//    if ((!Util.StringIsEmpty(contract.TradingClass) || contract.ConId > 0) &&
		//        !Check_serverVersion(MinServerVer.TRADING_CLASS, " It does not support conId not tradingClass parameter when exercising options."))
		//        return;

		//    int VERSION = 2;

		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.ExerciseOptions);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(tickerId);

		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.ConId);
		//    }
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    outgoingMessage.Write(contract.Multiplier);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.TradingClass);
		//    }
		//    outgoingMessage.Write(exerciseAction);
		//    outgoingMessage.Write(exerciseQuantity);
		//    outgoingMessage.Write(account);
		//    outgoingMessage.Write(ovrd);

		//    Send(paramsList, EClientErrors.FAIL_GENERIC);
		//}

		////WARN: Have not tested this yet!
		///**
		// * @brief Replaces Financial Advisor's settings
		// * A Financial Advisor can define three different configurations: 
		// *    1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
		// *    2. Profiles: let you allocate shares on an account-by-account basis using a predefined calculation value.
		// *    3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
		// * More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
		// * @param faDataType the configuration to change. Set to 1, 2 or 3 as defined above.
		// * @param xml the xml-formatted configuration string
		// * @sa requestFA 
		// */
		//public void replaceFA(int faDataType, string xml)
		//{
		//    if (!CheckConnection())
		//        return;

		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.ReplaceFA);
		//    outgoingMessage.Write(1);
		//    outgoingMessage.Write(faDataType);
		//    outgoingMessage.Write(xml);
		//    Send(paramsList, EClientErrors.FAIL_SEND_FA_REPLACE);
		//}

		///**
		// * @brief Requests the FA configuration
		// * A Financial Advisor can define three different configurations: 
		// *      1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
		// *      2. Profiles: let you allocate shares on an account-by-account basis using a predefined calculation value.
		// *      3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
		// * More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
		// * @param faDataType the configuration to change. Set to 1, 2 or 3 as defined above.
		// * @sa replaceFA 
		// */
		//public void requestFA(int faDataType)
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestFA);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(faDataType);
		//    Send(paramsList, EClientErrors.FAIL_SEND_FA_REQUEST);
		//}

		///**
		// * @brief Subscribes to an specific account's information and portfolio
		// * Through this method, a single account's subscription can be started/stopped. As a result from the subscription, the account's information, portfolio and last update time will be received at EWrapper::updateAccountValue, EWrapper::updateAccountPortfolio, EWrapper::updateAccountTime respectively.
		// * Only one account can be subscribed at a time. A second subscription request for another account when the previous one is still active will cause the first one to be canceled in favour of the second one. Consider user reqPositions if you want to retrieve all your accounts' portfolios directly.
		// * @param subscribe set to true to start the subscription and to false to stop it.
		// * @param acctCode the account id (i.e. U123456) for which the information is requested.
		// * @sa reqPositions, EWrapper::updateAccountValue, EWrapper::updateAccountPortfolio, EWrapper::updateAccountTime
		// */
		//public void reqAccountUpdates(bool subscribe, string acctCode)
		//{
		//    int VERSION = 2;
		//    if (!CheckConnection())
		//        return;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestAccountData);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(subscribe);
		//    if (serverVersion >= 9)
		//        outgoingMessage.Write(acctCode);
		//    Send(paramsList, EClientErrors.FAIL_SEND_REQACCOUNTDATA);
		//}

		///**
		// * @brief Requests the contract's Reuters' global fundamental data.
		// * Reuters funalmental data will be returned at EWrapper::fundamentalData
		// * @param reqId the request's unique identifier.
		// * @param contract the contract's description for which the data will be returned.
		// * @param reportType there are three available report types: 
		// *      - ReportSnapshot: Company overview
		// *      - ReportsFinSummary: Financial summary
		//        - ReportRatios:	Financial ratios
		//        - ReportsFinStatements:	Financial statements
		//        - RESC: Analyst estimates
		//        - CalendarReport: Company calendar
		// * @sa EWrapper::fundamentalData
		// */
		//public void reqFundamentalData(int reqId, Contract contract, String reportType, List<TagValue> fundamentalDataOptions)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(reqId, MinServerVer.FUNDAMENTAL_DATA, " It does not support Fundamental Data requests."))
		//        return;
		//    if (!IsEmpty(contract.TradingClass) || contract.ConId > 0 || !IsEmpty(contract.Multiplier))
		//    {
		//        if (!Check_serverVersion(reqId, MinServerVer.TRADING_CLASS, ""))
		//            return;
		//    }

		//    const int VERSION = 3;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestFundamentalData);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(reqId);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        //WARN: why are we checking the trading class and multiplier above never send them?
		//        outgoingMessage.Write(contract.ConId);
		//    }
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.PrimaryExch);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    outgoingMessage.Write(reportType);

		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        int tagValuesCount = fundamentalDataOptions == null ? 0 : fundamentalDataOptions.Count;
		//        outgoingMessage.Write(tagValuesCount);
		//        outgoingMessage.Write(TagValueListToString(fundamentalDataOptions));
		//    }

		//    Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQFUNDDATA);
		//}

		///**
		// * @brief Requests contracts' historical data.
		// * When requesting historical data, a finishing time and date is required along with a duration string. For example, having: 
		// *      - endDateTime: 20130701 23:59:59 GMT
		// *      - durationStr: 3 D
		// * will return three days of data counting backwards from July 1st 2013 at 23:59:59 GMT resulting in all the available bars of the last three days until the date and time specified. It is possible to specify a timezone optionally. The resulting bars will be returned in EWrapper::historicalData
		// * @param tickerId the request's unique identifier.
		// * @param contract the contract for which we want to retrieve the data.
		// * @param endDateTime request's ending time with format yyyyMMdd HH:mm:ss {TMZ}
		// * @param durationString the amount of time for which the data needs to be retrieved:
		// *      - " S (seconds)
		// *      - " D (days)
		// *      - " W (weeks)
		// *      - " M (months)
		// *      - " Y (years)
		// * @param barSizeSetting the size of the bar:
		// *      - 1 sec
		// *      - 5 secs
		// *      - 15 secs
		// *      - 30 secs
		// *      - 1 min
		// *      - 2 mins
		// *      - 3 mins
		// *      - 5 mins
		// *      - 15 mins
		// *      - 30 mins
		// *      - 1 hour
		// *      - 1 day
		// * @param whatToShow the kind of information being retrieved:
		// *      - TRADES
		// *      - MIDPOINT
		// *      - BID
		// *      - ASK
		// *      - BID_ASK
		// *      - HISTORICAL_VOLATILITY
		// *      - OPTION_IMPLIED_VOLATILITY
		// * @param useRTH set to 0 to obtain the data which was also generated ourside of the Regular Trading Hours, set to 1 to obtain only the RTH data
		// * @param formatDate set to 1 to obtain the bars' time as yyyyMMdd HH:mm:ss, set to 2 to obtain it like system time format in seconds
		// * @sa EWrapper::historicalData
		// */
		//public void reqHistoricalData(int tickerId, Contract contract, string endDateTime,
		//    string durationString, string barSizeSetting, string whatToShow, int useRTH, int formatDate, List<TagValue> chartOptions)
		//{
		//    if (!CheckConnection())
		//        return;

		//    if (!Check_serverVersion(tickerId, 16))
		//        return;

		//    if (!IsEmpty(contract.TradingClass) || contract.ConId > 0)
		//    {
		//        if (!Check_serverVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support conId nor trading class parameters when requesting historical data."))
		//            return;
		//    }

		//    const int VERSION = 6;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestHistoricalData);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(tickerId);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//        outgoingMessage.Write(contract.ConId);
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    outgoingMessage.Write(contract.Multiplier);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.PrimaryExch);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.TradingClass);
		//    }

		//    outgoingMessage.Write(contract.IncludeExpired ? 1 : 0);


		//    outgoingMessage.Write(endDateTime);
		//    outgoingMessage.Write(barSizeSetting);

		//    outgoingMessage.Write(durationString);
		//    outgoingMessage.Write(useRTH);
		//    outgoingMessage.Write(whatToShow);

		//    outgoingMessage.Write(formatDate);

		//    if (StringsAreEqual(Constants.BagSecType, contract.SecType))
		//    {
		//        //if (contract.ComboLegs == null)
		//        //{
		//        outgoingMessage.Write(0);
		//        //}
		//        //else
		//        //{
		//        //    outgoingMessage.Write(contract.ComboLegs.Count);

		//        //    ComboLeg comboLeg;
		//        //    for (int i = 0; i < contract.ComboLegs.Count; i++)
		//        //    {
		//        //        comboLeg = (ComboLeg)contract.ComboLegs[i];
		//        //        outgoingMessage.Write(comboLeg.ConId);
		//        //        outgoingMessage.Write(comboLeg.Ratio);
		//        //        outgoingMessage.Write(comboLeg.Action);
		//        //        outgoingMessage.Write(comboLeg.Exchange);
		//        //    }
		//        //}
		//    }

		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        outgoingMessage.Write(TagValueListToString(chartOptions));
		//    }

		//    Send(paramsList, EClientErrors.FAIL_SEND_REQHISTDATA);
		//}

		///**
		// * @brief Requests the next valid order id.
		// * @param numIds deprecate
		// * @sa EWrapper::nextValidId
		// */
		//public void reqIds(int numIds)
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 1;

		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestIds);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(numIds);
		//    Send(paramsList, EClientErrors.FAIL_GENERIC);
		//}

		///**
		// * @brief Requests the accounts to which the logged user has access to.
		// * @sa EWrapper::managedAccounts
		// */
		//public void reqManagedAccts()
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestManagedAccounts);
		//    outgoingMessage.Write(VERSION);
		//    Send(paramsList, EClientErrors.FAIL_GENERIC);
		//}

		///**
		// * @brief indicates the TWS to switch to "frozen" market data.
		// * The API can receive frozen market data from Trader Workstation. Frozen market data is the last data recorded in our system. During normal trading hours, the API receives real-time market data. If you use this function, you are telling TWS to automatically switch to frozen market data after the close. Then, before the opening of the next trading day, market data will automatically switch back to real-time market data.
		// * @param marketDataType set to 1 for real time streaming, set to 2 for frozen market data.
		// */
		//public void reqMarketDataType(int marketDataType)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.REQ_MARKET_DATA_TYPE, " It does not support market data type requests."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestMarketDataType);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(marketDataType);
		//    Send(paramsList, EClientErrors.FAIL_SEND_REQMARKETDATATYPE);
		//}

		///**
		// * @brief Requests the contract's market depth (order book).
		// * @param tickerId the request's identifier
		// * @param contract the Contract for which the depth is being requested
		// * @param numRows the number of rows on each side of the order book
		// * @sa cancelMktDepth, EWrapper::updateMktDepth, EWrapper::updateMktDepthL2
		// */
		//public void reqMarketDepth(int tickerId, Contract contract, int numRows, List<TagValue> mktDepthOptions)
		//{
		//    if (!CheckConnection())
		//        return;

		//    if (!IsEmpty(contract.TradingClass) || contract.ConId > 0)
		//    {
		//        if (!Check_serverVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqMktDepth."))
		//            return;
		//    }

		//    const int VERSION = 5;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestMarketDepth);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(tickerId);

		//    // outgoingMessage.Write contract fields
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.ConId);
		//    }
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    if (serverVersion >= 15)
		//    {
		//        outgoingMessage.Write(contract.Multiplier);
		//    }
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.TradingClass);
		//    }
		//    if (serverVersion >= 19)
		//    {
		//        outgoingMessage.Write(numRows);
		//    }
		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        //int tagValuesCount = mktDepthOptions == null ? 0 : mktDepthOptions.Count;
		//        //outgoingMessage.Write(tagValuesCount);
		//        outgoingMessage.Write(TagValueListToString(mktDepthOptions));
		//    }
		//    Send(paramsList, EClientErrors.FAIL_SEND_REQMKTDEPTH);
		//}

		///**
		// * @brief Subscribes to IB's News Bulletins
		// * @param allMessages if set to true, will return all the existing bulletins for the current day, set to false to receive only the new bulletins.
		// * @sa cancelNewsBulletins, EWrapper::updateNewsBulletins
		// */
		//public void reqNewsBulletins(bool allMessages)
		//{
		//    if (!CheckConnection())
		//        return;

		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestNewsBulletins);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(allMessages);
		//    Send(paramsList, EClientErrors.FAIL_GENERIC);
		//}

		///**
		// * @brief Requests real time bars
		// * Currently, only 5 seconds bars are provided. This request ius suject to the same pacing as any historical data request: no more than 60 API queries in more than 600 seconds
		// * @param tickerId the request's unique identifier.
		// * @param contract the Contract for which the depth is being requested
		// * @param barSize currently being ignored
		// * @param whatToShow the nature of the data being retrieved:
		// *      - TRADES
		// *      - MIDPOINT
		// *      - BID
		// *      - ASK
		// * @param useRTH set to 0 to obtain the data which was also generated ourside of the Regular Trading Hours, set to 1 to obtain only the RTH data
		// * @sa cancelRealTimeBars, EWrapper::realTimeBar
		// */
		//public void reqRealTimeBars(int tickerId, Contract contract, int barSize, string whatToShow, bool useRTH, List<TagValue> realTimeBarsOptions)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(tickerId, MinServerVer.REAL_TIME_BARS, " It does not support real time bars."))
		//        return;

		//    if (!IsEmpty(contract.TradingClass) || contract.ConId > 0)
		//    {
		//        if (!Check_serverVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqRealTimeBars."))
		//            return;
		//    }

		//    const int VERSION = 3;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestRealTimeBars);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(tickerId);

		//    // outgoingMessage.Write contract fields
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.ConId);
		//    }
		//    outgoingMessage.Write(contract.Symbol);
		//    outgoingMessage.Write(contract.SecType);
		//    outgoingMessage.Write(contract.Expiry);
		//    outgoingMessage.Write(contract.Strike);
		//    outgoingMessage.Write(contract.Right);
		//    outgoingMessage.Write(contract.Multiplier);
		//    outgoingMessage.Write(contract.Exchange);
		//    outgoingMessage.Write(contract.PrimaryExch);
		//    outgoingMessage.Write(contract.Currency);
		//    outgoingMessage.Write(contract.LocalSymbol);
		//    if (serverVersion >= MinServerVer.TRADING_CLASS)
		//    {
		//        outgoingMessage.Write(contract.TradingClass);
		//    }
		//    outgoingMessage.Write(barSize);  // this parameter is not currently used
		//    outgoingMessage.Write(whatToShow);
		//    outgoingMessage.Write(useRTH);
		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        outgoingMessage.Write(TagValueListToString(realTimeBarsOptions));
		//    }
		//    Send(paramsList, EClientErrors.FAIL_SEND_REQRTBARS);
		//}

		///**
		// * @brief Requests all possible parameters which can be used for a scanner subscription
		// * @sa reqScannerSubscription
		// */
		//public void reqScannerParameters()
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestScannerParameters);
		//    outgoingMessage.Write(VERSION);
		//    Send(paramsList, EClientErrors.FAIL_SEND_REQSCANNERPARAMETERS);
		//}

		///**
		// * @brief Starts a subscription to market scan results based on the provided parameters.
		// * @param reqId the request's identifier
		// * @param subscription summary of the scanner subscription including its filters.
		// * @sa reqScannerParameters, ScannerSubscription, EWrapper::scannerData
		// */
		//public void reqScannerSubscription(int reqId, ScannerSubscription subscription, List<TagValue> scannerSubscriptionOptions)
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 4;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.RequestScannerSubscription);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(reqId);
		//    outgoingMessage.WriteMax(subscription.NumberOfRows);
		//    outgoingMessage.Write(subscription.Instrument);
		//    outgoingMessage.Write(subscription.LocationCode);
		//    outgoingMessage.Write(subscription.ScanCode);
		//    outgoingMessage.WriteMax(subscription.AbovePrice);
		//    outgoingMessage.WriteMax(subscription.BelowPrice);
		//    outgoingMessage.WriteMax(subscription.AboveVolume);
		//    outgoingMessage.WriteMax(subscription.MarketCapAbove);
		//    outgoingMessage.WriteMax(subscription.MarketCapBelow);
		//    outgoingMessage.Write(subscription.MoodyRatingAbove);
		//    outgoingMessage.Write(subscription.MoodyRatingBelow);
		//    outgoingMessage.Write(subscription.SpRatingAbove);
		//    outgoingMessage.Write(subscription.SpRatingBelow);
		//    outgoingMessage.Write(subscription.MaturityDateAbove);
		//    outgoingMessage.Write(subscription.MaturityDateBelow);
		//    outgoingMessage.WriteMax(subscription.CouponRateAbove);
		//    outgoingMessage.WriteMax(subscription.CouponRateBelow);
		//    outgoingMessage.Write(subscription.ExcludeConvertible);
		//    if (serverVersion >= 25)
		//    {
		//        outgoingMessage.WriteMax(subscription.AverageOptionVolumeAbove);
		//        outgoingMessage.Write(subscription.ScannerSettingPairs);
		//    }
		//    if (serverVersion >= 27)
		//    {
		//        outgoingMessage.Write(subscription.StockTypeFilter);
		//    }

		//    if (serverVersion >= MinServerVer.LINKING)
		//    {
		//        //int tagValuesCount = scannerSubscriptionOptions == null ? 0 : scannerSubscriptionOptions.Count;
		//        //outgoingMessage.Write(tagValuesCount);
		//        outgoingMessage.Write(TagValueListToString(scannerSubscriptionOptions));
		//    }

		//    Send(paramsList, EClientErrors.FAIL_SEND_REQSCANNER);
		//}

		///**
		// * @brief Changes the TWS/GW log level.
		// * Valid values are:\n
		// * 1 = SYSTEM\n
		// * 2 = ERROR\n
		// * 3 = WARNING\n
		// * 4 = INFORMATION\n
		// * 5 = DETAIL\n
		// */
		//public void setServerLogLevel(int logLevel)
		//{
		//    if (!CheckConnection())
		//        return;
		//    const int VERSION = 1;

		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.ChangeServerLog);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(logLevel);

		//    Send(paramsList, EClientErrors.FAIL_SEND_SERVER_LOG_LEVEL);
		//}

		//public void verifyRequest(string apiName, string apiVersion)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support verification request."))
		//        return;
		//    if (!extraAuth)
		//    {
		//        ReportError(IncomingMessage.NotValid, EClientErrors.FAIL_SEND_VERIFYMESSAGE, " Intent to authenticate needs to be expressed during initial connect request.");
		//        return;
		//    }

		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.VerifyRequest);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(apiName);
		//    outgoingMessage.Write(apiVersion);
		//    Send(paramsList, EClientErrors.FAIL_SEND_VERIFYREQUEST);
		//}

		//public void verifyMessage(string apiData)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support verification message sending."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.VerifyMessage);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(apiData);
		//    Send(paramsList, EClientErrors.FAIL_SEND_VERIFYMESSAGE);
		//}

		//public void queryDisplayGroups(int requestId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support queryDisplayGroups request."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.QueryDisplayGroups);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(requestId);
		//    Send(paramsList, EClientErrors.FAIL_SEND_QUERYDISPLAYGROUPS);
		//}

		//public void subscribeToGroupEvents(int requestId, int groupId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support subscribeToGroupEvents request."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.SubscribeToGroupEvents);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(requestId);
		//    outgoingMessage.Write(groupId);
		//    Send(paramsList, EClientErrors.FAIL_SEND_SUBSCRIBETOGROUPEVENTS);
		//}

		//public void updateDisplayGroup(int requestId, string contractInfo)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support updateDisplayGroup request."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.UpdateDisplayGroup);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(requestId);
		//    outgoingMessage.Write(contractInfo);
		//    Send(paramsList, EClientErrors.FAIL_SEND_UPDATEDISPLAYGROUP);
		//}

		//public void unsubscribeFromGroupEvents(int requestId)
		//{
		//    if (!CheckConnection())
		//        return;
		//    if (!Check_serverVersion(MinServerVer.LINKING, " It does not support unsubscribeFromGroupEvents request."))
		//        return;
		//    const int VERSION = 1;
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(OutgoingMessages.UnsubscribeFromGroupEvents);
		//    outgoingMessage.Write(VERSION);
		//    outgoingMessage.Write(requestId);
		//    Send(paramsList, EClientErrors.FAIL_SEND_UNSUBSCRIBEFROMGROUPEVENTS);
		//}

		//protected bool Check_serverVersion(int requiredVersion)
		//{
		//    return Check_serverVersion(requiredVersion, "");
		//}

		//protected bool Check_serverVersion(int requestId, int requiredVersion)
		//{
		//    return Check_serverVersion(requestId, requiredVersion, "");
		//}

		//protected bool Check_serverVersion(int requiredVersion, string updatetail)
		//{
		//    return Check_serverVersion(IncomingMessage.NotValid, requiredVersion, updatetail);
		//}

		//protected bool Check_serverVersion(int tickerId, int requiredVersion, string updatetail)
		//{
		//    if (serverVersion < requiredVersion)
		//    {
		//        ReportUpdateTWS(tickerId, updatetail);
		//        return false;
		//    }
		//    return true;
		//}

		//protected void SendCancelRequest(OutgoingMessages msgType, int version, int reqId, CodeMsgPair errorMessage)
		//{
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(msgType);
		//    outgoingMessage.Write(version);
		//    outgoingMessage.Write(reqId);
		//    try
		//    {
		//        lock (this)
		//        {
		//            Send(paramsList);
		//        }
		//    }
		//    catch (Exception)
		//    {
		//        wrapper.error(reqId, errorMessage.Code, errorMessage.Message);
		//        Close();
		//    }
		//}

		//protected void SendCancelRequest(OutgoingMessages msgType, int version, CodeMsgPair errorMessage)
		//{
		//    IBParamsList paramsList = new IBParamsList();
		//    outgoingMessage.Write(msgType);
		//    outgoingMessage.Write(version);
		//    try
		//    {
		//        lock (this)
		//        {
		//            Send(paramsList);
		//        }
		//    }
		//    catch (Exception)
		//    {
		//        wrapper.error(IncomingMessage.NotValid, errorMessage.Code, errorMessage.Message);
		//        Close();
		//    }
		//}

		private string TagValueListToString(List<KeyValuePair<string, string>> tagValues)
		{
			StringBuilder tagValuesStr = new StringBuilder();
			int tagValuesCount = tagValues == null ? 0 : tagValues.Count;

			for (int i = 0; i < tagValuesCount; i++)
			{
				KeyValuePair<string, string> tagValue = tagValues[i];
				tagValuesStr.Append(tagValue.Key).Append("=").Append(tagValue.Value).Append(";");
			}
			return tagValuesStr.ToString();
		}

		private void Send(System.IO.MemoryStream stream)
		{
			if (!IsConnected)
				throw new System.Exception();

			//TODO do this more efficiently using a buffer.
			_writer.Write(stream.ToArray());
		}

	}
}
