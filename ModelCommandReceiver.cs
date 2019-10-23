using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {        
        hom3r.coreLink.SubscribeCommandObserver(DoModelCommand, UndoModelCommand);  //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoModelCommand, UndoModelCommand);        
    }

    private void DoModelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CModelCommand)) { command.Do(this); }
        else { /* Error - Do nothing */ }
    }

    private void UndoModelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CModelCommand)) { command.Undo(this); }
        else { /* Error - Do nothing */ }
    }    
}

[System.Serializable]
public class CIO3DFileData
{
    public string fileName;
    public string fileURL;
    public string fileType;
}

[System.Serializable]
public class CIOEditProductModel
{    
    public string command;
    public string title;
    public string navigationAxis;
    public string id;
    public string description;
    public string name;
    public string parentId;
    public List<string> childrenIdList;
}

/// <summary>Model commands</summary>
public enum TModelCommands
{
    LoadProductModel, ResetProductModel, ModifyProductModel,
    Load3DFiles,
    SetNavigationAxis,
    SetModelScale
}

/// <summary>Model data</summary>
public class CModelCommandData
{
    public TModelCommands command;

    public string productModelURL { get; set; }    
    public string explosionModelURL { get; set; }
    public List<CIO3DFileData> listOfFiles { get; set; }
    public float scale { get; set; }
    public string navigationAxis { get; set; }
    public CIOEditProductModel editProductModelData { get; set; }

    public CModelCommandData(TModelCommands _command)
    {
        this.command = _command;
    }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CModelCommand : CCoreCommand
{
    public CModelCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CModelCommand(TModelCommands _command)
    {
        data = new CModelCommandData(_command);
    }
    public CModelCommand(TModelCommands _command, float _scale)
    {
        data = new CModelCommandData(_command);
        data.scale = _scale;
    }
    public CModelCommand(TModelCommands _command, string _navigationAxis)
    {
        data = new CModelCommandData(_command);

        data.navigationAxis = _navigationAxis;        
    }

    public CModelCommand(TModelCommands _command, string _productModelURL, string _explosionModelURL)
    {
        data = new CModelCommandData(_command);

        data.productModelURL = _productModelURL;
        data.explosionModelURL = _explosionModelURL;
    }
    public CModelCommand(TModelCommands _command, List<CIO3DFileData> _listOfFiles)
    {
        data = new CModelCommandData(_command);

        data.listOfFiles = _listOfFiles;
    }
    public CModelCommand(TModelCommands _command, CIOEditProductModel _editData)
    {
        data = new CModelCommandData(_command);
        data.editProductModelData = _editData;        
    }

    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.command)
            {
                case TModelCommands.LoadProductModel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().LoadProductModels(data.productModelURL, data.explosionModelURL);
                    break;

                case TModelCommands.ResetProductModel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().ResetModel();
                    break;

                case TModelCommands.ModifyProductModel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().ModifyModel(data.editProductModelData); 
                    break;

                case TModelCommands.Load3DFiles:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Load3DFilesWithOutProductModel(data.listOfFiles);
                    break;
                case TModelCommands.SetNavigationAxis:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().SetNavigationAxis(data.navigationAxis);
                    break;
                case TModelCommands.SetModelScale:
                    hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Set3DModelScale(data.scale);
                    break;
                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Model command without a valid command");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
}
