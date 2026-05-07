using System.Reflection;
using DemoMod.TheGleaner.Encounters;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace DemoMod.TheGleaner;

[ModInitializer(nameof(initialize))]
public class TheGleanerMod
{
    private const string HarmonyId = "TheGleanerMod";

    private static readonly Type[] EncounterModelTypes =
    [
        typeof(GleanerCastleAndPawnsEncounter),
        typeof(GleanerCastle),
        typeof(GleanerPawn),
        typeof(GleanerPromotionPower),
        typeof(GleanerIvoryAndEbonyEncounter),
        typeof(GleanerIvoryBishop),
        typeof(GleanerEbonyBishop),
        typeof(GleanerWhiteSquareDomainPower),
        typeof(GleanerBlackSquareDomainPower),
        typeof(GleanerFanaticPower),
        typeof(GleanerStrengthDecayPower)
    ];

    private static Harmony? _harmony;
    private static bool _encounterHooksInstalled;

    public static void initialize()
    {
        InjectEncounterSavedPropertyCaches();
        EnsureEncounterModelsRegisteredIfModelDbInitialized();

        Harmony harmony = _harmony ??= new Harmony(HarmonyId);
        harmony.PatchAll();
        InstallEncounterHooks(harmony);

        ResetModelDbCaches();
        ResetActEncounterCaches<Glory>();

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(TheGleanerMod).Assembly);
    }

    private static void InjectEncounterSavedPropertyCaches()
    {
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerPawn));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerPromotionPower));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerIvoryBishop));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerEbonyBishop));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerWhiteSquareDomainPower));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerBlackSquareDomainPower));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerFanaticPower));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(GleanerStrengthDecayPower));
    }

    private static bool EnsureEncounterModelsRegisteredIfModelDbInitialized()
    {
        if (!IsModelDbInitialized())
        {
            return false;
        }

        foreach (Type type in EncounterModelTypes)
        {
            if (!ModelDb.Contains(type))
            {
                ModelDb.Inject(type);
            }

            ModelId id = ModelDb.GetId(type);
            if (ModelIdSerializationCache.TryGetNetIdForCategory(id.Category, out _)
                && ModelIdSerializationCache.TryGetNetIdForEntry(id.Entry, out _))
            {
                TryInitializeModelId(ModelDb.GetByIdOrNull<AbstractModel>(id), id);
            }
        }

        return true;
    }

    private static void TryInitializeModelId(AbstractModel? model, ModelId id)
    {
        if (model == null)
        {
            return;
        }

        try
        {
            model.InitId(id);
        }
        catch (ArgumentException)
        {
            // NetIds may be initialized later during game startup.
        }
    }

    private static bool IsModelDbInitialized()
    {
        return ModelDb.Contains(typeof(Glory));
    }

    private static void InstallEncounterHooks(Harmony harmony)
    {
        if (_encounterHooksInstalled)
        {
            return;
        }

        _encounterHooksInstalled = true;
        harmony.Patch(
            RequireMethod(typeof(Glory), nameof(Glory.GenerateAllEncounters), BindingFlags.Instance | BindingFlags.Public),
            postfix: new HarmonyMethod(typeof(TheGleanerMod), nameof(GloryGenerateAllEncountersPostfix)));
    }

    private static void GloryGenerateAllEncountersPostfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = AppendUnique(
            __result,
            GetEncounter<GleanerCastleAndPawnsEncounter>(),
            GetEncounter<GleanerIvoryAndEbonyEncounter>());
    }

    private static EncounterModel? GetEncounter<T>() where T : EncounterModel
    {
        if (!EnsureEncounterModelsRegisteredIfModelDbInitialized())
        {
            return null;
        }

        return ModelDb.GetByIdOrNull<EncounterModel>(ModelDb.GetId<T>());
    }

    private static IEnumerable<EncounterModel> AppendUnique(IEnumerable<EncounterModel> original, params EncounterModel?[] extras)
    {
        HashSet<ModelId> seen = new();
        foreach (EncounterModel encounter in original)
        {
            if (seen.Add(encounter.Id))
            {
                yield return encounter;
            }
        }

        foreach (EncounterModel? extra in extras)
        {
            if (extra != null && seen.Add(extra.Id))
            {
                yield return extra;
            }
        }
    }

    private static void ResetModelDbCaches()
    {
        foreach (string fieldName in new[] { "_allEncounters", "_allMonsters", "_allPowers" })
        {
            typeof(ModelDb).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
        }
    }

    private static void ResetActEncounterCaches<TAct>() where TAct : ActModel
    {
        if (!ModelDb.Contains(typeof(TAct)))
        {
            return;
        }

        ActModel act = ModelDb.Act<TAct>();
        foreach (string fieldName in new[] { "_allEncounters", "_allWeakEncounters", "_allRegularEncounters", "_allEliteEncounters", "_allBossEncounters", "_allMonsters" })
        {
            typeof(ActModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(act, null);
        }
    }

    private static System.Reflection.MethodInfo RequireMethod(Type type, string methodName, BindingFlags flags, params Type[] parameterTypes)
    {
        return type.GetMethod(methodName, flags, null, parameterTypes, null)
            ?? throw new InvalidOperationException($"Could not find method {type.FullName}.{methodName}.");
    }
}
