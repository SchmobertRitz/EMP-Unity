//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace EMP.ChatterBox
{
    public class ChatterBox : MonoBehaviour
    {
        public enum ECachingMode
        {
            NoCaching, CacheInFileSystem, CacheInUnity
        }

        public AudioSource DefaultAudioSource
        { get; set; }

        public string UnityCachePath = "ChatterBoxCache/Resources";
        public string FilesystemCachePath = "ChatterBoxCache";

        private static ChatterBox instance;
        public static ChatterBox Instance
        { get { return instance; } }

        ITextToSpeech tts;

        private void Awake()
        {
            ChatterBox.instance = this;
            tts = new ResponsiveVoiceGermanFemale(this);
        }

        private void OnDestroy()
        {
            ChatterBox.instance = null;
        }

        public void Say(string text, AudioSource audioSource = null, ECachingMode cachingMode = ECachingMode.CacheInFileSystem)
        {
            if (ChatterBox.Instance == null)
            {
                Debug.LogError("Unable to speak text. ChatterBox not ready.");
                return;
            }

            string cacheName = tts.GetCacheFilename(text);

            if (cachingMode == ECachingMode.CacheInUnity) {
                string resourceName = cacheName.Substring(0, cacheName.Length - Path.GetExtension(cacheName).Length);
                AudioClip audioClip = Resources.Load<AudioClip>(resourceName);
                if (audioClip != null)
                {
                    PlayAudioClip(audioClip, audioSource);
                    return;
                }
            }

            if (cachingMode != ECachingMode.NoCaching)
            {
                string fileSystemName = Path.Combine(FilesystemCachePath, cacheName);
                if (File.Exists(fileSystemName))
                {
                    LoadAndPlayAudioClip(cacheName, cachingMode, audioSource);
                    return;
                }
            }

            // no cache hit. Needs to be generated.
            tts.CreateAudioFileFromText(
                text,
                filename => LoadAndPlayAudioClip(filename, cachingMode, audioSource),
                error => Debug.LogError(error)
            );
        }

        private void LoadAndPlayAudioClip(string filename, ECachingMode cachingMode, AudioSource audioSource)
        {
            StartCoroutine(
                LoadAudioClip(
                    filename,
                    cachingMode,
                    audioClip => PlayAudioClip(audioClip, audioSource)
                )
            );
        }

        private IEnumerator LoadAudioClip(string filename, ECachingMode cachingMode, Action<AudioClip> resultHandler)
        {
            string fullFilePath = Path.Combine(FilesystemCachePath, filename);
            WWW www = new WWW(string.Format(@"file://{0}", fullFilePath));
            yield return www;
            resultHandler(www.GetAudioClip(false, false, AudioType.WAV));
            if (cachingMode == ECachingMode.NoCaching)
            {
                File.Delete(fullFilePath);
            } else if (cachingMode == ECachingMode.CacheInUnity && Application.isEditor)
            {
                string fullUnityPath = GetFullUnityCachePath();
                string fullUnityFilePath = Path.Combine(fullUnityPath, filename);
                if (!File.Exists(fullUnityFilePath))
                {
                    Directory.CreateDirectory(fullUnityPath);
                    File.Move(fullFilePath, Path.Combine(fullUnityPath, filename));
                } else
                {
                    Debug.LogWarning("Tried to duplicate TTS cache. Seems that the Unity TTS cache is not yet imported. Try refreshing the project view.");
                }
            }
        }

        private void PlayAudioClip(AudioClip audioClip, AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioClip);
            }
            else if (DefaultAudioSource != null)
            {
                DefaultAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                Debug.LogWarning("Unable to play tts. No AudioSource given and no default AudioSource set.");
            }
        }

        private string GetFullUnityCachePath()
        {
            return Path.Combine("Assets", UnityCachePath);
        }
    }
}
