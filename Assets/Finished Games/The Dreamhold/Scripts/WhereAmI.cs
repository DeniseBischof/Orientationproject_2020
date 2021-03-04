using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class WhereAmI : MonoBehaviour
{
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    public enum CurrentLocation
    {
        Cell,
        NarrowHallway,
        CrowdedStudy,
        IronCorridor,
        Laboratory,
        SittingRoom,
        CurtainedRoom,
        CurvingHall,
        CurvingHallSouthEnd,
        DimShed,
        MosaicRoom,
        Atelier,
        CurvingHallPedestal,
        CurvingHallWest,
        NaturalPassage,
        HarpChamber,
        GreyChamber,
        ConfusingPassage,
        DeadEnd,
        LedgeInPit,
        VaultingCavern,
        DeepInPit,
        DepthsInPit,
        Arboretum,
        DarkJungle,
        ShoreOfRiver,
        FarShoreOfRiver,
        RiverCrawl
    }

    public enum MoveDirections
    {
        north,
        east,
        south,
        west
    }
    
    MoveDirections Direction;

    [SerializeField]
    CurrentLocation roomName;

    public Animator animator;

    void Start()
    {
        keyActs.Add("east", isEast);
        keyActs.Add("west", isWest);
        keyActs.Add("north", isNorth);
        keyActs.Add("south", isSouth);
        keyActs.Add("right", isEast);
        keyActs.Add("left", isWest);
        keyActs.Add("up", isNorth);
        keyActs.Add("down", isSouth);
        keyActs.Add("go right", isEast);
        keyActs.Add("go left", isWest);
        keyActs.Add("go up", isNorth);
        keyActs.Add("go down", isSouth);
        keyActs.Add("go east", isEast);
        keyActs.Add("go west", isWest);
        keyActs.Add("go north", isNorth);
        keyActs.Add("go south", isSouth);

        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void isEast()
    {
        Direction = MoveDirections.east;
        GoToNextRoom();
    }

    public void isWest()
    {
        Direction = MoveDirections.west;
        GoToNextRoom();
    }

    public void isNorth()
    {
        Direction = MoveDirections.north;
        GoToNextRoom();
    }

    public void isSouth()
    {
        Direction = MoveDirections.south;
        GoToNextRoom();
    }


    
    public void GoToNextRoom()
    {

        switch (roomName)
        {
            case CurrentLocation.Cell:
                switch (Direction)
                {
                    case MoveDirections.north:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.east:
                        animator.Play("Go_NarrowHallway");
                        roomName = CurrentLocation.NarrowHallway;
                        break;
                    case MoveDirections.south:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.west:
                        Debug.Log("Not possible");
                        break;
                }
                break;
            case CurrentLocation.NarrowHallway:
                switch (Direction)
                {
                    case MoveDirections.north:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.east:
                        animator.Play("Go_CrowdedStudy");
                        roomName = CurrentLocation.CrowdedStudy;
                        break;
                    case MoveDirections.south:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.west:
                        animator.Play("Go_Cell");
                        roomName = CurrentLocation.Cell;
                        break;
                }
                break;
            case CurrentLocation.CrowdedStudy:
                switch (Direction)
                {
                    case MoveDirections.north:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.east:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.south:
                        animator.Play("Go_IronCorridor");
                        roomName = CurrentLocation.IronCorridor;
                        break;
                    case MoveDirections.west:
                        animator.Play("Go_NarrowHallway");
                        roomName = CurrentLocation.NarrowHallway;
                        break;
                }
                break;
            case CurrentLocation.IronCorridor:
                switch (Direction)
                {
                    case MoveDirections.north:
                        animator.Play("Go_CrowdedStudy");
                        roomName = CurrentLocation.CrowdedStudy;
                        break;
                    case MoveDirections.east:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.south:
                        Debug.Log("Not possible");
                        break;
                    case MoveDirections.west:
                        Debug.Log("Not possible");
                        break;
                }
                break;
        }
    }

    public void possibleDirections(CurrentLocation north, CurrentLocation east, CurrentLocation south, CurrentLocation west)
    {
        switch (Direction)
        {
            case MoveDirections.north:
                animator.Play("Go_"+ north.ToString());
                roomName = north;
                break;
            case MoveDirections.east:
                animator.Play("Go_" + east.ToString());
                roomName = east;
                break;
            case MoveDirections.south:
                animator.Play("Go_" + south.ToString());
                roomName = south;
                break;
            case MoveDirections.west:
                animator.Play("Go_" + west.ToString());
                roomName = west;
                break;
        }
    }

}
