using UnityEngine;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour
{
    private Material matUniversal;
    public List<GameObject> objetosEscena = new List<GameObject>();

    private GameObject _techo;
    private GameObject _paredes;

    void Start()
    {
        matUniversal = new Material(Shader.Find("Custom/ShaderUnversal"));
        string assetsPath = UnityEngine.Application.dataPath + "/Models/casa/";

        // --- 1. ESTRUCTURA ---
        GameObject piso = BuildFromPath("Piso_OBJ", assetsPath + "piso.obj", null, Color.gray);
        if (piso != null) AplicarTransformacion(piso, Vector3.zero, Vector3.zero, Vector3.one);

        _paredes = BuildFromPath("Paredes_OBJ", assetsPath + "paredes.obj", null, Color.white);
        if (_paredes != null) AplicarTransformacion(_paredes, new Vector3(0, 1.25f, 0), Vector3.zero, Vector3.one);

        // Guardamos la referencia del techo para el toggle
        _techo = BuildFromPath("Techo_OBJ", assetsPath + "techo.obj", null, Color.blue);
        if (_techo != null) AplicarTransformacion(_techo, new Vector3(0, 2.5f, 0), Vector3.zero, Vector3.one);

        // --- 2. ABERTURAS ---
        GameObject puertaEnt = BuildFromPath("Puerta_Entrada", assetsPath + "puerta.obj", null, Color.red);
        if (puertaEnt != null) AplicarTransformacion(puertaEnt, new Vector3(-2.05f, 1.0f, -2.51f), Vector3.zero, Vector3.one);

        GameObject ventana = BuildFromPath("Ventana_Fondo", assetsPath + "ventana.obj", null, Color.cyan);
        if (ventana != null) AplicarTransformacion(ventana, new Vector3(0, 1.5f, 2.51f), Vector3.zero, Vector3.one);

        float rotY = 90f * Mathf.Deg2Rad;
        GameObject puertaBano = BuildFromPath("Puerta_Bano", assetsPath + "puerta.obj", null, Color.yellow);
        if (puertaBano != null) AplicarTransformacion(puertaBano, new Vector3(1.51f, 1.0f, -0.1f), new Vector3(0, rotY, 0), Vector3.one);

        // --- 3. CÁMARA ---
        CamaraController cam = Object.FindFirstObjectByType<CamaraController>();
        if (cam != null)
        {
            cam.ConfigurarCamara(new Vector3(0, 1.25f, 0), 12f, 55f, new Vector3(0, 1.5f, 0));
        }
    }

    void Update()
    {
        // TOGGLE DEL TECHO (Tecla T)
        if (Input.GetKeyDown(KeyCode.T) && _techo != null)
        {
            MeshRenderer mr = _techo.GetComponent<MeshRenderer>();
            mr.enabled = !mr.enabled;
        }

        // TOGGLE DE PAREDES (Tecla P) - Opcional para inspección
        if (Input.GetKeyDown(KeyCode.P) && _paredes != null)
        {
            MeshRenderer mr = _paredes.GetComponent<MeshRenderer>();
            mr.enabled = !mr.enabled;
        }
    }

    GameObject BuildFromPath(string id, string path, Texture2D tex, Color col)
    {
        Mesh m = ObjParser.Parse(path);
        if (m == null) return null;
        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);

        GameObject go = new GameObject(id);
        go.transform.SetParent(this.transform);
        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(matUniversal);

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
            obj.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", m);
        }
    }
}