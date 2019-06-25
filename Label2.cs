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

    private void Awake()
    {
        
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
        this.gameObject.transform.position = this.position.panelPosition;
        this.gameObject.transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text = this.text;
        // resize        
    }





    public string GetLabelId() { return this.id; }
    

}
