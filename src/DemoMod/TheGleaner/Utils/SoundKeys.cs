using System.Reflection;

namespace DemoMod.TheGleaner.Utils;

public class SoundKeys {
    public static string FLUTE_1 = "res://TheGleaner/audio/sfx/flute1_stereo.wav";
    public static string FLUTE_2 = "res://TheGleaner/audio/sfx/flute2_stereo.wav";
    public static string FLUTE_3 = "res://TheGleaner/audio/sfx/flute3_stereo.wav";
    public static string FLUTE_4 = "res://TheGleaner/audio/sfx/flute4_stereo.wav";
    public static string HORN_1 = "res://TheGleaner/audio/sfx/horn1_stereo.wav";
    public static string HORN_2 = "res://TheGleaner/audio/sfx/horn2_stereo.wav";
    public static string HORN_3 = "res://TheGleaner/audio/sfx/horn3_stereo.wav";
    public static string HORN_4 = "res://TheGleaner/audio/sfx/horn4_stereo.wav";
    public static string BELL_1_CONCERTO = "res://TheGleaner/audio/sfx/bell1_concerto.wav";
    public static string BELL_2_CONCERTO = "res://TheGleaner/audio/sfx/bell2_concerto.wav";
    public static string BELL_3_CONCERTO = "res://TheGleaner/audio/sfx/bell3_concerto.wav";
    public static string BELL_1_ATTACK = "res://TheGleaner/audio/sfx/bell1_attack.wav";
    public static string BELL_2_ATTACK = "res://TheGleaner/audio/sfx/bell2_attack.wav";
    public static string BELL_3_ATTACK = "res://TheGleaner/audio/sfx/bell3_attack.wav";
    public static string HEART_BEAT = "res://TheGleaner/audio/sfx/heart_beat.wav";
    
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
