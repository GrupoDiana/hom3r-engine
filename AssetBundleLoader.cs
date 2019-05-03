using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleLoader : MonoBehaviour {
	
    public void ProcessAssetBundleDownload(C3DFileData file)
    {
        StartCoroutine(DownloadAssetBundle(file));        
    }

   
    IEnumerator DownloadAssetBundle(C3DFileData file)
    {
        bool error = false;
        string uri = file.fileUrl;

        UnityEngine.Networking.UnityWebRequest request;
        //Start download
        if (file.version == 0 || file.crc == "") {
            request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        }
        else
        {
            uint version = (uint)file.version;
            uint crc = System.Convert.ToUInt32(file.crc);
            request = UnityWebRequestAssetBundle.GetAssetBundle(uri, version, crc);   //Start download
        }
                
        //yield return request.Send();
        //request.Send();
        request.SendWebRequest();
        while (!request.isDone)
        {
            SendMessageToUI("Downloading 3D Models from " + uri + " : " + Mathf.Round(request.downloadProgress * 100.0f).ToString() + "%", 0.0f);
            yield return new WaitForSeconds(0.1f);
        }

        if (request.error != null)
        {
            Debug.LogError("File " + uri + " download error: " + request.error);
            SendMessageToUI("File " + uri + " download error: " + request.error, 0.0f);
            error = true;
            this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);                        
            yield break;
        }        
        SendMessageToUI("Downloading 3D Models from " + uri + " : 100%", 0.0f);
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);    // Get assetbundle handler
        if (file.fileOrigin == T3DFileOrigin.productModelJSON)
        {
            yield return StartCoroutine(ExtractAssetsOneByOne(file, bundle));
        }
        else
        {
            yield return StartCoroutine(ExtractAllAssets(file, bundle));
        }
    }

    IEnumerator ExtractAssetsOneByOne(C3DFileData file, AssetBundle bundle)
    {
        bool error = false;

        string assetName = file.assetName;
        SendMessageToUI("Loading 3D Asset of " + assetName, 0.0f);
        AssetBundleRequest assetbundleRequest = bundle.LoadAssetAsync<GameObject>(assetName);
        //yield return assetbundleRequest;
        while (!assetbundleRequest.isDone)
        {
            float percentage = Mathf.Round(assetbundleRequest.progress * 100.0f);
            SendMessageToUI("Loading 3D Asset of " + assetName + " " + percentage.ToString() + "%", 0.0f);
            yield return new WaitForSeconds(0.1f);
        }
        if (assetbundleRequest.asset == null)
        {
            SendMessageToUI("Asset called " + assetName + " not found.", 0.0f);                
            string message = "Asset called " + assetName + " not found.";
            //hom3r.coreLink.Do(new CIOCommand(TIOCommands.DebugConsoleWeb, message), Constants.undoNotAllowed);
            this.SendMessageToConsole(message);
            error = true;
            this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);
            yield break;
        }
        else
        {
            SendMessageToUI("Preparing 3D Model of " + assetName, 0.0f);

            var loadedAsset = assetbundleRequest.asset;
            //Instantiate(loadedAsset);
            GameObject loadedAssetGameObject = (GameObject)Instantiate(loadedAsset);                        // Create a GameObject from the asset readed and instantiate in the 3D enviroment
                
            this.GetComponent<_3DFileManager>().ProcessAfterAssetBundleLoad(file, loadedAssetGameObject);   // Make actions after load the assetBundle
        }
       
        SendCleanMessageToUI();
        this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);

        //Clean memory
        bundle.Unload(false);
        //When unloadAllLoadedObjects is false, compressed file data for assets inside the bundle will be unloaded, but any actual objects already loaded from this bundle will be kept intact.Of course you won't be able to load any more objects from this bundle
        //When unloadAllLoadedObjects is true, all objects that were loaded from this bundle will be destroyed as well. If there are game objects in your scene referencing those assets, the references to them will become missing.
    }

    IEnumerator ExtractAllAssets(C3DFileData file, AssetBundle bundle)
    {
        bool error = false;

        AssetBundleRequest assetbundleRequest = bundle.LoadAllAssetsAsync<GameObject>();

        while (!assetbundleRequest.isDone)
        {
            float percentage = Mathf.Round(assetbundleRequest.progress * 100.0f);
            SendMessageToUI("Loading 3D Models from " + file.fileName + " " + percentage.ToString() + "%", 0.0f);
            yield return new WaitForSeconds(0.1f);
        }

        if (assetbundleRequest.asset == null)
        {
            SendMessageToUI("Error: Loading 3D Models from " + file.fileName, 0.0f);            
            string message = "Error: Loading 3D Models from " + file.fileName;
            //hom3r.coreLink.Do(new CIOCommand(TIOCommands.DebugConsoleWeb, message), Constants.undoNotAllowed);
            this.SendMessageToConsole(message);
            error = true;
            this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);
            yield break;
        }
        else
        {
            SendMessageToUI("Preparing 3D Model of " + file.fileName, 0.0f);            
            var loadedAsset = assetbundleRequest.asset;
            //Instantiate(loadedAsset);
            GameObject loadedAssetGameObject = (GameObject)Instantiate(loadedAsset);                        // Create a GameObject from the asset readed and instantiate in the 3D enviroment

            this.GetComponent<_3DFileManager>().ProcessAfterAssetBundleLoad(file, loadedAssetGameObject);   // Make actions after load the assetBundle
        }
        SendCleanMessageToUI();
        this.GetComponent<_3DFileManager>().SetFileDownloadFinished(file.fileID, error);

        //Clean memory
        bundle.Unload(false);        
        //When unloadAllLoadedObjects is false, compressed file data for assets inside the bundle will be unloaded, but any actual objects already loaded from this bundle will be kept intact.Of course you won't be able to load any more objects from this bundle
        //When unloadAllLoadedObjects is true, all objects that were loaded from this bundle will be destroyed as well. If there are game objects in your scene referencing those assets, the references to them will become missing.
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
    }
    void SendMessageToConsole(string _message)
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent._3DFileManager_ShowMessageConsole, _message));
        //hom3r.coreLink.Do(new CIOCommand(TIOCommands.DebugConsoleWeb, message), Constants.undoNotAllowed);
    }

}
