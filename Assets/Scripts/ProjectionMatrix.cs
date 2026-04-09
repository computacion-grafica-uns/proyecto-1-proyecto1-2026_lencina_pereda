using UnityEngine;

public class ProjectionMatrix : MonoBehaviour
{
    public Matrix4x4 CalculatePerspectiveProjectionMatrix(float fov, float aspect, float n, float f)
    {
        // Convertimos FOV a radianes y calculamos la tangente de la mitad [cite: 23, 36]
        float tanHalfFOV = Mathf.Tan((fov * 0.5f) * Mathf.Deg2Rad);
        Matrix4x4 p = Matrix4x4.zero;

        // Implementación de la fórmula del PDF [cite: 36]
        p[0, 0] = 1.0f / (aspect * tanHalfFOV);
        p[1, 1] = 1.0f / tanHalfFOV;
        p[2, 2] = (f + n) / (n - f);
        p[2, 3] = (2.0f * f * n) / (n - f);
        p[3, 2] = -1.0f; // Para división perspectiva [cite: 36]

        return p;
    }
}
