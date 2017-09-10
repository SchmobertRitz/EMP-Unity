//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Forms;
using UnityEngine;

namespace EMP.ChatterBox.Test
{
    public class TestUi : MonoBehaviour
    {
        private Form form;
        private TextField txtText;

        private void Say(Button obj)
        {
            new TTS
            {
                AudioSource = GetComponent<AudioSource>(),
                TextToSpeech = new ResponsiveVoiceGermanFemale()
            }.Say(txtText.Text, null, ChatterBox.ECachingMode.CacheInUnity);
        }

        private Form GetForm()
        {
            if (form == null)
            {
                form = new Form();
                form.Add(txtText = new TextField());
                form.Add(new Button("Say", Say));
            }
            return form;
        }

        private void OnGUI()
        {
            GetForm().OnGUI(new Rect(0, 0, Screen.width, Screen.height));
        }
    }
}
