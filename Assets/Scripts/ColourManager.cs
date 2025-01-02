using UnityEngine;

public class ColorManager : MonoBehaviour
{
    // Define max saturation (from 0 to 1)
    [SerializeField] private float maxSaturation = 0.8f;

    void Start()
    {
        // Example usage: Apply a randomized color to the shirt
        ApplyRandomColorToShirt();
    }

    void ApplyRandomColorToShirt()
    {
        // Find the shirt child by name or tag
        SpriteRenderer shirtRenderer = GetComponent<SpriteRenderer>();

        if (shirtRenderer != null)
        {
            // Generate a random color
            Color randomColor = Random.ColorHSV();

            // Convert the color to HSL
            Color.RGBToHSV(randomColor, out float hue, out float saturation, out float value);

            // Clamp the saturation to the desired max value
            saturation = Mathf.Clamp(saturation, 0, maxSaturation);

            // Convert back to RGB and apply it to the shirt
            shirtRenderer.color = Color.HSVToRGB(hue, saturation, value);
        }
    }
}
