using System.Reflection;
using System.Text.Json;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

public static class UncappedActsConfig
{
    public static bool UncappedActsEnabled { get; private set; } = true;
    public static bool AscensionIncreaseEnabled { get; private set; } = true;
    public static float ScalingHpIncrement { get; private set; } = 1.15f;
    public static float ScalingDmgIncrement { get; private set; } = 1.1f;

    private class FileFormat
    {
        public bool UncappedActsEnabled { get; set; } = true;
        public bool AscensionIncreaseEnabled { get; set; } = true;
        public float ScalingHpIncrement { get; set; } = 1.15f;
        public float ScalingDmgIncrement { get; set; } = 1.1f;
    }

    public static void Load()
    {
        try
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(assemblyDir, "UncappedActsConfig.json");

            if (!File.Exists(path))
            {
                MainFile.Logger.Warn($"UncappedActsConfig.json not found at {path}; using defaults.");
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<FileFormat>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data == null)
            {
                MainFile.Logger.Warn("UncappedActsConfig.json failed to parse; using defaults.");
                return;
            }

            UncappedActsEnabled = data.UncappedActsEnabled;
            AscensionIncreaseEnabled = data.AscensionIncreaseEnabled;
            ScalingHpIncrement = data.ScalingHpIncrement;
            ScalingDmgIncrement = data.ScalingDmgIncrement;

            MainFile.Logger.Info(
                $"Loaded Uncapped Acts config: enabled={UncappedActsEnabled}, hpIncrement={ScalingHpIncrement}, dmgIncrement={ScalingDmgIncrement}, ascension={AscensionIncreaseEnabled}.");
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Failed to load UncappedActsConfig.json: {e}");
        }
    }
}
