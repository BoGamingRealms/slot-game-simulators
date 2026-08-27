using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CashVortexGame;
using CashVortexGame.Config;
using SlotFramework.Models;
using SlotFramework.Utilities;

namespace CashVortexSimulator;

public class CashVortexSimWorkerStats
{
    public long TotalWin { get; set; }
    public long TotalLineWin { get; set; }
    public int WinSpins { get; set; }
    public int TotalSlingoLinesCompleted { get; set; }

    public int JackpotCoinHits { get; set; }
    public int MiniVortexHits { get; set; }
    public int MiniVortexZeroHits { get; set; }
    public int MegaVortexHits { get; set; }
    public int MegaVortexZeroHits { get; set; }
    public int UltraVortexHits { get; set; }
    public int UltraVortexZeroHits { get; set; }
    public int MiniStrikeHits { get; set; }
    public int MiniStrikeZeroHits { get; set; }
    public int MegaStrikeHits { get; set; }
    public int MegaStrikeZeroHits { get; set; }
    public int UltraStrikeHits { get; set; }
    public int UltraStrikeZeroHits { get; set; }
    public int XWheelHits { get; set; }

    // X-Wheel Feature stats (Top of reels)
    public long XWheelTotalWin { get; set; }
    public int[] WheelReachHits { get; set; } = new int[4];
    public long[] WheelRtpWin { get; set; } = new long[4];
    public Dictionary<string, int> WheelPrizeHits { get; set; } = new();
    public Dictionary<string, long> WheelPrizeWins { get; set; } = new();

    // Center Wild Wheel Bonus stats
    public int CenterWheelTriggers { get; set; }
    public long CenterWheelTotalWin { get; set; }
    public Dictionary<string, int> CenterWheelPrizeHits { get; set; } = new();
    public Dictionary<string, long> CenterWheelPrizeWins { get; set; } = new();

    public Dictionary<string, int> JackpotHits { get; set; } = new();
    public Dictionary<string, long> JackpotWins { get; set; } = new();

    // Lock & Slingo Bonus stats
    public int LockAndSlingoTriggers { get; set; }
    public long LockAndSlingoTotalWin { get; set; }
    public long LockAndSlingoTotalSpinsPlayed { get; set; }
    public long LockAndSlingoTotalSlingos { get; set; }
    public int LockAndSlingoFullHouses { get; set; }
    public int[] LockAndSlingoLadderHits { get; set; } = new int[13];
    public long[] LockAndSlingoLadderWins { get; set; } = new long[13];
    public long[] LockAndSlingoLadderBoardWins { get; set; } = new long[13];
    public long[] LockAndSlingoLadderPrizeWins { get; set; } = new long[13];

    public CashVortexSimWorkerStats(CashVortexConfig config)
    {
        foreach (var jp in config.JackpotCoins)
        {
            JackpotHits[jp.JackpotName] = 0;
            JackpotWins[jp.JackpotName] = 0;
        }
    }

