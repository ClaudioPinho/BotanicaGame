using System.Collections.Generic;
using BotanicaGame.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace BotanicaGame.Game;

public class GameAudio
{
    public enum EAudioGroup
    {
        None = 0,
        UI = 1,
        Background = 2,
        Enemy = 3,
        Friendly = 4
    }

    /// <summary>
    /// Get or Set the master volume for every audio group
    /// </summary>
    public static float MasterVolume
    {
        get => _instance?._masterVolume ?? 0;
        set
        {
            if (_instance != null)
            {
                _instance._masterVolume = value;
            }
        }
    }
    
    private static GameAudio _instance;

    public int AudioInstancesCount => _soundEffectInstances.Count;

    private Dictionary<EAudioGroup, float> _groupVolumes;
    private float _masterVolume;

    private List<SoundEffectInstance> _soundEffectInstances;

    public GameAudio(Dictionary<EAudioGroup, float> groupVolumes, float masterVolume)
    {
        _soundEffectInstances = [];
        _groupVolumes = groupVolumes;
        _masterVolume = masterVolume;
        _instance = this;
    }

    /// <summary>
    /// This should only be called at the end of frame in order to clean up sound effect instances that have
    /// already expired or stopped
    /// </summary>
    public void PerformAudioCleanup() =>
        _soundEffectInstances.RemoveAll(x => x.State == SoundState.Stopped || x.IsDisposed);

    /// <summary>
    /// Stops every audio instance from playing, this will mark it for clean up at the end of this frame
    /// </summary>
    public void ForceAllAudioStop() => _soundEffectInstances.ForEach(x => x.Stop(true));
    
    public static bool PlaySoundEffect(SoundEffect soundEffect, EAudioGroup group = EAudioGroup.None,
        AudioEmitter audioEmitter = null, Vector3? position = null)
    {
        if (soundEffect == null) return false;
        if (_instance != null) return _instance.handleSoundPlay(soundEffect, group, audioEmitter, position);
        DebugUtils.PrintError("No 'GameAudio' instance found in order to playback sound!");
        return false;
    }

    private bool handleSoundPlay(SoundEffect soundEffect, EAudioGroup group = EAudioGroup.None,
        AudioEmitter audioEmitter = null, Vector3? position = null)
    {
        var instance = soundEffect.CreateInstance();
        _soundEffectInstances.Add(instance);

        if (audioEmitter != null && position != null)
        {
            // todo: implement 3D audio for the game
            // instance.Apply3D(null, audioEmitter);
        }
        else
        {
            instance.Volume = GetVolumeForGroup(group);
            instance.Play();
        }

        return true;
    }

    private float GetVolumeForGroup(EAudioGroup group)
    {
        if (_groupVolumes.TryGetValue(group, out var groupVolume))
            return groupVolume * _masterVolume;
        DebugUtils.PrintWarning($"Audio group '{group}' not found, using default values...");
        return 1f * _masterVolume;
    }
}