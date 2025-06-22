using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTrigger : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;
    void Start()
    {
        Time.timeScale = 1f; 
        clearPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            clearPanel.SetActive(true);
            Time.timeScale = 0f; 
        }
    }

}
