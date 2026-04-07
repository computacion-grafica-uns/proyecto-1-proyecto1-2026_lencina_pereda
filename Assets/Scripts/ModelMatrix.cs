using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelMatrix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
  public Matrix4x4 CreateModelMatrix(Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
  {
    // Matriz de Traslación
    Matrix4x4 positionMatrix = new Matrix4x4(
        new Vector4(1f, 0f, 0f, newPosition.x), // Primera columna
        new Vector4(0f, 1f, 0f, newPosition.y), // Segunda columna
        new Vector4(0f, 0f, 1f, newPosition.z), // Tercera columna
        new Vector4(0f, 0f, 0f, 1f)              // Cuarta columna
    );
    positionMatrix = positionMatrix.transpose;

    // Matriz de Rotación en X
    Matrix4x4 rotationMatrixX = new Matrix4x4(
        new Vector4(1f, 0f, 0f, 0f), 
        new Vector4(0f, Mathf.Cos(newRotation.x), Mathf.Sin(newRotation.x), 0f), 
        new Vector4(0f, -Mathf.Sin(newRotation.x), Mathf.Cos(newRotation.x), 0f), 
        new Vector4(0f, 0f, 0f, 1f)
    );
    rotationMatrixX = rotationMatrixX.transpose;

    // Matriz de Rotación en Y
    Matrix4x4 rotationMatrixY = new Matrix4x4(
        new Vector4(Mathf.Cos(newRotation.y), 0f, -Mathf.Sin(newRotation.y), 0f),
        new Vector4(0f, 1f, 0f, 0f),
        new Vector4(Mathf.Sin(newRotation.y), 0f, Mathf.Cos(newRotation.y), 0f),
        new Vector4(0f, 0f, 0f, 1f)
    );
    rotationMatrixY = rotationMatrixY.transpose;

    // Matriz de Rotación en Z
    Matrix4x4 rotationMatrixZ = new Matrix4x4(
        new Vector4(Mathf.Cos(newRotation.z), Mathf.Sin(newRotation.z), 0f, 0f),
        new Vector4(-Mathf.Sin(newRotation.z), Mathf.Cos(newRotation.z), 0f, 0f),
        new Vector4(0f, 0f, 1f, 0f),
        new Vector4(0f, 0f, 0f, 1f)
    );
    rotationMatrixZ = rotationMatrixZ.transpose;

    // Matriz de Escalado
    Matrix4x4 scaleMatrix = new Matrix4x4(
        new Vector4(newScale.x, 0f, 0f, 0f),
        new Vector4(0f, newScale.y, 0f, 0f),
        new Vector4(0f, 0f, newScale.z, 0f),
        new Vector4(0f, 0f, 0f, 1f)
    );
    scaleMatrix = scaleMatrix.transpose;

    // Combinación de matrices (Orden: T * R * S)
    Matrix4x4 rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
    Matrix4x4 finalMatrix = positionMatrix * rotationMatrix * scaleMatrix;

    return finalMatrix;
  }
   
}
