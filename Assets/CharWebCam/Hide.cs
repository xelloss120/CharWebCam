using System.Collections;
using UnityEngine;

public class Hide : MonoBehaviour
{
    public GameObject Target;

    void Start()
    {
        StartCoroutine("GetKeyEsc");
    }

    IEnumerator GetKeyEsc()
    {
        while (!Input.GetKey(KeyCode.Escape)) yield return null;
        Target.SetActive(false);
    }
}
