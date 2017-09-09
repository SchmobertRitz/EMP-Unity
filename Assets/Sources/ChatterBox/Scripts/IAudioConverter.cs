//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EMP.ChatterBox
{
    public interface IAudioConverter
    {
        void ConvertToWav(AudioType audioType, Stream inputStream, Stream outputStream);
    }
}
