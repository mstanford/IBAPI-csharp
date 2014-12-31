using System;
using System.Collections.Generic;
using System.Text;

namespace IBAPI
{
	public class IncomingMessage
	{
		public const int NotValid = -1;
		public const int TickPrice = 1;
		public const int TickSize = 2;
		public const int OrderStatus = 3;
		public const int Error = 4;
		public const int OpenOrder = 5;
		public const int AccountValue = 6;
		public const int PortfolioValue = 7;
		public const int AccountUpdateTime = 8;
		public const int NextValidId = 9;
		public const int ContractData = 10;
		public const int ExecutionData = 11;
		public const int MarketDepth = 12;
		public const int MarketDepthL2 = 13;
		public const int NewsBulletins = 14;
		public const int ManagedAccounts = 15;
		public const int ReceiveFA = 16;
		public const int HistoricalData = 17;
		public const int BondContractData = 18;
		public const int ScannerParameters = 19;
		public const int ScannerData = 20;
		public const int TickOptionComputation = 21;
		public const int TickGeneric = 45;
		public const int TickString = 46;
		public const int TickEFP = 47;
		public const int CurrentTime = 49;
		public const int RealTimeBars = 50;
		public const int FundamentalData = 51;
		public const int ContractDataEnd = 52;
		public const int OpenOrderEnd = 53;
		public const int AccountDownloadEnd = 54;
		public const int ExecutionDataEnd = 55;
		public const int DeltaNeutralValidation = 56;
		public const int TickSnapshotEnd = 57;
		public const int MarketDataType = 58;
		public const int CommissionsReport = 59;
		public const int Position = 61;
		public const int PositionEnd = 62;
		public const int AccountSummary = 63;
		public const int AccountSummaryEnd = 64;
		public const int VerifyMessageApi = 65;
		public const int VerifyCompleted = 66;
		public const int DisplayGroupList = 67;
		public const int DisplayGroupUpdated = 68;



