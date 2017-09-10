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

        public string UnityCachePath = "ChatterBoxCache/Resources";
        public string FilesystemCachePath = "ChatterBoxCache";

        private static ChatterBox instance;
        public static ChatterBox Instance
        { get { return instance; } }
        
        private void Awake()
        {
            ChatterBox.instance = this;
        }

        private void OnDestroy()
        {
            ChatterBox.instance = null;
        }

        internal void Prepare(TTSText ttsText, TTS tts, Action<Action<Action>> onPreparedHandler, ECachingMode cachingMode = ECachingMode.CacheInFileSystem)
        {
            if (ChatterBox.Instance == null)
            {
                Debug.LogError("Unable to speak text. ChatterBox not ready.");
                return;
            }

            string cacheName = tts.TextToSpeech.GetCacheFilename(ttsText.SpokenText);

            if (cachingMode == ECachingMode.CacheInUnity)
            {
                string resourceName = cacheName.Substring(0, cacheName.Length - Path.GetExtension(cacheName).Length);
                AudioClip audioClip = Resources.Load<AudioClip>(resourceName);
                if (audioClip != null)
                {
                    onPreparedHandler(onTssEndedAction => tts.PlayAudioClip(audioClip, ttsText, onTssEndedAction));
                    return;
                }
            }

            if (cachingMode != ECachingMode.NoCaching)
            {
                string fileSystemName = Path.Combine(FilesystemCachePath, cacheName);
                if (File.Exists(fileSystemName))
                {
                    PrepareAudioClip(
                        cacheName,
                        cachingMode,
                        audioClip => onPreparedHandler(onTssEndedAction => tts.PlayAudioClip(audioClip, ttsText, onTssEndedAction))
                     );
                    return;
                }
            }

            // no cache hit. Needs to be generated.
            tts.TextToSpeech.CreateAudioFileFromText(
                ttsText.SpokenText,
                filename => PrepareAudioClip(
                    filename,
                    cachingMode,
                    audioClip => onPreparedHandler(onTssEndedAction => tts.PlayAudioClip(audioClip, ttsText, onTssEndedAction))
                ),
                error => Debug.LogError(error)
            );
        }

        private void PrepareAudioClip(string filename, ECachingMode cachingMode, Action<AudioClip> onPreparedHandler)
        {
            StartCoroutine(
                LoadAudioClip(
                    filename,
                    cachingMode,
                    onPreparedHandler
                )
            );
        }

        private IEnumerator LoadAudioClip(string filename, ECachingMode cachingMode, Action<AudioClip> resultHandler)
        {
            string fullFilePath = Path.Combine(FilesystemCachePath, filename);
            string url = string.Format(@"file:///{0}", Path.GetDirectoryName(Application.dataPath) + "/" + fullFilePath.Replace(@"\", @"/"));
            WWW www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
            }
            resultHandler(www.GetAudioClip(false, false, AudioType.WAV));
            if (cachingMode == ECachingMode.NoCaching)
            {
                File.Delete(fullFilePath);
            }
            else if (cachingMode == ECachingMode.CacheInUnity && Application.isEditor)
            {
                string fullUnityPath = GetFullUnityCachePath();
                string fullUnityFilePath = Path.Combine(fullUnityPath, filename);
                if (!File.Exists(fullUnityFilePath))
                {
                    Directory.CreateDirectory(fullUnityPath);
                    File.Move(fullFilePath, Path.Combine(fullUnityPath, filename));
                }
                else
                {
                    Debug.LogWarning("Tried to duplicate TTS cache. Seems that the Unity TTS cache is not yet imported. Try refreshing the project view.");
                }
            }
        }
        
        private string GetFullUnityCachePath()
        {
            return Path.Combine("Assets", UnityCachePath);
        }
    }
}
