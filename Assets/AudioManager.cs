using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class PlayableClip
{

    public string Name;
    public AudioClip AudioClip;

}

public class AudioManager : MonoBehaviour
{
    public AudioSource layer0;

    public AudioSource layer1;

    public PlayableClip menuClip;

    public List<PlayableClip> sfxClips = new List<PlayableClip>();




    void Start()
    {

        //Event subscriptions
        BrainControl.Get().eventManager.e_gameInitialised.AddListener(() =>
        {
            SetMusic(menuClip.AudioClip);
        });

        //Event subscriptions
        BrainControl.Get().eventManager.e_levelLoaded.AddListener((l) =>
        {
            SetMusic(l.data.music.AudioClip);
        });

        BrainControl.Get().eventManager.e_getVowel.AddListener((c) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_getConsonant.AddListener((c) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_fillRack.AddListener((i, c) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_newRack.AddListener((i, c) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));

        BrainControl.Get().eventManager.e_beginInput.AddListener((b) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "blockPlaced").AudioClip));
        BrainControl.Get().eventManager.e_updateInput.AddListener((b) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "blockPlaced").AudioClip));

        BrainControl.Get().eventManager.e_pathComplete.AddListener(() => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "pathComplete").AudioClip));

        BrainControl.Get().eventManager.e_validateFail.AddListener(() => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "validateFail").AudioClip));
        BrainControl.Get().eventManager.e_validateSuccess.AddListener((i) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "validateSuccess").AudioClip));

        SetMusic(menuClip.AudioClip);

    }

    public void SetMusic(AudioClip _music)
    {

        if (layer0.clip == _music) return;

        LeanTween.value(1, 0, 1.5f).setOnUpdate((v) => layer0.volume = v).setOnComplete(() =>
        {
            layer0.Stop();
            layer0.clip = _music;
            layer0.loop = true;

            layer0.Play();
            LeanTween.value(0, 1, 1.5f).setOnUpdate((v) => layer0.volume = v);
        });
    }

    public void PlaySFX(AudioClip _sfx)
    {
        layer1.PlayOneShot(_sfx);
    }
}
