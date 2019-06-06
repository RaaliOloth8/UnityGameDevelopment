using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioSource single;
    public AudioClip clip;
    public static SoundManager instance = null;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void PlaySingle()
    {
        single.clip = clip;
        single.Play();
    }
}
