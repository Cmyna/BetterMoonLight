// CopyRight (c) CMyna. All Rights Preserved.
// file "Normal.cs".
// Licensed under MIT License.

using UnityEngine;


/**
 * About Red Normal Map: check https://blenderartists.org/t/red-normal-map/1459867/2
 * `This is Unity normal map optimization. 
 * To get normal Normal map move Alpha channel to Green channel and fill Blue channel with pure white`
 */


namespace BetterMoonLight.Utils.TextureConverter
{
    public static class NormalMap
    {

        public static Texture2D ConvertToDXT5nm(Texture2D source)
        {
            int width = source.width;
            int height = source.height;

            // normal map require linear color values
            Texture2D convertedMap = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

            Color32[] cols = source.GetPixels32();

            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].a = cols[i].r; // flip Red & Alpha channel
                cols[i].r = 255; 
                cols[i].b = 255;
            }

            convertedMap.SetPixels32(cols);
            convertedMap.Apply();

            return convertedMap;
        }

    }
}
