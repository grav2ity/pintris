using UnityEngine;


public class ColorChanger : MonoBehaviour
{
    public Color[] backgroundColor;

    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] private SpriteRenderer leftFlipperSpriteRenderer;
    [SerializeField] private SpriteRenderer rightFlipperSpriteRenderer;
    [SerializeField] private SpriteRenderer obstacleSpriteRenderer;


    private void Awake()
    {
        ChangeColor();
    }

    public void ChangeColor()
    {
        Color color = backgroundColor[Random.Range(0, backgroundColor.Length)];
        backgroundSpriteRenderer.color = color;
        leftFlipperSpriteRenderer.color = color;
        rightFlipperSpriteRenderer.color = color;
        obstacleSpriteRenderer.color = color;
    }
}
