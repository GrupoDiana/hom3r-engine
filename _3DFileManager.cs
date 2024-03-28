using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public enum T3DFileState { idle, loading, loaded, error };
public enum T3DFileOrigin { productModelJSON, url };

/// <summary> Struct to store the asset that has to be load</summary>
public class C3DFileData : CGeometryFileData
{
    public uint fileID;    
    public T3DFileOrigin fileOrigin;
    public T3DFileState fileState;
    
    public C3DFileData(uint _fileID, string _fileName, string _fileUrl, string _fileType) {
        this.fileID = _fileID;
        this.fileName = _fileName;
        this.fileUrl = _fileUrl;
        this.fileType = _fileType;
    }
    public C3DFileData(uint _fileID, CGeometryFileData _fileData)
    {
        this.Clear();
        this.fileID = _fileID;                
        this.fileName = _fileData.fileName;
        this.fileUrl = _fileData.fileUrl;
        this.fileType = _fileData.fileType;
        this.version = _fileData.version;
        this.crc = _fileData.crc;
        this.assetName = _fileData.assetName;
        this.active = _fileData.active;
        this.invertZAxis = _fileData.invertZAxis;
        this.position_x = _fileData.position_x;
        this.position_y = _fileData.position_y;
        this.position_z = _fileData.position_z;
        this.rotation_w = _fileData.rotation_w;
        this.rotation_x = _fileData.rotation_x;
        this.rotation_y = _fileData.rotation_y;
        this.rotation_z = _fileData.rotation_z;
        this.scale_x = _fileData.scale_x;
        this.scale_y = _fileData.scale_y;
        this.scale_z = _fileData.scale_z;
    }

    public void Clear()
    {
        this.fileID = 0;
        this.fileName = null;
        this.fileUrl = null;
        this.fileType = null;
        this.version = 0;
        this.crc = null;
        this.assetName = null;
        this.active = false;
        this.position_x = 0d;
        this.position_y = 0d;
        this.position_z = 0d;
        this.rotation_w = 0d;
        this.rotation_x = 0d;
        this.rotation_y = 0d;
        this.rotation_z = 0d;
        this.fileOrigin = T3DFileOrigin.productModelJSON;
        this.fileState = T3DFileState.idle;        
    }
}

public class _3DFileManager : MonoBehaviour {

    bool downloadingFile;                       // To store if we are downloading a file or not
    bool coroutineStarted;                      // To know when the corouine is working
    //public GameObject goScript { get; set; }    // To store a link to the object that contain the scripts

    Queue <C3DFileData> _3DFilesQueue;
    List<CResourceFileData> resourceFilesList;
    //List <C3DFileData> ResourceFilesList;

    private void Awake()
    {
        _3DFilesQueue = new Queue<C3DFileData>();
        resourceFilesList = new List<CResourceFileData>();
        downloadingFile = false;
        coroutineStarted = false;
    }
	
            
    /// <summary>Load 3D files</summary>
    /// <param name="_3DFilesToDownload">List of Files to download</param>
    public void Load3DFiles(List<C3DFileData> _3DFilesToDownload, List<CResourceFileData> _ResourcesFilesToDownload)
    {
        resourceFilesList.AddRange(_ResourcesFilesToDownload);              //Store into resources files list
        //Enqueue 3D file and start download
        foreach (C3DFileData file in _3DFilesToDownload)    { _3DFilesQueue.Enqueue(file); }        
        if (!coroutineStarted)
        {
            coroutineStarted = true;
            StartCoroutine(CoroutineLoad3DFiles());
        }
    }

    /// <summary>Load 3D file one by one</summary>
    IEnumerator CoroutineLoad3DFiles()
    {
        C3DFileData file;
        while (_3DFilesQueue.Count > 0)
        {
            file = _3DFilesQueue.Dequeue();
            switch (file.fileType)
            {
                case "assetbundle":
                    StartAssetBundleDownload(file);
                    break;
                case "obj":
                    StartOBJDownload(file);
                    break;                
                default:
                    SendMessageToUI("File type does't support by HOM3R");
                    break;
            }
            //We download one by one, so we wait until this file download have been finished
            while (downloadingFile) { yield return new WaitForSeconds(1.0f); }  
        }
        coroutineStarted = false;
    }
            
