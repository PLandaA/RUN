using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenDoor : MonoBehaviour
{
    public Text uiText;
    private Coroutine clearTextRoutine;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            if (other.gameObject.name == "RedKey")
            {
                GameManager.doorOpen = true;
            }
            else
            {
                ShowMessage("You need the red key");
            }
        }
        else if (other.CompareTag("Player"))
        {
            ShowMessage("You need a key to get out");
        }
    }

    private void ShowMessage(string message)
    {
        if (clearTextRoutine != null)
        {
            StopCoroutine(clearTextRoutine);
            clearTextRoutine = null;
        }
        uiText.text = message;
    }


    private void OnTriggerExit(Collider other)
    {
        if (clearTextRoutine != null)
        {
            StopCoroutine(clearTextRoutine);
        }
        clearTextRoutine = StartCoroutine(WaitForDelete());
    }

    IEnumerator WaitForDelete()
    {
        yield return new WaitForSeconds(3.5f);
        uiText.text = "";
    }
}
