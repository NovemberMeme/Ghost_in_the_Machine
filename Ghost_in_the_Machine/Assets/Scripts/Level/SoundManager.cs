using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip 
        SwordSwing, 
        BlockHit, 
        Parry, 
        Damaged1, 
        Damaged2, 
        Damaged3, 
        DamagedBoneBreak, 
        Landing, 
        Footstep, 
        PhaseShift, 
        TimeLapse,
        ChurchOfScience,
        BossBattle;

    static AudioSource swordSwingAudioSource;

    // Use this for initialization
    void Start()
    {
        SwordSwing = Resources.Load<AudioClip>("SwordSwing");
        BlockHit = Resources.Load<AudioClip>("BlockHit");
        Parry = Resources.Load<AudioClip>("Parry");
        Damaged1 = Resources.Load<AudioClip>("Damaged1");
        Damaged2 = Resources.Load<AudioClip>("Damaged2");
        Damaged3 = Resources.Load<AudioClip>("Damaged3");
        DamagedBoneBreak = Resources.Load<AudioClip>("DamagedBoneBreak");
        Landing = Resources.Load<AudioClip>("Landing");
        Footstep = Resources.Load<AudioClip>("Footstep");
        PhaseShift = Resources.Load<AudioClip>("PhaseShift");
        TimeLapse = Resources.Load<AudioClip>("TimeLapse");
        ChurchOfScience = Resources.Load<AudioClip>("ChurchOfScience");
        BossBattle = Resources.Load<AudioClip>("BossBattle");

        swordSwingAudioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip, string name)
    {
        swordSwingAudioSource.Stop();

        switch (clip)
        {
            case "SwordSwing":
                swordSwingAudioSource.PlayOneShot(SwordSwing);
                break;
            case "BlockHit":
                swordSwingAudioSource.PlayOneShot(BlockHit);
                break;
            case "Parry":
                swordSwingAudioSource.PlayOneShot(Parry);
                break;
            case "Damaged1":
                swordSwingAudioSource.PlayOneShot(Damaged1);
                break;
            case "Damaged2":
                swordSwingAudioSource.PlayOneShot(Damaged2);
                break;
            case "Damaged3":
                swordSwingAudioSource.PlayOneShot(Damaged3);
                break;
            case "DamagedBoneBreak":
                swordSwingAudioSource.PlayOneShot(DamagedBoneBreak);
                break;
            case "Landing":
                swordSwingAudioSource.PlayOneShot(Landing);
                break;
            case "Footstep":
                swordSwingAudioSource.PlayOneShot(Footstep);
                break;
            case "PhaseShift":
                swordSwingAudioSource.PlayOneShot(PhaseShift);
                break;
            case "TimeLapse":
                swordSwingAudioSource.PlayOneShot(TimeLapse);
                break;
            case "ChurchOfScience":
                swordSwingAudioSource.PlayOneShot(ChurchOfScience);
                break;
            case "BossBattle":
                swordSwingAudioSource.PlayOneShot(BossBattle);
                break;
        }

        Debug.Log(name + ": " + clip);
    }
}
