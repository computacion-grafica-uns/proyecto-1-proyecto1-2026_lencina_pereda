using UnityEngine;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour
{
    private Material matUniversal;
    public List<GameObject> objetosEscena = new List<GameObject>();

    [Header("Texturas de Muebles (Desde Inspector)")]
    public Texture2D texInodoro;
    public Texture2D texBacha;
    public Texture2D texEspejo;
    public Texture2D texDucha;

    private GameObject _techo;
    private GameObject _paredes;

    void Start()
    {
        // Inicializamos el material con tu Shader "Custom/ShaderUnversal"
        Shader shaderUnversal = Shader.Find("Custom/ShaderUnversal");
        if (shaderUnversal == null)
        {
            Debug.LogError("¡No se encontró el shader! Asegurate de que el archivo .shader diga 'Custom/ShaderUnversal'");
            return;
        }
        matUniversal = new Material(shaderUnversal);

        // Rutas base
        string casaPath = Application.dataPath + "/Models/casa/";
        string bathPath = Application.dataPath + "/Models/Bathroom/";

        // --- 1. ESTRUCTURA Y ABERTURAS (Colores sólidos) ---

        // Piso
        GameObject piso = BuildFromPath("Piso", casaPath + "piso.obj", Color.gray, null);
        if (piso != null) AplicarTransformacion(piso, Vector3.zero, Vector3.zero, Vector3.one);

        // Paredes
        _paredes = BuildFromPath("Paredes", casaPath + "paredes.obj", Color.white, null);
        if (_paredes != null) AplicarTransformacion(_paredes, new Vector3(0, 1.25f, 0), Vector3.zero, Vector3.one);

        // Techo
        _techo = BuildFromPath("Techo", casaPath + "techo.obj", Color.blue, null);
        if (_techo != null) AplicarTransformacion(_techo, new Vector3(0, 2.5f, 0), Vector3.zero, Vector3.one);

        // Puerta Entrada (Roja)
        GameObject pEnt = BuildFromPath("Puerta_Entrada", casaPath + "puerta.obj", Color.red, null);
        if (pEnt != null) AplicarTransformacion(pEnt, new Vector3(-2.05f, 1.0f, -2.51f), Vector3.zero, Vector3.one);

        // Ventana (Cian)
        GameObject ventana = BuildFromPath("Ventana", casaPath + "ventana.obj", Color.cyan, null);
        if (ventana != null) AplicarTransformacion(ventana, new Vector3(0, 1.5f, 2.51f), Vector3.zero, Vector3.one);

        // Puerta Baño (Amarilla)
        GameObject pBano = BuildFromPath("Puerta_Bano", casaPath + "puerta.obj", Color.yellow, null);
        if (pBano != null)
        {
            float rot90 = 90f * Mathf.Deg2Rad;
            AplicarTransformacion(pBano, new Vector3(1.51f, 1.0f, -0.1f), new Vector3(0, rot90, 0), Vector3.one);
        }

        // --- 2. MUEBLES DEL BAÑO (Rutas por carpetas y posiciones separadas) ---

        // DUCHA: Esquina fondo (Z positivo)
        GameObject shower = BuildFromPath("Ducha", bathPath + "shower/shower.obj", Color.white, texDucha);
        if (shower != null)
        {
            AplicarTransformacion(shower, new Vector3(2f, 1.1f, 2.0f), Vector3.zero, Vector3.one);
        }

        // INODORO: Posición central. Ruta: Bathroom/toilet/toilet1/toilet1.obj
        GameObject toilet = BuildFromPath("Inodoro", bathPath + "toilet/toilet1/toilet1.obj", Color.white, texInodoro);
        if (toilet != null)
        {
            // Rotado 180 para que mire al frente
            AplicarTransformacion(toilet, new Vector3(2.5f, 0.85f, 0.5f), new Vector3(0, 180f * Mathf.Deg2Rad, 0), Vector3.one);
        }

        // LAVAMANOS (Sink): Cerca de la puerta (Z negativo)
        GameObject sink = BuildFromPath("Lavamanos", bathPath + "sink/sink.obj", Color.white, texBacha);
        if (sink != null)
        {
            AplicarTransformacion(sink, new Vector3(2.1f, 0.9f, -2.2f), new Vector3(0, -90f * Mathf.Deg2Rad, 0), Vector3.one);
        }

        // ESPEJO: Sobre el lavamanos, pegado a la pared derecha
        GameObject mirror = BuildFromPath("Espejo", bathPath + "mirror/mirror.obj", Color.white, texEspejo);
        if (mirror != null)
        {
            // X=2.48 (pared derecha), Y=1.7 (altura ojos)
            AplicarTransformacion(mirror, new Vector3(2.1f, 1.7f, -2.48f), new Vector3(0, -90f * Mathf.Deg2Rad, 0), new Vector3(0.5f, 0.5f, 0.5f));
        }

        // --- 3. CONFIGURACIÓN CÁMARA ---
        CamaraController cam = Object.FindFirstObjectByType<CamaraController>();
        if (cam != null) cam.ConfigurarCamara(new Vector3(0, 1.25f, 0), 12f, 55f, new Vector3(0, 1.5f, 0));
    }

    // El método de construcción que ya tenías
    GameObject BuildFromPath(string id, string path, Color col, Texture2D tex)
    {
        Mesh m = ObjParser.Parse(path);
        if (m == null) return null;

        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);

        GameObject go = new GameObject(id);
        go.transform.SetParent(this.transform);
        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        // Material instanciado para cada objeto
        mr.material = new Material(matUniversal);

        if (tex != null)
        {
            // Usamos el nombre _MainTex que está en tu .shader
            mr.material.SetTexture("_MainTex", tex);
            mr.material.SetFloat("_UseTexture", 1.0f);
        }
        else
        {
            mr.material.SetFloat("_UseTexture", 0.0f);
        }

        Color[] colors = new Color[m.vertexCount];
        for (int i = 0; i < colors.Length; i++) colors[i] = col;
        m.colors = colors;

        go.AddComponent<ModelMatrix>();
        objetosEscena.Add(go);
        return go;
    }

    public void AplicarTransformacion(GameObject obj, Vector3 p, Vector3 r, Vector3 s)
    {
        obj.transform.position = p;
        obj.transform.eulerAngles = r * Mathf.Rad2Deg;

        ModelMatrix mm = obj.GetComponent<ModelMatrix>();
        if (mm != null)
        {
            Matrix4x4 m = mm.CreateModelMatrix(p, r, s);
            // Seteamos la matriz de modelo inicial
            obj.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", m);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && _techo != null)
            _techo.GetComponent<MeshRenderer>().enabled = !_techo.GetComponent<MeshRenderer>().enabled;
        if (Input.GetKeyDown(KeyCode.P) && _paredes != null)
            _paredes.GetComponent<MeshRenderer>().enabled = !_paredes.GetComponent<MeshRenderer>().enabled;
    }
}