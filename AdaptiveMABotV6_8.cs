// Adaptive MA Bot: Enhanced Correlation Analysis and Adaptive Learning
// Description:
// An automated trading bot for the cTrader platform designed for intraday and swing trading on cryptocurrencies like BTCUSD, ETHUSD.
// It uses three Moving Averages (MAs) to identify trend direction, a momentum filter based on MA1 percentage change,
// an optional RSI filter, an optional ATR filter for volatility, and confirmation candles to validate trade entries.
// The bot implements risk management with configurable stop-loss, take-profit, trailing stop settings, a cooldown period
// after consecutive losses, and swap avoidance. It displays trade statistics, status, and MA lines in a chart overlay.
// FEATURES PHASE 1 ADAPTIVE LEARNING: Market regime detection, performance-based parameter adjustment, rolling statistics.
// FEATURES PHASE 2 ENHANCED PARAMETER CORRELATION ANALYSIS with configurable bounds, adaptive sampling, and flexible optimization thresholds
// for continuous optimization based on trading performance and market conditions.
//
// cTrader Version: 5.4.9, Release Date: 2025-07-15
// Compiler: .NET 6.0
// Version: 6.514, Date: 2025-08-31, Time: 02:52 CEST
// Author: deesun
// Website:
//
//
//
// Features:
// * Uses three Moving Averages (MAs, selectable: Simple, Exponential, Weighted, Hull, DoubleExponential) to identify trend direction
// * Optional RSI filter for overbought/oversold conditions, with divergence check option
// * Optional ATR filter with same MA types as above to control trade entries based on volatility, with advanced modes (threshold, position sizing, multi-timeframe)
// * Momentum filter based on MA1 percentage change, with adaptive option
// * Confirmation candles to validate trade entries (directional: bullish for long, bearish for short)
// * Selectable entry conditions (Crossover, Breakout, OpenCloseCross, Pullback, MA2/MA3 Crossover, MA2 Resistance/Support)
// * Additional filters: ADX for trend strength, Volume for liquidity (static or dynamic mode with min/max), Spread for execution quality, MA2/MA3 Spread for crossover validation
// * Risk management with configurable stop-loss, take-profit, and trailing stop; selectable mode (static or dynamic ATR-based)
// * Cooldown period after consecutive losses
// * Configurable soft and hard time limits for position closure, with dropdown menu to enable/disable
// * Avoid swaps by closing profitable positions 1 hour before swap time
// * Weekend filter to block trades before and after the weekend, with enable/disable option
// * Chart overlay with trade statistics, status display, including Sharpe Ratio, and MA lines (MA1, MA2, MA3) with configurable colors and show/hide switches
// * Position info in the status block (profit/loss in %)
// * Removes only bot-created markers on start to preserve user-defined markers
// * Configurable info/status block display and chart marker visibility
// * Trade direction control (both, only longs, only shorts) and signal reversal option
// * Trading session filter (All, London, NewYork, Tokyo, Sydney, LondonNewYorkOverlap) with session display in info block
// * Look-Ahead methods: News Event Avoid, Time-Based Look-Ahead
// * MA2/MA3 Spread filter for MA2/MA3 Crossover entry condition, with Spread Candles parameter to hold entry condition
// * MA2ResistanceSupport requires true bounce from MA2 (wick touch or near, entry on candle close after direction change)
// * ADAPTIVE LEARNING FEATURES (Phase 1):
//   * Market Regime Detection: Identifies Trending/Ranging/HighVolatility/LowVolatility using ATR/ADX with configurable thresholds
//   * Performance-Based Parameter Adjustment: Automatically adjusts ATR/RSI thresholds based on recent win rate analysis
//   * Rolling Statistics: Smoothed ATR/volume/ADX calculations for improved regime detection accuracy
//   * Independent Feature Controls: Enable/disable learning features individually with detailed tooltips
//   * Comprehensive Learning Logging: [LEARNING] prefixed messages with detailed parameter adjustment reports
// * ENHANCED PARAMETER CORRELATION ANALYSIS (Phase 2):
//   * Configurable Parameter Bounds: Flexible min/max limits for ATR, RSI, and momentum thresholds (prevents extremes while allowing adaptation)
//   * Adaptive Sample Size: Dynamic minimum sample requirements based on market volatility for better statistical reliability
//   * Configurable Correlation Threshold: Adjustable sensitivity for optimization suggestions (0.1-0.8 range)
//   * Enhanced Statistical Methods: Improved correlation calculations with adaptive sampling for varying market conditions
//
// Change Log:

