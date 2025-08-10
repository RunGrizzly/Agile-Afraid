using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

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

    public PlayableClip m_winMusic = null;
    
    public PlayableClip m_failMusic = null;

    public List<PlayableClip> sfxClips = new List<PlayableClip>();

    [SerializeField]
    private float m_musicFadeOutDuration = 1.5f;

    [SerializeField]
    private float m_musicFadeInDuration = 1.5f;

    void Start()
    {

        //Event subscriptions
        BrainControl.Get().eventManager.e_gameInitialised.AddListener(() =>
        {
            SetMusic(menuClip.AudioClip);
        });
        
        BrainControl.Get().eventManager.e_levelLoaded.AddListener((l) =>
        {
            if (l != null && l.Data.music !=null)
            {
                SetMusic(l.Data.music.AudioClip);   
            }
        });
        
        BrainControl.Get().eventManager.e_failRun.AddListener(() =>
        {
                SetMusic(m_failMusic.AudioClip);   
        });
        
        BrainControl.Get().eventManager.e_winRun.AddListener(() =>
        {
            SetMusic(m_winMusic.AudioClip);   
        });

        BrainControl.Get().eventManager.e_getVowelRequest.AddListener((costsScore,costsTime) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_getConsonantRequest.AddListener((costsScore, costsTime) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_fillRackRequest.AddListener((fillTo, costsScore, costsTime) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));
        BrainControl.Get().eventManager.e_newRackRequest.AddListener((seedChars,fillTo, costsScore,costsTime) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "tileAction").AudioClip));

        BrainControl.Get().eventManager.e_beginInput.AddListener((block) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "blockPlaced").AudioClip));
        BrainControl.Get().eventManager.e_updateInput.AddListener((block) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "blockPlaced").AudioClip));

        BrainControl.Get().eventManager.e_levelSuccess.AddListener((level) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "levelSuccess").AudioClip, true));
        BrainControl.Get().eventManager.e_levelFail.AddListener((level) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "levelFail").AudioClip, true));

        
        BrainControl.Get().eventManager.e_validateFail.AddListener(() => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "validateFail").AudioClip));
        BrainControl.Get().eventManager.e_validateSuccess.AddListener((blockInput) => PlaySFX(sfxClips.FirstOrDefault(x => x.Name == "validateSuccess").AudioClip));

        SetMusic(menuClip.AudioClip);

    }

    public void SetMusic(AudioClip _music, bool freshStart = false)
    {
        //Should I restart same music?
        if (layer0.clip == _music && !freshStart)
        {
            return;
        }

        LeanTween.value(1, 0, m_musicFadeOutDuration).setOnUpdate((v) => layer0.volume = v).setOnComplete(() =>
        {
            layer0.Stop();
            layer0.clip = _music;
            layer0.loop = true;

            layer0.Play();
            LeanTween.value(0, 1, m_musicFadeInDuration/2).setOnUpdate((v) => layer0.volume = v);
        });
    }

    public void PlaySFX(AudioClip _sfx, bool bulldoze = false)
    {
        if (bulldoze)
        {
            layer1.Stop();
        }
        
        layer1.PlayOneShot(_sfx);
    }
}
