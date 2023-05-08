using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkableNPCs : MonoBehaviour, IActions
{
    [SerializeField] float typeSpeed = .1f;
    [SerializeField] Text textComponent;
    public void Action()
    {

    }
    void StartDialogue()
    {

    }
    //IEnumerator TypeLine()
    //{
    //    foreach(char c in line[index].toCarArray())
    //    {
    //        textComponent.text += c;
    //        yield return new WaitForSeconds(typeSpeed);
    //    }
    //}
}