    public void Record(SpinResult result, CashVortexSlotEngine engine)
    {
        TotalWin += result.TotalWin;

        if (result.TotalWin > 0)
        {
            WinSpins++;
        }

        foreach (var lw in result.LineWins)
        {
            TotalLineWin += lw.Payout;
            TotalSlingoLinesCompleted++;
        }

        foreach (var pot in result.TriggeredPotBonuses)
        {
            if (pot.BonusName.StartsWith("CenterWheel:"))
            {
                CenterWheelTriggers++;
                CenterWheelTotalWin += pot.Win;
                string prizeName = pot.BonusName.Substring("CenterWheel:".Length);
                CenterWheelPrizeHits[prizeName] = CenterWheelPrizeHits.GetValueOrDefault(prizeName) + 1;
                CenterWheelPrizeWins[prizeName] = CenterWheelPrizeWins.GetValueOrDefault(prizeName) + pot.Win;
            }
            else if (pot.BonusName.StartsWith("XWheel:"))
            {
                XWheelTotalWin += pot.Win;
                var parts = pot.BonusName.Split(':');
                if (parts.Length >= 3)
                {
                    string wStr = parts[1]; // e.g. "W1", "W2", "W3"
                    string prizeName = parts[2];
                    if (wStr.Length >= 2 && int.TryParse(wStr.Substring(1), out int wLevel) && wLevel >= 1 && wLevel <= 3)
                    {
                        WheelReachHits[wLevel]++;
                        WheelRtpWin[wLevel] += pot.Win;

                        string prizeKey = $"{wStr}:{prizeName}";
                        WheelPrizeHits[prizeKey] = WheelPrizeHits.GetValueOrDefault(prizeKey) + 1;
                        WheelPrizeWins[prizeKey] = WheelPrizeWins.GetValueOrDefault(prizeKey) + pot.Win;
                    }
                }
            }
            else if (pot.BonusName.Equals("Lock & Slingo", StringComparison.OrdinalIgnoreCase))
            {
                LockAndSlingoTriggers++;
                LockAndSlingoTotalWin += pot.Win;
                LockAndSlingoTotalSpinsPlayed += pot.SpinsPlayed;
                LockAndSlingoTotalSlingos += pot.CompletedSlingos;

                int slingoIdx = Math.Clamp(pot.CompletedSlingos, 0, 12);
                LockAndSlingoLadderHits[slingoIdx]++;
                LockAndSlingoLadderWins[slingoIdx] += pot.Win;
                LockAndSlingoLadderBoardWins[slingoIdx] += pot.BaseBoardWinCents;
                LockAndSlingoLadderPrizeWins[slingoIdx] += pot.LadderPrizeWinCents;

                if (slingoIdx == 12)
                {
                    LockAndSlingoFullHouses++;
                }
            }
        }

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                var cell = engine.Grid[r, c];
                if (cell.JustLanded)
                {
                    switch (cell.Type)
                    {
                        case SymbolType.JackpotCoin:
                            JackpotCoinHits++;
                            if (cell.JackpotType != null)
                            {
                                JackpotHits[cell.JackpotType] = JackpotHits.GetValueOrDefault(cell.JackpotType) + 1;
                                long jpWin = (long)Math.Round(cell.CashValue * 100);
                                JackpotWins[cell.JackpotType] = JackpotWins.GetValueOrDefault(cell.JackpotType) + jpWin;
                            }
                            break;
                        case SymbolType.MiniVortex:
                            MiniVortexHits++;
                            if (cell.TargetAffectedCount == 0 || cell.CashValue == 0.0) MiniVortexZeroHits++;
                            break;
                        case SymbolType.MegaVortex:
                            MegaVortexHits++;
                            if (cell.TargetAffectedCount == 0 || cell.CashValue == 0.0) MegaVortexZeroHits++;
                            break;
                        case SymbolType.UltraVortex:
                            UltraVortexHits++;
                            if (cell.TargetAffectedCount == 0 || cell.CashValue == 0.0) UltraVortexZeroHits++;
                            break;
                        case SymbolType.MiniStrike:
                            MiniStrikeHits++;
                            if (cell.TargetAffectedCount == 0) MiniStrikeZeroHits++;
                            break;
                        case SymbolType.MegaStrike:
                            MegaStrikeHits++;
                            if (cell.TargetAffectedCount == 0) MegaStrikeZeroHits++;
                            break;
                        case SymbolType.UltraStrike:
                            UltraStrikeHits++;
                            if (cell.TargetAffectedCount == 0) UltraStrikeZeroHits++;
                            break;
                        case SymbolType.XWheel: XWheelHits++; break;
                    }
                }
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("                CASH VORTEX - TRIPLE POWER SIMULATOR                                     ");
        Console.WriteLine("=========================================================================================");

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string downloadsFolder = Path.Combine(userProfile, "Downloads");
        string localDefault = Path.Combine(downloadsFolder, "CashVortexTriplePower95.xlsx");
        if (!File.Exists(localDefault))
        {
            localDefault = "CashVortexTriplePower95.xlsx";
        }

        string configSource = File.Exists(localDefault) ? localDefault : CashVortexExcelLoader.DefaultGoogleSheetUrl;
        string resultsPath = Directory.Exists(downloadsFolder)
            ? Path.Combine(downloadsFolder, "CashVortexTriplePower95_Results.xlsx")
            : "CashVortexTriplePower95_Results.xlsx";

        bool trackFullStats = true;
        bool useBalanced = false;
        bool evalPrizes = false;
        bool showDataTab = false;
        int totalSpinsArg = 1_000_000;
        string? explicitTargetConfig = null;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals("--basic", StringComparison.OrdinalIgnoreCase))
            {
                trackFullStats = false;
            }
            else if (arg.Equals("--full", StringComparison.OrdinalIgnoreCase))
            {
                trackFullStats = true;
            }
            else if (arg.Equals("--balanced", StringComparison.OrdinalIgnoreCase) || arg.Equals("--955", StringComparison.OrdinalIgnoreCase))
            {
                useBalanced = true;
            }
            else if (arg.Equals("--prizes", StringComparison.OrdinalIgnoreCase) || arg.Equals("--eval-prizes", StringComparison.OrdinalIgnoreCase))
            {
                evalPrizes = true;
            }
            else if (arg.Equals("--data", StringComparison.OrdinalIgnoreCase) || arg.Equals("--show-data", StringComparison.OrdinalIgnoreCase) || arg.Equals("--export-data", StringComparison.OrdinalIgnoreCase))
            {
                showDataTab = true;
            }
            else if ((arg.Equals("--spins", StringComparison.OrdinalIgnoreCase) || arg.Equals("-s", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int s)) totalSpinsArg = s;
            }
            else if ((arg.Equals("--target", StringComparison.OrdinalIgnoreCase) || arg.Equals("--excel", StringComparison.OrdinalIgnoreCase) || arg.Equals("--config", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                explicitTargetConfig = args[++i];
            }
            else if (arg.Equals("--url", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                configSource = args[++i];
            }
            else if (!arg.StartsWith("-"))
            {
                configSource = arg;
            }
        }

        try
        {
            CashVortexConfig config;
            if (useBalanced)
            {
                Console.WriteLine("Loading Balanced 95.5% RTP mathematical configuration profile...");
                config = CashVortexConfig.CreateBalanced955();
            }
            else if (SlotFramework.Utilities.GoogleSheetDownloader.IsOnlineSource(configSource))
            {
                Console.WriteLine($"Loading configuration online from Google Sheet: {configSource}...");
                config = CashVortexExcelLoader.Load(configSource);
            }
            else
            {
                Console.WriteLine($"Loading configuration from local file: {configSource}...");
                config = CashVortexExcelLoader.Load(configSource);
            }

            Console.WriteLine("\nLoaded Configuration Summary:");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"Table Selections Count: {config.TableSelections.Count}");
            Console.WriteLine($"Special Symbol Types: {config.SpecialSymbolDefs.Count}");
            Console.WriteLine($"Jackpot Types: {config.JackpotCoins.Count}");
            Console.WriteLine($"Cash Strike Values Count: {config.CashStrikeValues.Count}");
            Console.WriteLine($"Cash Coin Values Count: {config.CashCoinValues.Count}");
            Console.WriteLine($"Cash Vortex Base Pays: {config.CashVortexBasePays.Count} (Mini: {config.MiniVortexBasePay}x, Mega: {config.MegaVortexBasePay}x, Ultra: {config.UltraVortexBasePay}x)");
            foreach (var vp in config.CashVortexBasePays)
            {
                Console.WriteLine($"   * {vp.VortexName}: BasePay = {vp.BaseMultiplier}x");
            }
            Console.WriteLine($"Center Wheel Prizes: {config.CenterWheelPrizes.Count}");
            Console.WriteLine($"X-Wheel Mini Wheel Prizes: {config.MiniWheelPrizes.Count}");
            Console.WriteLine($"X-Wheel Mega Wheel Prizes: {config.MegaWheelPrizes.Count}");
            Console.WriteLine($"X-Wheel Ultra Wheel Prizes: {config.UltraWheelPrizes.Count}");
            Console.WriteLine($"Slingo Ladder Prizes: {config.SlingoLadderPrizes.Count}");
            foreach (var lp in config.SlingoLadderPrizes.OrderBy(x => x.SlingoCount))
            {
                Console.WriteLine($"   * Slingo {lp.SlingoCount,2}: PrizeString = \"{lp.PrizeString}\" | Type = {lp.Type} | ParameterValue = {lp.ParameterValue} | JackpotType = {lp.JackpotType ?? "None"}");
            }
            Console.WriteLine($"Bonus Landing Base Factor: {config.BonusBaseFactor}");
            Console.WriteLine($"Bonus Outcome Types: {config.BonusOutcomeDefs.Count}");
            Console.WriteLine("-------------------------------------------------------------------------------------\n");

            if (showDataTab)
            {
                PrintConfigDataTabFormat(config);
                return;
            }

            if (evalPrizes)
            {
                RunPrizeEvaluation(config, totalSpinsArg);
                return;
            }

            int totalSpins = totalSpinsArg;
            int workerCount = Environment.ProcessorCount;
            int spinsPerWorker = totalSpins / workerCount;
            var workers = new CashVortexSimWorkerStats[workerCount];

            Console.WriteLine($"Generating real simulation results ({totalSpins:N0} spins)...\n");

            var sw = Stopwatch.StartNew();

            Parallel.For(0, workerCount, w =>
            {
                int seed = Guid.NewGuid().GetHashCode();
                var localRng = new FastRandom((uint)seed);
                var localEngine = new CashVortexSlotEngine(config);
                int spinsForThisWorker = (w == workerCount - 1) ? (totalSpins - spinsPerWorker * (workerCount - 1)) : spinsPerWorker;
                var localStats = new CashVortexSimWorkerStats(config);

                for (int i = 0; i < spinsForThisWorker; i++)
                {
                    var spinResult = localEngine.Spin(localRng);
                    localStats.Record(spinResult, localEngine);
                }

                workers[w] = localStats;
            });

            sw.Stop();
            Console.WriteLine($"Simulation finished in {sw.ElapsedMilliseconds} ms ({totalSpins / (sw.Elapsed.TotalSeconds):N0} spins/sec across {workerCount} CPU threads)!");

            // Aggregating statistics
            long totalWin = 0;
            long totalLineWin = 0;
            int winSpins = 0;
            int totalSlingoLines = 0;

            int jackpotCoinHits = 0;
            int miniVortexHits = 0;
            int miniVortexZeroHits = 0;
            int megaVortexHits = 0;
            int megaVortexZeroHits = 0;
            int ultraVortexHits = 0;
            int ultraVortexZeroHits = 0;
            int miniStrikeHits = 0;
            int miniStrikeZeroHits = 0;
            int megaStrikeHits = 0;
            int megaStrikeZeroHits = 0;
            int ultraStrikeHits = 0;
            int ultraStrikeZeroHits = 0;
            int xWheelHits = 0;
            long xWheelTotalWin = 0;

            int centerWheelTriggers = 0;
            long centerWheelTotalWin = 0;
            var centerWheelPrizeHits = new Dictionary<string, int>();
            var centerWheelPrizeWins = new Dictionary<string, long>();

            int[] wheelReachHits = new int[4];
            long[] wheelRtpWin = new long[4];
            var wheelPrizeHits = new Dictionary<string, int>();
            var wheelPrizeWins = new Dictionary<string, long>();

            int lockAndSlingoTriggers = 0;
            long lockAndSlingoTotalWin = 0;
            long lockAndSlingoTotalSpinsPlayed = 0;
            long lockAndSlingoTotalSlingos = 0;
            int lockAndSlingoFullHouses = 0;
            int[] lockAndSlingoLadderHits = new int[13];
            long[] lockAndSlingoLadderWins = new long[13];
            long[] lockAndSlingoLadderBoardWins = new long[13];
            long[] lockAndSlingoLadderPrizeWins = new long[13];

            var jackpotHits = new Dictionary<string, int>();
            var jackpotWins = new Dictionary<string, long>();
            foreach (var jp in config.JackpotCoins)
            {
                jackpotHits[jp.JackpotName] = 0;
                jackpotWins[jp.JackpotName] = 0;
            }

            foreach (var w in workers)
            {
                totalWin += w.TotalWin;
                totalLineWin += w.TotalLineWin;
                winSpins += w.WinSpins;
                totalSlingoLines += w.TotalSlingoLinesCompleted;

                jackpotCoinHits += w.JackpotCoinHits;
                miniVortexHits += w.MiniVortexHits;
                miniVortexZeroHits += w.MiniVortexZeroHits;
                megaVortexHits += w.MegaVortexHits;
                megaVortexZeroHits += w.MegaVortexZeroHits;
                ultraVortexHits += w.UltraVortexHits;
                ultraVortexZeroHits += w.UltraVortexZeroHits;
                miniStrikeHits += w.MiniStrikeHits;
                miniStrikeZeroHits += w.MiniStrikeZeroHits;
                megaStrikeHits += w.MegaStrikeHits;
                megaStrikeZeroHits += w.MegaStrikeZeroHits;
                ultraStrikeHits += w.UltraStrikeHits;
                ultraStrikeZeroHits += w.UltraStrikeZeroHits;
                xWheelHits += w.XWheelHits;
                xWheelTotalWin += w.XWheelTotalWin;

                centerWheelTriggers += w.CenterWheelTriggers;
                centerWheelTotalWin += w.CenterWheelTotalWin;
                foreach (var kvp in w.CenterWheelPrizeHits)
                {
                    centerWheelPrizeHits[kvp.Key] = centerWheelPrizeHits.GetValueOrDefault(kvp.Key) + kvp.Value;
                }
                foreach (var kvp in w.CenterWheelPrizeWins)
                {
                    centerWheelPrizeWins[kvp.Key] = centerWheelPrizeWins.GetValueOrDefault(kvp.Key) + kvp.Value;
                }

                lockAndSlingoTriggers += w.LockAndSlingoTriggers;
                lockAndSlingoTotalWin += w.LockAndSlingoTotalWin;
                lockAndSlingoTotalSpinsPlayed += w.LockAndSlingoTotalSpinsPlayed;
                lockAndSlingoTotalSlingos += w.LockAndSlingoTotalSlingos;
                lockAndSlingoFullHouses += w.LockAndSlingoFullHouses;
                for (int s = 0; s <= 12; s++)
                {
                    lockAndSlingoLadderHits[s] += w.LockAndSlingoLadderHits[s];
                    lockAndSlingoLadderWins[s] += w.LockAndSlingoLadderWins[s];
                    lockAndSlingoLadderBoardWins[s] += w.LockAndSlingoLadderBoardWins[s];
                    lockAndSlingoLadderPrizeWins[s] += w.LockAndSlingoLadderPrizeWins[s];
                }

                for (int l = 1; l <= 3; l++)
                {
                    wheelReachHits[l] += w.WheelReachHits[l];
                    wheelRtpWin[l] += w.WheelRtpWin[l];
                }

                foreach (var kvp in w.WheelPrizeHits)
                {
                    wheelPrizeHits[kvp.Key] = wheelPrizeHits.GetValueOrDefault(kvp.Key) + kvp.Value;
                }
                foreach (var kvp in w.WheelPrizeWins)
                {
                    wheelPrizeWins[kvp.Key] = wheelPrizeWins.GetValueOrDefault(kvp.Key) + kvp.Value;
                }

                foreach (var kvp in w.JackpotHits)
                {
                    jackpotHits[kvp.Key] = jackpotHits.GetValueOrDefault(kvp.Key) + kvp.Value;
                }
                foreach (var kvp in w.JackpotWins)
                {
                    jackpotWins[kvp.Key] = jackpotWins.GetValueOrDefault(kvp.Key) + kvp.Value;
                }
            }

            double totalRtp = (double)totalWin / (totalSpins * 100.0);
            double lineWinRtp = (double)totalLineWin / (totalSpins * 100.0);
            double centerWheelRtp = (double)centerWheelTotalWin / (totalSpins * 100.0);
            double xWheelRtp = (double)xWheelTotalWin / (totalSpins * 100.0);
            double lockAndSlingoRtp = (double)lockAndSlingoTotalWin / (totalSpins * 100.0);
            double hitFreq = (double)winSpins / totalSpins;

            Console.WriteLine($"\nSimulation complete!");
            Console.WriteLine($"  - Total RTP: {totalRtp:P2}");
            Console.WriteLine($"    - Line Payout RTP: {lineWinRtp:P2}");
            Console.WriteLine($"    - Center Wild Wheel Bonus RTP: {centerWheelRtp:P2}");
            Console.WriteLine($"    - X-Wheel Direct RTP: {xWheelRtp:P2}");
            Console.WriteLine($"    - Lock & Slingo™ Bonus RTP: {lockAndSlingoRtp:P2}");
            Console.WriteLine($"  - Hit Frequency: {hitFreq:P2}");
            Console.WriteLine($"  - Total Slingo Lines Completed: {totalSlingoLines:N0} (1 in {((double)totalSpins / Math.Max(1, totalSlingoLines)):F2} spins)");

            int totalStrikeHits = miniStrikeHits + megaStrikeHits + ultraStrikeHits;
            int totalStrikeZeroHits = miniStrikeZeroHits + megaStrikeZeroHits + ultraStrikeZeroHits;
            int totalVortexHits = miniVortexHits + megaVortexHits + ultraVortexHits;
            int totalVortexZeroHits = miniVortexZeroHits + megaVortexZeroHits + ultraVortexZeroHits;

            Console.WriteLine("\n=========================================================================================");
            Console.WriteLine("          SPECIAL SYMBOL TARGET EFFICIENCY & ZERO-EFFECT ANALYSIS                        ");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine($"1. CASH STRIKES (Landed, but boosted 0 targets in range):");
            Console.WriteLine($"   * Overall Cash Strikes: {totalStrikeZeroHits:N0} / {totalStrikeHits:N0} boosted NOTHING ({((double)totalStrikeZeroHits / Math.Max(1, totalStrikeHits)):P2} of strike landings | {((double)totalStrikeZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, totalStrikeZeroHits)):F1} spins)");
            Console.WriteLine($"     - Mini Strike:  {miniStrikeZeroHits,6:N0} / {miniStrikeHits,6:N0} ({((double)miniStrikeZeroHits / Math.Max(1, miniStrikeHits)),6:P2}) boosted 0 targets | {((double)miniStrikeZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, miniStrikeZeroHits)):F1} spins");
            Console.WriteLine($"     - Mega Strike:  {megaStrikeZeroHits,6:N0} / {megaStrikeHits,6:N0} ({((double)megaStrikeZeroHits / Math.Max(1, megaStrikeHits)),6:P2}) boosted 0 targets | {((double)megaStrikeZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, megaStrikeZeroHits)):F1} spins");
            Console.WriteLine($"     - Ultra Strike: {ultraStrikeZeroHits,6:N0} / {ultraStrikeHits,6:N0} ({((double)ultraStrikeZeroHits / Math.Max(1, ultraStrikeHits)),6:P2}) boosted 0 targets | {((double)ultraStrikeZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, ultraStrikeZeroHits)):F1} spins");

            Console.WriteLine($"\n2. CASH VORTEXES (Landed, but collected 0 targets / 0 cash value in range):");
            Console.WriteLine($"   * Overall Cash Vortexes: {totalVortexZeroHits:N0} / {totalVortexHits:N0} collected NOTHING ({((double)totalVortexZeroHits / Math.Max(1, totalVortexHits)):P2} of vortex landings | {((double)totalVortexZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, totalVortexZeroHits)):F1} spins)");
            Console.WriteLine($"     - Mini Vortex:  {miniVortexZeroHits,6:N0} / {miniVortexHits,6:N0} ({((double)miniVortexZeroHits / Math.Max(1, miniVortexHits)),6:P2}) collected 0 targets | {((double)miniVortexZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, miniVortexZeroHits)):F1} spins");
            Console.WriteLine($"     - Mega Vortex:  {megaVortexZeroHits,6:N0} / {megaVortexHits,6:N0} ({((double)megaVortexZeroHits / Math.Max(1, megaVortexHits)),6:P2}) collected 0 targets | {((double)megaVortexZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, megaVortexZeroHits)):F1} spins");
            Console.WriteLine($"     - Ultra Vortex: {ultraVortexZeroHits,6:N0} / {ultraVortexHits,6:N0} ({((double)ultraVortexZeroHits / Math.Max(1, ultraVortexHits)),6:P2}) collected 0 targets | {((double)ultraVortexZeroHits / totalSpins):P4} of spins | 1 in {((double)totalSpins / Math.Max(1, ultraVortexZeroHits)):F1} spins");
            Console.WriteLine("=========================================================================================");

            Console.WriteLine("\n[Center Wild Wheel Bonus Breakdown]");
            Console.WriteLine($"  - Total Triggers: {centerWheelTriggers:N0} (1 in {((double)totalSpins / Math.Max(1, centerWheelTriggers)):F2} spins | {((double)centerWheelTriggers / totalSpins):P2})");
            Console.WriteLine($"  - Direct Bonus RTP: {centerWheelRtp:P2}");
            if (trackFullStats)
            {
                foreach (var prize in config.CenterWheelPrizes)
                {
                    int hits = centerWheelPrizeHits.GetValueOrDefault(prize.PrizeString);
                    long win = centerWheelPrizeWins.GetValueOrDefault(prize.PrizeString);
                    double wheelChance = centerWheelTriggers > 0 ? (double)hits / centerWheelTriggers : 0;
                    double spinChance = (double)hits / totalSpins;
                    double prizeRtp = (double)win / (totalSpins * 100.0);
                    Console.WriteLine($"    * {prize.PrizeString,-15}: Hits = {hits,6:N0} | Wheel Chance = {wheelChance,7:P2} | Total Chance = {spinChance,7:P4} | RTP = {prizeRtp,7:P4}");
                }
            }

            Console.WriteLine("\n[Lock & Slingo™ Bonus Breakdown]");
            double lnsTrigChance = (double)lockAndSlingoTriggers / totalSpins;
            double avgLnsWin = lockAndSlingoTriggers > 0 ? (double)lockAndSlingoTotalWin / (lockAndSlingoTriggers * 100.0) : 0;
            double avgLnsSpins = lockAndSlingoTriggers > 0 ? (double)lockAndSlingoTotalSpinsPlayed / lockAndSlingoTriggers : 0;
            double avgLnsSlingos = lockAndSlingoTriggers > 0 ? (double)lockAndSlingoTotalSlingos / lockAndSlingoTriggers : 0;

            Console.WriteLine($"  - Total Triggers: {lockAndSlingoTriggers:N0} (1 in {((double)totalSpins / Math.Max(1, lockAndSlingoTriggers)):F2} spins | {lnsTrigChance:P4})");
            Console.WriteLine($"  - Total Bonus RTP: {lockAndSlingoRtp:P2}");
            Console.WriteLine($"  - Average Bonus Win: {avgLnsWin:F2}x bet");
            Console.WriteLine($"  - Average Spins per Bonus: {avgLnsSpins:F2}");
            Console.WriteLine($"  - Average Slingos per Bonus: {avgLnsSlingos:F2}");
            Console.WriteLine($"  - Full House Hits (25 cells): {lockAndSlingoFullHouses:N0} ({((double)lockAndSlingoFullHouses / Math.Max(1, lockAndSlingoTriggers)):P2} of bonuses)");

            Console.WriteLine("\n[Special Symbol Hits]");
            Console.WriteLine($"  - Jackpot Coins: {jackpotCoinHits:N0}");
            Console.WriteLine($"  - Mini Vortexes: {miniVortexHits:N0}");
            Console.WriteLine($"  - Mega Vortexes: {megaVortexHits:N0}");
            Console.WriteLine($"  - Ultra Vortexes: {ultraVortexHits:N0}");
            Console.WriteLine($"  - Mini Strikes: {miniStrikeHits:N0}");
            Console.WriteLine($"  - Mega Strikes: {megaStrikeHits:N0}");
            Console.WriteLine($"  - Ultra Strikes: {ultraStrikeHits:N0}");
            Console.WriteLine($"  - X Wheel Triggers: {xWheelHits:N0} (Hit Chance: {((double)xWheelHits / totalSpins):P2})");

            Console.WriteLine("\n[X-Wheel Feature Breakdown]");
            for (int wL = 1; wL <= 3; wL++)
            {
                string wName = wL == 1 ? "Mini Wheel (Wheel 1)" : (wL == 2 ? "Mega Wheel (Wheel 2)" : "Ultra Wheel (Wheel 3)");
                int reach = wheelReachHits[wL];
                double reachSpinsChance = (double)reach / totalSpins;
                double reachTrigChance = xWheelHits > 0 ? (double)reach / xWheelHits : 0;
                double wRtp = (double)wheelRtpWin[wL] / (totalSpins * 100.0);
                Console.WriteLine($"  - {wName}: Reached = {reach:N0} times ({reachSpinsChance:P2} of spins | {reachTrigChance:P2} of triggers) | Direct RTP = {wRtp:P4}");
            }

            if (trackFullStats)
            {
                Console.WriteLine("\n[X-Wheel Detailed Prize Stats]");
                for (int wL = 1; wL <= 3; wL++)
                {
                    string wTag = $"W{wL}";
                    string wTitle = wL == 1 ? "Wheel 1 (Mini)" : (wL == 2 ? "Wheel 2 (Mega)" : "Wheel 3 (Ultra)");
                    int wheelTotalSpins = wheelReachHits[wL];
                    Console.WriteLine($"  --- {wTitle} Prizes ---");

                    var prizeList = wL == 1 ? config.MiniWheelPrizes : (wL == 2 ? config.MegaWheelPrizes : config.UltraWheelPrizes);
                    foreach (var prizeDef in prizeList)
                    {
                        string pKey = $"{wTag}:{prizeDef.PrizeString}";
                        int hits = wheelPrizeHits.GetValueOrDefault(pKey);
                        long win = wheelPrizeWins.GetValueOrDefault(pKey);
                        double hitChanceWheel = wheelTotalSpins > 0 ? (double)hits / wheelTotalSpins : 0;
                        double hitChanceSpins = (double)hits / totalSpins;
                        double prizeRtp = (double)win / (totalSpins * 100.0);

                        Console.WriteLine($"    * {prizeDef.PrizeString,-15}: Hits = {hits,6:N0} | Wheel Chance = {hitChanceWheel,7:P2} | Total Chance = {hitChanceSpins,7:P4} | RTP = {prizeRtp,7:P4}");
                    }
                }

                Console.WriteLine("\n[Lock & Slingo Ladder Achievements & Average Payouts]");
                for (int s = 0; s <= 12; s++)
                {
                    if (s == 11) continue;
                    int sHits = lockAndSlingoLadderHits[s];
                    double sChance = lockAndSlingoTriggers > 0 ? (double)sHits / lockAndSlingoTriggers : 0;
                    var ladderDef = config.SlingoLadderPrizes.FirstOrDefault(p => p.SlingoCount == s);
                    string desc = ladderDef?.PrizeString ?? (s == 0 ? "No Lines" : "Standard");
                    if (s == 12) desc = "FULL HOUSE (Ultra 500x)";
                    else if (s == 8) desc = "Mega Jackpot (50x)";
                    else if (s == 4) desc = "Mini Jackpot (5x)";

                    double avgBonusWin = sHits > 0 ? (double)lockAndSlingoLadderWins[s] / (sHits * 100.0) : 0;
                    double avgBoardWin = sHits > 0 ? (double)lockAndSlingoLadderBoardWins[s] / (sHits * 100.0) : 0;
                    double avgPrizeWin = sHits > 0 ? (double)lockAndSlingoLadderPrizeWins[s] / (sHits * 100.0) : 0;

                    Console.WriteLine($"  - Slingo {s,2} Line(s) [{desc,-25}]: Hits = {sHits,6:N0} ({sChance,6:P2}) | Avg Board = {avgBoardWin,5:F2}x | Avg Ladder Prize = {avgPrizeWin,6:F2}x | Avg Total Bonus Win = {avgBonusWin,6:F2}x");
                }
            }

            Console.WriteLine("\n[Jackpot Breakdown]");
            foreach (var jp in config.JackpotCoins)
            {
                int hits = jackpotHits.GetValueOrDefault(jp.JackpotName);
                long win = jackpotWins.GetValueOrDefault(jp.JackpotName);
                double jpRtp = (double)win / (totalSpins * 100.0);
                Console.WriteLine($"  - {jp.JackpotName,-6} Jackpot ({jp.Multiplier}x): Hits = {hits,6:N0} | RTP = {jpRtp,8:P4}");
            }

            int[] strikeTotalLandings = { totalStrikeHits, miniStrikeHits, megaStrikeHits, ultraStrikeHits };
            int[] strikeZeroHits = { totalStrikeZeroHits, miniStrikeZeroHits, megaStrikeZeroHits, ultraStrikeZeroHits };
            int[] vortexTotalLandings = { totalVortexHits, miniVortexHits, megaVortexHits, ultraVortexHits };
            int[] vortexZeroHits = { totalVortexZeroHits, miniVortexZeroHits, megaVortexZeroHits, ultraVortexZeroHits };

            // Write results Excel
            Console.WriteLine($"\nWriting simulation results to: {resultsPath}");
            using var workbook = new XLWorkbook();
            
            // Add primary single consolidated "Stats" worksheet
            var wsStats = workbook.Worksheets.Add("Stats");
            PopulateStatsWorksheet(
                wsStats,
                config,
                totalSpins,
                totalRtp,
                lineWinRtp,
                centerWheelRtp,
                xWheelRtp,
                lockAndSlingoRtp,
                hitFreq,
                totalSlingoLines,
                centerWheelTriggers,
                lockAndSlingoTriggers,
                avgLnsWin,
                avgLnsSpins,
                avgLnsSlingos,
                lockAndSlingoFullHouses,
                jackpotCoinHits,
                miniVortexHits,
                megaVortexHits,
                ultraVortexHits,
                miniStrikeHits,
                megaStrikeHits,
                ultraStrikeHits,
                xWheelHits,
                strikeZeroHits,
                strikeTotalLandings,
                vortexZeroHits,
                vortexTotalLandings,
                wheelReachHits,
                wheelRtpWin,
                centerWheelPrizeHits,
                centerWheelPrizeWins,
                wheelPrizeHits,
                wheelPrizeWins,
                lockAndSlingoLadderHits,
                lockAndSlingoLadderWins,
                lockAndSlingoLadderBoardWins,
                lockAndSlingoLadderPrizeWins,
                jackpotHits,
                jackpotWins);

            // Add detailed worksheets
            var wsCenter = workbook.Worksheets.Add("Center Wheel Details");
            wsCenter.Cell(1, 1).Value = "Prize";
            wsCenter.Cell(1, 2).Value = "Hits";
            wsCenter.Cell(1, 3).Value = "Wheel Hit Chance %";
            wsCenter.Cell(1, 4).Value = "Total Spins Hit Chance %";
            wsCenter.Cell(1, 5).Value = "Direct Win";
            wsCenter.Cell(1, 6).Value = "Direct RTP %";
            wsCenter.Row(1).Style.Font.Bold = true;

            int cRow = 2;
            foreach (var prize in config.CenterWheelPrizes)
            {
                int hits = centerWheelPrizeHits.GetValueOrDefault(prize.PrizeString);
                long win = centerWheelPrizeWins.GetValueOrDefault(prize.PrizeString);
                double wheelChance = centerWheelTriggers > 0 ? (double)hits / centerWheelTriggers : 0;
                double spinChance = (double)hits / totalSpins;
                double prizeRtp = (double)win / (totalSpins * 100.0);

                wsCenter.Cell(cRow, 1).Value = prize.PrizeString;
                wsCenter.Cell(cRow, 2).Value = hits;
                wsCenter.Cell(cRow, 3).Value = $"{wheelChance:P2}";
                wsCenter.Cell(cRow, 4).Value = $"{spinChance:P4}";
                wsCenter.Cell(cRow, 5).Value = win;
                wsCenter.Cell(cRow, 6).Value = $"{prizeRtp:P4}";
                cRow++;
            }
            wsCenter.Columns().AdjustToContents();

            var wsWheel = workbook.Worksheets.Add("X Wheel Details");
            wsWheel.Cell(1, 1).Value = "Wheel";
            wsWheel.Cell(1, 2).Value = "Prize";
            wsWheel.Cell(1, 3).Value = "Hits";
            wsWheel.Cell(1, 4).Value = "Wheel Hit Chance %";
            wsWheel.Cell(1, 5).Value = "Total Spins Hit Chance %";
            wsWheel.Cell(1, 6).Value = "Direct Win";
            wsWheel.Cell(1, 7).Value = "Direct RTP %";
            wsWheel.Row(1).Style.Font.Bold = true;

            int wRow = 2;
            for (int wL = 1; wL <= 3; wL++)
            {
                string wTag = $"W{wL}";
                string wTitle = wL == 1 ? "Wheel 1 (Mini)" : (wL == 2 ? "Wheel 2 (Mega)" : "Wheel 3 (Ultra)");
                int wheelTotalSpins = wheelReachHits[wL];
                var prizeList = wL == 1 ? config.MiniWheelPrizes : (wL == 2 ? config.MegaWheelPrizes : config.UltraWheelPrizes);

                foreach (var prizeDef in prizeList)
                {
                    string pKey = $"{wTag}:{prizeDef.PrizeString}";
                    int hits = wheelPrizeHits.GetValueOrDefault(pKey);
                    long win = wheelPrizeWins.GetValueOrDefault(pKey);
                    double hitChanceWheel = wheelTotalSpins > 0 ? (double)hits / wheelTotalSpins : 0;
                    double hitChanceSpins = (double)hits / totalSpins;
                    double prizeRtp = (double)win / (totalSpins * 100.0);

                    wsWheel.Cell(wRow, 1).Value = wTitle;
                    wsWheel.Cell(wRow, 2).Value = prizeDef.PrizeString;
                    wsWheel.Cell(wRow, 3).Value = hits;
                    wsWheel.Cell(wRow, 4).Value = $"{hitChanceWheel:P2}";
                    wsWheel.Cell(wRow, 5).Value = $"{hitChanceSpins:P4}";
                    wsWheel.Cell(wRow, 6).Value = win;
                    wsWheel.Cell(wRow, 7).Value = $"{prizeRtp:P4}";
                    wRow++;
                }
            }
            wsWheel.Columns().AdjustToContents();

            var wsLns = workbook.Worksheets.Add("Lock & Slingo Details");
            wsLns.Cell(1, 1).Value = "Slingo Level";
            wsLns.Cell(1, 2).Value = "Ladder Award";
            wsLns.Cell(1, 3).Value = "Hits";
            wsLns.Cell(1, 4).Value = "Bonus Hit Chance %";
            wsLns.Cell(1, 5).Value = "Total Spins Chance %";
            wsLns.Cell(1, 6).Value = "Avg Base Board Win (x bet)";
            wsLns.Cell(1, 7).Value = "Avg Ladder Prize Win (x bet)";
            wsLns.Cell(1, 8).Value = "Avg Total Bonus Win (x bet)";
            wsLns.Row(1).Style.Font.Bold = true;

            int lnsRow = 2;
            for (int s = 0; s <= 12; s++)
            {
                if (s == 11) continue;
                int sHits = lockAndSlingoLadderHits[s];
                double sChance = lockAndSlingoTriggers > 0 ? (double)sHits / lockAndSlingoTriggers : 0;
                double sChanceSpins = (double)sHits / totalSpins;

                var ladderDef = config.SlingoLadderPrizes.FirstOrDefault(p => p.SlingoCount == s);
                string desc = ladderDef?.PrizeString ?? (s == 0 ? "No Lines" : "Standard");
                if (s == 12) desc = "FULL HOUSE (Ultra Jackpot 500x)";
                else if (s == 8) desc = "Mega Jackpot (50x)";
                else if (s == 4) desc = "Mini Jackpot (5x)";

                double avgBonusWin = sHits > 0 ? (double)lockAndSlingoLadderWins[s] / (sHits * 100.0) : 0;
                double avgBoardWin = sHits > 0 ? (double)lockAndSlingoLadderBoardWins[s] / (sHits * 100.0) : 0;
                double avgPrizeWin = sHits > 0 ? (double)lockAndSlingoLadderPrizeWins[s] / (sHits * 100.0) : 0;

                wsLns.Cell(lnsRow, 1).Value = $"Slingo {s}";
                wsLns.Cell(lnsRow, 2).Value = desc;
                wsLns.Cell(lnsRow, 3).Value = sHits;
                wsLns.Cell(lnsRow, 4).Value = $"{sChance:P2}";
                wsLns.Cell(lnsRow, 5).Value = $"{sChanceSpins:P4}";
                wsLns.Cell(lnsRow, 6).Value = $"{avgBoardWin:F2}";
                wsLns.Cell(lnsRow, 7).Value = $"{avgPrizeWin:F2}";
                wsLns.Cell(lnsRow, 8).Value = $"{avgBonusWin:F2}";
                lnsRow++;
            }
            wsLns.Columns().AdjustToContents();

            workbook.SaveAs(resultsPath);
            Console.WriteLine("Results successfully written to Excel workbook!");

            // If a local game config file exists or was specified, update its "Stats" tab directly
            string? localConfigToUpdate = explicitTargetConfig;
            if (string.IsNullOrEmpty(localConfigToUpdate))
            {
                if (File.Exists(configSource) && (configSource.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)))
                {
                    localConfigToUpdate = configSource;
                }
                else if (File.Exists(localDefault))
                {
                    localConfigToUpdate = localDefault;
                }
            }

            if (!string.IsNullOrEmpty(localConfigToUpdate) && File.Exists(localConfigToUpdate))
            {
                try
                {
                    Console.WriteLine($"\nUpdating 'Stats' tab in game config file: {localConfigToUpdate}...");
                    SanitizeDrawingIds(localConfigToUpdate);
                    using var configWorkbook = new XLWorkbook(localConfigToUpdate);

                    var configStatsWs = configWorkbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Stats", StringComparison.OrdinalIgnoreCase));
                    if (configStatsWs == null)
                    {
                        configStatsWs = configWorkbook.Worksheets.Add("Stats");
                    }
                    else
                    {
                        configStatsWs.Clear();
                    }

                    PopulateStatsWorksheet(
                        configStatsWs,
                        config,
                        totalSpins,
                        totalRtp,
                        lineWinRtp,
                        centerWheelRtp,
                        xWheelRtp,
                        lockAndSlingoRtp,
                        hitFreq,
                        totalSlingoLines,
                        centerWheelTriggers,
                        lockAndSlingoTriggers,
                        avgLnsWin,
                        avgLnsSpins,
                        avgLnsSlingos,
                        lockAndSlingoFullHouses,
                        jackpotCoinHits,
                        miniVortexHits,
                        megaVortexHits,
                        ultraVortexHits,
                        miniStrikeHits,
                        megaStrikeHits,
                        ultraStrikeHits,
                        xWheelHits,
                        strikeZeroHits,
                        strikeTotalLandings,
                        vortexZeroHits,
                        vortexTotalLandings,
                        wheelReachHits,
                        wheelRtpWin,
                        centerWheelPrizeHits,
                        centerWheelPrizeWins,
                        wheelPrizeHits,
                        wheelPrizeWins,
                        lockAndSlingoLadderHits,
                        lockAndSlingoLadderWins,
                        lockAndSlingoLadderBoardWins,
                        lockAndSlingoLadderPrizeWins,
                        jackpotHits,
                        jackpotWins);

                    configWorkbook.Save();
                    Console.WriteLine($"[SUCCESS] Game config 'Stats' tab successfully updated!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NOTE] Could not update 'Stats' tab in {localConfigToUpdate}: {ex.Message}");
                }
            }

            Console.WriteLine("=========================================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Simulation failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("=========================================================================================");
        }
    }

