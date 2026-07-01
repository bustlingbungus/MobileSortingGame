using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Collections;
using UnityEngine;

public class ImageToTexture : MonoBehaviour
{
    public GameObject spritePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject obj = InstantiateSpriteFixedAspectRatio(spritePrefab, "Assets/Images/Mona Lisa.png", 150f);
            DownscaleSpriteTexture(obj.GetComponent<SpriteRenderer>().sprite, 0.01f);
        }
    }


    public static Sprite LoadSprite(string filename)
    {
        if (string.IsNullOrEmpty(filename)) return null;
        if (System.IO.File.Exists(filename))
        {
            int sprite_width = 100;
            int sprite_height = 100;

            byte[] bytes = System.IO.File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(sprite_width, sprite_height, TextureFormat.RGB24, false);
            if (bytes == null || texture == null) return null;

            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        return null;
    }

    public static GameObject InstantiateSprite(GameObject prefab, string filename, float width, float height)
    {
        Sprite sprite = LoadSprite(filename);
        GameObject obj = Instantiate(prefab);
        if (obj == null) return null;
        
        float r_wid = width / sprite.texture.width;
        float r_height = height / sprite.texture.height;
        obj.transform.localScale = new Vector3(r_wid, r_height, obj.transform.localScale.z);

        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        if (rend != null) rend.sprite = sprite;
        return obj;
    }

    public static GameObject InstantiateSpriteFixedAspectRatio(GameObject prefab, string filename, float height)
    {
        Sprite sprite = LoadSprite(filename);
        GameObject obj = Instantiate(prefab);
        if (obj == null) return null;
        
        float ratio = height / sprite.texture.height;
        obj.transform.localScale = new Vector3(ratio, ratio, obj.transform.localScale.z);

        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        if (rend != null) rend.sprite = sprite;
        return obj;
    }

    public static void DownscaleSpriteTexture(Sprite sprite, float downscale_ratio)
    {
        if (downscale_ratio >= 1f) return; // cannot upscale
        Texture2D tex = sprite.texture;
        if (tex == null) return;

        int r_width = (int)(tex.width * downscale_ratio);
        int r_height = (int)(tex.height * downscale_ratio);

        Color[] pixels = tex.GetPixels();
        Color[] new_pixels = new Color[pixels.Length];

        int px_width = tex.width / r_width;
        int px_height = tex.height / r_height;

        for (int i = 0; i < r_width; ++i) // big pixels horizontal
        {
            for (int j = 0; j < r_height; ++j) // big pixels vertical
            {
                Color avgColor = new Color(0f, 0f, 0f);
                for (int x = i * px_width; x < ((i + 1) * px_width); ++x) // real pixels within big, horizontal
                {
                    for (int y = j * px_height; y < ((j + 1) * px_height); ++y) // real pixels within big, vertical
                    {
                        avgColor += pixels[(y * tex.width) + x];
                    }
                }
                avgColor /= px_width * px_height;

                // assign color
                for (int x = i * px_width; x < ((i + 1) * px_width); ++x) // real pixels within big, horizontal
                {
                    for (int y = j * px_height; y < ((j + 1) * px_height); ++y) // real pixels within big, vertical
                    {
                        new_pixels[(y * tex.width) + x] = avgColor;
                    }
                }
            }
        }

        // Debug.Log(new_pixels.Length);
        tex.SetPixels(new_pixels);
        tex.Apply();
    }
}
