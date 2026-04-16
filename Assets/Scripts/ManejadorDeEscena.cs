using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManejadorDeEscena : MonoBehaviour
{
    [Header("Configuración del Bosque")]
    public int cantidadArboles = 200;
    public float radioBosque = 300f;
    [Range(1, 3)] public int iteraciones = 3;
    public float velocidadGiro = 20f;

    [Header("Referencias de Matrices")]
    private ViewMatrix viewMath;
    private ProjectionMatrix projMath;

    private Material universalMaterial;
    private Vector3[] vertices;
    private int[] triangles;

    private Camera camaraPrincipal;
    private List<GameObject> piezasBosque = new List<GameObject>();

    private struct PoseRama
    {
        public Vector3 posOriginal;
        public Quaternion rotOriginal;
        public Vector3 centroArbol;
    }
    private List<PoseRama> datosPiezas = new List<PoseRama>();

    void Start()
    {
        viewMath = GetComponent<ViewMatrix>();
        if (viewMath == null) viewMath = gameObject.AddComponent<ViewMatrix>();

        projMath = GetComponent<ProjectionMatrix>();
        if (projMath == null) projMath = gameObject.AddComponent<ProjectionMatrix>();

        CreateMaterial();
        CreateModel(); // Ahora solo definimos vértices y triángulos aquí
        PlantarBosque();
        CreateCamera();
    }

    void Update()
    {
        if (camaraPrincipal == null) return;

        // 1. Matrices globales de cámara
        Vector3 eye = camaraPrincipal.transform.position;
        Vector3 target = Vector3.zero;
        Matrix4x4 vMat = viewMath.CreateViewMatrix(eye, target, Vector3.up);

        float aspect = (float)Screen.width / Screen.height;
        Matrix4x4 pMatRaw = projMath.CalculatePerspectiveProjectionMatrix(
            camaraPrincipal.fieldOfView,
            aspect,
            camaraPrincipal.nearClipPlane,
            camaraPrincipal.farClipPlane
        );
        Matrix4x4 pMatGPU = GL.GetGPUProjectionMatrix(pMatRaw, true);

        float rotacionDinamica = Time.time * velocidadGiro;

        // 2. Inyectamos matrices en cada rama
        for (int i = 0; i < piezasBosque.Count; i++)
        {
            PoseRama dato = datosPiezas[i];
            Quaternion rotGiroY = Quaternion.Euler(0, rotacionDinamica, 0);

            Vector3 posRelativa = dato.posOriginal - dato.centroArbol;
            Vector3 posFinal = (rotGiroY * posRelativa) + dato.centroArbol;
            Vector3 rotFinalRad = (rotGiroY * dato.rotOriginal).eulerAngles * Mathf.Deg2Rad;

            ModelMatrix mm = piezasBosque[i].GetComponent<ModelMatrix>();
            if (mm != null)
            {
                Matrix4x4 mMat = mm.CreateModelMatrix(posFinal, rotFinalRad, Vector3.one);
                Renderer r = piezasBosque[i].GetComponent<Renderer>();
                r.material.SetMatrix("_ModelMatrix", mMat);
                r.material.SetMatrix("_ViewMatrix", vMat);
                r.material.SetMatrix("_ProjectionMatrix", pMatGPU);
            }
        }
    }

    void CreateMaterial() { universalMaterial = new Material(Shader.Find("Custom/ShaderUnversal")); }

    void CreateModel()
    {
        // Geometría en cruz (X) para evitar parpadeos
        vertices = new Vector3[] {
            new Vector3(0, 2.5f, 0),
            new Vector3(-0.4f, 0, 0),
            new Vector3(0.4f, 0, 0),
            new Vector3(0, 0, -0.4f),
            new Vector3(0, 0, 0.4f)
        };

        triangles = new int[] {
            0, 2, 1, 0, 1, 2, // Plano A (frente y atrás)
            0, 4, 3, 0, 3, 4  // Plano B (frente y atrás)
        };
    }

    void PlantarBosque()
    {
        for (int i = 0; i < cantidadArboles; i++)
        {
            Vector3 posArbol = new Vector3(
                Random.Range(-radioBosque, radioBosque),
                0,
                Random.Range(-radioBosque, radioBosque)
            );

            // 1. Generamos un verde aleatorio para este árbol
            Color colorAleatorio = new Color(0, Random.Range(0.4f, 1f), 0);

            // 2. Pasamos la posición Y el color
            GenerarArbolLSystem(posArbol, colorAleatorio);
        }
    }

    // CORRECCIÓN: Ahora acepta Color como segundo argumento
    void GenerarArbolLSystem(Vector3 offset, Color colorArbol)
    {
        string frase = "F";
        string regla = "F[+F]F[-F]F";
        for (int i = 0; i < iteraciones; i++)
        {
            string nueva = "";
            foreach (char c in frase) nueva += (c == 'F') ? regla : c.ToString();
            frase = nueva;
        }

        Stack<PoseRama> pila = new Stack<PoseRama>();
        Vector3 posActual = offset;
        Quaternion rotActual = Quaternion.identity;

        foreach (char c in frase)
        {
            if (c == 'F')
            {
                // Pasamos el color a BuildBranch
                BuildBranch(posActual, rotActual, offset, colorArbol);
                posActual += rotActual * Vector3.up * 2.0f;
            }
            else if (c == '+') rotActual *= Quaternion.Euler(0, 0, 25f);
            else if (c == '-') rotActual *= Quaternion.Euler(0, 0, -25f);
            else if (c == '[') pila.Push(new PoseRama { posOriginal = posActual, rotOriginal = rotActual });
            else if (c == ']')
            {
                var t = pila.Pop();
                posActual = t.posOriginal;
                rotActual = t.rotOriginal;
            }
        }
    }

    // CORRECCIÓN: Ahora recibe el color para aplicarlo a la malla
    void BuildBranch(Vector3 pos, Quaternion rot, Vector3 centro, Color colorRama)
    {
        GameObject rama = new GameObject("Rama_LSystem");

        // Creamos el array de colores para los 5 vértices
        Color[] listaColores = new Color[vertices.Length];
        for (int i = 0; i < listaColores.Length; i++) listaColores[i] = colorRama;

        Mesh m = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            colors = listaColores // Aplicamos el color aquí
        };

        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);

        rama.AddComponent<MeshFilter>().mesh = m;
        rama.AddComponent<MeshRenderer>().material = universalMaterial;
        rama.AddComponent<ModelMatrix>();

        piezasBosque.Add(rama);
        datosPiezas.Add(new PoseRama { posOriginal = pos, rotOriginal = rot, centroArbol = centro });
    }

    void CreateCamera()
    {
        GameObject camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        camaraPrincipal = camObj.AddComponent<Camera>();
        camaraPrincipal.farClipPlane = 5000f;
        camaraPrincipal.backgroundColor = Color.black;
        camaraPrincipal.clearFlags = CameraClearFlags.SolidColor;
        camaraPrincipal.fieldOfView = 60f;
        camObj.transform.position = new Vector3(0, 150, -400);
        camObj.transform.LookAt(Vector3.zero);
    }
}