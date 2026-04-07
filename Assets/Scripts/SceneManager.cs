using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SceneManager : MonoBehaviour
{
    private Material matUniversal;
    // Lista pública que tu CamaraController ya sabe recorrer
    public List<GameObject> objetosEscena = new List<GameObject>(); 

    private GameObject _techo;
    private GameObject _paredes;

    void Start() {
        // Inicializamos el material con tu shader manual
        matUniversal = new Material(Shader.Find("Custom/ShaderUnversal"));
        
        // Ruta a tus modelos (en minúscula según tus capturas)
        string assetsPath = Application.dataPath + "/Models/casa/"; 

        // --- CARGA DE ELEMENTOS (Hijos del SceneManager, Transform en 0) ---
        
        // PISO: Color Gris. Se queda en el origen.
        GameObject piso = BuildFromPath("Piso_OBJ", assetsPath + "piso.obj", null, Color.gray);
        if (piso != null) AplicarTransformacion(piso, Vector3.zero, Vector3.zero, Vector3.one);

        // PAREDES: Miden 2.5m de alto. El CentrarVertices las deja en 0. 
        // Subimos 1.25m en la matriz para que la base toque el suelo.
        _paredes = BuildFromPath("Paredes_OBJ", assetsPath + "paredes.obj", null, Color.white);
        if (_paredes != null) AplicarTransformacion(_paredes, new Vector3(0, 1.25f, 0), Vector3.zero, Vector3.one);
        
        // TECHO: Sube 2.5m (la altura total de las paredes) para que se vea arriba.
        _techo = BuildFromPath("Techo_OBJ", assetsPath + "techo.obj", null, Color.blue);
        if (_techo != null) AplicarTransformacion(_techo, new Vector3(0, 2.5f, 0), Vector3.zero, Vector3.one);

        // --- CARGA DE ABERTURAS ---
        
        // PUERTA: Mide 2m -> Centro en 1.0m. Z=-2.51f para que salga de la pared.
        GameObject puerta = BuildFromPath("Puerta_Entrada", assetsPath + "puerta.obj", null, Color.red);
        if (puerta != null) AplicarTransformacion(puerta, new Vector3(-2.05f, 1.0f, -2.51f), Vector3.zero, Vector3.one);
        
        // VENTANA: Centro del hueco en Y=1.5f. Z=2.51f (pared trasera).
        GameObject ventana = BuildFromPath("Ventana_Fondo", assetsPath + "ventana.obj", null, Color.cyan);
        if (ventana != null) AplicarTransformacion(ventana, new Vector3(0, 1.5f, 2.51f), Vector3.zero, Vector3.one);
    }

    void Update() {
        // Interacciones por teclado (Requisito Proyecto 1)
        if (Input.GetKeyDown(KeyCode.T) && _techo != null)
            _techo.GetComponent<MeshRenderer>().enabled = !_techo.GetComponent<MeshRenderer>().enabled;

        if (Input.GetKeyDown(KeyCode.P) && _paredes != null)
            _paredes.GetComponent<MeshRenderer>().enabled = !_paredes.GetComponent<MeshRenderer>().enabled;
    }

    // MÉTODO ACTIVIDAD 6: Crea el objeto como hijo y lo agrega a la lista
    GameObject BuildFromPath(string id, string path, Texture2D tex, Color col) {
        Mesh m = ObjParser.Parse(path);
        if (m == null) return null;

        GameObject go = new GameObject(id);
        
        // Clave Actividad 6: Los hacemos hijos de este objeto en la jerarquía
        go.transform.SetParent(this.transform);

        go.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(matUniversal);
        
        if (tex != null) {
            mr.material.SetTexture("_MainTex", tex);
            mr.material.SetFloat("_UseTexture", 1.0f);
        }

        Color[] colors = new Color[m.vertexCount];
        for (int i = 0; i < colors.Length; i++) colors[i] = col;
        m.colors = colors;

        go.AddComponent<ModelMatrix>();
        objetosEscena.Add(go); 
        return go;
    }

    // MÉTODO CLASE PRÁCTICA 3: Inyecta la matriz al Shader sin mover el Transform
    public void AplicarTransformacion(GameObject obj, Vector3 p, Vector3 r, Vector3 s) {
        ModelMatrix mm = obj.GetComponent<ModelMatrix>();
        // Generamos la matriz 4x4 (p, r, s)
        Matrix4x4 m = mm.CreateModelMatrix(p, r, s);
        // La inyectamos al shader
        obj.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", m);
    }
}