using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

namespace Forefinger;

[ModInitializer(nameof(Initialize))]
public static class Entry
{
    public const string ModId = "Forefinger";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; private set; } = null!;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Logger = RitsuLibFramework.CreateLogger(ModId);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        Logger.Info("Forefinger initialized.");
    }
}