    public static void PrintConfigDataTabFormat(CashVortexConfig config)
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("            'DATA' TAB CONFIGURATION TABLE (TAB-SEPARATED FOR EXCEL/SHEETS)              ");
        Console.WriteLine("=========================================================================================\n");

        // 1. Table Selections
        Console.WriteLine("Table Selections\tWeight");
        foreach (var ts in config.TableSelections)
        {
            Console.WriteLine($"{ts.Description}\t{ts.Weight}");
        }
        Console.WriteLine();

        // 2. Special Symbols Chance
        Console.WriteLine("Special Symbols Chance\tSpecial Symbol\tNo Special Symbol");
        foreach (var sc in config.SpecialSymbolChances)
        {
            Console.WriteLine($"{sc.Description}\t{sc.SpecialSymbolWeight}\t{sc.NoSpecialSymbolWeight}");
        }
        Console.WriteLine();

        // 3. Special Symbols
        Console.WriteLine("Special Symbols\tWeight");
        foreach (var ss in config.SpecialSymbolDefs)
        {
            Console.WriteLine($"{ss.SymbolName}\t{ss.Weight}");
        }
        Console.WriteLine();

        // 4. Cash Vortex Base Pays
        Console.WriteLine("Cash Vortex\tBase Pay");
        foreach (var vp in config.CashVortexBasePays)
        {
            Console.WriteLine($"{vp.VortexName}\t{vp.BaseMultiplier}");
        }
        Console.WriteLine();

