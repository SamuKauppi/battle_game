using UnityEngine;
using UnityEngine.UI;

public class ColorWheel : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    [SerializeField] private Image colorPickSprite;
    [SerializeField] private Slider hueSlider;

    private Color newColor;
    private Color selectedColor;
    private float hue = 0;
    private float saturation = 0;
    private float brightness = 0;
    [SerializeField] private Image targetImage;

    private bool isBeingHeld;

    private void Update()
    {
        hue = hueSlider.value;
        // Check if the mouse is within the color image's rect
        if (RectTransformUtility.RectangleContainsScreenPoint(colorImage.rectTransform, Input.mousePosition)
            && Input.GetMouseButtonDown(0))
        {
            isBeingHeld = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isBeingHeld = false;
        }

        if (isBeingHeld)
        {
            // Get the normalized position of the mouse within the color image
            RectTransformUtility.ScreenPointToLocalPointInRectangle(colorImage.rectTransform, Input.mousePosition, null, out Vector2 localMousePos);
  
            // Clamp the sprite's position to stay within the color image
            float clampedX = Mathf.Clamp(localMousePos.x, -colorImage.rectTransform.rect.width / 2f, colorImage.rectTransform.rect.width / 2f);
            float clampedY = Mathf.Clamp(localMousePos.y, -colorImage.rectTransform.rect.height / 2f, colorImage.rectTransform.rect.height / 2f);
            colorPickSprite.rectTransform.anchoredPosition = new Vector2(clampedX, clampedY);


            // Calculate the normalized position within the color image
            saturation = (localMousePos.x + colorImage.rectTransform.rect.width / 2f) / colorImage.rectTransform.rect.width;
            brightness = (localMousePos.y + colorImage.rectTransform.rect.height / 2f) / colorImage.rectTransform.rect.height;
        }

        // Convert HSV to RGB color
        newColor = Color.HSVToRGB(hue, saturation, brightness);

        if (newColor != selectedColor)
        {
            SetSelectedColor();
        }
    }

    public void GenerateRandomColor()
    {
        newColor = new(Random.value, Random.value, Random.value);
    }

    private void SetSelectedColor()
    {
        selectedColor = newColor;
        targetImage.color = selectedColor;
    }
}