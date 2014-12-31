/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace IBAPI
{
    /**
     * @class Order
     * @brief The order's description.
     * @sa Contract, OrderComboLeg, OrderState
     */
    public class Order
    {
		public static int CUSTOMER = 0;

		//// if false, order will be created but not transmited
		//private bool transmit;

		public int OrderId;
		public int ClientId;

        // The Host order identifier.
        public int PermId;

        public string Action;

        // The number of positions being bought/sold.
        public int TotalQuantity;

        // The order's type.  Available Orders are at https://www.interactivebrokers.com/en/software/api/apiguide/tables/supported_order_types.htm 
        public string OrderType;

        // The LIMIT price.  Used for limit, stop-limit and relative orders. In all other cases specify zero. For relative orders with no limit price, also specify zero.
		public double LimitPrice = Double.MaxValue;

        // Generic field to contain the stop price for STP LMT orders, trailing amount, etc.
		public double AuxPrice = Double.MaxValue;

        /**
          * @brief The time in force.
         * Valid values are: \n
         *      DAY - Valid for the day only.\n
         *      GTC - Good until canceled. The order will continue to work within the system and in the marketplace until it executes or is canceled. GTC orders will be automatically be cancelled under the following conditions:
         *          \t\t If a corporate action on a security results in a stock split (forward or reverse), exchange for shares, or distribution of shares.
         *          \t\t If you do not log into your IB account for 90 days.\n
         *          \t\t At the end of the calendar quarter following the current quarter. For example, an order placed during the third quarter of 2011 will be canceled at the end of the first quarter of 2012. If the last day is a non-trading day, the cancellation will occur at the close of the final trading day of that quarter. For example, if the last day of the quarter is Sunday, the orders will be cancelled on the preceding Friday.\n
         *          \t\t Orders that are modified will be assigned a new “Auto Expire” date consistent with the end of the calendar quarter following the current quarter.\n
         *          \t\t Orders submitted to IB that remain in force for more than one day will not be reduced for dividends. To allow adjustment to your order price on ex-dividend date, consider using a Good-Til-Date/Time (GTD) or Good-after-Time/Date (GAT) order type, or a combination of the two.\n
         *      IOC - Immediate or Cancel. Any portion that is not filled as soon as it becomes available in the market is canceled.\n
         *      GTD. - Good until Date. It will remain working within the system and in the marketplace until it executes or until the close of the market on the date specified\n
         *      OPG - Use OPG to send a market-on-open (MOO) or limit-on-open (LOO) order.\n
         *      FOK - If the entire Fill-or-Kill order does not execute as soon as it becomes available, the entire order is canceled.\n
         *      DTC - Day until Canceled \n
          */
        public string TimeInForce;


        // One-Cancels-All group identifier.
        public string OneCancelsAllGroup;

        /**
         * @brief Tells how to handle remaining orders in an OCA group when one order or part of an order executes.
         * Valid values are:\n
         *      \t\t 1 = Cancel all remaining orders with block.\n
         *      \t\t 2 = Remaining orders are proportionately reduced in size with block.\n
         *      \t\t 3 = Remaining orders are proportionately reduced in size with no block.\n
         * If you use a value "with block" gives your order has overfill protection. This means that only one order in the group will be routed at a time to remove the possibility of an overfill.
         */
        public int OneCancelsAllType;

        // The order reference.  Intended for institutional customers only, although all customers may use it to identify the API client that sent the order when multiple API clients are running.
        public string OrderRef;

        // Specifies whether the order will be transmitted by TWS. If set to false, the order will be created at TWS but will not be sent.
        public bool Transmit = true;

        // The order ID of the parent order, used for bracket and auto trailing stop orders.
        public int ParentId;

        // If set to true, specifies that the order is an ISE Block order.
        public bool BlockOrder;

        // If set to true, specifies that the order is a Sweep-to-Fill order.
        public bool SweepToFill;

        // The publicly disclosed order size, used when placing Iceberg orders.
        public int DisplaySize;

        /**
         * @brief Specifies how Simulated Stop, Stop-Limit and Trailing Stop orders are triggered.
         * Valid values are:\n
         *  0 - The default value. The "double bid/ask" function will be used for orders for OTC stocks and US options. All other orders will used the "last" function.\n
         *  1 - use "double bid/ask" function, where stop orders are triggered based on two consecutive bid or ask prices.\n
         *  2 - "last" function, where stop orders are triggered based on the last price.\n
         *  3 double last function.\n
         *  4 bid/ask function.\n
         *  7 last or bid/ask function.\n
         *  8 mid-point function.\n
         */
        public int TriggerMethod;

        // If set to true, allows orders to also trigger or fill outside of regular trading hours.
        public bool OutsideRth = false;

        // If set to true, the order will not be visible when viewing the market depth.  This option only applies to orders routed to the ISLAND exchange.
        public bool Hidden;

        // Specifies the date and time after which the order will be active.  Format: yyyymmdd hh:mm:ss {optional Timezone}
        public string GoodAfterTime;

        // The date and time until the order will be active.  You must enter GTD as the time in force to use this string. The trade's "Good Till Date," format "YYYYMMDD hh:mm:ss (optional time zone)"
        public string GoodTillDate;

        // Overrides TWS constraints.  Precautionary constraints are defined on the TWS Presets page, and help ensure tha tyour price and size order values are reasonable. Orders sent from the API are also validated against these safety constraints, and may be rejected if any constraint is violated. To override validation, set this parameter’s value to True.
        public bool OverridePercentageConstraints;

        /**
         * @brief -
         * Individual = 'I'\n
         * Agency = 'A'\n
         * AgentOtherMember = 'W'\n
         * IndividualPTIA = 'J'\n
         * AgencyPTIA = 'U'\n
         * AgentOtherMemberPTIA = 'M'\n
         * IndividualPT = 'K'\n
         * AgencyPT = 'Y'\n
         * AgentOtherMemberPT = 'N'\n
         */
        public string Rule80A;

        // Indicates whether or not all the order has to be filled on a single execution.
        public bool AllOrNone;

        // Identifies a minimum quantity order type.
        public int MinQty = Int32.MaxValue;

        // The percent offset amount for relative orders.
        public double PercentOffset = Double.MaxValue;

        // Trail stop price for TRAILIMIT orders.
        public double TrailStopPrice = Double.MaxValue;

        /**
         * @brief Specifies the trailing amount of a trailing stop order as a percentage.
         * Observe the following guidelines when using the trailingPercent field:\n
         *    - This field is mutually exclusive with the existing trailing amount. That is, the API client can send one or the other but not both.\n
         *    - This field is read AFTER the stop price (barrier price) as follows: deltaNeutralAuxPrice stopPrice, trailingPercent, scale order attributes\n
         *    - The field will also be sent to the API in the openOrder message if the API client version is >= 56. It is sent after the stopPrice field as follows: stopPrice, trailingPct, basisPoint\n
         */
        public double TrailingPercent = Double.MaxValue;

        // The Financial Advisor group the trade will be allocated to.  Use an empty string if not applicable.
        public string FaGroup;

        // The Financial Advisor allocation profile the trade will be allocated to.  Use an empty string if not applicable.
        public string FaProfile;

        // The Financial Advisor allocation method the trade will be allocated to.  Use an empty string if not applicable.
        public string FaMethod;

        // The Financial Advisor percentage concerning the trade's allocation.  Use an empty string if not applicable.
        public string FaPercentage;

        // For institutional customers only.  Available for institutional clients to determine if this order is to open or close a position. Valid values are O (open), C (close).
        public string OpenClose = "O";

        // The order's origin.  Same as TWS "Origin" column. Identifies the type of customer from which the order originated. Valid values are 0 (customer), 1 (firm).
        public int Origin = CUSTOMER;

        // For institutions only. Valid values are: 1 (broker holds shares) or 2 (shares come from elsewhere).
        public int ShortSaleSlot;

        // Used only when shortSaleSlot is 2.  For institutions only. Indicates the location where the shares to short come from. Used only when short sale slot is set to 2 (which means that the shares to short are held elsewhere and not with IB).
        public string DesignatedLocation = "";

        public int ExemptCode = -1;

        // The amount off the limit price allowed for discretionary orders.
        public double DiscretionaryAmt;

        // Trade with electronic quotes.
        public bool ETradeOnly;

        // Trade with firm quotes.
        public bool FirmQuoteOnly;

        // Maximum smart order distance from the NBBO.
        public double NbboPriceCap = Double.MaxValue;

        // Use to opt out of default SmartRouting for orders routed directly to ASX.  This attribute defaults to false unless explicitly set to true. When set to false, orders routed directly to ASX will NOT use SmartRouting. When set to true, orders routed directly to ASX orders WILL use SmartRouting.
        public bool OptOutSmartRouting = false;

        /**
         * @brief - 
         * For BOX orders only. Values include:
         *      1 - match \n
         *      2 - improvement \n
         *      3 - transparent \n
         */
        public int AuctionStrategy;

        // The auction's starting price.  For BOX orders only.
        public double StartingPrice = Double.MaxValue;

        // The stock's reference price.  The reference price is used for VOL orders to compute the limit price sent to an exchange (whether or not Continuous Update is selected), and for price range monitoring.
        public double StockRefPrice = Double.MaxValue;

        // The stock's Delta.  For orders on BOX only.
        public double Delta = Double.MaxValue;

        // The lower value for the acceptable underlying stock price range.  For price improvement option orders on BOX and VOL orders with dynamic management.
        public double StockRangeLower = Double.MaxValue;

        // The upper value for the acceptable underlying stock price range.  For price improvement option orders on BOX and VOL orders with dynamic management.
        public double StockRangeUpper = Double.MaxValue;

        // The option price in volatility, as calculated by TWS' Option Analytics.  This value is expressed as a percent and is used to calculate the limit price sent to the exchange.
        public double Volatility = Double.MaxValue;

        /**
         * @brief
         * Values include:\n
         *      1 - Daily Volatility
         *      2 - Annual Volatility
         */
        public int VolatilityType = Int32.MaxValue;

        // Specifies whether TWS will automatically update the limit price of the order as the underlying price moves.  VOL orders only.
        public int ContinuousUpdate;

        /**
         * @brief Specifies how you want TWS to calculate the limit price for options, and for stock range price monitoring.
         * VOL orders only. Valid values include: \n
         *      1 - Average of NBBO \n
         *      2 - NBB or the NBO depending on the action and right. \n
         */
        public int ReferencePriceType = Int32.MaxValue;

        /**
         * @brief Enter an order type to instruct TWS to submit a delta neutral trade on full or partial execution of the VOL order.
         * VOL orders only. For no hedge delta order to be sent, specify NONE.
         */
		public string DeltaNeutralOrderType = "";

        // Use this field to enter a value if the value in the deltaNeutralOrderType field is an order type that requires an Aux price, such as a REL order.  VOL orders only.
        public double DeltaNeutralAuxPrice = Double.MaxValue;

        public int DeltaNeutralConId = 0;

		public string DeltaNeutralSettlingFirm = "";

		public string DeltaNeutralClearingAccount = "";

		public string DeltaNeutralClearingIntent = "";

        // Specifies whether the order is an Open or a Close order and is used when the hedge involves a CFD and and the order is clearing away.
		public string DeltaNeutralOpenClose = "";

        // Used when the hedge involves a stock and indicates whether or not it is sold short.
        public bool DeltaNeutralShortSale = false;

        // Has a value of 1 (the clearing broker holds shares) or 2 (delivered from a third party). If you use 2, then you must specify a deltaNeutralDesignatedLocation.
        public int DeltaNeutralShortSaleSlot = 0;

        // Used only when deltaNeutralShortSaleSlot = 2.
		public string DeltaNeutralDesignatedLocation = "";

        // For EFP orders only.
        public double BasisPoints = Double.MaxValue;

        // For EFP orders only.
        public int BasisPointsType = Int32.MaxValue;

        // Defines the size of the first, or initial, order component.  For Scale orders only.
        public int ScaleInitLevelSize = Int32.MaxValue;

        // Defines the order size of the subsequent scale order components.  For Scale orders only. Used in conjunction with scaleInitLevelSize().
        public int ScaleSubsLevelSize = Int32.MaxValue;

        // Defines the price increment between scale components.  For Scale orders only. This value is compulsory.
        public double ScalePriceIncrement = Double.MaxValue;

        // For extended Scale orders.
        public double ScalePriceAdjustValue = Double.MaxValue;

        // For extended Scale orders.
        public int ScalePriceAdjustInterval = Int32.MaxValue;

        // For extended scale orders.
        public double ScaleProfitOffset = Double.MaxValue;

        // For extended scale orders.
        public bool ScaleAutoReset = false;

        // For extended scale orders.
        public int ScaleInitPosition = Int32.MaxValue;

        // For extended scale orders.
        public int ScaleInitFillQty = Int32.MaxValue;

        // For extended scale orders.
        public bool ScaleRandomPercent = false;

        /**
         * @brief For hedge orders.
         * Possible values include:\n
         *      D - delta \n
         *      B - beta \n
         *      F - FX \n
         *      P - Pair \n
         */
        public string HedgeType;

        // Beta = x for Beta hedge orders, ratio = y for Pair hedge order
        public string HedgeParam;

        // The account the trade will be allocated to.
        public string Account;

        // Institutions only. Indicates the firm which will settle the trade.
        public string SettlingFirm;

        // Specifies the true beneficiary of the order.  For IBExecution customers. This value is required for FUT/FOP orders for reporting to the exchange.
        public string ClearingAccount;

        // For exeuction-only clients to know where do they want their shares to be cleared at.  Valid values are: IB, Away, and PTA (post trade allocation).
        public string ClearingIntent;

        /**
         * @brief The algorithm strategy.
         * As of API verion 9.6, the following algorithms are supported:\n
         *      ArrivalPx - Arrival Price \n
         *      DarkIce - Dark Ice \n
         *      PctVol - Percentage of Volume \n
         *      Twap - TWAP (Time Weighted Average Price) \n
         *      Vwap - VWAP (Volume Weighted Average Price) \n
         * For more information about IB's API algorithms, refer to https://www.interactivebrokers.com/en/software/api/apiguide/tables/ibalgo_parameters.htm
        */
        public string AlgoStrategy;

        // The list of parameters for the IB algorithm.  For more information about IB's API algorithms, refer to https://www.interactivebrokers.com/en/software/api/apiguide/tables/ibalgo_parameters.htm
		public List<KeyValuePair<string, string>> AlgoParams;

        // Allows to retrieve the commissions and margin information.  When placing an order with this attribute set to true, the order will not be placed as such. Instead it will used to request the commissions and margin information that would result from this order.
        public bool WhatIf = false;

        public string AlgoId;

        // Orders routed to IBDARK are tagged as “post only” and are held in IB's order book, where incoming SmartRouted orders from other IB customers are eligible to trade against them.  For IBDARK orders only.
        public bool NotHeld = false;

        // Parameters for combo routing.  For more information, refer to https://www.interactivebrokers.com/en/software/api/apiguide/tables/smart_combo_routing.htm   
		public List<KeyValuePair<string, string>> SmartComboRoutingParams;

        //// The attributes for all legs within a combo order.
		//public List<OrderComboLeg> OrderComboLegs;

		public List<KeyValuePair<string, string>> OrderMiscOptions;

        // for GTC orders.
		public string ActiveStartTime = "";

        // for GTC orders.
		public string ActiveStopTime = "";

		// Used for scale orders.
		public string ScaleTable = "";

    }
}
