namespace DemoMod.TheGleaner.Utils;

public static class HitSfxGroup {
    private static string[][] groups = [
        ["c5", "g4", "c5", "d5", "e5", "g5", "c6", "d6", "e6"],
        ["c5", "g5", "b5", "g5", "c6", "g5", "d6", "g5", "e6"]
    ];

    private static List<string> pool = [];
    private static int currentTrack = -1;

    public static string nextNote(string instrument) {
        return nextGroup(instrument, 1)[0];
    }
    
    public static List<string> nextGroup(string instrument, int notes) {
        List<string> ret = [];
        for (int i = 0; i < notes; i++) {
            refreshPool();
            ret.Add(concatAudioPath(instrument, pool[0]));
            pool.RemoveAt(0);
        }
        return ret;
    }

    private static void refreshPool() {
        if (pool.Count == 0) {
            currentTrack++;
            if (currentTrack >= groups.Length) {
                currentTrack = 0;
            }
            pool.AddRange(groups[currentTrack]);
        }
    }

    private static string concatAudioPath(string instrument, string note) {
        return "res://TheGleaner/audio/sfx/instruments/" + instrument + "/" + note + ".wav";
    }
}
