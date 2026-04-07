using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public enum ModoCamara { OrbitalAuto, PrimeraPersona, OrbitalPasos }
    public ModoCamara modoActual = ModoCamara.OrbitalPasos; // Arranca estática

    private ViewMatrix viewMath;
    private ModelMatrix modelMath; // Usamos su implementación manual
    private SceneManager sceneManager;

    [Header("Posición de la Cámara")]
    public Vector3 eye;
    public Vector3 target = Vector3.zero;
    
    private float yaw = 0f;
    private float pitch = 15f; 
    private float distancia = 25f;

    void Start() {
        viewMath = GetComponent<ViewMatrix>();
        modelMath = GetComponent<ModelMatrix>(); //
        sceneManager = Object.FindFirstObjectByType<SceneManager>();
        
        // Setup básico para el Viewport de Unity
        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;

        // FORZAMOS POSICIÓN ESTÁTICA INICIAL (Actividad 5)
        modoActual = ModoCamara.OrbitalPasos;
        distancia = 25f;
        yaw = 0f;
        pitch = 15f;
        
        CalcularPosicionOrbital();
        ActualizarTodo();
    }

    void Update() {
        // Tecla C: Cambia el modo
        if (Input.GetKeyDown(KeyCode.C)) {
            modoActual = (ModoCamara)(((int)modoActual + 1) % 3);
            
            // Si pasamos a Primera Persona, reseteamos la vista para no mirar al vacío
            if (modoActual == ModoCamara.PrimeraPersona) {
                eye = new Vector3(0, 5, -20); // Posición similar a la orbital
                yaw = 0f; pitch = 0f; // Mirando al frente (eje Z positivo)
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            distancia = 25f; yaw = 0f; pitch = 15f;
            target = Vector3.zero;
            CalcularPosicionOrbital();
        }

        switch (modoActual) {
            case ModoCamara.OrbitalAuto: ControlOrbitalAuto(); break;
            case ModoCamara.PrimeraPersona: ControlPrimeraPersona(); break;
            case ModoCamara.OrbitalPasos: ControlOrbitalPasos(); break;
        }

        ActualizarTodo();
    }

    void ControlOrbitalAuto() {
        yaw += 25f * Time.deltaTime;
        CalcularPosicionOrbital();
    }

    void ControlOrbitalPasos() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) yaw += 15f;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) yaw -= 15f;
        if (Input.GetKey(KeyCode.UpArrow)) distancia -= 10f * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) distancia += 10f * Time.deltaTime;
        distancia = Mathf.Clamp(distancia, 5f, 100f);
        CalcularPosicionOrbital();
    }

    void ControlPrimeraPersona() {
        // Sensibilidad ajustada para que no gire de golpe
        yaw += Input.GetAxis("Mouse X") * 2f;
        pitch -= Input.GetAxis("Mouse Y") * 2f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Vector director basado en yaw y pitch
        Vector3 dir = new Vector3(
            Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Sin(yaw * Mathf.Deg2Rad),
            Mathf.Sin(pitch * Mathf.Deg2Rad),
            Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Cos(yaw * Mathf.Deg2Rad)
        ).normalized;

        // Movimiento WASD
        eye += dir * (Input.GetAxis("Vertical") * 8f * Time.deltaTime);
        eye += Vector3.Cross(Vector3.up, dir).normalized * -(Input.GetAxis("Horizontal") * 8f * Time.deltaTime);
        
        target = eye + dir; // El target siempre está "adelante" del ojo
    }

    void CalcularPosicionOrbital() {
        float rY = yaw * Mathf.Deg2Rad, rP = pitch * Mathf.Deg2Rad;
        eye.x = target.x + distancia * Mathf.Cos(rP) * Mathf.Sin(rY);
        eye.y = target.y + distancia * Mathf.Sin(rP);
        eye.z = target.z - distancia * Mathf.Cos(rP) * Mathf.Cos(rY);
    }

    void ActualizarTodo() {
        if (sceneManager == null || viewMath == null || modelMath == null) return;
        
        // 1. Matriz de Vista (Manual)
        Matrix4x4 vMat = viewMath.CreateViewMatrix(eye, target, Vector3.up);
        
        // 2. Inyectamos la matriz en los objetos del SceneManager
        foreach (GameObject obj in sceneManager.objetosEscena) {
            if (obj != null)
                obj.GetComponent<Renderer>().material.SetMatrix("_ViewMatrix", vMat);
        }

        // 3. Matriz de Modelo para la cámara (Consistencia interna)
        // Usamos su script para que el objeto Camaras en Unity sepa su posición
        modelMath.CreateModelMatrix(eye, new Vector3(pitch, yaw, 0), Vector3.one);
    }
}