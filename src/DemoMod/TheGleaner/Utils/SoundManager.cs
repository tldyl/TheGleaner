using Godot;
using MegaCrit.Sts2.Core.Saves;

namespace DemoMod.TheGleaner.Utils;

public partial class SoundManager : Node {
    // 单例实例
    public static SoundManager Instance { get; private set; }

    // 音效池，避免频繁创建/销毁
    private List<AudioStreamPlayer> soundPool = new List<AudioStreamPlayer>();
    private const int POOL_SIZE = 40;

    public override void _Ready() {
        // 单例初始化
        if (Instance == null) {
            Instance = this;
            InitializeSoundPool();
        } else {
            QueueFree(); // 如果已存在，则销毁新实例
        }
    }

    // 初始化音效池
    private void InitializeSoundPool() {
        for (int i = 0; i < POOL_SIZE; i++) {
            AudioStreamPlayer player = new AudioStreamPlayer();
            player.Name = $"SoundPlayer_{i}";
            AddChild(player);
            soundPool.Add(player);
        }
    }

    // 播放音效（从池中获取可用播放器）
    public void PlaySound(string audioPath, float volume = 0.5f, float pitch = 1.0f) {
        AudioStreamPlayer availablePlayer = soundPool.Find(p => !p.Playing);

        if (availablePlayer != null) {
            // 加载并播放音频
            AudioStream audio = GD.Load<AudioStream>(audioPath);
            if (audio != null) {
                availablePlayer.Stream = audio;
                availablePlayer.VolumeLinear = volume * SaveManager.Instance.SettingsSave.VolumeSfx;
                availablePlayer.PitchScale = pitch;
                availablePlayer.Play();
            }
        }
    }
    
    public int PlaySoundLoop(string audioPath, float volume = 0.5f, float pitch = 1.0f) {
        AudioStreamPlayer availablePlayer = soundPool.Find(p => !p.Playing);

        if (availablePlayer != null) {
            AudioStream audio = GD.Load<AudioStream>(audioPath);
            if (audio != null) {
                switch (audio) {
                    case AudioStreamWav streamWav:
                        streamWav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
                        break;
                    case AudioStreamOggVorbis oggVorbis:
                        oggVorbis.Loop = true;
                        break;
                }
                availablePlayer.Stream = audio;
                availablePlayer.VolumeLinear = volume * SaveManager.Instance.SettingsSave.VolumeSfx;
                availablePlayer.PitchScale = pitch;
                availablePlayer.Play();
            }
        }
        
        return soundPool.IndexOf(availablePlayer);
    }

    public void StopLoopSound(int playerIndex) {
        soundPool[playerIndex].Stop();
    }
    
    // 播放音效（带位置，2D游戏）
    public void PlaySoundAtPosition(string audioPath, Vector2 position, float volume = 0.5f) {
        AudioStreamPlayer2D player = new AudioStreamPlayer2D();
        player.Name = "PositionalSound";
        AddChild(player);

        AudioStream audio = GD.Load<AudioStream>(audioPath);
        if (audio != null) {
            player.Stream = audio;
            player.VolumeLinear = volume * SaveManager.Instance.SettingsSave.VolumeSfx;
            player.GlobalPosition = position;
            player.Play();

            // 播放完成后自动清理
            player.Connect("finished", Callable.From(new Action<AudioStreamPlayer2D>(OnPositionalSoundFinished)));
        }
    }

    private void OnPositionalSoundFinished(AudioStreamPlayer2D player) {
        player.QueueFree();
    }
    
    // 停止所有音效
    public void StopAllSounds() {
        foreach (var player in soundPool) {
            if (player.Playing) {
                player.Stop();
            }
        }
    }
}
