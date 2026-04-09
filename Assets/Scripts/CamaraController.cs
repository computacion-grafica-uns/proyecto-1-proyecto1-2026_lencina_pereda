using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public enum ModoCamara { OrbitalAuto, PrimeraPersona, OrbitalPasos }
    public ModoCamara modoActual = ModoCamara.OrbitalPasos;

    private ViewMatrix viewMath;
    private SceneManager sceneManager;

    [Header("Parámetros de Vista")]
    public Vector3 eye;
    public Vector3 target = Vector3.zero;

    private float yaw = 0f;
    private float pitch = 15f;
    private float distancia = 20f;

    // Memoria para el Reset (Tecla R)
    private Vector3 posInicialFPP;
    private float yawInicial, pitchInicial, distInicial;

    public void ConfigurarCamara(Vector3 centro, float dist, float inclinacion, Vector3 fppInicio)
    {
        this.target = centro;
        this.distancia = dist;
        this.pitch = inclinacion;
        this.posInicialFPP = fppInicio;

        // Parámetros originales de "fábrica"
        this.yawInicial = 0f;
        this.pitchInicial = inclinacion;
        this.distInicial = dist;

        if (modoActual == ModoCamara.PrimeraPersona) eye = posInicialFPP;
        else CalcularPosicionOrbital();

        ActualizarTodo();
    }

    void Start()
    {
        viewMath = GetComponent<ViewMatrix>();
        sceneManager = Object.FindFirstObjectByType<SceneManager>();

        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void Update()
    {
        // TECLA C: Cambia el modo
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (modoActual == ModoCamara.OrbitalAuto) modoActual = ModoCamara.PrimeraPersona;
            else if (modoActual == ModoCamara.PrimeraPersona) modoActual = ModoCamara.OrbitalPasos;
            else modoActual = ModoCamara.OrbitalAuto;

            if (modoActual == ModoCamara.PrimeraPersona)
            {
                eye = posInicialFPP;
                yaw = 0f;
                pitch = 0f;
            }
            else
            {
                CalcularPosicionOrbital();
            }
        }

        // --- TECLA R: RESETEA SEGÚN EL MODO (SIN CAMBIAR DE MODO) ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (modoActual == ModoCamara.PrimeraPersona)
            {
                // RESET INTERNO: Te devuelve al living y resetea la mirada
                eye = posInicialFPP;
                yaw = 0f;
                pitch = 0f;
                target = eye + new Vector3(0, 0, 1); // Mira hacia adelante
            }
            else
            {
                // RESET EXTERNO: Devuelve a la vista aérea original
                yaw = yawInicial;
                pitch = pitchInicial;
                distancia = distInicial;
                CalcularPosicionOrbital(); // SOLO se calcula si es orbital
            }
        }

        if (modoActual == ModoCamara.PrimeraPersona) ControlFPP();
        else ControlOrbital();

        ActualizarTodo();
    }

    void ControlOrbital()
    {
        // --- ZOOM GLOBAL (Auto y Pasos) ---
        if (Input.GetKey(KeyCode.UpArrow)) distancia -= 20f * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) distancia += 20f * Time.deltaTime;
        distancia = Mathf.Clamp(distancia, 4f, 45f);

        if (modoActual == ModoCamara.OrbitalAuto)
        {
            yaw += 20f * Time.deltaTime;
        }
        else
        {
            // Manual
            yaw -= Input.GetAxis("Mouse X") * 5f; 
            pitch += Input.GetAxis("Mouse Y") * 5f; 
            if (Input.GetKey(KeyCode.RightArrow)) yaw += 60f * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow)) yaw -= 60f * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, -89f, 89f);
        CalcularPosicionOrbital();
    }

    void ControlFPP()
    {
        // 1. Mirada natural (Mouse derecha = Mira derecha / Mouse arriba = Mira arriba)
        yaw += Input.GetAxis("Mouse X") * 3f;
        pitch += Input.GetAxis("Mouse Y") * 3f; // Cambiado de -= a +=
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        Vector3 dir = new Vector3(
            Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Sin(yaw * Mathf.Deg2Rad),
            Mathf.Sin(pitch * Mathf.Deg2Rad),
            Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Cos(yaw * Mathf.Deg2Rad)
        ).normalized;

        float speed = 6f;
        // Movimiento adelante/atrás
        eye += dir * (Input.GetAxis("Vertical") * speed * Time.deltaTime);

        // 2. CORRECCIÓN LATERAL: Quitamos el "-" para que D sea Derecha y A sea Izquierda
        // En Unity, el producto cruzado (Up x Dir) nos da el vector hacia la derecha
        eye += Vector3.Cross(Vector3.up, dir).normalized * (Input.GetAxis("Horizontal") * speed * Time.deltaTime);

        target = eye + dir;
    }

    void CalcularPosicionOrbital()
    {
        float rY = yaw * Mathf.Deg2Rad, rP = pitch * Mathf.Deg2Rad;
        eye.x = target.x + distancia * Mathf.Cos(rP) * Mathf.Sin(rY);
        eye.y = target.y + distancia * Mathf.Sin(rP);
        eye.z = target.z - distancia * Mathf.Cos(rP) * Mathf.Cos(rY);
    }

    void ActualizarTodo()
    {
        if (sceneManager == null || viewMath == null) return;

        // Obtenemos el script de proyección
        ProjectionMatrix projMath = GetComponent<ProjectionMatrix>();

        if (projMath == null) projMath = gameObject.AddComponent<ProjectionMatrix>();

        // 1. Calculamos la matriz manual (FOV 90, Aspect 16/9, Near 0.1, Far 1000) [cite: 23, 24, 25]
        Matrix4x4 pMatRaw = projMath.CalculatePerspectiveProjectionMatrix(90f, 16f / 9f, 0.1f, 1000f);

       // 2. Convertimos para la GPU (Crucial para DirectX/OpenGL) 
        Matrix4x4 pMatGPU = GL.GetGPUProjectionMatrix(pMatRaw, true);

        Matrix4x4 vMat = viewMath.CreateViewMatrix(eye, target, Vector3.up);

        foreach (GameObject obj in sceneManager.objetosEscena)
        {
            if (obj != null)
            {
                Renderer r = obj.GetComponent<Renderer>();

                ModelMatrix mm = obj.GetComponent<ModelMatrix>();
                if (r != null)
                {
                    r.material.SetMatrix("_ProjectionMatrix", pMatGPU);
                    r.material.SetMatrix("_ViewMatrix", vMat);
                    if (mm != null)
                    {
                        Matrix4x4 mMat = mm.CreateModelMatrix(obj.transform.position, obj.transform.eulerAngles * Mathf.Deg2Rad, obj.transform.localScale);
                        r.material.SetMatrix("_ModelMatrix", mMat);
                    }
                }
            }
        }
    }
}