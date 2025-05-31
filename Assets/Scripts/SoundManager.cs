using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum SoundType
{
    ROTATE,
    WIN
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    private static SoundManager Instance;
    private AudioSource audioSource;
    private System.Random random;

    [SerializeField]
    private NamedSound[] sounds;

    [ContextMenu("Generate Sound List")]
    private void GenerateNamedSounds()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Dictionary<string, NamedSound> soundOf = sounds.ToDictionary(item => item.name);

        sounds = new NamedSound[names.Length];


        sounds = names.Select(
                    name => soundOf.TryGetValue(name, out NamedSound sound) 
                            ? sound : new NamedSound() { name = name })
                    .ToArray();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            audioSource = GetComponent<AudioSource>();
            Instance = this;
            random = new System.Random();
        }
    }

    internal static void PlaySound(SoundType soundType, float volume = 1, int soundIndex = -1)
    {
        NamedSound namedSound = Instance.sounds[(int)soundType];
        if (soundIndex < 0 || namedSound.sounds.Length <= soundIndex) {
            soundIndex = Instance.random.Next(namedSound.sounds.Length);
        }
        Instance.audioSource.PlayOneShot(namedSound.sounds[soundIndex], volume);
    }

}

[Serializable]
public struct NamedSound
{
    [HideInInspector] public string name;
    public AudioClip[] sounds;
}
