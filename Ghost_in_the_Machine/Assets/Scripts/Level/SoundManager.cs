using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip Hit, swordSlash, Jump, Dash, Phase, Death;
    static AudioSource audioSrc;


    // Use this for initialization
    void Start()
    {

        Hit = Resources.Load<AudioClip>("Hit");
        swordSlash = Resources.Load<AudioClip>("swordSlash");
        Jump = Resources.Load<AudioClip>("Jump");
        Dash = Resources.Load<AudioClip>("Dash");
        Phase = Resources.Load<AudioClip>("Phase");
        Death = Resources.Load<AudioClip>("Death");


        audioSrc = GetComponent<AudioSource>();
    }

    public void PlaySound(string clip)
    {
        switch (clip)
        {
            case "hit":
                audioSrc.PlayOneShot(Hit);
                break;
            case "attack":
                audioSrc.PlayOneShot(swordSlash);
                break;
            case "jump":
                audioSrc.PlayOneShot(Jump);
                break;
            case "dash":
                audioSrc.PlayOneShot(Dash);
                break;
            case "phase":
                audioSrc.PlayOneShot(Phase);
                break;
            case "death":
                audioSrc.PlayOneShot(Death);
                break;
        }
    }
}