        // 5. Jackpot Coins
        Console.WriteLine("Jackpot Coins\tMultiplier\tWeight");
        foreach (var jp in config.JackpotCoins)
        {
            Console.WriteLine($"{jp.JackpotName}\t{jp.Multiplier}\t{jp.Weight}");
        }
        Console.WriteLine();

        // 6. Cash Strikes
        Console.WriteLine("Cash Strikes\tWeight");
        foreach (var cs in config.CashStrikeValues)
        {
            Console.WriteLine($"{cs.Multiplier}\t{cs.Weight}");
        }
        Console.WriteLine();

        // 7. Cash Coins Chance
        Console.WriteLine("Cash Coins Chance\tCash Coin\tBlank");
        foreach (var cc in config.CashCoinChances)
        {
            Console.WriteLine($"{cc.Description}\t{cc.CoinWeight}\t{cc.BlankWeight}");
        }
        Console.WriteLine();

        // 8. Cash Coins Values
        Console.WriteLine("Cash Coins\tWeight");
        foreach (var cv in config.CashCoinValues)
        {
            Console.WriteLine($"{cv.Multiplier}\t{cv.Weight}");
        }
        Console.WriteLine();

        // 9. Wheel Bonus (Center Wild Wheel)
        Console.WriteLine("Wheel Bonus\tWeight");
        foreach (var cwp in config.CenterWheelPrizes)
        {
            Console.WriteLine($"{cwp.PrizeString}\t{cwp.Weight}");
        }
        Console.WriteLine();

