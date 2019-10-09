using UnityEngine;
using System.Collections;
//using LitJson;
using System.Collections.Generic;
using UnityEngine.Networking;



public class ModelManager : MonoBehaviour {
    
    //////////////////
    ///Private vars //
    ////////////////// 
    
    private CProductModel           productModel;           //Store the product Model    
    private bool                    productModelLoaded;     //Store if the model has been loaded correctly        
    private List<C3DFileData>       list3DFiles;            //Var to store the list of 3D files        
    private List<CResourceFileData> listResourceFiles;      //To store the list of resource files (material and textures files)
    private Bounds modelBoundingBox;                        // Store the 3D model bounding box
    private bool modelBoundingBoxInit;                      // If the 3D model bounding box has been initiated or not
    private float modelScale;                               // Store the model scale


    //Others    
    private string      explosionXML_url;   // Store the URL of the explosionXML
    private string      _3DFilePath;        // Store model path when its a local file
    private uint        files3DID;          // Last 3D file ID
    
    void Awake()
    {
        InitVars();
    }

    /// <summary>
    /// Initialize all the variables
    /// </summary>
    private void InitVars()
    {
        productModel = new CProductModel();          // Initialize the product model var
        productModelLoaded = false;                        // Initialize to false, the model hasn't be loaded at this moment.             
        list3DFiles = new List<C3DFileData>();        // Initialize List of files to load        
        listResourceFiles = new List<CResourceFileData>();
        files3DID = 0;                            // Store 3D files ID
        _3DFilePath = "";                           // Store the 3D file path when we are on the Editor   
        modelBoundingBoxInit = false;                       //
        modelScale = 1.0f;                                  // Initialization of model scale
    }

    void Start()
    {        
        StartModelManagement();                         // Start        
    }


    /// <summary>Initialization the class and start the model load in case</summary>
    private void StartModelManagement()
    {
        this.SendMessageToUI("HOM3R Ready!!! Waiting Product Model...", 0.0f);  //Update UI - Message telling that we are waiting the product model.             
    }

    ////////////////////////
    // Load Model Methods //
    ////////////////////////

