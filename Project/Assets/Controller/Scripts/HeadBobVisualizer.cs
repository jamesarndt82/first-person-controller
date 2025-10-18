using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Attach this to the Camera to visualize head bob parameters in the Scene view
/// Helps with fine-tuning bob amplitudes and frequencies
/// </summary>
public class HeadBobVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color horizontalBobColor = Color.cyan;
    [SerializeField] private Color verticalBobColor = Color.yellow;
    [SerializeField] private Color centerColor = Color.green;
    [SerializeField] private float gizmoScale = 1f;

    [Header("References")]
    [SerializeField] private FirstPersonCamera cameraController;

    private Vector3 restPosition;

    private void Start()
    {
        if (cameraController == null)
        {
            cameraController = GetComponentInParent<FirstPersonCamera>();
        }
        
        restPosition = transform.localPosition;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw center point
        Gizmos.color = centerColor;
        Gizmos.DrawWireSphere(transform.position, 0.05f * gizmoScale);

        // Draw horizontal bob range (X-axis)
        Gizmos.color = horizontalBobColor;
        Vector3 horizontalStart = transform.position - transform.right * 0.05f * gizmoScale;
        Vector3 horizontalEnd = transform.position + transform.right * 0.05f * gizmoScale;
        Gizmos.DrawLine(horizontalStart, horizontalEnd);
        Gizmos.DrawWireSphere(horizontalStart, 0.02f * gizmoScale);
        Gizmos.DrawWireSphere(horizontalEnd, 0.02f * gizmoScale);

        // Draw vertical bob range (Y-axis)
        Gizmos.color = verticalBobColor;
        Vector3 verticalStart = transform.position - transform.up * 0.05f * gizmoScale;
        Vector3 verticalEnd = transform.position + transform.up * 0.05f * gizmoScale;
        Gizmos.DrawLine(verticalStart, verticalEnd);
        Gizmos.DrawWireSphere(verticalStart, 0.02f * gizmoScale);
        Gizmos.DrawWireSphere(verticalEnd, 0.02f * gizmoScale);

#if UNITY_EDITOR
        // Draw labels
        Handles.Label(horizontalEnd + Vector3.right * 0.1f, "Horizontal Bob", new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = horizontalBobColor },
            fontSize = 12
        });
        
        Handles.Label(verticalEnd + Vector3.up * 0.1f, "Vertical Bob", new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = verticalBobColor },
            fontSize = 12
        });
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HeadBobVisualizer))]
    public class HeadBobVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HeadBobVisualizer visualizer = (HeadBobVisualizer)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This visualizer shows the head bob range in the Scene view.\n\n" +
                "• Cyan = Horizontal (side-to-side) movement\n" +
                "• Yellow = Vertical (up-down) movement\n" +
                "• Green = Center/rest position\n\n" +
                "Adjust the gizmo scale if visualization is too small/large.",
                MessageType.Info
            );

            if (visualizer.GetComponentInParent<FirstPersonCamera>() == null)
            {
                EditorGUILayout.HelpBox(
                    "FirstPersonCamera not found! Attach this to the Camera that has the FirstPersonCamera component.",
                    MessageType.Warning
                );
            }
        }
    }
#endif
}