        // 10. Mini Wheel (Wheel 1)
        Console.WriteLine("Mini Wheel\tPrize\tWeight");
        int mwId = 1;
        foreach (var p in config.MiniWheelPrizes)
        {
            Console.WriteLine($"{mwId++}\t{p.PrizeString}\t{p.Weight}");
        }
        Console.WriteLine();

        // 11. Mega Wheel (Wheel 2)
        Console.WriteLine("Mega Wheel\tPrize\tWeight");
        int mgwId = 1;
        foreach (var p in config.MegaWheelPrizes)
        {
            Console.WriteLine($"{mgwId++}\t{p.PrizeString}\t{p.Weight}");
        }
        Console.WriteLine();

        // 12. Ultra Wheel (Wheel 3)
        Console.WriteLine("Ultra Wheel\tPrize\tWeight");
        int uwId = 1;
        foreach (var p in config.UltraWheelPrizes)
        {
            Console.WriteLine($"{uwId++}\t{p.PrizeString}\t{p.Weight}");
        }
        Console.WriteLine();

        // 13. Slingo Ladder
        Console.WriteLine("Slingo Ladder\tPrize");
        foreach (var lp in config.SlingoLadderPrizes.OrderBy(x => x.SlingoCount))
        {
            Console.WriteLine($"{lp.SlingoCount}\t{lp.PrizeString}");
        }
        Console.WriteLine();

