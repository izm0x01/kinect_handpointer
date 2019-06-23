using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class markermanager : MonoBehaviour {

    [SerializeField]GameObject handtracking_obj;
    [SerializeField] RawImage RightHand_obj;
    [SerializeField] RawImage LeftHand_obj;

    public Texture openhand_texture;
    public Texture closehand_texture;
    [Range(50, 150)] public int kando=110;

    RawImage RR;
    RawImage LR;
    handtracking _handtracking;

    // Use this for initialization
    void Start () {

        RR = RightHand_obj.GetComponent<RawImage>();
        RR.texture = openhand_texture;
        LR = LeftHand_obj.GetComponent<RawImage>();
        LR.texture = openhand_texture;

        
        var scale = LR.rectTransform.localScale;
        scale = new Vector3(scale.x*-1,scale.y,scale.z);
        LR.rectTransform.localScale = scale;

        _handtracking = handtracking_obj.GetComponent<handtracking>();
    }

    // Update is called once per frame
    void Update () {

        LR.rectTransform.localPosition = _handtracking.righthand_pos * kando;
        RR.rectTransform.localPosition = _handtracking.lefthand_pos * kando;

        if (_handtracking.righthandstate == "Closed")
        {
        RR.texture = closehand_texture;
        }
        else if (_handtracking.righthandstate == "Open")
        {
        RR.texture = openhand_texture;
        }

        if (_handtracking.lefthandstate == "Closed")
        {
        LR.texture = closehand_texture;
        }
        else if (_handtracking.lefthandstate == "Open")
        {
            LR.texture = openhand_texture;
        }


    }
}
