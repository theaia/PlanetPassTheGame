using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepController : MonoBehaviour
{
    [Header("Movement")]
    public Transform target;
    public float moveSpeed = 5f;

    public PlaceObject placeScript;

    bool dragged;
    Camera gameCam;

    [SerializeField]
    private Transform sheepVisual;

    private Quaternion lastParentRotation = Quaternion.identity;
    private Transform parentTrans;

    [Header("Sheep Stuff")]
    public bool hasWool = true;
    public float timeToRegrow = 5f;
    public GameObject woolModel;

    private void Start()
    {
        parentTrans = transform.parent;
        gameCam = Camera.main;	

        woolModel.SetActive(true);
    }

    public void StartDraggingSheep()
    {
        dragged = true;
        placeScript.isMoving = true;
    }

    // public void StopDraggingSheep()
    // {
    //     dragged = false;
    // }

    public void HarvestWool()
    {
        hasWool = false;

        woolModel.SetActive(false);

        Invoke("RegrowWool", timeToRegrow);
    }

    private void RegrowWool()
    {
        hasWool = true;
        
        woolModel.SetActive(true);
    }

    private void OnDisable()
    {
        GameObject.Find("Game Control").GetComponent<GameControl>().SheepDied();
    }
}