    /// <summary>Load Product Models and download the 3d files indicate</summary>  
    ///<param name="productModel_url">URL of the product model</param>    
    ///<param name="_explosionXML_url">URL of the product explosion model</param>    
    public void LoadProductModels(string productModel_url, string _explosionXML_url)
    {
        if ((!productModelLoaded) && (productModel_url != ""))
        {
            explosionXML_url = _explosionXML_url;
            _3DFilePath = GetPathIfLocalFile(productModel_url);
            StartCoroutine(CoroutineLoadProductModel(productModel_url));    //Download and Load the product model from web-server             
        }
        else
        {
            string errorMessage = "ERROR: Product model incorrect!!";
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ModelLoadError, errorMessage));
            hom3r.state.productModelLoaded = false;
            SendMessageToUI(errorMessage, 5.0f);
        }
    }

    /// <summary> Coroutine to load Product Model</summary>
    /// <param name="productModelUrl">Product model URL</param>    
    IEnumerator CoroutineLoadProductModel(string productModelUrl)
    {        
        //WWW request = new WWW(productModelUrl);
        UnityWebRequest fileWWW = UnityWebRequest.Get(productModelUrl);
        fileWWW.SendWebRequest();

        //Show downloaded progress
        while (!fileWWW.isDone)
        {            
            SendMessageToUI("Loading Model from DataBase: " + Mathf.Round(fileWWW.downloadProgress * 100.0f).ToString() + "%", 0.0f);
            yield return new WaitForSeconds(0.1f);
        }


        //Has been download correctly?
        if (fileWWW.isNetworkError || fileWWW.isHttpError)
        {
            string errorMessage = "Error: Product Model cannot be download from the URL specified." + fileWWW.error;
            Debug.LogError("WWW error: " + productModelUrl + " : " + fileWWW.error);
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ModelLoadError, errorMessage));
            hom3r.state.productModelLoaded = false;
            SendMessageToUI(errorMessage);
        }
        else
        {
            productModel = JsonUtility.FromJson<CProductModel>(fileWWW.downloadHandler.text);   //Load product model from the JSON file downloaded
            if (productModel.isInit())
            {
                productModelLoaded = productModel.SetupDictionaries();  //Initialize the class with the data uploaded
                if (productModelLoaded)
                {
                    SendMessageToUI("hom3r: Product model loaded correctly!!!", 5.0f);
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ProductModelLoadSuccess));
                    hom3r.state.productModelLoaded = true;
                    Load3DFilesFromProductModel();      //Download the 3D files                    
                }
                else
                {
                    string errorMessage = "ERROR: Product model incorrect!!!";
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ModelLoadError, errorMessage));
                    hom3r.state.productModelLoaded = false;
                    SendMessageToUI(errorMessage, 5.0f);
                }
            }
            if ((productModel == null) || !productModelLoaded)
            {
                //Send error to Application and UI
                string errorMessage = "Error: The product model has a bad format";
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ModelLoadError, errorMessage));
                hom3r.state.productModelLoaded = false;
                SendMessageToUI(errorMessage);
            }
        }
    }
       
    /// <summary>Load explosion model</summary>
    /// <param name="url">Product explosion model</param>
    private void LoadExplosionModel(string url)
    {
        //if ((url != null) && (url != ""))
        //{
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ReadyToLoadExplosionModel, explosionXML_url));
        //} 
    }


    /// <summary>Load a 3D files and create the product model from it</summary>
    /// <param name="listOf3DFiles"></param>
    public void Load3DFilesWithOutProductModel(List<CIO3DFileData> listOf3DFiles)
    {
        //Load3DFilesFromURL(listOf3DFiles);      //Download the 3D files


        List<C3DFileData> list_3DFilesToDownload = new List<C3DFileData>();
        List<CResourceFileData> list_ResourcesFilesToDownload = new List<CResourceFileData>();

        foreach (CIO3DFileData file in listOf3DFiles)
        {
            //Classify each file
            if (hom3r.quickLinks.scriptsObject.GetComponent<_3DFileManager>().isValid3DFile(file.fileType))
            {
                C3DFileData newFile = new C3DFileData(GetFileID(), file.fileName, file.fileURL, file.fileType);
                newFile.fileOrigin = T3DFileOrigin.url;
                list_3DFilesToDownload.Add(newFile);
            }else
            {
                CResourceFileData newFile = new CResourceFileData(file.fileName, file.fileURL, file.fileType);
                list_ResourcesFilesToDownload.Add(newFile);
            }                        
        }
        list3DFiles.AddRange(list_3DFilesToDownload);                                                   //Add to the manager file list
        listResourceFiles.AddRange(list_ResourcesFilesToDownload);
        SetBlockUserInterfaceDuringFileLoading();                                                       //Block Interface during loading        
        this.GetComponent<_3DFileManager>().Load3DFiles(list_3DFilesToDownload, listResourceFiles); 
    }

    ////////////////////////////////
    // 3D File Management Methods 
    ////////////////////////////////
    /// <summary>Return file info by is id</summary>
    /// <param name="fileID"></param>
    /// <returns></returns>
    private C3DFileData Get3DFileData(uint fileID) {
        int index = list3DFiles.FindIndex(r => r.fileID == fileID);
        if (index != -1) { return list3DFiles[index]; }
        else return null;
    }

    /// <summary>Report to the model manager that one file download has started</summary>
    /// <param name="fileID">ID of the file whose download has started</param>
    public void SetFileDownloadStarted(uint fileID)
    {
        int index = list3DFiles.FindIndex(r => r.fileID == fileID);
        if (list3DFiles[index].fileState == T3DFileState.idle)
        {
            list3DFiles[index].fileState = T3DFileState.loading;            
        }
    }
    
    /// <summary>Report to the model manager that one file download has finished</summary>
    /// <param name="fileID">ID of the file whose download has finished</param>
    /// <param name="result">Result of the file download</param>
    public void SetFileDownloadFinished(uint fileID, T3DFileState result)
    {
        int index = list3DFiles.FindIndex(r => r.fileID == fileID);

        if (list3DFiles[index].fileState == T3DFileState.loading)
        {
            list3DFiles[index].fileState = result;
            //Apply3DModelScale();
            //Calculate3DModelBoundingBox();
            ApplyDefault3DModelScale();

            //Invoke("DoAfterScaling", 4);
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadSuccess, productModel.GetNavigationAxis()));
            if (!IsAnyFileLoading())
            {
                LoadExplosionModel(explosionXML_url);                
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DListLoadSuccess));                
                SetUnblockUserInterfaceAfterFileLoading();
            }
        }
        else
        {
            //error
        }        
    }

    /*private void DoAfterScaling() {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_3DLoadSuccess, productModel.GetNavigationAxis()));
        if (!IsAnyFileLoading())
        {
            LoadExplosionModel(explosionXML_url);
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ProductModelLoadOk));
            SetUnblockUserInterfaceAfterFileLoading();
        }
    }*/

    /// <summary>Check if any file is downloading</summary>
    /// <returns>true if any file is downloading</returns>
    private bool IsAnyFileLoading()
    {
        return list3DFiles.Exists(r => r.fileState == T3DFileState.loading);        
    }

    /// <summary>Blocks some of the action of the User interface</summary>
    private void SetBlockUserInterfaceDuringFileLoading()
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_FileDownloadBegin));        
    }
    
    /// <summary>Unblocks some of the action of the User interface</summary>
    private void SetUnblockUserInterfaceAfterFileLoading()
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_FileDownloadEnd));        
    }
  
    /// <summary>Start the download of the 3D files received in the product model</summary>
    private void Load3DFilesFromProductModel()
    {
        if (productModelLoaded)
        {
            //Get from the model 3D files to be downloaded
            List<C3DFileData> listOf3DFiles_ToLoad = Get3DFilesToDownload();
            list3DFiles.AddRange(listOf3DFiles_ToLoad);                               //Add to the manager file list
            //Get from the model the resources files to be downloaded
            List<CResourceFileData> listResourcesFiles_ToLoad = GetResourcesFileToDownload();
            listResourceFiles.AddRange(listResourcesFiles_ToLoad);
            //Other
            SetBlockUserInterfaceDuringFileLoading();                                                           //Block Interface during loading
            this.GetComponent<_3DFileManager>().Load3DFiles(listOf3DFiles_ToLoad, listResourcesFiles_ToLoad);   //Send this list to download manager
        }
    }

    /// <summary>Create a list of 3D files that have to be downloaded</summary>
    /// <returns>List of 3D files that have to be downloaded</returns>
    private List<C3DFileData> Get3DFilesToDownload()
    {
        List<C3DFileData> list_3DFilesToDownload = new List<C3DFileData>();
        foreach (CGeometryFileData file in productModel.geometry_files)
        {
            int index = list_3DFilesToDownload.FindIndex(r => r.fileName == file.fileName);
            if (index == -1) { list_3DFilesToDownload.Add(new C3DFileData(GetFileID(), file)); }
        }
        return list_3DFilesToDownload;       
    }
    
    /// <summary>Create a list of resources files that have to be downloaded/used</summary>
    /// <returns>List of resources files to be downloaded/use</returns>
    private List<CResourceFileData> GetResourcesFileToDownload()
    {
        List<CResourceFileData> resources_file = new List<CResourceFileData>();
        foreach (CResourceFileData file in productModel.resource_files)
        {
            int index = resources_file.FindIndex(r=> r.fileName == file.fileName );
            if (index == -1) { resources_file.Add(file); }
        }
        return resources_file;
    }

        
    private uint GetFileID()
    {
        return files3DID++;
    }
    
    
    //////////////////////////
    // Create Model Methods //
    //////////////////////////   
    /// <summary>Update the product model after download a 3D file</summary>
    /// <param name="loadedGameObject"></param>
    public void Add3DFileToProductModel(uint fileID, GameObject loadedGameObject)
    {
        List<CNodeData> listOfNodes = new List<CNodeData>();
        List<CLeafData> listOfLeaves = new List<CLeafData>();
        List<CAreaData> listOfAreas = new List<CAreaData>();
        List<CGeometryFileData> listOfGeometryData =  new List<CGeometryFileData>();

        CreateModelFromGameObject(loadedGameObject, out listOfNodes, out listOfLeaves, out listOfAreas);    //Create the list of nodes, leaves and areas loaded from the game object
        CreateGeometryModel(fileID, out listOfGeometryData);
        
        //Store the created product model
        if (!productModelLoaded)
        {            
            //Product model is empty
            productModel = new CProductModel(listOfNodes, listOfLeaves, listOfAreas, listOfGeometryData);         //Create as a new product model            
            if (productModel.isInit())
            {
                productModel.navigation_axis = "vertical";
                productModelLoaded = productModel.SetupDictionaries();                                  // Initialize the class with the data uploaded
            }
        }
        else
        {       
            //TODO check if there are geometry id collisions...                                                    
            productModelLoaded = productModel.Add(listOfNodes, listOfLeaves, listOfAreas, listOfGeometryData);                   //Add to product model            
        }
        AddResourcesFilesToProductModel();
        // Send Message to UI
        string fileName = list3DFiles.Find(r => r.fileID == fileID).fileName;     // Get file Name
        if (productModelLoaded) { SendMessageToUI("Product model created/updated correctly from " + fileName + " !!!", 10.0f);            }
        else {                    SendMessageToUI("ERROR: hom3r failed trying to create/updated the product model from " + fileName + " !!!", 10.0f);    }
    }

    private void CreateGeometryModel(uint fileID, out List<CGeometryFileData> listOfGeometryData)
    {                
        CGeometryFileData geometryData;
        listOfGeometryData = new List<CGeometryFileData>();
        C3DFileData fileData = Get3DFileData(fileID);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(fileData.fileName);        
        geometryData = new CGeometryFileData(fileData.fileName, fileData.fileUrl, fileData.fileType, 1, fileName, true, 0d, 0d, 0d, 0d, 0d, 0d, 0d);
        listOfGeometryData.Add(geometryData);
    }

    private void AddResourcesFilesToProductModel()
    {
        foreach (CResourceFileData file in listResourceFiles)
        {
            int index = productModel.resource_files.FindIndex(r => r.fileName == file.fileName);
            if (index == -1) { productModel.resource_files.Add(file); }
        }
    }

    /// <summary>Create a product model from a Game Object. 
    /// Assuming that this game object has been loaded from a 3D file.</summary>
    /// <param name="loadedGameObject">in parameter. Object to be read</param>
    /// <param name="listOfNodes">out parameter. List of nodes to be filled</param>
    /// <param name="listOfAreas">out parameter. List of areas to be filled</param>
    private void CreateModelFromGameObject(GameObject loadedGameObject, out List<CNodeData> listOfNodes, out List<CLeafData> listOfLeaves, out List<CAreaData> listOfAreas)
    {
        string rootNodeID;
        listOfNodes = new List<CNodeData>();
        listOfLeaves = new List<CLeafData>();
        listOfAreas = new List<CAreaData>();

        if (!productModelLoaded)
        {
            //Create a Root node        
            CNodeData rootNode = CreateNewProductModelNode("#", "scene", false);// Create new node
            listOfNodes.Add(rootNode);                                              // Introduce node into node list        
            rootNodeID = rootNode.id;                                               // Get Root Node ID
        }
        else
        {
            rootNodeID = productModel.GetRootNodeId();                          // Get Root Node ID
        }

        //Create a this file product node
        CNodeData fileRootNode = CreateNewProductModelNode(rootNodeID, loadedGameObject.name, true);    // Create new node       
        listOfNodes.Add(fileRootNode);                                                                  // Introduce node into node list
       
        if (loadedGameObject.transform.childCount == 0)
        {
            CLeafData leaf = CreateNewProductModelLeaf(fileRootNode.id, loadedGameObject.name, true);   // Create new leaf 
            CAreaData area = CreateNewAreaNode(leaf.id, loadedGameObject.name, loadedGameObject.name);  // Create new area            
            listOfLeaves.Add(leaf);                                                                     // Introduce node into node list
            listOfAreas.Add(area);                                                                      // Introduce area into area list
        }
        else
        {
            //Call again to itself with one child
            for (int i = loadedGameObject.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = loadedGameObject.transform.GetChild(i).gameObject;                
                CreateModelFromGameObjectRecursively(child, listOfNodes, listOfLeaves,listOfAreas, fileRootNode.id);
            }
        }
    }

    void CreateModelFromGameObjectRecursively(GameObject obj, List<CNodeData> listOfNodes, List<CLeafData> listOfLeaves, List<CAreaData> listOfAreas, string parentID)
    {
        
        if ((obj.GetComponent<Renderer>()))
        {
            if (obj.transform.childCount == 0)
            {
                CLeafData leaf = CreateNewProductModelLeaf(parentID, obj.name, true);   // Create new node                 
                CAreaData area = CreateNewAreaNode(leaf.id, obj.name, obj.name);        // Create new area            
                listOfLeaves.Add(leaf);                                                 // Introduce node into node list
                listOfAreas.Add(area);                                                  // Introduce area into area list                 
            }            
        }        
        else
        {
            if (obj.transform.childCount != 0)
            {
                CNodeData node = CreateNewProductModelNode(parentID, obj.name, false);    // Create new node 
                listOfNodes.Add(node);                                                    // Introduce node into node list
                for (int i = obj.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject child = obj.transform.GetChild(i).gameObject;
                    CreateModelFromGameObjectRecursively(child, listOfNodes, listOfLeaves, listOfAreas, node.id);
                }
            }           
        }        
    }

    /// <summary>Created a new product model node.</summary>
    /// <param name="_parentId">node parent id</param>
    /// <param name="_text">node description </param>    
    /// <param name="_specialNode">if it is special node or not</param>
    /// <returns></returns>
    private CNodeData CreateNewProductModelNode(string _parentId, string _text, bool _specialNode)
    {
        string _id = productModel.GetAutogeneratedID();                          // Get new ID
        CNodeData newNode = new CNodeData(_id, _parentId, _text, _specialNode);  // Create new node
        return newNode;
    }

    private CLeafData CreateNewProductModelLeaf(string _parentId, string _description, bool _selectable)
    {
        string _id = productModel.GetAutogeneratedID();                                // Get new ID
        CLeafData newLeaf = new CLeafData(_id, _parentId, _description, _selectable);  // Create new node
        return newLeaf;
    }

    /// <summary>Created a new product model area.</summary>
    /// <param name="_parent">node parent id</param>
    /// <param name="_text">area description</param>
    /// <param name="_meshName">mesh name in the graph scene</param>
    /// <returns></returns>
    private CAreaData CreateNewAreaNode(string _parent, string _text, string _meshName)
    {
        string id = productModel.GetAutogeneratedID();                                  // Get new ID
        CAreaData newArea = new CAreaData(id, _parent, _text, _meshName);       // Create new area
        return newArea;
    }

    //////////////////////////
    // Reset Model Methods  //
    //////////////////////////
    /// <summary>Reset Product Model</summary>
    public void ResetModel()
    {
        if (!IsAnyFileLoading())
        {
            SetBlockUserInterfaceDuringFileLoading();                                       // Block UI            
                        
            for (int i = 0; i < hom3r.quickLinks._3DModelRoot.transform.childCount; i++)
            {
                GameObject child = hom3r.quickLinks._3DModelRoot.transform.GetChild(i).gameObject;
                Destroy(child);
            }
            
            list3DFiles.Clear();
            productModelLoaded = false;
            productModel.Clear();

            InitVars();     // Initialize all the variables

            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ResetModel));
            Resources.UnloadUnusedAssets();
                                  
            SetUnblockUserInterfaceAfterFileLoading();          // UnBlock UI
        }        
    }




    //////////////////////////
    // Modify Model Methods  //
    //////////////////////////
    public void ModifyModel(CIOEditProductModel editModelData)
    {
        bool result;
        
        if (editModelData.command == "title")
        {
            result = this.ChangeModelTitle(editModelData.title);
        }
        else if (editModelData.command == "navigation_axis")
        {
            result = this.ChangeNavigationAxis(editModelData.navigationAxis);
        }
        else if (editModelData.command == "rename_part")
        {
            result = this.RenamePart(editModelData.id, editModelData.description);
        }
        else if (editModelData.command == "move_part")
        {
            result = this.MovePart(editModelData.id, editModelData.parentId);
        }
        else if (editModelData.command == "remove_part")
        {
            result = this.RemovePart(editModelData.id);
        }
        else if (editModelData.command == "create_node")
        {
            this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.DeselectAllParts, ""));
            result = this.CreateNode(editModelData.name, editModelData.parentId, editModelData.childrenIdList);
        }        
        else
        {
            result = false;
        }

        // Report the result        
        if (result)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ProductModelEditOk));            
        }
        else
        {
            string errorMessage= "Error modifying the model with the command: " + editModelData.command;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ProductModelEditOk, errorMessage));
            Debug.LogError(errorMessage);
        }        
    }

    private bool RenamePart(string id, string newDescription)
    {
        bool result;
        string nodeType = productModel.GetNodeType(id);
        if (nodeType == "node")         { result = productModel.SetNodeDescription(id, newDescription); }
        else if (nodeType == "leaf")    { result = productModel.SetLeafDescription(id, newDescription); }
        else if (nodeType == "area")    { result = productModel.SetAreaDescription(id, newDescription); }
        else { result = false; }
        return result;
    }

    private bool ChangeModelTitle(string newModelName)
    {        
        if ((productModelLoaded) && newModelName != null && newModelName != "")
        {
            productModel.title = newModelName;
            return true;
        }
        return false;
    }

    private bool ChangeNavigationAxis(string newNavigationAxis)
    {
        if ((productModelLoaded) && newNavigationAxis != null && newNavigationAxis != "")
        {
            productModel.SetNavigationAxis(newNavigationAxis);            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_NavigationAxisChanged, newNavigationAxis));
            return true;
        }
        return false;
    }

    private bool MovePart(string id, string newParentId)
    {
        bool result = false;
        string nodeType = productModel.GetNodeType(id);
        if (nodeType == "node") { /*result = productModel.SetNodeDescription(id, newParentId);*/ }
        else if (nodeType == "leaf")  {result = productModel.MoveLeaf(id, newParentId); }
        else if (nodeType == "area") { result = productModel.MoveArea(id, newParentId); }
        else { result = false; }
        return result;
    }

    private bool RemovePart(string id)
    {
        bool result = false;
        string nodeType = productModel.GetNodeType(id);
        if (nodeType == "node") {      result = productModel.RemoveNode(id); }
        else if (nodeType == "leaf") { result = productModel.RemoveLeaf(id); }
        else if (nodeType == "area") { result = false; }

        return result;
    }

    /// <summary>Create a new node a move other nodes to be its children</summary>
    /// <param name="name">New node name</param>
    /// <param name="parentId">New node parent</param>
    /// <param name="childrenList">Nodes to be moved to be its children</param>
    /// <returns>True if everything go well</returns>
    private bool CreateNode(string name, string parentId, List<string> childrenList)
    {
        bool result = false;
        
        if (!IsNode(parentId)) { return false; }            // Check if the parent Id exist
        if (!IsLeaveList(childrenList)) { return false; }   // Return if the children are no leaves       

        //TODO Resolve when children are not leaves

        CNodeData newNode = CreateNewProductModelNode(parentId, name, false);    // Create new node               
        if (productModel.AddNode(newNode)) {            
            bool control = productModel.MoveLeaves(childrenList, newNode.id);            
            result = control;          
        }        
        return result;
    }

    //////////////////////////
    // Get Model Methods    //
    //////////////////////////
    public CFileProductModel GetProductModel()
    {
        if (productModelLoaded)
        {
            CFileProductModel _productModel = new CFileProductModel(productModel);            
            return _productModel;
        }
        else
        {
            CFileProductModel _productModel = new CFileProductModel();
            return _productModel;
        }
        
    }

    //////////////////////
    // 3D Model Scale   //
    //////////////////////
    public void Set3DModelScale(float _modelScale)
    {
        if (modelScale != _modelScale)
        {
            modelScale = _modelScale;
            Apply3DModelScale();
        }        
    }

    private void Apply3DModelScale()
    {
        Vector3 currentScale = hom3r.quickLinks._3DModelRoot.transform.localScale;
        Vector3 newScale = modelScale * currentScale;
        hom3r.quickLinks._3DModelRoot.transform.localScale = newScale;
        Invoke("Calculate3DModelBoundingBox", 0.2f);
    }

    private void ApplyDefault3DModelScale()
    {
        modelBoundingBox = Get3DModelBoundingBox(true);

        float max = Mathf.Max(modelBoundingBox.extents.x, modelBoundingBox.extents.y, modelBoundingBox.extents.z);
        if (max > 100f)
        {
            float scale = 100f / max;
            Set3DModelScale(scale);
        }             
    }

    /////////////////////////////
    // 3D Model Bounding Box   //
    /////////////////////////////
    public Bounds Get3DModelBoundingBox(bool force = false)
    {
        if (force) {
            Debug.Log("Calculating a new 3D Model BoundingBox");
            Calculate3DModelBoundingBox();
        } else if (!modelBoundingBoxInit) {
            Calculate3DModelBoundingBox();
        }
        return modelBoundingBox;
    }
    
    /// <summary>Obtain the BoundingBox of whole 3D Model</summary>
    private void Calculate3DModelBoundingBox()
    {
        modelBoundingBox = Calculate3DModelBoundingBox(hom3r.quickLinks._3DModelRoot);  // Update 3D model bounding box  
        modelBoundingBoxInit = true;
    }
    
    /// <summary>
    /// Return the bounding box of a Object
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public Bounds CalculateExtern3DModelBoundingBox(GameObject target)
    {
        return Calculate3DModelBoundingBox(target);  // Update 3D model bounding box
    }

    /// <summary>Obtain the BoundingBox of a 3D Model</summary>
    /// <param name="target">obj to calculate bounding box</param>
    /// <returns>Target Bounding Box</returns>
    private Bounds Calculate3DModelBoundingBox(GameObject target)
    {
        //Initialization OBJ listBoundingBox
        List<GameObject> productTargetList = new List<GameObject>();
        productTargetList.Add(target);

        Bounds targetTotalBB = ComputeBoundingBox(productTargetList);

        return targetTotalBB;
    }

    /// <summary>Calculate the BoundingBox of a list of objects</summary>
    /// <param name="objects"> The list of Game objects </param> 
    /// <returns> The axis aligned bounding box. </returns> 
    private Bounds ComputeBoundingBox(List<GameObject> objects)
    {
        Bounds totalBB = new Bounds();
        bool firstObject = true;		// Needed to avoid to add over a bounding box which has not been initialized yet, because in this case Unity includes (0,0,0) center inside the BB        
        // Get info from all objects 
        foreach (GameObject go in objects)
        {
            // Expand bounding box 
            if (go != null)
            {
                if (go.GetComponent<Collider>() != null)
                {
                    Bounds goBB = go.GetComponent<Collider>().bounds;

                    if (firstObject)
                    {
                        totalBB = goBB;
                        firstObject = false;                        
                    }
                    else
                    {
                        totalBB.Encapsulate(goBB);
                    }                        
                }

                // Do the same for all children objects
                List<GameObject> children = new List<GameObject>();
                for (int i = go.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject newChild = go.transform.GetChild(i).gameObject;
                    if (newChild != null)
                        children.Add(newChild);
                }
                if (children.Count > 0)
                {
                    Bounds childrenBB = ComputeBoundingBox(children);
                    if (firstObject) { totalBB = childrenBB; }          // Avoid to add over a bounding box which has not been initialized yet, because in this case Unity includes (0,0,0) center inside the BB 
                    else { totalBB.Encapsulate(childrenBB); }                                        
                }
            }
        }
        return totalBB;
    }

    public float CalculateDiagonalBoundingBox(bool force = false)
    {
        float diagonal = 0;
        Bounds modelBoundingBox = Get3DModelBoundingBox(force);

        float BBx = modelBoundingBox.size.x;
        float BBy = modelBoundingBox.size.y;
        float BBz = modelBoundingBox.size.z;

        float low = (Mathf.Min(BBx, BBy, BBz));
        float high = 5.0f * low; // We consider that 5 times bigger is out of range

        if (BBx < high && BBy < high && BBz < high)
        {
            //Calculate Diagonal3D
            diagonal = Mathf.Sqrt(MathHom3r.Pow2(modelBoundingBox.size.x) + MathHom3r.Pow2(modelBoundingBox.size.y) + MathHom3r.Pow2(modelBoundingBox.size.z));
        }
        //Calculate diagonal 2D or 1D
        else if (BBx > high)
        {
            if (BBy > high)
                diagonal = MathHom3r.Pow2(BBz);
            else if (BBz > high)
                diagonal = MathHom3r.Pow2(BBy);
            else
                diagonal = Mathf.Sqrt(MathHom3r.Pow2(BBy) + MathHom3r.Pow2(BBz));
        }

        else if (BBy > high)
        {
            if (BBx > high)
                diagonal = MathHom3r.Pow2(BBz);
            else if (BBz > high)
                diagonal = MathHom3r.Pow2(BBx);
            else 
                diagonal = Mathf.Sqrt(MathHom3r.Pow2(BBx) + MathHom3r.Pow2(BBz));
        }

        else if (BBz > high)
        {
            if (BBx > high)
                diagonal = MathHom3r.Pow2(BBy);
            else if (BBy > high)
                diagonal = MathHom3r.Pow2(BBx);
            else
                diagonal = Mathf.Sqrt(MathHom3r.Pow2(BBx) + MathHom3r.Pow2(BBy));
        }

        return diagonal;
    }

    /////////////////////////////
    // ModelManagement Methods //
    /////////////////////////////

    /// <summary>Returns if one ID is a area of the product model</summary>
    /// <param name="areaID">ID to check</param>
    /// <returns>Returns true if this ID correspond to a area of the product node</returns>
    public bool IsArea(string areaID) { return productModel.IsArea(areaID); }

    /// <summary>Returns if this leaf ID is a leaf of the product model</summary>
    /// <param name="leafId">Leaf ID to check</param>
    /// <returns>Returns true if this ID correspond to a leaf of the product node</returns>
    public bool IsLeaf(string leafId) { return productModel.IsLeaf(leafId); }

    /// <summary>Returns if this list of leaf ID is a leaf of the product model</summary>
    /// <param name="leaveList">List of leaves IDs to check</param>
    /// <returns>Returns true if all the ID correspond to a leaf of the product node</returns>
    public bool IsLeaveList(List<string> leaveList) { return productModel.IsLeafList(leaveList); }

    /// <summary>Returns if one ID is a node of the product model</summary>
    /// <param name="nodeID">ID to check</param>
    /// <returns>Returns true if this ID correspond to a node of the product node</returns>
    public bool IsNode(string nodeID) { return productModel.IsNode(nodeID);   }
    
    /// <summary>Returns if one ID is a special node of the product model</summary>
    /// <param name="nodeID">ID to check</param>
    /// <returns>Returns true if this ID correspond to a special node of the product node</returns>    
    public bool IsSpecialNode(string nodeID) { return productModel.IsSpecialNode(nodeID);   }

    /// <summary>
    /// Return if an Area is selectable or not
    /// </summary>
    /// <param name="areaID">areaID</param>
    /// <returns></returns>
    public bool IsAreaSelectable(string areaID)
    {
        return productModel.IsAreaSelectable(areaID);
    }

    /// <summary>
    /// Return the mode navigation axis read from the product model file
    /// </summary>
    /// <returns>string that contains "vertical" or "horizontal"</returns>
    public string GetModelNavigationAxis() { return productModel.navigation_axis; }

    /// <summary>Get the full description of a node by its ID. Full description contains its description plus its parent description.</summary>
    /// <param name="nodeID">ID of the node</param>
    /// <returns>String contains the full description</returns>
    public string GetProductNodeFullDescription_ByNodeID(string nodeID)
    {
        if (productModelLoaded)
        {            
            string parentDescription = productModel.GetNodeDescriptionByNodeID(productModel.GetNodeParentIDByNodeID(nodeID));
            string description = parentDescription + " / " + productModel.GetNodeDescriptionByNodeID(nodeID);
            return description;
        }
        return null;
    }

    public string GetProductNodeFullDescription1Line_ByNodeID(string nodeID)
    {
        if (productModelLoaded)
        {
            string parentDescription = productModel.GetNodeDescriptionByNodeID(productModel.GetNodeParentIDByNodeID(nodeID));
            string description = parentDescription + " / " + productModel.GetNodeDescriptionByNodeID(nodeID);
            return description;
        }
        return null;
    }

    public List<string> GetSpecialAncestorIDList_ByProductID(string nodeID)
    {
        return productModel.GetSpecialAncestorIDListByNodeID(nodeID);       
    }
 
    public string GetAreaDescriptionByAreaID(string areaID)
    {
        if (productModel.IsArea(areaID))
        {
            return productModel.GetAreaDescription(areaID);
        }
        return null;
    }

    public string GetAreaFullDescription_ByAreaID(string areaID)
    {       
        if (productModel.IsArea(areaID))
        {
            string componentDescription = GetProductNodeFullDescription_ByNodeID(GetSpecialAncestorID_ByAreaID(areaID));
            string description = componentDescription + "\n" + productModel.GetAreaDescription(areaID);
            return description;
        }
        return null;
    }

    public string GetAreaFullDescription1Line_ByAreaID(string areaID)
    {        
        if (productModel.IsArea(areaID))
        {
            string componentDescription = GetProductNodeFullDescription_ByNodeID(GetSpecialAncestorID_ByAreaID(areaID));
            string description = componentDescription + " / " + productModel.GetAreaDescription(areaID);
            return description;
        }
        return null;
    }

    /// <summary> Return the parent ID of an area. </summary>
    /// <param name="areaID">area ID</param>
    /// <returns>Parent ID</returns>
    public string GetNodeLeafID_ByAreaID(string areaID)
    {
        if (areaID != null)
        {
            return productModel.GetAreaParentIDByAreaID(areaID);
        }
        return null;
    }

    public string GetSpecialAncestorID_ByAreaID(string areaID)  {   return productModel.GetSpecialAncestorIDByAreaID(areaID);   }

    /// <summary>Get a pointer to an area by its area ID </summary>
    /// <param name="areaID"></param>
    /// <returns>Pointer to game object</returns>
    public GameObject GetAreaGameObject_ByAreaID(string areaID) {   return productModel.GetAreaGameObjectByAreaID(areaID);  }
        
    /// <summary>Return a list of game objects of the areas that are hanging from a product node</summary>
    /// <param name="nodeID">Product node ID</param>
    /// <returns>List of game objects</returns>
    public List<GameObject> GetAreaGameObjectList_ByProductNodeID(string nodeID)    { return productModel.GetAreaGameObjectListByNodeID(nodeID);    }

    /// <summary>Get a list of GameObjets of the areas hanging from a special node ID.</summary>
    /// <param name="specialAncestorID"></param>
    /// <returns>Game Object list</returns>
    public List<GameObject> GetAreaGameObjectList_BySpecialAncestorID(string specialAncestorID)
    {        
        return productModel.GetAreaGameObjectListBySpecialAncestorID(specialAncestorID);
    }

    /// <summary>Get a list of IDs of the areas hanging from a special node ID.</summary>
    /// <param name="specialAncestorID"></param>
    /// <returns>Game Object list</returns>
    public List<string> GetAreaIDList_BySpecialAncestorID(string specialAncestorID)
    {
        return productModel.GetAreaIDListBySpecialAncestorID(specialAncestorID);
    }
        
    /// <summary>Search all the areas hanging from a list of leaf node IDs.</summary>
    /// <param name="nodeLeafList">List of leaf nodes</param>
    /// <returns>List of game objects</returns>
    public List<GameObject> GetAreaGameObjectList_ByNodeLeafIDList(List<string> nodeLeafList)
    {
        List<GameObject> gameObjectList;
        gameObjectList = new List<GameObject>();

        if (productModelLoaded)
        {
            foreach (string cell in nodeLeafList)
            {
                gameObjectList.AddRange(GetAreaGameObjectList_ByLeafID(cell));
            }
            return gameObjectList;
        }
        return null;
    }

    /// <summary>Search all the areas hanging from a leaf node ID. </summary>
    /// <param name="nodeLeafList">leaf node ID</param>
    /// <returns>List of game objects</returns>
    public List<string> GetAreaList_ByLeafID(string LeafID)
    {
        return productModel.GetAreaIDListByLeafID(LeafID);
    }

    /// <summary>Search all the areas hanging from a leaf node ID. </summary>
    /// <param name="nodeLeafList">leaf node ID</param>
    /// <returns>List of game objects</returns>
    public List<GameObject> GetAreaGameObjectList_ByLeafID(string nodeLeafID)
    {       
        return productModel.GetAreaGameObjectListByLeafID(nodeLeafID);        
    }

    /// <summary>Search all the areas hanging from a leaf node ID by one of is areas </summary>
    /// <param name="obj">Pointer to one of is areas</param>
    /// <returns>List of game objects</returns>
    public List<GameObject> GetAreaGameObjectList_ByArea(GameObject obj)
    {
        string areaID = obj.GetComponent<ObjectStateManager>().areaID;
        string parentID = GetNodeLeafID_ByAreaID(areaID);
        return GetAreaGameObjectList_ByLeafID(parentID);
    }

    /// <summary>
    /// Get the private list that contains every node of the product model
    /// </summary>
    /// <returns>productNodes_List </returns>
    public List<GameObject> GetAreaNodeList() {
        //return areaNodes_List;
        return productModel.GetAllAreaGameobjects();
    }
   
    /// <summary>Add ID  to the object and Add a pointer to its dictionary</summary>
    /// <param name="objectName"></param>
    /// <param name="gameObjectPointer"></param>
    public void SetAreaID_And_GameObject(string objectName, GameObject gameObjectPointer)
    {        
        productModel.UpdateAreaDictionaryAndGameObject(objectName, gameObjectPointer);        
    }
    
    public string GetGeometryFileRotation(int fileId)
    {
        return productModel.GetGeometryFileRotation(fileId);
    }


    /// <summary>
    /// Return the first Area on the list ID. We need now for boards labels.
    /// TODO think if we should delete or do in a different way
    /// </summary>
    /// <returns></returns>
    public string GetFirstAreaId()
    {
        if (!productModelLoaded) { return null; }
        return productModel.areas[0].id;
    }

    ////////////////////
    /// Other Methods
    ////////////////////    

    /// <summary>Save the file local path when we are in this situation</summary>
    /// <param name="productModel_url">Product Model URL</param>
    /// <returns>return the local file path</returns>
    private string GetPathIfLocalFile(string productModel_url)
    {
        if (productModel_url.Substring(0, 4) == "file")
        {
            return productModel_url.Substring(0, productModel_url.LastIndexOf("/") + 1);     //Work with model store in the HDD and inside the editor
        }
        else
        {
            return "";
        }
    }

    public List<GameObject> ClearAreaSelectionColour() {  return productModel.GetAllAreaGameObjectList(); }     
                        

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
