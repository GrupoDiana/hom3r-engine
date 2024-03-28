using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectStateMaterialUtils{

    public enum TMaterialState { Visible, Transparent, Hide };       // Use to indicate the new material state
    public enum TBlendMode { Opaque, Cutout, Fade, Transparent };   // Use to change Material Standart Shader Render Mode
    

    /// <summary>Change the Material Standart Shader Render mode in runtime</summary>
    /// <param name="material">Material to be changed</param>
    /// <param name="blendMode">New render mode of the standar shader of this material</param>
    public static void SetMaterialRenderingMode(Material material, TBlendMode blendMode)
    {
        switch (blendMode)
        {
            case TBlendMode.Opaque:
                if (material.GetFloat("_Mode") != 0)
                {
                    material.SetFloat("_Mode", 0);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                }                
                break;
            case TBlendMode.Cutout:
                if (material.GetFloat("_Mode") != 1)
                {
                    material.SetFloat("_Mode", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                }               
                break;
            case TBlendMode.Fade:
                if (material.GetFloat("_Mode") != 2)
                {
                    material.SetFloat("_Mode", 2);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                }                    
                break;
            case TBlendMode.Transparent:
                if (material.GetFloat("_Mode") != 3)
                {
                    material.SetFloat("_Mode", 3);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                }                   
                break;
        }
    }


    /// <summary>Change alpha colour component of object material</summary>
    /// <param name="alpha">New alpha component to be set on the material colour</param>
    public static void SetAlphaColorToMaterial(Material material, float alpha)
    {
        Color currentColor = material.GetColor("_Color");
        currentColor.a = alpha;
        material.SetColor("_Color", currentColor);
    }


    /// <summary>Set a new colour to this object material</summary>
    /// <param name="newColor">New colour to set into this object material</param>
    public static void SetColourToMaterial(Material material, Color32 newColor)
    {
        material.color = newColor;
    }

    /// <summary>Get object material colour</summary>
    /// <returns>Material color</returns>
    public static Color32 GetColourOfMaterial(Material material)
    {
        return material.color;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }//hexToColor

    public static Color32 IndicatedColourCalculate(Color32 _confirmedColour)
    {
        Color32 _indicatedColor;

        float h, s, v;

        Color.RGBToHSV(_confirmedColour, out h, out s, out v);
        s -= s * 0.75f;

        _indicatedColor = Color.HSVToRGB(h, s, v);

        return _indicatedColor;
    }
}
