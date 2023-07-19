using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrapped code from ColorWheel.cs 
/// Creates a cool gradient that is used in game but needs to be made only once
/// </summary>
public class CreateColorGradient : MonoBehaviour
{
    private Texture2D texture;
    [SerializeField] private Image colorImage;
    public Color topLeftColor;
    public Color topRightColor;
    public Color bottomLeftColor;
    public Color bottomRightColor;

    private void Start()
    {
        topLeftColor = new Color(0, 0, 0, 0);
        topRightColor = new(0, 0, 0, 1);
        bottomLeftColor = new(1, 1, 1, 0);
        bottomRightColor = new(1, 1, 1, 1);
        ApplyGradientColor();
    }
    public void ApplyGradientColor()
    {
        if (colorImage != null)
        {
            // Get the size of the target image
            Vector2 imageSize = colorImage.rectTransform.rect.size;
            texture = new Texture2D((int)imageSize.x, (int)imageSize.y);

            // Apply the gradient color to the texture
            Color32[] colors = new Color32[texture.width * texture.height];
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    float normalizedX = Mathf.InverseLerp(0, texture.width - 1, x);
                    float normalizedY = Mathf.InverseLerp(0, texture.height - 1, y);

                    colors[y * texture.width + x] = Color.Lerp(
                        Color.Lerp(bottomLeftColor, bottomRightColor, normalizedX),
                        Color.Lerp(topLeftColor, topRightColor, normalizedX),
                        normalizedY); ;
                }
            }

            // Apply the colors to the texture
            texture.SetPixels32(colors);
            texture.Apply();

            // Apply the texture to the target image
            colorImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            // Save the texture as a PNG file
            byte[] imageBytes = texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/gradient.png", imageBytes);
        }
    }
}
