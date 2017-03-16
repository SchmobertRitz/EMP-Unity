using UnityEngine;
using System.Collections;

public class FloatModel : MonoBehaviour
{
    public string name;

    [SerializeField]
    private float value;

    public delegate void OnFloatModelValueChanged(FloatModel model);
    public OnFloatModelValueChanged onFloatModelValueChangedHandler;

    public void SetValue(float value)
    {
        this.value = Mathf.Max(0, Mathf.Min(1, value));
        onFloatModelValueChangedHandler(this);
    }

    public float GetValue()
    {
        return value;
    }
}
