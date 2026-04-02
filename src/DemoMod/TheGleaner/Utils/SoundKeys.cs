using System.Reflection;

namespace DemoMod.TheGleaner.Utils;

public class SoundKeys {
    public static string FLUTE_1 = "res://TheGleaner/audio/sfx/flute1.wav";
    public static string FLUTE_2 = "res://TheGleaner/audio/sfx/flute2.wav";
    public static string FLUTE_3 = "res://TheGleaner/audio/sfx/flute3.wav";
    public static string FLUTE_4 = "res://TheGleaner/audio/sfx/flute4.wav";
    public static string HORN_1 = "res://TheGleaner/audio/sfx/horn1.wav";
    public static string HORN_2 = "res://TheGleaner/audio/sfx/horn2.wav";
    public static string HORN_3 = "res://TheGleaner/audio/sfx/horn3.wav";
    public static string HORN_4 = "res://TheGleaner/audio/sfx/horn4.wav";
    
    private static Dictionary<string, string> soundKeyMap = new Dictionary<string, string>();

    public static void Initialize() {
        foreach (FieldInfo field in typeof(SoundKeys).GetFields(BindingFlags.Static | BindingFlags.Public)) {
            soundKeyMap.Add(field.Name, field.GetValue(null).ToString());
        }
    }

    public static string GetSoundResourcePath(string soundKey) {
        return soundKeyMap[soundKey];
    }
}
