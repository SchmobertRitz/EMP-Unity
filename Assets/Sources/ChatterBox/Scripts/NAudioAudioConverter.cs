//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using NAudio.Wave;
using System.IO;
using UnityEngine;

namespace EMP.ChatterBox
{
    public class NAudioAudioConverter : IAudioConverter
    {
        public void ConvertToWav(AudioType audioType, Stream inputStream, Stream outputStream)
        {
            switch(audioType)
            {
                case AudioType.MPEG:
                    using (Mp3FileReader reader = new Mp3FileReader(inputStream))
                    {
                        WaveFileWriter.WriteWavFileToStream(outputStream, reader);
                    }
                    break;
                default:
                    Debug.LogError("This audio converter does not support the audio type " + audioType.ToString());
                    break;
            }
        }
    }
}
