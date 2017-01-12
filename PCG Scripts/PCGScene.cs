using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
/* Custom class for our scenes
 * This class stores the scene and adds extra data
 * to help us design the room, such as nodes to other rooms
 */

public class PCGScene{


    public string thisScene;

    /* Nodes to store adjacent rooms */

    private PCGScene northRoom;
    private PCGScene southRoom;
    private PCGScene westRoom;
    private PCGScene eastRoom;

    /* Doors in this scene */
    /* Type must be bool because the scene where the doors
     * exist won't be loaded when this object is created */
    private bool northDoor = false;
    private bool southDoor = false;
    private bool westDoor = false;
    private bool eastDoor = false;

    private bool northDoorBlocked = false;
    private bool southDoorBlocked = false;
    private bool westDoorBlocked = false;
    private bool eastDoorBlocked = false;

    /* Variables to contain information about the room.
     * This can be expanded upon to contain whatever we need
     */

    private int numberOfEnemies;

    private bool isCorrectPath;
    private Vector2 roomCoordinates;

    private int sceneDifficulty;

    public PCGScene(string scene)
    {
        thisScene = scene;
        SetSceneDifficulty(scene);
        
    }

    private void SetSceneDifficulty(string scene)
    {
        char[] diff = scene.ToCharArray();
        char sceneDiff = diff[0];

        double s = char.GetNumericValue(sceneDiff);
        sceneDifficulty = Convert.ToInt32(s);

        if (sceneDifficulty == -1)
        {
            sceneDifficulty = 2;
        }
    }

    public int GetSceneDifficulty()
    {
        return sceneDifficulty;
    }

    public string GetSceneName()
    {
        return thisScene;
    }

    public void SetSceneName(string sceneName)
    {
        thisScene = sceneName;
    }

    public PCGScene NorthRoom
    {
        get{return northRoom;}
        set
        {
            northRoom = value;
            northDoor = true;
        }
    }

    public PCGScene SouthRoom
    {
        get{return southRoom;}
        set
        {
            southRoom = value;
            southDoor = true;
        }
    }

    public PCGScene EastRoom
    {
        get{return eastRoom;}
        set
        {
            eastRoom = value;
            eastDoor = true;
        }
    }

    public PCGScene WestRoom
    {
        get{return westRoom;}
        set
        {
            westRoom = value;
            westDoor = true;
        }
    }

    public bool CorrectRoom
    {
        get { return isCorrectPath; }
        set { isCorrectPath = value; }
    }

    public Vector2 RoomCoordinates
    {
        get { return roomCoordinates; }
        set { roomCoordinates = value; }
    }

    public void SetDoors()
    {
        GameObject north = GameObject.FindGameObjectWithTag("NorthDoor");
        GameObject south = GameObject.FindGameObjectWithTag("SouthDoor");
        GameObject west = GameObject.FindGameObjectWithTag("WestDoor");
        GameObject east = GameObject.FindGameObjectWithTag("EastDoor");

        //Debug.Log("North Door is " + northDoor);

        if (north != null)
            north.SetActive(northDoor);
        if (south != null)
            south.SetActive(southDoor);
        if (west != null)
            west.SetActive(westDoor);
        if (east != null)
            east.SetActive(eastDoor);
    }

    public List<string> GetAvailableDoors()
    {
        List<string> roomList = new List<string>();

        if (!northDoor && !northDoorBlocked)
            roomList.Add("NorthDoor");
        if (!southDoor && !southDoorBlocked)
            roomList.Add("SouthDoor");
        if (!westDoor && !westDoorBlocked)
            roomList.Add("WestDoor");
        if (!eastDoor && !eastDoorBlocked)
            roomList.Add("EastDoor");

        return roomList;
    }

    public void BlockDoor(string door)
    {
        switch (door)
        {
            case "NorthDoor":
                northDoorBlocked = true;
                break;
            case "SouthDoor":
                southDoorBlocked = true;
                break;
            case "WestDoor":
                westDoorBlocked = true;                
                break;
            case "EastDoor":
                eastDoorBlocked = true;                
                break;
        }
    }
}
