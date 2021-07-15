using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextArea : MonoBehaviour
{
    public ByteLengthEnum ByteLength;
    public enum ByteLengthEnum
    {
        int8 = 255,
        int16 = 65000,
        int32 = 999999999
    };

    public DecimalPointEnum DecimalPoint;
    public enum DecimalPointEnum
    {
        noDP = 1,
        oneDP = 10,
        twoDP = 100
    }

    [Space, Header("Minus numbers?")]
    public bool Signed;

    [HideInInspector]
    public int Value;

    Button LeftIncrement;
    Button RightIncrement;
    TMP_InputField InputField;

    string oldValue;
    int maxValue;

    private void Awake()
    {
        try
        {
            LeftIncrement = transform.Find("Button (Left Increment)").GetComponent<Button>();
            RightIncrement = transform.Find("Button (Right Increment)").GetComponent<Button>();

            InputField = transform.Find("InputField").GetComponent<TMP_InputField>();

            maxValue = Mathf.FloorToInt((int)ByteLength / (int)DecimalPoint);
            maxValue = Signed ? Mathf.FloorToInt(maxValue / 2) : maxValue;
        }
        catch
        {
            Debug.LogError("TextArea.cs on gameObject '" + gameObject.name + "' cannot get child UI components");
        }
    }

    private void FixedUpdate()
    {
        if (InputField.text != oldValue)
        {
            bool isV = float.TryParse(InputField.text, out float temp1);
            if (isV)
            {
                float temp2 = Signed ? Mathf.Clamp(temp1, 0 - maxValue, maxValue) : Mathf.Clamp(temp1, 0, maxValue);
                Value = (int)Mathf.Round(temp2 * (int)DecimalPoint);

                InputField.text = Value.ToString();
                oldValue = InputField.text;
            }
        }
    }
}
