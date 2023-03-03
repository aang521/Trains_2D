using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathGenerator))]
public class PathEditor : Editor
{
    PathGenerator generator;
    Path path
    {
        get
        {
            return generator.path;
        }
    }

    const float segmentSelectDistTres = 0.1f;
    int currentSelectedSegmentIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate new path"))
        {
            generator.GeneratePath();
            SceneView.RepaintAll();
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Closed");

        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(generator, "Toggle closed");
            path.IsClosed = isClosed;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    private void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (currentSelectedSegmentIndex != -1)
            {
                Undo.RecordObject(generator, "Split segment");
                path.SplitSegment(mousePos, currentSelectedSegmentIndex);
            }
            else if (path.IsClosed == false)
            {
                Undo.RecordObject(generator, "Add Segment");
                path.AddSegment(mousePos);
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistAnchor = generator.AnchorDia * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < path.NumberOfPoints; i += 3)
            {
                float dist = Vector2.Distance(mousePos, path[i]);
                if (dist < minDistAnchor)
                {
                    dist = minDistAnchor;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(generator, "Delete segment");
                path.DeleteSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDistToSegment = segmentSelectDistTres;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < path.NumberOfSegments; i++)
            {
                Vector2[] points = path.GetPointsInSegment(i);
                float dist = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dist < minDistToSegment)
                {
                    minDistToSegment = dist;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != currentSelectedSegmentIndex)
            {
                currentSelectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }

    private void Draw()
    {
        for (int i = 0; i < path.NumberOfSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            if (generator.displayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color segmentColor = (i == currentSelectedSegmentIndex && Event.current.shift) ? generator.SelectedSegmentColor : generator.SegmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);

        }

        for (int i = 0; i < path.NumberOfPoints; i++)
        {
            if ((i % 3 == 0) || generator.displayControlPoints)
            {
                Handles.color = (i % 3 == 0) ? generator.AnchorColor : generator.ControlColor;
                float handleSize = (i % 3 == 0) ? generator.AnchorDia : generator.controlDia;
                Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, handleSize, Vector2.zero, Handles.CylinderHandleCap);
                if (path[i] != newPos)
                {
                    Undo.RecordObject(generator, "Move Point");
                    path.MovePoint(i, newPos);
                }
            }
        }
    }

    private void OnEnable()
    {
        generator = (PathGenerator)target;
        if (generator.path == null)
            generator.GeneratePath();
    }
}
