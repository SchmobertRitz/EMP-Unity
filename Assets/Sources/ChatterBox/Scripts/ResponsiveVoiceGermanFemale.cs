//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace EMP.ChatterBox
{
    internal class ResponsiveVoiceGermanFemale : ITextToSpeech
    {
        private readonly ChatterBox chatterBox;
        private readonly SHA256 hasher;
        private IAudioConverter audioConverter = new NAudioAudioConverter();

        internal ResponsiveVoiceGermanFemale()
        {
            this.chatterBox = ChatterBox.Instance;
            hasher = SHA256.Create();
#if UNITY_EDITOR
            Debug.Log("** This TTS implementation uses https://responsivevoice.org/. If you use it for commercial projects, don't forget to pay for it. **");
#endif
        }

        public void CreateAudioFileFromText(string text, Action<string> resultHandler, Action<string> errorHandler)
        {
            chatterBox.StartCoroutine(DownloadAndSaveAudioFile(text, resultHandler, errorHandler));
        }

        private IEnumerator DownloadAndSaveAudioFile(string text, Action<string> resultHandler, Action<string> errorHandler)
        {
            WWW www = new WWW(string.Format(@"https://code.responsivevoice.org/getvoice.php?t={0}&tl=de&sv=&vn=&pitch=0.5&rate=0.5&vol=1", WWW.EscapeURL(text)));
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                errorHandler(www.error);
                yield break;
            }
            string filename = GetCacheFilename(text);
            string fullCacheFileName = Path.Combine(chatterBox.FilesystemCachePath, filename);
            Directory.CreateDirectory(chatterBox.FilesystemCachePath);
            using (FileStream output = File.Open(fullCacheFileName, FileMode.Create))
            using (MemoryStream input = new MemoryStream(www.bytes))
            {
                audioConverter.ConvertToWav(AudioType.MPEG, input, output);
            }
            resultHandler(filename);
        }

        public string GetCacheFilename(string text)
        {
            byte[] hashInput = Encoding.ASCII.GetBytes(GetType().FullName + text);
            string filename = string.Format(@"tts.{0}.wav", BitConverter.ToString(hasher.ComputeHash(hashInput)).Replace("-", ""));
            return filename;
        }
    }
}