        // 14. Symbol Landing Chance (Bonus Lives & Bucket Weights)
        Console.WriteLine("Symbol Landing Chance");
        Console.WriteLine($"Base Factor\t{config.BonusBaseFactor}");
        Console.WriteLine("Spaces Left\t1-5\t6-10\t11-15\t16-20\t21-24");
        Console.WriteLine($"3 Lives\t{config.BonusLandingWeightsByLifeAndBucket[3, 0]}\t{config.BonusLandingWeightsByLifeAndBucket[3, 1]}\t{config.BonusLandingWeightsByLifeAndBucket[3, 2]}\t{config.BonusLandingWeightsByLifeAndBucket[3, 3]}\t{config.BonusLandingWeightsByLifeAndBucket[3, 4]}");
        Console.WriteLine($"2 Lives\t{config.BonusLandingWeightsByLifeAndBucket[2, 0]}\t{config.BonusLandingWeightsByLifeAndBucket[2, 1]}\t{config.BonusLandingWeightsByLifeAndBucket[2, 2]}\t{config.BonusLandingWeightsByLifeAndBucket[2, 3]}\t{config.BonusLandingWeightsByLifeAndBucket[2, 4]}");
        Console.WriteLine($"1 Life\t{config.BonusLandingWeightsByLifeAndBucket[1, 0]}\t{config.BonusLandingWeightsByLifeAndBucket[1, 1]}\t{config.BonusLandingWeightsByLifeAndBucket[1, 2]}\t{config.BonusLandingWeightsByLifeAndBucket[1, 3]}\t{config.BonusLandingWeightsByLifeAndBucket[1, 4]}");
        Console.WriteLine();

        // 15. Symbols Landing Selections (Bonus Outcomes)
        Console.WriteLine("Symbols Landing Selections");
        Console.WriteLine("Spaces Left\t1-5\t6-10\t11-15\t16-20\t21-24");
        foreach (var bo in config.BonusOutcomeDefs)
        {
            Console.WriteLine($"{bo.Description}\t{bo.WeightsBySpaceBucket[0]}\t{bo.WeightsBySpaceBucket[1]}\t{bo.WeightsBySpaceBucket[2]}\t{bo.WeightsBySpaceBucket[3]}\t{bo.WeightsBySpaceBucket[4]}");
        }
        Console.WriteLine();

