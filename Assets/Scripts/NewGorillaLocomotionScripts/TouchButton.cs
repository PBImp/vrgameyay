using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class TouchButton : MonoBehaviour
{
    public Color pressedColor = Color.yellow;
    public GameObject lightUpObject;
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Renderer buttonRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color originalColor;
    private int touchCount;

    private void Awake()
    {
        buttonRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalColor = buttonRenderer.sharedMaterial.GetColor(BaseColorId);

        if (lightUpObject != null)
        {
            lightUpObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        touchCount++;
        if (touchCount == 1)
        {
            SetColor(pressedColor);
            if (lightUpObject != null)
            {
                lightUpObject.SetActive(true);
            }
            onPressed.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        touchCount = Mathf.Max(0, touchCount - 1);
        if (touchCount == 0)
        {
            SetColor(originalColor);
            if (lightUpObject != null)
            {
                lightUpObject.SetActive(false);
            }
            onReleased.Invoke();
        }
    }

    private void SetColor(Color color)
    {
        propertyBlock.SetColor(BaseColorId, color);
        buttonRenderer.SetPropertyBlock(propertyBlock);
    }
}
