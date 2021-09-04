using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TextArea : MonoBehaviour
{
    public delegate void ValueChangedCallback(float value);
    public ValueChangedCallback ValueChanged;

    TMP_InputField InputField;

    string oldValue;

    private void Awake()
    {
        try
        {
            InputField = GetComponent<TMP_InputField>();
            InputField.onEndEdit.AddListener(CheckValues);
        }
        catch
        {
            Debug.LogError("TextArea.cs on gameObject '" + gameObject.name + "' cannot get child UI components");
        }
    }

    void CheckValues(string value)
    {
        bool isV = float.TryParse(InputField.text, out float temp1);
        if (isV)
        {
            ValueChanged(temp1);
            oldValue = InputField.text;
        }
        else
        {
            InputField.text = "Invalid";
        }
    }

    public void InitializeArea(float startingValue = 0)
    {
        InputField.text = startingValue.ToString();
        oldValue = InputField.text;
    }
}
