using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textGenerator : MonoBehaviour
{
    public MonoBehaviour mono;
    public string beforeValue;
    public string afterValue;
    public string variable;
    public enum type {Int, Float, String, Uint, Bool};
    public type Type;
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Type == type.Float)
        {
            text.text = beforeValue + ((float)mono.GetType().GetField(variable).GetValue(mono)).ToString() + afterValue;
        }
        if (Type == type.Int)
        {
            text.text = beforeValue + ((int)mono.GetType().GetField(variable).GetValue(mono)).ToString() + afterValue;
        }
        if (Type == type.String)
        {
            text.text = beforeValue + ((string)mono.GetType().GetField(variable).GetValue(mono)).ToString() + afterValue;
        }
        if (Type == type.Uint)
        {
            text.text = beforeValue + ((uint)mono.GetType().GetField(variable).GetValue(mono)).ToString() + afterValue;
        }if (Type == type.Bool)
        {
            text.text = beforeValue + ((bool)mono.GetType().GetField(variable).GetValue(mono)).ToString() + afterValue;
        }
    }
}
