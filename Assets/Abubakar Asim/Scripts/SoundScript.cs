using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundScript : MonoBehaviour
{

    [SerializeField] private AudioSource MainMenuMusic;

    private void OnEnable()
    {
        MainMenuMusic.Play();

    }
    private void OnDisable()
    {
        MainMenuMusic.Stop();
    }

}
