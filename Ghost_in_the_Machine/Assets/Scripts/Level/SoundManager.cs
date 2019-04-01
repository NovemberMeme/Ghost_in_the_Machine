using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static AudioClip
        SwordSwingSound,
        BlockHitSound,
        ParrySound,
        DamagedSound1,
        DamagedSound2,
        DamagedSound3,
        DamagedBoneBreakSound,
        LandingSound,
        FootstepSound,
        PhaseShiftSound,
        TimeLapseSound,
        ChurchOfScienceSound,
        BossBattleSound;

    static AudioSource audioSource1;
    static AudioSource audioSource2;

    public void Start()
    {
        SwordSwingSound = Resources.Load<AudioClip>("SwordSwing");
        BlockHitSound = Resources.Load<AudioClip>("BlockHit");
        ParrySound = Resources.Load<AudioClip>("Parry");
        DamagedSound1 = Resources.Load<AudioClip>("Damaged1");
        DamagedSound2 = Resources.Load<AudioClip>("Damaged2");
        DamagedSound3 = Resources.Load<AudioClip>("Damaged3");
        DamagedBoneBreakSound = Resources.Load<AudioClip>("DamagedBoneBreak");
        LandingSound = Resources.Load<AudioClip>("Landing");
        FootstepSound = Resources.Load<AudioClip>("Footstep");
        PhaseShiftSound = Resources.Load<AudioClip>("PhaseShift");
        TimeLapseSound = Resources.Load<AudioClip>("TimeLapse");
        ChurchOfScienceSound = Resources.Load<AudioClip>("ChurchOfScience");
        BossBattleSound = Resources.Load<AudioClip>("BossBattle");

        audioSource1 = transform.GetChild(0).GetComponent<AudioSource>();
        audioSource2 = transform.GetChild(1).GetComponent<AudioSource>();

        //PlayMusic("ChurchOfScienceSound");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayMusic("BossBattle");
        }
    }

    public static void PlayMusic(string clip)
    {
        switch (clip)
        {
            case "ChurchOfScience":
                audioSource2.PlayOneShot(ChurchOfScienceSound);
                break;
            case "BossBattle":
                audioSource2.PlayOneShot(BossBattleSound);
                break;
        }
    }

    public static void PlaySound(string clip, string name)
    {
        //audioSource1.Stop();

        switch (clip)
        {
            case "SwordSwing":
                audioSource1.PlayOneShot(SwordSwingSound);
                break;
            case "BlockHit":
                audioSource1.PlayOneShot(BlockHitSound);
                break;
            case "Parry":
                audioSource1.PlayOneShot(ParrySound);
                break;
            case "Damaged1":
                audioSource1.PlayOneShot(DamagedSound1);
                break;
            case "Damaged2":
                audioSource1.PlayOneShot(DamagedSound2);
                break;
            case "Damaged3":
                audioSource1.PlayOneShot(DamagedSound3);
                break;
            case "DamagedBoneBreak":
                audioSource1.PlayOneShot(DamagedBoneBreakSound);
                break;
            case "Landing":
                audioSource1.PlayOneShot(LandingSound);
                break;
            case "Footstep":
                audioSource1.PlayOneShot(FootstepSound);
                break;
            case "PhaseShift":
                audioSource1.PlayOneShot(PhaseShiftSound);
                break;
            case "TimeLapse":
                audioSource1.PlayOneShot(TimeLapseSound);
                break;
            case "ChurchOfScience":
                audioSource1.PlayOneShot(ChurchOfScienceSound);
                break;
            case "BossBattle":
                audioSource1.PlayOneShot(BossBattleSound);
                break;
        }

        Debug.Log(name + ": " + clip);
    }

    public static void PlaySound(string clip)
    {
        //audioSource1.Stop();

        switch (clip)
        {
            case "SwordSwing":
                audioSource1.PlayOneShot(SwordSwingSound);
                break;
            case "BlockHit":
                audioSource1.PlayOneShot(BlockHitSound);
                break;
            case "Parry":
                audioSource1.PlayOneShot(ParrySound);
                break;
            case "Damaged1":
                audioSource1.PlayOneShot(DamagedSound1);
                break;
            case "Damaged2":
                audioSource1.PlayOneShot(DamagedSound2);
                break;
            case "Damaged3":
                audioSource1.PlayOneShot(DamagedSound3);
                break;
            case "DamagedBoneBreak":
                audioSource1.PlayOneShot(DamagedBoneBreakSound);
                break;
            case "Landing":
                audioSource1.PlayOneShot(LandingSound);
                break;
            case "Footstep":
                audioSource1.PlayOneShot(FootstepSound);
                break;
            case "PhaseShift":
                audioSource1.PlayOneShot(PhaseShiftSound);
                break;
            case "TimeLapse":
                audioSource1.PlayOneShot(TimeLapseSound);
                break;
            case "ChurchOfScience":
                audioSource1.PlayOneShot(ChurchOfScienceSound);
                break;
            case "BossBattle":
                audioSource1.PlayOneShot(BossBattleSound);
                break;
        }
    }
}
