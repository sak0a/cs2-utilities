using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace CS2Utilities;

// [MinimumApiVersion(160)]
public class CS2Utilities : BasePlugin, IPluginConfig<CS2UtilitiesConfig>
{
    public override string ModuleName => "CS2-Utilities";
    public override string ModuleDescription => "";
    public override string ModuleAuthor => "sak0a";
    public override string ModuleVersion => "0.0.1";

    public CS2UtilitiesConfig Config { get; set; } = new();

    public void OnConfigParsed(CS2UtilitiesConfig config)
    {
        Console.WriteLine($"{config.StringValue} {config.IntValue}!");
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        Console.WriteLine($"{ModuleName} loaded successfully!");
    }
}