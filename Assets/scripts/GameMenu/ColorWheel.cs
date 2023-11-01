using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ColorWheel : MonoBehaviour
{
    [SerializeField] private Image colorImage;          // Gradient image that user uses to pick color
    [SerializeField] private Image previewImage;        // The selected color is set on
    [SerializeField] private Image colorPickSprite;     // Small circle that user uses to pick color
    [SerializeField] private Slider hueSlider;

    private Image targetImage;
    private Color newColor;
    private float hue = 0;
    private float saturation = 0;
    private float brightness = 0;
    private bool colorIsBeingHeld;

    private bool IsActive { get; set; }

    private void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(colorImage.rectTransform, Input.mousePosition))
        {
            colorIsBeingHeld = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            colorIsBeingHeld = false;
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

    public void ChangeHue(float sliderValue)
    {
        hue = sliderValue;
        newColor = Color.HSVToRGB(hue, saturation, brightness);
        colorImage.color = Color.HSVToRGB(hue, 1f, 1f);
    }

    public void GenerateRandomColor(Image target)
    {
        target.color = new(Random.value, Random.value, Random.value);
    }
    private void SetSelectedColor()
    {
        previewImage.color = newColor;
    }

    public void ActivateColorPicker(Image target)
    {
        targetImage = target;
        IsActive = true;
        SetSelectedColor();
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