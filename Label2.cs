using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;




public class Label2 : MonoBehaviour
{
    string id;
    TLabelType type;
    string areaId;
    string text;
    CLabelPosition position;

    GameObject textGO;
    GameObject panelGO;


    private void Awake()
    {
        textGO = this.transform.Find("TextMeshPro").gameObject;
        panelGO = this.transform.Find("Panel3D").gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void Create(string _labelId, string _areaID, TLabelType _labelType, string _text, CLabelPosition _labelPosition)
    {
        this.id = _labelId;
        this.areaId = _areaID;
        this.type = _labelType;
        this.text = _text;
        this.position = _labelPosition;


        if (type == TLabelType.board)
        {
            this.CreateBoard();
        } else if (type == TLabelType.anchoredLabel)
        {
            // Whatever
        } else
        {
            //Error
        }

    }

    private void CreateBoard()
    {
        this.transform.position = this.position.panelPosition;
        this.transform.localRotation = position.panelRotation;

        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;
        
        // resize   
        float upDownPadding = 0.1f;
        panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + upDownPadding, panelGO.transform.localScale.z);

        Invoke("Invoke_RelocateBoard", 0.05f);
    }

    private void Invoke_RelocateBoard() { RelocateBoard(); }

    public void RelocateBoard()
    {
        //Recolocate board over the AR plane
        Bounds boardBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(this.gameObject);

        float yPos = panelGO.transform.localPosition.y + boardBounds.extents.y;
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, yPos, this.transform.localPosition.z);

        //current_descriptionPanelGO.SetActive(true);
    }


    /////////////////
    //  SET/ GET   //
    /////////////////

    public string GetLabelId() { return this.id; }
    
}
