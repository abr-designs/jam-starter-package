using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sounds;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Audio
{
    public class SFXManager : MonoBehaviour, ISetVolume
    {
        //============================================================================================================//
        [Serializable]
        private class SfxData
        {
            [SerializeField] internal string name;
            public SFX type;
            [SerializeField]
            private AudioClip[] audioClips;

            [Range(0f, 1f)]
            public float volume;

            [SerializeField, Min(0)]
            public int maxPlaying;

            public AudioClip GetRandomAudioClip()
            {
                return audioClips.Length == 1 ? audioClips[0] : audioClips[Random.Range(0, audioClips.Length)];
            }
        }

        //============================================================================================================//

        [SerializeField]
        private AudioMixerGroup sfxAudioMixer;
        [SerializeField]
        private AudioSource sfxAudioSource;

        [SerializeField]
        private AudioSource sfxSourcePrefab;
        private List<AudioSource> _audioSources;
        internal static SFXManager Instance;

        [SerializeField] private SfxData[] sfxDatas;

        private Dictionary<SFX, SfxData> _sfxDataDictionary;
        private Dictionary<SFX, int> _sfxAntiSpam;

        //Unity Functions
        //============================================================================================================//

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Assert.IsNotNull(sfxAudioMixer);
            
        }

        // Start is called before the first frame update
        private void Start()
        {
            InitVfxLibrary();

            _sfxAntiSpam = new Dictionary<SFX, int>();
        }

        //============================================================================================================//

        private void InitVfxLibrary()
        {
            var count = sfxDatas.Length;
            _sfxDataDictionary = new Dictionary<SFX, SfxData>(count);
            for (var i = 0; i < count; i++)
            {
                var vfxData = sfxDatas[i];
                _sfxDataDictionary.Add(vfxData.type, vfxData);
            }
            //------------------------------------------------//

            _audioSources = new List<AudioSource>();
        }

        //============================================================================================================//

        internal void PlaySound(SFX sfx, float volume = 1f)
        {
            var sfxData = GetSFXData(sfx);

            var hasAntiSpam = _sfxAntiSpam.TryGetValue(sfx, out var count);
            if (sfxData.maxPlaying > 0 && hasAntiSpam && count > sfxData.maxPlaying)
                return;
            
            if(hasAntiSpam == false)
                _sfxAntiSpam.Add(sfx, 0);
            
            
            var audioClip = sfxData.GetRandomAudioClip();

            Assert.IsNotNull(sfxData);
            Assert.IsNotNull(audioClip);

            sfxAudioSource.PlayOneShot(audioClip, volume);
            //FIXME This should just use a loop, and not a coroutine
            StartCoroutine(DequeueSFXCoroutine(sfx, audioClip.length));
        }
        //This is meant to be called via the VFXExtensions class
        internal void PlaySoundAtLocation(SFX vfx, Vector3 worldPosition)
        {
            var sfxData = GetSFXData(vfx);

            var audioSource = TryGet3DAudioSource();
            audioSource.transform.position = worldPosition;


            var audioClip = sfxData.GetRandomAudioClip();
            audioSource.clip = audioClip;
            audioSource.volume = sfxData.volume;
            audioSource.Play();

            StartCoroutine(WaitForSoundFinishCoroutine(audioSource, audioClip.length));
        }

        //============================================================================================================//

        private SfxData GetSFXData(SFX sfx)
        {
            if (_sfxDataDictionary.TryGetValue(sfx, out var vfxData) == false)
                return null;

            Assert.IsNotNull(vfxData);

            return vfxData;
        }

        private AudioSource TryGet3DAudioSource()
        {
            AudioSource audioSource = null;
            if (_audioSources.Count > 0)
                audioSource = _audioSources.FirstOrDefault(x => x.gameObject.activeSelf == false);

            if (audioSource != null)
            {
                audioSource.gameObject.SetActive(true);
                return audioSource;
            }

            audioSource = Instantiate(sfxSourcePrefab, transform);
            audioSource.name = $"[{_audioSources.Count}]_SFX_AudioSource";
            _audioSources.Add(audioSource);

            return audioSource;
        }

        private static IEnumerator WaitForSoundFinishCoroutine(AudioSource targetAudioSource, float time)
        {
            yield return new WaitForSeconds(time);

            targetAudioSource.gameObject.SetActive(false);
            targetAudioSource.Stop();
        }
        
        private IEnumerator DequeueSFXCoroutine(SFX sfx, float time)
        {
            _sfxAntiSpam[sfx]++;
            
            yield return new WaitForSeconds(time);

            _sfxAntiSpam[sfx]--;
        }

        //Set Volume
        //============================================================================================================//
        
        //Based on: https://johnleonardfrench.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/
        public void SetVolume(float volume)
        {
            var v = Mathf.Log10(volume) * 20;
            Instance.sfxAudioMixer.audioMixer.SetFloat(ISetVolume.VOLUME_ID, v);
        }

        //Unity Editor Functions
        //============================================================================================================//

#if UNITY_EDITOR

        private void OnValidate()
        {
            for (int i = 0; i < sfxDatas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(sfxDatas[i].name))
                    sfxDatas[i].name = sfxDatas[i].type.ToString();
            }

            UnityEditor.EditorUtility.SetDirty(this);

            var enumTypes = (SFX[])Enum.GetValues(typeof(SFX));

            foreach (var enumType in enumTypes)
            {
                Assert.IsTrue(sfxDatas.Count(x => x.type == enumType) <= 1,
                    $"<b><color=\"red\">ERROR</color></b>\nMore than 1 SFX found in the SFX manager for {enumType}. <color=\"red\"><b>CAN ONLY HAVE 1</b></color>");
            }

        }

#endif


        //============================================================================================================//



    }
}
