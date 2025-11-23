using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class DebugNoteStatus : MonoBehaviour
{
    public TextMeshProUGUI text;
    public List<NotesCollisionMaganger> managerList;

    void Awake()
    {
        foreach( var list in managerList)
        {
            list.DisableCallBack = StatusOutput;
        }
    }


    void StatusOutput(int count,int max)
    {
        string status;
        if(count == max)
        {
            status = "Perfect";
        }
        else if((float)count >= (float)max * 0.8f)
        {
            status = "Good";
        }
        else if((float)count >= (float)max * 0.5f)
        {
            status = "Okay";
        }
        else
        {
            status = "Bad";
        }
        string str = $"All Notes: {max}\n Get NotesÅF{count}\n Result:{status}";
        text.text = str;
    }
}