		public static Dictionary<string, object> Read(System.IO.BinaryReader reader, int serverVersion)
		{
			Dictionary<string, object> message = new Dictionary<string, object>();

			int messageType = ReadInt(reader);
			message["MessageTypeId"] = messageType;

			switch (messageType)
			{
				case IncomingMessage.NotValid:
					message["MessageType"] = "NotValid";
					break;
				case IncomingMessage.TickPrice:
					{
						message["MessageType"] = "TickPrice";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						int tickType;
						message["TickType"] = tickType = ReadInt(reader);
						message["Price"] = ReadDouble(reader);
						if (msgVersion >= 2)
							message["Size"] = ReadInt(reader);
						if (msgVersion >= 3)
							message["CanAutoExecute"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.TickSize:
					{
						message["MessageType"] = "TickSize";
						message["Version"] = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						message["TickType"] = ReadInt(reader);
						message["Size"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.TickString:
					{
						message["MessageType"] = "TickString";
						message["Version"] = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						message["TickType"] = ReadInt(reader);
						message["Value"] = ReadString(reader);
						break;
					}
				case IncomingMessage.TickGeneric:
					{
						message["MessageType"] = "TickGeneric";
						message["Version"] = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						message["TickType"] = ReadInt(reader);
						message["Value"] = ReadDouble(reader);
						break;
					}
				case IncomingMessage.TickEFP:
					{
						message["MessageType"] = "TickEFP";
						message["Version"] = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						message["TickType"] = ReadInt(reader);
						message["BasisPoints"] = ReadDouble(reader);
						message["FormattedBasisPoints"] = ReadString(reader);
						message["ImpliedFuturesPrice"] = ReadDouble(reader);
						message["HoldDays"] = ReadInt(reader);
						message["FutureExpiry"] = ReadString(reader);
						message["DividendImpact"] = ReadDouble(reader);
						message["DividendsToExpiry"] = ReadDouble(reader);
						break;
					}
				case IncomingMessage.TickSnapshotEnd:
					{
						message["MessageType"] = "TickSnapshotEnd";
						message["Version"] = ReadInt(reader);
                        message["TickerId"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.Error:
					{
						message["MessageType"] = "Error";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						if (msgVersion < 2)
						{
							message["Error"] = ReadString(reader);
						}
						else
						{
                            message["RequestId"] = ReadInt(reader);
							message["ErrorCode"] = ReadInt(reader);
							message["ErrorMessage"] = ReadString(reader);
						}
						break;
					}
				case IncomingMessage.CurrentTime:
					{
						message["MessageType"] = "CurrentTime";
						message["Version"] = ReadInt(reader);
						message["Time"] = ReadLong(reader);
						break;
					}
				case IncomingMessage.ManagedAccounts:
					{
						message["MessageType"] = "ManagedAccounts";
						message["Version"] = ReadInt(reader);
						message["AccountsList"] = ReadString(reader);
						break;
					}
				case IncomingMessage.NextValidId:
					{
						message["MessageType"] = "NextValidId";
						message["Version"] = ReadInt(reader);
						message["OrderId"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.DeltaNeutralValidation:
					{
						message["MessageType"] = "DeltaNeutralValidation";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["UnderlyingComponentContractId"] = ReadInt(reader);
						message["UnderlyingComponentDelta"] = ReadDouble(reader);
						message["UnderlyingComponentPrice"] = ReadDouble(reader);
						break;
					}
				case IncomingMessage.TickOptionComputation:
					{
						message["MessageType"] = "TickOptionComputation";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						int tickType = ReadInt(reader);
						double impliedVolatility = ReadDouble(reader);
						if (impliedVolatility < 0)
							impliedVolatility = Double.MaxValue;
						double delta = ReadDouble(reader);
						if (Math.Abs(delta) > 1)
							delta = Double.MaxValue;
						double optPrice = Double.MaxValue;
						double pvDividend = Double.MaxValue;
						double gamma = Double.MaxValue;
						double vega = Double.MaxValue;
						double theta = Double.MaxValue;
						double undPrice = Double.MaxValue;
						if (msgVersion >= 6 || tickType == TickType.MODEL_OPTION)
						{
							optPrice = ReadDouble(reader);
							if (optPrice < 0)
							{ // -1 is the "not yet computed" indicator
								optPrice = Double.MaxValue;
							}
							pvDividend = ReadDouble(reader);
							if (pvDividend < 0)
							{ // -1 is the "not yet computed" indicator
								pvDividend = Double.MaxValue;
							}
						}
						if (msgVersion >= 6)
						{
							gamma = ReadDouble(reader);
							if (Math.Abs(gamma) > 1)
							{ // -2 is the "not yet computed" indicator
								gamma = Double.MaxValue;
							}
							vega = ReadDouble(reader);
							if (Math.Abs(vega) > 1)
							{ // -2 is the "not yet computed" indicator
								vega = Double.MaxValue;
							}
							theta = ReadDouble(reader);
							if (Math.Abs(theta) > 1)
							{ // -2 is the "not yet computed" indicator
								theta = Double.MaxValue;
							}
							undPrice = ReadDouble(reader);
							if (undPrice < 0)
							{ // -1 is the "not yet computed" indicator
								undPrice = Double.MaxValue;
							}
						}

						//parent.Wrapper.tickOptionComputation(requestId, tickType, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice);
						throw new System.Exception();
						break;
					}
				case IncomingMessage.AccountSummary:
					{
						message["MessageType"] = "AccountSummary";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["Account"] = ReadString(reader);
						message["Tag"] = ReadString(reader);
						message["Value"] = ReadString(reader);
						message["Currency"] = ReadString(reader);
						break;
					}
				case IncomingMessage.AccountSummaryEnd:
					{
						message["MessageType"] = "AccountSummaryEnd";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.AccountValue:
					{
						message["MessageType"] = "AccountValue";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["Key"] = ReadString(reader);
						message["Value"] = ReadString(reader);
						message["Currency"] = ReadString(reader);
						if (msgVersion >= 2)
							message["AccountName"] = ReadString(reader);
						break;
					}
				case IncomingMessage.PortfolioValue:
					{
						message["MessageType"] = "PortfolioValue";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						if (msgVersion >= 6)
							message.Add("ContractId", ReadInt(reader));
						message["Symbol"] = ReadString(reader);
						message["SecurityType"] = ReadString(reader);
						message["Expiry"] = ReadString(reader);
						message["Strike"] = ReadDouble(reader);
						message["Right"] = ReadString(reader);
						if (msgVersion >= 7)
						{
							message["Multiplier"] = ReadString(reader);
							message["PrimaryExch"] = ReadString(reader);
						}
						message["Currency"] = ReadString(reader);
						if (msgVersion >= 2)
							message["LocalSymbol"] = ReadString(reader);
						if (msgVersion >= 8)
							message["TradingClass"] = ReadString(reader);
						message.Add("Position", ReadInt(reader));
						message.Add("MarketPrice", ReadDouble(reader));
						message.Add("MarketValue", ReadDouble(reader));
						if (msgVersion >= 3)
						{
							message.Add("AverageCost", ReadDouble(reader));
							message.Add("UnrealizedPNL", ReadDouble(reader));
							message.Add("RealizedPNL", ReadDouble(reader));
						}
						if (msgVersion >= 4)
							message.Add("AccountName", ReadString(reader));
						if (msgVersion == 6 && serverVersion == 39)
							message.Add("PrimaryExch", ReadString(reader));
						break;
					}
				case IncomingMessage.AccountUpdateTime:
					{
						message["MessageType"] = "AccountUpdateTime";
						message["Version"] = ReadInt(reader);
						message["Timestamp"] = ReadString(reader);
						break;
					}
				case IncomingMessage.AccountDownloadEnd:
					{
						message["MessageType"] = "AccountDownloadEnd";
						message["Version"] = ReadInt(reader);
						message["Account"] = ReadString(reader);
						break;
					}
				case IncomingMessage.OrderStatus:
					{
						message["MessageType"] = "OrderStatus";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["OrderId"] = ReadInt(reader);
						message["Status"] = ReadString(reader);
						message["Filled"] = ReadInt(reader);
						message["Remaining"] = ReadInt(reader);
						message["AvgFillPrice"] = ReadDouble(reader);
						if (msgVersion >= 2)
							message["PermId"] = ReadInt(reader);
						if (msgVersion >= 3)
							message["ParentId"] = ReadInt(reader);
						if (msgVersion >= 4)
							message["LastFillPrice"] = ReadDouble(reader);
						if (msgVersion >= 5)
							message["ClientId"] = ReadInt(reader);
						if (msgVersion >= 6)
							message["WhyHeld"] = ReadString(reader);
						break;
					}
				case IncomingMessage.OpenOrder:
					{
						message["MessageType"] = "OpenOrder";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						// read order id
						//Order order = new Order();
						message.Add("OrderId", ReadInt(reader));

						// read contract fields
						//Contract contract = new Contract();
						if (msgVersion >= 17)
							message.Add("ContractId", ReadInt(reader));
						message["Symbol"] = ReadString(reader);
						message["SecurityType"] = ReadString(reader);
						message["Expiry"] = ReadString(reader);
						message["Strike"] = ReadDouble(reader);
						message["Right"] = ReadString(reader);
						if (msgVersion >= 32)
							message["Multiplier"] = ReadString(reader);
						message["Exchange"] = ReadString(reader);
						message["Currency"] = ReadString(reader);
						if (msgVersion >= 2)
							message["LocalSymbol"] = ReadString(reader);
						if (msgVersion >= 32)
							message["TradingClass"] = ReadString(reader);


						// read order fields
						message.Add("Action", ReadString(reader));
						message.Add("TotalQuantity", ReadInt(reader));
						message.Add("OrderType", ReadString(reader));
						if (msgVersion < 29)
						{
							message.Add("LmtPrice", ReadDouble(reader));
						}
						else
						{
							message.Add("LmtPrice", ReadDoubleMax(reader));
						}

						if (msgVersion < 30)
						{
							message.Add("AuxPrice", ReadDouble(reader));
						}
						else
						{
							message.Add("AuxPrice", ReadDoubleMax(reader));
						}
						message.Add("Tif", ReadString(reader));
						message.Add("OcaGroup", ReadString(reader));
						message.Add("Account", ReadString(reader));
						message.Add("OpenClose", ReadString(reader));
						message.Add("Origin", ReadInt(reader));
						message.Add("OrderRef", ReadString(reader));

						if (msgVersion >= 3)
						{
							message.Add("ClientId", ReadInt(reader));
						}

						if (msgVersion >= 4)
						{
							message.Add("PermId", ReadInt(reader));
							if (msgVersion < 18)
							{
								// will never happen
								/* order.ignoreRth = */
								ReadBoolFromInt(reader);
							}
							else
							{
								message.Add("OutsideRth", ReadBoolFromInt(reader));
							}
							message.Add("Hidden", ReadInt(reader) == 1);
							message.Add("DiscretionaryAmt", ReadDouble(reader));
						}

						if (msgVersion >= 5)
						{
							message.Add("GoodAfterTime", ReadString(reader));
						}

						if (msgVersion >= 6)
						{
							// skip deprecated sharesAllocation field
							ReadString(reader);
						}

						if (msgVersion >= 7)
						{
							message.Add("FaGroup", ReadString(reader));
							message.Add("FaMethod", ReadString(reader));
							message.Add("FaPercentage", ReadString(reader));
							message.Add("FaProfile", ReadString(reader));
						}

						if (msgVersion >= 8)
						{
							message.Add("GoodTillDate", ReadString(reader));
						}

						if (msgVersion >= 9)
						{
							message.Add("Rule80A", ReadString(reader));
							message.Add("PercentOffset", ReadDoubleMax(reader));
							message.Add("SettlingFirm", ReadString(reader));
							message.Add("ShortSaleSlot", ReadInt(reader));
							message.Add("DesignatedLocation", ReadString(reader));
							if (serverVersion == 51)
							{
								ReadInt(reader); // exemptCode
							}
							else if (msgVersion >= 23)
							{
								message.Add("ExemptCode", ReadInt(reader));
							}
							message.Add("AuctionStrategy", ReadInt(reader));
							message.Add("StartingPrice", ReadDoubleMax(reader));
							message.Add("StockRefPrice", ReadDoubleMax(reader));
							message.Add("Delta", ReadDoubleMax(reader));
							message.Add("StockRangeLower", ReadDoubleMax(reader));
							message.Add("StockRangeUpper", ReadDoubleMax(reader));
							message.Add("DisplaySize", ReadInt(reader));
							if (msgVersion < 18)
							{
								// will never happen
								/* order.rthOnly = */
								ReadBoolFromInt(reader);
							}
							message.Add("BlockOrder", ReadBoolFromInt(reader));
							message.Add("SweepToFill", ReadBoolFromInt(reader));
							message.Add("AllOrNone", ReadBoolFromInt(reader));
							message.Add("MinQty", ReadIntMax(reader));
							message.Add("OcaType", ReadInt(reader));
							message.Add("ETradeOnly", ReadBoolFromInt(reader));
							message.Add("FirmQuoteOnly", ReadBoolFromInt(reader));
							message.Add("NbboPriceCap", ReadDoubleMax(reader));
						}

						if (msgVersion >= 10)
						{
							message.Add("ParentId", ReadInt(reader));
							message.Add("TriggerMethod", ReadInt(reader));
						}

						if (msgVersion >= 11)
						{
							message.Add("Volatility", ReadDoubleMax(reader));
							message.Add("VolatilityType", ReadInt(reader));
							if (msgVersion == 11)
							{
								int receivedInt = ReadInt(reader);
								message.Add("DeltaNeutralOrderType", (receivedInt == 0) ? "NONE" : "MKT");
							}
							else
							{ // msgVersion 12 and up
								string deltaNeutralOrderType = ReadString(reader);
								message.Add("DeltaNeutralOrderType", deltaNeutralOrderType);
								message.Add("DeltaNeutralAuxPrice", ReadDoubleMax(reader));

								if (msgVersion >= 27 && deltaNeutralOrderType != null && deltaNeutralOrderType.Length > 0)
								{
									message.Add("DeltaNeutralConId", ReadInt(reader));
									message.Add("DeltaNeutralSettlingFirm", ReadString(reader));
									message.Add("DeltaNeutralClearingAccount", ReadString(reader));
									message.Add("DeltaNeutralClearingIntent", ReadString(reader));
								}

								if (msgVersion >= 31 && deltaNeutralOrderType != null && deltaNeutralOrderType.Length > 0)
								{
									message.Add("DeltaNeutralOpenClose", ReadString(reader));
									message.Add("DeltaNeutralShortSale", ReadBoolFromInt(reader));
									message.Add("DeltaNeutralShortSaleSlot", ReadInt(reader));
									message.Add("DeltaNeutralDesignatedLocation", ReadString(reader));
								}
							}
							message.Add("ContinuousUpdate", ReadInt(reader));
							if (serverVersion == 26)
							{
								message.Add("StockRangeLower", ReadDouble(reader));
								message.Add("StockRangeUpper", ReadDouble(reader));
							}
							message.Add("ReferencePriceType", ReadInt(reader));
						}

						if (msgVersion >= 13)
						{
							message.Add("TrailStopPrice", ReadDoubleMax(reader));
						}

						if (msgVersion >= 30)
						{
							message.Add("TrailingPercent", ReadDoubleMax(reader));
						}

						if (msgVersion >= 14)
						{
							message.Add("BasisPoints", ReadDoubleMax(reader));
							message.Add("BasisPointsType", ReadIntMax(reader));
							message.Add("ComboLegsDescription", ReadString(reader));
						}

						if (msgVersion >= 29)
						{
							int comboLegsCount = ReadInt(reader);
							if (comboLegsCount > 0)
							{
								throw new System.Exception();
								//contract.ComboLegs = new List<ComboLeg>(comboLegsCount);
								//for (int i = 0; i < comboLegsCount; ++i)
								//{
								//    int conId = ReadInt(reader);
								//    int ratio = ReadInt(reader);
								//    String action = ReadString(reader);
								//    String exchange = ReadString(reader);
								//    int openClose = ReadInt(reader);
								//    int shortSaleSlot = ReadInt(reader);
								//    String designatedLocation = ReadString(reader);
								//    int exemptCode = ReadInt(reader);

								//    ComboLeg comboLeg = new ComboLeg(conId, ratio, action, exchange, openClose,
								//            shortSaleSlot, designatedLocation, exemptCode);
								//    contract.ComboLegs.Add(comboLeg);
								//}
							}

							int orderComboLegsCount = ReadInt(reader);
							if (orderComboLegsCount > 0)
							{
								throw new System.Exception();
								//order.OrderComboLegs = new List<OrderComboLeg>(orderComboLegsCount);
								//for (int i = 0; i < orderComboLegsCount; ++i)
								//{
								//    double price = ReadDoubleMax();

								//    OrderComboLeg orderComboLeg = new OrderComboLeg(price);
								//    order.OrderComboLegs.Add(orderComboLeg);
								//}
							}
						}

						if (msgVersion >= 26)
						{
							int smartComboRoutingParamsCount = ReadInt(reader);
							if (smartComboRoutingParamsCount > 0)
							{
								throw new System.Exception();
								//order.SmartComboRoutingParams = new List<TagValue>(smartComboRoutingParamsCount);
								//for (int i = 0; i < smartComboRoutingParamsCount; ++i)
								//{
								//    TagValue tagValue = new TagValue();
								//    tagValue.Tag = ReadString(reader);
								//    tagValue.Value = ReadString(reader);
								//    order.SmartComboRoutingParams.Add(tagValue);
								//}
							}
						}

						double scalePriceIncrement = Double.MaxValue;
						if (msgVersion >= 15)
						{
							if (msgVersion >= 20)
							{
								message.Add("ScaleInitLevelSize", ReadIntMax(reader));
								message.Add("ScaleSubsLevelSize", ReadIntMax(reader));
							}
							else
							{
								/* int notSuppScaleNumComponents = */
								ReadIntMax(reader);
								message.Add("ScaleInitLevelSize", ReadIntMax(reader));
							}
							scalePriceIncrement = ReadDoubleMax(reader);
							message.Add("ScalePriceIncrement", scalePriceIncrement);
						}

						if (msgVersion >= 28 && scalePriceIncrement > 0.0 && scalePriceIncrement != Double.MaxValue)
						{
							message.Add("ScalePriceAdjustValue", ReadDoubleMax(reader));
							message.Add("ScalePriceAdjustInterval", ReadIntMax(reader));
							message.Add("ScaleProfitOffset", ReadDoubleMax(reader));
							message.Add("ScaleAutoReset", ReadBoolFromInt(reader));
							message.Add("ScaleInitPosition", ReadIntMax(reader));
							message.Add("ScaleInitFillQty", ReadIntMax(reader));
							message.Add("ScaleRandomPercent", ReadBoolFromInt(reader));
						}

						if (msgVersion >= 24)
						{
							string hedgeType = ReadString(reader);
							message.Add("HedgeType", hedgeType);
							if (hedgeType != null && hedgeType.Length > 0)
								message.Add("HedgeParam", ReadString(reader));
						}

						if (msgVersion >= 25)
						{
							message.Add("OptOutSmartRouting", ReadBoolFromInt(reader));
						}

						if (msgVersion >= 19)
						{
							message.Add("ClearingAccount", ReadString(reader));
							message.Add("ClearingIntent", ReadString(reader));
						}

						if (msgVersion >= 22)
						{
							message.Add("NotHeld", ReadBoolFromInt(reader));
						}

						if (msgVersion >= 20)
						{
							if (ReadBoolFromInt(reader))
							{
								message.Add("UnderlyingComponentContractId", ReadInt(reader));
								message.Add("UnderlyingComponentDelta", ReadDouble(reader));
								message.Add("UnderlyingComponentPrice", ReadDouble(reader));
							}
						}

						if (msgVersion >= 21)
						{
							string algoStrategy = ReadString(reader);
							message.Add("AlgoStrategy", algoStrategy);
							if (algoStrategy != null && algoStrategy.Length > 0)
							{
								throw new System.Exception();
								//int algoParamsCount = ReadInt(reader);
								//if (algoParamsCount > 0)
								//{
								//    order.AlgoParams = new List<TagValue>(algoParamsCount);
								//    for (int i = 0; i < algoParamsCount; ++i)
								//    {
								//        TagValue tagValue = new TagValue();
								//        tagValue.Tag = ReadString(reader);
								//        tagValue.Value = ReadString(reader);
								//        order.AlgoParams.Add(tagValue);
								//    }
								//}
							}
						}

						if (msgVersion >= 16)
						{
							message.Add("WhatIf", ReadBoolFromInt(reader));
							message.Add("Status", ReadString(reader));
							message.Add("InitMargin", ReadString(reader));
							message.Add("MaintMargin", ReadString(reader));
							message.Add("EquityWithLoan", ReadString(reader));
							message.Add("Commission", ReadDoubleMax(reader));
							message.Add("MinCommission", ReadDoubleMax(reader));
							message.Add("MaxCommission", ReadDoubleMax(reader));
							message.Add("CommissionCurrency", ReadString(reader));
							message.Add("WarningText", ReadString(reader));
						}

						break;
					}
				case IncomingMessage.OpenOrderEnd:
					{
						message["MessageType"] = "OpenOrderEnd";
						message["Version"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.ContractData:
					{
						message["MessageType"] = "ContractData";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						int requestId = -1;
						if (msgVersion >= 3)
							requestId = ReadInt(reader);
						message.Add("Symbol", ReadString(reader));
						message.Add("SecurityType", ReadString(reader));
						message.Add("Expiry", ReadString(reader));
						message.Add("Strike", ReadDouble(reader));
						message.Add("Right", ReadString(reader));
						message.Add("Exchange", ReadString(reader));
						message.Add("Currency", ReadString(reader));
						message.Add("LocalSymbol", ReadString(reader));
						message.Add("MarketName", ReadString(reader));
						message.Add("TradingClass", ReadString(reader));
						message.Add("ContractId", ReadInt(reader));
						message.Add("MinTick", ReadDouble(reader));
						message.Add("Multiplier", ReadString(reader));
						message.Add("OrderTypes", ReadString(reader));
						message.Add("ValidExchanges", ReadString(reader));
						if (msgVersion >= 2)
						{
							message.Add("PriceMagnifier", ReadInt(reader));
						}
						if (msgVersion >= 4)
						{
							message.Add("UnderConId", ReadInt(reader));
						}
						if (msgVersion >= 5)
						{
							message.Add("LongName", ReadString(reader));
							message.Add("PrimaryExch", ReadString(reader));
						}
						if (msgVersion >= 6)
						{
							message.Add("ContractMonth", ReadString(reader));
							message.Add("Industry", ReadString(reader));
							message.Add("Category", ReadString(reader));
							message.Add("Subcategory", ReadString(reader));
							message.Add("TimeZoneId", ReadString(reader));
							message.Add("TradingHours", ReadString(reader));
							message.Add("LiquidHours", ReadString(reader));
						}
						if (msgVersion >= 8)
						{
							message.Add("EvRule", ReadString(reader));
							message.Add("EvMultiplier", ReadDouble(reader));
						}
						if (msgVersion >= 7)
						{
							int secIdListCount = ReadInt(reader);
							if (secIdListCount > 0)
							{
								throw new System.Exception();
								//contract.SecIdList = new List<TagValue>(secIdListCount);
								//for (int i = 0; i < secIdListCount; ++i)
								//{
								//    TagValue tagValue = new TagValue();
								//    tagValue.Tag = ReadString(reader);
								//    tagValue.Value = ReadString(reader);
								//    contract.SecIdList.Add(tagValue);
								//}
							}
						}

						break;
					}
				case IncomingMessage.ContractDataEnd:
					{
						message["MessageType"] = "ContractDataEnd";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.ExecutionData:
					{
						message["MessageType"] = "ExecutionData";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						//int requestId = -1;
						if (msgVersion >= 7)
							message["RequestId"] = ReadInt(reader);
						message["OrderId"] = ReadInt(reader);
						if (msgVersion >= 5)
							message["ContractId"] = ReadInt(reader);
						message["Symbol"] = ReadString(reader);
						message["SecurityType"] = ReadString(reader);
						message["Expiry"] = ReadString(reader);
						message["Strike"] = ReadDouble(reader);
						message["Right"] = ReadString(reader);
						if (msgVersion >= 9)
							message["Multiplier"] = ReadString(reader);
						message["Exchange"] = ReadString(reader);
						message["Currency"] = ReadString(reader);
						message["LocalSymbol"] = ReadString(reader);
						if (msgVersion >= 10)
							message["TradingClass"] = ReadString(reader);
						message["ExecutionId"] = ReadString(reader);
						message["Time"] = ReadString(reader);
						message["Account"] = ReadString(reader);
						message["Exchange"] = ReadString(reader);
						message["Side"] = ReadString(reader);
						message["Shares"] = ReadInt(reader);
						message["Price"] = ReadDouble(reader);
						if (msgVersion >= 2)
							message["PermId"] = ReadInt(reader);
						if (msgVersion >= 3)
							message["ClientId"] = ReadInt(reader);
						if (msgVersion >= 4)
							message["Liquidation"] = ReadInt(reader);
						if (msgVersion >= 6)
						{
							message["CumQty"] = ReadInt(reader);
							message["AvgPrice"] = ReadDouble(reader);
						}
						if (msgVersion >= 8)
							message["OrderRef"] = ReadString(reader);
						if (msgVersion >= 9)
						{
							message["EvRule"] = ReadString(reader);
							message["EvMultiplier"] = ReadDouble(reader);
						}
						break;
					}
				case IncomingMessage.ExecutionDataEnd:
					{
						message["MessageType"] = "ExecutionDataEnd";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.CommissionsReport:
					{
						message["MessageType"] = "CommissionsReport";
						message["Version"] = ReadInt(reader);
						message["ExecutionId"] = ReadString(reader);
						message["Commission"] = ReadDouble(reader);
						message["Currency"] = ReadString(reader);
						message["RealizedPNL"] = ReadDouble(reader);
						message["Yield"] = ReadDouble(reader);
						message["YieldRedemptionDate"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.FundamentalData:
					{
						message["MessageType"] = "FundamentalData";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["FundamentalData"] = ReadString(reader);
						break;
					}
				case IncomingMessage.HistoricalData:
					{
						message["MessageType"] = "HistoricalData";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						string startDateStr = "";
						string endDateStr = "";
						string completedIndicator = "finished";
						if (msgVersion >= 2)
						{
							startDateStr = ReadString(reader);
							endDateStr = ReadString(reader);
							completedIndicator += "-" + startDateStr + "-" + endDateStr;
						}
						int itemCount = ReadInt(reader);
						for (int ctr = 0; ctr < itemCount; ctr++)
						{
							string date = ReadString(reader);
							double open = ReadDouble(reader);
							double high = ReadDouble(reader);
							double low = ReadDouble(reader);
							double close = ReadDouble(reader);
							int volume = ReadInt(reader);
							double WAP = ReadDouble(reader);
							string hasGaps = ReadString(reader);
							int barCount = -1;
							if (msgVersion >= 3)
							{
								barCount = ReadInt(reader);
							}
							//parent.Wrapper.historicalData(requestId, date, open, high, low, close, volume, barCount, WAP, Boolean.Parse(hasGaps));
						}

						// send end of dataset marker.
						//parent.Wrapper.historicalDataEnd(requestId, startDateStr, endDateStr);
						throw new System.Exception();
						break;
					}
				case IncomingMessage.MarketDataType:
					{
						message["MessageType"] = "MarketDataType";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["MarketDataType"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.MarketDepth:
					{
						message["MessageType"] = "MarketDepth";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["Position"] = ReadInt(reader);
						message["Operation"] = ReadInt(reader);
						message["Side"] = ReadInt(reader);
						message["Price"] = ReadDouble(reader);
						message["Size"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.MarketDepthL2:
					{
						message["MessageType"] = "MarketDepthL2";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["Position"] = ReadInt(reader);
						message["MarketMaker"] = ReadString(reader);
						message["Operation"] = ReadInt(reader);
						message["Side"] = ReadInt(reader);
						message["Price"] = ReadDouble(reader);
						message["Size"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.NewsBulletins:
					{
						message["MessageType"] = "NewsBulletins";
						message["Version"] = ReadInt(reader);
						message["NewsMsgId"] = ReadInt(reader);
						message["NewsMsgType"] = ReadInt(reader);
						message["NewsMessage"] = ReadString(reader);
						message["OriginatingExch"] = ReadString(reader);
						break;
					}
				case IncomingMessage.Position:
					{
						message["MessageType"] = "Position";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["Account"] = ReadString(reader);
						message["ContractId"] = ReadInt(reader);
						message["Symbol"] = ReadString(reader);
						message["SecurityType"] = ReadString(reader);
						message["Expiry"] = ReadString(reader);
						message["Strike"] = ReadDouble(reader);
						message["Right"] = ReadString(reader);
						message["Multiplier"] = ReadString(reader);
						message["Exchange"] = ReadString(reader);
						message["Currency"] = ReadString(reader);
						message["LocalSymbol"] = ReadString(reader);
						if (msgVersion >= 2)
							message["TradingClass"] = ReadString(reader);
						message["Size"] = ReadInt(reader);
						if (msgVersion >= 3)
							message["AverageCost"] = ReadDouble(reader);
						break;
					}
				case IncomingMessage.PositionEnd:
					{
						message["MessageType"] = "PositionEnd";
						message["Version"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.RealTimeBars:
					{
						message["MessageType"] = "RealTimeBars";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["Time"] = ReadLong(reader);
						message["Open"] = ReadDouble(reader);
						message["High"] = ReadDouble(reader);
						message["Low"] = ReadDouble(reader);
						message["Close"] = ReadDouble(reader);
						message["Volume"] = ReadLong(reader);
						message["WAP"] = ReadDouble(reader);
						message["Count"] = ReadInt(reader);
						break;
					}
				case IncomingMessage.ScannerParameters:
					{
						message["MessageType"] = "ScannerParameters";
						message["Version"] = ReadInt(reader);
						message["Xml"] = ReadString(reader);
						break;
					}
				case IncomingMessage.ScannerData:
					{
						message["MessageType"] = "ScannerData";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);

						int numberOfElements = ReadInt(reader);
						for (int i = 0; i < numberOfElements; i++)
						{
							throw new System.Exception();
							//int rank = ReadInt(reader);
							//if (msgVersion >= 3)
							//    conDet.Summary.ConId = ReadInt(reader);
							//conDet.Summary.Symbol = ReadString(reader);
							//conDet.Summary.SecType = ReadString(reader);
							//conDet.Summary.Expiry = ReadString(reader);
							//conDet.Summary.Strike = ReadDouble(reader);
							//conDet.Summary.Right = ReadString(reader);
							//conDet.Summary.Exchange = ReadString(reader);
							//conDet.Summary.Currency = ReadString(reader);
							//conDet.Summary.LocalSymbol = ReadString(reader);
							//conDet.MarketName = ReadString(reader);
							//conDet.Summary.TradingClass = ReadString(reader);
							//string distance = ReadString(reader);
							//string benchmark = ReadString(reader);
							//string projection = ReadString(reader);
							//string legsStr = null;
							//if (msgVersion >= 2)
							//{
							//    legsStr = ReadString(reader);
							//}
							////parent.Wrapper.scannerData(requestId, rank, conDet, distance, benchmark, projection, legsStr);
						}

						break;
					}
				case IncomingMessage.ReceiveFA:
					{
						message["MessageType"] = "ReceiveFA";
						message["Version"] = ReadInt(reader);
						message["FADataType"] = ReadInt(reader);
						message["FAData"] = ReadString(reader);
						break;
					}
				case IncomingMessage.BondContractData:
					{
						message["MessageType"] = "BondContractData";
						int msgVersion;
						message["Version"] = msgVersion = ReadInt(reader);
						if (msgVersion >= 3)
							message["RequestId"] = ReadInt(reader);

						throw new System.Exception();
						//contract.Summary.Symbol = ReadString(reader);
						//contract.Summary.SecType = ReadString(reader);
						//contract.Cusip = ReadString(reader);
						//contract.Coupon = ReadDouble(reader);
						//contract.Maturity = ReadString(reader);
						//contract.IssueDate = ReadString(reader);
						//contract.Ratings = ReadString(reader);
						//contract.BondType = ReadString(reader);
						//contract.CouponType = ReadString(reader);
						//contract.Convertible = ReadBoolFromInt(reader);
						//contract.Callable = ReadBoolFromInt(reader);
						//contract.Putable = ReadBoolFromInt(reader);
						//contract.DescAppend = ReadString(reader);
						//contract.Summary.Exchange = ReadString(reader);
						//contract.Summary.Currency = ReadString(reader);
						//contract.MarketName = ReadString(reader);
						//contract.Summary.TradingClass = ReadString(reader);
						//contract.Summary.ConId = ReadInt(reader);
						//contract.MinTick = ReadDouble(reader);
						//contract.OrderTypes = ReadString(reader);
						//contract.ValidExchanges = ReadString(reader);
						//if (msgVersion >= 2)
						//{
						//    contract.NextOptionDate = ReadString(reader);
						//    contract.NextOptionType = ReadString(reader);
						//    contract.NextOptionPartial = ReadBoolFromInt(reader);
						//    contract.Notes = ReadString(reader);
						//}
						//if (msgVersion >= 4)
						//{
						//    contract.LongName = ReadString(reader);
						//}
						//if (msgVersion >= 6)
						//{
						//    contract.EvRule = ReadString(reader);
						//    contract.EvMultiplier = ReadDouble(reader);
						//}

						if (msgVersion >= 5)
						{
							int secIdListCount = ReadInt(reader);
							if (secIdListCount > 0)
							{
								throw new System.Exception();
								//contract.SecIdList = new List<TagValue>();
								//for (int i = 0; i < secIdListCount; ++i)
								//{
								//    TagValue tagValue = new TagValue();
								//    tagValue.Tag = ReadString(reader);
								//    tagValue.Value = ReadString(reader);
								//    contract.SecIdList.Add(tagValue);
								//}
							}
						}
						//parent.Wrapper.bondContractDetails(requestId, contract);
						throw new System.Exception();
						break;
					}
				case IncomingMessage.VerifyMessageApi:
					{
						message["MessageType"] = "VerifyMessageApi";
						message["Version"] = ReadInt(reader);
						message["ApiData"] = ReadString(reader);
						break;
					}
				case IncomingMessage.VerifyCompleted:
					{
						message["MessageType"] = "VerifyCompleted";
						message["Version"] = ReadInt(reader);
						bool isSuccessful = String.Compare(ReadString(reader), "true", true) == 0;
						string errorText = ReadString(reader);

						if (isSuccessful)
							throw new System.Exception();
							//parent.startApi();

						//parent.Wrapper.verifyCompleted(isSuccessful, errorText);
						throw new System.Exception();
						break;
					}
				case IncomingMessage.DisplayGroupList:
					{
						message["MessageType"] = "DisplayGroupList";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["Groups"] = ReadString(reader);
						break;
					}
				case IncomingMessage.DisplayGroupUpdated:
					{
						message["MessageType"] = "DisplayGroupUpdated";
						message["Version"] = ReadInt(reader);
						message["RequestId"] = ReadInt(reader);
						message["ContractInfo"] = ReadString(reader);
						break;
					}
				default:
					{
						throw new System.Exception();
					}
			}

			return message;
		}

		public static double ReadDouble(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0)
				return 0;
			return Double.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
		}

		public static double ReadDoubleMax(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0)
				return Double.MaxValue; 
			return Double.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
		}

		public static long ReadLong(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0)
				return 0;
			else return Int64.Parse(str);
		}

		public static int ReadInt(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0)
				return 0;
			return Int32.Parse(str);
		}

		public static int ReadIntMax(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0) 
				return Int32.MaxValue;
			return Int32.Parse(str);
		}

		public static bool ReadBoolFromInt(System.IO.BinaryReader reader)
		{
			string str = ReadString(reader);
			if (str == null || str.Length == 0)
				return false;
			return Int32.Parse(str) != 0;
		}

		private static readonly StringBuilder _stringBuilder = new StringBuilder();

		public static string ReadString(System.IO.BinaryReader reader)
		{
            // Null terminated strings.
            byte b = reader.ReadByte();
            if (b == 0)
                return null;
            _stringBuilder.Length = 0;
            _stringBuilder.Append((char)b);
            while (true)
            {
                b = reader.ReadByte();
                if (b == 0)
                    break;
                _stringBuilder.Append((char)b);
            }
            return _stringBuilder.ToString();
        }

	}
}
