//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using UnityEngine;

namespace EMP.ChatterBox
{
    public interface ITextToSpeech
    {
        void CreateAudioFileFromText(string text, Action<string> resultHandler, Action<string> errorHandler);
        string GetCacheFilename(string text);
    }
}