    /////////////////////////////////////
    // Actions to start files Download
    /////////////////////////////////////

    /// <summary>Reset product root scale</summary>
    private void ResetProductScale()
    {
        GameObject productRoot = hom3r.quickLinks._3DModelRoot;
        productRoot.transform.localScale = new Vector3(1f, 1f, 1f);
    }
    
    /// <summary>Report to Model manager that one file has started its download</summary>
    /// <param name="fileID">File ID</param>
    private void SetFileDownloadStarted(uint fileID)
    {
        ResetProductScale();
        downloadingFile = true;
        this.GetComponent<ModelManager>().SetFileDownloadStarted(fileID);    //Notify to the model management
    }

    /// <summary>Start Assetbundle file download</summary>
    /// <param name="file">File to be downloaded</param>
    private void StartAssetBundleDownload(C3DFileData file)
    {
        if (!downloadingFile)
        {
            SetFileDownloadStarted(file.fileID);
            this.GetComponent<AssetBundleLoader>().ProcessAssetBundleDownload(file);
        }
    }

    /// <summary>Start OBJ file download</summary>
    /// <param name="file">File to be downloaded</param>
    private void StartOBJDownload(C3DFileData file)
    {
        if (!downloadingFile)
        {
            SetFileDownloadStarted(file.fileID);
            SendMessageToUI("Downloading 3D model from " + file.fileUrl, 1f);            
            this.GetComponent<OBJLoader>().StartOBJDownload(file, hom3r.quickLinks._3DModelRoot);
        }
    }

    ///////////////////////////////////////////////
    // Actions after finished the file download
    ///////////////////////////////////////////////
    
    /// <summary>Actions after assetbundle download from json product model</summary>
    /// <param name="asset">asset data read from json</param>
    /// <param name="loadedGameObject">Gameobject created by the aseetbundleloader</param>
    public void ProcessAfterAssetBundleLoad(C3DFileData file, GameObject loadedGameObject)
    {
        if (file.fileOrigin == T3DFileOrigin.productModelJSON)
        {
            EmplaceAsset(file, loadedGameObject);           //Make actions after load the assetBundle        
        }
        else
        {
            this.GetComponent<ModelManager>().Add3DFileToProductModel(file.fileID, loadedGameObject);        
        }
        AddComponents_and_Others(loadedGameObject);     //Make actions after load the assetBundle       
    }

    ///// <summary>Actions after assetbundle download without json product model</summary>
    ///// <param name="fileID">file ID</param>
    ///// <param name="loadedGameObject">Gameobject created by the aseetbundleloader</param>
    //public void ProcessAfterAssetBundleLoad(uint fileID, GameObject loadedGameObject)
    //{       
    //    this.GetComponent<ModelManager>().Add3DFileToProductModel(fileID, loadedGameObject);
    //    AddComponents_and_Others(loadedGameObject);     //Make actions after load the assetBundle        
    //}
   
    /// <summary>Actions after OBJ download without json product model</summary>
    /// <param name="fileID">file ID</param>
    /// <param name="loadedGameObject">Gameobject created by the OBJloader</param>
    public void ProcessAfterOBJLoad(C3DFileData file, GameObject loadedGameObject)
    {        
        if (file.fileOrigin == T3DFileOrigin.productModelJSON)
        {
            EmplaceAsset(file, loadedGameObject);            //Make actions after loading the obj file         
        }
        else
        {
            this.GetComponent<ModelManager>().Add3DFileToProductModel(file.fileID, loadedGameObject);            
        }
        AddComponents_and_Others(loadedGameObject);     //Make actions after loading the obj file  
    }

    /// <summary>Report Model manager that one file has finished its download</summary>
    /// <param name="fileID">file ID</param>
    /// <param name="error">error result</param>
    public void SetFileDownloadFinished(uint fileID, bool error)
    {
        if (downloadingFile)
        {
            //Error managemenet
            T3DFileState downloadResult;
            if (error) { downloadResult = T3DFileState.error; }
            else { downloadResult = T3DFileState.loaded; }
            
            //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadFinished));
            this.GetComponent<ModelManager>().SetFileDownloadFinished(fileID, downloadResult);       //Notify that we are finish the download of this file
            downloadingFile = false;
        }
    }

  
   
