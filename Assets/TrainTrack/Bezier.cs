using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EveluateQubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 =EvaluateQuadratic(a, b, c, t);
        Vector2 p1 =EvaluateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 GetTanget(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
	{
        Vector2 c1 = (d - (3 * c) + (3 * b) - a);
        Vector2 c2 = ((3*c)-(6*b)+(3*a));
        Vector2 c3 = ((3*b)-(3*a));

        return ((3 * c1 * t * t) + (2 * c2 * t) + c3);
	}
}
