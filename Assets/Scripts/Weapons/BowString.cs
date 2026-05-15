using UnityEngine;

/// <summary>
/// Draws a bow string between the top, hand, and bottom points.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BowString : MonoBehaviour
{
    [Header("Bow String Points")]
    [Tooltip("Top of the bow where the string attaches.")]
    public Transform bowTop;

    [Tooltip("Hand position that pulls the string.")]
    public Transform bowHand;

    [Tooltip("Bottom of the bow where the string attaches.")]
    public Transform bowBottom;

    [Header("Rendering")]
    public LineRenderer lineRenderer;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 3;
        }
    }

    private void LateUpdate()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        if (lineRenderer == null)
        {
            return;
        }

        if (bowTop == null || bowHand == null || bowBottom == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bowTop.position);
        lineRenderer.SetPosition(1, bowHand.position);
        lineRenderer.SetPosition(2, bowBottom.position);
    }
}
