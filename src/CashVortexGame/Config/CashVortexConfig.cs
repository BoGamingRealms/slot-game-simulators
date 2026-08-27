using System;
using System.Collections.Generic;
using System.Linq;
using SlotFramework.Models;
using SlotFramework.Utilities;

namespace CashVortexGame.Config;

public enum WheelPrizeType
{
    Multiplier,
    UltraStrike,
    Jackpot,
    LockAndSlingo,
    Upgrade,
    InstantCash
}

public class WheelPrizeDef
{
    public int PrizeId { get; set; }
    public string PrizeString { get; set; } = string.Empty;
    public int Weight { get; set; }
    public WheelPrizeType Type { get; set; }
    public double ParameterValue { get; set; } = 0.0;
    public string? JackpotType { get; set; }
}

public class TableSelection
{
    public int TableId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class SpecialSymbolChance
{
    public int TableId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int SpecialSymbolWeight { get; set; }
    public int NoSpecialSymbolWeight { get; set; }
}

public class SpecialSymbolDef
{
    public int SymbolId { get; set; }
    public string SymbolName { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class JackpotCoinDef
{
    public int JackpotId { get; set; }
    public string JackpotName { get; set; } = string.Empty;
    public double Multiplier { get; set; }
    public int Weight { get; set; }
}

public class CashValueDef
{
    public double Multiplier { get; set; }
    public int Weight { get; set; }
}

public class CashCoinChance
{
    public int TableId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CoinWeight { get; set; }
    public int BlankWeight { get; set; }
}

public class SlingoLadderPrizeDef
{
    public int SlingoCount { get; set; }
    public string PrizeString { get; set; } = string.Empty;
    public WheelPrizeType Type { get; set; }
    public double ParameterValue { get; set; }
    public string? JackpotType { get; set; }
}

public class BonusOutcomeItem
{
    public SymbolType Type { get; set; }
    public int Count { get; set; } = 1;
}

public class BonusOutcomeDef
{
    public int OutcomeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int[] WeightsBySpaceBucket { get; set; } = new int[5];
    public List<BonusOutcomeItem> Items { get; set; } = new();
}

public class CashVortexConfig
{
    public string GameName { get; set; } = "Cash Vortex - Triple Power";

    // Base Game Config
    public List<TableSelection> TableSelections { get; set; } = new();
    public List<SpecialSymbolChance> SpecialSymbolChances { get; set; } = new();
    public List<SpecialSymbolDef> SpecialSymbolDefs { get; set; } = new();
    public List<JackpotCoinDef> JackpotCoins { get; set; } = new();
    public List<CashValueDef> CashStrikeValues { get; set; } = new();
    public List<CashCoinChance> CashCoinChances { get; set; } = new();
    public List<CashValueDef> CashCoinValues { get; set; } = new();

    // 3-Wheel System at top of reels (Triggered by X Symbol)
    public List<WheelPrizeDef> MiniWheelPrizes { get; set; } = new();
    public List<WheelPrizeDef> MegaWheelPrizes { get; set; } = new();
    public List<WheelPrizeDef> UltraWheelPrizes { get; set; } = new();

    // Center Wild Slingo Wheel Bonus (Triggered by lines passing through center wild star)
    public List<WheelPrizeDef> CenterWheelPrizes { get; set; } = new();

    // Lock & Slingo Bonus Config
    public List<SlingoLadderPrizeDef> SlingoLadderPrizes { get; set; } = new();
    public int BonusBaseFactor { get; set; } = 400;
    public int[,] BonusLandingWeightsByLifeAndBucket { get; set; } = new int[4, 5]; // life: 1..3, bucket: 0..4
    public List<BonusOutcomeDef> BonusOutcomeDefs { get; set; } = new();
    public List<JackpotCoinDef> BonusJackpotCoins { get; set; } = new();
    public List<SpecialSymbolDef> BonusCashStrikeTypes { get; set; } = new();
    public List<CashValueDef> BonusCashStrikeValues { get; set; } = new();
    public List<CashValueDef> BonusCashCoinValues { get; set; } = new();

    public List<WheelPrizeDef> BonusMiniWheelPrizes { get; set; } = new();
    public List<WheelPrizeDef> BonusMegaWheelPrizes { get; set; } = new();
    public List<WheelPrizeDef> BonusUltraWheelPrizes { get; set; } = new();

    // Fast sampling structures
    public WeightTable TableSelectionWeights { get; set; } = new(Array.Empty<int>());
    public Dictionary<int, WeightTable> SpecialSymbolChanceWeights { get; set; } = new();
    public WeightTable SpecialSymbolTypeWeights { get; set; } = new(Array.Empty<int>());
    public WeightTable JackpotTypeWeights { get; set; } = new(Array.Empty<int>());
    public WeightTable CashStrikeValueWeights { get; set; } = new(Array.Empty<int>());
    public Dictionary<int, WeightTable> CashCoinChanceWeights { get; set; } = new();
    public WeightTable CashCoinValueWeights { get; set; } = new(Array.Empty<int>());

    public WeightTable MiniWheelWeightTable { get; set; } = new(Array.Empty<int>());
    public WeightTable MegaWheelWeightTable { get; set; } = new(Array.Empty<int>());
    public WeightTable UltraWheelWeightTable { get; set; } = new(Array.Empty<int>());
    public WeightTable CenterWheelWeightTable { get; set; } = new(Array.Empty<int>());

    // Bonus sampling structures
    public WeightTable[] BonusOutcomeWeightsByBucket { get; set; } = Array.Empty<WeightTable>();
    public WeightTable BonusJackpotWeights { get; set; } = new(Array.Empty<int>());
    public WeightTable BonusCashStrikeTypeWeights { get; set; } = new(Array.Empty<int>());
    public WeightTable BonusCashStrikeValueWeights { get; set; } = new(Array.Empty<int>());
    public WeightTable BonusCashCoinValueWeights { get; set; } = new(Array.Empty<int>());

    public WeightTable BonusMiniWheelWeightTable { get; set; } = new(Array.Empty<int>());
    public WeightTable BonusMegaWheelWeightTable { get; set; } = new(Array.Empty<int>());
    public WeightTable BonusUltraWheelWeightTable { get; set; } = new(Array.Empty<int>());

    public void BuildWeightTables()
    {
        TableSelectionWeights = new WeightTable(TableSelections.Select(t => t.Weight).ToArray());

        SpecialSymbolChanceWeights.Clear();
        foreach (var ssc in SpecialSymbolChances)
        {
            SpecialSymbolChanceWeights[ssc.TableId] = new WeightTable(new[] { ssc.SpecialSymbolWeight, ssc.NoSpecialSymbolWeight });
        }

        SpecialSymbolTypeWeights = new WeightTable(SpecialSymbolDefs.Select(s => s.Weight).ToArray());
        JackpotTypeWeights = new WeightTable(JackpotCoins.Select(j => j.Weight).ToArray());
        CashStrikeValueWeights = new WeightTable(CashStrikeValues.Select(c => c.Weight).ToArray());

        CashCoinChanceWeights.Clear();
        foreach (var ccc in CashCoinChances)
        {
            CashCoinChanceWeights[ccc.TableId] = new WeightTable(new[] { ccc.CoinWeight, ccc.BlankWeight });
        }

        CashCoinValueWeights = new WeightTable(CashCoinValues.Select(c => c.Weight).ToArray());

        MiniWheelWeightTable = new WeightTable(MiniWheelPrizes.Select(p => p.Weight).ToArray());
        MegaWheelWeightTable = new WeightTable(MegaWheelPrizes.Select(p => p.Weight).ToArray());
        UltraWheelWeightTable = new WeightTable(UltraWheelPrizes.Select(p => p.Weight).ToArray());
        CenterWheelWeightTable = new WeightTable(CenterWheelPrizes.Select(p => p.Weight).ToArray());

        // Build Bonus Weight Tables
        BonusOutcomeWeightsByBucket = new WeightTable[5];
        for (int b = 0; b < 5; b++)
        {
            var bucketWeights = BonusOutcomeDefs.Select(o => (b < o.WeightsBySpaceBucket.Length) ? o.WeightsBySpaceBucket[b] : 0).ToArray();
            BonusOutcomeWeightsByBucket[b] = new WeightTable(bucketWeights);
        }

        BonusJackpotWeights = new WeightTable((BonusJackpotCoins.Count > 0 ? BonusJackpotCoins : JackpotCoins).Select(j => j.Weight).ToArray());
        BonusCashStrikeTypeWeights = new WeightTable(BonusCashStrikeTypes.Select(s => s.Weight).ToArray());
        BonusCashStrikeValueWeights = new WeightTable((BonusCashStrikeValues.Count > 0 ? BonusCashStrikeValues : CashStrikeValues).Select(c => c.Weight).ToArray());
        BonusCashCoinValueWeights = new WeightTable((BonusCashCoinValues.Count > 0 ? BonusCashCoinValues : CashCoinValues).Select(c => c.Weight).ToArray());

        BonusMiniWheelWeightTable = new WeightTable((BonusMiniWheelPrizes.Count > 0 ? BonusMiniWheelPrizes : MiniWheelPrizes).Select(p => p.Weight).ToArray());
        BonusMegaWheelWeightTable = new WeightTable((BonusMegaWheelPrizes.Count > 0 ? BonusMegaWheelPrizes : MegaWheelPrizes).Select(p => p.Weight).ToArray());
        BonusUltraWheelWeightTable = new WeightTable((BonusUltraWheelPrizes.Count > 0 ? BonusUltraWheelPrizes : UltraWheelPrizes).Select(p => p.Weight).ToArray());
    }

    public static CashVortexConfig CreateBalanced955()
    {
        var config = new CashVortexConfig
        {
            GameName = "Cash Vortex - Triple Power (Balanced 95.5% RTP)"
        };

        // Table Selections
        config.TableSelections.Add(new TableSelection { TableId = 0, Description = "Low Symbol Chance", Weight = 1000 });
        config.TableSelections.Add(new TableSelection { TableId = 1, Description = "Medium Symbol Chance", Weight = 300 });
        config.TableSelections.Add(new TableSelection { TableId = 2, Description = "High Symbol Chance", Weight = 100 });

        // Special Symbol Chances per Table
        config.SpecialSymbolChances.Add(new SpecialSymbolChance { TableId = 0, Description = "Low Symbol Chance", SpecialSymbolWeight = 150, NoSpecialSymbolWeight = 1000 });
        config.SpecialSymbolChances.Add(new SpecialSymbolChance { TableId = 1, Description = "Medium Symbol Chance", SpecialSymbolWeight = 200, NoSpecialSymbolWeight = 1000 });
        config.SpecialSymbolChances.Add(new SpecialSymbolChance { TableId = 2, Description = "High Symbol Chance", SpecialSymbolWeight = 250, NoSpecialSymbolWeight = 1000 });

        // Special Symbol Types (Same types, balanced weights)
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolId = 0, SymbolName = "Jackpot Coin", Weight = 500 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Mini Vortex", Weight = 800 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Mega Vortex", Weight = 250 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Ultra Vortex", Weight = 80 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Mini Strike", Weight = 800 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Mega Strike", Weight = 250 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "Ultra Strike", Weight = 80 });
        config.SpecialSymbolDefs.Add(new SpecialSymbolDef { SymbolName = "X Wheel", Weight = 800 });

        // Jackpot Coins (Same values: 5x, 50x, 500x)
        config.JackpotCoins.Add(new JackpotCoinDef { JackpotId = 0, JackpotName = "Mini", Multiplier = 5.0, Weight = 1000 });
        config.JackpotCoins.Add(new JackpotCoinDef { JackpotId = 1, JackpotName = "Mega", Multiplier = 50.0, Weight = 25 });
        config.JackpotCoins.Add(new JackpotCoinDef { JackpotId = 2, JackpotName = "Ultra", Multiplier = 500.0, Weight = 1 });

        // Cash Strike Values (Exact same values, balanced weights)
        double[] strikeVals = { 0.2, 0.4, 0.6, 0.8, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0 };
        int[] strikeW = { 2500, 2000, 1500, 800, 500, 200, 100, 50, 30, 20, 15, 10, 5 };
        for (int i = 0; i < strikeVals.Length; i++)
        {
            config.CashStrikeValues.Add(new CashValueDef { Multiplier = strikeVals[i], Weight = strikeW[i] });
        }

        // Cash Coin Landing Chances per Table
        config.CashCoinChances.Add(new CashCoinChance { TableId = 0, Description = "Low Symbol Chance", CoinWeight = 68, BlankWeight = 1000 });
        config.CashCoinChances.Add(new CashCoinChance { TableId = 1, Description = "Medium Symbol Chance", CoinWeight = 155, BlankWeight = 1000 });
        config.CashCoinChances.Add(new CashCoinChance { TableId = 2, Description = "High Symbol Chance", CoinWeight = 310, BlankWeight = 1000 });

        // Cash Coin Values (Exact same values, balanced weights)
        double[] coinVals = { 0.2, 0.4, 0.6, 0.8, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0 };
        int[] coinW = { 2500, 2250, 1800, 900, 500, 200, 85, 40, 25, 14, 9, 6, 4 };
        for (int i = 0; i < coinVals.Length; i++)
        {
            config.CashCoinValues.Add(new CashValueDef { Multiplier = coinVals[i], Weight = coinW[i] });
        }

        // Reel-Top X Wheels (Mini, Mega, Ultra)
        config.MiniWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(0, "x2", 1000));
        config.MiniWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(1, "2", 1200));
        config.MiniWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(2, "Mini Jackpot", 400));
        config.MiniWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(3, "Upgrade", 250));

