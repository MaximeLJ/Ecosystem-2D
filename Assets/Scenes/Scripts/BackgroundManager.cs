using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public bool useGradient;
    public Color innerColor = Color.white; // Public variable for inner color
    public Color outerColor = Color.black; // Public variable for outer color

    Color lastColor;
    [Range(0f, 1f)] 
    public float innerColorPercentage = 0.5f; // Public variable for inner color percentage

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();

        Camera cameraComponent = Camera.main;
        int width = (int)(2 * cameraComponent.orthographicSize * cameraComponent.aspect +1)*2;
        int height = (int)(Camera.main.orthographicSize*2 +1)*2;

        gameManager.background.transform.localScale = new Vector3(width, height);

        if (useGradient) {
            SetRadialGradient(width, height);
        }
    }

    void SetRadialGradient(float width, float height)
    {
        SpriteRenderer spriteRenderer = gameManager.background.GetComponent<SpriteRenderer>();

        Texture2D texture = new Texture2D((int)width, (int)height);

        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(texture.width / 2, texture.height / 2));
                float normalizedDistance = distance / (texture.width / 2);

                // Adjust the interpolation based on innerColorPercentage
                float interpolation = Mathf.Clamp01(1f - Mathf.Pow(normalizedDistance, 2));
                Color pixelColor = Color.Lerp(outerColor, innerColor, interpolation * innerColorPercentage);
                texture.SetPixel(x, y, pixelColor);

                lastColor = pixelColor;
            }
        }

        texture.Apply();

        // Create a sprite with the gradient texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);

        // Set the sprite to the SpriteRenderer
        spriteRenderer.sprite = sprite;
    }
}