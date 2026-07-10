using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class TouchToggleButton : MonoBehaviour
{
    public Color toggledOnColor = Color.yellow;
    public GameObject lightUpObject;
    public bool startToggledOn;
    public UnityEvent onToggledOn;
    public UnityEvent onToggledOff;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Renderer buttonRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color originalColor;
    private int touchCount;
    private bool isToggledOn;

    private void Awake()
    {
        buttonRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalColor = buttonRenderer.sharedMaterial.GetColor(BaseColorId);

        isToggledOn = startToggledOn;
        ApplyVisualState();
    }

    private void OnTriggerEnter(Collider other)
    {
        touchCount++;
        if (touchCount == 1)
        {
            SetToggled(!isToggledOn);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        touchCount = Mathf.Max(0, touchCount - 1);
    }

    private void SetToggled(bool toggledOn)
    {
        isToggledOn = toggledOn;
        ApplyVisualState();

        if (isToggledOn)
        {
            onToggledOn.Invoke();
        }
        else
        {
            onToggledOff.Invoke();
        }
    }

    private void ApplyVisualState()
    {
        SetColor(isToggledOn ? toggledOnColor : originalColor);
        if (lightUpObject != null)
        {
            lightUpObject.SetActive(isToggledOn);
        }
    }

    private void SetColor(Color color)
    {
        propertyBlock.SetColor(BaseColorId, color);
        buttonRenderer.SetPropertyBlock(propertyBlock);
    }
}
