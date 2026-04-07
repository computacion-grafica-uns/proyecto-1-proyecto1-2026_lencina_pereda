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
	
	[Header("Texturas de Cocina")]
	public Texture2D texHeladera;
	public Texture2D texCocina;
	public Texture2D texBachaCocina;
	public Texture2D texBajoMesada;
	public Texture2D texAlacena;

    private GameObject _techo;
    private GameObject _paredes;

    void Start()
    {
        // Declaramos rot90 una sola vez aquí para usarla en todo el Start
        float rot90 = 90f * Mathf.Deg2Rad;

        Shader shaderUnversal = Shader.Find("Custom/ShaderUnversal");
        if (shaderUnversal == null) return;
        matUniversal = new Material(shaderUnversal);

        string casaPath = Application.dataPath + "/Models/casa/";
        string bathPath = Application.dataPath + "/Models/Bathroom/";

        // --- 1. ESTRUCTURA ---
        GameObject piso = BuildFromPath("Piso", casaPath + "piso.obj", Color.gray, null);
        if (piso != null) AplicarTransformacion(piso, Vector3.zero, Vector3.zero, Vector3.one);

        _paredes = BuildFromPath("Paredes", casaPath + "paredes.obj", Color.white, null);
        if (_paredes != null) AplicarTransformacion(_paredes, new Vector3(0, 1.25f, 0), Vector3.zero, Vector3.one);

        _techo = BuildFromPath("Techo", casaPath + "techo.obj", Color.blue, null);
        if (_techo != null) AplicarTransformacion(_techo, new Vector3(0, 2.5f, 0), Vector3.zero, Vector3.one);

        GameObject pEnt = BuildFromPath("Puerta_Entrada", casaPath + "puerta.obj", Color.red, null);
        if (pEnt != null) AplicarTransformacion(pEnt, new Vector3(-2.05f, 1.0f, -2.51f), Vector3.zero, Vector3.one);

        GameObject ventana = BuildFromPath("Ventana", casaPath + "ventana.obj", Color.cyan, null);
        if (ventana != null) AplicarTransformacion(ventana, new Vector3(0, 1.5f, 2.51f), Vector3.zero, Vector3.one);

        GameObject pBano = BuildFromPath("Puerta_Bano", casaPath + "puerta.obj", Color.yellow, null);
        if (pBano != null)
        {
            // Usamos rot90 sin volver a poner "float"
            AplicarTransformacion(pBano, new Vector3(1.51f, 1.0f, -0.1f), new Vector3(0, rot90, 0), Vector3.one);
        }

        // --- 2. MUEBLES DEL BAÑO ---
        GameObject shower = BuildFromPath("Ducha", bathPath + "shower/shower.obj", Color.white, texDucha);
        if (shower != null) AplicarTransformacion(shower, new Vector3(2f, 1.1f, 2.0f), Vector3.zero, Vector3.one);

        GameObject toilet = BuildFromPath("Inodoro", bathPath + "toilet/toilet1/toilet1.obj", Color.white, texInodoro);
        if (toilet != null) AplicarTransformacion(toilet, new Vector3(3f, 0.85f, 0.5f), new Vector3(0, 180f * Mathf.Deg2Rad, 0), Vector3.one);

        GameObject sink = BuildFromPath("Lavamanos", bathPath + "sink/sink.obj", Color.white, texBacha);
        if (sink != null) AplicarTransformacion(sink, new Vector3(2.1f, 0.9f, -2.2f), new Vector3(0, -rot90, 0), Vector3.one);

        GameObject mirror = BuildFromPath("Espejo", bathPath + "mirror/mirror.obj", Color.white, texEspejo);
        if (mirror != null) AplicarTransformacion(mirror, new Vector3(2.1f, 1.7f, -2.48f), new Vector3(0, -rot90, 0), new Vector3(0.5f, 0.5f, 0.5f));
		
		// --- 3. SECTOR COCINA (Pared Izquierda) ---
		string kitchenPath = Application.dataPath + "/Models/Kitchen/";
		string cabinetsPath = kitchenPath + "Cabinets/";

		// Heladera
		GameObject fridge = BuildFromPath("Heladera", kitchenPath + "Fridge/Fridge.obj", Color.white, texHeladera);
		if (fridge != null) AplicarTransformacion(fridge, new Vector3(-3f, 1.0f, 2.0f), new Vector3(0, rot90, 0), Vector3.one);

		// Cocina con Horno
		GameObject stove = BuildFromPath("Cocina", cabinetsPath + "KitchenStoveWithOven/KitchenStoveWithOven.obj", Color.white, texCocina);
		if (stove != null) AplicarTransformacion(stove, new Vector3(-3f, 0.85f, 1.0f), new Vector3(0, rot90, 0), Vector3.one);

		// Bajo Mesada
		GameObject cabinet = BuildFromPath("BajoMesada", cabinetsPath + "KitchenCabinet1/KitchenCabinet1.obj", Color.white, texBajoMesada);
		if (cabinet != null) AplicarTransformacion(cabinet, new Vector3(-3f, 0.85f, 0.1f), new Vector3(0, rot90, 0), Vector3.one);

		// Bajo Mesada con Bacha
		GameObject sinkKitchen = BuildFromPath("BachaCocina", cabinetsPath + "KithcenCabinet1WithSink/KitchenCabinet1WithSink.obj", Color.white, texBachaCocina);
		if (sinkKitchen != null) AplicarTransformacion(sinkKitchen, new Vector3(-3f, 0.85f, -0.8f), new Vector3(0, rot90, 0), Vector3.one);

		// Alacenas
		GameObject upperCab = BuildFromPath("Alacena", cabinetsPath + "UpperCabinet/UpperCabinet.obj", Color.white, texAlacena);
		if (upperCab != null) AplicarTransformacion(upperCab, new Vector3(-3f, 1.5f, 0.1f), new Vector3(0, rot90, 0), Vector3.one);
		
        CamaraController cam = Object.FindFirstObjectByType<CamaraController>();
        if (cam != null) cam.ConfigurarCamara(new Vector3(0, 1.25f, 0), 12f, 55f, new Vector3(0, 1.5f, 0));
    }

    GameObject BuildFromPath(string id, string path, Color col, Texture2D tex)
    {
        Mesh m = ObjParser.Parse(path);
        if (m == null) return null;
        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        GameObject go = new GameObject(id);
        go.transform.SetParent(this.transform);
        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(matUniversal);
        if (tex != null)
        {
            mr.material.SetTexture("_MainTex", tex);
            mr.material.SetFloat("_UseTexture", 1.0f);
        }
        else mr.material.SetFloat("_UseTexture", 0.0f);
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && _techo != null)
            _techo.GetComponent<MeshRenderer>().enabled = !_techo.GetComponent<MeshRenderer>().enabled;
        if (Input.GetKeyDown(KeyCode.P) && _paredes != null)
            _paredes.GetComponent<MeshRenderer>().enabled = !_paredes.GetComponent<MeshRenderer>().enabled;
    }
}