// Version 6.5_enhanced_correlation, 2025-08-30, 00:52 CEST: Enhanced Parameter Correlation Analysis
// - ENHANCED: Configurable Parameter Bounds
//   * Added configurable min/max bounds for ATR, RSI, and Momentum thresholds
//   * ATR Threshold Min/Max: 0.0005-0.05 (expanded from fixed 0.001-0.01)
//   * RSI Buy/Sell Threshold Min/Max: 20-40/60-80 (expanded ranges)
//   * Momentum Threshold Min/Max: 0.01-1.0 (expanded from fixed 0.01-1.0)
//   * Prevents extreme values while allowing flexibility for different market conditions
// - ENHANCED: Adaptive Sample Size Requirements
//   * Added AdaptiveSampleSize parameter (default: true) for dynamic sample requirements
//   * Automatically adjusts minimum samples based on market volatility (ATR factor)
//   * Higher volatility requires more samples for statistical reliability
//   * Base requirement * volatility factor, bounded between base and 3x base
// - ENHANCED: Configurable Correlation Threshold
//   * Added OptimizationCorrelationThreshold parameter (default: 0.3, range: 0.1-0.8)
//   * Controls minimum correlation strength for optimization suggestions
//   * Allows fine-tuning sensitivity of parameter optimization
// - ENHANCED: Improved Parameter Correlation Analysis
//   * All correlation methods now use adaptive sample sizing when enabled
//   * Better statistical reliability in varying market conditions
//   * More flexible optimization suggestions based on configurable thresholds
//   * Enhanced logging with adaptive sample size information
//
// Version 6.0_learningRegime, 2025-08-30, 02:17 CEST: Phase 1 Adaptive Learning Implementation
// - ADDED: Market Regime Detection using ATR and ADX indicators
//   * Identifies Trending, Ranging, High Volatility, and Low Volatility conditions
//   * Configurable lookback period (50 bars) and threshold multipliers
//   * Real-time regime display in TopRight info panel
// - ADDED: Performance-Based Parameter Adjustment
//   * Automatically tightens/loosens ATR and RSI thresholds based on recent win rate
//   * Analyzes last 20 trades for performance assessment
//   * Tightens filters when win rate < 40%, loosens when > 60%
//   * Includes parameter bounds to prevent extreme values
// - ADDED: Rolling Statistics for Key Metrics
//   * Rolling ATR, volume, and ADX calculations with configurable period (20)
//   * Improved regime detection accuracy with smoothed data
//   * Efficient queue-based implementation for real-time updates
// - ADDED: Independent Learning Feature Controls
//   * EnableLearningFeatures: Toggle parameter adjustment (default: true)
//   * EnableRegimeDetection: Toggle market regime detection (default: true)
//   * EnableRollingStats: Toggle rolling statistics (default: true)
//   * All features can be enabled/disabled independently
// - ENHANCED: Comprehensive Learning Logging
//   * [LEARNING] prefixed messages for easy identification
//   * Detailed parameter adjustment reports with before/after values
//   * Performance analysis with win rate calculations
//   * Transparent operation with clear reasoning for adjustments
//
//
// To-Do:
// * Create demo version with fixed 10 EUR position size, non-adjustable, for free testing (user must buy full version for adjustable sizing)
//
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class AdaptiveMABot_v6_514_crypto : Robot
    {
        // Constants for magic numbers to improve maintainability
        private const double PERCENTAGE_MULTIPLIER = 100.0;
        private const double ATR_MULTIPLIER_DEFAULT = 2.0;
        private const double EMA_SMOOTHING_FACTOR = 2.0;
        private const double SWAP_AVOIDANCE_HOUR = 23.0;
        private const double DEFAULT_MARKER_OFFSET = 0.00001;
        private const double BUY_MARKER_OFFSET = -0.00002;
        private const double SELL_MARKER_OFFSET = 0.00002;
        private const double CURRENCY_TEXT_OFFSET = 0.00004;
        private const double DOJI_BODY_RATIO = 0.1;
        private const double HAMMER_BODY_RATIO = 0.3;
        private const double HAMMER_WICK_MULTIPLIER = 2.0;
        private const double CHART_MARKER_MULTIPLIER = 2.0;
        private const double CHART_TEXT_MULTIPLIER = 3.0;
        private const double CHART_CURRENCY_MULTIPLIER = 4.0;
        private const double STATUS_FIELD_HEIGHT_MULTIPLIER = 30.0;
        private const double STATUS_FIELD_WIDTH_MULTIPLIER = 60.0;
        private const double MAX_VOLUME_LIMIT = 100.0; // Maximum volume in lots to prevent broker limit violations
        private const double MAX_RISK_PERCENTAGE = 0.1; // Maximum 10% of account balance per trade

        // Time constants for session management
        private static readonly TimeSpan LONDON_OPEN = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan LONDON_CLOSE = new TimeSpan(17, 0, 0);
        private static readonly TimeSpan NEWYORK_OPEN = new TimeSpan(13, 0, 0);
        private static readonly TimeSpan NEWYORK_CLOSE = new TimeSpan(22, 0, 0);
        private static readonly TimeSpan TOKYO_OPEN = new TimeSpan(0, 0, 0);
        private static readonly TimeSpan TOKYO_CLOSE = new TimeSpan(9, 0, 0);
        private static readonly TimeSpan SYDNEY_OPEN = new TimeSpan(22, 0, 0);
        private static readonly TimeSpan SYDNEY_CLOSE = new TimeSpan(7, 0, 0);
        private static readonly TimeSpan LONDON_NEWYORK_OVERLAP_START = new TimeSpan(13, 0, 0);
        private static readonly TimeSpan LONDON_NEWYORK_OVERLAP_END = new TimeSpan(17, 0, 0);

        // News event time windows
        private static readonly TimeSpan NEWS_EVENT_START = new TimeSpan(13, 0, 0);
        private static readonly TimeSpan NEWS_EVENT_END = new TimeSpan(14, 0, 0);

        // High volatility time windows
        private static readonly TimeSpan HIGH_VOLATILITY_BUFFER = new TimeSpan(0, 15, 0);
        private static readonly TimeSpan LONDON_VOLATILITY_START = new TimeSpan(7, 45, 0);
        private static readonly TimeSpan LONDON_VOLATILITY_END = new TimeSpan(8, 15, 0);
        private static readonly TimeSpan NEWYORK_VOLATILITY_START = new TimeSpan(12, 45, 0);
        private static readonly TimeSpan NEWYORK_VOLATILITY_END = new TimeSpan(13, 15, 0);
        private static readonly TimeSpan TOKYO_VOLATILITY_START = new TimeSpan(23, 45, 0);
        private static readonly TimeSpan TOKYO_VOLATILITY_END = new TimeSpan(0, 15, 0);

        public enum TimeLimitMode
        {
            Yes,
            No,
            OnlySoft,
            OnlyHard
        }
        public enum MovingAverageType
        {
            Simple,
            Exponential,
            Weighted,
            Hull,
            DoubleExponential
        }
        public enum EntryConditionType
        {
            Crossover,
            Breakout,
            OpenCloseCross,
            Pullback,
            MA2MA3Crossover,
            MA2ResistanceSupport
        }
        public enum RiskManagementMode
        {
            Static,
            DynamicATR
        }
        public enum MomentumMode
        {
            Static,
            Dynamic
        }
        public enum RsiMode
        {
            Normal,
            Divergence
        }
        public enum AtrFilterMode
        {
            Threshold,
            PositionSizing,
            MultiTimeframe
        }
        public enum VolumeFilterMode
        {
            Static,
            Dynamic
        }
        public enum InfoBlockMode
        {
            On,
            Off,
            OnlyInfoBlock,
            OnlyStatusBlock
        }
        public enum ChartMarkerMode
        {
            Yes,
            No
        }
        public enum TradeDirectionMode
        {
            Both,
            OnlyLongs,
            OnlyShorts
        }
        public enum ReverseTradingMode
        {
            Yes,
            No
        }
        public enum TrailingStopMode
        {
            Off,
            TSRemovesTP,
            TSPlusTP
        }
        public enum RemoveMarkersMode
        {
            None,
            All,
            OnlyText,
            OnlyArrows
        }
        public enum ChartTypeMode
        {
            CandleStick,
            HeikinAshi,
            Bar
        }
        public enum TradingSessionMode
        {
            All,
            London,
            NewYork,
            Tokyo,
            Sydney,
            LondonNewYorkOverlap
        }
        public enum CandlePatternMode
        {
            None,
            Engulfing,
            Doji,
            Hammer,
            ShootingStar,
            MorningStar,
            EveningStar,
            BullishHarami,
            BearishHarami,
            ThreeWhiteSoldiers,
            ThreeBlackCrows,
            BullishEngulfing,
            BearishEngulfing,
            PiercingPattern,
            DarkCloudCover
        }
        public enum MaPlotMode
        {
            Full,
            Optimized,
            Polyline
        }
        public enum MarketRegime
        {
            Trending,
            Ranging,
            HighVolatility,
            LowVolatility
        }
        public enum LoggingLevel
        {
            Off,
            Full,
            Info,
            Debug,
            OnlyImportant,
            Warning,
            Error,
            OnlyCritical,
            OnlyTrades
        }
        public enum ExecutionMode
        {
            OnTick,
            OnBar,
            Hybrid
        }
        [Parameter("Use Chart Timeframe", DefaultValue = true, Group = "Chart & Display Settings", Description = "Sync AnalysisTimeframe with current chart timeframe: Yes (auto-sync); No (use manual AnalysisTimeframe).")]
        public bool UseChartTimeframe { get; set; }
        [Parameter("Analysis Timeframe", DefaultValue = "m15", Group = "Chart & Display Settings", Description = "Chart timeframe for analysis; only takes effect if 'Use Chart Timeframe' is set to No.")]
        public TimeFrame AnalysisTimeframe { get; set; }
        [Parameter("Chart Type", DefaultValue = ChartTypeMode.CandleStick, Group = "Chart & Display Settings", Description = "Chart type for analysis: CandleStick (standard candles); HeikinAshi (smoothed candles); Bar (OHLC bars).")]
        public ChartTypeMode ChartType { get; set; }
        [Parameter("Historical Bars To Load", DefaultValue = 1000, MinValue = 1000, MaxValue = 20000, Group = "Chart & Display Settings", Description = "Number of historical bars to load for MA calculation and plotting; higher values for more history.")]
        public int HistoricalBarsToLoad { get; set; }
        [Parameter("Trading Sessions", DefaultValue = TradingSessionMode.All, Group = "Trading Setup", Description = "Select trading session: All (any time); London (08:00-17:00 UTC); NewYork (13:00-22:00 UTC); Tokyo (00:00-09:00 UTC); Sydney (22:00-07:00 UTC); LondonNewYorkOverlap (13:00-17:00 UTC).")]
        public TradingSessionMode TradingSession { get; set; }
        [Parameter("Show Info Blocks", DefaultValue = InfoBlockMode.Off, Group = "Chart & Display Settings", Description = "Display info/status blocks: On (both); Off (none); OnlyInfoBlock (top block); OnlyStatusBlock (bottom block).")]
        public InfoBlockMode ShowInfoBlocks { get; set; }
        [Parameter("Show Chart Markers", DefaultValue = ChartMarkerMode.No, Group = "Chart & Display Settings", Description = "Show chart markers for entries/exits: Yes (show); No (hide).")]
        public ChartMarkerMode ShowChartMarkers { get; set; }
        [Parameter("Remove Markers", DefaultValue = RemoveMarkersMode.None, Group = "Chart & Display Settings", Description = "Control which markers are removed: None (keep all); All (remove all); OnlyText (remove text only); OnlyArrows (remove arrows only).")]
        public RemoveMarkersMode RemoveMarkers { get; set; }
        [Parameter("Remove Markers Candles", DefaultValue = 500, MinValue = 1, Group = "Chart & Display Settings", Description = "Number of candles after which chart markers get removed; e.g., 500 candles.")]
        public int RemoveMarkersCandles { get; set; }
        [Parameter("Trade Direction", DefaultValue = TradeDirectionMode.Both, Group = "Trading Setup", Description = "Allowed trade direction: Both (longs and shorts); OnlyLongs (only buys); OnlyShorts (only sells).")]
        public TradeDirectionMode TradeDirection { get; set; }
        [Parameter("Reverse Trading", DefaultValue = ReverseTradingMode.Yes, Group = "Trading Setup", Description = "Reverse trade signals: Yes (long signals become shorts, short signals become longs); No (normal signals).")]
        public ReverseTradingMode ReverseTrading { get; set; }
        [Parameter("Entry Condition", DefaultValue = EntryConditionType.Crossover, Group = "Entry Conditions", Description = "Select entry type: Crossover (price crosses MA3); Breakout (price breaks MA3 high/low); OpenCloseCross (candle opens below/above MA3, closes above/below); Pullback (price pulls back to MA3, then bullish/bearish candle); MA2MA3Crossover (MA3 crosses MA2); MA2ResistanceSupport (price bounces from MA2 after crossing MA3, wick touch or near, no break-through, entry on close after direction change).")]
        public EntryConditionType EntryCondition { get; set; }
        [Parameter("Use MA2/MA3 Spread", DefaultValue = false, Group = "Entry Conditions", Description = "Enable MA2/MA3 spread filter for MA2/MA3 Crossover; requires minimum % spread between MA2 and MA3 within Spread Candles.")]
        public bool UseMa2Ma3Spread { get; set; }
        [Parameter("MA2/MA3 Spread (%)", DefaultValue = 0.1, MinValue = 0.0, Step = 0.01, Group = "Entry Conditions", Description = "Minimum % spread between MA2 and MA3 for MA2/MA3 Crossover entry; e.g., 0.1 requires 0.1% price difference.")]
        public double Ma2Ma3Spread { get; set; }
        [Parameter("Spread Candles", DefaultValue = 2, MinValue = 1, Group = "Entry Conditions", Description = "Number of candles to hold entry condition and check MA2/MA3 spread; e.g., 2 candles to confirm spread meets threshold.")]
        public int SpreadCandles { get; set; }
        [Parameter("Price Distance to MA2 (Pips)", DefaultValue = 5, MinValue = 0, Group = "Entry Conditions", Description = "Maximum distance in pips from price to MA2 for bounce detection in MA2ResistanceSupport; e.g., 5 allows bounce within 5 pips of MA2.")]
        public double PriceDistanceToMa2 { get; set; }
        [Parameter("Consider MA1", DefaultValue = false, Group = "Entry Conditions", Description = "Consider MA1 in entry conditions: Yes (price must be above/below MA1); No (ignore MA1 for Above/Below checks).")]
        public bool ConsiderMa1 { get; set; }
        [Parameter("MA 1 Type", DefaultValue = MovingAverageType.Exponential, Group = "Moving Averages", Description = "Type of slowest MA: Simple (equal weight); Exponential (recent data focus); Weighted (linear weight); Hull (low lag); DoubleExponential (faster response).")]
        public MovingAverageType Ma1Type { get; set; }
        [Parameter("MA 1 Period (150)", DefaultValue = 150, MinValue = 1, Group = "Moving Averages", Description = "Period for slowest MA to detect long-term trends; higher values smooth more, but increase lag.")]
        public int Ma1Period { get; set; }
        [Parameter("Show MA1", DefaultValue = true, Group = "Moving Averages", Description = "Show MA1 line in chart: Yes (show); No (hide).")]
        public bool ShowMa1 { get; set; }
        [Parameter("MA 1 Color", DefaultValue = "Orange", Group = "Moving Averages", Description = "Color for MA1 line in chart; select via color picker (default orange).")]
        public Color Ma1Color { get; set; }
        [Parameter("MA 2 Type", DefaultValue = MovingAverageType.Simple, Group = "Moving Averages", Description = "Type of medium MA; same options as MA1.")]
        public MovingAverageType Ma2Type { get; set; }
        [Parameter("MA 2 Period (55)", DefaultValue = 55, MinValue = 1, Group = "Moving Averages", Description = "Period for medium MA for intermediate trends; e.g., 55 for intraday.")]
        public int Ma2Period { get; set; }
        [Parameter("Show MA2", DefaultValue = true, Group = "Moving Averages", Description = "Show MA2 line in chart: Yes (show); No (hide).")]
        public bool ShowMa2 { get; set; }
        [Parameter("MA 2 Color", DefaultValue = "#ff2da9ff", Group = "Moving Averages", Description = "Color for MA2 line in chart; select via color picker (default darker blue).")]
        public Color Ma2Color { get; set; }
        [Parameter("MA 3 Type", DefaultValue = MovingAverageType.Hull, Group = "Moving Averages", Description = "Type of fastest MA; same options as MA1.")]
        public MovingAverageType Ma3Type { get; set; }
        [Parameter("MA 3 Period (21)", DefaultValue = 21, MinValue = 1, Group = "Moving Averages", Description = "Period for fastest MA for short-term signals; e.g., 21 for quick entries.")]
        public int Ma3Period { get; set; }
        [Parameter("Show MA3", DefaultValue = true, Group = "Moving Averages", Description = "Show MA3 line in chart: Yes (show); No (hide).")]
        public bool ShowMa3 { get; set; }
        [Parameter("MA 3 Color", DefaultValue = "#FF4000FF", Group = "Moving Averages", Description = "Color for MA3 line in chart; select via color picker (default blue-purple).")]
        public Color Ma3Color { get; set; }
        [Parameter("Ma Plot Mode", DefaultValue = MaPlotMode.Full, Group = "Trading Setup", Description = "MA plotting mode: Full (every bar); Optimized (every n. bar for performance); Polyline (continuous line if possible).")]
        public MaPlotMode PlotMode { get; set; }
        [Parameter("Ma Plot Interval", DefaultValue = 1, MinValue = 1, Group = "Trading Setup", Description = "Interval for optimized plotting; 1 = every bar; 5 = every 5th bar.")]
        public int MaPlotInterval { get; set; }
        [Parameter("Use RSI Filter", DefaultValue = true, Group = "RSI Filter", Description = "Enable RSI filter to avoid entries in overbought/oversold markets; requires RSI above buy threshold for buys, below sell threshold for sells.")]
        public bool UseRsiFilter { get; set; }
        [Parameter("RSI Mode", DefaultValue = RsiMode.Normal, Group = "RSI Filter", Description = "RSI mode: Normal (threshold check only); Divergence (requires RSI/price divergence for stronger signals, e.g., higher RSI low with lower price low for buys).")]
        public RsiMode RsiFilterMode { get; set; }
        [Parameter("RSI Period", DefaultValue = 14, MinValue = 1, Group = "RSI Filter", Description = "Periods for RSI calculation; measures momentum over n candles; lower values increase sensitivity; typical: 14.")]
        public int RsiPeriod { get; set; }
        [Parameter("RSI Buy Threshold", DefaultValue = 25, MinValue = 0, MaxValue = 100, Group = "RSI Filter", Description = "Min RSI for buy entries; signals oversold conditions; e.g., 25 allows buys in weak markets; lower values permit more entries.")]
        public double RsiBuyThreshold { get; set; }
        [Parameter("RSI Sell Threshold", DefaultValue = 75, MinValue = 0, MaxValue = 100, Group = "RSI Filter", Description = "Max RSI for sell entries; signals overbought conditions; e.g., 75 restricts sells to strong markets; higher values reduce entries.")]
        public double RsiSellThreshold { get; set; }
        [Parameter("RSI Divergence Lookback", DefaultValue = 5, MinValue = 2, Group = "RSI Filter", Description = "Bars to check for RSI divergence; e.g., 5 compares recent RSI/price lows (buys) or highs (sells) for divergence signals.")]
        public int RsiDivergenceLookback { get; set; }
        [Parameter("Use MA Filter", DefaultValue = false, Group = "Trend & Momentum Filter", Description = "Enable MA1 momentum filter; requires MA1 % change to exceed threshold for entries.")]
        public bool UseMaFilter { get; set; }
        [Parameter("Momentum Mode", DefaultValue = MomentumMode.Static, Group = "Trend & Momentum Filter", Description = "Momentum threshold mode: Static (fixed %); Dynamic (ATR-based for adaptive threshold).")]
        public MomentumMode MaMomentumMode { get; set; }
        [Parameter("MA1 Momentum (%)", DefaultValue = 0.15, MinValue = 0.0, Step = 0.01, Group = "Trend & Momentum Filter", Description = "Min % change in MA1 over MA1 Candles for static mode entries; higher values require stronger trends; e.g., 0.15.")]
        public double Ma1Momentum { get; set; }
        [Parameter("MA1 Momentum ATR Factor", DefaultValue = 0.5, MinValue = 0.0, Step = 0.1, Group = "Trend & Momentum Filter", Description = "ATR multiplier for dynamic momentum threshold; e.g., 0.5 adjusts based on volatility.")]
        public double Ma1MomentumAtrFactor { get; set; }
        [Parameter("MA1 Candles", DefaultValue = 50, MinValue = 1, Group = "Trend & Momentum Filter", Description = "Candles to calculate MA1 % change; longer windows smooth the filter; e.g., 50.")]
        public int Ma1Candles { get; set; }
        [Parameter("Use ATR Filter", DefaultValue = false, Group = "Volatility & Volume Filter", Description = "Enable ATR filter to restrict entries to volatile markets; requires ATR >= threshold.")]
        public bool UseAtrFilter { get; set; }
        [Parameter("ATR Filter Mode", DefaultValue = AtrFilterMode.Threshold, Group = "Volatility & Volume Filter", Description = "ATR mode: Threshold (min ATR); PositionSizing (adjusts volume); MultiTimeframe (uses higher TF ATR).")]
        public AtrFilterMode AtrMode { get; set; }
        [Parameter("ATR Period", DefaultValue = 14, MinValue = 1, Group = "Volatility & Volume Filter", Description = "Periods for ATR calculation; measures volatility over n candles; e.g., 14.")]
        public int AtrPeriod { get; set; }
        [Parameter("ATR Threshold", DefaultValue = 0.005, MinValue = 0.0, Step = 0.0001, Group = "Volatility & Volume Filter", Description = "Min ATR for entries in threshold mode; higher values filter low volatility; e.g., 0.005.")]
        public double AtrThreshold { get; set; }
        [Parameter("ATR Filter Multiplicator", DefaultValue = 1.0, MinValue = 0.1, Step = 0.1, Group = "Volatility & Volume Filter", Description = "Multiplier for ATR threshold; e.g., 1.0 uses ATR as-is, 2.0 doubles the threshold.")]
        public double AtrFilterMultiplicator { get; set; }
        [Parameter("ATR MA Type", DefaultValue = MovingAverageType.Simple, Group = "Volatility & Volume Filter", Description = "MA type for ATR smoothing; same options as MAs; e.g., Hull.")]
        public MovingAverageType AtrMaType { get; set; }
        [Parameter("ATR Position Sizing Factor", DefaultValue = 1.0, MinValue = 0.1, Group = "Volatility & Volume Filter", Description = "Factor to adjust volume in position sizing mode; smaller at high ATR; e.g., 1.0.")]
        public double AtrPositionSizingFactor { get; set; }
        [Parameter("Multi Timeframe for ATR", DefaultValue = "h4", Group = "Volatility & Volume Filter", Description = "Higher timeframe for multi-timeframe ATR; e.g., Hour4 for broader volatility context.")]
        public TimeFrame MultiAtrTimeframe { get; set; }
        [Parameter("Use ADX Filter", DefaultValue = false, Group = "Trend & Momentum Filter", Description = "Enable ADX filter to ensure strong trends; requires ADX > threshold; e.g., true for trend-focused entries.")]
        public bool UseAdxFilter { get; set; }
        [Parameter("ADX Period", DefaultValue = 14, MinValue = 1, Group = "Trend & Momentum Filter", Description = "Periods for ADX calculation; measures trend strength; e.g., 14.")]
        public int AdxPeriod { get; set; }
        [Parameter("ADX Threshold", DefaultValue = 25, MinValue = 0, Group = "Trend & Momentum Filter", Description = "Min ADX for entries; higher values ensure stronger trends; e.g., 25.")]
        public double AdxThreshold { get; set; }
        [Parameter("Regime Lookback Period", DefaultValue = 50, MinValue = 10, Group = "Market Regime", Description = "Number of bars to analyze for market regime detection; e.g., 50.")]
        public int RegimeLookbackPeriod { get; set; }
        [Parameter("Regime ADX Threshold", DefaultValue = 25, MinValue = 0, Group = "Market Regime", Description = "ADX threshold for trending regime detection; e.g., 25.")]
        public double RegimeAdxThreshold { get; set; }
        [Parameter("Regime ATR Trending Multiplier", DefaultValue = 1.2, MinValue = 0.1, Step = 0.1, Group = "Market Regime", Description = "ATR multiplier for trending regime (current ATR > avgATR * multiplier); e.g., 1.2.")]
        public double RegimeAtrTrendingMultiplier { get; set; }
        [Parameter("Regime ATR High Volatility Multiplier", DefaultValue = 1.5, MinValue = 0.1, Step = 0.1, Group = "Market Regime", Description = "ATR multiplier for high volatility regime (current ATR > avgATR * multiplier); e.g., 1.5.")]
        public double RegimeAtrHighVolMultiplier { get; set; }
        [Parameter("Regime ATR Low Volatility Multiplier", DefaultValue = 0.8, MinValue = 0.1, Step = 0.1, Group = "Market Regime", Description = "ATR multiplier for low volatility regime (current ATR < avgATR * multiplier); e.g., 0.8.")]
        public double RegimeAtrLowVolMultiplier { get; set; }
        [Parameter("Use Volume Filter", DefaultValue = false, Group = "Volatility & Volume Filter", Description = "Enable volume filter for liquidity; requires volume >= min and <= max (static or dynamic).")]
        public bool UseVolumeFilter { get; set; }
        [Parameter("Volume Mode", DefaultValue = VolumeFilterMode.Static, Group = "Volatility & Volume Filter", Description = "Volume filter mode: Static (fixed min/max volume); Dynamic (adjusts min/max based on daily average).")]
        public VolumeFilterMode VolumeMode { get; set; }
        [Parameter("Min Volume", DefaultValue = 200, MinValue = 0, Group = "Volatility & Volume Filter", Description = "Min tick volume for static mode entries; higher values filter low-liquidity; e.g., 1000000.")]
        public int MinVolume { get; set; }
        [Parameter("Max Volume", DefaultValue = 2000, MinValue = 0, Group = "Volatility & Volume Filter", Description = "Max tick volume for static mode entries; lower values filter high-volatility spikes; e.g., 10000000.")]
        public int MaxVolume { get; set; }
        [Parameter("Volume Dynamic Factor", DefaultValue = 0.8, MinValue = 0.1, Step = 0.1, Group = "Volatility & Volume Filter", Description = "Factor for dynamic min volume threshold; multiplies avg daily volume; e.g., 0.8 for 80% of daily average.")]
        public double VolumeDynamicFactor { get; set; }
        [Parameter("Volume Dynamic Max Factor", DefaultValue = 1.5, MinValue = 0.1, Step = 0.1, Group = "Volatility & Volume Filter", Description = "Factor for dynamic max volume threshold; multiplies avg daily volume; e.g., 1.5 for 150% of daily average.")]
        public double VolumeDynamicMaxFactor { get; set; }
        [Parameter("Use Spread Filter", DefaultValue = false, Group = "Volatility & Volume Filter", Description = "Enable spread filter for execution quality; requires spread <= max; e.g., false to allow all spreads.")]
        public bool UseSpreadFilter { get; set; }
        [Parameter("Max Spread (Pips)", DefaultValue = 5.0, MinValue = 0.0, Group = "Volatility & Volume Filter", Description = "Max spread in pips for entries; lower values avoid high costs; e.g., 5.0.")]
        public double MaxSpreadPips { get; set; }
        [Parameter("Risk Management Mode", DefaultValue = RiskManagementMode.Static, Group = "Risk Management", Description = "Mode for SL/TP/TS: Static (fixed pips); DynamicATR (ATR-multiplied for volatility adaptation).")]
        public RiskManagementMode RiskMode { get; set; }
        [Parameter("Risk Percentage", DefaultValue = 0.5, MinValue = 0.1, MaxValue = 10.0, Group = "Risk Management", Description = "Balance % to risk per trade; used for volume calculation based on SL; e.g., 0.5.")]
        public double RiskPercentage { get; set; }
        [Parameter("ATR Multiplier Risk", DefaultValue = 1.0, MinValue = 0.1, Step = 0.1, Group = "Risk Management", Description = "ATR multiplier for risk calculation in dynamic mode; risk amount = ATR * multiplier; e.g., 1.0 uses ATR as risk amount.")]
        public double AtrMultiplierRisk { get; set; }
        [Parameter("Stop Loss (Pips)", DefaultValue = 5000, MinValue = 1, Group = "Risk Management", Description = "Fixed SL in pips for static mode; e.g., 50.")]
        public int StopLossPips { get; set; }
        [Parameter("Take Profit (Pips)", DefaultValue = 10000, MinValue = 1, Group = "Risk Management", Description = "Fixed TP in pips for static mode; e.g., 100.")]
        public int TakeProfitPips { get; set; }
        [Parameter("ATR Multiplier SL", DefaultValue = 2.0, MinValue = 0.1, Group = "Risk Management", Description = "ATR multiplier for dynamic SL; higher increases SL distance; e.g., 2.0.")]
        public double AtrMultiplierSl { get; set; }
        [Parameter("ATR Multiplier TP", DefaultValue = 3.0, MinValue = 0.1, Group = "Risk Management", Description = "ATR multiplier for dynamic TP; higher increases TP distance; e.g., 3.0.")]
        public double AtrMultiplierTp { get; set; }
        [Parameter("Trailing Stop", DefaultValue = TrailingStopMode.TSRemovesTP, Group = "Risk Management", Description = "Trailing stop mode: Off (disabled); TSRemovesTP (removes TP after TS activates); TSPlusTP (keeps TP while TS is active).")]
        public TrailingStopMode TrailingStop { get; set; }
        [Parameter("Trailing Stop Distance (Pips)", DefaultValue = 15, MinValue = 1, Group = "Risk Management", Description = "Fixed TS distance in pips for static mode; e.g., 15.")]
        public int TrailingStopPips { get; set; }
        [Parameter("Trailing Stop Trigger (Pips)", DefaultValue = 50, MinValue = 1, Group = "Risk Management", Description = "Profit in pips to start TS in static mode; e.g., 50.")]
        public int TrailingStopTriggerPips { get; set; }
        [Parameter("ATR Multiplier TS Distance", DefaultValue = 1.5, MinValue = 0.1, Group = "Risk Management", Description = "ATR multiplier for dynamic TS distance; e.g., 1.5.")]
        public double AtrMultiplierTsDistance { get; set; }
        [Parameter("ATR Multiplier TS Trigger", DefaultValue = 2.0, MinValue = 0.1, Group = "Risk Management", Description = "ATR multiplier for dynamic TS trigger; e.g., 2.0.")]
        public double AtrMultiplierTsTrigger { get; set; }
        [Parameter("Max Consecutive Losses", DefaultValue = 3, MinValue = 1, Group = "Trade Protection", Description = "Max losing trades in a row before cooldown; e.g., 3.")]
        public int MaxConsecutiveLosses { get; set; }
        [Parameter("Block Candles", DefaultValue = 50, MinValue = 1, Group = "Trade Protection", Description = "Candles to block trades after max losses; e.g., 50.")]
        public int BlockCandles { get; set; }
        [Parameter("Use Time Limits", DefaultValue = TimeLimitMode.No, Group = "Trade Protection", Description = "Time limit mode: Yes (soft/hard); No (none); OnlySoft (close profitable after soft limit); OnlyHard (close all after hard limit).")]
        public TimeLimitMode UseTimeLimits { get; set; }
        [Parameter("Time Limit Soft", DefaultValue = 24, MinValue = 1, Group = "Trade Protection", Description = "Hours to close profitable positions; e.g., 12.")]
        public double TimeLimitSoft { get; set; }
        [Parameter("Time Limit Hard", DefaultValue = 48.0, MinValue = 1, Group = "Trade Protection", Description = "Hours to close all positions; e.g., 48.")]
        public double TimeLimitHard { get; set; }
        [Parameter("Avoid Swaps", DefaultValue = false, Group = "Trade Protection", Description = "Close profitable positions 1 hour before swap to avoid fees; e.g., true.")]
        public bool AvoidSwaps { get; set; }
        [Parameter("Use Weekend Filter", DefaultValue = false, Group = "Trade Protection", Description = "Block trades near weekends to avoid gaps; e.g., false for 24/7 crypto trading.")]
        public bool UseWeekendFilter { get; set; }
        [Parameter("Hours Before Weekend", DefaultValue = 1, MinValue = 0, Group = "Trade Protection", Description = "Hours before Friday 22:00 UTC to block trades; e.g., 0 for crypto.")]
        public double HoursBeforeWeekend { get; set; }

        [Parameter("Hours After Weekend", DefaultValue = 2, MinValue = 0, Group = "Trade Protection", Description = "Hours after Sunday 22:00 UTC to block trades; e.g., 2.")]

        public double HoursAfterWeekend { get; set; }

        [Parameter("Rolling Stats Period", DefaultValue = 20, MinValue = 1, Group = "Rolling Statistics", Description = "Period for rolling statistics calculation; e.g., 20.")]

        public int RollingStatsPeriod { get; set; }

        [Parameter("Recent Trades Lookback", DefaultValue = 20, MinValue = 1, Group = "Adaptive Parameters", Description = "Number of recent trades to analyze for performance adaptation; e.g., 20.")]
        public int RecentTradesLookback { get; set; }

        [Parameter("Adjustment Factor", DefaultValue = 0.1, MinValue = 0.0, Step = 0.01, Group = "Adaptive Parameters", Description = "Factor for parameter adjustments (not directly used in logic); e.g., 0.1.")]
        public double AdjustmentFactor { get; set; }

        [Parameter("Enable Learning Features", DefaultValue = false, Group = "Adaptive Learning", Description = "Enable adaptive parameter adjustment based on recent trading performance. When enabled, the bot will automatically tighten/loosen ATR and RSI thresholds based on win rate analysis to optimize entry conditions.")]
        public bool EnableLearningFeatures { get; set; }

        [Parameter("Enable Regime Detection", DefaultValue = false, Group = "Adaptive Learning", Description = "Enable market regime detection using ATR and ADX indicators. When enabled, the bot identifies Trending, Ranging, High Volatility, or Low Volatility conditions and displays the current regime in the info panel.")]
        public bool EnableRegimeDetection { get; set; }

        [Parameter("Enable Rolling Statistics", DefaultValue = false, Group = "Adaptive Learning", Description = "Enable rolling statistics calculation for ATR, volume, and ADX. When enabled, these smoothed metrics improve regime detection accuracy and provide more stable market analysis.")]
        public bool EnableRollingStats { get; set; }

        [Parameter("Max Trade History Size", DefaultValue = 100, MinValue = 100, MaxValue = 10000, Group = "Trade Learning", Description = "Maximum number of historical trades to store for learning analysis. Prevents memory issues by limiting history size.")]
        public int MaxTradeHistorySize { get; set; }

        [Parameter("Rolling Analysis Window 1", DefaultValue = 20, MinValue = 5, MaxValue = 500, Group = "Rolling Performance", Description = "First rolling analysis window size in trades for performance metrics calculation.")]
        public int RollingWindow1 { get; set; }

        [Parameter("Rolling Analysis Window 2", DefaultValue = 50, MinValue = 5, MaxValue = 500, Group = "Rolling Performance", Description = "Second rolling analysis window size in trades for performance metrics calculation.")]
        public int RollingWindow2 { get; set; }

        [Parameter("Rolling Analysis Window 3", DefaultValue = 100, MinValue = 5, MaxValue = 500, Group = "Rolling Performance", Description = "Third rolling analysis window size in trades for performance metrics calculation.")]
        public int RollingWindow3 { get; set; }

        [Parameter("Performance Update Frequency", DefaultValue = 10, MinValue = 1, MaxValue = 100, Group = "Rolling Performance", Description = "Number of ticks between performance metric updates to balance accuracy and performance.")]
        public int PerformanceUpdateFrequency { get; set; }

        [Parameter("Enable Rolling Performance Analysis", DefaultValue = false, Group = "Rolling Performance", Description = "Enable advanced rolling performance analysis including Sharpe ratio, drawdown, profit factor, and risk-adjusted metrics.")]
        public bool EnableRollingPerformance { get; set; }

        [Parameter("Enable Correlation Analysis", DefaultValue = false, Group = "Parameter Analysis", Description = "Enable parameter correlation analysis to identify relationships between bot parameters and trading performance.")]
        public bool EnableCorrelationAnalysis { get; set; }

        [Parameter("Correlation Analysis Window Size", DefaultValue = 50, MinValue = 10, MaxValue = 500, Group = "Parameter Analysis", Description = "Number of recent trades to analyze for parameter correlation calculations.")]
        public int CorrelationAnalysisWindowSize { get; set; }

        [Parameter("Minimum Sample Requirements", DefaultValue = 20, MinValue = 5, MaxValue = 100, Group = "Parameter Analysis", Description = "Minimum number of trades required before correlation analysis begins.")]
        public int MinimumSampleRequirements { get; set; }

        [Parameter("ATR Threshold Min", DefaultValue = 0.0005, MinValue = 0.0001, MaxValue = 0.01, Step = 0.0001, Group = "Parameter Bounds", Description = "Minimum allowed ATR threshold value to prevent extreme restrictions.")]
        public double ATRThresholdMin { get; set; }

        [Parameter("ATR Threshold Max", DefaultValue = 0.05, MinValue = 0.01, MaxValue = 0.1, Step = 0.001, Group = "Parameter Bounds", Description = "Maximum allowed ATR threshold value for flexible volatility requirements.")]
        public double ATRThresholdMax { get; set; }

        [Parameter("RSI Buy Threshold Min", DefaultValue = 20.0, MinValue = 10, MaxValue = 30, Group = "Parameter Bounds", Description = "Minimum allowed RSI buy threshold for oversold conditions.")]
        public double RSIBuyThresholdMin { get; set; }

        [Parameter("RSI Buy Threshold Max", DefaultValue = 40.0, MinValue = 30, MaxValue = 50, Group = "Parameter Bounds", Description = "Maximum allowed RSI buy threshold for flexible entry conditions.")]
        public double RSIBuyThresholdMax { get; set; }

        [Parameter("RSI Sell Threshold Min", DefaultValue = 60.0, MinValue = 50, MaxValue = 70, Group = "Parameter Bounds", Description = "Minimum allowed RSI sell threshold for overbought conditions.")]
        public double RSISellThresholdMin { get; set; }

        [Parameter("RSI Sell Threshold Max", DefaultValue = 80.0, MinValue = 70, MaxValue = 90, Group = "Parameter Bounds", Description = "Maximum allowed RSI sell threshold for flexible exit conditions.")]
        public double RSISellThresholdMax { get; set; }

        [Parameter("Momentum Threshold Min", DefaultValue = 0.01, MinValue = 0.001, MaxValue = 0.1, Step = 0.001, Group = "Parameter Bounds", Description = "Minimum allowed momentum threshold for trend detection.")]
        public double MomentumThresholdMin { get; set; }

        [Parameter("Momentum Threshold Max", DefaultValue = 1.0, MinValue = 0.5, MaxValue = 2.0, Step = 0.1, Group = "Parameter Bounds", Description = "Maximum allowed momentum threshold for flexible trend requirements.")]
        public double MomentumThresholdMax { get; set; }

        [Parameter("Adaptive Sample Size", DefaultValue = false, Group = "Parameter Analysis", Description = "Enable adaptive sample size calculation based on market volatility for more reliable statistical analysis.")]
        public bool AdaptiveSampleSize { get; set; }

        [Parameter("Optimization Correlation Threshold", DefaultValue = 0.3, MinValue = 0.1, MaxValue = 0.8, Step = 0.05, Group = "Parameter Analysis", Description = "Minimum correlation strength required for optimization suggestions (0.1-0.8).")]
        public double OptimizationCorrelationThreshold { get; set; }
        [Parameter("Max Cache Size", DefaultValue = 1000, MinValue = 100, MaxValue = 10000, Group = "Performance & Execution", Description = "Maximum number of entries per cache to prevent memory bloat. Lower values reduce memory usage but may impact performance.")]
        public int MaxCacheSize { get; set; }
        [Parameter("Logging Level", DefaultValue = LoggingLevel.OnlyCritical, Group = "Trading Setup", Description = "Control verbosity of console logging: Off (no logging); Full (all messages); Info (info messages); Debug (debug messages); OnlyImportant (important events); Warning (warnings and above); Error (errors and above); OnlyCritical (critical errors only); OnlyTrades (trade-related messages only).")]
        public LoggingLevel LogLevel { get; set; }
        [Parameter("Use Confirmation Candles", DefaultValue = false, Group = "Entry Conditions", Description = "Require consecutive bullish (long) or bearish (short) candles to confirm entries; reduces false signals; e.g., true.")]
        public bool UseConfirmationCandles { get; set; }
        [Parameter("Confirmation Candles", DefaultValue = 1, MinValue = 1, Group = "Entry Conditions", Description = "Number of confirming bullish/bearish candles; higher values for stricter validation; e.g., 1.")]
        public int ConfirmationCandles { get; set; }
        [Parameter("Use News Avoid", DefaultValue = false, Group = "Entry Conditions", Description = "Avoid trades during major news events (e.g., NFP, ECB rates); e.g., true to block trades during predefined news windows.")]
        public bool UseNewsAvoid { get; set; }
        [Parameter("Use Time Look-Ahead", DefaultValue = false, Group = "Entry Conditions", Description = "Require trades during high-volatility session start times (e.g., London open); e.g., true to restrict to volatile periods.")]
        public bool UseTimeLookAhead { get; set; }
        [Parameter("Execution Mode", DefaultValue = ExecutionMode.OnTick, Group = "Performance & Execution", Description = "Execution mode for the bot: OnTick (executes on every tick); OnBar (executes on bar close); Hybrid (combines both for optimal performance).")]
        public ExecutionMode BotExecutionMode { get; set; }
        [Parameter("Chart Update Frequency", DefaultValue = 10, MinValue = 1, MaxValue = 100, Group = "Performance & Execution", Description = "Frequency of chart updates in ticks (higher values reduce redraw overhead but may delay visual updates).")]
        public int ChartUpdateFrequency { get; set; }
        [Parameter("Enable Chart Batching", DefaultValue = true, Group = "Performance & Execution", Description = "Enable batching of chart operations to reduce redraw overhead.")]
        public bool EnableChartBatching { get; set; }
        [Parameter("Chart Update Batch Size", DefaultValue = 10, MinValue = 1, MaxValue = 100, Group = "Performance & Execution", Description = "Number of chart objects to accumulate before batch drawing. Higher values reduce redraw frequency but may delay visual updates.")]
        public int ChartUpdateBatchSize { get; set; }
        [Parameter("Analysis Frequency (Bars)", DefaultValue = 1, MinValue = 1, MaxValue = 100, Group = "Parameter Analysis", Description = "Number of bars between analysis updates. Higher values reduce computational load but may delay analysis. Default: 1 (every bar).")]
        public int AnalysisFrequencyBars { get; set; }
        [Parameter("Correlation Data Sampling Rate", DefaultValue = 1, MinValue = 1, MaxValue = 10, Group = "Parameter Analysis", Description = "Sampling rate for correlation data (1 = every data point, 2 = every 2nd point, etc.). Higher values reduce data points for better performance.")]
        public int CorrelationSamplingRate { get; set; }
        private IndicatorDataSeries ma1;
        private IndicatorDataSeries ma2;
        private IndicatorDataSeries ma3;
        private RelativeStrengthIndex rsi;
        private IndicatorDataSeries atr;
        private Bars analysisBars;
        private DirectionalMovementSystem adx;
        private IndicatorDataSeries multiAtr;
        private Bars multiBars;
        private RollingStatistics rollingATR;
        private RollingStatistics rollingVolume;
        private RollingStatistics rollingADX;
        private int tickCounter = 0;
        private int checkForEntriesCallCount = 0;
        private int lastEntryBarIndex = -1;
        private int performanceUpdateCounter = 0;
        private int chartUpdateCounter = 0;
        private bool isNewBar = false;
        private DateTime lastBarTime;

        // Chart batching system fields
        private List<ChartObjectBatch> chartObjectBatch = new List<ChartObjectBatch>();
        private int currentBatchSize = 0;
        private long chartUpdateTime = 0;
        private int chartUpdateCount = 0;

        // Caching fields for expensive computations with LRU eviction
        private LRUCache<int, double> atrCache;
        private LRUCache<int, double> ma1Cache;
        private LRUCache<int, double> ma2Cache;
        private LRUCache<int, double> ma3Cache;
        private LRUCache<int, double> ma2Ma3SpreadCache;
        private LRUCache<int, double> momentumThresholdCache;
        private LRUCache<string, LRUCache<int, double>> correlationCache;
        private LRUCache<int, double> sharpeRatioCache;
        private int lastCachedBarIndex = -1;

        // Rolling Performance Analysis fields
        private List<double> rollingReturns1 = new List<double>();
        private List<double> rollingReturns2 = new List<double>();
        private List<double> rollingReturns3 = new List<double>();
        private double rollingSharpe1 = 0;
        private double rollingSharpe2 = 0;
        private double rollingSharpe3 = 0;
        private double rollingMaxDrawdown1 = 0;
        private double rollingMaxDrawdown2 = 0;
        private double rollingMaxDrawdown3 = 0;
        private double rollingProfitFactor1 = 0;
        private double rollingProfitFactor2 = 0;
        private double rollingProfitFactor3 = 0;
        private double rollingWinRate1 = 0;
        private double rollingWinRate2 = 0;
        private double rollingWinRate3 = 0;
        private double rollingSortino1 = 0;
        private double rollingSortino2 = 0;
        private double rollingSortino3 = 0;
        private double rollingCalmar1 = 0;
        private double rollingCalmar2 = 0;
        private double rollingCalmar3 = 0;
        private string performanceTrend = "Stable";
        private Dictionary<MarketRegime, List<double>> regimeReturns = new Dictionary<MarketRegime, List<double>>();
        private Dictionary<MarketRegime, double> regimeSharpe = new Dictionary<MarketRegime, double>();
        private Dictionary<MarketRegime, double> regimeWinRate = new Dictionary<MarketRegime, double>();
        private int consecutiveTPs;
        private int consecutiveSLs;
        private bool lastTradeWasTP;
        private bool lastTradeWasSL;
        private int totalTrades;
        private int winningTrades;
        private int losingTrades;
        private double totalProfit;
        private double totalLoss;
        private double totalPips;
        private bool isTrailingStopActive;
        private int lastLossBarIndex;
        private bool isTradeBlocked;
        private int consecutiveConfirmationCandles;
        private bool isLongSignalActive;
        private bool isShortSignalActive;
        private int spreadCandleCount;
        private bool isSpreadLongSignalActive;
        private bool isSpreadShortSignalActive;
        private MarketRegime currentRegime;

        // Pre-computed condition variables for performance optimization
        private bool preComputedIsTradeBlocked;
        private bool preComputedIsWeekendBlocked;
        private bool preComputedIsTradingAllowed;
        private bool preComputedIsNewsEvent;
        private double preComputedMa1Value;
        private double preComputedMa2Value;
        private double preComputedMa3Value;
        private bool preComputedIsBullishCandle;
        private bool preComputedIsBearishCandle;
        private bool preComputedIsAboveAllMas;
        private bool preComputedIsBelowAllMas;
        private double preComputedMa1ChangePercent;
        private bool preComputedHasSufficientMomentum;
        private bool preComputedHasSufficientVolatility;
        private bool preComputedHasStrongTrend;
        private bool preComputedHasSufficientVolume;
        private bool preComputedHasAcceptableSpread;
        private bool preComputedRsiConditionBuy;
        private bool preComputedRsiConditionSell;
        private bool preComputedHasSufficientMa2Ma3Spread;
        private bool preComputedLongEntryConditions;
        private bool preComputedShortEntryConditions;
        private int preComputedLastBarIndex;

        // Caching for complex boolean expressions
        private Dictionary<string, bool> conditionCache;
        private Dictionary<string, DateTime> conditionCacheTimestamps;
        private long conditionCacheHits;
        private long conditionCacheMisses;

        private DoubleExponentialMovingAverage dema1;
        private DoubleExponentialMovingAverage dema2;
        private DoubleExponentialMovingAverage dema3;
        private CustomHullAtr customHullAtr;
        private CustomDoubleExponentialAtr customDemaAtr;
        private List<double> tradeReturns = new List<double>();
        private List<double> recentProfits = new List<double>();

        // Trade Learning Data Structure
        internal class TradeLearningData
        {
            // Trade metadata
            public DateTime EntryTime { get; set; }
            public DateTime ExitTime { get; set; }
            public double EntryPrice { get; set; }
            public double ExitPrice { get; set; }
            public TimeSpan HoldingTime { get; set; }
            public double ProfitLoss { get; set; }

            // Market conditions at entry
            public double AtrAtEntry { get; set; }
            public double AdxAtEntry { get; set; }
            public double RsiAtEntry { get; set; }
            public double VolumeAtEntry { get; set; }
            public double SpreadAtEntry { get; set; }

            // Bot parameters at time of trade
            public double AtrThresholdAtEntry { get; set; }
            public double RsiBuyThresholdAtEntry { get; set; }
            public double RsiSellThresholdAtEntry { get; set; }
            public double MomentumThresholdAtEntry { get; set; }

            // Market regime and trend information
            public MarketRegime RegimeAtEntry { get; set; }
            public string TrendDirectionAtEntry { get; set; }

            // Performance metrics
            public bool IsWin { get; set; }
            public double Pips { get; set; }
            public double ReturnPercentage { get; set; }
        }

        private List<TradeLearningData> tradeHistory = new List<TradeLearningData>();

        // Parameter Validation System
        internal class ParameterValidator
        {
            private readonly AdaptiveMABot_v6_514_crypto _bot;
            private readonly List<string> _validationErrors;
            private readonly List<string> _validationWarnings;

            public ParameterValidator(AdaptiveMABot_v6_514_crypto bot)
            {
                _bot = bot;
                _validationErrors = new List<string>();
                _validationWarnings = new List<string>();
            }

            public bool ValidateAllParameters()
            {
                _validationErrors.Clear();
                _validationWarnings.Clear();

                ValidateMovingAverageParameters();
                ValidateRSIParameters();
                ValidateATRParameters();
                ValidateRiskManagementParameters();
                ValidateGeneralParameters();
                ValidateParameterBounds();

                // Log validation results
                if (_validationErrors.Count > 0)
                {
                    _bot.LogMessage("=== PARAMETER VALIDATION ERRORS ===", LoggingLevel.OnlyCritical);
                    foreach (var error in _validationErrors)
                    {
                        _bot.LogMessage($"ERROR: {error}", LoggingLevel.OnlyCritical);
                    }
                    _bot.LogMessage("===================================", LoggingLevel.OnlyCritical);
                    return false;
                }

                if (_validationWarnings.Count > 0)
                {
                    _bot.LogMessage("=== PARAMETER VALIDATION WARNINGS ===", LoggingLevel.OnlyImportant);
                    foreach (var warning in _validationWarnings)
                    {
                        _bot.LogMessage($"WARNING: {warning}", LoggingLevel.OnlyImportant);
                    }
                    _bot.LogMessage("=====================================", LoggingLevel.OnlyImportant);
                }

                _bot.LogMessage("✓ All parameters validated successfully", LoggingLevel.OnlyImportant);
                return true;
            }

            private void ValidateMovingAverageParameters()
            {
                // MA Periods validation
                if (_bot.Ma1Period <= 0)
                    _validationErrors.Add("MA 1 Period must be greater than 0");
                else if (_bot.Ma1Period < 5)
                    _validationWarnings.Add("MA 1 Period is very low (< 5), may cause excessive signals");

                if (_bot.Ma2Period <= 0)
                    _validationErrors.Add("MA 2 Period must be greater than 0");
                else if (_bot.Ma2Period < 5)
                    _validationWarnings.Add("MA 2 Period is very low (< 5), may cause excessive signals");

                if (_bot.Ma3Period <= 0)
                    _validationErrors.Add("MA 3 Period must be greater than 0");
                else if (_bot.Ma3Period < 5)
                    _validationWarnings.Add("MA 3 Period is very low (< 5), may cause excessive signals");

                // MA Period relationships
                if (_bot.Ma1Period <= _bot.Ma2Period)
                    _validationErrors.Add("MA 1 Period must be greater than MA 2 Period for proper trend detection");

                if (_bot.Ma2Period <= _bot.Ma3Period)
                    _validationErrors.Add("MA 2 Period must be greater than MA 3 Period for proper trend detection");

                // MA Candles validation
                if (_bot.Ma1Candles <= 0)
                    _validationErrors.Add("MA1 Candles must be greater than 0");
                else if (_bot.Ma1Candles < 10)
                    _validationWarnings.Add("MA1 Candles is very low (< 10), momentum calculation may be unstable");
            }

            private void ValidateRSIParameters()
            {
                if (_bot.RsiPeriod <= 0)
                    _validationErrors.Add("RSI Period must be greater than 0");
                else if (_bot.RsiPeriod < 2)
                    _validationWarnings.Add("RSI Period is very low (< 2), may cause unreliable signals");
                else if (_bot.RsiPeriod > 50)
                    _validationWarnings.Add("RSI Period is very high (> 50), may be too slow to react");

                if (_bot.RsiBuyThreshold < 0 || _bot.RsiBuyThreshold > 100)
                    _validationErrors.Add("RSI Buy Threshold must be between 0 and 100");
                else if (_bot.RsiBuyThreshold > 50)
                    _validationWarnings.Add("RSI Buy Threshold is high (> 50), may miss oversold conditions");

                if (_bot.RsiSellThreshold < 0 || _bot.RsiSellThreshold > 100)
                    _validationErrors.Add("RSI Sell Threshold must be between 0 and 100");
                else if (_bot.RsiSellThreshold < 50)
                    _validationWarnings.Add("RSI Sell Threshold is low (< 50), may miss overbought conditions");

                if (_bot.RsiBuyThreshold >= _bot.RsiSellThreshold)
                    _validationErrors.Add("RSI Buy Threshold must be less than RSI Sell Threshold");

                if (_bot.RsiDivergenceLookback < 2)
                    _validationErrors.Add("RSI Divergence Lookback must be at least 2");
                else if (_bot.RsiDivergenceLookback > 20)
                    _validationWarnings.Add("RSI Divergence Lookback is high (> 20), may be too slow");
            }

            private void ValidateATRParameters()
            {
                if (_bot.AtrPeriod <= 0)
                    _validationErrors.Add("ATR Period must be greater than 0");
                else if (_bot.AtrPeriod < 5)
                    _validationWarnings.Add("ATR Period is low (< 5), volatility calculation may be unstable");

                if (_bot.AtrThreshold <= 0)
                    _validationErrors.Add("ATR Threshold must be greater than 0");
                else if (_bot.AtrThreshold > 0.1)
                    _validationWarnings.Add("ATR Threshold is very high (> 0.1), may filter out most trades");

                if (_bot.AtrFilterMultiplicator <= 0)
                    _validationErrors.Add("ATR Filter Multiplicator must be greater than 0");
                else if (_bot.AtrFilterMultiplicator > 5)
                    _validationWarnings.Add("ATR Filter Multiplicator is very high (> 5), may be too restrictive");

                if (_bot.AtrPositionSizingFactor <= 0)
                    _validationErrors.Add("ATR Position Sizing Factor must be greater than 0");
                else if (_bot.AtrPositionSizingFactor > 2)
                    _validationWarnings.Add("ATR Position Sizing Factor is high (> 2), position sizes may be too small");

                if (_bot.AtrMultiplierSl <= 0)
                    _validationErrors.Add("ATR Multiplier SL must be greater than 0");
                else if (_bot.AtrMultiplierSl > 5)
                    _validationWarnings.Add("ATR Multiplier SL is very high (> 5), stop losses may be too wide");

                if (_bot.AtrMultiplierTp <= 0)
                    _validationErrors.Add("ATR Multiplier TP must be greater than 0");
                else if (_bot.AtrMultiplierTp < 1)
                    _validationWarnings.Add("ATR Multiplier TP is low (< 1), take profits may be too close");

                if (_bot.AtrMultiplierTsDistance <= 0)
                    _validationErrors.Add("ATR Multiplier TS Distance must be greater than 0");

                if (_bot.AtrMultiplierTsTrigger <= 0)
                    _validationErrors.Add("ATR Multiplier TS Trigger must be greater than 0");
            }

            private void ValidateRiskManagementParameters()
            {
                if (_bot.RiskPercentage <= 0 || _bot.RiskPercentage > 10)
                    _validationErrors.Add("Risk Percentage must be between 0.1 and 10.0");

                if (_bot.StopLossPips <= 0)
                    _validationErrors.Add("Stop Loss (Pips) must be greater than 0");
                else if (_bot.StopLossPips > 500)
                    _validationWarnings.Add("Stop Loss (Pips) is very high (> 500), risk management may be ineffective");

                if (_bot.TakeProfitPips <= 0)
                    _validationErrors.Add("Take Profit (Pips) must be greater than 0");
                else if (_bot.TakeProfitPips < _bot.StopLossPips)
                    _validationWarnings.Add("Take Profit (Pips) is less than Stop Loss, risk-reward ratio is negative");

                if (_bot.TrailingStopPips <= 0)
                    _validationErrors.Add("Trailing Stop Distance (Pips) must be greater than 0");

                if (_bot.TrailingStopTriggerPips <= 0)
                    _validationErrors.Add("Trailing Stop Trigger (Pips) must be greater than 0");

                if (_bot.MaxConsecutiveLosses <= 0)
                    _validationErrors.Add("Max Consecutive Losses must be greater than 0");
                else if (_bot.MaxConsecutiveLosses > 10)
                    _validationWarnings.Add("Max Consecutive Losses is high (> 10), may allow excessive drawdown");

                if (_bot.BlockCandles <= 0)
                    _validationErrors.Add("Block Candles must be greater than 0");
                else if (_bot.BlockCandles > 1000)
                    _validationWarnings.Add("Block Candles is very high (> 1000), recovery time may be too long");
            }

            private void ValidateGeneralParameters()
            {
                if (_bot.HistoricalBarsToLoad < 1000)
                    _validationErrors.Add("Historical Bars To Load must be at least 1000");
                else if (_bot.HistoricalBarsToLoad > 20000)
                    _validationWarnings.Add("Historical Bars To Load is very high (> 20000), may impact performance");

                if (_bot.RemoveMarkersCandles <= 0)
                    _validationErrors.Add("Remove Markers Candles must be greater than 0");

                if (_bot.ConfirmationCandles <= 0)
                    _validationErrors.Add("Confirmation Candles must be greater than 0");
                else if (_bot.ConfirmationCandles > 10)
                    _validationWarnings.Add("Confirmation Candles is high (> 10), may miss trading opportunities");

                if (_bot.SpreadCandles <= 0)
                    _validationErrors.Add("Spread Candles must be greater than 0");


                if (_bot.TimeLimitSoft <= 0)
                    _validationErrors.Add("Time Limit Soft must be greater than 0");

                if (_bot.TimeLimitHard <= 0)
                    _validationErrors.Add("Time Limit Hard must be greater than 0");

                if (_bot.TimeLimitSoft >= _bot.TimeLimitHard)
                    _validationErrors.Add("Time Limit Soft must be less than Time Limit Hard");

                if (_bot.HoursBeforeWeekend < 0)
                    _validationErrors.Add("Hours Before Weekend cannot be negative");

                if (_bot.HoursAfterWeekend < 0)
                    _validationErrors.Add("Hours After Weekend cannot be negative");

                if (_bot.RollingStatsPeriod <= 0)
                    _validationErrors.Add("Rolling Stats Period must be greater than 0");

                if (_bot.RecentTradesLookback <= 0)
                    _validationErrors.Add("Recent Trades Lookback must be greater than 0");

                if (_bot.AdjustmentFactor < 0)
                    _validationErrors.Add("Adjustment Factor cannot be negative");

                if (_bot.MaxTradeHistorySize < 100)
                    _validationErrors.Add("Max Trade History Size must be at least 100");

                if (_bot.RollingWindow1 <= 0 || _bot.RollingWindow1 > 500)
                    _validationErrors.Add("Rolling Analysis Window 1 must be between 1 and 500");

                if (_bot.RollingWindow2 <= 0 || _bot.RollingWindow2 > 500)
                    _validationErrors.Add("Rolling Analysis Window 2 must be between 1 and 500");

                if (_bot.RollingWindow3 <= 0 || _bot.RollingWindow3 > 500)
                    _validationErrors.Add("Rolling Analysis Window 3 must be between 1 and 500");

                if (_bot.PerformanceUpdateFrequency <= 0)
                    _validationErrors.Add("Performance Update Frequency must be greater than 0");

                if (_bot.CorrelationAnalysisWindowSize < 10)
                    _validationErrors.Add("Correlation Analysis Window Size must be at least 10");

                if (_bot.MinimumSampleRequirements < 5)
                    _validationErrors.Add("Minimum Sample Requirements must be at least 5");

                if (_bot.OptimizationCorrelationThreshold < 0.1 || _bot.OptimizationCorrelationThreshold > 0.8)
                    _validationErrors.Add("Optimization Correlation Threshold must be between 0.1 and 0.8");
            }

            private void ValidateParameterBounds()
            {
                if (_bot.ATRThresholdMin <= 0)
                    _validationErrors.Add("ATR Threshold Min must be greater than 0");

                if (_bot.ATRThresholdMax <= _bot.ATRThresholdMin)
                    _validationErrors.Add("ATR Threshold Max must be greater than ATR Threshold Min");

                if (_bot.AtrThreshold < _bot.ATRThresholdMin || _bot.AtrThreshold > _bot.ATRThresholdMax)
                    _validationErrors.Add($"ATR Threshold ({_bot.AtrThreshold}) must be between ATR Threshold Min ({_bot.ATRThresholdMin}) and Max ({_bot.ATRThresholdMax})");

                if (_bot.RSIBuyThresholdMin < 0 || _bot.RSIBuyThresholdMin > 100)
                    _validationErrors.Add("RSI Buy Threshold Min must be between 0 and 100");

                if (_bot.RSIBuyThresholdMax <= _bot.RSIBuyThresholdMin || _bot.RSIBuyThresholdMax > 100)
                    _validationErrors.Add("RSI Buy Threshold Max must be greater than Min and <= 100");

                if (_bot.RsiBuyThreshold < _bot.RSIBuyThresholdMin || _bot.RsiBuyThreshold > _bot.RSIBuyThresholdMax)
                    _validationErrors.Add($"RSI Buy Threshold ({_bot.RsiBuyThreshold}) must be between Min ({_bot.RSIBuyThresholdMin}) and Max ({_bot.RSIBuyThresholdMax})");

                if (_bot.RSISellThresholdMin < _bot.RSIBuyThresholdMax)
                    _validationErrors.Add("RSI Sell Threshold Min must be greater than or equal to RSI Buy Threshold Max");

                if (_bot.RSISellThresholdMax <= _bot.RSISellThresholdMin || _bot.RSISellThresholdMax > 100)
                    _validationErrors.Add("RSI Sell Threshold Max must be greater than Min and <= 100");

                if (_bot.RsiSellThreshold < _bot.RSISellThresholdMin || _bot.RsiSellThreshold > _bot.RSISellThresholdMax)
                    _validationErrors.Add($"RSI Sell Threshold ({_bot.RsiSellThreshold}) must be between Min ({_bot.RSISellThresholdMin}) and Max ({_bot.RSISellThresholdMax})");

                if (_bot.MomentumThresholdMin <= 0)
                    _validationErrors.Add("Momentum Threshold Min must be greater than 0");

                if (_bot.MomentumThresholdMax <= _bot.MomentumThresholdMin)
                    _validationErrors.Add("Momentum Threshold Max must be greater than Momentum Threshold Min");

                if (_bot.RegimeLookbackPeriod < 10)
                    _validationErrors.Add("Regime Lookback Period must be at least 10");

                if (_bot.RegimeAdxThreshold <= 0)
                    _validationErrors.Add("Regime ADX Threshold must be greater than 0");

                if (_bot.RegimeAtrTrendingMultiplier <= 0)
                    _validationErrors.Add("Regime ATR Trending Multiplier must be greater than 0");

                if (_bot.RegimeAtrHighVolMultiplier <= 0)
                    _validationErrors.Add("Regime ATR High Volatility Multiplier must be greater than 0");

                if (_bot.RegimeAtrLowVolMultiplier <= 0)
                    _validationErrors.Add("Regime ATR Low Volatility Multiplier must be greater than 0");

                if (_bot.MinVolume < 0)
                    _validationErrors.Add("Min Volume cannot be negative");

                if (_bot.MaxVolume <= _bot.MinVolume)
                    _validationErrors.Add("Max Volume must be greater than Min Volume");

                if (_bot.VolumeDynamicFactor <= 0)
                    _validationErrors.Add("Volume Dynamic Factor must be greater than 0");

                if (_bot.VolumeDynamicMaxFactor <= _bot.VolumeDynamicFactor)
                    _validationErrors.Add("Volume Dynamic Max Factor must be greater than Volume Dynamic Factor");

                if (_bot.MaxSpreadPips < 0)
                    _validationErrors.Add("Max Spread (Pips) cannot be negative");

                if (_bot.Ma2Ma3Spread < 0)
                    _validationErrors.Add("MA2/MA3 Spread cannot be negative");

                if (_bot.PriceDistanceToMa2 < 0)
                    _validationErrors.Add("Price Distance to MA2 cannot be negative");
            }

            public List<string> GetValidationErrors() => new List<string>(_validationErrors);
            public List<string> GetValidationWarnings() => new List<string>(_validationWarnings);
        }

        // Parameter Correlation Analysis Data Structures
        internal class ParameterCorrelationData
        {
            public string ParameterName { get; set; }
            public double ParameterValue { get; set; }
            public double PerformanceMetric { get; set; }
            public MarketRegime Regime { get; set; }
            public DateTime Timestamp { get; set; }
        }

        internal class CorrelationMatrix
        {
            public Dictionary<string, Dictionary<string, double>> Correlations { get; set; } = new Dictionary<string, Dictionary<string, double>>();
            public Dictionary<string, int> SampleSizes { get; set; } = new Dictionary<string, int>();
            public DateTime LastUpdated { get; set; }
        }

        internal class ParameterOptimizationSuggestion
        {
            public string ParameterName { get; set; }
            public double CurrentValue { get; set; }
            public double SuggestedValue { get; set; }
            public double ExpectedImprovement { get; set; }
            public string Reasoning { get; set; }
            public MarketRegime TargetRegime { get; set; }
        }

        // Correlation analysis fields
        private List<ParameterCorrelationData> correlationData = new List<ParameterCorrelationData>();
        private CorrelationMatrix parameterMatrix = new CorrelationMatrix();
        private Dictionary<MarketRegime, CorrelationMatrix> regimeMatrices = new Dictionary<MarketRegime, CorrelationMatrix>();
        private List<ParameterOptimizationSuggestion> optimizationSuggestions = new List<ParameterOptimizationSuggestion>();
        private Queue<double> rollingCorrelations = new Queue<double>();
        internal class DoubleExponentialMovingAverage
        {
            private readonly DataSeries source;
            private readonly int period;
            private readonly ExponentialMovingAverage ema;
            private readonly ExponentialMovingAverage emaOfEma;
            private readonly IndicatorDataSeries result;
            private readonly Robot robot;
            private int lastCalculatedIndex = -1;
            private long calculationCount = 0;
            private long totalCalculationTime = 0;

            public IndicatorDataSeries Result => result;

            public DoubleExponentialMovingAverage(Robot robot, DataSeries source, int period)
            {
                this.robot = robot;
                this.source = source;
                this.period = period;
                this.result = robot.CreateDataSeries();
                this.ema = robot.Indicators.ExponentialMovingAverage(source, period);
                this.emaOfEma = robot.Indicators.ExponentialMovingAverage(ema.Result, period);
            }

            public void Calculate(int index)
            {
                // Early validation and bounds checking
                if (index < 0 || index >= result.Count || period <= 0)
                {
                    return;
                }

                // Skip if already calculated for this index
                if (index == lastCalculatedIndex)
                {
                    return;
                }

                // Performance monitoring
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    if (index < period)
                    {
                        result[index] = double.NaN;
                        return;
                    }

                    // Validate EMA values before calculation
                    double emaValue = ema.Result[index];
                    double emaOfEmaValue = emaOfEma.Result[index];

                    if (double.IsNaN(emaValue) || double.IsNaN(emaOfEmaValue))
                    {
                        result[index] = double.NaN;
                        return;
                    }

                    // Optimized calculation: 2 * EMA - EMA_of_EMA
                    result[index] = 2.0 * emaValue - emaOfEmaValue;

                    lastCalculatedIndex = index;
                }
                finally
                {
                    stopwatch.Stop();
                    calculationCount++;
                    totalCalculationTime += stopwatch.ElapsedTicks;
                }
            }

            public double GetAverageCalculationTimeMs()
            {
                return calculationCount > 0 ? (totalCalculationTime * 1000.0) / (System.Diagnostics.Stopwatch.Frequency * calculationCount) : 0;
            }
        }
        internal class CustomHullAtr
        {
            private readonly Bars bars;
            private readonly int period;
            private readonly IndicatorDataSeries result;
            private readonly Robot robot;
            private readonly IndicatorDataSeries wma1;
            private readonly IndicatorDataSeries wma2;
            private readonly IndicatorDataSeries rawHma;
            private readonly IndicatorDataSeries trueRangeSeries;
            private readonly Dictionary<int, double> trueRangeCache;
            private int lastCalculatedIndex = -1;
            private long calculationCount = 0;
            private long totalCalculationTime = 0;
            private readonly int period1;
            private readonly int hmaPeriod;

            public IndicatorDataSeries Result => result;

            public CustomHullAtr(Robot robot, Bars bars, int period)
            {
                this.robot = robot;
                this.bars = bars;
                this.period = period;
                this.result = robot.CreateDataSeries();
                this.wma1 = robot.CreateDataSeries();
                this.wma2 = robot.CreateDataSeries();
                this.rawHma = robot.CreateDataSeries();
                this.trueRangeSeries = robot.CreateDataSeries();
                this.trueRangeCache = new Dictionary<int, double>();
                this.period1 = period / 2;
                this.hmaPeriod = (int)Math.Sqrt(period);
            }

            private double CalculateTrueRange(int index)
            {
                // Thread-safe cache access (though cTrader is single-threaded, good practice)
                lock (trueRangeCache)
                {
                    if (trueRangeCache.TryGetValue(index, out double cachedTr))
                    {
                        return cachedTr;
                    }

                    if (index < 1 || index >= bars.HighPrices.Count || index >= bars.LowPrices.Count || index >= bars.ClosePrices.Count)
                    {
                        return double.NaN;
                    }

                    double highLow = bars.HighPrices[index] - bars.LowPrices[index];
                    double highClose = Math.Abs(bars.HighPrices[index] - bars.ClosePrices[index - 1]);
                    double lowClose = Math.Abs(bars.LowPrices[index] - bars.ClosePrices[index - 1]);
                    double trueRange = Math.Max(highLow, Math.Max(highClose, lowClose));

                    trueRangeCache[index] = trueRange;
                    return trueRange;
                }
            }

            public void Calculate(int index)
            {
                // Early validation and bounds checking
                if (index < 0 || index >= result.Count || period <= 0 || index < period || index < 1)
                {
                    result[index] = double.NaN;
                    return;
                }

                // Skip if already calculated for this index
                if (index == lastCalculatedIndex)
                {
                    return;
                }

                // Performance monitoring
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    // Calculate WMA1 (period/2) with optimized True Range calculation
                    double sum1 = 0;
                    double weightSum1 = 0;
                    for (int i = 0; i < period1; i++)
                    {
                        int trIndex = index - i;
                        double trueRange = CalculateTrueRange(trIndex);
                        if (double.IsNaN(trueRange)) continue;

                        double weight = period1 - i;
                        sum1 += trueRange * weight;
                        weightSum1 += weight;
                    }

                    // Calculate WMA2 (full period) with optimized True Range calculation
                    double sum2 = 0;
                    double weightSum2 = 0;
                    for (int i = 0; i < period; i++)
                    {
                        int trIndex = index - i;
                        double trueRange = CalculateTrueRange(trIndex);
                        if (double.IsNaN(trueRange)) continue;

                        double weight = period - i;
                        sum2 += trueRange * weight;
                        weightSum2 += weight;
                    }

                    // Calculate WMA values
                    wma1[index] = weightSum1 > 0 ? sum1 / weightSum1 : double.NaN;
                    wma2[index] = weightSum2 > 0 ? sum2 / weightSum2 : double.NaN;

                    // Calculate raw HMA
                    if (double.IsNaN(wma1[index]) || double.IsNaN(wma2[index]))
                    {
                        rawHma[index] = double.NaN;
                        result[index] = double.NaN;
                        return;
                    }

                    rawHma[index] = 2.0 * wma1[index] - wma2[index];

                    // Calculate final HMA with optimized weighting
                    double hmaSum = 0;
                    double hmaWeightSum = 0;
                    for (int i = 0; i < hmaPeriod; i++)
                    {
                        int hmaIndex = index - i;
                        if (hmaIndex < 0 || double.IsNaN(rawHma[hmaIndex])) continue;

                        double weight = hmaPeriod - i;
                        hmaSum += rawHma[hmaIndex] * weight;
                        hmaWeightSum += weight;
                    }

                    result[index] = hmaWeightSum > 0 ? hmaSum / hmaWeightSum : double.NaN;
                    lastCalculatedIndex = index;
                }
                finally
                {
                    stopwatch.Stop();
                    calculationCount++;
                    totalCalculationTime += stopwatch.ElapsedTicks;
                }
            }

            public double GetAverageCalculationTimeMs()
            {
                return calculationCount > 0 ? (totalCalculationTime * 1000.0) / (System.Diagnostics.Stopwatch.Frequency * calculationCount) : 0;
            }
        }
        internal class CustomDoubleExponentialAtr
        {
            private readonly Bars bars;
            private readonly int period;
            private readonly IndicatorDataSeries result;
            private readonly Robot robot;
            private readonly IndicatorDataSeries ema;
            private readonly IndicatorDataSeries emaOfEma;
            private readonly Dictionary<int, double> trueRangeCache;
            private int lastCalculatedIndex = -1;
            private long calculationCount = 0;
            private long totalCalculationTime = 0;
            private readonly double smoothingFactor;

            public IndicatorDataSeries Result => result;

            public CustomDoubleExponentialAtr(Robot robot, Bars bars, int period)
            {
                this.robot = robot;
                this.bars = bars;
                this.period = period;
                this.result = robot.CreateDataSeries();
                this.ema = robot.CreateDataSeries();
                this.emaOfEma = robot.CreateDataSeries();
                this.trueRangeCache = new Dictionary<int, double>();
                this.smoothingFactor = EMA_SMOOTHING_FACTOR / (1.0 + period);
            }

            private double CalculateTrueRange(int index)
            {
                // Thread-safe cache access (though cTrader is single-threaded, good practice)
                lock (trueRangeCache)
                {
                    if (trueRangeCache.TryGetValue(index, out double cachedTr))
                    {
                        return cachedTr;
                    }

                    if (index < 1 || index >= bars.HighPrices.Count || index >= bars.LowPrices.Count || index >= bars.ClosePrices.Count)
                    {
                        return double.NaN;
                    }

                    double highLow = bars.HighPrices[index] - bars.LowPrices[index];
                    double highClose = Math.Abs(bars.HighPrices[index] - bars.ClosePrices[index - 1]);
                    double lowClose = Math.Abs(bars.LowPrices[index] - bars.ClosePrices[index - 1]);
                    double trueRange = Math.Max(highLow, Math.Max(highClose, lowClose));

                    trueRangeCache[index] = trueRange;
                    return trueRange;
                }
            }

            public void Calculate(int index)
            {
                // Early validation and bounds checking
                if (index < 0 || index >= result.Count || period <= 0 || index < period || index < 1)
                {
                    result[index] = double.NaN;
                    return;
                }

                // Skip if already calculated for this index
                if (index == lastCalculatedIndex)
                {
                    return;
                }

                // Performance monitoring
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    double currentTrueRange = CalculateTrueRange(index);
                    if (double.IsNaN(currentTrueRange))
                    {
                        result[index] = double.NaN;
                        return;
                    }

                    // Calculate EMA with optimized initialization
                    if (index == period)
                    {
                        // Initialize EMA with simple average for first calculation
                        double sum = 0;
                        int validCount = 0;
                        for (int i = 0; i < period; i++)
                        {
                            double tr = CalculateTrueRange(index - i);
                            if (!double.IsNaN(tr))
                            {
                                sum += tr;
                                validCount++;
                            }
                        }
                        ema[index] = validCount > 0 ? sum / validCount : double.NaN;
                    }
                    else
                    {
                        // Incremental EMA calculation
                        double prevEma = ema[index - 1];
                        if (!double.IsNaN(prevEma))
                        {
                            ema[index] = (currentTrueRange * smoothingFactor) + (prevEma * (1.0 - smoothingFactor));
                        }
                        else
                        {
                            ema[index] = currentTrueRange;
                        }
                    }

                    // Calculate EMA of EMA
                    if (index == period)
                    {
                        emaOfEma[index] = ema[index];
                    }
                    else
                    {
                        double prevEmaOfEma = emaOfEma[index - 1];
                        double currentEma = ema[index];

                        if (!double.IsNaN(prevEmaOfEma) && !double.IsNaN(currentEma))
                        {
                            emaOfEma[index] = (currentEma * smoothingFactor) + (prevEmaOfEma * (1.0 - smoothingFactor));
                        }
                        else
                        {
                            emaOfEma[index] = currentEma;
                        }
                    }

                    // Final DEMA calculation with validation
                    double emaValue = ema[index];
                    double emaOfEmaValue = emaOfEma[index];

                    if (double.IsNaN(emaValue) || double.IsNaN(emaOfEmaValue))
                    {
                        result[index] = double.NaN;
                    }
                    else
                    {
                        result[index] = 2.0 * emaValue - emaOfEmaValue;
                    }

                    lastCalculatedIndex = index;
                }
                finally
                {
                    stopwatch.Stop();
                    calculationCount++;
                    totalCalculationTime += stopwatch.ElapsedTicks;
                }
            }

            public double GetAverageCalculationTimeMs()
            {
                return calculationCount > 0 ? (totalCalculationTime * 1000.0) / (System.Diagnostics.Stopwatch.Frequency * calculationCount) : 0;
            }
        }
        private TimeSpan GetTimeFrameSpan(TimeFrame timeFrame)
        {
            // Convert TimeFrame to TimeSpan for approximate bar duration
            string tf = timeFrame.ToString();
            switch (tf)
            {
                case "Minute":
                    return TimeSpan.FromMinutes(1);
                case "Minute5":
                    return TimeSpan.FromMinutes(5);
                case "Minute15":
                    return TimeSpan.FromMinutes(15);
                case "Minute30":
                    return TimeSpan.FromMinutes(30);
                case "Hour":
                    return TimeSpan.FromHours(1);
                case "Hour4":
                    return TimeSpan.FromHours(4);
                case "Hour12":
                    return TimeSpan.FromHours(12);
                case "Daily":
                    return TimeSpan.FromDays(1);
                case "Weekly":
                    return TimeSpan.FromDays(7);
                case "Monthly":
                    return TimeSpan.FromDays(30);
                default:
                    return TimeSpan.FromHours(1); // Default to 1 hour for unknown timeframes
            }
        }

        // ### ENDE MODUL 1 - FÜGE HIER MODUL 2 EIN ###
        // ### MODUL 2 - Adaptive MA Bot v5.6_crypto ###
        // Contains the initialization: OnStart.
        // Handles timeframe synchronization, historical bar loading, indicator initialization.
        // Insert this module directly after the marker '// ### ENDE MODUL 1 - FÜGE HIER MODUL 2 EIN ###'
        // in Module 1, within the AdaptiveMABot_v5_6_crypto class, followed by Module 3 after the marker
        // '// ### ENDE MODUL 2 - FÜGE HIER MODUL 3 EIN ###'.
        //
        // Change Log:
        // Version 5.6_crypto, 2025-08-29, 12:51 CEST: Crypto-optimized version with adjusted parameters for BTCUSD/ETHUSD trading
        // - RiskPercentage: 0.5% (reduced from 1.0% for crypto volatility)
        // - ATR enabled with 0.005 threshold (crypto volatility filter)
        // - RSI thresholds: 25/75 (wider range for crypto momentum)
        // - ADX threshold: 25 (stronger trend requirement for crypto)
        // - Hull MAs for all MA types (better for crypto trends)
        // - Weekend filter disabled (24/7 crypto markets)
        // - Trading sessions set to All (24/7 availability)

        protected override void OnStart()
        {
            // Initialize parameter validator and validate all parameters at startup
            var parameterValidator = new ParameterValidator(this);
            bool parametersValid = parameterValidator.ValidateAllParameters();

            if (!parametersValid)
            {
                LogMessage("=== CRITICAL: Bot startup aborted due to invalid parameters ===", LoggingLevel.OnlyCritical);
                LogMessage("Please review and correct the parameter configuration before restarting the bot.", LoggingLevel.OnlyCritical);
                LogMessage("Check the parameter validation errors above for specific issues.", LoggingLevel.OnlyCritical);
                return; // Gracefully exit OnStart without initializing the bot
            }



            if (UseChartTimeframe)
            {
                AnalysisTimeframe = TimeFrame;
            }
            analysisBars = MarketData.GetBars(AnalysisTimeframe, Symbol.Name);
            if (analysisBars.Count < HistoricalBarsToLoad)
            {
                LogMessage($"Warning: Only {analysisBars.Count} bars loaded, but HistoricalBarsToLoad is {HistoricalBarsToLoad}. Consider adjusting or checking data availability.", LoggingLevel.OnlyImportant);
            }
            ma1 = CreateDataSeries();
            ma2 = CreateDataSeries();
            ma3 = CreateDataSeries();
            rsi = Indicators.RelativeStrengthIndex(analysisBars.ClosePrices, RsiPeriod);
            atr = CreateDataSeries();
            adx = Indicators.DirectionalMovementSystem(analysisBars, AdxPeriod);
            if (AtrMode == AtrFilterMode.MultiTimeframe)
            {
                multiBars = MarketData.GetBars(MultiAtrTimeframe, Symbol.Name);
                multiAtr = CreateDataSeries();
            }
            switch (Ma1Type)
            {
                case MovingAverageType.Simple:
                    ma1 = Indicators.SimpleMovingAverage(analysisBars.ClosePrices, Ma1Period).Result;
                    break;
                case MovingAverageType.Exponential:
                    ma1 = Indicators.ExponentialMovingAverage(analysisBars.ClosePrices, Ma1Period).Result;
                    break;
                case MovingAverageType.Weighted:
                    ma1 = Indicators.WeightedMovingAverage(analysisBars.ClosePrices, Ma1Period).Result;
                    break;
                case MovingAverageType.Hull:
                    ma1 = Indicators.HullMovingAverage(analysisBars.ClosePrices, Ma1Period).Result;
                    break;
                case MovingAverageType.DoubleExponential:
                    dema1 = new DoubleExponentialMovingAverage(this, analysisBars.ClosePrices, Ma1Period);
                    ma1 = dema1.Result;
                    break;
            }
            switch (Ma2Type)
            {
                case MovingAverageType.Simple:
                    ma2 = Indicators.SimpleMovingAverage(analysisBars.ClosePrices, Ma2Period).Result;
                    break;
                case MovingAverageType.Exponential:
                    ma2 = Indicators.ExponentialMovingAverage(analysisBars.ClosePrices, Ma2Period).Result;
                    break;
                case MovingAverageType.Weighted:
                    ma2 = Indicators.WeightedMovingAverage(analysisBars.ClosePrices, Ma2Period).Result;
                    break;
                case MovingAverageType.Hull:
                    ma2 = Indicators.HullMovingAverage(analysisBars.ClosePrices, Ma2Period).Result;
                    break;
                case MovingAverageType.DoubleExponential:
                    dema2 = new DoubleExponentialMovingAverage(this, analysisBars.ClosePrices, Ma2Period);
                    ma2 = dema2.Result;
                    break;
            }
            switch (Ma3Type)
            {
                case MovingAverageType.Simple:
                    ma3 = Indicators.SimpleMovingAverage(analysisBars.ClosePrices, Ma3Period).Result;
                    break;
                case MovingAverageType.Exponential:
                    ma3 = Indicators.ExponentialMovingAverage(analysisBars.ClosePrices, Ma3Period).Result;
                    break;
                case MovingAverageType.Weighted:
                    ma3 = Indicators.WeightedMovingAverage(analysisBars.ClosePrices, Ma3Period).Result;
                    break;
                case MovingAverageType.Hull:
                    ma3 = Indicators.HullMovingAverage(analysisBars.ClosePrices, Ma3Period).Result;
                    break;
                case MovingAverageType.DoubleExponential:
                    dema3 = new DoubleExponentialMovingAverage(this, analysisBars.ClosePrices, Ma3Period);
                    ma3 = dema3.Result;
                    break;
            }
            switch (AtrMaType)
            {
                case MovingAverageType.Simple:
                    atr = Indicators.SimpleMovingAverage(Indicators.TrueRange(analysisBars).Result, AtrPeriod).Result;
                    break;
                case MovingAverageType.Exponential:
                    atr = Indicators.ExponentialMovingAverage(Indicators.TrueRange(analysisBars).Result, AtrPeriod).Result;
                    break;
                case MovingAverageType.Weighted:
                    atr = Indicators.WeightedMovingAverage(Indicators.TrueRange(analysisBars).Result, AtrPeriod).Result;
                    break;
                case MovingAverageType.Hull:
                    customHullAtr = new CustomHullAtr(this, analysisBars, AtrPeriod);
                    atr = customHullAtr.Result;
                    break;
                case MovingAverageType.DoubleExponential:
                    customDemaAtr = new CustomDoubleExponentialAtr(this, analysisBars, AtrPeriod);
                    atr = customDemaAtr.Result;
                    break;
            }
            if (AtrMode == AtrFilterMode.MultiTimeframe)
            {
                switch (AtrMaType)
                {
                    case MovingAverageType.Simple:
                        multiAtr = Indicators.SimpleMovingAverage(Indicators.TrueRange(multiBars).Result, AtrPeriod).Result;
                    break;
                    case MovingAverageType.Exponential:
                        multiAtr = Indicators.ExponentialMovingAverage(Indicators.TrueRange(multiBars).Result, AtrPeriod).Result;
                        break;
                    case MovingAverageType.Weighted:
                        multiAtr = Indicators.WeightedMovingAverage(Indicators.TrueRange(multiBars).Result, AtrPeriod).Result;
                        break;
                    case MovingAverageType.Hull:
                        customHullAtr = new CustomHullAtr(this, multiBars, AtrPeriod);
                        multiAtr = customHullAtr.Result;
                        break;
                    case MovingAverageType.DoubleExponential:
                        customDemaAtr = new CustomDoubleExponentialAtr(this, multiBars, AtrPeriod);
                        multiAtr = customDemaAtr.Result;
                        break;
                }
            }
            Positions.Opened += OnPositionOpenedEvent;
            Positions.Closed += OnPositionClosedEvent;
            Chart.ObjectsAdded += (ChartObjectsAddedEventArgs e) =>
            {
                foreach (var chartObject in e.ChartObjects)
                {
                    if (!chartObject.Name.StartsWith("Entry") && !chartObject.Name.StartsWith("Exit") && !chartObject.Name.StartsWith("TS Update"))
                    {
                        // Preserve user-defined markers
                    }
                }
            };
            consecutiveTPs = 0;
            consecutiveSLs = 0;
            lastTradeWasTP = false;
            lastTradeWasSL = false;
            totalTrades = 0;
            winningTrades = 0;
            losingTrades = 0;
            totalProfit = 0;
            totalLoss = 0;
            isTrailingStopActive = false;
            lastLossBarIndex = -1;
            isTradeBlocked = false;
            consecutiveConfirmationCandles = 0;
            isLongSignalActive = false;
            isShortSignalActive = false;
            spreadCandleCount = 0;
            isSpreadLongSignalActive = false;
            isSpreadShortSignalActive = false;
            tradeReturns = new List<double>();
            recentProfits = new List<double>();
            currentRegime = MarketRegime.Ranging; // Initialize to default
            rollingATR = new RollingStatistics(RollingStatsPeriod);
            rollingVolume = new RollingStatistics(RollingStatsPeriod);
            rollingADX = new RollingStatistics(RollingStatsPeriod);

            // Initialize rolling performance analysis
            if (EnableRollingPerformance)
            {
                regimeReturns[MarketRegime.Trending] = new List<double>();
                regimeReturns[MarketRegime.Ranging] = new List<double>();
                regimeReturns[MarketRegime.HighVolatility] = new List<double>();
                regimeReturns[MarketRegime.LowVolatility] = new List<double>();
            }

            // Initialize correlation analysis
            if (EnableCorrelationAnalysis)
            {
                correlationData = new List<ParameterCorrelationData>();
                parameterMatrix = new CorrelationMatrix();
                regimeMatrices = new Dictionary<MarketRegime, CorrelationMatrix>();
                optimizationSuggestions = new List<ParameterOptimizationSuggestion>();
                rollingCorrelations = new Queue<double>();

                LogMessage($"[CORRELATION] Initialized correlation analysis - Window: {CorrelationAnalysisWindowSize}, Min Samples: {MinimumSampleRequirements}", LoggingLevel.OnlyImportant);
            }

            // Initialize LRU caches with size limits
            atrCache = new LRUCache<int, double>(MaxCacheSize);
            ma1Cache = new LRUCache<int, double>(MaxCacheSize);
            ma2Cache = new LRUCache<int, double>(MaxCacheSize);
            ma3Cache = new LRUCache<int, double>(MaxCacheSize);
            ma2Ma3SpreadCache = new LRUCache<int, double>(MaxCacheSize);
            momentumThresholdCache = new LRUCache<int, double>(MaxCacheSize);
            correlationCache = new LRUCache<string, LRUCache<int, double>>(MaxCacheSize / 10); // Smaller for nested cache
            sharpeRatioCache = new LRUCache<int, double>(MaxCacheSize / 10); // Smaller for Sharpe calculations

            // Initialize condition caching system
            conditionCache = new Dictionary<string, bool>();
            conditionCacheTimestamps = new Dictionary<string, DateTime>();
            conditionCacheHits = 0;
            conditionCacheMisses = 0;

            // Initialize pre-computed conditions
            preComputedLastBarIndex = -1;

            LogMessage($"[CACHE] Initialized LRU caches with max size: {MaxCacheSize}", LoggingLevel.OnlyImportant);
            LogMessage($"[CONDITION_CACHE] Initialized condition caching system", LoggingLevel.OnlyImportant);

            // Initialize performance monitoring

            LogMessage("[PERFORMANCE] Performance monitoring initialized", LoggingLevel.OnlyImportant);
        }

        // ### ENDE MODUL 2 - FÜGE HIER MODUL 3 EIN ###
        // ### MODUL 3 - Adaptive MA Bot v5.6_crypto ###
        // Contains OnTick and PlotMovingAverages.
        // Handles real-time updates, MA calculations, and plotting.
        // Insert this module directly after the marker '// ### ENDE MODUL 2 - FÜGE HIER MODUL 3 EIN ###'
        // in Module 2, within the AdaptiveMABot_v5_6_crypto class, followed by Module 4 after the marker
        // '// ### ENDE MODUL 3 - FÜGE HIER MODUL 4 EIN ###'.

        protected override void OnTick()
        {
            try
            {
                // Check for new bar
                if (analysisBars != null && analysisBars.OpenTimes != null && analysisBars.OpenTimes.Count > 0)
                {
                    DateTime currentBarTime = analysisBars.OpenTimes.LastValue;
                    if (currentBarTime != lastBarTime)
                    {
                        isNewBar = true;
                        lastBarTime = currentBarTime;
                    }
                    else
                    {
                        isNewBar = false;
                    }
                }

                // Execute tick-based operations (critical real-time operations)
                ExecuteTickOperations();

                // Execute bar-based operations based on execution mode
                if (BotExecutionMode == ExecutionMode.OnTick)
                {
                    // OnTick mode: Execute bar operations on every tick
                    ExecuteBarOperations();
                }
                else if (BotExecutionMode == ExecutionMode.OnBar)
                {
                    // OnBar mode: Execute bar operations only on new bar
                    if (isNewBar)
                    {
                        ExecuteBarOperations();
                    }
                }
                else if (BotExecutionMode == ExecutionMode.Hybrid)
                {
                    // Hybrid mode: Execute both tick and bar operations on new bar
                    if (isNewBar)
                    {
                        ExecuteTickOperations();
                        ExecuteBarOperations();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in OnTick: {ex.Message}. Stack trace: {ex.StackTrace}", LoggingLevel.OnlyCritical);
            }
        }

        protected override void OnBar()
        {
            try
            {
                // Execute bar operations on bar close
                ExecuteBarOperations();
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in OnBar: {ex.Message}. Stack trace: {ex.StackTrace}", LoggingLevel.OnlyCritical);
            }
        }

        /// <summary>
        /// Executes operations that should run on every tick (critical real-time operations)
        /// </summary>
        private void ExecuteTickOperations()
        {
            try
            {
                // Validate essential data with detailed error messages
                if (analysisBars == null)
                {
                    LogMessage("Critical Error: Analysis bars object is null. Bot cannot function without market data.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices == null)
                {
                    LogMessage("Critical Error: Close prices data series is null. Market data may be corrupted.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices.Count == 0)
                {
                    LogMessage("Warning: No price data available yet. Waiting for market data to load.", LoggingLevel.OnlyCritical);
                    return;
                }

                int index = analysisBars.ClosePrices.Count - 1;
                if (index < 0)
                {
                    LogMessage("Critical Error: Invalid index calculation. This should not happen.", LoggingLevel.OnlyCritical);
                    return;
                }

                // Validate sufficient historical data
                int requiredBars = Math.Max(Math.Max(Ma1Period, Ma2Period), Ma3Period);
                if (requiredBars <= 0)
                {
                    LogMessage($"Critical Error: Invalid MA periods detected. MA1: {Ma1Period}, MA2: {Ma2Period}, MA3: {Ma3Period}", LoggingLevel.OnlyCritical);
                    return;
                }

                if (index < requiredBars)
                {
                    LogMessage($"Warning: Insufficient historical data. Need {requiredBars} bars, currently have {index + 1}. Waiting for more data.", LoggingLevel.Warning);
                    return;
                }

                // Validate MA data series with detailed checks
                if (ma1 == null)
                {
                    LogMessage("Critical Error: MA1 data series not initialized. Check MA1 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }
                if (ma2 == null)
                {
                    LogMessage("Critical Error: MA2 data series not initialized. Check MA2 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }
                if (ma3 == null)
                {
                    LogMessage("Critical Error: MA3 data series not initialized. Check MA3 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }

                // Validate data series bounds
                if (index >= ma1.Count || index >= ma2.Count || index >= ma3.Count)
                {
                    LogMessage($"Critical Error: Index {index} out of bounds for MA data series. MA1 count: {ma1.Count}, MA2 count: {ma2.Count}, MA3 count: {ma3.Count}", LoggingLevel.OnlyCritical);
                    return;
                }

                // Critical real-time operations that need to run on every tick
                try
                {
                    ManagePositions();
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in ManagePositions: {ex.Message}", LoggingLevel.Error);
                        // Continue with other operations even if position management fails
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Critical error in ExecuteTickOperations: {ex.Message}. Stack trace: {ex.StackTrace}", LoggingLevel.OnlyCritical);
                }
        }

        /// <summary>
        /// Executes operations that should run on bar close (heavy computational operations)
        /// </summary>
        private void ExecuteBarOperations()
        {
            try
            {
                // Validate essential data with detailed error messages
                if (analysisBars == null)
                {
                    LogMessage("Critical Error: Analysis bars object is null. Bot cannot function without market data.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices == null)
                {
                    LogMessage("Critical Error: Close prices data series is null. Market data may be corrupted.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices.Count == 0)
                {
                    LogMessage("Warning: No price data available yet. Waiting for market data to load.", LoggingLevel.OnlyCritical);
                    return;
                }

                int index = analysisBars.ClosePrices.Count - 1;

                // Validate sufficient historical data
                int requiredBars = Math.Max(Math.Max(Ma1Period, Ma2Period), Ma3Period);
                if (requiredBars <= 0)
                {
                    LogMessage($"Critical Error: Invalid MA periods detected. MA1: {Ma1Period}, MA2: {Ma2Period}, MA3: {Ma3Period}", LoggingLevel.OnlyCritical);
                    return;
                }

                if (index < requiredBars)
                {
                    LogMessage($"Warning: Insufficient historical data. Need {requiredBars} bars, currently have {index + 1}. Waiting for more data.", LoggingLevel.Warning);
                    return;
                }

                // Validate MA data series with detailed checks
                if (ma1 == null)
                {
                    LogMessage("Critical Error: MA1 data series not initialized. Check MA1 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }
                if (ma2 == null)
                {
                    LogMessage("Critical Error: MA2 data series not initialized. Check MA2 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }
                if (ma3 == null)
                {
                    LogMessage("Critical Error: MA3 data series not initialized. Check MA3 configuration.", LoggingLevel.OnlyCritical);
                    return;
                }

                // Validate data series bounds
                if (index >= ma1.Count || index >= ma2.Count || index >= ma3.Count)
                {
                    LogMessage($"Critical Error: Index {index} out of bounds for MA data series. MA1 count: {ma1.Count}, MA2 count: {ma2.Count}, MA3 count: {ma3.Count}", LoggingLevel.OnlyCritical);
                    return;
                }

                // Cache invalidation: Clear caches when bar index changes significantly
                if (index != lastCachedBarIndex)
                {
                    // Only clear caches if we've moved more than 10 bars (prevents excessive clearing)
                    if (lastCachedBarIndex == -1 || Math.Abs(index - lastCachedBarIndex) > 10)
                    {
                        ClearCaches();
                        ClearConditionCache();
                        // Warm caches with recent values after clearing
                        WarmCaches();
                    }
                    lastCachedBarIndex = index;
                }

                // Pre-compute common conditions for performance optimization
                if (index != preComputedLastBarIndex)
                {
                    PreComputeConditions(index);
                    preComputedLastBarIndex = index;
                }

                // Calculate custom MAs if needed with individual error handling
                try
                {
                    if (Ma1Type == MovingAverageType.DoubleExponential)
                    {
                        if (dema1 == null)
                        {
                            LogMessage("Warning: DEMA1 not initialized despite being selected", LoggingLevel.Warning);
                        }
                        else if (index >= Ma1Period && index < analysisBars.ClosePrices.Count)
                        {
                            var demaStopwatch = System.Diagnostics.Stopwatch.StartNew();
                            dema1.Calculate(index);
                            demaStopwatch.Stop();
                            LogMessage($"[PERFORMANCE] DEMA1 calculation: {demaStopwatch.ElapsedTicks} ticks ({demaStopwatch.Elapsed.TotalMilliseconds:F3}ms)", LoggingLevel.OnlyImportant);
                        }
                    }

                    if (Ma2Type == MovingAverageType.DoubleExponential)
                    {
                        if (dema2 == null)
                        {
                            LogMessage("Warning: DEMA2 not initialized despite being selected", LoggingLevel.Warning);
                        }
                        else if (index >= Ma2Period && index < analysisBars.ClosePrices.Count)
                        {
                            var demaStopwatch = System.Diagnostics.Stopwatch.StartNew();
                            dema2.Calculate(index);
                            demaStopwatch.Stop();
                            if (LogLevel == LoggingLevel.Full)
                            {
                                LogMessage($"[PERFORMANCE] DEMA2 calculation: {demaStopwatch.ElapsedTicks} ticks ({demaStopwatch.Elapsed.TotalMilliseconds:F3}ms)", LoggingLevel.Full);
                            }
                        }
                    }

                    if (Ma3Type == MovingAverageType.DoubleExponential)
                    {
                        if (dema3 == null)
                        {
                            LogMessage("Warning: DEMA3 not initialized despite being selected", LoggingLevel.Warning);
                        }
                        else if (index >= Ma3Period && index < analysisBars.ClosePrices.Count)
                        {
                            var demaStopwatch = System.Diagnostics.Stopwatch.StartNew();
                            dema3.Calculate(index);
                            demaStopwatch.Stop();
                            if (LogLevel == LoggingLevel.Full)
                            {
                                LogMessage($"[PERFORMANCE] DEMA3 calculation: {demaStopwatch.Elapsed.TotalMilliseconds:F3}ms)", LoggingLevel.Full);
                            }
                        }
                    }

                    // Handle ATR calculations with performance monitoring
                    if (AtrMaType == MovingAverageType.Hull)
                    {
                        if (customHullAtr == null)
                        {
                            LogMessage("Warning: Custom Hull ATR not initialized despite being selected", LoggingLevel.Warning);
                        }
                        else if (index >= AtrPeriod && index < analysisBars.ClosePrices.Count)
                        {
                            var atrStopwatch = System.Diagnostics.Stopwatch.StartNew();
                            customHullAtr.Calculate(index);
                            atrStopwatch.Stop();
                            LogMessage($"[PERFORMANCE] CustomHullAtr calculation: {atrStopwatch.ElapsedTicks} ticks ({atrStopwatch.Elapsed.TotalMilliseconds:F3}ms)", LoggingLevel.OnlyImportant);
                        }
                    }
                    else if (AtrMaType == MovingAverageType.DoubleExponential)
                    {
                        if (customDemaAtr == null)
                        {
                            LogMessage("Warning: Custom DEMA ATR not initialized despite being selected", LoggingLevel.Warning);
                        }
                        else if (index >= AtrPeriod && index < analysisBars.ClosePrices.Count)
                        {
                            var atrStopwatch = System.Diagnostics.Stopwatch.StartNew();
                            customDemaAtr.Calculate(index);
                            atrStopwatch.Stop();
                            if (LogLevel == LoggingLevel.Full)
                            {
                                LogMessage($"[PERFORMANCE] CustomDoubleExponentialAtr calculation: {atrStopwatch.ElapsedTicks} ticks ({atrStopwatch.Elapsed.TotalMilliseconds:F3}ms)", LoggingLevel.Full);
                            }
                        }
                    }

                    // Handle multi-timeframe ATR
                    if (AtrMode == AtrFilterMode.MultiTimeframe)
                    {
                        if (multiBars == null)
                        {
                            LogMessage("Warning: Multi-timeframe bars not available despite multi-timeframe ATR being enabled", LoggingLevel.Warning);
                        }
                        else if (multiBars.ClosePrices == null || multiBars.ClosePrices.Count == 0)
                        {
                            LogMessage("Warning: Multi-timeframe close prices not available", LoggingLevel.Warning);
                        }
                        else
                        {
                            int multiIndex = multiBars.ClosePrices.Count - 1;
                            if (multiIndex < 0)
                            {
                                LogMessage("Warning: Invalid multi-timeframe index", LoggingLevel.Warning);
                            }
                            else
                            {
                                if (AtrMaType == MovingAverageType.Hull && customHullAtr != null)
                                {
                                    customHullAtr.Calculate(multiIndex);
                                }
                                else if (AtrMaType == MovingAverageType.DoubleExponential && customDemaAtr != null)
                                {
                                    customDemaAtr.Calculate(multiIndex);
                                }
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    LogMessage($"Error in MA calculations - Index out of range: {ex.Message}", LoggingLevel.Error);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    LogMessage($"Error in MA calculations - Invalid operation: {ex.Message}", LoggingLevel.Error);
                    return;
                }
                catch (Exception ex)
                {
                    LogMessage($"Unexpected error in MA calculations: {ex.Message}", LoggingLevel.Error);
                    return;
                }

                // Update rolling statistics (with performance monitoring)
                if (EnableRollingStats)
                {
                    var statsStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        if (atr != null && atr.Count > 0 && !double.IsNaN(atr.LastValue))
                            rollingATR.AddValue(atr.LastValue);
                        if (analysisBars != null && analysisBars.TickVolumes != null && analysisBars.TickVolumes.Count > 0)
                            rollingVolume.AddValue(analysisBars.TickVolumes.LastValue);
                        if (adx != null && adx.ADX != null && adx.ADX.Count > 0 && !double.IsNaN(adx.ADX.LastValue))
                            rollingADX.AddValue(adx.ADX.LastValue);

                        statsStopwatch.Stop();
                        LogMessage($"[PERFORMANCE] Rolling stats update: {statsStopwatch.Elapsed.TotalMilliseconds:F3}ms", LoggingLevel.OnlyImportant);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error updating rolling statistics: {ex.Message}", LoggingLevel.Error);
                    }
                }

                // Determine if chart updates should be performed based on execution mode and frequency
                bool shouldUpdateChart = false;
                chartUpdateCounter++;

                if (BotExecutionMode == ExecutionMode.OnBar)
                {
                    shouldUpdateChart = true; // Always update on bar for OnBar mode
                }
                else if (BotExecutionMode == ExecutionMode.Hybrid)
                {
                    shouldUpdateChart = (chartUpdateCounter % ChartUpdateFrequency == 0);
                }
                else // OnTick
                {
                    shouldUpdateChart = (chartUpdateCounter % ChartUpdateFrequency == 0);
                }

                // Execute heavy computational operations
                try
                {
                    if (shouldUpdateChart)
                    {
                        PlotMovingAverages(index);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in PlotMovingAverages: {ex.Message}", LoggingLevel.Error);
                    // Continue with other operations even if plotting fails
                }

                try
                {
                    CheckForEntries();
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in CheckForEntries: {ex.Message}", LoggingLevel.Error);
                    // Continue with other operations even if entry checking fails
                }

                try
                {
                    if (shouldUpdateChart)
                    {
                        UpdateStatusField();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in UpdateStatusField: {ex.Message}", LoggingLevel.Error);
                    // Status update failure is least critical
                }

                // Periodic updates every 100 ticks (but only on bar for performance)
                tickCounter++;
                if (tickCounter % 100 == 0)
                {
                    try
                    {
                        AnalyzeAndAdapt();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error in periodic AnalyzeAndAdapt: {ex.Message}", LoggingLevel.Error);
                    }
                }

                // Update rolling performance metrics based on frequency setting
                performanceUpdateCounter++;
                if (performanceUpdateCounter % PerformanceUpdateFrequency == 0)
                {
                    try
                    {
                        UpdateRollingPerformanceMetrics();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error in periodic performance update: {ex.Message}", LoggingLevel.Error);
                    }
                }

                // Periodic correlation analysis updates (bar-based with performance monitoring)
                if (EnableCorrelationAnalysis)
                {
                    barCounter++;
                    if (barCounter % AnalysisFrequencyBars == 0)
                    {
                        var correlationStopwatch = System.Diagnostics.Stopwatch.StartNew();
                        try
                        {
                            if (correlationData.Count >= MinimumSampleRequirements)
                            {
                                UpdateRollingCorrelations();
                                LogCorrelationResults();

                                correlationStopwatch.Stop();
                                LogMessage($"[PERFORMANCE] Correlation analysis: {correlationStopwatch.Elapsed.TotalMilliseconds:F3}ms (every {AnalysisFrequencyBars} bars, sampling: {CorrelationSamplingRate}x)", LoggingLevel.OnlyImportant);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error in periodic correlation update: {ex.Message}", LoggingLevel.Error);
                        }
                    }
                }

                // Periodic performance reporting for optimized indicators
                if (tickCounter % 500 == 0) // Every 500 ticks
                {
                    try
                    {
                        LogIndicatorPerformance();
                        LogCacheStatistics();
                        LogChartPerformance();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error in periodic performance reporting: {ex.Message}", LoggingLevel.Error);
                    }
                }

                // Periodic cache performance testing
                if (tickCounter % 1000 == 0) // Every 1000 ticks
                {
                    try
                    {
                        TestCachePerformance();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error in cache performance test: {ex.Message}", LoggingLevel.Error);
                    }
                }

                try
                {
                    if (shouldUpdateChart || tickCounter % 50 == 0) // Clean up markers less frequently
                    {
                        RemoveOldMarkers();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in RemoveOldMarkers: {ex.Message}", LoggingLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in ExecuteBarOperations: {ex.Message}. Stack trace: {ex.StackTrace}", LoggingLevel.OnlyCritical);
            }
        }

        private void PlotMovingAverages(int index)
        {
            if (index < 1) return;

            // Plot historical bars to show trend, but limit to prevent performance issues
            // Use a reasonable window (100 bars) instead of all historical bars
            int historicalBarsToShow = Math.Min(100, Math.Min(HistoricalBarsToLoad, index + 1));
            int startIndex = Math.Max(0, index - historicalBarsToShow + 1);

            // Determine redraw strategy based on plot mode and batching
            bool shouldRedraw = false;
            int plotInterval = 1;

            if (PlotMode == MaPlotMode.Full)
            {
                shouldRedraw = true;
            }
            else if (PlotMode == MaPlotMode.Optimized)
            {
                plotInterval = Math.Max(1, MaPlotInterval);
                shouldRedraw = (index % plotInterval == 0);
            }
            else if (PlotMode == MaPlotMode.Polyline)
            {
                shouldRedraw = isNewBar; // Only redraw on new bars for polyline mode
            }

            // Skip if batching is enabled and not time to update
            if (EnableChartBatching && !shouldRedraw)
            {
                return;
            }

            // Plot MA1 line
            if (ShowMa1)
            {
                PlotSingleMALine("MA1", ma1, Ma1Color, startIndex, index, shouldRedraw, plotInterval);
            }

            // Plot MA2 line
            if (ShowMa2)
            {
                PlotSingleMALine("MA2", ma2, Ma2Color, startIndex, index, shouldRedraw, plotInterval);
            }

            // Plot MA3 line
            if (ShowMa3)
            {
                PlotSingleMALine("MA3", ma3, Ma3Color, startIndex, index, shouldRedraw, plotInterval);
            }

            // Force process any remaining batch objects after plotting
            if (EnableChartBatching && shouldRedraw)
            {
                ForceProcessChartBatch();
            }
        }

        private void PlotSingleMALine(string maName, IndicatorDataSeries maSeries, Color lineColor, int startIndex, int endIndex, bool shouldRedraw, int plotInterval = 1)
        {
            try
            {
                // Remove existing line if redrawing
                if (shouldRedraw)
                {
                    var existingObjects = Chart.Objects.Where(obj => obj.Name.StartsWith(maName + "_")).ToList();
                    foreach (var obj in existingObjects)
                    {
                        Chart.RemoveObject(obj.Name);
                    }
                }

                // Collect valid MA points for continuous line segments, respecting plot interval
                List<DateTime> times = new List<DateTime>();
                List<double> values = new List<double>();

                for (int i = startIndex; i <= endIndex; i += plotInterval)
                {
                    double maValue = maSeries[i];
                    if (!double.IsNaN(maValue) && maValue > 0)
                    {
                        times.Add(analysisBars.OpenTimes[i]);
                        values.Add(maValue);
                    }
                    else
                    {
                        // Draw current segment if we have points, then start new segment
                        if (times.Count >= 2)
                        {
                            DrawMALineSegmentBatched(maName, times, values, lineColor);
                        }
                        times.Clear();
                        values.Clear();
                    }
                }

                // Draw final segment if we have points
                if (times.Count >= 2)
                {
                    DrawMALineSegmentBatched(maName, times, values, lineColor);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error plotting {maName} line: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void DrawMALineSegmentBatched(string maName, List<DateTime> times, List<double> values, Color lineColor)
        {
            if (times.Count < 2 || values.Count < 2 || times.Count != values.Count)
                return;

            try
            {
                // Create unique ID for this line segment
                string lineId = $"{maName}_{times[0].Ticks}_{times[times.Count - 1].Ticks}";

                // Check if line already exists to prevent duplicates
                if (Chart.Objects.Any(obj => obj.Name == lineId))
                    return;

                // Draw the line segment using multiple points for smoothness
                for (int i = 0; i < times.Count - 1; i++)
                {
                    string segmentId = $"{lineId}_segment_{i}";
                    if (!Chart.Objects.Any(obj => obj.Name == segmentId))
                    {
                        // Use batching system for trend lines - pass next time point for continuous lines
                        AddToChartBatch(segmentId, ChartObjectType.TrendLine, times[i], values[i],
                                      lineColor, "", ChartIconType.Circle, values[i + 1], MarkerPriority.Low, times[i + 1]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing {maName} line segment: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void RemoveOldMarkers()
        {
            if (RemoveMarkers == RemoveMarkersMode.None || analysisBars == null || analysisBars.OpenTimes == null || analysisBars.OpenTimes.Count == 0)
                return;

            int currentIndex = analysisBars.OpenTimes.Count - 1;
            DateTime currentTime = analysisBars.OpenTimes[currentIndex];

            try
            {
                // Get all chart objects
                var chartObjects = Chart.Objects.ToList();

                // Group markers by priority for selective cleanup
                var lowPriorityMarkers = new List<ChartObject>();
                var normalPriorityMarkers = new List<ChartObject>();
                var highPriorityMarkers = new List<ChartObject>();
                var criticalMarkers = new List<ChartObject>();

                foreach (var chartObject in chartObjects)
                {
                    if (chartObject == null) continue;

                    string objectName = chartObject.Name;

                    // Check if this is a marker we created
                    bool isEntryMarker = objectName.StartsWith("Entry Long_") || objectName.StartsWith("Entry Short_") ||
                                        objectName.StartsWith("Buy_") || objectName.StartsWith("Sell_");
                    bool isExitMarker = objectName.StartsWith("Exit ") || objectName.StartsWith("TS Update_");

                    if (isEntryMarker || isExitMarker)
                    {
                        try
                        {
                            // Extract timestamp from object name
                            string timePart = "";
                            if (isEntryMarker)
                            {
                                if (objectName.Contains("Entry Long_"))
                                    timePart = objectName.Replace("Entry Long_", "");
                                else if (objectName.Contains("Entry Short_"))
                                    timePart = objectName.Replace("Entry Short_", "");
                                else if (objectName.Contains("Buy_"))
                                    timePart = objectName.Replace("Buy_", "");
                                else if (objectName.Contains("Sell_"))
                                    timePart = objectName.Replace("Sell_", "");
                            }
                            else if (isExitMarker)
                            {
                                if (objectName.Contains("Exit "))
                                {
                                    // Handle exit markers with different formats
                                    int underscoreIndex = objectName.LastIndexOf('_');
                                    if (underscoreIndex > 0)
                                        timePart = objectName.Substring(underscoreIndex + 1);
                                }
                                else if (objectName.Contains("TS Update_"))
                                    timePart = objectName.Replace("TS Update_", "");
                            }

                            if (long.TryParse(timePart, out long ticks))
                            {
                                DateTime markerTime = new DateTime(ticks);
                                TimeSpan age = currentTime - markerTime;
                                double candlesOld = age.TotalMinutes / GetTimeFrameSpan(AnalysisTimeframe).TotalMinutes;

                                if (candlesOld >= RemoveMarkersCandles)
                                {
                                    // Determine marker priority based on type and content
                                    MarkerPriority priority = DetermineMarkerPriority(objectName, isEntryMarker, isExitMarker);

                                    // Group markers by priority
                                    switch (priority)
                                    {
                                        case MarkerPriority.Low:
                                            lowPriorityMarkers.Add(chartObject);
                                            break;
                                        case MarkerPriority.Normal:
                                            normalPriorityMarkers.Add(chartObject);
                                            break;
                                        case MarkerPriority.High:
                                            highPriorityMarkers.Add(chartObject);
                                            break;
                                        case MarkerPriority.Critical:
                                            criticalMarkers.Add(chartObject);
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error processing marker {objectName}: {ex.Message}", LoggingLevel.Error);
                        }
                    }
                }

                // Perform selective cleanup based on priority and removal mode
                PerformSelectiveCleanup(lowPriorityMarkers, normalPriorityMarkers, highPriorityMarkers, criticalMarkers);
            }
            catch (Exception ex)
            {
                LogMessage($"Error in RemoveOldMarkers: {ex.Message}", LoggingLevel.Error);
            }
        }

        private MarkerPriority DetermineMarkerPriority(string objectName, bool isEntryMarker, bool isExitMarker)
        {
            // Critical markers (never remove or remove last)
            if (objectName.Contains("Exit ") && (objectName.Contains("TakeProfit") || objectName.Contains("StopLoss")))
                return MarkerPriority.Critical;

            // High priority markers (important signals)
            if (isEntryMarker && (objectName.Contains("Entry Long_") || objectName.Contains("Entry Short_")))
                return MarkerPriority.High;

            // Normal priority markers (standard signals)
            if (isExitMarker && objectName.Contains("TS Update_"))
                return MarkerPriority.Normal;

            // Low priority markers (decorative or redundant)
            if (objectName.Contains("Buy_") || objectName.Contains("Sell_") ||
                (objectName.Contains("Exit ") && objectName.Contains(" Arrow")))
                return MarkerPriority.Low;

            return MarkerPriority.Normal;
        }

        private void PerformSelectiveCleanup(List<ChartObject> lowPriority, List<ChartObject> normalPriority,
                                           List<ChartObject> highPriority, List<ChartObject> criticalPriority)
        {
            int totalMarkers = lowPriority.Count + normalPriority.Count + highPriority.Count + criticalPriority.Count;

            if (totalMarkers == 0)
                return;

            // Always preserve critical markers
            // Remove low priority markers first
            foreach (var marker in lowPriority)
            {
                if (ShouldRemoveBasedOnMode(marker.Name))
                {
                    Chart.RemoveObject(marker.Name);
                }
            }

            // Remove normal priority markers if still needed
            if (Chart.Objects.Count > 200) // Performance threshold
            {
                foreach (var marker in normalPriority)
                {
                    if (ShouldRemoveBasedOnMode(marker.Name))
                    {
                        Chart.RemoveObject(marker.Name);
                    }
                }
            }

            // Remove high priority markers only if absolutely necessary
            if (Chart.Objects.Count > 500) // Critical performance threshold
            {
                foreach (var marker in highPriority)
                {
                    if (ShouldRemoveBasedOnMode(marker.Name))
                    {
                        Chart.RemoveObject(marker.Name);
                    }
                }
            }

            // Log cleanup statistics
            LogMessage($"[MARKER_CLEANUP] Cleaned up markers - Low: {lowPriority.Count}, Normal: {normalPriority.Count}, High: {highPriority.Count}, Critical: {criticalPriority.Count} preserved", LoggingLevel.OnlyImportant);
        }

        private bool ShouldRemoveBasedOnMode(string objectName)
        {
            if (RemoveMarkers == RemoveMarkersMode.All)
            {
                return true;
            }
            else if (RemoveMarkers == RemoveMarkersMode.OnlyText)
            {
                // Only remove text objects (Buy, Sell, Exit text)
                return objectName.Contains("Buy_") || objectName.Contains("Sell_") ||
                      (objectName.Contains("Exit ") && !objectName.Contains(" Arrow"));
            }
            else if (RemoveMarkers == RemoveMarkersMode.OnlyArrows)
            {
                // Only remove arrow objects
                return objectName.Contains(" Arrow") ||
                      objectName.Contains("Entry Long_") || objectName.Contains("Entry Short_");
            }

            return false;
        }

        // ### ENDE MODUL 3 - FÜGE HIER MODUL 4 EIN ###
        // ### MODUL 4 - Adaptive MA Bot v5.6_crypto ###
        // Contains helper methods: CalculateSharpeRatio, CalculateVolume, CalculateDynamicMinVolume, CalculateDynamicMaxVolume, GetCurrentSession, IsTradingAllowed, IsNewsEvent, IsHighVolatilityTime, CalculateMa2Ma3Spread, GetMomentumThreshold, GetAtrValue, HasRsiDivergence.
        // Insert this module directly after the marker '// ### ENDE MODUL 3 - FÜGE HIER MODUL 4 EIN ###'
        // in Module 3, within the AdaptiveMABot_v5_6_crypto class, followed by Module 5 after the marker
        // '// ### ENDE MODUL 4 - FÜGE HIER MODUL 5 EIN ###'.

        private double CalculateSharpeRatio()
        {
            try
            {
                // Validate trade returns list
                if (tradeReturns == null)
                {
                    LogMessage("Warning: Trade returns list is null", LoggingLevel.Warning);
                    return 0;
                }

                if (tradeReturns.Count < 2)
                {
                    LogMessage($"Warning: Insufficient trade data for Sharpe ratio calculation. Need at least 2 trades, got {tradeReturns.Count}", LoggingLevel.Warning);
                    return 0;
                }

                // Filter out NaN and infinite values with detailed logging
                var validReturns = tradeReturns.Where(r =>
                {
                    if (double.IsNaN(r))
                    {
                        LogMessage($"Warning: NaN value found in trade returns, excluding from calculation", LoggingLevel.Warning);
                        return false;
                    }
                    if (double.IsInfinity(r))
                    {
                        LogMessage($"Warning: Infinite value found in trade returns, excluding from calculation", LoggingLevel.Warning);
                        return false;
                    }
                    return true;
                }).ToList();

                if (validReturns.Count < 2)
                {
                    LogMessage($"Warning: After filtering invalid values, insufficient data for Sharpe ratio. Valid returns: {validReturns.Count}", LoggingLevel.Warning);
                    return 0;
                }

                // Calculate average return with overflow protection
                double avgReturn;
                try
                {
                    avgReturn = validReturns.Average();
                    if (double.IsNaN(avgReturn) || double.IsInfinity(avgReturn))
                    {
                        LogMessage($"Warning: Invalid average return calculated: {avgReturn}", LoggingLevel.Warning);
                        return 0;
                    }
                }
                catch (OverflowException)
                {
                    LogMessage("Warning: Overflow occurred calculating average return", LoggingLevel.Warning);
                    return 0;
                }

                // Calculate variance with overflow protection
                double variance;
                try
                {
                    variance = validReturns.Sum(r =>
                    {
                        double diff = r - avgReturn;
                        return diff * diff;
                    }) / (validReturns.Count - 1);

                    if (double.IsNaN(variance) || double.IsInfinity(variance))
                    {
                        LogMessage($"Warning: Invalid variance calculated: {variance}", LoggingLevel.Warning);
                        return 0;
                    }

                    if (variance < 0) // Handle potential floating point precision issues
                    {
                        LogMessage($"Warning: Negative variance detected: {variance}, likely due to floating point precision issues", LoggingLevel.Warning);
                        return 0;
                    }
                }
                catch (OverflowException)
                {
                    LogMessage("Warning: Overflow occurred calculating variance", LoggingLevel.Warning);
                    return 0;
                }

                // Calculate standard deviation with overflow protection
                double stdDev;
                try
                {
                    stdDev = Math.Sqrt(variance);
                    if (double.IsNaN(stdDev) || double.IsInfinity(stdDev))
                    {
                        LogMessage($"Warning: Invalid standard deviation calculated: {stdDev}", LoggingLevel.Warning);
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Warning: Error calculating standard deviation: {ex.Message}", LoggingLevel.Warning);
                    return 0;
                }

                // Calculate Sharpe ratio with division by zero protection
                if (stdDev <= 0)
                {
                    LogMessage($"Warning: Standard deviation too small for Sharpe ratio calculation: {stdDev}", LoggingLevel.Warning);
                    return 0;
                }

                double sharpeRatio = avgReturn / stdDev;
                if (double.IsNaN(sharpeRatio) || double.IsInfinity(sharpeRatio))
                {
                    LogMessage($"Warning: Invalid Sharpe ratio calculated: {sharpeRatio}", LoggingLevel.Warning);
                    return 0;
                }

                return sharpeRatio;
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in CalculateSharpeRatio: {ex.Message}. Stack trace: {ex.StackTrace}", LoggingLevel.OnlyCritical);
                return 0;
            }
        }

        private void UpdateRollingPerformanceMetrics()
        {
            if (tradeHistory == null) return;
            if (!EnableRollingPerformance || tradeHistory.Count < 5)
                return;

            try
            {
                // Get recent trades for analysis
                var recentTrades = tradeHistory.Where(t => t.ExitTime != default(DateTime)).ToList();
                if (recentTrades.Count < 5)
                    return;

                // Calculate returns for each window
                UpdateRollingReturns(recentTrades);

                // Calculate metrics for each window
                rollingSharpe1 = CalculateRollingSharpe(rollingReturns1);
                rollingSharpe2 = CalculateRollingSharpe(rollingReturns2);
                rollingSharpe3 = CalculateRollingSharpe(rollingReturns3);

                rollingMaxDrawdown1 = CalculateRollingMaxDrawdown(rollingReturns1);
                rollingMaxDrawdown2 = CalculateRollingMaxDrawdown(rollingReturns2);
                rollingMaxDrawdown3 = CalculateRollingMaxDrawdown(rollingReturns3);

                rollingProfitFactor1 = CalculateRollingProfitFactor(recentTrades, RollingWindow1);
                rollingProfitFactor2 = CalculateRollingProfitFactor(recentTrades, RollingWindow2);
                rollingProfitFactor3 = CalculateRollingProfitFactor(recentTrades, RollingWindow3);

                rollingWinRate1 = CalculateRollingWinRate(recentTrades, RollingWindow1);
                rollingWinRate2 = CalculateRollingWinRate(recentTrades, RollingWindow2);
                rollingWinRate3 = CalculateRollingWinRate(recentTrades, RollingWindow3);

                rollingSortino1 = CalculateRollingSortino(rollingReturns1);
                rollingSortino2 = CalculateRollingSortino(rollingReturns2);
                rollingSortino3 = CalculateRollingSortino(rollingReturns3);

                rollingCalmar1 = CalculateRollingCalmar(rollingReturns1, rollingMaxDrawdown1);
                rollingCalmar2 = CalculateRollingCalmar(rollingReturns2, rollingMaxDrawdown2);
                rollingCalmar3 = CalculateRollingCalmar(rollingReturns3, rollingMaxDrawdown3);

                // Update regime-specific metrics
                UpdateRegimeMetrics(recentTrades);

                // Analyze performance trend
                performanceTrend = AnalyzePerformanceTrend(recentTrades);

                LogMessage($"[ROLLING_PERF] Updated metrics - Sharpe: {rollingSharpe1:F2}/{rollingSharpe2:F2}/{rollingSharpe3:F2}, WinRate: {rollingWinRate1:P1}/{rollingWinRate2:P1}/{rollingWinRate3:P1}", LoggingLevel.OnlyImportant);
            }
            catch (Exception ex)
            {
                LogMessage($"Error updating rolling performance metrics: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void UpdateRollingReturns(List<TradeLearningData> recentTrades)
        {
            rollingReturns1.Clear();
            rollingReturns2.Clear();
            rollingReturns3.Clear();

            int count = recentTrades.Count;
            for (int i = Math.Max(0, count - RollingWindow3); i < count; i++)
            {
                double returnPct = recentTrades[i].ReturnPercentage;
                if (i >= count - RollingWindow1)
                    rollingReturns1.Add(returnPct);
                if (i >= count - RollingWindow2)
                    rollingReturns2.Add(returnPct);
                rollingReturns3.Add(returnPct);
            }
        }

        private double CalculateRollingSharpe(List<double> returns)
        {
            if (returns.Count < 2) return 0;

            double avgReturn = returns.Average();
            double variance = returns.Sum(r => Math.Pow(r - avgReturn, 2)) / (returns.Count - 1);
            double stdDev = Math.Sqrt(variance);

            return stdDev > 0 ? avgReturn / stdDev : 0;
        }

        private double CalculateRollingMaxDrawdown(List<double> returns)
        {
            if (returns.Count < 2) return 0;

            double peak = 0;
            double maxDrawdown = 0;
            double runningTotal = 0;

            foreach (double ret in returns)
            {
                runningTotal += ret;
                if (runningTotal > peak)
                    peak = runningTotal;
                double drawdown = peak - runningTotal;
                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }

            return maxDrawdown;
        }

        private double CalculateRollingProfitFactor(List<TradeLearningData> trades, int window)
        {
            if (trades.Count < window) return 0;

            var windowTrades = trades.Skip(Math.Max(0, trades.Count - window)).ToList();
            double grossProfit = windowTrades.Where(t => t.ProfitLoss > 0).Sum(t => t.ProfitLoss);
            double grossLoss = Math.Abs(windowTrades.Where(t => t.ProfitLoss < 0).Sum(t => t.ProfitLoss));

            return grossLoss > 0 ? grossProfit / grossLoss : (grossProfit > 0 ? double.PositiveInfinity : 0);
        }

        private double CalculateRollingWinRate(List<TradeLearningData> trades, int window)
        {
            if (trades.Count < window) return 0;

            var windowTrades = trades.Skip(Math.Max(0, trades.Count - window)).ToList();
            return (double)windowTrades.Count(t => t.IsWin) / windowTrades.Count;
        }

        private double CalculateRollingSortino(List<double> returns)
        {
            if (returns.Count < 2) return 0;

            double avgReturn = returns.Average();
            var negativeReturns = returns.Where(r => r < 0).ToList();
            if (negativeReturns.Count == 0) return double.PositiveInfinity;

            double downsideVariance = negativeReturns.Sum(r => Math.Pow(r - avgReturn, 2)) / negativeReturns.Count;
            double downsideStdDev = Math.Sqrt(downsideVariance);

            return downsideStdDev > 0 ? avgReturn / downsideStdDev : 0;
        }

        private double CalculateRollingCalmar(List<double> returns, double maxDrawdown)
        {
            if (returns.Count < 2 || maxDrawdown <= 0) return 0;

            double avgReturn = returns.Average();
            return maxDrawdown > 0 ? avgReturn / maxDrawdown : 0;
        }

        private void UpdateRegimeMetrics(List<TradeLearningData> trades)
        {
            foreach (var regime in new[] { MarketRegime.Trending, MarketRegime.Ranging, MarketRegime.HighVolatility, MarketRegime.LowVolatility })
            {
                var regimeTrades = trades.Where(t => t.RegimeAtEntry == regime).ToList();
                if (regimeTrades.Count > 0)
                {
                    var returns = regimeTrades.Select(t => t.ReturnPercentage).ToList();
                    regimeReturns[regime] = returns;
                    regimeSharpe[regime] = CalculateRollingSharpe(returns);
                    regimeWinRate[regime] = (double)regimeTrades.Count(t => t.IsWin) / regimeTrades.Count;
                }
                else
                {
                    regimeReturns[regime] = new List<double>();
                    regimeSharpe[regime] = 0;
                    regimeWinRate[regime] = 0;
                }
            }
        }

        private string AnalyzePerformanceTrend(List<TradeLearningData> trades)
        {
            if (trades.Count < 10) return "Insufficient Data";

            // Compare recent performance to earlier performance
            int halfPoint = trades.Count / 2;
            var earlyTrades = trades.Take(halfPoint).ToList();
            var recentTrades = trades.Skip(halfPoint).ToList();

            double earlyWinRate = (double)earlyTrades.Count(t => t.IsWin) / earlyTrades.Count;
            double recentWinRate = (double)recentTrades.Count(t => t.IsWin) / recentTrades.Count;

            double earlyAvgReturn = earlyTrades.Average(t => t.ReturnPercentage);
            double recentAvgReturn = recentTrades.Average(t => t.ReturnPercentage);

            if (recentWinRate > earlyWinRate + 0.1 && recentAvgReturn > earlyAvgReturn + 0.5)
                return "Improving";
            else if (recentWinRate < earlyWinRate - 0.1 && recentAvgReturn < earlyAvgReturn - 0.5)
                return "Declining";
            else
                return "Stable";
        }

        // Parameter Correlation Analysis Methods
        private double CalculatePearsonCorrelation(List<double> x, List<double> y)
        {
            if (x.Count != y.Count || x.Count < 2)
                return 0;

            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = 0;
            double sumX2 = 0;
            double sumY2 = 0;
            int n = x.Count;

            for (int i = 0; i < n; i++)
            {
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
                sumY2 += y[i] * y[i];
            }

            double numerator = n * sumXY - sumX * sumY;
            double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));

            return denominator == 0 ? 0 : numerator / denominator;
        }

        private void UpdateCorrelationData(Position position)
        {
            if (!EnableCorrelationAnalysis || tradeHistory.Count < MinimumSampleRequirements)
                return;

            try
            {
                var latestTrade = tradeHistory.LastOrDefault(t => t.ExitTime != default(DateTime));
                if (latestTrade == null) return;

                // Add correlation data points for key parameters
                correlationData.Add(new ParameterCorrelationData
                {
                    ParameterName = "ATR_Threshold",
                    ParameterValue = latestTrade.AtrThresholdAtEntry,
                    PerformanceMetric = latestTrade.ReturnPercentage,
                    Regime = latestTrade.RegimeAtEntry,
                    Timestamp = latestTrade.ExitTime
                });

                correlationData.Add(new ParameterCorrelationData
                {
                    ParameterName = "RSI_Buy_Threshold",
                    ParameterValue = latestTrade.RsiBuyThresholdAtEntry,
                    PerformanceMetric = latestTrade.ReturnPercentage,
                    Regime = latestTrade.RegimeAtEntry,
                    Timestamp = latestTrade.ExitTime
                });

                correlationData.Add(new ParameterCorrelationData
                {
                    ParameterName = "Momentum_Threshold",
                    ParameterValue = latestTrade.MomentumThresholdAtEntry,
                    PerformanceMetric = latestTrade.ReturnPercentage,
                    Regime = latestTrade.RegimeAtEntry,
                    Timestamp = latestTrade.ExitTime
                });

                // Maintain window size
                if (correlationData.Count > CorrelationAnalysisWindowSize)
                {
                    correlationData.RemoveRange(0, correlationData.Count - CorrelationAnalysisWindowSize);
                }

                // Update correlation matrix
                UpdateCorrelationMatrix();
            }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error updating correlation data: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void UpdateCorrelationMatrix()
        {
            if (correlationData.Count < MinimumSampleRequirements)
                return;

            try
            {
                // Group data by parameter
                var parameterGroups = correlationData.GroupBy(d => d.ParameterName)
                    .Where(g => g.Count() >= MinimumSampleRequirements)
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (parameterGroups.Count < 2)
                    return;

                // Calculate correlations between all parameter pairs
                var parameterNames = parameterGroups.Keys.ToList();
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    for (int j = i + 1; j < parameterNames.Count; j++)
                    {
                        string param1 = parameterNames[i];
                        string param2 = parameterNames[j];

                        var data1 = parameterGroups[param1];
                        var data2 = parameterGroups[param2];

                        // Find matching data points (same timestamp and regime)
                        var matchedData = data1.Join(data2,
                            d1 => new { d1.Timestamp, d1.Regime },
                            d2 => new { d2.Timestamp, d2.Regime },
                            (d1, d2) => new { Param1 = d1.ParameterValue, Param2 = d2.ParameterValue })
                            .ToList();

                        if (matchedData.Count >= MinimumSampleRequirements)
                        {
                            var values1 = matchedData.Select(m => m.Param1).ToList();
                            var values2 = matchedData.Select(m => m.Param2).ToList();

                            double correlation = CalculatePearsonCorrelation(values1, values2);

                            // Store in global matrix
                            if (!parameterMatrix.Correlations.ContainsKey(param1))
                                parameterMatrix.Correlations[param1] = new Dictionary<string, double>();
                            parameterMatrix.Correlations[param1][param2] = correlation;

                            if (!parameterMatrix.Correlations.ContainsKey(param2))
                                parameterMatrix.Correlations[param2] = new Dictionary<string, double>();
                            parameterMatrix.Correlations[param2][param1] = correlation;

                            parameterMatrix.SampleSizes[$"{param1}_{param2}"] = matchedData.Count;
                        }
                    }
                }

                parameterMatrix.LastUpdated = Server.Time;

                // Update regime-specific matrices
                UpdateRegimeSpecificCorrelations();
            }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error updating correlation matrix: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void UpdateRegimeSpecificCorrelations()
        {
            var regimes = correlationData.Select(d => d.Regime).Distinct();

            foreach (var regime in regimes)
            {
                if (!regimeMatrices.ContainsKey(regime))
                    regimeMatrices[regime] = new CorrelationMatrix();

                var regimeData = correlationData.Where(d => d.Regime == regime).ToList();
                if (regimeData.Count < MinimumSampleRequirements)
                    continue;

                var parameterGroups = regimeData.GroupBy(d => d.ParameterName)
                    .Where(g => g.Count() >= MinimumSampleRequirements)
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (parameterGroups.Count < 2)
                    continue;

                var parameterNames = parameterGroups.Keys.ToList();
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    for (int j = i + 1; j < parameterNames.Count; j++)
                    {
                        string param1 = parameterNames[i];
                        string param2 = parameterNames[j];

                        var data1 = parameterGroups[param1];
                        var data2 = parameterGroups[param2];

                        var matchedData = data1.Join(data2,
                            d1 => d1.Timestamp,
                            d2 => d2.Timestamp,
                            (d1, d2) => new { Param1 = d1.ParameterValue, Param2 = d2.ParameterValue })
                            .ToList();

                        if (matchedData.Count >= MinimumSampleRequirements)
                        {
                            var values1 = matchedData.Select(m => m.Param1).ToList();
                            var values2 = matchedData.Select(m => m.Param2).ToList();

                            double correlation = CalculatePearsonCorrelation(values1, values2);

                            if (!regimeMatrices[regime].Correlations.ContainsKey(param1))
                                regimeMatrices[regime].Correlations[param1] = new Dictionary<string, double>();
                            regimeMatrices[regime].Correlations[param1][param2] = correlation;

                            if (!regimeMatrices[regime].Correlations.ContainsKey(param2))
                                regimeMatrices[regime].Correlations[param2] = new Dictionary<string, double>();
                            regimeMatrices[regime].Correlations[param2][param1] = correlation;

                            regimeMatrices[regime].SampleSizes[$"{param1}_{param2}"] = matchedData.Count;
                        }
                    }
                }

                regimeMatrices[regime].LastUpdated = Server.Time;
            }
        }

        private void GenerateOptimizationSuggestions()
        {
            if (!EnableCorrelationAnalysis || correlationData.Count < MinimumSampleRequirements)
                return;

            optimizationSuggestions.Clear();

            try
            {
                // Analyze parameter-performance correlations
                var parameterPerformanceData = correlationData.GroupBy(d => d.ParameterName)
                    .Where(g => g.Count() >= MinimumSampleRequirements)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var paramData in parameterPerformanceData)
                {
                    string paramName = paramData.Key;
                    var data = paramData.Value;

                    var paramValues = data.Select(d => d.ParameterValue).ToList();
                    var performanceValues = data.Select(d => d.PerformanceMetric).ToList();

                    double correlation = CalculatePearsonCorrelation(paramValues, performanceValues);

                    // Generate suggestions based on correlation strength
                    if (Math.Abs(correlation) > OptimizationCorrelationThreshold) // Significant correlation
                    {
                        var currentValue = GetCurrentParameterValue(paramName);
                        var suggestedValue = CalculateOptimalParameterValue(paramName, correlation, data);

                        if (Math.Abs(suggestedValue - currentValue) > GetParameterTolerance(paramName))
                        {
                            optimizationSuggestions.Add(new ParameterOptimizationSuggestion
                            {
                                ParameterName = paramName,
                                CurrentValue = currentValue,
                                SuggestedValue = suggestedValue,
                                ExpectedImprovement = Math.Abs(correlation) * 10, // Rough estimate
                                Reasoning = $"{paramName} shows {(correlation > 0 ? "positive" : "negative")} correlation ({correlation:F2}) with performance",
                                TargetRegime = MarketRegime.Ranging // Default, could be made regime-specific
                            });
                        }
                    }
                    }
                // Sort by expected improvement
                optimizationSuggestions = optimizationSuggestions.OrderByDescending(s => s.ExpectedImprovement).ToList();
                }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error generating optimization suggestions: {ex.Message}", LoggingLevel.Error);
            }
        }

        private double GetCurrentParameterValue(string paramName)
        {
            return paramName switch
            {
                "ATR_Threshold" => AtrThreshold,
                "RSI_Buy_Threshold" => RsiBuyThreshold,
                "Momentum_Threshold" => Ma1Momentum,
                _ => 0
            };
        }

        private double CalculateOptimalParameterValue(string paramName, double correlation, List<ParameterCorrelationData> data)
        {
            // Simple optimization: find parameter value with highest average performance
            var groupedByValue = data.GroupBy(d => Math.Round(d.ParameterValue, 4))
                .Select(g => new
                {
                    Value = g.Key,
                    AvgPerformance = g.Average(d => d.PerformanceMetric),
                    Count = g.Count()
                })
                .Where(x => x.Count >= 3) // Minimum samples per value
                .OrderByDescending(x => x.AvgPerformance)
                .FirstOrDefault();

            return groupedByValue?.Value ?? GetCurrentParameterValue(paramName);
        }

        private double GetParameterTolerance(string paramName)
        {
            return paramName switch
            {
                "ATR_Threshold" => 0.0005,
                "RSI_Buy_Threshold" => 2.0,
                "Momentum_Threshold" => 0.05,
                _ => 0.01
            };
        }

        private void UpdateRollingCorrelations()
        {
            if (!EnableCorrelationAnalysis || correlationData.Count < MinimumSampleRequirements)
                return;

            try
            {
                // Calculate rolling correlation between ATR threshold and performance with sampling
                var allData = correlationData.Where(d => d.ParameterName == "ATR_Threshold")
                    .OrderByDescending(d => d.Timestamp)
                    .ToList();

                // Apply sampling to reduce data points
                var sampledData = new List<ParameterCorrelationData>();
                for (int i = 0; i < allData.Count; i += CorrelationSamplingRate)
                {
                    sampledData.Add(allData[i]);
                }

                // Take the window size from sampled data
                var recentData = sampledData.Take(CorrelationAnalysisWindowSize).ToList();

                if (recentData.Count >= MinimumSampleRequirements)
                {
                    var paramValues = recentData.Select(d => d.ParameterValue).ToList();
                    var performanceValues = recentData.Select(d => d.PerformanceMetric).ToList();

                    double correlation = CalculatePearsonCorrelation(paramValues, performanceValues);

                    if (!double.IsNaN(correlation) && !double.IsInfinity(correlation))
                    {
                        rollingCorrelations.Enqueue(correlation);
                        if (rollingCorrelations.Count > CorrelationAnalysisWindowSize)
                            rollingCorrelations.Dequeue();
                    }

                    LogMessage($"[CORRELATION] Updated rolling correlations - Sampled {sampledData.Count} points (rate: {CorrelationSamplingRate}), Window: {recentData.Count}, Correlation: {correlation:F3}", LoggingLevel.OnlyImportant);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error updating rolling correlations: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void ApplyOptimizationSuggestion(ParameterOptimizationSuggestion suggestion)
        {
            try
            {
                double oldValue = GetCurrentParameterValue(suggestion.ParameterName);
                double newValue = suggestion.SuggestedValue;

                // Apply bounds and constraints
                newValue = ApplyParameterBounds(suggestion.ParameterName, newValue);

                // Only apply if change is significant
                if (Math.Abs(newValue - oldValue) > GetParameterTolerance(suggestion.ParameterName))
                {
                    SetParameterValue(suggestion.ParameterName, newValue);
                    LogMessage($"[CORRELATION] Applied optimization: {suggestion.ParameterName} {oldValue:F4} → {newValue:F4} ({suggestion.Reasoning})", LoggingLevel.Info);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error applying optimization suggestion: {ex.Message}", LoggingLevel.Error);
            }
        }

        private double ApplyParameterBounds(string paramName, double value)
        {
            return paramName switch
            {
                "ATR_Threshold" => Math.Max(ATRThresholdMin, Math.Min(ATRThresholdMax, value)),
                "RSI_Buy_Threshold" => Math.Max(RSIBuyThresholdMin, Math.Min(RSIBuyThresholdMax, value)),
                "RSI_Sell_Threshold" => Math.Max(RSISellThresholdMin, Math.Min(RSISellThresholdMax, value)),
                "Momentum_Threshold" => Math.Max(MomentumThresholdMin, Math.Min(MomentumThresholdMax, value)),
                _ => value
            };
        }

        private void SetParameterValue(string paramName, double value)
        {
            switch (paramName)
            {
                case "ATR_Threshold":
                    AtrThreshold = value;
                    break;
                case "RSI_Buy_Threshold":
                    RsiBuyThreshold = value;
                    break;
                case "Momentum_Threshold":
                    Ma1Momentum = value;
                    break;
                }
            }

        private void LogCorrelationResults()
        {
            if (!EnableCorrelationAnalysis || correlationData.Count < MinimumSampleRequirements)
                return;

            try
            {
                LogMessage($"[CORRELATION] Analysis Results - Total Sample Size: {correlationData.Count}, Sampling Rate: {CorrelationSamplingRate}, Analysis Frequency: {AnalysisFrequencyBars} bars", LoggingLevel.Info);

                // Log parameter correlations
                if (parameterMatrix.Correlations.Count > 0)
                {
                    LogMessage($"[CORRELATION] Parameter Correlations (Sampled):", LoggingLevel.Info);
                    foreach (var param1 in parameterMatrix.Correlations.Keys.OrderBy(k => k))
                    {
                        foreach (var param2 in parameterMatrix.Correlations[param1].Keys.OrderBy(k => k))
                        {
                            if (string.Compare(param1, param2) < 0) // Avoid duplicate logging
                            {
                                double correlation = parameterMatrix.Correlations[param1][param2];
                                int sampleSize = parameterMatrix.SampleSizes.GetValueOrDefault($"{param1}_{param2}", 0);
                                LogMessage($"[CORRELATION] {param1} ↔ {param2}: {correlation:F3} (n={sampleSize}, sampled)", LoggingLevel.Info);
                            }
                        }
                    }
                }

                // Log regime-specific correlations
                foreach (var regime in regimeMatrices.Keys)
                {
                    if (regimeMatrices[regime].Correlations.Count > 0)
                    {
                        LogMessage($"[CORRELATION] {regime} Regime Correlations (Sampled):", LoggingLevel.Info);
                        foreach (var param1 in regimeMatrices[regime].Correlations.Keys.OrderBy(k => k))
                        {
                            foreach (var param2 in regimeMatrices[regime].Correlations[param1].Keys.OrderBy(k => k))
                            {
                                if (string.Compare(param1, param2) < 0)
                                {
                                    double correlation = regimeMatrices[regime].Correlations[param1][param2];
                                    int sampleSize = regimeMatrices[regime].SampleSizes.GetValueOrDefault($"{param1}_{param2}", 0);
                                    LogMessage($"[CORRELATION] {param1} ↔ {param2}: {correlation:F3} (n={sampleSize}, sampled)", LoggingLevel.Info);
                                }
                            }
                        }
                    }
                }

                // Log optimization suggestions
                if (optimizationSuggestions.Count > 0)
                {
                    LogMessage($"[CORRELATION] Optimization Suggestions (based on sampled data):", LoggingLevel.Info);
                    foreach (var suggestion in optimizationSuggestions.Take(3)) // Top 3 suggestions
                    {
                        LogMessage($"[CORRELATION] {suggestion.ParameterName}: {suggestion.CurrentValue:F4} → {suggestion.SuggestedValue:F4} ({suggestion.Reasoning})", LoggingLevel.Info);
                    }
                }

                // Log rolling correlation trend with performance info
                if (rollingCorrelations.Count > 1)
                {
                    double avgCorrelation = rollingCorrelations.Average();
                    double latestCorrelation = rollingCorrelations.Last();
                    double correlationVariance = rollingCorrelations.Sum(c => Math.Pow(c - avgCorrelation, 2)) / rollingCorrelations.Count;
                    LogMessage($"[CORRELATION] Rolling Correlation (ATR-Performance): Avg={avgCorrelation:F3}, Latest={latestCorrelation:F3}, Variance={correlationVariance:F4}, Window={rollingCorrelations.Count}", LoggingLevel.Info);
                }

                // Log performance metrics for analysis operations
                LogMessage($"[CORRELATION] Performance: Analysis runs every {AnalysisFrequencyBars} bars, Data sampling rate: {CorrelationSamplingRate}x", LoggingLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"[CORRELATION] Error logging results: {ex.Message}", LoggingLevel.Error);
            }
        }

        private double CalculateVolume(double atrValue = 0)
        {
            // Validate account and symbol
            if (Account == null || Account.Balance <= 0)
            {
                LogMessage("Warning: Invalid account balance, cannot calculate volume", LoggingLevel.Warning);
                return 0;
            }
            if (Symbol == null || Symbol.PipSize <= 0)
            {
                LogMessage("Warning: Invalid symbol or pip size, cannot calculate volume", LoggingLevel.Warning);
                return 0;
            }

            // Calculate risk amount based on risk management mode
            double riskAmount;
            if (RiskMode == RiskManagementMode.DynamicATR)
            {
                // Dynamic mode: Risk amount = ATR * ATR Multiplier Risk
                double effectiveAtr = atrValue != 0 ? atrValue : (atr != null && atr.Count > 0 ? atr.LastValue : AtrThreshold);
                if (double.IsNaN(effectiveAtr) || effectiveAtr <= 0)
                {
                    LogMessage("Warning: Invalid ATR value for dynamic risk calculation, falling back to static mode", LoggingLevel.Warning);
                    riskAmount = Account.Balance * (RiskPercentage / PERCENTAGE_MULTIPLIER);
                }
                else
                {
                    riskAmount = effectiveAtr * AtrMultiplierRisk;
                }
            }
            else
            {
                // Static mode: Risk amount = Account Balance * Risk Percentage
                riskAmount = Account.Balance * (RiskPercentage / PERCENTAGE_MULTIPLIER);
            }

            // Cap risk amount to prevent excessive account exposure
            double maxRiskAmount = Account.Balance * MAX_RISK_PERCENTAGE;
            if (riskAmount > maxRiskAmount)
            {
                riskAmount = maxRiskAmount;
                LogMessage("Warning: Risk amount exceeded " + (MAX_RISK_PERCENTAGE * 100) + "% of account balance, capping risk", LoggingLevel.Warning);
            }

            if (riskAmount <= 0)
            {
                LogMessage("Warning: Risk amount is zero or negative, cannot calculate volume", LoggingLevel.Warning);
                return 0;
            }

            // Calculate stop loss distance
            double stopLossDistance;
            if (RiskMode == RiskManagementMode.DynamicATR)
            {
                double effectiveAtr = atrValue != 0 ? atrValue : (atr != null && atr.Count > 0 ? atr.LastValue : AtrThreshold);
                if (double.IsNaN(effectiveAtr) || effectiveAtr <= 0)
                {
                    LogMessage("Warning: Invalid ATR value, using static stop loss", LoggingLevel.Warning);
                    stopLossDistance = StopLossPips * Symbol.PipSize;
                }
                else
                {
                    stopLossDistance = effectiveAtr * AtrMultiplierSl;
                }
            }
            else
            {
                stopLossDistance = StopLossPips * Symbol.PipSize;
            }

            if (stopLossDistance <= 0)
            {
                LogMessage("Warning: Stop loss distance is zero or negative, using minimum fallback value", LoggingLevel.Warning);
                stopLossDistance = 0.00001; // Minimum stop loss distance fallback
            }

            // Calculate volume: Volume = Risk Amount / Stop Loss Distance
            double volumeInUnits = riskAmount / stopLossDistance;

            // Cap volume to prevent broker limit violations
            if (volumeInUnits > MAX_VOLUME_LIMIT)
            {
                volumeInUnits = MAX_VOLUME_LIMIT;
                LogMessage("Warning: Calculated volume exceeded maximum of " + MAX_VOLUME_LIMIT + " lots, capping volume", LoggingLevel.Warning);
            }

            // Apply ATR position sizing factor if enabled (separate from risk calculation)
            if (AtrMode == AtrFilterMode.PositionSizing && UseAtrFilter)
            {
                double effectiveAtr = atrValue != 0 ? atrValue : (atr != null && atr.Count > 0 ? atr.LastValue : AtrThreshold);
                if (double.IsNaN(effectiveAtr) || effectiveAtr <= 0)
                {
                    effectiveAtr = AtrThreshold;
                }
                volumeInUnits *= AtrPositionSizingFactor / effectiveAtr;
            }

            if (volumeInUnits <= 0)
            {
                LogMessage("Warning: Calculated volume is zero or negative", LoggingLevel.Warning);
                return 0;
            }

            double normalizedVolume = Symbol.NormalizeVolumeInUnits(volumeInUnits, RoundingMode.ToNearest);
            return normalizedVolume;
        }

        private double CalculateDynamicMinVolume(int index)
        {
            if (index < 0) return MinVolume;
            var currentDay = analysisBars.OpenTimes[index].Date;
            double volumeSum = 0;
            int candleCount = 0;
            for (int i = index; i >= 0; i--)
            {
                if (analysisBars.OpenTimes[i].Date < currentDay)
                    break;
                volumeSum += analysisBars.TickVolumes[i];
                candleCount++;
            }
            double avgDailyVolume = candleCount > 0 ? volumeSum / candleCount : 0;
            return avgDailyVolume * VolumeDynamicFactor;
        }

        private double CalculateDynamicMaxVolume(int index)
        {
            if (index < 0) return MaxVolume;
            var currentDay = analysisBars.OpenTimes[index].Date;
            double volumeSum = 0;
            int candleCount = 0;
            for (int i = index; i >= 0; i--)
            {
                if (analysisBars.OpenTimes[i].Date < currentDay)
                    break;
                volumeSum += analysisBars.TickVolumes[i];
                candleCount++;
            }
            double avgDailyVolume = candleCount > 0 ? volumeSum / candleCount : 0;
            return avgDailyVolume * VolumeDynamicMaxFactor;
        }

        private string GetCurrentSession()
        {
            var currentTime = Server.Time;
            var timeOfDay = currentTime.TimeOfDay;
            var dayOfWeek = currentTime.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                return "None";
            if (timeOfDay >= LONDON_OPEN && timeOfDay < LONDON_CLOSE)
                return "London";
            if (timeOfDay >= NEWYORK_OPEN && timeOfDay < NEWYORK_CLOSE)
                return "NewYork";
            if (timeOfDay >= TOKYO_OPEN && timeOfDay < TOKYO_CLOSE)
                return "Tokyo";
            if (timeOfDay >= SYDNEY_OPEN || timeOfDay < SYDNEY_CLOSE)
                return "Sydney";
            if (timeOfDay >= LONDON_NEWYORK_OVERLAP_START && timeOfDay < LONDON_NEWYORK_OVERLAP_END)
                return "LondonNewYorkOverlap";
            return "None";
        }

        private bool IsTradingAllowed()
        {
            if (TradingSession == TradingSessionMode.All) return true;
            var currentSession = GetCurrentSession();
            return currentSession switch
            {
                "London" => TradingSession == TradingSessionMode.London || TradingSession == TradingSessionMode.LondonNewYorkOverlap,
                "NewYork" => TradingSession == TradingSessionMode.NewYork || TradingSession == TradingSessionMode.LondonNewYorkOverlap,
                "Tokyo" => TradingSession == TradingSessionMode.Tokyo,
                "Sydney" => TradingSession == TradingSessionMode.Sydney,
                "LondonNewYorkOverlap" => TradingSession == TradingSessionMode.LondonNewYorkOverlap,
                _ => false
            };
        }




        private bool IsNewsEvent()
        {
            var currentTime = Server.Time;
            var timeOfDay = currentTime.TimeOfDay;
            var dayOfWeek = currentTime.DayOfWeek;
            // Example: US Non-Farm Payrolls (NFP) first Friday of month at 13:30 UTC
            if (dayOfWeek == DayOfWeek.Friday && currentTime.Day <= 7 && timeOfDay >= NEWS_EVENT_START && timeOfDay < NEWS_EVENT_END)
                return true;
            // Example: ECB Interest Rate Decision (Thursday, irregular, ~13:15 UTC)
            if (dayOfWeek == DayOfWeek.Thursday && timeOfDay >= NEWS_EVENT_START && timeOfDay < NEWS_EVENT_END)
                return true;
            return false;
        }

        private bool IsHighVolatilityTime()
        {
            var currentTime = Server.Time;
            var timeOfDay = currentTime.TimeOfDay;
            var dayOfWeek = currentTime.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) return false;
            // High volatility at session starts: London (08:00), NewYork (13:00), Tokyo (00:00)
            return (timeOfDay >= LONDON_VOLATILITY_START && timeOfDay < LONDON_VOLATILITY_END) ||
                   (timeOfDay >= NEWYORK_VOLATILITY_START && timeOfDay < NEWYORK_VOLATILITY_END) ||
                   (timeOfDay >= TOKYO_VOLATILITY_START || timeOfDay < TOKYO_VOLATILITY_END);
        }

        private double CalculateMa2Ma3Spread(int index)
        {
            // Bounds checking to prevent IndexOutOfRangeException
            if (index < 0 || index >= analysisBars.ClosePrices.Count || index >= ma2.Count || index >= ma3.Count)
            {
                LogMessage($"MA2/MA3 Spread calculation failed: Index {index} out of bounds (ClosePrices: {analysisBars.ClosePrices.Count}, MA2: {ma2.Count}, MA3: {ma3.Count})", LoggingLevel.OnlyCritical);
                return 0;
            }

            int requiredBars = Math.Max(Ma2Period, Ma3Period);
            if (index < requiredBars)
            {
                LogMessage($"MA2/MA3 Spread calculation failed at index {index}: Insufficient bars, required={requiredBars}", LoggingLevel.OnlyCritical);
                return 0;
            }

            // Validate data series
            if (ma2 == null || ma3 == null || analysisBars == null || analysisBars.ClosePrices == null)
            {
                LogMessage($"MA2/MA3 Spread calculation failed: Data series not initialized", LoggingLevel.OnlyCritical);
                return 0;
            }

            double ma2Value = GetCachedMa2Value(index);
            double ma3Value = GetCachedMa3Value(index);
            double closePrice = analysisBars.ClosePrices[index];

            if (double.IsNaN(ma2Value) || double.IsNaN(ma3Value) || double.IsNaN(closePrice) || closePrice <= 0)
            {
                LogMessage($"MA2/MA3 Spread calculation failed at index {index}: MA2={ma2Value:F5}, MA3={ma3Value:F5}, ClosePrice={closePrice:F5}", LoggingLevel.OnlyCritical);
                return 0;
            }

            if (ma2Value == ma3Value)
            {
                LogMessage($"Warning: MA2 and MA3 are identical at index {index}: MA2={ma2Value:F5}, MA3={ma3Value:F5}, MA2Type={Ma2Type}, MA2Period={Ma2Period}, MA3Type={Ma3Type}, MA3Period={Ma3Period}", LoggingLevel.Warning);
                return 0;
            }

            // Prevent division by zero or very small numbers
            if (Math.Abs(closePrice) < 1e-8)
            {
                LogMessage($"Warning: Close price too small for spread calculation at index {index}: {closePrice:F8}", LoggingLevel.Warning);
                return 0;
            }

            double spread = Math.Abs(ma2Value - ma3Value) / closePrice * 100;
            LogMessage($"MA2/MA3 Spread calculated at index {index}: MA2={ma2Value:F5}, MA3={ma3Value:F5}, ClosePrice={closePrice:F5}, Spread={spread:F2}%", LoggingLevel.Info);
            return spread;
        }

        private double GetMomentumThreshold(int index)
        {
            if (MaMomentumMode == MomentumMode.Dynamic)
            {
                if (index < 0 || index >= atr.Count || double.IsNaN(atr[index]))
                {
                    LogMessage($"Warning: Invalid ATR value at index {index}, using static momentum threshold", LoggingLevel.Warning);
                    return Ma1Momentum;
                }
                return atr[index] * Ma1MomentumAtrFactor * 100;
            }
            return Ma1Momentum;
        }

        private double GetAtrValue(int index)
        {
            if (AtrMode == AtrFilterMode.MultiTimeframe)
            {
                if (multiBars == null || multiBars.ClosePrices == null || multiBars.ClosePrices.Count == 0)
                {
                    LogMessage("Warning: Multi-timeframe bars not available, using main timeframe ATR", LoggingLevel.Warning);
                    return GetAtrValueMain(index);
                }
                int multiIndex = multiBars.ClosePrices.Count - 1;
                if (multiIndex < 0 || multiAtr == null || multiIndex >= multiAtr.Count || double.IsNaN(multiAtr[multiIndex]))
                {
                    LogMessage("Warning: Invalid multi-timeframe ATR value, using main timeframe ATR", LoggingLevel.Warning);
                    return GetAtrValueMain(index);
                }
                return multiAtr[multiIndex];
            }
            return GetAtrValueMain(index);
        }

        private double GetAtrValueMain(int index)
        {
            if (index < 0 || index >= atr.Count || double.IsNaN(atr[index]))
            {
                LogMessage($"Warning: Invalid ATR value at index {index}, using default ATR threshold", LoggingLevel.Warning);
                return AtrThreshold;
            }
            return atr[index];
        }

        private bool HasRsiDivergence(int index, TradeType tradeType)
        {
            // Bounds checking to prevent IndexOutOfRangeException
            if (index < 0 || RsiDivergenceLookback <= 0 || RsiDivergenceLookback > index + 1) return false;
            if (rsi == null || rsi.Result == null || rsi.Result.Count == 0) return false;
            if (analysisBars == null || analysisBars.LowPrices == null || analysisBars.HighPrices == null) return false;
            if (index >= analysisBars.LowPrices.Count || index >= analysisBars.HighPrices.Count || index >= rsi.Result.Count) return false;

            if (tradeType == TradeType.Buy)
            {
                // Bullish divergence: Price makes lower low, RSI makes higher low
                double priceLow = double.MaxValue;
                int priceLowIndex = -1;
                double rsiLow = double.MaxValue;
                int rsiLowIndex = -1;
                for (int i = 0; i < RsiDivergenceLookback; i++)
                {
                    int checkIndex = index - i;
                    if (checkIndex < 0 || checkIndex >= analysisBars.LowPrices.Count || checkIndex >= rsi.Result.Count) continue;
                    if (double.IsNaN(analysisBars.LowPrices[checkIndex]) || double.IsNaN(rsi.Result[checkIndex])) continue;

                    if (analysisBars.LowPrices[checkIndex] < priceLow)
                    {
                        priceLow = analysisBars.LowPrices[checkIndex];
                        priceLowIndex = checkIndex;
                    }
                    if (rsi.Result[checkIndex] < rsiLow)
                    {
                        rsiLow = rsi.Result[checkIndex];
                        rsiLowIndex = checkIndex;
                    }
                }
                if (priceLowIndex < 0 || rsiLowIndex < 0 || rsiLowIndex - 1 < 0 || rsiLowIndex - 1 >= rsi.Result.Count) return false;
                if (rsiLowIndex >= rsi.Result.Count) return false;
                if (double.IsNaN(rsi.Result[rsiLowIndex]) || double.IsNaN(rsi.Result[rsiLowIndex - 1])) return false;
                return priceLowIndex == index && rsi.Result[rsiLowIndex] > rsi.Result[rsiLowIndex - 1];
            }
            else
            {
                // Bearish divergence: Price makes higher high, RSI makes lower high
                double priceHigh = double.MinValue;
                int priceHighIndex = -1;
                double rsiHigh = double.MinValue;
                int rsiHighIndex = -1;
                for (int i = 0; i < RsiDivergenceLookback; i++)
                {
                    int checkIndex = index - i;
                    if (checkIndex < 0 || checkIndex >= analysisBars.HighPrices.Count || checkIndex >= rsi.Result.Count) continue;
                    if (double.IsNaN(analysisBars.HighPrices[checkIndex]) || double.IsNaN(rsi.Result[checkIndex])) continue;

                    if (analysisBars.HighPrices[checkIndex] > priceHigh)
                    {
                        priceHigh = analysisBars.HighPrices[checkIndex];
                        priceHighIndex = checkIndex;
                    }
                    if (rsi.Result[checkIndex] > rsiHigh)
                    {
                        rsiHigh = rsi.Result[checkIndex];
                        rsiHighIndex = checkIndex;
                    }
                }
                if (priceHighIndex < 0 || rsiHighIndex < 0 || rsiHighIndex - 1 < 0 || rsiHighIndex - 1 >= rsi.Result.Count) return false;
                if (rsiHighIndex >= rsi.Result.Count) return false;
                if (double.IsNaN(rsi.Result[rsiHighIndex]) || double.IsNaN(rsi.Result[rsiHighIndex - 1])) return false;
                return priceHighIndex == index && rsi.Result[rsiHighIndex] < rsi.Result[rsiHighIndex - 1];
            }
        }

        private MarketRegime DetectCurrentRegime(int lookbackPeriod)
        {
            if (!EnableRegimeDetection)
            {
                return MarketRegime.Ranging;
            }

            if (lookbackPeriod <= 0 || analysisBars == null || analysisBars.ClosePrices == null || analysisBars.ClosePrices.Count < lookbackPeriod)
            {
                return MarketRegime.Ranging; // Default if insufficient data
            }

            int index = analysisBars.ClosePrices.Count - 1;
            if (index < lookbackPeriod)
            {
                return MarketRegime.Ranging;
            }
        
            if (atr == null || double.IsNaN(atr.LastValue))
            {
                return MarketRegime.Ranging;
            }
        
            // Use rollingATR.Mean instead of manually calculating average ATR
            double avgATR = rollingATR.Mean;
            if (double.IsNaN(avgATR) || avgATR <= 0)
            {
                // Fallback to manual calculation if rollingATR not available
                avgATR = 0;
                int validCount = 0;
                for (int i = index - lookbackPeriod + 1; i <= index; i++)
                {
                    double atrValue = GetAtrValueMain(i);
                    if (!double.IsNaN(atrValue) && atrValue > 0)
                    {
                        avgATR += atrValue;
                        validCount++;
                    }
                }
                if (validCount == 0)
                {
                    return MarketRegime.Ranging;
                }
                avgATR /= validCount;
            }

            // Get current ATR and ADX
            double currentATR = GetAtrValueMain(index);
            double currentADX = adx != null && adx.ADX.Count > index ? adx.ADX[index] : 0;

            // Determine regime
            if (currentADX > RegimeAdxThreshold && currentATR > avgATR * RegimeAtrTrendingMultiplier)
            {
                return MarketRegime.Trending;
            }
            else if (currentATR > avgATR * RegimeAtrHighVolMultiplier)
            {
                return MarketRegime.HighVolatility;
            }
            else if (currentATR < avgATR * RegimeAtrLowVolMultiplier)
            {
                return MarketRegime.LowVolatility;
            }
            else
            {
                return MarketRegime.Ranging;
            }
        }

        // ### ENDE MODUL 4 - FÜGE HIER MODUL 5 EIN ###
        // ### MODUL 5 - Adaptive MA Bot v5.6_crypto ###
        // Contains core trading logic: UpdateStatusField, CheckForEntries, HandlePositionExit, OnPositionOpenedEvent, OnPositionClosedEvent, ManagePositions.
        // Insert this module directly after the marker '// ### ENDE MODUL 4 - FÜGE HIER MODUL 5 EIN ###'
        // in Module 4, within the AdaptiveMABot_v5_6_crypto class, to complete the code.

        private void UpdateStatusField()
        {
            // Validate essential data before proceeding
            if (analysisBars == null)
            {
                LogMessage("Critical Error: Analysis bars object is null. Cannot update status.", LoggingLevel.OnlyCritical);
                return;
            }

            if (analysisBars.ClosePrices == null)
            {
                LogMessage("Critical Error: Close prices data series is null. Cannot update status.", LoggingLevel.OnlyCritical);
                return;
            }

            if (analysisBars.ClosePrices.Count == 0)
            {
                LogMessage("Warning: No price data available yet. Skipping status update.", LoggingLevel.OnlyCritical);
                return;
            }

            // Validate MA data series
            if (ma1 == null || ma2 == null || ma3 == null)
            {
                LogMessage("Critical Error: One or more MA data series not initialized. Cannot update status.", LoggingLevel.OnlyCritical);
                return;
            }

            // Clean up old chart objects
            CleanupChartObjects();

            if (ShowInfoBlocks == InfoBlockMode.Off)
                return;

            // Calculate trading metrics
            var metrics = CalculateTradingMetrics();

            // Determine current status and filter conditions
            var statusInfo = DetermineStatusAndFilters(metrics);

            // Get enhanced status message
            metrics.Status = GetEnhancedStatusMessage(metrics, statusInfo);

            // Calculate volume information
            var volumeInfo = CalculateVolumeInformation(metrics.Index);

            // Generate display texts
            var displayTexts = GenerateDisplayTexts(metrics, statusInfo, volumeInfo);

            // Draw status information on chart
            DrawStatusOnChart(displayTexts);
        }

        private void CleanupChartObjects()
        {
            if (ShowInfoBlocks == InfoBlockMode.Off || ShowInfoBlocks == InfoBlockMode.OnlyStatusBlock)
            {
                Chart.RemoveObject("StatusFieldBackgroundMain");
                Chart.RemoveObject("StatusFieldMain");
            }
            if (ShowInfoBlocks == InfoBlockMode.Off || ShowInfoBlocks == InfoBlockMode.OnlyInfoBlock)
            {
                Chart.RemoveObject("StatusFieldBackgroundStatus");
                Chart.RemoveObject("StatusFieldStatus");
            }
        }

        private TradingMetrics CalculateTradingMetrics()
        {
            // Additional validation (though UpdateStatusField should have already checked)
            if (analysisBars == null || analysisBars.ClosePrices == null || analysisBars.ClosePrices.Count == 0)
            {
                LogMessage("Critical Error: Market data not available in CalculateTradingMetrics.", LoggingLevel.OnlyCritical);
                return new TradingMetrics(); // Return empty metrics
            }

            var metrics = new TradingMetrics();

            metrics.RiskRewardRatio = RiskMode == RiskManagementMode.Static
                ? (double)TakeProfitPips / StopLossPips
                : AtrMultiplierTp / AtrMultiplierSl;

            metrics.VolumeInUnits = CalculateVolume(atr.LastValue);
            metrics.VolumeInCurrency = metrics.VolumeInUnits;
            metrics.VolumeInLots = metrics.VolumeInUnits / Symbol.LotSize;

            // Calculate actual risk percentage based on risk management mode
            double actualRiskAmount;
            if (RiskMode == RiskManagementMode.DynamicATR)
            {
                double effectiveAtr = atr != null && atr.Count > 0 ? atr.LastValue : AtrThreshold;
                actualRiskAmount = effectiveAtr * AtrMultiplierRisk;
                metrics.ActualRiskPercent = (actualRiskAmount / Account.Balance) * PERCENTAGE_MULTIPLIER;
            }
            else
            {
                actualRiskAmount = Account.Balance * (RiskPercentage / PERCENTAGE_MULTIPLIER);
                metrics.ActualRiskPercent = RiskPercentage;
            }

            metrics.ProfitOnTpPercent = metrics.ActualRiskPercent * metrics.RiskRewardRatio;
            metrics.LossOnSlPercent = metrics.ActualRiskPercent;
            metrics.Margin = (metrics.VolumeInUnits * Symbol.Ask) / Account.PreciseLeverage;
            metrics.Leverage = Account.PreciseLeverage;

            // Determine current status
            if (Positions.Count == 0)
            {
                metrics.Status = "Scanning for new Entry";
            }
            else if (Positions[0].TradeType == TradeType.Buy)
            {
                metrics.Status = "Long Position open";
            }
            else
            {
                metrics.Status = "Short Position open";
            }

            // Calculate current position info
            metrics.CurrentPositionText = "Position: None";
            if (Positions.Count > 0)
            {
                var position = Positions[0];
                double profitLoss = position.EntryPrice > 0 && Symbol.PipSize > 0
                    ? (position.Pips * Symbol.PipSize / position.EntryPrice) * PERCENTAGE_MULTIPLIER
                    : 0;
                metrics.CurrentPositionText = $"Position: {(profitLoss >= 0 ? "+" : "")}{profitLoss:F2}%";
            }

            // Calculate MA values and trend
            metrics.Index = analysisBars.ClosePrices.Count - 1;
            metrics.Ma1ChangePercent = metrics.Index >= Ma1Candles
                ? ((GetCachedMa1Value(metrics.Index) - GetCachedMa1Value(metrics.Index - Ma1Candles)) / GetCachedMa1Value(metrics.Index - Ma1Candles)) * PERCENTAGE_MULTIPLIER
                : 0;
            metrics.Ma2Value = metrics.Index >= 0 ? GetCachedMa2Value(metrics.Index) : double.NaN;
            metrics.Ma3Value = metrics.Index >= 0 ? GetCachedMa3Value(metrics.Index) : double.NaN;
            metrics.Ma2Ma3SpreadPercent = GetCachedMa2Ma3Spread(metrics.Index);

            return metrics;
        }

        private StatusInfo DetermineStatusAndFilters(TradingMetrics metrics)
        {
            var statusInfo = new StatusInfo();

            // Check weekend blocking
            var currentTime = Server.Time;
            if (UseWeekendFilter)
            {
                var dayOfWeek = currentTime.DayOfWeek;
                var timeOfDay = currentTime.TimeOfDay;
                statusInfo.IsWeekendBlocked = (dayOfWeek == DayOfWeek.Friday && timeOfDay >= TimeSpan.FromHours(22 - HoursBeforeWeekend)) ||
                                             (dayOfWeek == DayOfWeek.Saturday) ||
                                             (dayOfWeek == DayOfWeek.Sunday && timeOfDay <= TimeSpan.FromHours(22 + HoursAfterWeekend));
            }

            // Determine filter status
            if (Positions.Count > 0)
            {
                statusInfo.FilterStatus = "Position open";
            }
            else if (statusInfo.IsWeekendBlocked && UseWeekendFilter)
            {
                statusInfo.FilterStatus = "Trades blocked by weekend filter";
            }
            else if (isTradeBlocked)
            {
                statusInfo.FilterStatus = "Trades blocked due to consecutive losses";
            }
            else if (!IsTradingAllowed())
            {
                statusInfo.FilterStatus = "Trades blocked by session filter";
            }
            else if (UseNewsAvoid && IsNewsEvent())
            {
                statusInfo.FilterStatus = "Trades blocked by news filter";
            }
            else
            {
                statusInfo.FilterStatus = DetermineDetailedFilterStatus(metrics);
            }

            // Calculate additional status information
            statusInfo.ConsecutiveColor = lastTradeWasTP ? Color.LimeGreen : lastTradeWasSL ? Color.Orange : Color.White;
            statusInfo.BlockStatus = isTradeBlocked
                ? $"Trades Blocked: {(lastLossBarIndex >= 0 ? (analysisBars.ClosePrices.Count - 1 - lastLossBarIndex) : 0)}/{BlockCandles} candles"
                : "Trades Allowed";

            statusInfo.ConfirmationStatus = UseConfirmationCandles && (isLongSignalActive || isShortSignalActive)
                ? $"Confirming {(isLongSignalActive ? "Long" : "Short")}: {consecutiveConfirmationCandles}/{ConfirmationCandles} candles"
                : UseConfirmationCandles ? "Waiting for signal" : "No confirmation active";

            statusInfo.SpreadStatus = UseMa2Ma3Spread && (isSpreadLongSignalActive || isSpreadShortSignalActive)
                ? $"Checking Spread ({spreadCandleCount}/{SpreadCandles} candles)"
                : UseMa2Ma3Spread ? "Waiting for spread signal" : "No spread check";

            statusInfo.PatternStatus = "No pattern check";

            return statusInfo;
        }

        private string DetermineDetailedFilterStatus(TradingMetrics metrics)
        {
            // Bounds checking to prevent IndexOutOfRangeException
            if (metrics.Index < 0 || metrics.Index >= analysisBars.ClosePrices.Count || metrics.Index >= analysisBars.OpenPrices.Count) {
                return "Invalid index for filter status";
            }

            double ma1Value = GetCachedMa1Value(metrics.Index);
            bool isBullishCandle = analysisBars.ClosePrices[metrics.Index] > analysisBars.OpenPrices[metrics.Index];
            bool isBearishCandle = analysisBars.ClosePrices[metrics.Index] < analysisBars.OpenPrices[metrics.Index];
            bool isAboveAllMas = analysisBars.ClosePrices[metrics.Index] > metrics.Ma2Value && analysisBars.ClosePrices[metrics.Index] > metrics.Ma3Value &&
                                (ConsiderMa1 ? analysisBars.ClosePrices[metrics.Index] > ma1Value : true);
            bool isBelowAllMas = analysisBars.ClosePrices[metrics.Index] < metrics.Ma2Value && analysisBars.ClosePrices[metrics.Index] < metrics.Ma3Value &&
                                (ConsiderMa1 ? analysisBars.ClosePrices[metrics.Index] < ma1Value : true);
            bool hasSufficientMomentum = !UseMaFilter || Math.Abs(metrics.Ma1ChangePercent) >= GetCachedMomentumThreshold(metrics.Index);

            if (EntryCondition == EntryConditionType.MA2MA3Crossover && UseMa2Ma3Spread && metrics.Ma2Ma3SpreadPercent < Ma2Ma3Spread)
            {
                return $"MA2/MA3 Spread too low ({metrics.Ma2Ma3SpreadPercent:F2}% < {Ma2Ma3Spread:F2}%)";
            }
            else if (!hasSufficientMomentum)
            {
                return "Insufficient Momentum";
            }
            else if (UseRsiFilter && isAboveAllMas && isBullishCandle && rsi.Result[metrics.Index] <= RsiBuyThreshold)
            {
                return "RSI too low";
            }
            else if (UseRsiFilter && isBelowAllMas && isBearishCandle && rsi.Result[metrics.Index] >= RsiSellThreshold)
            {
                return "RSI too high";
            }
            else if (UseAtrFilter && GetCachedAtrValue(metrics.Index) < AtrThreshold)
            {
                return "Insufficient Volatility (ATR)";
            }
            else
            {
                return "Waiting for entry conditions";
            }
        }

        private string GetEnhancedStatusMessage(TradingMetrics metrics, StatusInfo statusInfo)
        {
            // If we have a position, show position status
            if (Positions.Count > 0)
            {
                var position = Positions[0];
                double entryTimeHours = (Server.Time - position.EntryTime).TotalHours;
                string direction = position.TradeType == TradeType.Buy ? "LONG" : "SHORT";
                string pnl = position.NetProfit >= 0 ? $"+{position.NetProfit:F2}" : $"{position.NetProfit:F2}";

                if (entryTimeHours < 1)
                    return $"{direction} {pnl} ({entryTimeHours:F1}h)";

                if (UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlySoft)
                {
                    double timeToSoft = TimeLimitSoft - entryTimeHours;
                    if (timeToSoft > 0 && timeToSoft <= 2)
                        return $"{direction} {pnl} - Soft close in {timeToSoft:F1}h";
                }

                if (UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlyHard)
                {
                    double timeToHard = TimeLimitHard - entryTimeHours;
                    if (timeToHard > 0 && timeToHard <= 4)
                        return $"{direction} {pnl} - Hard close in {timeToHard:F1}h";
                }

                return $"{direction} {pnl} ({entryTimeHours:F1}h)";
            }

            // Show blocking conditions with more detail
            if (isTradeBlocked)
            {
                int remainingCandles = lastLossBarIndex >= 0 ? (analysisBars.ClosePrices.Count - 1 - lastLossBarIndex) : 0;
                return $"BLOCKED: {remainingCandles}/{BlockCandles} candles (Consecutive losses)";
            }

            if (statusInfo.IsWeekendBlocked)
                return "BLOCKED: Weekend filter active";

            if (!IsTradingAllowed())
                return $"BLOCKED: Outside {GetCurrentSession()} session";

            if (UseNewsAvoid && IsNewsEvent())
                return "BLOCKED: Major news event";


            if (isLongSignalActive || isShortSignalActive)
            {
                string signalType = isLongSignalActive ? "LONG" : "SHORT";
                return $"Confirming {signalType}: {consecutiveConfirmationCandles}/{ConfirmationCandles} candles";
            }

            if (isSpreadLongSignalActive || isSpreadShortSignalActive)
            {
                string spreadType = isSpreadLongSignalActive ? "LONG" : "SHORT";
                return $"Spread Check: {spreadType} ({spreadCandleCount}/{SpreadCandles} candles)";
            }

            // Show market condition and readiness
            string trendDirection = metrics.Ma1ChangePercent >= 0.1 ? "↗ BULLISH" :
                                   metrics.Ma1ChangePercent <= -0.1 ? "↘ BEARISH" : "→ SIDEWAYS";

            double currentPrice = analysisBars.ClosePrices[metrics.Index];
            bool isAboveMa3 = currentPrice > metrics.Ma3Value;
            bool isAboveMa2 = currentPrice > metrics.Ma2Value;
            bool isAboveMa1 = ConsiderMa1 ? currentPrice > GetCachedMa1Value(metrics.Index) : true;

            string positionRelativeToMAs = isAboveMa1 && isAboveMa2 && isAboveMa3 ? "Above All MAs" :
                                          !isAboveMa1 && !isAboveMa2 && !isAboveMa3 ? "Below All MAs" : "Mixed MA Position";

            // Check if we have basic entry conditions met
            bool basicConditionsMet = false;
            if (EntryCondition == EntryConditionType.Crossover)
            {
                basicConditionsMet = (isAboveMa3 && metrics.Index > 0 && analysisBars.ClosePrices[metrics.Index - 1] <= metrics.Ma3Value && currentPrice > metrics.Ma3Value) ||
                                    (currentPrice < metrics.Ma3Value && metrics.Index > 0 && analysisBars.ClosePrices[metrics.Index - 1] >= metrics.Ma3Value && currentPrice < metrics.Ma3Value);
            }

            if (basicConditionsMet)
                return $"Ready: {trendDirection} | {positionRelativeToMAs}";
            else
                return $"Scanning: {trendDirection} | {positionRelativeToMAs}";
        }

        private VolumeInfo CalculateVolumeInformation(int index)
        {
            var volumeInfo = new VolumeInfo();

            // Bounds checking for current candle volume
            if (index >= 0 && index < analysisBars.TickVolumes.Count)
            {
                volumeInfo.CurrentCandleVolume = analysisBars.TickVolumes[index];
            }
            else
            {
                volumeInfo.CurrentCandleVolume = 0;
            }

            volumeInfo.AvgDailyVolume = 0;
            volumeInfo.DynamicMinVolume = MinVolume;
            volumeInfo.DynamicMaxVolume = MaxVolume;

            if (index >= 0 && index < analysisBars.OpenTimes.Count && index < analysisBars.TickVolumes.Count)
            {
                var currentDay = analysisBars.OpenTimes[index].Date;
                double volumeSum = 0;
                int candleCount = 0;
                for (int i = index; i >= 0; i--)
                {
                    // Bounds checking for loop indices
                    if (i >= analysisBars.OpenTimes.Count || i >= analysisBars.TickVolumes.Count)
                        break;

                    if (analysisBars.OpenTimes[i].Date < currentDay)
                        break;
                    volumeSum += analysisBars.TickVolumes[i];
                    candleCount++;
                }
                volumeInfo.AvgDailyVolume = candleCount > 0 ? volumeSum / candleCount : 0;
                volumeInfo.DynamicMinVolume = CalculateDynamicMinVolume(index);
                volumeInfo.DynamicMaxVolume = CalculateDynamicMaxVolume(index);
            }

            return volumeInfo;
        }

        private DisplayTexts GenerateDisplayTexts(TradingMetrics metrics, StatusInfo statusInfo, VolumeInfo volumeInfo)
        {
            var texts = new DisplayTexts();

            string currentSession = GetCurrentSession();
            string regimeText = EnableRegimeDetection ? $"🌊 Regime: {currentRegime}\n" : "🌊 Regime: Disabled\n";
            string riskModeText = RiskMode == RiskManagementMode.DynamicATR ? $"🎯 Risk: {metrics.ActualRiskPercent:F2}% (Dynamic)\n" : $"🎯 Risk: {metrics.ActualRiskPercent:F1}% (Static)\n";
            texts.MainText = $"⏰ Session: {currentSession}\n" +
                                regimeText +
                                $"💰 Risk-Reward: {metrics.RiskRewardRatio:F1}:1\n" +
                                riskModeText +
                                $"📈 Position Size: {metrics.VolumeInLots:F2} Lots\n" +
                                $"💵 Margin Used: {metrics.Margin:F2} {Account.Asset.Name}\n" +
                                $"{metrics.CurrentPositionText}\n";

            texts.StatusBlockText = $"🎯 Market Analysis\n" +
                                     $"Trend: {(metrics.Ma1ChangePercent >= 0.1 ? "↗ BULLISH" : metrics.Ma1ChangePercent <= -0.1 ? "↘ BEARISH" : "→ SIDEWAYS")}\n" +
                                     $"Momentum: {(metrics.Ma1ChangePercent >= 0 ? "+" : "")}{metrics.Ma1ChangePercent:F1}%\n" +
                                     $"Volume: {volumeInfo.CurrentCandleVolume:F0} ({(volumeInfo.CurrentCandleVolume > volumeInfo.AvgDailyVolume ? "HIGH" : "NORMAL")})\n" +
                                     $"Avg. Volume: {volumeInfo.AvgDailyVolume:F0}\n" +
                                     $"\n" +
                                     $"⚙️ System Status\n" +
                                     $"{statusInfo.BlockStatus}\n" +
                                     $"{statusInfo.FilterStatus}\n" +
                                     $"{metrics.Status}\n" +
                                     $"\n" +
                                     $"🔄 Active Signals\n" +
                                     $"{statusInfo.ConfirmationStatus}\n" +
                                     $"{statusInfo.SpreadStatus}\n" +
                                     $"{statusInfo.PatternStatus}";

            return texts;
        }

        private string GenerateLearningStatusContent()
        {
            var content = new StringBuilder();

            // Current Market Regime
            content.AppendLine("🧠 Learning Status");
            content.AppendLine($"🌊 Regime: {currentRegime}");

            // Performance Metrics
            if (EnableRollingPerformance && rollingReturns1.Count > 0)
            {
                content.AppendLine($"📊 Win Rate: {rollingWinRate1:P1} ({RollingWindow1} trades)");
                content.AppendLine($"📈 Sharpe: {rollingSharpe1:F2}");
                content.AppendLine($"📉 Max DD: {rollingMaxDrawdown1:F1}%");
                content.AppendLine($"💰 PF: {rollingProfitFactor1:F2}");
            }
            else
            {
                double winRate = totalTrades > 0 ? (double)winningTrades / totalTrades : 0;
                content.AppendLine($"📊 Win Rate: {winRate:P1} ({totalTrades} trades)");
                double profitFactor = totalLoss > 0 ? totalProfit / totalLoss : (totalProfit > 0 ? double.PositiveInfinity : 0);
                content.AppendLine($"💰 PF: {profitFactor:F2}");
            }

            // Trend Analysis
            content.AppendLine($"📈 Trend: {performanceTrend}");

            // Parameter Adjustments
            if (EnableLearningFeatures)
            {
                content.AppendLine("");
                content.AppendLine("⚙️ Parameter Adjustments");
                content.AppendLine($"ATR Threshold: {AtrThreshold:F4}");
                content.AppendLine($"RSI Buy: {RsiBuyThreshold:F1}");
                content.AppendLine($"Momentum: {Ma1Momentum:F2}");
            }

            // Correlation Analysis
            if (EnableCorrelationAnalysis && optimizationSuggestions.Count > 0)
            {
                content.AppendLine("");
                content.AppendLine("🔗 Optimization Suggestions");
                foreach (var suggestion in optimizationSuggestions.Take(2))
                {
                    content.AppendLine($"{suggestion.ParameterName}: {suggestion.SuggestedValue:F3}");
                }
            }

            return content.ToString();
        }

        private string GetSystemStatus()
        {
            try
            {
                // Check for critical system errors
                if (analysisBars == null || analysisBars.ClosePrices == null || analysisBars.ClosePrices.Count == 0)
                    return "ERROR: No market data";

                if (ma1 == null || ma2 == null || ma3 == null)
                    return "ERROR: MA data unavailable";

                if (Account == null || Account.Balance <= 0)
                    return "ERROR: Account data invalid";

                if (Symbol == null)
                    return "ERROR: Symbol data unavailable";

                // Check for data validation issues
                int index = analysisBars.ClosePrices.Count - 1;
                if (index < 0)
                    return "ERROR: Invalid data index";

                // Check for NaN values in critical data
                if (double.IsNaN(analysisBars.ClosePrices[index]))
                    return "ERROR: Invalid price data";

                double ma1Val = GetCachedMa1Value(index);
                double ma2Val = GetCachedMa2Value(index);
                double ma3Val = GetCachedMa3Value(index);

                if (ma1.Count > index && double.IsNaN(ma1Val))
                    return "ERROR: Invalid MA1 data";

                if (ma2.Count > index && double.IsNaN(ma2Val))
                    return "ERROR: Invalid MA2 data";

                if (ma3.Count > index && double.IsNaN(ma3Val))
                    return "ERROR: Invalid MA3 data";

                // Check for trade blocking conditions
                if (isTradeBlocked)
                    return "BLOCKED: Consecutive losses";

                // Check for weekend filter
                if (UseWeekendFilter)
                {
                    var currentTime = Server.Time;
                    var dayOfWeek = currentTime.DayOfWeek;
                    var timeOfDay = currentTime.TimeOfDay;
                    if ((dayOfWeek == DayOfWeek.Friday && timeOfDay >= TimeSpan.FromHours(22 - HoursBeforeWeekend)) ||
                        (dayOfWeek == DayOfWeek.Saturday) ||
                        (dayOfWeek == DayOfWeek.Sunday && timeOfDay <= TimeSpan.FromHours(22 + HoursAfterWeekend)))
                        return "BLOCKED: Weekend filter";
                }

                // Check for session filter
                if (!IsTradingAllowed())
                    return "BLOCKED: Session filter";

                // Check for news avoidance
                if (UseNewsAvoid && IsNewsEvent())
                    return "BLOCKED: News event";

                // All systems operational
                return "OK: All systems operational";
            }
            catch (Exception ex)
            {
                return $"ERROR: System check failed - {ex.Message}";
            }
        }

        private void LogMessage(string message, LoggingLevel requiredLevel = LoggingLevel.Full)
        {
            if (LogLevel == LoggingLevel.Off)
                return;

            if (LogLevel == LoggingLevel.Full ||
                (LogLevel == LoggingLevel.Info && (requiredLevel == LoggingLevel.Info || requiredLevel == LoggingLevel.Debug || requiredLevel == LoggingLevel.OnlyImportant || requiredLevel == LoggingLevel.Warning || requiredLevel == LoggingLevel.Error || requiredLevel == LoggingLevel.OnlyCritical || requiredLevel == LoggingLevel.OnlyTrades)) ||
                (LogLevel == LoggingLevel.Debug && (requiredLevel == LoggingLevel.Debug || requiredLevel == LoggingLevel.OnlyImportant || requiredLevel == LoggingLevel.Warning || requiredLevel == LoggingLevel.Error || requiredLevel == LoggingLevel.OnlyCritical || requiredLevel == LoggingLevel.OnlyTrades)) ||
                (LogLevel == LoggingLevel.OnlyImportant && (requiredLevel == LoggingLevel.OnlyImportant || requiredLevel == LoggingLevel.Warning || requiredLevel == LoggingLevel.Error || requiredLevel == LoggingLevel.OnlyCritical || requiredLevel == LoggingLevel.OnlyTrades)) ||
                (LogLevel == LoggingLevel.Warning && (requiredLevel == LoggingLevel.Warning || requiredLevel == LoggingLevel.Error || requiredLevel == LoggingLevel.OnlyCritical || requiredLevel == LoggingLevel.OnlyTrades)) ||
                (LogLevel == LoggingLevel.Error && (requiredLevel == LoggingLevel.Error || requiredLevel == LoggingLevel.OnlyCritical || requiredLevel == LoggingLevel.OnlyTrades)) ||
                (LogLevel == LoggingLevel.OnlyCritical && requiredLevel == LoggingLevel.OnlyCritical) ||
                (LogLevel == LoggingLevel.OnlyTrades && requiredLevel == LoggingLevel.OnlyTrades))
            {
                Print(message);
            }
        }

        private void LogIndicatorPerformance()
        {
            if (LogLevel == LoggingLevel.Off || LogLevel == LoggingLevel.OnlyTrades)
                return;

            try
            {
                LogMessage("[PERFORMANCE] === Optimized Indicator Performance Report ===", LoggingLevel.Info);

                // Log DEMA performance
                if (dema1 != null)
                    LogMessage($"[PERFORMANCE] DEMA1: {dema1.GetAverageCalculationTimeMs():F4}ms avg", LoggingLevel.Info);
                if (dema2 != null)
                    LogMessage($"[PERFORMANCE] DEMA2: {dema2.GetAverageCalculationTimeMs():F4}ms avg", LoggingLevel.Info);
                if (dema3 != null)
                    LogMessage($"[PERFORMANCE] DEMA3: {dema3.GetAverageCalculationTimeMs():F4}ms avg", LoggingLevel.Info);

                // Log Custom ATR performance
                if (customHullAtr != null)
                    LogMessage($"[PERFORMANCE] CustomHullAtr: {customHullAtr.GetAverageCalculationTimeMs():F4}ms avg", LoggingLevel.Info);
                if (customDemaAtr != null)
                    LogMessage($"[PERFORMANCE] CustomDoubleExponentialAtr: {customDemaAtr.GetAverageCalculationTimeMs():F4}ms avg", LoggingLevel.Info);

                LogMessage("[PERFORMANCE] =================================================", LoggingLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"[PERFORMANCE] Error generating performance report: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void LogChartPerformance()
        {
            if (LogLevel == LoggingLevel.Off || LogLevel == LoggingLevel.OnlyTrades)
                return;

            try
            {
                LogMessage("[CHART_PERFORMANCE] === Chart Performance Report ===", LoggingLevel.Info);

                // Log batching performance
                if (EnableChartBatching)
                {
                    LogMessage($"[CHART_PERFORMANCE] Batching: Enabled (Batch Size: {ChartUpdateBatchSize})", LoggingLevel.Info);
                    LogMessage($"[CHART_PERFORMANCE] Current Batch Size: {currentBatchSize}/{ChartUpdateBatchSize}", LoggingLevel.Info);
                    LogMessage($"[CHART_PERFORMANCE] Total Batches Processed: {chartUpdateCount}", LoggingLevel.Info);
                    LogMessage($"[CHART_PERFORMANCE] Average Batch Update Time: {GetAverageChartUpdateTimeMs():F4}ms", LoggingLevel.Info);
                    LogMessage($"[CHART_PERFORMANCE] Objects in Current Batch: {chartObjectBatch.Count}", LoggingLevel.Info);
                }
                else
                {
                    LogMessage($"[CHART_PERFORMANCE] Batching: Disabled (Real-time updates)", LoggingLevel.Info);
                }

                // Log update frequency
                LogMessage($"[CHART_PERFORMANCE] Update Frequency: Every {ChartUpdateFrequency} ticks", LoggingLevel.Info);
                LogMessage($"[CHART_PERFORMANCE] Chart Update Counter: {chartUpdateCounter}", LoggingLevel.Info);

                // Log chart object counts
                int totalChartObjects = Chart.Objects.Count;
                int maObjects = Chart.Objects.Count(obj => obj.Name.StartsWith("MA"));
                int markerObjects = Chart.Objects.Count(obj => obj.Name.Contains("Entry") || obj.Name.Contains("Exit") || obj.Name.Contains("Buy") || obj.Name.Contains("Sell"));

                LogMessage($"[CHART_PERFORMANCE] Total Chart Objects: {totalChartObjects}", LoggingLevel.Info);
                LogMessage($"[CHART_PERFORMANCE] MA Line Objects: {maObjects}", LoggingLevel.Info);
                LogMessage($"[CHART_PERFORMANCE] Marker Objects: {markerObjects}", LoggingLevel.Info);

                LogMessage("[CHART_PERFORMANCE] ======================================", LoggingLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"[CHART_PERFORMANCE] Error generating chart performance report: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void DrawStatusOnChart(DisplayTexts texts)
        {
            // Panel 1: Top Right - System & Position Info (Green/Red/White)
            if (ShowInfoBlocks == InfoBlockMode.On || ShowInfoBlocks == InfoBlockMode.OnlyInfoBlock)
            {
                DrawTopRightPanel(texts.MainText);
            }

            // Panel 2: Bottom Right - Trading Status (Blue/Orange/White)
            if (ShowInfoBlocks == InfoBlockMode.On || ShowInfoBlocks == InfoBlockMode.OnlyStatusBlock)
            {
                DrawBottomRightPanel(texts.StatusBlockText);
            }

            // Panel 3: Top Left - Performance Metrics (Blue/White)
            if (ShowInfoBlocks == InfoBlockMode.On)
            {
                DrawTopLeftPanel();
            }

            // Panel 4: Bottom Left - Learning Status (Purple/White)
            if (ShowInfoBlocks == InfoBlockMode.On && (EnableLearningFeatures || EnableRegimeDetection || EnableCorrelationAnalysis))
            {
                DrawBottomLeftPanel();
            }
        }

        private void DrawTopRightPanel(string mainText)
        {
            // Use FIXED SCREEN POSITIONING for top-right panel (doesn't move with chart)
            // Position relative to current chart view using time-based approach but with fixed offset
            DateTime currentTime = Bars.OpenTimes.LastValue;
            DateTime panelTime = currentTime.AddMinutes(5); // Small forward offset
            DateTime panelTimeLeft = panelTime.AddMinutes(-4); // Narrow panel width

            // Calculate panel boundaries for background
            double lineHeight = Symbol.PipSize > 0 ? Symbol.PipSize * 3.5 : 0.000035;
            var lines = mainText.Split('\n');
            double topY = Chart.TopY;
            double bottomY = topY - (lines.Length * lineHeight);

            // Draw background rectangle
            Chart.DrawRectangle("TopRightPanelBG",
                panelTimeLeft, topY,
                panelTime, bottomY,
                Color.FromArgb(180, 64, 64, 64), 0, LineStyle.Solid);

            // Use DrawStaticText for proper multi-line display
            try
            {
                Chart.DrawStaticText("TopRightPanel",
                    mainText, VerticalAlignment.Top, HorizontalAlignment.Right,
                    Color.LightBlue);
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing TopRight panel: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void DrawBottomRightPanel(string statusText)
        {
            // Use FIXED SCREEN POSITIONING for bottom-right panel (doesn't move with chart)
            // Position relative to current chart view using time-based approach but with fixed offset
            DateTime currentTime = Bars.OpenTimes.LastValue;
            DateTime panelTime = currentTime.AddMinutes(10); // Small forward offset (different from TopRight)
            DateTime panelTimeLeft = panelTime.AddMinutes(-4); // Narrow panel width

            // Calculate panel boundaries for background
            double lineHeight = Symbol.PipSize > 0 ? Symbol.PipSize * 2.8 : 0.000028;
            var lines = statusText.Split('\n');
            double bottomY = Chart.BottomY;
            double topY = bottomY + (lines.Length * lineHeight);

            // Draw background rectangle
            Chart.DrawRectangle("BottomRightPanelBG",
                panelTimeLeft, bottomY,
                panelTime, topY,
                Color.FromArgb(180, 64, 64, 64), 0, LineStyle.Solid);

            // Use DrawStaticText for proper multi-line display
            try
            {
                Chart.DrawStaticText("BottomRightPanel",
                    statusText, VerticalAlignment.Bottom, HorizontalAlignment.Right,
                    Color.LightBlue);
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing BottomRight panel: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void DrawTopLeftPanel()
        {
            // Performance metrics panel
            var metrics = CalculateTradingMetrics();

            // Use FIXED SCREEN POSITIONING for top-left panel (doesn't move with chart)
            // Position relative to current chart view using time-based approach but with fixed offset
            DateTime currentTime = Bars.OpenTimes.LastValue;
            DateTime panelTime = currentTime.AddMinutes(-10); // Backward offset for left side
            DateTime panelTimeRight = panelTime.AddMinutes(4); // Narrow panel width

            // Performance data as multi-line string
            string perfText;
            if (EnableRollingPerformance && rollingReturns1.Count > 0)
            {
                perfText = $"📊 Rolling Performance ({RollingWindow1}/{RollingWindow2}/{RollingWindow3})\n" +
                            $"WinRate: {rollingWinRate1:P1}/{rollingWinRate2:P1}/{rollingWinRate3:P1}\n" +
                            $"Sharpe: {rollingSharpe1:F2}/{rollingSharpe2:F2}/{rollingSharpe3:F2}\n" +
                            $"Sortino: {rollingSortino1:F2}/{rollingSortino2:F2}/{rollingSortino3:F2}\n" +
                            $"Calmar: {rollingCalmar1:F2}/{rollingCalmar2:F2}/{rollingCalmar3:F2}\n" +
                            $"MaxDD: {rollingMaxDrawdown1:F1}%/{rollingMaxDrawdown2:F1}%/{rollingMaxDrawdown3:F1}%\n" +
                            $"PF: {rollingProfitFactor1:F2}/{rollingProfitFactor2:F2}/{rollingProfitFactor3:F2}\n" +
                            $"Trend: {performanceTrend}";
            }
            else
            {
                perfText = $"📊 Performance Metrics\n" +
                            $"Trades: {totalTrades} | Winrate: {(totalTrades > 0 ? (winningTrades / (double)totalTrades) * PERCENTAGE_MULTIPLIER : 0):F1}%\n" +
                            $"Sharpe: {CalculateSharpeRatio():F2} | PF: {(totalLoss > 0 ? (totalProfit / totalLoss).ToString("F2") : "N/A")}\n" +
                            $"Profit: {(totalPips >= 0 ? "+" : "")}{totalPips:F1} pips | {(totalTrades > 0 ? ((totalProfit - totalLoss) / Math.Max(totalProfit + totalLoss, 1)) * PERCENTAGE_MULTIPLIER : 0):F1}% | {(totalProfit - totalLoss >= 0 ? "+" : "")}{(totalProfit - totalLoss):F2} {Account?.Asset?.Name ?? "EUR"}";
            }

            // Calculate panel boundaries for background
            double lineHeight = Symbol.PipSize > 0 ? Symbol.PipSize * 3.2 : 0.000032;
            var lines = perfText.Split('\n');
            double topY = Chart.TopY;
            double bottomY = topY - (lines.Length * lineHeight);

            // Draw background rectangle
            Chart.DrawRectangle("TopLeftPanelBG",
                panelTime, topY,
                panelTimeRight, bottomY,
                Color.FromArgb(180, 64, 64, 64), 0, LineStyle.Solid);

            // Use DrawStaticText for proper multi-line display
            try
            {
                Chart.DrawStaticText("TopLeftPanel",
                    perfText, VerticalAlignment.Top, HorizontalAlignment.Left,
                    Color.LightBlue);
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing TopLeft panel: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void DrawBottomLeftPanel()
        {
            // Learning status panel
            string learningText = GenerateLearningStatusContent();

            // Use FIXED SCREEN POSITIONING for bottom-left panel (doesn't move with chart)
            // Position relative to current chart view using time-based approach but with fixed offset
            DateTime currentTime = Bars.OpenTimes.LastValue;
            DateTime panelTime = currentTime.AddMinutes(-10); // Backward offset for left side
            DateTime panelTimeRight = panelTime.AddMinutes(4); // Narrow panel width

            // Calculate panel boundaries for background
            double lineHeight = Symbol.PipSize > 0 ? Symbol.PipSize * 2.5 : 0.000025;
            var lines = learningText.Split('\n');
            double bottomY = Chart.BottomY;
            double topY = bottomY + (lines.Length * lineHeight);

            // Draw background rectangle with purple tint for learning panel
            Chart.DrawRectangle("BottomLeftPanelBG",
                panelTime, bottomY,
                panelTimeRight, topY,
                Color.FromArgb(180, 128, 64, 128), 0, LineStyle.Solid);

            // Use DrawStaticText for proper multi-line display
            try
            {
                Chart.DrawStaticText("BottomLeftPanel",
                    learningText, VerticalAlignment.Bottom, HorizontalAlignment.Left,
                    Color.LightBlue);
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing BottomLeft panel: {ex.Message}", LoggingLevel.Error);
            }
        }


        // Helper classes for better code organization
        private class TradingMetrics
        {
            public double RiskRewardRatio { get; set; }
            public double VolumeInUnits { get; set; }
            public double VolumeInCurrency { get; set; }
            public double VolumeInLots { get; set; }
            public double ProfitOnTpPercent { get; set; }
            public double LossOnSlPercent { get; set; }
            public double ActualRiskPercent { get; set; }
            public double Margin { get; set; }
            public double Leverage { get; set; }
            public string Status { get; set; }
            public string CurrentPositionText { get; set; }
            public int Index { get; set; }
            public double Ma1ChangePercent { get; set; }
            public double Ma2Value { get; set; }
            public double Ma3Value { get; set; }
            public double Ma2Ma3SpreadPercent { get; set; }
        }

        private class StatusInfo
        {
            public bool IsWeekendBlocked { get; set; }
            public string FilterStatus { get; set; }
            public Color ConsecutiveColor { get; set; }
            public string BlockStatus { get; set; }
            public string ConfirmationStatus { get; set; }
            public string SpreadStatus { get; set; }
            public string PatternStatus { get; set; }
        }

        private class VolumeInfo
        {
            public double CurrentCandleVolume { get; set; }
            public double AvgDailyVolume { get; set; }
            public double DynamicMinVolume { get; set; }
            public double DynamicMaxVolume { get; set; }
        }

        private class DisplayTexts
        {
            public string MainText { get; set; }
            public string StatusBlockText { get; set; }
        }

        internal class ChartObjectBatch
        {
            public string ObjectName { get; set; }
            public ChartObjectType ObjectType { get; set; }
            public DateTime Time { get; set; }
            public DateTime Time2 { get; set; } // For trend lines end time
            public double Price { get; set; }
            public Color Color { get; set; }
            public string Text { get; set; }
            public ChartIconType IconType { get; set; }
            public double Price2 { get; set; } // For trend lines
            public MarkerPriority Priority { get; set; }
            public DateTime CreationTime { get; set; }

            public ChartObjectBatch()
            {
                CreationTime = DateTime.Now;
                Priority = MarkerPriority.Normal;
            }
        }

        internal enum ChartObjectType
        {
            TrendLine,
            Text,
            Icon,
            Rectangle
        }

        internal enum MarkerPriority
        {
            Low,
            Normal,
            High,
            Critical
        }

        internal class RollingStatistics
        {
            private Queue<double> values;
            private int period;
            private double sum;
            private double sumSquares;

            public RollingStatistics(int period)
            {
                this.period = period;
                values = new Queue<double>();
                sum = 0;
                sumSquares = 0;
            }

            public void AddValue(double value)
            {
                if (double.IsNaN(value) || double.IsInfinity(value)) return;
                if (values.Count >= period)
                {
                    double oldValue = values.Dequeue();
                    sum -= oldValue;
                    sumSquares -= oldValue * oldValue;
                }
                values.Enqueue(value);
                sum += value;
                sumSquares += value * value;
            }

            public double Mean
            {
                get
                {
                    if (values.Count == 0) return double.NaN;
                    return sum / values.Count;
                }
            }

            public double StdDev
            {
                get
                {
                    if (values.Count < 2) return double.NaN;
                    double mean = Mean;
                    double variance = (sumSquares - (sum * sum / values.Count)) / (values.Count - 1);
                    if (variance < 0) variance = 0;
                    return Math.Sqrt(variance);
                }
            }
        }

        /// <summary>
        /// LRU Cache implementation with size limits and eviction policy
        /// </summary>
        internal class LRUCache<TKey, TValue>
        {
            private readonly int maxSize;
            private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache;
            private readonly LinkedList<CacheItem> lruList;
            private long hits = 0;
            private long misses = 0;
            private long evictions = 0;

            internal class CacheItem
            {
                public TKey Key { get; set; }
                public TValue Value { get; set; }
            }

            public LRUCache(int maxSize)
            {
                this.maxSize = maxSize;
                cache = new Dictionary<TKey, LinkedListNode<CacheItem>>();
                lruList = new LinkedList<CacheItem>();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                if (cache.TryGetValue(key, out var node))
                {
                    // Move to front (most recently used)
                    lruList.Remove(node);
                    lruList.AddFirst(node);
                    value = node.Value.Value;
                    hits++;
                    return true;
                }
                value = default;
                misses++;
                return false;
            }

            public void Set(TKey key, TValue value)
            {
                if (cache.TryGetValue(key, out var existingNode))
                {
                    // Update existing value and move to front
                    existingNode.Value.Value = value;
                    lruList.Remove(existingNode);
                    lruList.AddFirst(existingNode);
                }
                else
                {
                    // Add new item
                    var newItem = new CacheItem { Key = key, Value = value };
                    var newNode = new LinkedListNode<CacheItem>(newItem);
                    cache[key] = newNode;
                    lruList.AddFirst(newNode);

                    // Evict if over capacity
                    if (cache.Count > maxSize)
                    {
                        var lastNode = lruList.Last;
                        if (lastNode != null)
                        {
                            cache.Remove(lastNode.Value.Key);
                            lruList.RemoveLast();
                            evictions++;
                        }
                    }
                }
            }

            public void Clear()
            {
                cache.Clear();
                lruList.Clear();
            }

            public int Count => cache.Count;

            public double HitRate => (hits + misses) > 0 ? (double)hits / (hits + misses) : 0;

            public long Evictions => evictions;

            public void WarmCache(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                foreach (var item in items)
                {
                    Set(item.Key, item.Value);
                }
            }
        }

        private void CheckForEntries()
        {
            // Performance monitoring: Start timing
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            checkForEntriesCallCount++;

            try
            {
                // Validate essential data with detailed error messages
                if (analysisBars == null)
                {
                    LogMessage("Critical Error: Analysis bars object is null. Bot cannot function without market data.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices == null)
                {
                    LogMessage("Critical Error: Close prices data series is null. Market data may be corrupted.", LoggingLevel.OnlyCritical);
                    return;
                }

                if (analysisBars.ClosePrices.Count == 0)
                {
                    LogMessage("Warning: No price data available yet. Waiting for market data to load.", LoggingLevel.OnlyCritical);
                    return;
                }

                int index = analysisBars.ClosePrices.Count - 1;

                // Prevent multiple entries within the same bar in OnTick mode
                if (index == lastEntryBarIndex) return;

                // Bounds checking to prevent IndexOutOfRangeException
                if (index < 0 || index >= analysisBars.ClosePrices.Count ||
                    index >= analysisBars.OpenPrices.Count || index >= analysisBars.HighPrices.Count ||
                    index >= analysisBars.LowPrices.Count || index >= analysisBars.TickVolumes.Count)
                {
                    LogMessage($"Critical Error: Index {index} out of bounds for analysis bars arrays", LoggingLevel.Error);
                    return;
                }

                // Validate sufficient historical data
                int requiredBars = Math.Max(Math.Max(Ma1Period, Ma2Period), Ma3Period);
                if (requiredBars <= 0)
                {
                    LogMessage($"Critical Error: Invalid MA periods detected. MA1: {Ma1Period}, MA2: {Ma2Period}, MA3: {Ma3Period}", LoggingLevel.Error);
                    return;
                }

                if (index < requiredBars)
                {
                    LogMessage($"Warning: Insufficient historical data. Need {requiredBars} bars, currently have {index + 1}. Waiting for more data.", LoggingLevel.Warning);
                    return;
                }

                // Validate MA data series with detailed checks
                if (ma1 == null)
                {
                    LogMessage("Critical Error: MA1 data series not initialized. Check MA1 configuration.", LoggingLevel.Error);
                    return;
                }
                if (ma2 == null)
                {
                    LogMessage("Critical Error: MA2 data series not initialized. Check MA2 configuration.", LoggingLevel.Error);
                    return;
                }
                if (ma3 == null)
                {
                    LogMessage("Critical Error: MA3 data series not initialized. Check MA3 configuration.", LoggingLevel.Error);
                    return;
                }

                // Validate data series bounds
                if (index >= ma1.Count || index >= ma2.Count || index >= ma3.Count)
                {
                    LogMessage($"Critical Error: Index {index} out of bounds for MA data series. MA1 count: {ma1.Count}, MA2 count: {ma2.Count}, MA3 count: {ma3.Count}", LoggingLevel.Error);
                    return;
                }

                // Additional bounds checking for offset indices used in the method
                if (index - 1 < 0 || index - 1 >= analysisBars.ClosePrices.Count ||
                    index - 2 < 0 || index - 2 >= analysisBars.ClosePrices.Count)
                {
                    LogMessage($"Warning: Insufficient historical data for offset calculations. Index: {index}, required minimum: 2", LoggingLevel.Warning);
                    return;
                }

                // Detect current market regime (with performance monitoring)
                if (EnableRegimeDetection)
                {
                    var regimeStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    currentRegime = DetectCurrentRegime(RegimeLookbackPeriod);
                    regimeStopwatch.Stop();
                    LogMessage($"[PERFORMANCE] Regime detection: {regimeStopwatch.Elapsed.TotalMilliseconds:F3}ms, Result: {currentRegime}", LoggingLevel.OnlyImportant);
                }
                else
                {
                    currentRegime = MarketRegime.Ranging;
                }

                // Early exit conditions using pre-computed values
                if (preComputedIsTradeBlocked)
                {
                    int currentBarIndex = analysisBars.ClosePrices.Count - 1;
                    if (lastLossBarIndex >= 0 && currentBarIndex - lastLossBarIndex >= BlockCandles)
                    {
                        isTradeBlocked = false;
                        preComputedIsTradeBlocked = false; // Update pre-computed value
                    }
                    else
                    {
                        return;
                    }
                }

                // Use pre-computed weekend blocking condition
                if (preComputedIsWeekendBlocked) return;

                // Use pre-computed trading session condition
                if (!preComputedIsTradingAllowed) return;

                // Use pre-computed news event condition
                if (preComputedIsNewsEvent) return;

            // Declare variables at method level to avoid scoping issues
            bool allowLongs = TradeDirection == TradeDirectionMode.Both || TradeDirection == TradeDirectionMode.OnlyLongs;
            bool allowShorts = TradeDirection == TradeDirectionMode.Both || TradeDirection == TradeDirectionMode.OnlyShorts;
            bool reverseSignals = ReverseTrading == ReverseTradingMode.Yes;
            
            // Declare additional variables at method level
            double ma1Value = 0;
            double ma2Value = 0;
            double ma3Value = 0;
            bool isBullishCandle = false;
            bool isBearishCandle = false;
            bool isAboveAllMas = false;
            bool isBelowAllMas = false;
            double ma1ChangePercent = 0;
            bool hasSufficientMomentum = false;
            bool hasSufficientVolatility = false;
            bool hasStrongTrend = false;
            bool hasSufficientVolume = false;
            bool hasAcceptableSpread = false;
            bool rsiConditionBuy = false;
            bool rsiConditionSell = false;
            bool longEntryConditions = false;
            bool shortEntryConditions = false;
            bool hasSufficientMa2Ma3Spread = false;

            // Calculate common values for entry conditions
            ma1Value = GetCachedMa1Value(index);
            ma2Value = GetCachedMa2Value(index);
            ma3Value = GetCachedMa3Value(index);
            isBullishCandle = analysisBars.ClosePrices[index] > analysisBars.OpenPrices[index];
            isBearishCandle = analysisBars.ClosePrices[index] < analysisBars.OpenPrices[index];
            isAboveAllMas = analysisBars.ClosePrices[index] > ma2Value && analysisBars.ClosePrices[index] > ma3Value &&
                            (ConsiderMa1 ? analysisBars.ClosePrices[index] > ma1Value : true);
            isBelowAllMas = analysisBars.ClosePrices[index] < ma2Value && analysisBars.ClosePrices[index] < ma3Value &&
                            (ConsiderMa1 ? analysisBars.ClosePrices[index] < ma1Value : true);
            ma1ChangePercent = index >= Ma1Candles ? ((ma1Value - GetCachedMa1Value(index - Ma1Candles)) / GetCachedMa1Value(index - Ma1Candles)) * PERCENTAGE_MULTIPLIER : 0;
            hasSufficientMomentum = !UseMaFilter || Math.Abs(ma1ChangePercent) >= GetCachedMomentumThreshold(index);
            hasSufficientVolatility = !UseAtrFilter || GetCachedAtrValue(index) >= AtrThreshold;
            hasStrongTrend = !UseAdxFilter || adx.ADX[index] >= AdxThreshold;
            hasSufficientVolume = !UseVolumeFilter || (VolumeMode == VolumeFilterMode.Static ?
                                analysisBars.TickVolumes[index] >= MinVolume && analysisBars.TickVolumes[index] <= MaxVolume :
                                analysisBars.TickVolumes[index] >= CalculateDynamicMinVolume(index) && analysisBars.TickVolumes[index] <= CalculateDynamicMaxVolume(index));
            hasAcceptableSpread = !UseSpreadFilter || Symbol.Spread <= MaxSpreadPips;
            rsiConditionBuy = !UseRsiFilter || rsi.Result[index] >= RsiBuyThreshold;
            rsiConditionSell = !UseRsiFilter || rsi.Result[index] <= RsiSellThreshold;
            hasSufficientMa2Ma3Spread = !UseMa2Ma3Spread || GetCachedMa2Ma3Spread(index) >= Ma2Ma3Spread;
            switch (EntryCondition)
            {
                case EntryConditionType.Crossover:
                {
                    longEntryConditions = isAboveAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && isBullishCandle && index > 0 && analysisBars.ClosePrices[index - 1] <= GetCachedMa3Value(index - 1) && analysisBars.ClosePrices[index] > ma3Value && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = isBelowAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && isBearishCandle && index > 0 && analysisBars.ClosePrices[index - 1] >= GetCachedMa3Value(index - 1) && analysisBars.ClosePrices[index] < ma3Value && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
                case EntryConditionType.Breakout:
                {
                    longEntryConditions = isAboveAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && analysisBars.HighPrices[index] > ma3Value && index > 0 && analysisBars.HighPrices[index - 1] <= GetCachedMa3Value(index - 1) && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = isBelowAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && analysisBars.LowPrices[index] < ma3Value && index > 0 && analysisBars.LowPrices[index - 1] >= GetCachedMa3Value(index - 1) && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
                case EntryConditionType.OpenCloseCross:
                {
                    longEntryConditions = isAboveAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && analysisBars.OpenPrices[index] < ma3Value && analysisBars.ClosePrices[index] > ma3Value && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = isBelowAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && analysisBars.OpenPrices[index] > ma3Value && analysisBars.ClosePrices[index] < ma3Value && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
                case EntryConditionType.Pullback:
                {
                    longEntryConditions = isAboveAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && isBullishCandle && index > 0 && analysisBars.ClosePrices[index - 1] <= GetCachedMa3Value(index - 1) && analysisBars.ClosePrices[index - 1] > GetCachedMa2Value(index - 1) && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = isBelowAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && isBearishCandle && index > 0 && analysisBars.ClosePrices[index - 1] >= GetCachedMa3Value(index - 1) && analysisBars.ClosePrices[index - 1] < GetCachedMa2Value(index - 1) && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
                case EntryConditionType.MA2MA3Crossover:
                {
                    longEntryConditions = index > 0 && GetCachedMa3Value(index - 1) <= GetCachedMa2Value(index - 1) && GetCachedMa3Value(index) > GetCachedMa2Value(index) && isBullishCandle && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = index > 0 && GetCachedMa3Value(index - 1) >= GetCachedMa2Value(index - 1) && GetCachedMa3Value(index) < GetCachedMa2Value(index) && isBearishCandle && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
                case EntryConditionType.MA2ResistanceSupport:
                {
                    double distancePips = PriceDistanceToMa2 * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                    bool isLongBounce = index > 1 && analysisBars.ClosePrices[index - 2] <= GetCachedMa3Value(index - 2) &&
                                        (Math.Abs(analysisBars.LowPrices[index - 1] - GetCachedMa2Value(index - 1)) <= distancePips || Math.Abs(analysisBars.ClosePrices[index - 1] - GetCachedMa2Value(index - 1)) <= distancePips) &&
                                        analysisBars.ClosePrices[index - 1] > GetCachedMa2Value(index - 1) &&
                                        isBearishCandle;
                    bool isShortBounce = index > 1 && analysisBars.ClosePrices[index - 2] >= GetCachedMa3Value(index - 2) &&
                                         (Math.Abs(analysisBars.HighPrices[index - 1] - GetCachedMa2Value(index - 1)) <= distancePips || Math.Abs(analysisBars.ClosePrices[index - 1] - GetCachedMa2Value(index - 1)) <= distancePips) &&
                                         analysisBars.ClosePrices[index - 1] < GetCachedMa2Value(index - 1) &&
                                         isBullishCandle;
                    longEntryConditions = isAboveAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionBuy && isLongBounce && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    shortEntryConditions = isBelowAllMas && hasSufficientMomentum && hasSufficientVolatility && rsiConditionSell && isShortBounce && hasStrongTrend && hasSufficientVolume && hasAcceptableSpread;
                    break;
                }
            }
            if (UseMa2Ma3Spread)
            {
                if (isSpreadLongSignalActive || isSpreadShortSignalActive)
                {
                    spreadCandleCount++;
                    if (spreadCandleCount >= SpreadCandles)
                    {
                        isSpreadLongSignalActive = false;
                        isSpreadShortSignalActive = false;
                        spreadCandleCount = 0;
                    }
                    else if (hasSufficientMa2Ma3Spread)
                    {
                        if (isSpreadLongSignalActive && longEntryConditions && isBullishCandle)
                        {
                            TradeType tradeType = reverseSignals ? TradeType.Sell : TradeType.Buy;
                            if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                            {
                                TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                                if (result.IsSuccessful && result.Position != null)
                                {
                                    isSpreadLongSignalActive = false;
                                    spreadCandleCount = 0;
                                    lastEntryBarIndex = index;
                                }
                            }
                        }
                        else if (isSpreadShortSignalActive && shortEntryConditions && isBearishCandle)
                        {
                            TradeType tradeType = reverseSignals ? TradeType.Buy : TradeType.Sell;
                            if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                            {
                                TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                                if (result.IsSuccessful && result.Position != null)
                                {
                                    isSpreadShortSignalActive = false;
                                    spreadCandleCount = 0;
                                    lastEntryBarIndex = index;
                                }
                            }
                        }
                    }
                    return;
                }
            }
            if (UseConfirmationCandles)
            {
                if (isLongSignalActive)
                {
                    if (longEntryConditions && isBullishCandle)
                    {
                        consecutiveConfirmationCandles++;
                        if (consecutiveConfirmationCandles >= ConfirmationCandles)
                        {
                            if (UseMa2Ma3Spread)
                            {
                                isSpreadLongSignalActive = true;
                                spreadCandleCount = 0;
                            }
                            else
                            {
                                TradeType tradeType = reverseSignals ? TradeType.Sell : TradeType.Buy;
                                if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                                {
                                    TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                                    if (result.IsSuccessful && result.Position != null)
                                    {
                                        consecutiveConfirmationCandles = 0;
                                        isLongSignalActive = false;
                                        lastEntryBarIndex = index;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        isLongSignalActive = false;
                        consecutiveConfirmationCandles = 0;
                    }
                }
                else if (isShortSignalActive)
                {
                    if (shortEntryConditions && isBearishCandle)
                    {
                        consecutiveConfirmationCandles++;
                        if (consecutiveConfirmationCandles >= ConfirmationCandles)
                        {
                            if (UseMa2Ma3Spread)
                            {
                                isSpreadShortSignalActive = true;
                                spreadCandleCount = 0;
                            }
                            else
                            {
                                TradeType tradeType = reverseSignals ? TradeType.Buy : TradeType.Sell;
                                if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                                {
                                    TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                                    if (result.IsSuccessful && result.Position != null)
                                    {
                                        consecutiveConfirmationCandles = 0;
                                        isShortSignalActive = false;
                                        lastEntryBarIndex = index;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        isShortSignalActive = false;
                        consecutiveConfirmationCandles = 0;
                    }
                }
                else
                {
                    if (longEntryConditions)
                    {
                        isLongSignalActive = true;
                        consecutiveConfirmationCandles = isBullishCandle ? 1 : 0;
                    }
                    else if (shortEntryConditions)
                    {
                        isShortSignalActive = true;
                        consecutiveConfirmationCandles = isBearishCandle ? 1 : 0;
                    }
                }
            }
            else
            {
                if (longEntryConditions)
                {
                    if (UseMa2Ma3Spread)
                    {
                        isSpreadLongSignalActive = true;
                        spreadCandleCount = 0;
                    }
                    else
                    {
                        if (Positions.Count > 0) return;
                        TradeType tradeType = reverseSignals ? TradeType.Sell : TradeType.Buy;
                        if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                        {
                            TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                            if (result.IsSuccessful && result.Position != null)
                            {
                                lastEntryBarIndex = index;
                            }
                        }
                    }
                }
                else if (shortEntryConditions)
                {
                    if (UseMa2Ma3Spread)
                    {
                        isSpreadShortSignalActive = true;
                        spreadCandleCount = 0;
                    }
                    else
                    {
                        if (Positions.Count > 0) return;
                        TradeType tradeType = reverseSignals ? TradeType.Buy : TradeType.Sell;
                        if ((tradeType == TradeType.Buy && allowLongs) || (tradeType == TradeType.Sell && allowShorts))
                        {
                            TradeResult result = ExecuteMarketOrder(tradeType, Symbol.Name, CalculateVolume(atr.LastValue), tradeType == TradeType.Buy ? "Long Entry" : "Short Entry");
                            if (result.IsSuccessful && result.Position != null)
                            {
                                lastEntryBarIndex = index;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Critical error in CheckForEntries: {ex.Message}", LoggingLevel.Error);
        }
        }
        private void HandlePositionExit(Position position, string exitType)
        {
            try
            {
                // Validate position data
                if (position == null)
                {
                    LogMessage("Error: Position object is null in HandlePositionExit", LoggingLevel.Error);
                    return;
                }

                // Validate bars data
                if (Bars == null || Bars.ClosePrices == null || Bars.ClosePrices.Count == 0)
                {
                    LogMessage("Error: Bars data not available for exit marker placement", LoggingLevel.Error);
                    return;
                }

                double markPrice = Bars.ClosePrices.LastValue;
                if (markPrice <= 0)
                {
                    LogMessage($"Warning: Invalid mark price {markPrice} for exit marker", LoggingLevel.Warning);
                    return;
                }

                // Calculate marker offset with validation
                double markOffset = 0;
                if (Symbol != null && Symbol.PipSize > 0)
                {
                    markOffset = position.TradeType == TradeType.Buy ? -Symbol.PipSize * 2 : Symbol.PipSize * 2;
                }
                else
                {
                    LogMessage("Warning: Using default marker offset due to invalid symbol data", LoggingLevel.Warning);
                    markOffset = position.TradeType == TradeType.Buy ? BUY_MARKER_OFFSET : SELL_MARKER_OFFSET;
                }

                // Calculate profit percentage with validation
                double profitPercent = 0;
                if (position.EntryPrice > 0 && Symbol != null && Symbol.PipSize > 0)
                {
                    profitPercent = (position.Pips * Symbol.PipSize / position.EntryPrice) * 100;
                }
                else
                {
                    LogMessage("Warning: Cannot calculate profit percentage due to invalid entry price or symbol data", LoggingLevel.Warning);
                }

                // Determine marker color and icon
                Color markerColor = position.NetProfit > 0 ? Color.LimeGreen : Color.Red;
                ChartIconType iconType = position.NetProfit > 0
                    ? (position.TradeType == TradeType.Buy ? ChartIconType.UpArrow : ChartIconType.DownArrow)
                    : (position.TradeType == TradeType.Buy ? ChartIconType.DownArrow : ChartIconType.UpArrow);

                // Override colors for special exit types
                if (exitType == "HardClose")
                    markerColor = Color.Orange;
                else if (exitType == "SoftClose")
                    markerColor = Color.Yellow;
                else if (exitType == "AvoidSwap")
                    markerColor = Color.Magenta;

                // Skip chart markers if disabled
                if (ShowChartMarkers == ChartMarkerMode.No)
                {
                    LogMessage("Chart markers disabled, skipping marker placement", LoggingLevel.Info);
                }
                else
                {
                    // Draw exit markers using batching system
                    try
                    {
                        string textId = $"Exit {exitType}_" + Bars.OpenTimes.LastValue.Ticks;
                        string text = $"Exit {exitType} ({position.Pips:F0} pips, {(profitPercent >= 0 ? "+" : "")}{profitPercent:F1}%)";
                        AddToChartBatch(textId, ChartObjectType.Text, Bars.OpenTimes.LastValue,
                                      markPrice + markOffset, markerColor, text,
                                      ChartIconType.Circle, 0, MarkerPriority.High);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error drawing exit text marker: {ex.Message}", LoggingLevel.Error);
                    }

                    try
                    {
                        string currencyTextId = $"Exit {exitType} Currency_" + Bars.OpenTimes.LastValue.Ticks;
                        string currencyText = $"{(position.NetProfit >= 0 ? "+" : "")}{position.NetProfit:F2} {Account?.Asset?.Name ?? "USD"}";
                        double currencyOffset = position.TradeType == TradeType.Buy
                            ? -(Symbol != null && Symbol.PipSize > 0 ? Symbol.PipSize * CHART_CURRENCY_MULTIPLIER : CURRENCY_TEXT_OFFSET)
                            : (Symbol != null && Symbol.PipSize > 0 ? Symbol.PipSize * CHART_CURRENCY_MULTIPLIER : CURRENCY_TEXT_OFFSET);
                        AddToChartBatch(currencyTextId, ChartObjectType.Text, Bars.OpenTimes.LastValue,
                                      markPrice + markOffset + currencyOffset, markerColor, currencyText,
                                      ChartIconType.Circle, 0, MarkerPriority.Normal);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error drawing currency text marker: {ex.Message}", LoggingLevel.Error);
                    }

                    try
                    {
                        string iconId = $"Exit {exitType} Arrow_" + Bars.OpenTimes.LastValue.Ticks;
                        AddToChartBatch(iconId, ChartObjectType.Icon, Bars.OpenTimes.LastValue,
                                      markPrice + markOffset, markerColor, "",
                                      iconType, 0, MarkerPriority.High);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error drawing exit icon marker: {ex.Message}", LoggingLevel.Error);
                    }
                }

                // Update trade statistics with validation
                try
                {
                    totalTrades++;

                    // Calculate trade return with division by zero protection
                    double denominator = Account.Balance - position.NetProfit;
                    double tradeReturn = 0;

                    if (Math.Abs(denominator) > double.Epsilon)
                    {
                        tradeReturn = position.NetProfit / denominator;
                        if (double.IsNaN(tradeReturn) || double.IsInfinity(tradeReturn))
                        {
                            LogMessage($"Warning: Invalid trade return calculated: {tradeReturn}, setting to 0", LoggingLevel.Warning);
                            tradeReturn = 0;
                        }
                    }
                    else
                    {
                        LogMessage("Warning: Denominator too small for trade return calculation, using 0", LoggingLevel.Warning);
                    }

                    tradeReturns.Add(tradeReturn);

                    // Update win/loss statistics
                    if (position.NetProfit > 0)
                    {
                        consecutiveTPs++;
                        consecutiveSLs = 0;
                        lastTradeWasTP = true;
                        lastTradeWasSL = false;
                        winningTrades++;
                        totalProfit += position.NetProfit;
                    }
                    else
                    {
                        consecutiveSLs++;
                        consecutiveTPs = 0;
                        lastTradeWasTP = false;
                        lastTradeWasSL = true;
                        losingTrades++;
                        totalLoss += Math.Abs(position.NetProfit);

                        // Check for trade blocking due to consecutive losses
                        if (consecutiveSLs >= MaxConsecutiveLosses)
                        {
                            isTradeBlocked = true;
                            lastLossBarIndex = Bars.ClosePrices.Count - 1;
                            LogMessage($"Trade blocking activated due to {consecutiveSLs} consecutive losses", LoggingLevel.Warning);
                        }
                    }

                    // Update total pips
                    totalPips += position.Pips;

                    // Update recent profits list
                    if (recentProfits.Count >= RecentTradesLookback * 2)
                        recentProfits.RemoveAt(0);
                    recentProfits.Add(position.NetProfit);

                    // Analyze and adapt parameters
                    AnalyzeAndAdapt();

                    isTrailingStopActive = false;
                }
                catch (Exception ex)
                {
                    LogMessage($"Error updating trade statistics: {ex.Message}", LoggingLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in HandlePositionExit: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void OnPositionOpenedEvent(PositionOpenedEventArgs args)
        {
            try
            {
                // Validate event arguments
                if (args == null)
                {
                    LogMessage("Error: PositionOpenedEventArgs is null", LoggingLevel.Error);
                    return;
                }

                var position = args.Position;
                if (position == null)
                {
                    LogMessage("Error: Position object is null in OnPositionOpenedEvent", LoggingLevel.Error);
                    return;
                }

                // Validate symbol data
                if (Symbol == null)
                {
                    LogMessage("Error: Symbol is null, cannot calculate position levels", LoggingLevel.Error);
                    return;
                }

                // Calculate stop loss distance with validation
                double slDistance;
                if (RiskMode == RiskManagementMode.DynamicATR)
                {
                    if (atr == null || atr.Count == 0 || double.IsNaN(atr.LastValue))
                    {
                        LogMessage("Warning: Invalid ATR value, using static stop loss", LoggingLevel.Warning);
                        slDistance = StopLossPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                    }
                    else
                    {
                        slDistance = atr.LastValue * AtrMultiplierSl;
                        if (double.IsNaN(slDistance) || double.IsInfinity(slDistance) || slDistance <= 0)
                        {
                            LogMessage($"Warning: Invalid ATR-based SL distance {slDistance}, using static", LoggingLevel.Warning);
                            slDistance = StopLossPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                        }
                    }
                }
                else
                {
                    slDistance = StopLossPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                }

                // Calculate take profit distance with validation
                double tpDistance;
                if (RiskMode == RiskManagementMode.DynamicATR)
                {
                    if (atr == null || atr.Count == 0 || double.IsNaN(atr.LastValue))
                    {
                        LogMessage("Warning: Invalid ATR value, using static take profit", LoggingLevel.Warning);
                        tpDistance = TakeProfitPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                    }
                    else
                    {
                        tpDistance = atr.LastValue * AtrMultiplierTp;
                        if (double.IsNaN(tpDistance) || double.IsInfinity(tpDistance) || tpDistance <= 0)
                        {
                            LogMessage($"Warning: Invalid ATR-based TP distance {tpDistance}, using static", LoggingLevel.Warning);
                            tpDistance = TakeProfitPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                        }
                    }
                }
                else
                {
                    tpDistance = TakeProfitPips * (Symbol.PipSize > 0 ? Symbol.PipSize : 0.00001);
                }

                // Calculate actual stop loss and take profit levels
                double sl, tp;
                try
                {
                    if (position.TradeType == TradeType.Buy)
                    {
                        sl = position.EntryPrice - slDistance;
                        tp = position.EntryPrice + tpDistance;
                    }
                    else
                    {
                        sl = position.EntryPrice + slDistance;
                        tp = position.EntryPrice - tpDistance;
                    }

                    // Validate calculated levels
                    if (double.IsNaN(sl) || double.IsInfinity(sl) || double.IsNaN(tp) || double.IsInfinity(tp))
                    {
                        LogMessage($"Error: Invalid SL/TP levels calculated. SL: {sl}, TP: {tp}", LoggingLevel.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error calculating SL/TP levels: {ex.Message}", LoggingLevel.Error);
                    return;
                }

                // Modify position with error handling
                try
                {
                    ModifyPosition(position, sl, tp, ProtectionType.Absolute);
                    LogMessage($"Position modified - SL: {sl:F5}, TP: {tp:F5}", LoggingLevel.OnlyTrades);
                }
                catch (Exception ex)
                {
                    LogMessage($"Error modifying position: {ex.Message}", LoggingLevel.Error);
                    return;
                }

                isTrailingStopActive = false;

                // Record trade entry for learning
                RecordTradeEntry(position);

                // Skip chart markers if disabled
                if (ShowChartMarkers == ChartMarkerMode.No)
                {
                    return;
                }

                // Draw entry markers with batching system
                try
                {
                    DateTime entryTime = position.EntryTime;
                    double entryPrice = position.EntryPrice;

                    if (Symbol.PipSize > 0)
                    {
                        if (position.TradeType == TradeType.Buy)
                        {
                            // Draw long entry markers using batching
                            AddToChartBatch("Entry Long_" + entryTime.Ticks, ChartObjectType.Icon, entryTime,
                                          entryPrice - Symbol.PipSize, Color.LimeGreen, "",
                                          ChartIconType.UpArrow, 0, MarkerPriority.High);

                            AddToChartBatch("Buy_" + entryTime.Ticks, ChartObjectType.Text, entryTime,
                                          entryPrice - Symbol.PipSize * CHART_TEXT_MULTIPLIER, Color.LimeGreen,
                                          "Buy", ChartIconType.Circle, 0, MarkerPriority.Normal);
                        }
                        else
                        {
                            // Draw short entry markers using batching
                            AddToChartBatch("Entry Short_" + entryTime.Ticks, ChartObjectType.Icon, entryTime,
                                          entryPrice + Symbol.PipSize, Color.Red, "",
                                          ChartIconType.DownArrow, 0, MarkerPriority.High);

                            AddToChartBatch("Sell_" + entryTime.Ticks, ChartObjectType.Text, entryTime,
                                          entryPrice + Symbol.PipSize * CHART_TEXT_MULTIPLIER, Color.Orange,
                                          "Sell", ChartIconType.Circle, 0, MarkerPriority.Normal);
                        }
                    }
                    else
                    {
                        LogMessage("Warning: Cannot draw entry markers due to invalid pip size", LoggingLevel.Warning);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in entry marker placement: {ex.Message}", LoggingLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in OnPositionOpenedEvent: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void OnPositionClosedEvent(PositionClosedEventArgs args)
        {
            var position = args.Position;
            string exitType = "Unknown";
            double entryTimeHours = (Server.Time - position.EntryTime).TotalHours;
            if (args.Reason == PositionCloseReason.StopLoss)
            {
                exitType = isTrailingStopActive ? "TrailingStop" : "StopLoss";
            }
            else if (args.Reason == PositionCloseReason.TakeProfit)
            {
                exitType = "TakeProfit";
            }
            else if (args.Reason == PositionCloseReason.Closed)
            {
                if (UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlyHard)
                {
                    if (entryTimeHours > TimeLimitHard)
                    {
                        exitType = "HardClose";
                    }
                }
                if (UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlySoft)
                {
                    if (entryTimeHours > TimeLimitSoft && position.NetProfit > 0)
                    {
                        exitType = "SoftClose";
                    }
                }
                if (AvoidSwaps && position.NetProfit > 0 && Server.Time.TimeOfDay >= TimeSpan.FromHours(SWAP_AVOIDANCE_HOUR))
                {
                    exitType = "AvoidSwap";
                }
            }
            if (exitType != "Unknown")
            {
                HandlePositionExit(position, exitType);
                // Record trade exit for learning
                RecordTradeExit(position);
            }
        }

        private void ManagePositions()
        {
            try
            {
                // Validate essential data
                if (Positions == null)
                {
                    LogMessage("Error: Positions collection is null", LoggingLevel.OnlyCritical);
                    return;
                }

                if (Server == null)
                {
                    LogMessage("Error: Server object is null", LoggingLevel.OnlyCritical);
                    return;
                }

                if (Symbol == null)
                {
                    LogMessage("Error: Symbol object is null", LoggingLevel.OnlyCritical);
                    return;
                }

                foreach (var position in Positions)
                {
                    try
                    {
                        // Validate position data
                        if (position == null)
                        {
                            LogMessage("Warning: Null position found in Positions collection, skipping", LoggingLevel.Warning);
                            continue;
                        }

                        // Calculate entry time hours with validation
                        double entryTimeHours;
                        try
                        {
                            entryTimeHours = (Server.Time - position.EntryTime).TotalHours;
                            if (double.IsNaN(entryTimeHours) || double.IsInfinity(entryTimeHours))
                            {
                                LogMessage($"Warning: Invalid entry time hours calculated for position {position.Id}: {entryTimeHours}", LoggingLevel.Warning);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error calculating entry time for position {position.Id}: {ex.Message}", LoggingLevel.Error);
                            continue;
                        }

                        // Handle hard time limit closure
                        if ((UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlyHard) && entryTimeHours > TimeLimitHard && entryTimeHours > 0)
                        {
                            try
                            {
                                ClosePosition(position);
                                LogMessage($"Position {position.Id} closed due to hard time limit ({entryTimeHours:F1} > {TimeLimitHard} hours)", LoggingLevel.OnlyTrades);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Error closing position {position.Id} for hard time limit: {ex.Message}", LoggingLevel.Error);
                                continue;
                            }
                        }

                        // Handle soft time limit closure
                        if ((UseTimeLimits == TimeLimitMode.Yes || UseTimeLimits == TimeLimitMode.OnlySoft) && entryTimeHours > TimeLimitSoft && position.NetProfit > 0 && entryTimeHours > 0)
                        {
                            try
                            {
                                ClosePosition(position);
                                LogMessage($"Position {position.Id} closed due to soft time limit ({entryTimeHours:F1} > {TimeLimitSoft} hours, profit: {position.NetProfit:F2})", LoggingLevel.OnlyTrades);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Error closing position {position.Id} for soft time limit: {ex.Message}", LoggingLevel.Error);
                                continue;
                            }
                        }

                        // Handle swap avoidance closure
                        if (AvoidSwaps && position.NetProfit > 0)
                        {
                            try
                            {
                                TimeSpan currentTimeOfDay = Server.Time.TimeOfDay;
                                if (currentTimeOfDay >= TimeSpan.FromHours(SWAP_AVOIDANCE_HOUR))
                                {
                                    ClosePosition(position);
                                    LogMessage($"Position {position.Id} closed to avoid swap (profit: {position.NetProfit:F2})", LoggingLevel.OnlyTrades);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Error checking/closing position {position.Id} for swap avoidance: {ex.Message}", LoggingLevel.Error);
                                continue;
                            }
                        }

                        // Handle trailing stop logic
                        if (TrailingStop != TrailingStopMode.Off && position.NetProfit > 0 && Symbol.PipSize > 0)
                        {
                            try
                            {
                                // Calculate distance in pips with validation
                                double distanceInPips;
                                if (position.TradeType == TradeType.Buy)
                                {
                                    if (Symbol.Ask <= 0 || position.EntryPrice <= 0)
                                    {
                                        LogMessage($"Warning: Invalid prices for trailing stop calculation on position {position.Id}", LoggingLevel.Warning);
                                        continue;
                                    }
                                    distanceInPips = (Symbol.Ask - position.EntryPrice) / Symbol.PipSize;
                                }
                                else
                                {
                                    if (position.EntryPrice <= 0 || Symbol.Bid <= 0)
                                    {
                                        LogMessage($"Warning: Invalid prices for trailing stop calculation on position {position.Id}", LoggingLevel.Warning);
                                        continue;
                                    }
                                    distanceInPips = (position.EntryPrice - Symbol.Bid) / Symbol.PipSize;
                                }

                                if (double.IsNaN(distanceInPips) || double.IsInfinity(distanceInPips))
                                {
                                    LogMessage($"Warning: Invalid distance in pips calculated: {distanceInPips}", LoggingLevel.Warning);
                                    continue;
                                }

                                // Calculate trailing stop trigger
                                double tsTrigger;
                                if (RiskMode == RiskManagementMode.DynamicATR)
                                {
                                    if (atr == null || atr.Count == 0 || double.IsNaN(atr.LastValue) || Symbol.PipSize <= 0)
                                    {
                                        LogMessage("Warning: Invalid ATR data for trailing stop trigger, using static", LoggingLevel.Warning);
                                        tsTrigger = TrailingStopTriggerPips;
                                    }
                                    else
                                    {
                                        tsTrigger = atr.LastValue * AtrMultiplierTsTrigger / Symbol.PipSize;
                                        if (double.IsNaN(tsTrigger) || double.IsInfinity(tsTrigger) || tsTrigger <= 0)
                                        {
                                            LogMessage($"Warning: Invalid ATR-based TS trigger {tsTrigger}, using static", LoggingLevel.Warning);
                                            tsTrigger = TrailingStopTriggerPips;
                                        }
                                    }
                                }
                                else
                                {
                                    tsTrigger = TrailingStopTriggerPips;
                                }

                                // Check if trailing stop should be activated
                                if (distanceInPips >= tsTrigger)
                                {
                                    // Calculate trailing stop distance
                                    double tsDistance;
                                    if (RiskMode == RiskManagementMode.DynamicATR)
                                    {
                                        if (atr == null || atr.Count == 0 || double.IsNaN(atr.LastValue))
                                        {
                                            LogMessage("Warning: Invalid ATR data for trailing stop distance, using static", LoggingLevel.Warning);
                                            tsDistance = TrailingStopPips * Symbol.PipSize;
                                        }
                                        else
                                        {
                                            tsDistance = atr.LastValue * AtrMultiplierTsDistance;
                                            if (double.IsNaN(tsDistance) || double.IsInfinity(tsDistance) || tsDistance <= 0)
                                            {
                                                LogMessage($"Warning: Invalid ATR-based TS distance {tsDistance}, using static", LoggingLevel.Warning);
                                                tsDistance = TrailingStopPips * Symbol.PipSize;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tsDistance = TrailingStopPips * Symbol.PipSize;
                                    }

                                    // Calculate new trailing stop level
                                    double trailTrigger;
                                    if (position.TradeType == TradeType.Buy)
                                    {
                                        if (Symbol.Bid <= 0)
                                        {
                                            LogMessage($"Warning: Invalid bid price for trailing stop on position {position.Id}", LoggingLevel.Warning);
                                            continue;
                                        }
                                        trailTrigger = Symbol.Bid - tsDistance;
                                    }
                                    else
                                    {
                                        if (Symbol.Ask <= 0)
                                        {
                                            LogMessage($"Warning: Invalid ask price for trailing stop on position {position.Id}", LoggingLevel.Warning);
                                            continue;
                                        }
                                        trailTrigger = Symbol.Ask + tsDistance;
                                    }

                                    if (double.IsNaN(trailTrigger) || double.IsInfinity(trailTrigger))
                                    {
                                        LogMessage($"Warning: Invalid trail trigger calculated: {trailTrigger}", LoggingLevel.Warning);
                                        continue;
                                    }

                                    // Apply trailing stop if conditions are met
                                    bool shouldTrail = false;
                                    if (position.TradeType == TradeType.Buy && (!position.StopLoss.HasValue || trailTrigger > position.StopLoss.Value))
                                    {
                                        shouldTrail = true;
                                    }
                                    else if (position.TradeType == TradeType.Sell && (!position.StopLoss.HasValue || trailTrigger < position.StopLoss.Value))
                                    {
                                        shouldTrail = true;
                                    }

                                    if (shouldTrail)
                                    {
                                        try
                                        {
                                            // Handle different trailing stop modes
                                            double? newTP = position.TakeProfit;
                                            if (TrailingStop == TrailingStopMode.TSRemovesTP)
                                            {
                                                newTP = null; // Remove take profit when trailing stop activates
                                            }
                                            // TSPlusTP keeps the existing take profit

                                            ModifyPosition(position, trailTrigger, newTP, ProtectionType.Absolute);
                                            LogMessage($"Trailing stop updated for position {position.Id} to {trailTrigger:F5} (Mode: {TrailingStop})", LoggingLevel.OnlyTrades);

                                            // Draw trailing stop update marker using batching
                                            if (ShowChartMarkers == ChartMarkerMode.Yes && Bars != null && Bars.OpenTimes != null && Bars.ClosePrices != null && Bars.ClosePrices.Count > 0)
                                            {
                                                try
                                                {
                                                    AddToChartBatch("TS Update_" + position.Id, ChartObjectType.Text, Bars.OpenTimes.LastValue,
                                                                   Bars.ClosePrices.LastValue, Color.LightBlue, "TS Update",
                                                                   ChartIconType.Circle, 0, MarkerPriority.Normal);
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogMessage($"Error drawing trailing stop marker for position {position.Id}: {ex.Message}", LoggingLevel.Error);
                                                }
                                            }

                                            isTrailingStopActive = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage($"Error modifying trailing stop for position {position.Id}: {ex.Message}", LoggingLevel.Error);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Error in trailing stop logic for position {position.Id}: {ex.Message}", LoggingLevel.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error processing position {position.Id}: {ex.Message}", LoggingLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Critical error in ManagePositions: {ex.Message}", LoggingLevel.OnlyCritical);
            }
        }

        private void AnalyzeAndAdapt()
        {
            if (!EnableLearningFeatures)
            {
                return;
            }

            try
            {
                // Ensure we have enough recent trades for meaningful analysis
                if (recentProfits.Count < RecentTradesLookback)
                {
                    LogMessage($"[LEARNING] Not enough recent trades for analysis: {recentProfits.Count}/{RecentTradesLookback}", LoggingLevel.Info);
                    return;
                }

                // Calculate win rate from recent trades to assess performance
                int recentWins = 0;
                for (int i = Math.Max(0, recentProfits.Count - RecentTradesLookback); i < recentProfits.Count; i++)
                {
                    if (recentProfits[i] > 0)
                        recentWins++;
                }
                double winRate = (double)recentWins / RecentTradesLookback;

                LogMessage($"[LEARNING] Analyzing recent performance: {recentWins}/{RecentTradesLookback} wins ({winRate:P2})", LoggingLevel.OnlyImportant);

                // Store original values for comparison
                double originalAtrThreshold = AtrThreshold;
                double originalRsiBuyThreshold = RsiBuyThreshold;
                double originalRsiSellThreshold = RsiSellThreshold;

                // Adaptive parameter adjustment based on win rate performance
                if (winRate < 0.4)
                {
                    // Tighten filters - poor performance indicates need for more restrictive entry conditions
                    AtrThreshold *= 1.1;
                    RsiBuyThreshold = Math.Min(RsiBuyThreshold + 5, 40);
                    RsiSellThreshold = Math.Min(RsiSellThreshold + 5, 80);
                    LogMessage($"[LEARNING] Tightening filters due to low win rate (<40%)", LoggingLevel.OnlyImportant);
                    LogMessage($"[LEARNING] Increased ATR threshold by 10% for stricter volatility requirements", LoggingLevel.OnlyImportant);
                    LogMessage($"[LEARNING] Increased RSI buy threshold by 5 points for more oversold conditions", LoggingLevel.OnlyImportant);
                    LogMessage($"[LEARNING] Increased RSI sell threshold by 5 points for more overbought conditions", LoggingLevel.OnlyImportant);
                }
                else if (winRate > 0.6)
                {
                    // Loosen filters - good performance indicates current filters may be too restrictive
                    AtrThreshold *= 0.95;
                    RsiBuyThreshold = Math.Max(RsiBuyThreshold - 2, 20);
                    RsiSellThreshold = Math.Max(RsiSellThreshold - 2, 60);
                    LogMessage($"[LEARNING] Loosening filters due to high win rate (>60%)", LoggingLevel.Info);
                    LogMessage($"[LEARNING] Decreased ATR threshold by 5% for more flexible volatility requirements", LoggingLevel.Info);
                    LogMessage($"[LEARNING] Decreased RSI buy threshold by 2 points for earlier entry signals", LoggingLevel.Info);
                    LogMessage($"[LEARNING] Decreased RSI sell threshold by 2 points for earlier exit signals", LoggingLevel.Info);
                }
                else
                {
                    LogMessage($"[LEARNING] Win rate in acceptable range (40%-60%), no parameter adjustments needed", LoggingLevel.Info);
                }

                // Apply bounds to prevent extreme values that could break the system
                                AtrThreshold = Math.Max(ATRThresholdMin, Math.Min(ATRThresholdMax, AtrThreshold));
                                RsiBuyThreshold = Math.Max(RSIBuyThresholdMin, Math.Min(RSIBuyThresholdMax, RsiBuyThreshold));
                                RsiSellThreshold = Math.Max(RSISellThresholdMin, Math.Min(RSISellThresholdMax, RsiSellThreshold));
                
                                // Log parameter changes for transparency
                                if (Math.Abs(AtrThreshold - originalAtrThreshold) > 0.00001)
                                {
                                    LogMessage($"[LEARNING] ATR Threshold adjusted: {originalAtrThreshold:F4} → {AtrThreshold:F4} ({((AtrThreshold - originalAtrThreshold) / originalAtrThreshold * 100):F1}%)", LoggingLevel.Info);
                                }
                                if (Math.Abs(RsiBuyThreshold - originalRsiBuyThreshold) > 0.1)
                                {
                                    LogMessage($"[LEARNING] RSI Buy Threshold adjusted: {originalRsiBuyThreshold:F1} → {RsiBuyThreshold:F1} ({(RsiBuyThreshold - originalRsiBuyThreshold):F1} points)", LoggingLevel.Info);
                                }
                                if (Math.Abs(RsiSellThreshold - originalRsiSellThreshold) > 0.1)
                                {
                                    LogMessage($"[LEARNING] RSI Sell Threshold adjusted: {originalRsiSellThreshold:F1} → {RsiSellThreshold:F1} ({(RsiSellThreshold - originalRsiSellThreshold):F1} points)", LoggingLevel.Info);
                                }

                LogMessage($"[LEARNING] Final parameters: ATR Threshold = {AtrThreshold:F4}, RSI Buy Threshold = {RsiBuyThreshold:F1}, RSI Sell Threshold = {RsiSellThreshold:F1}", LoggingLevel.OnlyImportant);

                // Perform correlation analysis and generate optimization suggestions
                if (EnableCorrelationAnalysis && correlationData.Count >= MinimumSampleRequirements)
                {
                    GenerateOptimizationSuggestions();
                    UpdateRollingCorrelations();

                    // Apply correlation-based optimizations if available
                    if (optimizationSuggestions.Count > 0)
                    {
                        var topSuggestion = optimizationSuggestions.First();
                        if (topSuggestion.ExpectedImprovement > 5.0) // Only apply if expected improvement is significant
                        {
                            ApplyOptimizationSuggestion(topSuggestion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[LEARNING] Error in AnalyzeAndAdapt: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void RecordTradeEntry(Position position)
        {
            try
            {
                var tradeData = new TradeLearningData();

                // Trade metadata
                tradeData.EntryTime = position.EntryTime;
                tradeData.EntryPrice = position.EntryPrice;

                // Market conditions at entry
                int index = analysisBars.ClosePrices.Count - 1;
            
                // Bounds checking to prevent IndexOutOfRangeException
                if (index < 0 || index >= analysisBars.ClosePrices.Count || index >= analysisBars.TickVolumes.Count || (rsi != null && index >= rsi.Result.Count) || (adx != null && index >= adx.ADX.Count) || (index - Ma1Candles < 0)) {
                    LogMessage($"[TRADE_LEARNING] Invalid index {index} for trade entry recording, skipping", LoggingLevel.Warning);
                    return;
                }
            
                tradeData.AtrAtEntry = GetCachedAtrValue(index);
                tradeData.AdxAtEntry = adx != null && adx.ADX.Count > index ? adx.ADX[index] : 0;
                tradeData.RsiAtEntry = rsi != null && rsi.Result.Count > index ? rsi.Result[index] : 0;
                tradeData.VolumeAtEntry = analysisBars.TickVolumes.Count > index ? analysisBars.TickVolumes[index] : 0;
                tradeData.SpreadAtEntry = Symbol.Spread;

                // Bot parameters at time of trade
                tradeData.AtrThresholdAtEntry = AtrThreshold;
                tradeData.RsiBuyThresholdAtEntry = RsiBuyThreshold;
                tradeData.RsiSellThresholdAtEntry = RsiSellThreshold;
                tradeData.MomentumThresholdAtEntry = GetCachedMomentumThreshold(index);

                // Market regime and trend information
                tradeData.RegimeAtEntry = currentRegime;
                double ma1ChangePercent = index >= Ma1Candles ? ((GetCachedMa1Value(index) - GetCachedMa1Value(index - Ma1Candles)) / GetCachedMa1Value(index - Ma1Candles)) * 100 : 0;
                tradeData.TrendDirectionAtEntry = ma1ChangePercent >= 0.1 ? "Bullish" : ma1ChangePercent <= -0.1 ? "Bearish" : "Sideways";

                // Add to history
                tradeHistory.Add(tradeData);

                // Memory management - remove oldest entries if exceeding max size
                if (tradeHistory.Count > MaxTradeHistorySize)
                {
                    tradeHistory.RemoveAt(0);
                }

                LogMessage($"[TRADE_LEARNING] Recorded trade entry: {position.TradeType} at {position.EntryPrice:F5}, Regime: {currentRegime}, Trend: {tradeData.TrendDirectionAtEntry}", LoggingLevel.OnlyTrades);
            }
            catch (Exception ex)
            {
                LogMessage($"[TRADE_LEARNING] Error recording trade entry: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void RecordTradeExit(Position position)
        {
            try
            {
                // Find the corresponding entry record
                var entryRecord = tradeHistory.LastOrDefault(t => t.EntryTime == position.EntryTime && t.EntryPrice == position.EntryPrice);
                if (entryRecord == null)
                {
                    LogMessage($"[TRADE_LEARNING] Warning: Could not find matching entry record for exit at {position.EntryTime}", LoggingLevel.Warning);
                    return;
                }

                // Update exit data
                entryRecord.ExitTime = Server.Time;
                entryRecord.ExitPrice = position.EntryPrice + (position.Pips * Symbol.PipSize);
                entryRecord.HoldingTime = entryRecord.ExitTime - entryRecord.EntryTime;
                entryRecord.ProfitLoss = position.NetProfit;

                // Performance metrics
                entryRecord.IsWin = position.NetProfit > 0;
                entryRecord.Pips = position.Pips;
                entryRecord.ReturnPercentage = position.EntryPrice > 0 ? (position.NetProfit / position.EntryPrice) * 100 : 0;

                LogMessage($"[TRADE_LEARNING] Recorded trade exit: {position.TradeType}, P&L: {position.NetProfit:F2}, Win: {entryRecord.IsWin}, Holding Time: {entryRecord.HoldingTime.TotalHours:F1}h", LoggingLevel.OnlyTrades);

                // Update correlation analysis with new trade data
                if (EnableCorrelationAnalysis)
                {
                    UpdateCorrelationData(position);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[TRADE_LEARNING] Error recording trade exit: {ex.Message}", LoggingLevel.Error);
            }
        }

        // Cache management methods
        private void ClearCaches()
        {
            atrCache.Clear();
            ma1Cache.Clear();
            ma2Cache.Clear();
            ma3Cache.Clear();
            ma2Ma3SpreadCache.Clear();
            momentumThresholdCache.Clear();
            correlationCache.Clear();
            sharpeRatioCache.Clear();
        }

        private void ClearConditionCache()
        {
            conditionCache.Clear();
            conditionCacheTimestamps.Clear();
            conditionCacheHits = 0;
            conditionCacheMisses = 0;
            LogMessage("[CONDITION_CACHE] Cleared condition cache", LoggingLevel.OnlyImportant);
        }

        /// <summary>
        /// Pre-computes common conditions used in CheckForEntries for performance optimization
        /// </summary>
        private void PreComputeConditions(int index)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
                // Bounds checking to prevent IndexOutOfRangeException
                if (index < 0 || index >= analysisBars.ClosePrices.Count || index >= analysisBars.OpenPrices.Count || index >= analysisBars.TickVolumes.Count || (rsi != null && index >= rsi.Result.Count) || (adx != null && index >= adx.ADX.Count) || (index - Ma1Candles < 0)) {
                    LogMessage($"[PRECOMPUTE] Invalid index {index} for pre-computation, skipping", LoggingLevel.Warning);
                    return;
                }
        
                // Pre-compute trading restrictions
                preComputedIsTradeBlocked = isTradeBlocked;
                preComputedIsWeekendBlocked = UseWeekendFilter && IsWeekendBlocked();
                preComputedIsTradingAllowed = IsTradingAllowed();
                preComputedIsNewsEvent = UseNewsAvoid && IsNewsEvent();
        
                // Pre-compute MA values and basic price conditions
                preComputedMa1Value = GetCachedMa1Value(index);
                preComputedMa2Value = GetCachedMa2Value(index);
                preComputedMa3Value = GetCachedMa3Value(index);
                preComputedIsBullishCandle = analysisBars.ClosePrices[index] > analysisBars.OpenPrices[index];
                preComputedIsBearishCandle = analysisBars.ClosePrices[index] < analysisBars.OpenPrices[index];

                // Pre-compute position relative to MAs
                preComputedIsAboveAllMas = analysisBars.ClosePrices[index] > preComputedMa2Value &&
                                          analysisBars.ClosePrices[index] > preComputedMa3Value &&
                                          (ConsiderMa1 ? analysisBars.ClosePrices[index] > preComputedMa1Value : true);
                preComputedIsBelowAllMas = analysisBars.ClosePrices[index] < preComputedMa2Value &&
                                          analysisBars.ClosePrices[index] < preComputedMa3Value &&
                                          (ConsiderMa1 ? analysisBars.ClosePrices[index] < preComputedMa1Value : true);

                // Pre-compute momentum
                preComputedMa1ChangePercent = index >= Ma1Candles
                    ? ((preComputedMa1Value - GetCachedMa1Value(index - Ma1Candles)) / GetCachedMa1Value(index - Ma1Candles)) * PERCENTAGE_MULTIPLIER
                    : 0;
                preComputedHasSufficientMomentum = !UseMaFilter || Math.Abs(preComputedMa1ChangePercent) >= GetCachedMomentumThreshold(index);

                // Pre-compute filters
                preComputedHasSufficientVolatility = !UseAtrFilter || GetCachedAtrValue(index) >= AtrThreshold;
                preComputedHasStrongTrend = !UseAdxFilter || adx.ADX[index] >= AdxThreshold;
                preComputedHasSufficientVolume = !UseVolumeFilter ||
                    (VolumeMode == VolumeFilterMode.Static ?
                        analysisBars.TickVolumes[index] >= MinVolume && analysisBars.TickVolumes[index] <= MaxVolume :
                        analysisBars.TickVolumes[index] >= CalculateDynamicMinVolume(index) &&
                        analysisBars.TickVolumes[index] <= CalculateDynamicMaxVolume(index));
                preComputedHasAcceptableSpread = !UseSpreadFilter || Symbol.Spread <= MaxSpreadPips;

                // Pre-compute RSI conditions
                preComputedRsiConditionBuy = !UseRsiFilter || rsi.Result[index] >= RsiBuyThreshold;
                preComputedRsiConditionSell = !UseRsiFilter || rsi.Result[index] <= RsiSellThreshold;

                // Pre-compute MA2/MA3 spread
                preComputedHasSufficientMa2Ma3Spread = !UseMa2Ma3Spread || GetCachedMa2Ma3Spread(index) >= Ma2Ma3Spread;

                // Pre-compute entry conditions (simplified versions)
                preComputedLongEntryConditions = preComputedIsAboveAllMas && preComputedHasSufficientMomentum &&
                                               preComputedHasSufficientVolatility && preComputedRsiConditionBuy &&
                                               preComputedIsBullishCandle && preComputedHasStrongTrend &&
                                               preComputedHasSufficientVolume && preComputedHasAcceptableSpread;

                preComputedShortEntryConditions = preComputedIsBelowAllMas && preComputedHasSufficientMomentum &&
                                                preComputedHasSufficientVolatility && preComputedRsiConditionSell &&
                                                preComputedIsBearishCandle && preComputedHasStrongTrend &&
                                                preComputedHasSufficientVolume && preComputedHasAcceptableSpread;

                stopwatch.Stop();
                LogMessage($"[PRECOMPUTE] Condition pre-computation completed in {stopwatch.Elapsed.TotalMilliseconds:F3}ms", LoggingLevel.OnlyImportant);
            }
            catch (Exception ex)
            {
                LogMessage($"[PRECOMPUTE] Error in PreComputeConditions: {ex.Message}", LoggingLevel.Error);
            }
        }

        /// <summary>
        /// Checks if weekend blocking is active
        /// </summary>
        private bool IsWeekendBlocked()
        {
            var currentTime = Server.Time;
            var dayOfWeek = currentTime.DayOfWeek;
            var timeOfDay = currentTime.TimeOfDay;
            return (dayOfWeek == DayOfWeek.Friday && timeOfDay >= TimeSpan.FromHours(22 - HoursBeforeWeekend)) ||
                   (dayOfWeek == DayOfWeek.Saturday) ||
                   (dayOfWeek == DayOfWeek.Sunday && timeOfDay <= TimeSpan.FromHours(22 + HoursAfterWeekend));
        }

        // Cache statistics logging
        private void LogCacheStatistics()
        {
            if (LogLevel == LoggingLevel.Off || LogLevel == LoggingLevel.OnlyTrades)
                return;

            LogMessage($"[CACHE_STATS] ATR Cache - Size: {atrCache.Count}/{MaxCacheSize}, Hit Rate: {atrCache.HitRate:P2}, Evictions: {atrCache.Evictions}", LoggingLevel.Info);
            LogMessage($"[CACHE_STATS] MA1 Cache - Size: {ma1Cache.Count}/{MaxCacheSize}, Hit Rate: {ma1Cache.HitRate:P2}, Evictions: {ma1Cache.Evictions}", LoggingLevel.Info);
            LogMessage($"[CACHE_STATS] MA2 Cache - Size: {ma2Cache.Count}/{MaxCacheSize}, Hit Rate: {ma2Cache.HitRate:P2}, Evictions: {ma2Cache.Evictions}", LoggingLevel.Info);
            LogMessage($"[CACHE_STATS] MA3 Cache - Size: {ma3Cache.Count}/{MaxCacheSize}, Hit Rate: {ma3Cache.HitRate:P2}, Evictions: {ma3Cache.Evictions}", LoggingLevel.Info);
            LogMessage($"[CACHE_STATS] Spread Cache - Size: {ma2Ma3SpreadCache.Count}/{MaxCacheSize}, Hit Rate: {ma2Ma3SpreadCache.HitRate:P2}, Evictions: {ma2Ma3SpreadCache.Evictions}", LoggingLevel.Info);
            LogMessage($"[CACHE_STATS] Momentum Cache - Size: {momentumThresholdCache.Count}/{MaxCacheSize}, Hit Rate: {momentumThresholdCache.HitRate:P2}, Evictions: {momentumThresholdCache.Evictions}", LoggingLevel.Info);
        }

        // Cache warming for frequently accessed values
        private void WarmCaches()
        {
            if (analysisBars == null || analysisBars.ClosePrices == null || analysisBars.ClosePrices.Count == 0)
                return;

            int currentIndex = analysisBars.ClosePrices.Count - 1;
            int warmCount = Math.Min(MaxCacheSize / 4, currentIndex + 1); // Warm with 25% of max cache size

            // Warm ATR cache with recent values
            for (int i = Math.Max(0, currentIndex - warmCount); i <= currentIndex; i++)
            {
                GetCachedAtrValue(i);
            }

            // Warm MA caches with recent values
            for (int i = Math.Max(0, currentIndex - warmCount); i <= currentIndex; i++)
            {
                GetCachedMa1Value(i);
                GetCachedMa2Value(i);
                GetCachedMa3Value(i);
            }

            // Warm momentum threshold cache
            for (int i = Math.Max(0, currentIndex - warmCount); i <= currentIndex; i++)
            {
                GetCachedMomentumThreshold(i);
            }

            LogMessage($"[CACHE] Warmed caches with {warmCount} recent values", LoggingLevel.OnlyImportant);
        }

        // Cache performance testing
        private void TestCachePerformance()
        {
            if (LogLevel == LoggingLevel.Off || analysisBars == null || analysisBars.ClosePrices == null)
                return;

            try
            {
                LogMessage("[CACHE_TEST] === Cache Performance Test ===", LoggingLevel.Debug);

                int testIndex = analysisBars.ClosePrices.Count - 1;
                if (testIndex < 0) return;

                // Test ATR cache performance
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    int randomIndex = Math.Max(0, testIndex - (i * 10));
                    GetCachedAtrValue(randomIndex);
                }
                stopwatch.Stop();
                LogMessage($"[CACHE_TEST] ATR Cache (100 lookups): {stopwatch.Elapsed.TotalMilliseconds:F2}ms, Hit Rate: {atrCache.HitRate:P2}", LoggingLevel.Debug);

                // Test MA cache performance
                stopwatch.Restart();
                for (int i = 0; i < 100; i++)
                {
                    int randomIndex = Math.Max(0, testIndex - (i * 10));
                    GetCachedMa1Value(randomIndex);
                    GetCachedMa2Value(randomIndex);
                    GetCachedMa3Value(randomIndex);
                }
                stopwatch.Stop();
                LogMessage($"[CACHE_TEST] MA Caches (300 lookups): {stopwatch.Elapsed.TotalMilliseconds:F2}ms", LoggingLevel.Debug);
                LogMessage($"[CACHE_TEST] MA1 Hit Rate: {ma1Cache.HitRate:P2}, MA2 Hit Rate: {ma2Cache.HitRate:P2}, MA3 Hit Rate: {ma3Cache.HitRate:P2}", LoggingLevel.Debug);

                // Test cache size limits
                LogMessage($"[CACHE_TEST] Cache Sizes - ATR: {atrCache.Count}/{MaxCacheSize}, MA1: {ma1Cache.Count}/{MaxCacheSize}", LoggingLevel.Debug);
                LogMessage($"[CACHE_TEST] Total Evictions - ATR: {atrCache.Evictions}, MA1: {ma1Cache.Evictions}", LoggingLevel.Debug);

                LogMessage("[CACHE_TEST] ===================================", LoggingLevel.Debug);
            }
            catch (Exception ex)
            {
                LogMessage($"[CACHE_TEST] Error in cache performance test: {ex.Message}", LoggingLevel.Error);
            }
        }

        // Condition cache performance testing
        private void TestConditionCachePerformance()
        {
            if (LogLevel == LoggingLevel.Off) return;

            try
            {
                LogMessage("[CONDITION_CACHE_TEST] === Condition Cache Performance Test ===", LoggingLevel.Debug);
                LogMessage($"[CONDITION_CACHE_TEST] Hits: {conditionCacheHits}, Misses: {conditionCacheMisses}, Hit Rate: {((conditionCacheHits + conditionCacheMisses) > 0 ? (double)conditionCacheHits / (conditionCacheHits + conditionCacheMisses) : 0):P2}", LoggingLevel.Debug);
                LogMessage($"[CONDITION_CACHE_TEST] Cache Size: {conditionCache.Count}", LoggingLevel.Debug);
                LogMessage($"[CONDITION_CACHE_TEST] Cache Efficiency: {(conditionCache.Count > 0 ? (double)conditionCacheHits / conditionCache.Count : 0):P2} hits per cached condition", LoggingLevel.Debug);

                // Test condition cache performance with sample lookups
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                int testIterations = 50;
                long testHits = 0;
                long testMisses = 0;

                for (int i = 0; i < testIterations; i++)
                {
                    string testKey = $"test_condition_{i % 10}";
                    if (conditionCache.ContainsKey(testKey))
                    {
                        testHits++;
                    }
                    else
                    {
                        testMisses++;
                    }
                }

                stopwatch.Stop();
                LogMessage($"[CONDITION_CACHE_TEST] Performance Test ({testIterations} lookups): {stopwatch.Elapsed.TotalMilliseconds:F2}ms", LoggingLevel.Debug);
                LogMessage($"[CONDITION_CACHE_TEST] Test Hit Rate: {((testHits + testMisses) > 0 ? (double)testHits / (testHits + testMisses) : 0):P2}", LoggingLevel.Debug);

                LogMessage("[CONDITION_CACHE_TEST] ======================================", LoggingLevel.Debug);
            }
            catch (Exception ex)
            {
                LogMessage($"[CONDITION_CACHE_TEST] Error in condition cache performance test: {ex.Message}", LoggingLevel.Error);
            }
        }

        private double GetCachedAtrValue(int index)
        {
            if (atrCache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            double atrValue = GetAtrValueMain(index);
            atrCache.Set(index, atrValue);
            return atrValue;
        }

        private double GetCachedMa1Value(int index)
        {
            if (ma1Cache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            if (ma1 != null && index >= 0 && index < ma1.Count)
            {
                cachedValue = ma1[index];
                ma1Cache.Set(index, cachedValue);
                return cachedValue;
            }

            return double.NaN;
        }

        private double GetCachedMa2Value(int index)
        {
            if (ma2Cache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            if (ma2 != null && index >= 0 && index < ma2.Count)
            {
                cachedValue = ma2[index];
                ma2Cache.Set(index, cachedValue);
                return cachedValue;
            }

            return double.NaN;
        }

        private double GetCachedMa3Value(int index)
        {
            if (ma3Cache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            if (ma3 != null && index >= 0 && index < ma3.Count)
            {
                cachedValue = ma3[index];
                ma3Cache.Set(index, cachedValue);
                return cachedValue;
            }

            return double.NaN;
        }

        private double GetCachedMa2Ma3Spread(int index)
        {
            if (ma2Ma3SpreadCache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            cachedValue = CalculateMa2Ma3Spread(index);
            ma2Ma3SpreadCache.Set(index, cachedValue);
            return cachedValue;
        }

        private double GetCachedMomentumThreshold(int index)
        {
            if (momentumThresholdCache.TryGetValue(index, out double cachedValue))
            {
                return cachedValue;
            }

            cachedValue = GetMomentumThreshold(index);
            momentumThresholdCache.Set(index, cachedValue);
            return cachedValue;
        }

        private double GetCachedSharpeRatio()
        {
            int cacheKey = tradeReturns.Count; // Use count as cache key since Sharpe depends on all returns
            if (sharpeRatioCache.TryGetValue(cacheKey, out double cachedValue))
            {
                return cachedValue;
            }

            cachedValue = CalculateSharpeRatio();
            sharpeRatioCache.Set(cacheKey, cachedValue);
            return cachedValue;
        }

        private double GetCachedCorrelation(string param1, string param2, int index)
        {
            string cacheKey = $"{param1}_{param2}";
            if (correlationCache.TryGetValue(cacheKey, out var subCache))
            {
                if (subCache.TryGetValue(index, out double cachedValue))
                {
                    return cachedValue;
                }
            }
            else
            {
                subCache = new LRUCache<int, double>(MaxCacheSize / 10);
                correlationCache.Set(cacheKey, subCache);
            }

            // For correlation caching, we'll use a simplified approach
            // In practice, you might want to cache the entire correlation matrix
            double correlationValue = 0; // Placeholder - actual correlation calculation would be complex
            subCache.Set(index, correlationValue);
            return correlationValue;
        }

        // Chart batching system methods
        private void AddToChartBatch(string objectName, ChartObjectType objectType, DateTime time, double price,
                                   Color color, string text = "", ChartIconType iconType = ChartIconType.Circle,
                                   double price2 = 0, MarkerPriority priority = MarkerPriority.Normal, DateTime? time2 = null)
        {
            if (!EnableChartBatching)
            {
                // If batching is disabled, draw immediately
                DrawChartObjectImmediate(objectName, objectType, time, price, color, text, iconType, price2, time2);
                return;
            }

            var batchObject = new ChartObjectBatch
            {
                ObjectName = objectName,
                ObjectType = objectType,
                Time = time,
                Time2 = time2 ?? time, // Default to same time if not provided
                Price = price,
                Color = color,
                Text = text,
                IconType = iconType,
                Price2 = price2,
                Priority = priority
            };

            chartObjectBatch.Add(batchObject);
            currentBatchSize++;

            // Process batch if it reaches the batch size limit
            if (currentBatchSize >= ChartUpdateBatchSize)
            {
                ProcessChartBatch();
            }
        }

        private void DrawChartObjectImmediate(string objectName, ChartObjectType objectType, DateTime time, double price,
                                            Color color, string text, ChartIconType iconType, double price2, DateTime? time2 = null)
        {
            try
            {
                switch (objectType)
                {
                    case ChartObjectType.TrendLine:
                        DateTime endTime = time2 ?? time; // Use time2 if provided, otherwise use same time
                        Chart.DrawTrendLine(objectName, time, price, endTime, price2, color, 2, LineStyle.Solid);
                        break;
                    case ChartObjectType.Text:
                        Chart.DrawText(objectName, text, time, price, color);
                        break;
                    case ChartObjectType.Icon:
                        Chart.DrawIcon(objectName, iconType, time, price, color);
                        break;
                    case ChartObjectType.Rectangle:
                        // For rectangles, we might need additional parameters
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error drawing chart object {objectName}: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void ProcessChartBatch()
        {
            if (chartObjectBatch.Count == 0)
                return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Sort batch by priority (high priority first)
                var sortedBatch = chartObjectBatch.OrderByDescending(obj => obj.Priority).ToList();

                foreach (var batchObject in sortedBatch)
                {
                    DrawChartObjectImmediate(batchObject.ObjectName, batchObject.ObjectType,
                                           batchObject.Time, batchObject.Price, batchObject.Color,
                                           batchObject.Text, batchObject.IconType, batchObject.Price2, batchObject.Time2);
                }

                chartUpdateCount++;
                chartUpdateTime += stopwatch.ElapsedTicks;

                LogMessage($"[CHART_BATCH] Processed {chartObjectBatch.Count} objects in batch", LoggingLevel.OnlyImportant);
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing chart batch: {ex.Message}", LoggingLevel.Error);
            }
            finally
            {
                // Clear the batch
                chartObjectBatch.Clear();
                currentBatchSize = 0;
                stopwatch.Stop();
            }
        }

        private void ForceProcessChartBatch()
        {
            if (chartObjectBatch.Count > 0)
            {
                ProcessChartBatch();
            }
        }

        private double GetAverageChartUpdateTimeMs()
        {
            return chartUpdateCount > 0 ? (chartUpdateTime * 1000.0) / (System.Diagnostics.Stopwatch.Frequency * chartUpdateCount) : 0;
        }

        // Helper variable for bar counting
#pragma warning disable CS0414
        private int barCounter = 0;
#pragma warning restore CS0414
   }
}
