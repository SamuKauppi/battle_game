using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorWheel : MonoBehaviour
{
    [SerializeField] private Image colorImage;          // Gradient image that user uses to pick color
    [SerializeField] private Image previewImage;         // The selected color is set on
    private Image targetImage;
    [SerializeField] private Image colorPickSprite;     // Small circle that user uses to pick color
    [SerializeField] private Slider hueSlider;

    private Color newColor;
    private float hue = 0;
    private float saturation = 0;
    private float brightness = 0;

    public Color topLeftColor;
    public Color topRightColor;
    public Color bottomLeftColor;
    public Color bottomRightColor;
    private Texture2D texture;

    private bool colorIsBeingHeld;
    private bool hueIsBeingHeld;

    private bool IsActive { get; set; }

    private void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(colorImage.rectTransform, Input.mousePosition))
        {
            colorIsBeingHeld = true;
        }

        if (hueSlider.value != hue)
        {
            hueIsBeingHeld = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (hueIsBeingHeld)
            {
                ChangeHue();
            }

            colorIsBeingHeld = false;
            hueIsBeingHeld = false;
        }

        if (colorIsBeingHeld)
        {
            // Get the normalized position of the mouse within the color image
            RectTransformUtility.ScreenPointToLocalPointInRectangle(colorImage.rectTransform, Input.mousePosition, null, out Vector2 localMousePos);

            // Clamp the sprite's position to stay within the color image
            float clampedX = Mathf.Clamp(localMousePos.x, -colorImage.rectTransform.rect.width / 2f, colorImage.rectTransform.rect.width / 2f);
            float clampedY = Mathf.Clamp(localMousePos.y, -colorImage.rectTransform.rect.height / 2f, colorImage.rectTransform.rect.height / 2f);
            colorPickSprite.rectTransform.anchoredPosition = new Vector2(clampedX, clampedY);


            // Calculate the normalized position within the color image
            saturation = Mathf.Clamp01((localMousePos.x + colorImage.rectTransform.rect.width / 2f) / colorImage.rectTransform.rect.width);
            brightness = Mathf.Clamp01((localMousePos.y + colorImage.rectTransform.rect.height / 2f) / colorImage.rectTransform.rect.height);

            // Convert HSV to RGB color
            newColor = Color.HSVToRGB(hue, saturation, brightness);
        }

        if (newColor != previewImage.color)
        {
            SetSelectedColor();
        }
    }

    private void ChangeHue()
    {
        hue = hueSlider.value;
        newColor = Color.HSVToRGB(hue, saturation, brightness);
        topRightColor = Color.HSVToRGB(hue, 1f, 1f);
        ApplyGradientColor();
    }

    public void GenerateRandomColor(Image target)
    {
        target.color = new(Random.value, Random.value, Random.value);
    }
    private void SetSelectedColor()
    {
        previewImage.color = newColor;
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
        }
    }

    public void ActivateColorPicker(Image target)
    {
        targetImage = target;
        IsActive = true;
        SetSelectedColor();
        ChangeHue();
    }

    public void SetColor()
    {
        IsActive = false;
        targetImage.color = newColor;
    }

    public void CancelColor()
    {
        IsActive = false;
        targetImage = null;
    }
}