using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour {
    public GameObject ReactionObject;
    public GameObject ActionObject;

    public void TextNotEmptyButtonInteractable()
    {
       if(ActionObject.GetComponent<InputField>().text.Length > 0)
        {
            ReactionObject.GetComponent<Button>().interactable = true;
        }
        else
        {
            ReactionObject.GetComponent<Button>().interactable = false;
        }
    }

}
