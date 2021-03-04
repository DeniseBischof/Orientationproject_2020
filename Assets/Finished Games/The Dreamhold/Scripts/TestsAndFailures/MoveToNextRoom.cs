using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MoveToNextRoom : MonoBehaviour
{
    public enum MoveDirections
    {
        north,
        east,
        south,
        west
    }

    public MoveDirections Direction;

    public string RoomAnimation;

    public Animator animator;

    public GameObject RoomNorth;
    public GameObject RoomEast;
    public GameObject RoomSouth;
    public GameObject RoomWest;

    void Start()
    {

    }

    public void GoToNextRoom()
    {
        animator.Play(RoomAnimation);

        switch (Direction)
        {
            case MoveDirections.north:
                break;
            case MoveDirections.east:
                break;
            case MoveDirections.south:
                break;
            case MoveDirections.west:
                break;
        }
    }
}
