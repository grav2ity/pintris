using System;

using UnityEngine;


[Serializable]
[RequireComponent(typeof(PolygonCollider2D), typeof(SpriteRenderer))]
public class BlockView : MonoBehaviour
{
    public Vector2Int block;

    [SerializeField] private PolygonCollider2D polygonCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool ready;


    public Color Color
    {
        get => spriteRenderer.color;
        set => spriteRenderer.color = value;
    }

    public float Size {
        set
        {
            var scale = Sprite.pixelsPerUnit / Sprite.rect.width * value;
            transform.localScale = new Vector3(scale, scale);
        }
    }

    public Sprite Sprite
    {
        get => spriteRenderer.sprite;
        set => spriteRenderer.sprite = value;
    }


    public void EnableCollider(bool p)
    {
        polygonCollider.enabled = p;
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    private void Awake()
    {
        if (ready == false)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            polygonCollider = GetComponent<PolygonCollider2D>();

            polygonCollider.usedByComposite = true;
        }
    }

    private void Start()
    {
        if (ready == false)
        {
            var scale = Sprite.pixelsPerUnit / Sprite.rect.width;
            float s =  0.5f / scale;

            polygonCollider.points = new []{
                new Vector2(-1f, -1f) * s,
                new Vector2(-1f, 1f) * s,
                new Vector2(1f, 1f) * s,
                new Vector2(1f, -1f) * s
            };

            ready = true;
        }
    }
}