    /// <summary>Empace a Asset, loaded from a file, in the position and rotation read from the model.</summary>
    /// <param name="_assetName">Id of the asset</param>
    /// <param name="_object">Object that represent the asset when has been loaded</param>
    private void EmplaceAsset(C3DFileData geometryData, GameObject _object)
    {
        ////Find the asset in de Model by it's id.               
        float position_x = (float)geometryData.position_x;
        float position_y = (float)geometryData.position_y;
        float position_z = (float)geometryData.position_z;
        float rotation_w = (float)geometryData.rotation_w;
        float rotation_x = (float)geometryData.rotation_x;
        float rotation_y = (float)geometryData.rotation_y;
        float rotation_z = (float)geometryData.rotation_z;

        //Emplace the object
        _object.transform.position = new Vector3(position_x, position_y, position_z);                   //Move the object        
        _object.transform.rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);    //Rotate the object

        // Aplly scale
        Vector3 newScale = new Vector3((float)geometryData.scale_x, (float)geometryData.scale_y, (float)geometryData.scale_z);
        if (newScale != Vector3.zero)
        {
            _object.transform.localScale = newScale;
        }
    }

    /// <summary>Go through all the objects loaded and add to them: scripts, colliders, ID.</summary>
    /// <param name="loadedRoot">Gameobject parent of all the object loaded</param>
    public void AddComponents_and_Others(GameObject loadedRoot)
    {
        GameObject productRoot = hom3r.quickLinks._3DModelRoot;
        if (loadedRoot.transform.parent != productRoot.transform)
        {
            loadedRoot.transform.parent = productRoot.transform;    //Establish this object as child of the product model main object
        }                       
        AddComponents_and_Others_Recursively(loadedRoot, productRoot);  //In a recursive way we go through all the objects loaded and add to them: scripts and colliders.
    }

    /// <summary>Recursively runs through the 3D model and add componets to the objects.</summary>
    /// <param name="obj">Gameobject parent</param>
    void AddComponents_and_Others_Recursively(GameObject obj, GameObject productRoot)
    {
        //We add components if the object is an area, and its happen if it has a mesh.
        if (obj.GetComponent<Renderer>())
        {
            if ((obj.GetComponent<Renderer>() != null))
            {
                obj.AddComponent<ObjectStateManager>();                         //Add Scripts                
                obj.AddComponent<MeshCollider>();                               //Add collider to be able to do raycasting and Convert the meshcollider to convex mesh                
                obj.layer = productRoot.layer;                                  //Add layer to the objects                
                //obj.GetComponent<ObjectState_Script>().InitializeAreaID();      //Initilize the var                            
                this.GetComponent<ModelManager>().SetAreaID_And_GameObject(obj.name, obj);   //Add its ID to the object and Add a pointer to its dictionary                 
            }
        }
        //Call again to itself with one child
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject newChild = obj.transform.GetChild(i).gameObject;
            AddComponents_and_Others_Recursively(newChild, productRoot);
        }
    }

    //////////////////
    // Files Manager
    ///////////////// 
 
    /// <summary>Get the resource file url</summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetResourceFileURL(string fileName)
    {
        int index = resourceFilesList.FindIndex(r=> r.fileName == fileName);
        //If the file URL has been received from the interface we return it,    
        if (index != -1) { return resourceFilesList[index].fileUrl; }
        else { return null; }
    }

    /// <summary>
    /// Check if it is a valid 3D file
    /// </summary>
    /// <param name="fileType"></param>
    /// <returns></returns>
    public bool isValid3DFile(string fileType)
    {
        bool control = false;
        switch (fileType)
        {
            case "assetbundle":
                control = true;
                break;
            case "obj":
                control = true;
                break;            
            default:
                control = false;
                break;
        }
        return control;
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
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent._3DFileManager_ShowMessage, _message, _time));
        Debug.Log(_message);
    }   
}
