using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Globalization;

public class OBJLoader : MonoBehaviour
{
    /// <summary>class for the vertex/uv couple</summary>
    public struct TVertexUv
    {
        public int u;
        public int v;

        public TVertexUv(int _u, int _v)
        {
            u = _u;
            v = _v;
        }
    }
    
    /// <summary>Class for the vertex/normal couple</summary>
    public struct TVertexNormal
    {
        public int u;
        public int v;

        public TVertexNormal(int _u, int _v)
        {
            u = _u;
            v = _v;
        }
    }
   
    /// <summary>Class for the vertex/uv/normal</summary>
    public struct TVertexUvNormal
    {
        public int t;
        public int u;
        public int v;

        public TVertexUvNormal(int _t, int _u, int _v)
        {
            t = _t;
            u = _u;
            v = _v;
        }
    }
    
    /// <summary>Class for add vertices, normals, uvs and triangles</summary>
    public class CMeshData
    {
        public List<Vector3> vertices;      //Store vertices
        public List<Vector3> normals;       //Store Normals
        public List<Vector2> uvs;           //Store UVS

        public CMeshData() //builder
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();           
            uvs = new List<Vector2>();
        }
    }

    /// <summary>Class to store any of the objects read from the object file</summary>
    public class COBJFileObject
    {

        public bool hasNormals;             //contains if the object has normals or not
        public bool hasUvs;                 //contains if the object has texture or not
                
        public List<int> triangles;         //Store the object triangles
        
        public List<string> identv;         //list for save the different identifiers of vertices
        public List<string> identu;         //list for save the different identifiers of uvs/texture
        public List<string> identn;         //list for save the different identifiers of normals
        public List<int> triangv;           //list for save the different identifiers of vertices ordered without repeat
        public List<int> triangu;           //list for save the different identifiers of uvs ordered without repeat
        public List<int> triangn;           //list for save the different identifiers of normals ordered without repeat
        public List<int> aIden;             //list for save the position of each vertex, uvs and normal, for unit them later and form triangles
                
        public int quantity;            //quantity of news numbers to identify
        public int account;             //counts the quantity of numbers identified

        public string name;             //Object name
        public int partNumber;          //Object part number. Zero if there just one part               
        public string materialName;             //Name of the material of the object

        // Constructors        
        public COBJFileObject() {
            Initialize();
        }

        public COBJFileObject(string _name)
        {
            Initialize();
            name = _name;
        }
       
        void Initialize()
        {
            hasNormals = false;
            hasUvs = false;

            quantity = 0;
            account = 0;

            partNumber = 0;
            materialName = "";

            //creates the lists
            identv = new List<string>();
            identu = new List<string>();
            identn = new List<string>();       
            triangv = new List<int>();
            triangu = new List<int>();
            triangn = new List<int>();
            aIden = new List<int>();
            
            //myMesh = new Mesh_type();
            triangles = new List<int>();            
        }
        public void Clear()
        {
            identv.Clear();
            identu  .Clear();
            identn  .Clear();
            triangv .Clear();
            triangu .Clear();
            triangn.Clear();
            aIden.Clear();
            triangles.Clear();
        }
    }

    const int M = 65000;                                //maximum number of vertices for each object son
    private enum fileOrigin_type { resources, web };    //two different origins for the file 
    private fileOrigin_type fileOrigin;                 //variables to select if, in the editor, we want to read the file from a file in the resources folder or from a URL (emulating WebGL)

    
    //string folder;                  //will be take the folder select between local or web file
    string fullFileUrl;
    //string localFile;
    string fullFilePath;                //file name
    string baseUrlOBJFile;
    string pathFilePath;

    C3DFileData file;                   //3D file data

    Hom3rFileReader fileToRead;         //will reads the file loaded    
    GameObject defaultParent;           //father object for each file loaded            

    string multiplePartObjectPrefix;    //part of the name of each object son        
    int numberOfObjectsRead;            //Store the number of objects read from the file
    GameObject multiplePartObject;      //Store the parent object when one object have multiple parts, more than 65000 vertices.

    bool loadingMaterialFile;               //Is true when a material file is been loading
    List<Material> listOfReadMaterials;     //list of materials loaded of the mtl file
    Texture2D webTexture;                   //Texture to download from the web
    bool firstmat = true;                   //control if the same object use various materials
    bool mapKsTextureExist;                 // Control if the KS texture exist or not

    public CMeshData mymesh;                    //Store vertices, normals and UVS from the read file
    COBJFileObject newReadObject;               //One object
    //List<COBJFileObject> listOfReadObjects;     //List of objects read from the file

    public Dictionary<TVertexUv, int> triangVertexUvDictionary;                //dictionary for find vertices/uvs faster
    public Dictionary<TVertexNormal, int> triangVertexNormalDictionary;        //dictionary for find vertices/uvs faster
    public Dictionary<TVertexUvNormal, int> triangVertexUvNormalDictionary;    //dictionary for find vertices/uvs faster
    public Dictionary<int, int> triangVertexDictionary;                        //dictionary for find vertices faster

    bool error;     //To store if exist any error during the file download

    System.Diagnostics.Stopwatch stopwatch;


    void Awake()
    {
        //Initialize variables
        numberOfObjectsRead = 0;
        loadingMaterialFile = false;
        listOfReadMaterials = new List<Material>();
        newReadObject = new COBJFileObject();
        //listOfReadObjects = new List<COBJFileObject>();
        mymesh = new CMeshData();
        triangVertexUvDictionary = new Dictionary<TVertexUv, int>();
        triangVertexNormalDictionary = new Dictionary<TVertexNormal, int>();
        triangVertexUvNormalDictionary = new Dictionary<TVertexUvNormal, int>();
        triangVertexDictionary = new Dictionary<int, int>();
        multiplePartObjectPrefix = "part";                              // Part of the name of each object son   
        error = false;                                                  // Initialize to false        
    }
   
    
    ////////////////////////
    //  Paint Methods
    ////////////////////////     

    /// <summary>Find a name to the object to be painted</summary>
    /// <param name="_name">Suggested name</param>
    /// <returns></returns>
    string FindNameToObject(string _name)
    {
        int i = 1;
        string finalName = _name;
        while (GameObject.Find(finalName) != null)
        {
            i++;
            finalName = _name + "_" + i.ToString();
        }
        return finalName;
    }

    /// <summary>Create a object in the Unity scene hierarchy tree</summary>
    /// <param name="_object">Object data read from the OBJ file</param>
    /// <returns></returns>
    GameObject PaintCreateObject(COBJFileObject _object)
    {
        GameObject newObject;

        if (_object.partNumber == 0)
        {
            if (_object.name == null) { return defaultParent; }             //If the file does not contain a object Name we make child of the default object
            newObject = new GameObject(FindNameToObject(_object.name));     //creates the object 3D with the name previously saved
            newObject.transform.parent = defaultParent.transform;           //sets the object parent            
            return newObject;                        
        }
        else if (_object.partNumber == 1)
        {
            if (_object.name == null)
            {
                multiplePartObject = defaultParent;         //If the file does not contain a object Name we make child of the default object
            }
            else
            {
                //Create the multiple part parent
                newObject = new GameObject(FindNameToObject(_object.name));     //Creates the multiple-part parent object 3D with the name previously saved
                newObject.transform.parent = defaultParent.transform;           //sets the object parent    
                multiplePartObject = newObject;                                 //Saves the multiple-part parent
            }            
            //Create the first part
            newObject = new GameObject(multiplePartObject.name + "_" + multiplePartObjectPrefix + "_" + _object.partNumber.ToString());       //Creates the object 3D with the name previously saved
            newObject.transform.parent = multiplePartObject.transform;                                                          //sets the object parent    

            return newObject;
        }
        else
        {
            if (multiplePartObject != null)
            {
                //Create the n part
                newObject = new GameObject(multiplePartObject.name + "_" + multiplePartObjectPrefix + "_" + _object.partNumber.ToString());       //Creates the object 3D with the name previously saved
                newObject.transform.parent = multiplePartObject.transform;                                                          //sets the object parent    

                return newObject;
            }
        }

        return null;
    }
    
    /// <summary>Paint one object</summary>
    /// <param name="_object">Object data read from the OBJ file</param>
    void PaintOneObject(COBJFileObject _object)
    {        
        GameObject newObject;
        CMeshData buildingMesh;                    //Store the final mesh
        buildingMesh = new CMeshData();            //Initialize Mesh structure;

        ////////////////////////
        // CREATE THE OBJECT
        ////////////////////////
        newObject = PaintCreateObject(_object);     //Create the object
        if (newObject==null) { return; }            //If we cannot create the object do nothing
        newObject.AddComponent<MeshFilter>();       //Adds to the object a new mesh
        newObject.AddComponent<MeshRenderer>();     //Adds to the object the new material  
        
        //////////
        // MESH
        //////////
        Mesh tempMesh = new Mesh();                    //creates the mesh
        tempMesh.name = _object.name;                  //name of the mesh
        //Vertices
        for (int i = 0; i < _object.triangv.Count; i++) //for all vertices
        {
            //takes the identifier of the vertex, searches it in personal mesh and adds it to the final mesh                    
            buildingMesh.vertices.Add(mymesh.vertices[_object.triangv[i]]);
        }        
        tempMesh.vertices = buildingMesh.vertices.ToArray(); //assigns the vertices of the final mesh to the mesh of the object created
        
        //Triangles
        tempMesh.triangles = _object.triangles.ToArray(); //assigns the triangles of the personal mesh to the mesh of the object
        
        //UVS
        if (_object.hasUvs == true) //if has texture, adds it
        {
            for (int i = 0; i < _object.triangu.Count; i++) //for all uvs
            {
                //takes the identifier of the uv, searches it in personal mesh and adds it to the final mesh
                buildingMesh.uvs.Add(mymesh.uvs[_object.triangu[i]]);
            }
            tempMesh.uv = buildingMesh.uvs.ToArray(); //assigns the uvs of the final mesh to the mesh of the object created
        }
        
        //Normals
        if (_object.hasNormals == true) //if the file has normals, adds them
        {
            for (int i = 0; i < _object.triangn.Count; i++) //for all normals
            {
                //takes the identifier of the normal, searches it in personal mesh and adds it to the final mesh
                buildingMesh.normals.Add(mymesh.normals[_object.triangn[i]]);
            }

            tempMesh.normals = buildingMesh.normals.ToArray(); //assigns the normals of the final mesh to the mesh of the object
        }
        else //if the file has not normals
        {
            tempMesh.RecalculateNormals(); //recalculates the normals
        }

        //////////////
        // MATERIAL
        //////////////        
        Material mat = new Material(Shader.Find("Standard"));   //loads the material         

        int index = listOfReadMaterials.FindIndex(r => r.name == _object.materialName);
        if (index != -1) { mat = listOfReadMaterials[index]; } //loads it
      
        /////////////////////////////////////////////
        // Copy Material and Mesh to the object
        /////////////////////////////////////////////              
        newObject.GetComponent<Renderer>().material = mat; //sets the material of the object
        newObject.GetComponent<MeshFilter>().mesh = tempMesh; //sets the mesh of the object and shows it                 
    }

    /// <summary>Clear all the structure when the execution has finished</summary>
    void Reset()
    {
        listOfReadMaterials.Clear();               //resets the list of the materials
        numberOfObjectsRead = 0;
        newReadObject = new COBJFileObject();              //Initialize the temp object to store one read object
        //listOfReadObjects.Clear();                        //Initialize the list of objects to be read
        mymesh = new CMeshData();
    }

        
    /////////////////////
    // Material loader
    /////////////////////

    /// <summary>Load texture file from a URL</summary>
    /// <param name="url">texture file path</param>
    /// <returns></returns>
    IEnumerator CoroutineLoadTextureFromWebFile(string pictureFileName, string url)    
    {
        if (url == null)
        {
            LoadTextureFromWebFile_Error("The texture file called " + pictureFileName + " cannot be found.");
            yield break;
        }

        //webTexture = new Texture2D(1, 1);                                   //Create a new texture file
        SendMessageToUI("Downloading " + pictureFileName + " : 0%");        //Send message to UI

        //WWW www = new WWW(url);                                             //Start download                
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        www.SendWebRequest();
        while (!www.isDone)
        {
            //Show downloaded progress
            SendMessageToUI("Downloading texture file " + pictureFileName + " : " + Mathf.Round(www.downloadProgress * 100.0f).ToString() + "%");        //Send message to UI            
            yield return new WaitForSeconds(0.1f);
        }

        if (www.isNetworkError || www.isHttpError) {
            LoadTextureFromWebFile_Error("Error downloading texture file " + pictureFileName);
            Debug.Log("Texture download error: " + www.error); //shows the error      
        } else {
            //if there is no error
            SendMessageToUI("Downloading texture file " + pictureFileName + " : 100%");        //Send message to UI            
            SendMessageToUI("Opening " + pictureFileName + "...");        //Send message to UI            

            webTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;            
            //www.LoadImageIntoTexture(webTexture);       //Load texture into the variable                
            
            webTexture.name = pictureFileName;          //Save texture name                    
        }        
    }
     
    private void LoadTextureFromWebFile_Error(string error_message)
    {
        error = true;        

        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadError, error_message));
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent._3DFileManager_ShowMessageConsole, "Texture download error: " + error_message));
        
    }

    /// <summary>Process material file line</summary>
    /// <param name="row">string line to process</param>
    /// <param name="material">material to store the info read</param>
    void ProcessMaterialFileLine(string row, ref Material material)
    {
        float Ns;
        float visibility;
        Color ctemp;

        if (row.StartsWith("newmtl ")) //new material
        {
            if (material != null) //if is not the first material
            {
                listOfReadMaterials.Add(material); //adds the last material to the list
            }

            material = new Material(Shader.Find("Standard")); //creates a new standard material by default
            material.name = row.Replace("newmtl ", "").Trim(); //name of the new material
            mapKsTextureExist = false;
        }
        else if (row.StartsWith("Ns ")) //specular exponent that ponders the value of specular colour
        {
            Ns = float.Parse(row.Replace("Ns ", "").Trim(), CultureInfo.InvariantCulture); //converts the value read to float
            Ns = Ns / 1000; //possible values between 0 and 1000
            if (!mapKsTextureExist)
            {
                //Map into the _Glossiness parameter because there are not KS texture                                
                material.SetFloat("_Glossiness", Ns); //sets this value in the material
            } else {
                //Map into the _GlossMapScale parameter because there are KS texture                                
                material.SetFloat("_GlossMapScale", Ns); //sets this value in the material   
            }

        }
        else if (row.StartsWith("Ka ")) //ambient colour
        {
            //converts the value read to colour type in secondary function and sets this value in the material
            material.SetColor("_EmissionColor", ConColor(row.Replace("Ka ", "").Trim(), 0.05f));
            material.EnableKeyword("_EMISSION"); //sets a shader keyword
        }
        else if (row.StartsWith("Kd ")) //diffuse colour
        {
            //converts the value read to colour type in secondary function and sets this value in the material
            material.SetColor("_Color", ConColor(row.Replace("Kd ", "").Trim()));
        }
        else if (row.StartsWith("Ks ")) //specular colour
        {
            //converts the value read to colour type in secondary function and sets this value in the material
            material.SetColor("_SpecColor", ConColor(row.Replace("Ks ", "").Trim()));
        }
        else if (row.StartsWith("d ") || row.StartsWith("Tr ")) //transparency
        {
            if (row.StartsWith("d ")) //Transparency factor, opaque(1) and transparent(0)
            {
                visibility = float.Parse(row.Replace("d ", "").Trim(), CultureInfo.InvariantCulture); //converts it to float
            }
            else if (row.StartsWith("Tr ")) //opposite to the transparency factor, transparent(1) and opaque(0)
            {
                visibility = 1 - float.Parse(row.Replace("Tr ", "").Trim(), CultureInfo.InvariantCulture); //transparency factor = 1 - value read
            }
            else //should not enter here, but fail if removed
            {
                visibility = 1; //for default, totally opaque
            }

            if (visibility < 1) //if is not totally opaque
            {
                ctemp = material.color; //loads the colour

                ctemp.a = visibility; //sets the new transparency factor
                material.SetColor("_Color", ctemp); //sets the new colour

                //transparency enabler
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
        else if (row.StartsWith("map_Kd ")) //texture
        {
            //loads texture from file            
            if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.OSXEditor) || (Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.OSXPlayer))
            {
                string dirIma = pathFilePath + row.Replace("map_Kd ", "").Trim(); //image direction
                if (File.Exists(dirIma)) //if exist the file
                {
                    string ext = Path.GetExtension(dirIma).ToLower(); //gets the image extension and converts in lower-case
                    //https://stackoverflow.com/questions/45416808/unity-create-texture-from-tga
                    if ((ext == ".png") || (ext == ".jpg")) //image format, png or jpg
                    {
                        Texture2D tex = new Texture2D(1, 1); //new texture
                        tex.LoadImage(File.ReadAllBytes(dirIma)); //loads the image and sets the texture
                        material.SetTexture("_MainTex", tex); //sets the material with the loaded texture
                    }
                    else //other types, not identified
                    {
                        Debug.Log("Texture not support: " + ext); //shows it the error
                    }
                }
            }           
        }
        else if (row.StartsWith("map_bump ") || row.StartsWith("bump ")) //bump mapping
        {
            if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.OSXEditor) || (Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.OSXPlayer))
            {
                //loads bump texture from file
                string dirIma; //saves the image direction
                if (row.StartsWith("map_bump "))
                {
                    dirIma = pathFilePath + row.Replace("map_bump ", "").Trim(); //image direction
                }
                else //row.StartsWith("bump ")
                {
                    dirIma = pathFilePath + row.Replace("bump ", "").Trim(); //image direction
                }

                if (File.Exists(dirIma)) //if exist the file
                {
                    string ext = Path.GetExtension(dirIma).ToLower(); //gets the image extension and converts in lower-case

                    if ((ext == ".png") || (ext == ".jpg")) //image format, png or jpg
                    {
                        Texture2D tex = new Texture2D(1, 1); //creates the new texture
                        tex.LoadImage(File.ReadAllBytes(dirIma)); //loads the image and sets the texture
                        Color[] pixels = tex.GetPixels(); //gets the pixels and assigns them to the array
                        for (int i = 0; i < pixels.Length; i++) //for all pixels
                        {
                            Color colTemp = pixels[i]; //creates a temporal colour
                            colTemp.r = pixels[i].g; //assigns the green to the red
                            colTemp.a = pixels[i].r; //assigns the red to the alpha
                            pixels[i] = colTemp; //saves the temporal colour modified
                        }
                        tex.SetPixels(pixels); //sets the texture with the new pixels
                        material.SetTexture("_BumpMap", tex); //sets the material with the loaded texture
                        material.EnableKeyword("_NORMALMAP"); //sets a shader keyword
                        material.SetFloat("_BumpScale", (float)0.5); //sets the intensity of the normal map
                    }
                    else //other types, not identified
                    {
                        Debug.Log("Texture not support: " + ext); //shows it the error
                    }
                }
            }    
        }
    }

    /// <summary>Loads the material file from a URL path</summary>
    /// <param name="url">URL file path</param>
    /// <returns></returns>
    IEnumerator CoroutineLoadMaterialFile_fromUrl(string materialFileName, string url)
    {
        if (url == null)
        {
            LoadMaterialFile_fromUrl_Error("The material file called " + materialFileName + " cannot be found.");
            yield break;
        }

        SendMessageToUI("Downloading " + materialFileName + " : 0%");       //Send message to UI

        //WWW fileWWW = new WWW(url);     //Download material file        
        UnityWebRequest fileWWW = UnityWebRequest.Get(url);
        fileWWW.SendWebRequest();

        //Show downloaded progress
        while (!fileWWW.isDone)
        {
            SendMessageToUI("Downloading " + materialFileName + " : " + Mathf.Round(fileWWW.downloadProgress * 100.0f).ToString() + "%");
            yield return new WaitForSeconds(0.1f);
        }
        
        // Check if error
        if (fileWWW.isNetworkError || fileWWW.isHttpError) {
            //error = true;
            //FinishOBJDownload();
            //hom3r.coreLink.Do(new CIOCommand(TIOCommands.ReportError, "Error downloading material file " + materialFileName), Constants.undoNotAllowed);

            LoadMaterialFile_fromUrl_Error("Error downloading material file " + materialFileName);
            Debug.LogError("WWW error: " + fileWWW.error); //shows the error
        } else {
            SendMessageToUI("Downloading " + materialFileName + " file : 100%");                        
            Hom3rFileReader materialFileToRead = new Hom3rFileReader(fileWWW);  //file read from the direction passed as parameter
            SendMessageToUI("Reading " + materialFileName + "...");
            //Process file line by line            
            bool nextLine = true;       
            string lineRead;
            Material material = null; //sets up the materials later            
            while (nextLine) //if can continue reading the file
            {
                lineRead = materialFileToRead.GetLine();    //reads the line in secondary function                                            
                if (lineRead != null)                       //if is not last line
                {
                    lineRead = lineRead.Trim();             //Clean spaces
                    if (lineRead.StartsWith("map_Kd "))
                    {
                        //loads texture from URL
                        string cleanedLine = lineRead.Replace("map_Kd ", "").Trim(); //image direction   

                        //Get Parameters and filename
                        string pictureFileName;
                        string[] parameters;    // TODO use parameters 
                        ExtractDataFrom(cleanedLine, out pictureFileName, out parameters);

                        //Image file extension
                        string pictureExtension = Path.GetExtension(pictureFileName).ToLower(); //gets the image extension and converts in lower-case                                                                                               
                        if ((pictureExtension == ".png") || (pictureExtension == ".jpg")) //image format, png or jpg
                        {
                            string pictureUrl = GetResourcesFileURL(pictureFileName);
                            //Texture webTexture = null; // new Texture();                     //Texture to download from the web
                            yield return StartCoroutine(CoroutineLoadTextureFromWebFile(pictureFileName, pictureUrl));        //Download the texture and wait until it finish
                            material.SetTexture("_MainTex", webTexture);                    //sets the material with the loaded texture

                            //Check Colour - If the colour is black we change it to white
                            if (material.GetColor("_Color") == Color.black) { material.SetColor("_Color", Color.white); }                            
                        }
                        else //other types, not identified
                        {
                            Debug.Log("Texture not support: " + pictureExtension); //shows it the error
                            this.SendMessageToUI("Texture not support: " + pictureExtension);
                        }
                    }
                    else if (lineRead.StartsWith("map_bump ") || lineRead.StartsWith("bump ")) //bump mapping
                    {
                        //loads bump texture from URL
                        string cleanedLine;
                        //Image path                       
                        cleanedLine = lineRead.Replace("bump ", "").Trim();
                        cleanedLine = lineRead.Replace("map_", "").Trim();

                        //Get Parameters and filename
                        string pictureFileName;
                        string[] parameters;    // TODO use parameters 
                        ExtractDataFrom(cleanedLine, out pictureFileName, out parameters);
                        
                        //Image file extension
                        string pictureExtension = Path.GetExtension(pictureFileName).ToLower(); //gets the image extension and converts in lower-case                                                
                        if ((pictureExtension == ".png") || (pictureExtension == ".jpg")) 
                        {
                            string pictureUrl = GetResourcesFileURL(pictureFileName);
                            Debug.Log(pictureUrl);                        
                            //Texture2D webTexture = new Texture2D(1, 1);                         //Texture to download from the web
                            yield return StartCoroutine(CoroutineLoadTextureFromWebFile(pictureFileName, pictureUrl));     //Download the texture and wait until it finish
                                                     
                            Color[] pixels = webTexture.GetPixels();    //gets the pixels and assigns them to the array
                            for (int i = 0; i < pixels.Length; i++)     //for all pixels
                            {
                                Color colTemp = pixels[i];  //creates a temporal colour
                                colTemp.r = pixels[i].g;    //assigns the green to the red
                                colTemp.a = pixels[i].r;    //assigns the red to the alpha
                                pixels[i] = colTemp;        //saves the temporal colour modified
                            }
                            webTexture.SetPixels(pixels); //sets the texture with the new pixels
                            material.SetTexture("_BumpMap", webTexture); //sets the material with the loaded texture
                            material.EnableKeyword("_NORMALMAP"); //sets a shader keyword
                            material.SetFloat("_BumpScale", (float)1f); //sets the intensity of the normal map
                                                     
                        }
                        else //other types, not identified
                        {
                            Debug.Log("Texture not support: " + pictureExtension); //shows it the error
                            this.SendMessageToUI("Texture not support: " + pictureExtension);
                        }
                    }
                    else if (lineRead.StartsWith("map_Ks "))
                    {
                        //loads texture from URL
                        string cleanedLine = lineRead.Replace("map_Ks ", "").Trim(); //image direction   

                        //Get Parameters and filename
                        string pictureFileName;
                        string[] parameters;    // TODO use parameters 
                        ExtractDataFrom(cleanedLine, out pictureFileName, out parameters);

                        //Image file extension
                        string pictureExtension = Path.GetExtension(pictureFileName).ToLower(); //gets the image extension and converts in lower-case                                                                                               
                        if ((pictureExtension == ".png") || (pictureExtension == ".jpg")) //image format, png or jpg
                        {
                            string pictureUrl = GetResourcesFileURL(pictureFileName);                            
                            yield return StartCoroutine(CoroutineLoadTextureFromWebFile(pictureFileName, pictureUrl));        //Download the texture and wait until it finish                            
                            material.SetTexture("_MetallicGlossMap", webTexture);                    //sets the material with the loaded texture                           
                            material.EnableKeyword("_METALLICGLOSSMAP");
                            //float temp = Mathf.Exp(-3.0f / 18f);
                            //material.SetFloat("_GlossMapScale", temp); //sets this value in the material   
                            mapKsTextureExist = true;                           
                        }
                        else //other types, not identified
                        {
                            Debug.Log("Texture not support: " + pictureExtension); //shows it the error
                            this.SendMessageToUI("Texture not support: " + pictureExtension);
                        }
                    }
                    else
                    {
                        ProcessMaterialFileLine(lineRead, ref material); //checks the line in secondary function              
                    }                            
                }
                else
                {
                    nextLine = false;
                    if (material != null) //if material exists
                    {                        
                        listOfReadMaterials.Add(material); //adds it to the list of materials
                    }
                }                
            }            
        }       
        loadingMaterialFile = false;        
    }


    private void LoadMaterialFile_fromUrl_Error(string error_message)
    {
        error = true;
        //hom3r.coreLink.Do(new CIOCommand(TIOCommands.ReportError, error_message), Constants.undoNotAllowed);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadError, error_message));
    }
    
    /// <summary>
    /// Get parameters and filename 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="fileName"></param>
    /// <param name="parameters"></param>
    private void ExtractDataFrom(string input, out string fileName, out string[] parameters)
    {
        parameters = input.Split(' ');

        fileName = parameters[parameters.Length - 1];

        Array.Resize<string>(ref parameters, parameters.Length - 1);        
    }

    /// <summary>Loads the material file from local path</summary>
    /// <param name="materialFilePath">file-path of the file</param>
    IEnumerator CoroutineLoadMaterialFile_fromFile(string materialFilePath)
    {
        Hom3rFileReader materialFileToRead = new Hom3rFileReader(materialFilePath); //loads the file
        bool nextLine = true;
        string lineRead;
        Material material = null; //sets up the materials later

        while (nextLine) //if can continue reading the file
        {
            lineRead = materialFileToRead.GetLine(); //reads the line in secondary function                        
            if (lineRead != null) //if is not last line
            {
                ProcessMaterialFileLine(lineRead.Trim(), ref material); //checks the line in secondary function              
            }
            else
            {
                nextLine = false;
                if (material != null) //if material exists
                {
                    listOfReadMaterials.Add(material); //adds it to the list of materials
                }
            }
        }
        materialFileToRead.Close(); //closes the file                
        loadingMaterialFile = false;
        yield return null;
    }

    /// <summary>Start to load the material file</summary>
    /// <param name="materialFileName">file-path or URL of the file</param>
    IEnumerator StartMaterialLoad(string materialFileName)
    {
        string url = GetResourcesFileURL(materialFileName);         //Get file URL
        loadingMaterialFile = true;
        if ((Application.platform == RuntimePlatform.WebGLPlayer) || (Application.platform == RuntimePlatform.Android)) //if we work from internet
        {                        
            yield return StartCoroutine(CoroutineLoadMaterialFile_fromUrl(materialFileName, url)); //loads the file from URL
        }
        else if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.OSXEditor) || (Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.OSXPlayer))
        {
            //if we work from unity
            if (fileOrigin == fileOrigin_type.resources) //if we select a local file
            {
                if (File.Exists(url)) //if exist the file in the direction
                {
                    yield return StartCoroutine(CoroutineLoadMaterialFile_fromFile(url));
                }
            }
            else //if we select a web file
            {
                yield return StartCoroutine(CoroutineLoadMaterialFile_fromUrl(materialFileName, url)); //loads the file from URL
            }
        }
    }

    /// <summary>Get the resource file URL</summary>
    /// <param name="fileName">file name</param>
    /// <returns>file URL</returns>
    string GetResourcesFileURL(string fileName)
    {
        string url = this.GetComponent<_3DFileManager>().GetResourceFileURL(fileName);

        //If the URL has been received from the interface we return it, 
        //if not we try to find it in the same URL that the OBJ file

        if ( url != null) { return url; }
        else if (baseUrlOBJFile != null) { return baseUrlOBJFile + fileName; }
        else { return null; }                    
    }

    /////////////////////
    // OBJ File loader
    /////////////////////

    /// <summary>Create a new object and reset temporary variables</summary>
    /// <param name="_objectName">String that contains the new object name</param>
    void CreateNewEmptyObject(string _objectName)
    {
        //clears the dictionaries for the next object
        triangVertexUvDictionary.Clear();
        triangVertexNormalDictionary.Clear();
        triangVertexUvNormalDictionary.Clear();
        triangVertexDictionary.Clear();

        newReadObject = new COBJFileObject(_objectName);    //Initialize the temp read object, removes "o " and assigns it to object name
    }
        
    void PaintObject(COBJFileObject newReadObject)
    {        
        //int index = listOfReadMaterials.FindIndex(r => r.name == newReadObject.materialName);
        //if ((!loadingMaterialFile) && (index != -1))
        //{
            //SendMessageToUI("Drawing " + newReadObject.name);   //UI Message
            PaintOneObject(newReadObject);       //Paint the object
        //}
        //else
        //{
        //    //Error     
        //}             
    }

    /// <summary>Process OBJ file line</summary>
    /// <param name="row">Line read from the file</param>
    void ProcessOBJFileLine(string row)
    {
        List<string> temp = new List<string>(); //temporal variable for reads lines of triangles
        List<string> pair = new List<string>(); //temporal variable for reads only pairs triangles

        if (row.StartsWith("mtllib ")) //material to load
        {          
            string materialFileName = row.Replace("mtllib ", "").Trim();    //material file path           
            StartMaterialLoad(materialFileName);
        }
        else if (row.StartsWith("o ") || row.StartsWith("g ")) //object name
        {           
            if (numberOfObjectsRead != 0) {                                
                //listOfReadObjects.Add(newReadObject);     //Save the read object in the list, non the first time
                PaintObject(newReadObject);
            }
            numberOfObjectsRead++;
            firstmat = true;                            //control if the same object use various materials
            
            if (row.StartsWith("o "))
            {                                                
                CreateNewEmptyObject(row.Replace("o ", "").Trim());
            }
            else //row.StartsWith("g ")
            {                                                
                CreateNewEmptyObject(row.Replace("g ", "").Trim());
            }
        }
        else if (row.StartsWith("v ")) //vertex
        {
            mymesh.vertices.Add(ConVec3(row.Replace("v ", "").Trim(), file.invertZAxis)); //converts the line to Vec3 and add the coordinates
        }
        else if (row.StartsWith("vn ")) //normal
        {
            mymesh.normals.Add(ConVec3(row.Replace("vn ", "").Trim(), file.invertZAxis)); //converts the line to Vec3 and add the coordinates
        }
        else if (row.StartsWith("vt ")) //texture
        {
            mymesh.uvs.Add(ConVec2(row.Replace("vt ", "").Trim())); //converts the line to Vec2 and add the coordinates
        }
        else if (row.StartsWith("usemtl ")) //name of the material used
        {
            if (firstmat)
            {
                firstmat = false; //the next material will not be the first material
            }
            else //if do not be the first material
            {               
                string saveName = newReadObject.name;
                //listOfReadObjects.Add(newReadObject);       //Save the read object in the list, non the first time                                 
                PaintObject(newReadObject);
                CreateNewEmptyObject(saveName);             //Create next part object 
            }            

            newReadObject.materialName = row.Replace("usemtl ", "").Trim(); //removes "usemtl " and assigns it to object name
        }
        else if (row.StartsWith("f ")) //triangles
        {
            // Check if the polygonal face data come in more of one line
            if (row[row.Length - 1] == '\\')
            {
                row = ReadMultipleLine_PolygonalFace(row);  // Read all the lines
            }

            ProcessOBJFileLine_PolygonalFace(row);  // Process the polygonal face
        }
    }

    /// <summary>Read all the file lines that contains the polygonal face data</summary>
    /// <param name="partialRow">First line</param>
    /// <returns>All the file lines together</returns>
    string ReadMultipleLine_PolygonalFace(string partialRow)
    {        
        List<string> partialRows = new List<string>();
        string fullRow = "";

        //Get all the partial Rows
        while (partialRow[partialRow.Length - 1] == '\\')
        {
            partialRows.Add(partialRow.TrimEnd('\\', ' '));
            partialRow = fileToRead.GetLine().Trim();
        }
        partialRows.Add(partialRow.TrimEnd('\\', ' '));
                
        foreach (string r in partialRows)
        {
            fullRow = fullRow + ' ' + r;
        }

        return fullRow;    
    }

    void ProcessOBJFileLine_PolygonalFace(string row)
    {
        int i;
        List<string> temp = new List<string>(); //temporal variable for reads lines of triangles
        List<string> pair = new List<string>(); //temporal variable for reads only pairs triangles
        int bar = 0; //counter of data taken
        int d1 = 0, d2 = 0, d3 = 0;

        row = row.Replace("f ", "").Trim(); //removes "f "

        temp.AddRange(row.Split(' ')); //removes all spaces and add the data to the temporal list

        if ((newReadObject.triangv.Count + temp.Count) > M) //If the old plus the new vertices is greater than the maximum
        {
            // More than 65000 vertexes Multiple part Object

            if (newReadObject.partNumber == 0) { newReadObject.partNumber = 1; }     //Check if it is the fist part of the object
                                                                                     //listOfReadObjects.Add(newReadObject);                                     //Save the read object in the list, non the first time
            PaintObject(newReadObject);
            //Save info from first part
            int _newPartNumber = newReadObject.partNumber + 1;                      //Save Next part number
            string _newName = newReadObject.name;                                   //Save Next part name                              
            string _materialName = newReadObject.materialName;                      //Save the material name
                                                                                    //Create next part
            CreateNewEmptyObject(_newName);                                                    //Create next part object  
            newReadObject.partNumber = _newPartNumber;                              //Update next part number
            newReadObject.materialName = _materialName;                             //Update the material name
        }

        for (i = 0; i < temp.Count; i++) //the new data
        {
            if (temp[i].IndexOf("//") == -1) //if has not "//"
            {
                if (temp[i].IndexOf("/") == -1) //if has not "/", format X X X X
                {
                    //this format, has not normals, has not texture
                    newReadObject.hasNormals = false;
                    newReadObject.hasUvs = false;

                    newReadObject.identv.Add(temp[i]); //add the identifier of the vertex
                }
                else //if has "/", format X/X or X/X/X
                {
                    //remove all '/' of the line and add it to the possibles identifiers
                    pair.AddRange(temp[i].Split('/'));

                    if ((pair.Count - bar) == 2) //if has two data, format X/X
                    {
                        //this format, has not normals, has texture
                        newReadObject.hasNormals = false;
                        newReadObject.hasUvs = true;

                        newReadObject.identv.Add(pair[i * 2]); //takes the first of two for the vertices
                        newReadObject.identu.Add(pair[(i * 2) + 1]); //takes the second of two for the uvs
                        bar = bar + 2; //sums the number of data taken
                    }
                    else if ((pair.Count - bar) == 3) //if has three data, format X/X/X
                    {
                        //this format, has normals, has texture
                        newReadObject.hasNormals = true;
                        newReadObject.hasUvs = true;
                        newReadObject.identv.Add(pair[i * 3]); //takes the first of three for the vertices
                        newReadObject.identu.Add(pair[(i * 3) + 1]); //takes the second of three for the uvs
                        newReadObject.identn.Add(pair[(i * 3) + 2]); //takes the third of three for the normals
                        bar = bar + 3; //sums the number of data taken
                    }
                }
            }
            else //if has "//", format X//X
            {
                //this format, has normals, has not texture
                newReadObject.hasNormals = true;
                newReadObject.hasUvs = false;

                //remove all '//' of the line and add it to the possibles identifiers
                pair.AddRange(temp[i].Split(new string[] { "//" }, StringSplitOptions.None));
                newReadObject.identv.Add(pair[i * 2]); //takes the first of two for the vertices
                newReadObject.identn.Add(pair[(i * 2) + 1]); //takes the second of two for the normals
            }
        }

        newReadObject.quantity = newReadObject.identv.Count - newReadObject.account; //checks how many vertices are news

        if (newReadObject.hasNormals == true) //the file has normals
        {
            if (newReadObject.hasUvs == true) //the file has uvs and normals, format X/X/X
            {
                for (i = newReadObject.account; i < newReadObject.identv.Count; i++) //the news
                {
                    d1 = int.Parse(newReadObject.identv[i]) - 1; //takes the new vertex and subtract 1
                    d2 = int.Parse(newReadObject.identu[i]) - 1; //takes the new uv and subtract 1
                    d3 = int.Parse(newReadObject.identn[i]) - 1; //takes the new normal and subtract 1

                    if (d1 < 0) { d1 = d1 + mymesh.vertices.Count + 1; }    // If index are negative in the file we have to convert to positive
                    if (d2 < 0) { d2 = d2 + mymesh.uvs.Count + 1; }         // If index are negative in the file we have to convert to positive
                    if (d3 < 0) { d3 = d3 + mymesh.normals.Count + 1; }     // If index are negative in the file we have to convert to positive

                    CheckVertUvNormDictionary(newReadObject, d1, d2, d3); //checks if exists the couple vertex/uv/normal
                }
            }
            else //the file has not uvs but has normals, format X//X
            {
                for (i = newReadObject.account; i < newReadObject.identv.Count; i++) //the news
                {
                    d1 = int.Parse(newReadObject.identv[i]) - 1; //takes the new vertex and subtract 1
                    d2 = int.Parse(newReadObject.identn[i]) - 1; //takes the new normal and subtract 1

                    if (d1 < 0) { d1 = d1 + mymesh.vertices.Count + 1; }    // If index are negative in the file we have to convert to positive
                    if (d2 < 0) { d2 = d2 + mymesh.normals.Count + 1; }     // If index are negative in the file we have to convert to positive

                    CheckVertNormDictionary(newReadObject, d1, d2); //checks if exists the couple vertex/normal
                }
            }
        }
        else //the file has not normals
        {
            if (newReadObject.hasUvs == true) //the file has uvs but has not normals, format X/X
            {
                for (i = newReadObject.account; i < newReadObject.identv.Count; i++) //the news
                {
                    d1 = int.Parse(newReadObject.identv[i]) - 1;            //takes the new vertex and subtract 1
                    d2 = int.Parse(newReadObject.identu[i]) - 1;            //takes the new uv and subtract 1

                    if (d1 < 0) { d1 = d1 + mymesh.vertices.Count + 1; }    // If index are negative in the file we have to convert to positive
                    if (d2 < 0) { d2 = d2 + mymesh.uvs.Count + 1; }         // If index are negative in the file we have to convert to positive

                    CheckVertUvDictionary(newReadObject, d1, d2);           //checks if exists the couple vertex/uv
                }
            }
            else //the file has not uvs or normals, format X X X X
            {
                for (i = newReadObject.account; i < newReadObject.identv.Count; i++) //the news
                {
                    if (Regex.IsMatch(newReadObject.identv[i], @"^\d+$"))
                    {
                        d1 = int.Parse(newReadObject.identv[i]) - 1;            //takes the new vertex and subtract 1
                        if (d1 < 0) { d1 = d1 + mymesh.vertices.Count + 1; }    // If index are negative in the file we have to convert to positive

                        CheckVertDictionary(newReadObject, d1);                 //checks if exists the vertex
                    }
                    else
                    {
                        Debug.LogError("OBJLoader error");
                        Debug.Log(newReadObject.name);
                        Debug.Log(i);
                        Debug.Log(newReadObject.identv[i]);
                    }
                }
            }
        }

        //for each square, there is four vertices, sets two triangles
        //for twenty vertices, sets eighteen triangles
        //etc..
        for (i = newReadObject.account; i < (newReadObject.aIden.Count - 2); i++)
        {
            //adds to myMesh class one number for each vertex
            newReadObject.triangles.Add(newReadObject.aIden[newReadObject.account]);
            newReadObject.triangles.Add(newReadObject.aIden[i + 1]);
            newReadObject.triangles.Add(newReadObject.aIden[i + 2]);
        }

        newReadObject.account += newReadObject.quantity; //increases the number of numbers identified
    }

    /// <summary>Reads the file line to line</summary>
    void LoadOBJFile()
    {        
        bool nextLine = true;
        string lineRead;

        numberOfObjectsRead = 0;                        //Initialize the number of objects read from a file

        while (nextLine && !error)                      //if can continue reading the file
        {
            lineRead = fileToRead.GetLine();            //reads the line in secondary function            
            if (lineRead != null)                       //if is not last line
            {
                ProcessOBJFileLine(lineRead.Trim());    //checks the line in secondary function
            }
            else //if last line
            {
                nextLine = false;                       //do not continues reading the file
                
                //Save the last object read, in the list, if it exist
                if (newReadObject.triangv.Count > 0)
                {
                    //listOfReadObjects.Add(newReadObject);
                    PaintObject(newReadObject);                 //Paint the last object read, if it exist
                }
                newReadObject = null;
            }
        }
        //SendMessageToUI("Drawing " + file.fileName + "...");
        //StartCoroutine(CoroutinePaint());    //Paint the objects in the scene when all the object file lines have been read 
        Reset();                //Clean for next execution                
        FinishOBJDownload();    //Indicate that the download has finished without error 
    }

    /// <summary>Loads the OBJ file from local path</summary>
    /// <param name="filePath">OBJ file path</param>
    /// <returns></returns>
    IEnumerator LoadOBJFile_fromFile(string filePath)
    {
        fileToRead = new Hom3rFileReader(filePath); //file read from the direction passed as parameter        
        LoadOBJFile(); //loads the file        
        fileToRead.Close(); //closes the file
        yield return null;
    }

    /// <summary>loads the OBJ file from URL path</summary>
    /// <param name="url">URL path of the OBJ file</param>
    /// <param name="version"></param>
    /// <returns></returns>
    IEnumerator LoadOBJFile_fromUrl(string url)
    {
        // WWW fileWWW = new WWW(url);     //Download File        
        UnityWebRequest fileWWW = UnityWebRequest.Get(url);
        fileWWW.SendWebRequest();

        //Show downloaded progress
        while (!fileWWW.isDone)
        {
            SendMessageToUI("Downloading " + file.fileName + " file : " + Mathf.Round(fileWWW.downloadProgress * 100.0f).ToString() + "%");
            yield return new WaitForSeconds(0.1f);
        }
        
        //Check errors
        if (fileWWW.isNetworkError || fileWWW.isHttpError) {
            error = true;
            FinishOBJDownload();                           //Finished with error
            //hom3r.coreLink.Do(new CIOCommand(TIOCommands.ReportError, "Error downloading object file " + file.fileName), Constants.undoNotAllowed);
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadError, "Error downloading .obj file " + file.fileName));
            Debug.LogError("Error downloading " + file.fileName + " : " + fileWWW.error); //shows the error
        } else {
            SendMessageToUI("Downloading " + file.fileName + " file : 100%");   //UI Message
            string readingMessage = "Reading " + file.fileName + "...";
            SendMessageToUI(readingMessage);                 //UI Message

            fileToRead = new Hom3rFileReader(fileWWW);     //file read from the direction passed as parameter            
            //LoadOBJFile(); //loads the file

            // Load OBJ File                                  
            bool nextLine = true;
            string lineRead;
            numberOfObjectsRead = 0;                        //Initialize the number of objects read from a file

            while (nextLine /*&& !error*/)                      //if can continue reading the file
            {                
                lineRead = fileToRead.GetLine();            //reads the line in secondary function            
                if (lineRead != null)                       //if is not last line
                {
                    lineRead = lineRead.Trim();             //Clean spaces
                    if (lineRead.StartsWith("mtllib "))     //Material to load
                    {
                        string materialFileName = lineRead.Replace("mtllib ", "").Trim();    //material file path           
                        Debug.Log("Total allocated memory before read material file : " + System.GC.GetTotalMemory(true) + " bytes");
                        yield return StartCoroutine(StartMaterialLoad(materialFileName));   //Wait until the material file has been read
                        Debug.Log("Total allocated memory after read material file : " + System.GC.GetTotalMemory(true) + " bytes");
                    }
                    else
                    {
                        ProcessOBJFileLine(lineRead);    //checks the line in secondary function
                    }                    
                }
                else //if last line
                {
                    nextLine = false;                       //Not continue reading the file
                    
                    if (newReadObject.triangv.Count > 0) {
                        //listOfReadObjects.Add(newReadObject);
                        PaintObject(newReadObject);                 //Paint the last object read, if it exist
                    }
                    newReadObject.Clear();
                    newReadObject = null;                    
                }
            }
            //SendMessageToUI("Drawing " + file.fileName + "...");
            //StartCoroutine(CoroutinePaint());    //Paint the objects in the scene when all the object file lines have been read 
            /////
            Reset();                //Clean for next execution                
            FinishOBJDownload();    //Indicate that the download has finished without error                         
        }        
    }

    /// <summary>Start the OBJ file load</summary>
    /// <param name="objectParentName"></param>
    private void StartOBJLoad()
    {                
        if ((Application.platform == RuntimePlatform.WebGLPlayer) || Application.platform==RuntimePlatform.Android)
        {            
            StartCoroutine(LoadOBJFile_fromUrl(fullFileUrl)); //loads the file from URL
        }
        else if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.OSXEditor) || (Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.OSXPlayer))
        {
            
            if (fileOrigin == fileOrigin_type.resources) //if we select a local file
            {         
                StartCoroutine(LoadOBJFile_fromFile(fullFilePath)); //loads the file from file
            }
            else //if we select a web file
            {                
                StartCoroutine(LoadOBJFile_fromUrl(fullFileUrl)); //loads the file from URL
            }
        }        
    }
    
    /// <summary>Get the object file base URL</summary>
    private string GetOBJFileBaseURL(string _fullFileUrl)
    {
        string baseUrlOBJFile;
        int index = _fullFileUrl.IndexOf(file.fileName);     //Check if the URL contains the object file name
        if (index != -1) {            
            baseUrlOBJFile = _fullFileUrl.Substring(0, _fullFileUrl.LastIndexOf("/") + 1);       //Save file URL path 
        }
        else
        {
            baseUrlOBJFile = null;      //If not, is not possible to know the base URL of the file
        }
        return baseUrlOBJFile;
    }

    public void StartOBJDownload(C3DFileData _file, GameObject productRoot)
    {
        Debug.Log("Total allocated memory before read file : " + System.GC.GetTotalMemory(true) + " bytes");
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        file = _file;                           //Store locally the file info
        
        fileOrigin = fileOrigin_type.web;                   // OBJ loader script implements read from file and from URL. We are going to use URL download methods
        fullFileUrl = file.fileUrl;                         // Save file URL 
        baseUrlOBJFile = GetOBJFileBaseURL(fullFileUrl);    // Get the object file base URL
        fullFilePath = file.fileUrl;                                                    //Save file URL 
        pathFilePath = file.fileUrl.Substring(0, file.fileUrl.LastIndexOf("/") + 1);    //Save file URL path 
        
        string objectName;
        if (file.fileOrigin == T3DFileOrigin.productModelJSON)  { objectName = file.fileName;  }  // Get Object name from JSON model data
        else { objectName = file.fileName.Substring(0, file.fileName.Length - 4); }                               // Get Object name from 3D file name

        defaultParent = new GameObject(objectName);                 // Create a new object. It's going to be the parent of all the files contains in this file
        defaultParent.transform.parent = productRoot.transform;     // Set this object as product root child

        StartOBJLoad();                                             // Start file downloading and paint 
    }

    private void FinishOBJDownload()
    {
        //if (!error)
        //{
            stopwatch.Stop();
            Debug.Log("Time taken: " + (stopwatch.Elapsed));
            stopwatch.Reset();            
            this.GetComponent<_3DFileManager>().ProcessAfterOBJLoad(file, defaultParent);
        //}       
        Debug.Log("Total allocated memory after paint objects : " + System.GC.GetTotalMemory(true) + " bytes");        
        this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);
        file.Clear();
    }

    ////////////////////////
    //  Auxiliary Methods
    ////////////////////////

    /// <summary> Converts string to colour</summary>
    /// <param name="str"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>    
    Color ConColor(string str, float scalar = 1.0f) //with scalar by default if do not parameters are passed
    {
        string[] temp = str.Split(' '); //converts the string in array of string

        float Kr = float.Parse(temp[0], CultureInfo.InvariantCulture) * scalar; //converts the string in float and scale it
        float Kg = float.Parse(temp[1], CultureInfo.InvariantCulture) * scalar; //converts the string in float and scale it
        float Kb = float.Parse(temp[2], CultureInfo.InvariantCulture) * scalar; //converts the string in float and scale it

        Color color = new Color(Kr, Kg, Kb); //sets the colour

        return color; //return the colour
    }

    /// <summary>Converter string to vector3 </summary>
    /// <param name="str"></param>
    /// <returns></returns>    
    Vector3 ConVec3(string str, bool _invertZAxis = false)
    {
        string[] vect = str.Split(null); //converts the string in array of string
        Vector3 v = new Vector3(); //saves the coordinates
        
        float.TryParse(vect[0], NumberStyles.Number, CultureInfo.InvariantCulture, out v.x); //converts the string in float, coordinate x
        float.TryParse(vect[1], NumberStyles.Number, CultureInfo.InvariantCulture, out v.y); //converts the string in float, coordinate y
        float.TryParse(vect[2], NumberStyles.Number, CultureInfo.InvariantCulture, out v.z); //converts the string in float, coordinate z

        if (_invertZAxis) {
            v.x = -v.x;
            v.z = -v.z;
        }

        return v; //returns the coordinates
    }

    /// <summary>Converter string to vector2</summary>
    /// <param name="str"></param>
    /// <returns></returns>    
    Vector2 ConVec2(string str)
    {
        string[] vect = str.Split(null); //converts the string in array of string
        Vector2 v = new Vector2(); //saves the coordinates

        float.TryParse(vect[0], NumberStyles.Number, CultureInfo.InvariantCulture, out v.x); //converts the string in float, coordinate x
        float.TryParse(vect[1], NumberStyles.Number, CultureInfo.InvariantCulture, out v.y); //converts the string in float, coordinate y

        return v; //returns the coordinates
    }

    /// <summary>Checks if exists one vertex, its uv and its normal</summary>
    /// <param name="vert"></param>
    /// <param name="uv"></param>
    /// <param name="norm"></param>
    void CheckVertUvNormDictionary(COBJFileObject readObject, int vert, int uv, int norm)
    {
        TVertexUvNormal temp = new TVertexUvNormal(vert, uv, norm); //creates a new couple
        int position;

        if (!triangVertexUvNormalDictionary.TryGetValue(temp, out position)) //checks if exists the couple
        { //if do not exists
            position = readObject.triangv.Count; //assigns the last position

            triangVertexUvNormalDictionary.Add(temp, position); //adds it to the dictionary

            readObject.triangv.Add(vert); //adds it to the array of vertices
            readObject.triangu.Add(uv); //adds it to the array of uvs
            readObject.triangn.Add(norm); //adds it to the array of normals
        } //if exists return his position

        readObject.aIden.Add(position); //adds the position to the list        
    }

    /// <summary>Checks if exists one vertex and its normal</summary>
    /// <param name="readObject"></param>
    /// <param name="vert"></param>
    /// <param name="norm"></param>    
    void CheckVertNormDictionary(COBJFileObject readObject, int vert, int norm)
    {
        TVertexNormal temp = new TVertexNormal(vert, norm); //creates a new couple
        int position;

        if (!triangVertexNormalDictionary.TryGetValue(temp, out position)) //checks if exists the couple
        { //if do not exists
            position = readObject.triangv.Count; //assigns the last position

            triangVertexNormalDictionary.Add(temp, position); //adds it to the dictionary

            readObject.triangv.Add(vert); //adds it to the array of vertices
            readObject.triangn.Add(norm); //adds it to the array of normals
        } //if exists return his position

        readObject.aIden.Add(position); //adds the position to the list
    }

    /// <summary>Checks if exists one vertex and his uv</summary>
    /// <param name="readObject"></param>
    /// <param name="vert"></param>
    /// <param name="uv"></param>    
    void CheckVertUvDictionary(COBJFileObject readObject, int vert, int uv)
    {
        TVertexUv temp = new TVertexUv(vert, uv); //creates a new couple
        int position;

        if (!triangVertexUvDictionary.TryGetValue(temp, out position)) //checks if exists the couple
        { //if do not exists
            position = readObject.triangv.Count; //assigns the last position

            triangVertexUvDictionary.Add(temp, position); //adds it to the dictionary

            readObject.triangv.Add(vert); //adds it to the array of vertices
            readObject.triangu.Add(uv); //adds it to the array of uvs
        } //if exists return his position

        readObject.aIden.Add(position); //adds the position to the list
    }

    /// <summary>Checks if exists one vertex</summary>
    /// <param name="readObject"></param>
    /// <param name="vert"></param>
    void CheckVertDictionary(COBJFileObject readObject, int vert)
    {
        int position;

        if (!triangVertexDictionary.TryGetValue(vert, out position)) //checks if exists the couple
        { //if do not exists
            position = readObject.triangv.Count; //assigns the last position

            triangVertexDictionary.Add(vert, position); //adds it to the dictionary

            readObject.triangv.Add(vert); //adds it to the array of vertices            
        } //if exists return his position

        readObject.aIden.Add(position); //adds the position to the list
    }

    /////////////////
    // UI Messages
    ////////////////    
    void SendCleanMessageToUI()
    {
        SendMessageToUI("", 0.0f);
    }
    void SendMessageToUI(string _message)
    {
        SendMessageToUI(_message, 0.0f);
    }
    void SendMessageToUI(string _message, float _time)
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ShowMessage, _message, _time));
        Debug.Log(_message);
    }
}