        config.MegaWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(0, "x3", 1000));
        config.MegaWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(1, "3", 1200));
        config.MegaWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(2, "Mega Jackpot", 100));
        config.MegaWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(3, "Upgrade", 250));

        config.UltraWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(0, "x5", 1000));
        config.UltraWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(1, "5", 1400));
        config.UltraWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(2, "Ultra Jackpot", 8));
        config.UltraWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(3, "Lock & Slingo", 450));

        // Center Wild Wheel Bonus (Exact same prizes, calibrated weights)
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(0, "1", 3350));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(1, "2", 2350));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(2, "3", 1400));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(3, "4", 700));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(4, "5", 350));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(5, "Mini Jackpot", 680));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(6, "Mega Jackpot", 32));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(7, "Ultra Jackpot", 1));
        config.CenterWheelPrizes.Add(CashVortexExcelLoader.ParsePrizeDef(8, "Lock&Slingo", 1350));

        // Lock & Slingo Bonus Config
        config.BonusBaseFactor = 400;
        int[,] landingWeights = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 290, 250, 200, 140, 60 }, // Life 1
            { 330, 290, 240, 170, 85 }, // Life 2
            { 370, 330, 270, 200, 110 } // Life 3
        };
        for (int l = 0; l <= 3; l++)
        {
            for (int b = 0; b < 5; b++)
            {
                config.BonusLandingWeightsByLifeAndBucket[l, b] = landingWeights[l, b];
            }
        }

        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 1, PrizeString = "0", Type = WheelPrizeType.Multiplier, ParameterValue = 0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 2, PrizeString = "0", Type = WheelPrizeType.Multiplier, ParameterValue = 0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 3, PrizeString = "0", Type = WheelPrizeType.Multiplier, ParameterValue = 0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 4, PrizeString = "Mini Jackpot", Type = WheelPrizeType.Jackpot, JackpotType = "Mini" });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 5, PrizeString = "1", Type = WheelPrizeType.UltraStrike, ParameterValue = 1.0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 6, PrizeString = "2", Type = WheelPrizeType.UltraStrike, ParameterValue = 2.0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 7, PrizeString = "3", Type = WheelPrizeType.UltraStrike, ParameterValue = 3.0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 8, PrizeString = "Mega Jackpot", Type = WheelPrizeType.Jackpot, JackpotType = "Mega" });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 9, PrizeString = "5", Type = WheelPrizeType.UltraStrike, ParameterValue = 5.0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 10, PrizeString = "x2", Type = WheelPrizeType.Multiplier, ParameterValue = 2.0 });
        config.SlingoLadderPrizes.Add(new SlingoLadderPrizeDef { SlingoCount = 12, PrizeString = "Ultra Jackpot", Type = WheelPrizeType.Jackpot, JackpotType = "Ultra" });

        config.BonusCashStrikeTypes.Add(new SpecialSymbolDef { SymbolName = "Mini Strike", Weight = 1000 });
        config.BonusCashStrikeTypes.Add(new SpecialSymbolDef { SymbolName = "Mega Strike", Weight = 300 });
        config.BonusCashStrikeTypes.Add(new SpecialSymbolDef { SymbolName = "Ultra Strike", Weight = 50 });

        config.BonusOutcomeDefs.Add(new BonusOutcomeDef
        {
            OutcomeId = 0,
            Description = "1 Cash Coin",
            WeightsBySpaceBucket = new[] { 1000, 1000, 1000, 1000, 1000 },
            Items = new List<BonusOutcomeItem> { new() { Type = SymbolType.CashCoin, Count = 1 } }
        });
        config.BonusOutcomeDefs.Add(new BonusOutcomeDef
        {
            OutcomeId = 1,
            Description = "2 Cash Coins",
            WeightsBySpaceBucket = new[] { 350, 280, 200, 110, 30 },
            Items = new List<BonusOutcomeItem> { new() { Type = SymbolType.CashCoin, Count = 2 } }
        });
        config.BonusOutcomeDefs.Add(new BonusOutcomeDef
        {
            OutcomeId = 2,
            Description = "Mini Strike",
            WeightsBySpaceBucket = new[] { 210, 165, 125, 75, 25 },
            Items = new List<BonusOutcomeItem> { new() { Type = SymbolType.MiniStrike, Count = 1 } }
        });
        config.BonusOutcomeDefs.Add(new BonusOutcomeDef
        {
            OutcomeId = 3,
            Description = "Mini Vortex",
            WeightsBySpaceBucket = new[] { 150, 115, 85, 45, 15 },
            Items = new List<BonusOutcomeItem> { new() { Type = SymbolType.MiniVortex, Count = 1 } }
        });
        config.BonusOutcomeDefs.Add(new BonusOutcomeDef
        {
            OutcomeId = 4,
            Description = "Jackpot Coin",
            WeightsBySpaceBucket = new[] { 50, 40, 28, 16, 6 },
            Items = new List<BonusOutcomeItem> { new() { Type = SymbolType.JackpotCoin, Count = 1 } }
        });

        config.BuildWeightTables();
        return config;
    }
}