        // 16. Bonus Cash Strikes
        if (config.BonusCashStrikeTypes.Count > 0)
        {
            Console.WriteLine("Cash Strikes\tWeight");
            foreach (var bcs in config.BonusCashStrikeTypes)
            {
                Console.WriteLine($"{bcs.SymbolName}\t{bcs.Weight}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("=========================================================================================\n");
    }

    private static void RunPrizeEvaluation(CashVortexConfig config, int totalRounds)
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("          LOCK & SLINGO™ - CANDIDATE LADDER PRIZES EVALUATION BENCHMARK                  ");
        Console.WriteLine("=========================================================================================");
        Console.WriteLine($"Simulating {totalRounds:N0} Lock & Slingo™ bonus rounds to measure candidate prize payouts...\n");

        int workerCount = Environment.ProcessorCount;
        int roundsPerWorker = totalRounds / workerCount;

        string[] prizeNames = {
            "Mini Strike 1 (+1.0 ortho)",
            "Mini Vortex (ortho collect)",
            "Mega Strike 2 (+2.0 row & col)",
            "Mega Vortex (row & col collect)",
            "Mini Jackpot (5x fixed)",
            "Ultra Strike 5 (+5.0 full grid)",
            "Mega Jackpot (50x fixed)",
            "Ultra Vortex (full grid collect)",
            "Ultra Jackpot (500x fixed)",
            "Multiplier x2 (2x non-JP coins)",
            "Multiplier x3 (3x non-JP coins)"
        };

        long[] prizeAddedWins = new long[11];
        long[] prizeTotalWins = new long[11];
        long totalBaseBoardWin = 0;
        int totalCompletedBonuses = 0;
        int[] slingoHits = new int[13];
        long[,] slingoPrizeAddedWins = new long[13, 11];
        long[,] slingoPrizeTotalWins = new long[13, 11];
        long[] slingoBaseBoardWins = new long[13];

        var tasks = new Task[workerCount];
        var workerResults = new (long[] addedWins, long[] totalWins, long baseBoardWin, int count, int[] hits, long[,] sAdded, long[,] sTotal, long[] sBase)[workerCount];

        var sw = Stopwatch.StartNew();

        for (int w = 0; w < workerCount; w++)
        {
            int workerIdx = w;
            int countToRun = (workerIdx == workerCount - 1) ? (totalRounds - workerIdx * roundsPerWorker) : roundsPerWorker;

            tasks[w] = Task.Run(() =>
            {
                var engine = new CashVortexSlotEngine(config);
                var rng = new FastRandom((uint)(Environment.TickCount + workerIdx * 31337 + 17));

                var locAddedWins = new long[11];
                var locTotalWins = new long[11];
                long locBaseBoardWin = 0;
                var locHits = new int[13];
                var locSAdded = new long[13, 11];
                var locSTotal = new long[13, 11];
                var locSBase = new long[13];

                for (int i = 0; i < countToRun; i++)
                {
                    var (completedSlingos, baseTotalWin, candidateAdded, candidateTotals) = engine.SimulateBonusForLadderCandidates(rng);
                    int sIdx = Math.Clamp(completedSlingos, 0, 12);
                    locHits[sIdx]++;

                    long baseCents = (long)Math.Round(baseTotalWin * 100);
                    locBaseBoardWin += baseCents;
                    locSBase[sIdx] += baseCents;

                    for (int p = 0; p < 11; p++)
                    {
                        long addCents = (long)Math.Round(candidateAdded[p] * 100);
                        long totCents = (long)Math.Round(candidateTotals[p] * 100);

                        locAddedWins[p] += addCents;
                        locTotalWins[p] += totCents;
                        locSAdded[sIdx, p] += addCents;
                        locSTotal[sIdx, p] += totCents;
                    }
                }

                workerResults[workerIdx] = (locAddedWins, locTotalWins, locBaseBoardWin, countToRun, locHits, locSAdded, locSTotal, locSBase);
            });
        }

        Task.WaitAll(tasks);
        sw.Stop();

        for (int w = 0; w < workerCount; w++)
        {
            var res = workerResults[w];
            totalCompletedBonuses += res.count;
            totalBaseBoardWin += res.baseBoardWin;

            for (int s = 0; s <= 12; s++)
            {
                slingoHits[s] += res.hits[s];
                slingoBaseBoardWins[s] += res.sBase[s];
                for (int p = 0; p < 11; p++)
                {
                    slingoPrizeAddedWins[s, p] += res.sAdded[s, p];
                    slingoPrizeTotalWins[s, p] += res.sTotal[s, p];
                }
            }

            for (int p = 0; p < 11; p++)
            {
                prizeAddedWins[p] += res.addedWins[p];
                prizeTotalWins[p] += res.totalWins[p];
            }
        }

        double avgBaseBoard = (double)totalBaseBoardWin / (totalCompletedBonuses * 100.0);
        Console.WriteLine($"Simulated {totalCompletedBonuses:N0} bonus rounds in {sw.ElapsedMilliseconds} ms.");
        Console.WriteLine($"Average Base Board Payout (before prize): {avgBaseBoard:F2}x bet\n");

        var prizeData = new List<(int id, string name, double avgAdded, double avgTotal)>();
        for (int p = 0; p < 11; p++)
        {
            double avgAdd = (double)prizeAddedWins[p] / (totalCompletedBonuses * 100.0);
            double avgTot = (double)prizeTotalWins[p] / (totalCompletedBonuses * 100.0);
            prizeData.Add((p, prizeNames[p], avgAdd, avgTot));
        }

        var sortedPrizes = prizeData.OrderBy(x => x.avgAdded).ToList();

        Console.WriteLine("=========================================================================================");
        Console.WriteLine("       RANKED 11 CANDIDATE PRIZES (ORDERED LOWEST TO HIGHEST AVERAGE VALUE)             ");
        Console.WriteLine("=========================================================================================");
        Console.WriteLine($"{"Rank",-5} | {"Prize Name",-35} | {"Avg Added Value",-16} | {"Avg Total Bonus Win",-20}");
        Console.WriteLine(new string('-', 85));

        for (int r = 0; r < sortedPrizes.Count; r++)
        {
            var p = sortedPrizes[r];
            Console.WriteLine($"{r + 1,5} | {p.name,-35} | {p.avgAdded,14:F2}x | {p.avgTotal,18:F2}x");
        }

        Console.WriteLine("\n=========================================================================================");
        Console.WriteLine("       DETAILED AVERAGE ADDED PAYOUT BY SLINGO LEVEL (0 .. 12 SLINGOS)                   ");
        Console.WriteLine("=========================================================================================");
        for (int s = 0; s <= 12; s++)
        {
            if (s == 11) continue;
            int sCount = slingoHits[s];
            if (sCount == 0) continue;
            double sChance = (double)sCount / totalCompletedBonuses;
            double avgSBase = (double)slingoBaseBoardWins[s] / (sCount * 100.0);

            Console.WriteLine($"\n--- Slingo {s,2} Line(s) (Hits: {sCount,6:N0} | {sChance,6:P2} of bonuses | Avg Base Board: {avgSBase:F2}x) ---");
            for (int r = 0; r < sortedPrizes.Count; r++)
            {
                var p = sortedPrizes[r];
                double sAdd = (double)slingoPrizeAddedWins[s, p.id] / (sCount * 100.0);
                double sTot = (double)slingoPrizeTotalWins[s, p.id] / (sCount * 100.0);
                Console.WriteLine($"  #{r + 1,2} {p.name,-34}: Added = {sAdd,6:F2}x | Total Win = {sTot,6:F2}x");
            }
        }
        Console.WriteLine("=========================================================================================\n");
    }

    private static void PopulateStatsWorksheet(
        IXLWorksheet ws,
        CashVortexConfig config,
        int totalSpins,
        double totalRtp,
        double lineWinRtp,
        double centerWheelRtp,
        double xWheelRtp,
        double lockAndSlingoRtp,
        double hitFreq,
        int totalSlingoLines,
        int centerWheelTriggers,
        int lockAndSlingoTriggers,
        double avgLnsWin,
        double avgBonusSpins,
        double avgBonusSlingos,
        int lockAndSlingoFullHouses,
        int jackpotCoinHits,
        int miniVortexHits,
        int megaVortexHits,
        int ultraVortexHits,
        int miniStrikeHits,
        int megaStrikeHits,
        int ultraStrikeHits,
        int xWheelHits,
        int[] strikeZeroHits,
        int[] strikeTotalLandings,
        int[] vortexZeroHits,
        int[] vortexTotalLandings,
        int[] wheelReachHits,
        long[] wheelRtpWin,
        Dictionary<string, int> centerWheelPrizeHits,
        Dictionary<string, long> centerWheelPrizeWins,
        Dictionary<string, int> wheelPrizeHits,
        Dictionary<string, long> wheelPrizeWins,
        int[] lockAndSlingoLadderHits,
        long[] lockAndSlingoLadderWins,
        long[] lockAndSlingoLadderBoardWins,
        long[] lockAndSlingoLadderPrizeWins,
        Dictionary<string, int> jackpotHits,
        Dictionary<string, long> jackpotWins)
    {
        // Title Banner
        ws.Cell("A1").Value = "CASH VORTEX: TRIPLE POWER – SIMULATION & MATHEMATICAL STATS DASHBOARD";
        ws.Range("A1:G1").Merge();
        ws.Range("A1:G1").Style.Font.Bold = true;
        ws.Range("A1:G1").Style.Font.FontSize = 14;
        ws.Range("A1:G1").Style.Font.FontColor = XLColor.White;
        ws.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.FromArgb(24, 43, 73);
        ws.Range("A1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A2").Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Simulated Spins: {totalSpins:N0} | Total Game RTP: {totalRtp:P2}";
        ws.Range("A2:G2").Merge();
        ws.Range("A2:G2").Style.Font.Italic = true;
        ws.Range("A2:G2").Style.Font.FontSize = 10;
        ws.Range("A2:G2").Style.Font.FontColor = XLColor.FromArgb(100, 110, 120);

        int r = 4;

        // SECTION 1: EXECUTIVE SUMMARY & RTP BREAKDOWN
        ws.Cell(r, 1).Value = "1. EXECUTIVE SUMMARY & RTP BREAKDOWN";
        ws.Range(r, 1, r, 5).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 5).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Metric / Component";
        ws.Cell(r, 2).Value = "Value / Metric Count";
        ws.Cell(r, 3).Value = "Hit Frequency / Trigger Rate";
        ws.Cell(r, 4).Value = "Spin Chance %";
        ws.Cell(r, 5).Value = "Contribution RTP %";
        ws.Range(r, 1, r, 5).Style.Font.Bold = true;
        ws.Range(r, 1, r, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        void AddSummaryRow(string metric, object val, string hitFreqStr, double spinChance, double rtpVal, bool highlight = false)
        {
            ws.Cell(r, 1).Value = metric;
            if (val is double d) ws.Cell(r, 2).Value = $"{d:F2}";
            else if (val is int i) ws.Cell(r, 2).Value = i;
            else if (val is long l) ws.Cell(r, 2).Value = l;
            else ws.Cell(r, 2).Value = val.ToString();

            ws.Cell(r, 3).Value = hitFreqStr;
            ws.Cell(r, 4).Value = spinChance > 0 ? $"{spinChance:P2}" : "-";
            ws.Cell(r, 5).Value = rtpVal > 0 ? $"{rtpVal:P2}" : "-";

            if (highlight)
            {
                ws.Range(r, 1, r, 5).Style.Font.Bold = true;
                ws.Range(r, 1, r, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(232, 245, 233);
            }
            r++;
        }

        AddSummaryRow("Total Game RTP", $"{totalRtp:P2}", "-", 1.0, totalRtp, true);
        AddSummaryRow("Base Game Slingo Lines Payout RTP", $"{lineWinRtp:P2}", $"1 in {totalSpins / (double)Math.Max(1, totalSlingoLines):F2} spins", (double)totalSlingoLines / totalSpins, lineWinRtp);
        AddSummaryRow("Center Wild Wheel Bonus RTP", $"{centerWheelRtp:P2}", $"1 in {totalSpins / (double)Math.Max(1, centerWheelTriggers):F2} spins", (double)centerWheelTriggers / totalSpins, centerWheelRtp);
        AddSummaryRow("Reel-Top X-Wheel Feature Direct RTP", $"{xWheelRtp:P2}", $"1 in {totalSpins / (double)Math.Max(1, xWheelHits):F2} spins", (double)xWheelHits / totalSpins, xWheelRtp);
        AddSummaryRow("Lock & Slingo™ Hold & Respin Bonus RTP", $"{lockAndSlingoRtp:P2}", $"1 in {totalSpins / (double)Math.Max(1, lockAndSlingoTriggers):F2} spins", (double)lockAndSlingoTriggers / totalSpins, lockAndSlingoRtp);
        AddSummaryRow("Overall Hit Frequency (Any Win)", $"{hitFreq:P2}", $"1 in {1.0 / Math.Max(0.0001, hitFreq):F2} spins", hitFreq, 0);
        AddSummaryRow("Total Slingo Lines Completed", totalSlingoLines, $"1 in {totalSpins / (double)Math.Max(1, totalSlingoLines):F2} spins", (double)totalSlingoLines / totalSpins, 0);
        AddSummaryRow("Center Wild Wheel Triggers", centerWheelTriggers, $"1 in {totalSpins / (double)Math.Max(1, centerWheelTriggers):F2} spins", (double)centerWheelTriggers / totalSpins, 0);
        AddSummaryRow("Lock & Slingo™ Bonus Triggers", lockAndSlingoTriggers, $"1 in {totalSpins / (double)Math.Max(1, lockAndSlingoTriggers):F2} spins", (double)lockAndSlingoTriggers / totalSpins, 0);
        AddSummaryRow("Average Bonus Win (x bet)", $"{avgLnsWin:F2}x", "-", 0, 0);
        AddSummaryRow("Average Spins per Bonus Round", $"{avgBonusSpins:F2}", "-", 0, 0);
        AddSummaryRow("Average Slingo Lines per Bonus Round", $"{avgBonusSlingos:F2}", "-", 0, 0);
        AddSummaryRow("Full House (25 Cells / 12 Slingos) Hits", lockAndSlingoFullHouses, $"{((double)lockAndSlingoFullHouses / Math.Max(1, lockAndSlingoTriggers)):P2} of bonuses", (double)lockAndSlingoFullHouses / totalSpins, 0);
        r += 2;

        // SECTION 2: SPECIAL SYMBOL TARGET EFFICIENCY & ZERO-EFFECT ANALYSIS
        ws.Cell(r, 1).Value = "2. SPECIAL SYMBOL TARGET EFFICIENCY & ZERO-EFFECT ANALYSIS";
        ws.Range(r, 1, r, 6).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Symbol Category";
        ws.Cell(r, 2).Value = "Total Landings";
        ws.Cell(r, 3).Value = "0-Effect Landings";
        ws.Cell(r, 4).Value = "0-Effect %";
        ws.Cell(r, 5).Value = "Spin Chance %";
        ws.Cell(r, 6).Value = "1 in N Spins";
        ws.Range(r, 1, r, 6).Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        void AddSymbolEffRow(string symName, int totalL, int zeroL)
        {
            ws.Cell(r, 1).Value = symName;
            ws.Cell(r, 2).Value = totalL;
            ws.Cell(r, 3).Value = zeroL;
            ws.Cell(r, 4).Value = totalL > 0 ? $"{((double)zeroL / totalL):P2}" : "0.00%";
            ws.Cell(r, 5).Value = $"{((double)totalL / totalSpins):P4}";
            ws.Cell(r, 6).Value = totalL > 0 ? $"1 in {totalSpins / (double)totalL:F1}" : "-";
            r++;
        }

        AddSymbolEffRow("Cash Strikes (All Categories)", strikeTotalLandings[0], strikeZeroHits[0]);
        AddSymbolEffRow("  * Mini Strike (Ortho Range)", strikeTotalLandings[1], strikeZeroHits[1]);
        AddSymbolEffRow("  * Mega Strike (Row & Col Range)", strikeTotalLandings[2], strikeZeroHits[2]);
        AddSymbolEffRow("  * Ultra Strike (Full Grid)", strikeTotalLandings[3], strikeZeroHits[3]);
        AddSymbolEffRow("Cash Vortexes (All Categories)", vortexTotalLandings[0], vortexZeroHits[0]);
        AddSymbolEffRow("  * Mini Vortex (Ortho Range)", vortexTotalLandings[1], vortexZeroHits[1]);
        AddSymbolEffRow("  * Mega Vortex (Row & Col Range)", vortexTotalLandings[2], vortexZeroHits[2]);
        AddSymbolEffRow("  * Ultra Vortex (Full Grid)", vortexTotalLandings[3], vortexZeroHits[3]);
        r += 2;

        // SECTION 3: CENTER WILD WHEEL BONUS PRIZE BREAKDOWN
        ws.Cell(r, 1).Value = "3. CENTER WILD WHEEL BONUS PRIZE BREAKDOWN";
        ws.Range(r, 1, r, 6).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Wheel Prize";
        ws.Cell(r, 2).Value = "Hits";
        ws.Cell(r, 3).Value = "Wheel Hit Chance %";
        ws.Cell(r, 4).Value = "Total Spins Hit Chance %";
        ws.Cell(r, 5).Value = "Direct Win Amount";
        ws.Cell(r, 6).Value = "Direct RTP %";
        ws.Range(r, 1, r, 6).Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        foreach (var prize in config.CenterWheelPrizes)
        {
            int hits = centerWheelPrizeHits.GetValueOrDefault(prize.PrizeString);
            long win = centerWheelPrizeWins.GetValueOrDefault(prize.PrizeString);
            double wheelChance = centerWheelTriggers > 0 ? (double)hits / centerWheelTriggers : 0;
            double spinChance = (double)hits / totalSpins;
            double prizeRtp = (double)win / (totalSpins * 100.0);

            ws.Cell(r, 1).Value = prize.PrizeString;
            ws.Cell(r, 2).Value = hits;
            ws.Cell(r, 3).Value = $"{wheelChance:P2}";
            ws.Cell(r, 4).Value = $"{spinChance:P4}";
            ws.Cell(r, 5).Value = win;
            ws.Cell(r, 6).Value = $"{prizeRtp:P4}";
            r++;
        }
        r += 2;

        // SECTION 4: REEL-TOP X-WHEEL DETAILED BREAKDOWN
        ws.Cell(r, 1).Value = "4. REEL-TOP X-WHEEL FEATURE DETAILED BREAKDOWN";
        ws.Range(r, 1, r, 7).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 7).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Wheel Tier";
        ws.Cell(r, 2).Value = "Prize String";
        ws.Cell(r, 3).Value = "Hits";
        ws.Cell(r, 4).Value = "Wheel Hit Chance %";
        ws.Cell(r, 5).Value = "Total Spins Hit Chance %";
        ws.Cell(r, 6).Value = "Direct Win Amount";
        ws.Cell(r, 7).Value = "Direct RTP %";
        ws.Range(r, 1, r, 7).Style.Font.Bold = true;
        ws.Range(r, 1, r, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        for (int wL = 1; wL <= 3; wL++)
        {
            string wTag = $"W{wL}";
            string wTitle = wL == 1 ? "Wheel 1 (Mini)" : (wL == 2 ? "Wheel 2 (Mega)" : "Wheel 3 (Ultra)");
            int wheelTotalSpins = wheelReachHits[wL];
            var prizeList = wL == 1 ? config.MiniWheelPrizes : (wL == 2 ? config.MegaWheelPrizes : config.UltraWheelPrizes);

            foreach (var prizeDef in prizeList)
            {
                string pKey = $"{wTag}:{prizeDef.PrizeString}";
                int hits = wheelPrizeHits.GetValueOrDefault(pKey);
                long win = wheelPrizeWins.GetValueOrDefault(pKey);
                double hitChanceWheel = wheelTotalSpins > 0 ? (double)hits / wheelTotalSpins : 0;
                double hitChanceSpins = (double)hits / totalSpins;
                double prizeRtp = (double)win / (totalSpins * 100.0);

                ws.Cell(r, 1).Value = wTitle;
                ws.Cell(r, 2).Value = prizeDef.PrizeString;
                ws.Cell(r, 3).Value = hits;
                ws.Cell(r, 4).Value = $"{hitChanceWheel:P2}";
                ws.Cell(r, 5).Value = $"{hitChanceSpins:P4}";
                ws.Cell(r, 6).Value = win;
                ws.Cell(r, 7).Value = $"{prizeRtp:P4}";
                r++;
            }
        }
        r += 2;

        // SECTION 5: LOCK & SLINGO™ LADDER ACHIEVEMENTS
        ws.Cell(r, 1).Value = "5. LOCK & SLINGO™ RESPIN LADDER ACHIEVEMENTS";
        ws.Range(r, 1, r, 7).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 7).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Slingos Completed";
        ws.Cell(r, 2).Value = "Ladder Award Description";
        ws.Cell(r, 3).Value = "Hits";
        ws.Cell(r, 4).Value = "Bonus Hit Chance %";
        ws.Cell(r, 5).Value = "Avg Base Board (x bet)";
        ws.Cell(r, 6).Value = "Avg Ladder Prize (x bet)";
        ws.Cell(r, 7).Value = "Avg Total Bonus Win (x bet)";
        ws.Range(r, 1, r, 7).Style.Font.Bold = true;
        ws.Range(r, 1, r, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        for (int s = 0; s <= 12; s++)
        {
            if (s == 11) continue;
            int sHits = lockAndSlingoLadderHits[s];
            double sChance = lockAndSlingoTriggers > 0 ? (double)sHits / lockAndSlingoTriggers : 0;

            var ladderDef = config.SlingoLadderPrizes.FirstOrDefault(p => p.SlingoCount == s);
            string desc = ladderDef?.PrizeString ?? (s == 0 ? "No Lines" : "Standard");
            if (s == 12) desc = "FULL HOUSE (Ultra Jackpot 500x)";
            else if (s == 8) desc = "Mega Jackpot (50x)";
            else if (s == 4) desc = "Mini Jackpot (5x)";

            double avgBonusWin = sHits > 0 ? (double)lockAndSlingoLadderWins[s] / (sHits * 100.0) : 0;
            double avgBoardWin = sHits > 0 ? (double)lockAndSlingoLadderBoardWins[s] / (sHits * 100.0) : 0;
            double avgPrizeWin = sHits > 0 ? (double)lockAndSlingoLadderPrizeWins[s] / (sHits * 100.0) : 0;

            ws.Cell(r, 1).Value = $"Slingo {s}";
            ws.Cell(r, 2).Value = desc;
            ws.Cell(r, 3).Value = sHits;
            ws.Cell(r, 4).Value = $"{sChance:P2}";
            ws.Cell(r, 5).Value = $"{avgBoardWin:F2}";
            ws.Cell(r, 6).Value = $"{avgPrizeWin:F2}";
            ws.Cell(r, 7).Value = $"{avgBonusWin:F2}";
            r++;
        }
        r += 2;

        // SECTION 6: JACKPOT SYSTEM BREAKDOWN
        ws.Cell(r, 1).Value = "6. JACKPOT SYSTEM BREAKDOWN";
        ws.Range(r, 1, r, 6).Merge().Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Font.FontSize = 11;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(215, 228, 242);
        r++;

        ws.Cell(r, 1).Value = "Jackpot Tier";
        ws.Cell(r, 2).Value = "Multiplier";
        ws.Cell(r, 3).Value = "Total Hits";
        ws.Cell(r, 4).Value = "Total Win Amount";
        ws.Cell(r, 5).Value = "Contribution RTP %";
        ws.Cell(r, 6).Value = "1 in N Spins";
        ws.Range(r, 1, r, 6).Style.Font.Bold = true;
        ws.Range(r, 1, r, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(238, 243, 250);
        ws.Range(r, 1, r, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        r++;

        foreach (var jp in config.JackpotCoins)
        {
            int hits = jackpotHits.GetValueOrDefault(jp.JackpotName);
            long win = jackpotWins.GetValueOrDefault(jp.JackpotName);
            double jpRtp = (double)win / (totalSpins * 100.0);

            ws.Cell(r, 1).Value = jp.JackpotName;
            ws.Cell(r, 2).Value = $"{jp.Multiplier}x";
            ws.Cell(r, 3).Value = hits;
            ws.Cell(r, 4).Value = win;
            ws.Cell(r, 5).Value = $"{jpRtp:P4}";
            ws.Cell(r, 6).Value = hits > 0 ? $"1 in {totalSpins / (double)hits:F1}" : "-";
            r++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void SanitizeDrawingIds(string filePath)
    {
        try
        {
            using var zip = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Update);
            int nextId = 1;
            var drawingEntries = zip.Entries
                .Where(e => e.FullName.StartsWith("xl/drawings/drawing", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var entry in drawingEntries)
            {
                string content;
                using (var reader = new StreamReader(entry.Open()))
                {
                    content = reader.ReadToEnd();
                }

                if (content.Contains("cNvPr id="))
                {
                    string fullName = entry.FullName;
                    string modified = System.Text.RegularExpressions.Regex.Replace(content, @"cNvPr id=""[^""]+""", m => $@"cNvPr id=""{nextId++}""");
                    entry.Delete();
                    var newEntry = zip.CreateEntry(fullName);
                    using var writer = new StreamWriter(newEntry.Open());
                    writer.Write(modified);
                }
            }
        }
        catch
        {
            // Ignore non-fatal drawing sanitize errors
        }
    }
}
