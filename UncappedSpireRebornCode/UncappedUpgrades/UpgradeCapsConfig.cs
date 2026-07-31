using System.Reflection;
using System.Text.Json;
using MegaCrit.Sts2.Core.Models;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedUpgrades;

public static class UpgradeCapsConfig
{
    public static HashSet<ModelId> EnergyOnlyCards { get; private set; } = [];
    public static Dictionary<ModelId, int> CardUpgradeMaxMap { get; private set; } = new();

    private class FileFormat
    {
        public List<string> EnergyOnlyCardIds { get; set; } = [];
        public Dictionary<string, int> CardUpgradeMax { get; set; } = new();
    }

    public static void Load()
    {
        try
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(assemblyDir, "UpgradeCaps.json");

            if (!File.Exists(path))
            {
                MainFile.Logger.Warn($"UpgradeCaps.json not found at {path}; all cards will be fully uncapped.");
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<FileFormat>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data == null)
            {
                MainFile.Logger.Warn("UpgradeCaps.json failed to parse; all cards will be fully uncapped.");
                return;
            }

            EnergyOnlyCards = data.EnergyOnlyCardIds
                .Select(id => new ModelId("CARD", id))
                .ToHashSet();

            CardUpgradeMaxMap = data.CardUpgradeMax
                .ToDictionary(kv => new ModelId("CARD", kv.Key), kv => kv.Value);

            MainFile.Logger.Info(
                $"Loaded upgrade caps: {EnergyOnlyCards.Count} energy-only, {CardUpgradeMaxMap.Count} explicit caps.");
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Failed to load UpgradeCaps.json: {e}");
        }
    }
}