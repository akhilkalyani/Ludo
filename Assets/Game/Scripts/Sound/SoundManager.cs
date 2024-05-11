using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource BackgroundMusicSource;
    [SerializeField] private AudioSource GamePlaySource;
    [SerializeField] private AudioClip TokenMoveBeepSoundClip;
    [SerializeField] private AudioClip KillTokenSoundClip;
    [SerializeField] private AudioClip TokenArrivedHomeSoundClip;
    [SerializeField] private AudioClip BackGroundSoundClip;
    [SerializeField] private AudioClip CelebrationSoundClip;
    public static SoundManager instance;
    private void Awake()
    {
        instance = this;
        BackgroundMusicSource.clip = BackGroundSoundClip;
    }
    public void PlayBackGroundMusic()
    {
        BackgroundMusicSource.Play();
    }
    public void PauseBackGroundMusic()
    {
        BackgroundMusicSource.Pause();
    }
    public void PlayTokenBeepSound()
    {
        if(GamePlaySource.clip)
        {
            if(GamePlaySource.clip!=TokenMoveBeepSoundClip)
            {
                GamePlaySource.clip = TokenMoveBeepSoundClip;
            }
        }else
        {
            GamePlaySource.clip = TokenMoveBeepSoundClip;
        }
        GamePlaySource.Play();
    }
    public void PlayKillTokenSound()
    {
        if(GamePlaySource.clip)
        {
            if(!GamePlaySource.clip!=KillTokenSoundClip)
            {
                GamePlaySource.clip = KillTokenSoundClip;
            }
        }else
        {
                GamePlaySource.clip = KillTokenSoundClip;
        }
        GamePlaySource.Play();
    }
    public void PlayTokenArrivedAtHome()
    {
        if (GamePlaySource.clip)
        {
            if (!GamePlaySource.clip != TokenArrivedHomeSoundClip)
            {
                GamePlaySource.clip = TokenArrivedHomeSoundClip;
            }
        }
        else
        {
            GamePlaySource.clip = TokenArrivedHomeSoundClip;
        }
        GamePlaySource.Play();
    }
    public void PlayCelebrationSound()
    {
        if (GamePlaySource.clip)
        {
            if (!GamePlaySource.clip != CelebrationSoundClip)
            {
                GamePlaySource.clip = CelebrationSoundClip;
            }
        }
        else
        {
            GamePlaySource.clip = CelebrationSoundClip;
        }
        GamePlaySource.Play();
    }
}
