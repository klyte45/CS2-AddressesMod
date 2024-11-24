using Game.Areas;
using System.Linq;
using System;
using UnityEngine;

namespace BelzontAdr
{
    public static class ADRMapUtils
    {
        public enum MapType
        {
            Topographic,
            Transparency
        }
        public static Color MapTopographyColor(float rampValue)
        {

            return rampValue switch
            {
                >= 1.00f => new Color32(0xCC, 0xCC, 0xCC, 0xFF),
                >= .9570f => Color.Lerp(new Color32(0xB2, 0x94, 0x6D, 0xFF), new Color32(0xCC, 0xCC, 0xCC, 0xFF), (rampValue - .9570f) / (1.00f - .9570f)),
                >= .9135f => Color.Lerp(new Color32(0x99, 0x5D, 0x0E, 0xFF), new Color32(0xB2, 0x94, 0x6D, 0xFF), (rampValue - .9135f) / (.9570f - .9135f)),
                >= .8700f => Color.Lerp(new Color32(0x90, 0x57, 0x0C, 0xFF), new Color32(0x99, 0x5D, 0x0E, 0xFF), (rampValue - .8700f) / (.9135f - .8700f)),
                >= .8265f => Color.Lerp(new Color32(0x87, 0x52, 0x0B, 0xFF), new Color32(0x90, 0x57, 0x0C, 0xFF), (rampValue - .8265f) / (.8700f - .8265f)),
                >= .7830f => Color.Lerp(new Color32(0x7F, 0x4C, 0x0A, 0xFF), new Color32(0x87, 0x52, 0x0B, 0xFF), (rampValue - .7830f) / (.8265f - .7830f)),
                >= .7395f => Color.Lerp(new Color32(0x76, 0x47, 0x08, 0xFF), new Color32(0x7F, 0x4C, 0x0A, 0xFF), (rampValue - .7395f) / (.7830f - .7395f)),
                >= .6960f => Color.Lerp(new Color32(0x6D, 0x41, 0x07, 0xFF), new Color32(0x76, 0x47, 0x08, 0xFF), (rampValue - .6960f) / (.7395f - .6960f)),
                >= .6525f => Color.Lerp(new Color32(0x65, 0x3C, 0x06, 0xFF), new Color32(0x6D, 0x41, 0x07, 0xFF), (rampValue - .6525f) / (.6960f - .6525f)),
                >= .6090f => Color.Lerp(new Color32(0x5F, 0x46, 0x0A, 0xFF), new Color32(0x65, 0x3C, 0x06, 0xFF), (rampValue - .6090f) / (.6525f - .6090f)),
                >= .5655f => Color.Lerp(new Color32(0x5A, 0x50, 0x0E, 0xFF), new Color32(0x5F, 0x46, 0x0A, 0xFF), (rampValue - .5655f) / (.6090f - .5655f)),
                >= .5220f => Color.Lerp(new Color32(0x55, 0x5B, 0x13, 0xFF), new Color32(0x5A, 0x50, 0x0E, 0xFF), (rampValue - .5220f) / (.5655f - .5220f)),
                >= .4785f => Color.Lerp(new Color32(0x50, 0x65, 0x17, 0xFF), new Color32(0x55, 0x5B, 0x13, 0xFF), (rampValue - .4785f) / (.5220f - .4785f)),
                >= .4350f => Color.Lerp(new Color32(0x4A, 0x6F, 0x1B, 0xFF), new Color32(0x50, 0x65, 0x17, 0xFF), (rampValue - .4350f) / (.4785f - .4350f)),
                >= .3915f => Color.Lerp(new Color32(0x45, 0x7A, 0x20, 0xFF), new Color32(0x4A, 0x6F, 0x1B, 0xFF), (rampValue - .3915f) / (.4350f - .3915f)),
                >= .3480f => Color.Lerp(new Color32(0x40, 0x84, 0x24, 0xFF), new Color32(0x45, 0x7A, 0x20, 0xFF), (rampValue - .3480f) / (.3915f - .3480f)),
                >= .3045f => Color.Lerp(new Color32(0x3B, 0x8E, 0x28, 0xFF), new Color32(0x40, 0x84, 0x24, 0xFF), (rampValue - .3045f) / (.3480f - .3045f)),
                >= .2610f => Color.Lerp(new Color32(0x36, 0x99, 0x2D, 0xFF), new Color32(0x3B, 0x8E, 0x28, 0xFF), (rampValue - .2610f) / (.3045f - .2610f)),
                >= .2175f => Color.Lerp(new Color32(0x4F, 0xAA, 0x46, 0xFF), new Color32(0x36, 0x99, 0x2D, 0xFF), (rampValue - .2175f) / (.2610f - .2175f)),
                >= .1740f => Color.Lerp(new Color32(0x68, 0xBB, 0x60, 0xFF), new Color32(0x4F, 0xAA, 0x46, 0xFF), (rampValue - .1740f) / (.2175f - .1740f)),
                >= .1305f => Color.Lerp(new Color32(0x81, 0xCC, 0x7A, 0xFF), new Color32(0x68, 0xBB, 0x60, 0xFF), (rampValue - .1305f) / (.1740f - .1305f)),
                >= .0870f => Color.Lerp(new Color32(0x9A, 0xDD, 0x94, 0xFF), new Color32(0x81, 0xCC, 0x7A, 0xFF), (rampValue - .0870f) / (.1305f - .0870f)),
                >= .0435f => Color.Lerp(new Color32(0xAC, 0xE9, 0xBD, 0xFF), new Color32(0x9A, 0xDD, 0x94, 0xFF), (rampValue - .0435f) / (.0870f - .0435f)),
                >= 0 => Color.Lerp(new Color32(0xBE, 0xF5, 0xE7, 0xFF), new Color32(0xAC, 0xE9, 0xBD, 0xFF), rampValue / .0435f),
                _ => new Color32(0xBE, 0xF5, 0xE7, 0xFF)
            };
        }

        public static Texture2D RenderTextureTo2D(RenderTexture tex, Func<Color, float> mappingFn, MapType mapType)
        {
            if (!tex) return null;
            Texture2D texture = new(tex.width, tex.height, TextureFormat.RGBA32, false);
            var oldActive = RenderTexture.active;
            RenderTexture.active = tex;
            texture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            switch (mapType)
            {
                case MapType.Topographic:
                    texture.SetPixels(texture.GetPixels().Select(x => MapTopographyColor(mappingFn(x))).ToArray());
                    break;
                case MapType.Transparency:
                    texture.SetPixels(texture.GetPixels().Select(x => new Color(1, 1, 1, mappingFn(x))).ToArray());
                    break;
            }
            texture.Apply();
            RenderTexture.active = oldActive;
            return texture;
        }
    }
}
