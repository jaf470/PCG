using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

/* Script for Procedural Level Generation */

public class PCG : MonoBehaviour {

    public static PCG PCGSingleton;

    private string playerPosition;

    /* RNG value.  A value of 100 means it will definitely happen.
     * Zero means it will never happen
     */
    public int rngHundred = 50;

    /* Number of rooms to generate */
    private int numberOfRoomsDesired;

    private int numberOfRoomsGenerated = 0;

    private List<Vector2> allRoomCoordinates = new List<Vector2>();

    /* Current scene the player is looking at */
    private PCGScene currentScene;

    private PCGScene startScene;

    private PCGScene lastScene;
    /* must not exceed our room difficulty
     * eg. Do not put 5 if we have no 5_ rooms
     */
    public int maxDifficulty = 4;

    /* This can be any value as it reuses rooms */
    public int rmsPerDif = 3;

    /* Where we are at in our list of rooms */
    private int sceneIndex = 0;

    private int listIndex = 1;

    List<PCGScene> sceneOrder = new List<PCGScene>();


    //Create a static instance of this class on Start
    void Start () {

        if (PCGSingleton == null)
        {
            DontDestroyOnLoad(gameObject);
            PCGSingleton = this;            
            RoomStartup();           
        }
        else if (PCGSingleton != this)
        {
            Destroy(gameObject);
        }
        
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire3"))
            Reset();
    }

    public int RoomDifficulty()
    {
        return currentScene.GetSceneDifficulty();
    }

    private void PositionPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        switch (playerPosition)
        {
            case "North":
                player.transform.position = new Vector2(0, 1.5f);
                break;
            case "South":
                player.transform.position = new Vector2(0, -1.5f);
                break;
            case "West":
                player.transform.position = new Vector2(-5, 0);
                break;
            case "East":
                player.transform.position = new Vector2(5, 0);
                break;
            default:
                player.transform.position = new Vector2(0, 0);
                break;           
        }
    }

    private void RoomStartup()
    {
        currentScene = new PCGScene("Start");
        currentScene.RoomCoordinates = new Vector2(0, 0);
        startScene = currentScene;
        allRoomCoordinates.Add(currentScene.RoomCoordinates);
        GenerateRoomOrder();
        GenerateRoomPositions();
        StartCoroutine(SetDoorsInScene());
        Minimap.miniMap.AddRoomToMinimap(startScene.RoomCoordinates);
    }

    /* Uses currentScene and examines adjacent coordinates to
     * set which rooms can be used for transition
     */
    private void SetDoorAvailability()
    {
        Vector2 coords = currentScene.RoomCoordinates;

        if (allRoomCoordinates.Contains(coords + Vector2.up))
            currentScene.BlockDoor("NorthDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.right))
            currentScene.BlockDoor("EastDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.down))
            currentScene.BlockDoor("SouthDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.left))
            currentScene.BlockDoor("WestDoor");

    }

    private void SetDoorAvailability(PCGScene scene)
    {
        Vector2 coords = scene.RoomCoordinates;

        if (allRoomCoordinates.Contains(coords + Vector2.up))
            scene.BlockDoor("NorthDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.right))
            scene.BlockDoor("EastDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.down))
            scene.BlockDoor("SouthDoor");
        if (allRoomCoordinates.Contains(coords + Vector2.left))
            scene.BlockDoor("WestDoor");

    }
    /* Generate New Room for PCGScene */
    private PCGScene GenerateNewRoom()
    {
        if (listIndex > (rmsPerDif * maxDifficulty))
        {
            PCGScene finalRoom = new PCGScene("BossRoom");
            return finalRoom;
        }
        else
        {
            PCGScene newRoom = new global::PCGScene(sceneOrder[listIndex].GetSceneName());
            listIndex++;

            return newRoom;
        }
    }

    /* Room Transition Script.  Gets called by SceneTransition script */
    public void RoomTransition(string direction)
    {
        PCGScene temp = currentScene;

        if (direction == "East")
        {            
            currentScene.EastRoom.WestRoom = temp;            
            currentScene = currentScene.EastRoom;
            playerPosition = "West";
        }
        if (direction == "West")
        {
            currentScene.WestRoom.EastRoom = temp;
            currentScene = currentScene.WestRoom;
            playerPosition = "East";
        }
        if (direction == "South")
        {
            currentScene.SouthRoom.NorthRoom = temp;
            currentScene = currentScene.SouthRoom;
            playerPosition = "North";
        }
        if (direction == "North")
        {
            currentScene.NorthRoom.SouthRoom = temp;
            currentScene = currentScene.NorthRoom;
            playerPosition = "South";
        }

        Minimap.miniMap.AddRoomToMinimap(currentScene.RoomCoordinates);
        LoadScene();
    }

    public Vector2 RoomCoordinates()
    {
        return currentScene.RoomCoordinates;
    }

    private void PCGRoomTransition(string direction)
    {

        PCGScene temp = currentScene;
        lastScene = temp;

        Vector2 coordinates = currentScene.RoomCoordinates;
        Vector2 newCoordinates = new Vector2();

        //Debug.Log("Direction is " + direction);

        if (direction == "EastDoor")
        {
            if (currentScene.EastRoom == null)
                currentScene.EastRoom = GenerateNewRoom();
            else
                Debug.Log("PCG East Transition is not null when it should have been");
                  
			currentScene.EastRoom.WestRoom = temp;
            currentScene = currentScene.EastRoom;
            newCoordinates = coordinates + Vector2.right;          
        }

        if (direction == "WestDoor")
        {
            if (currentScene.WestRoom == null)            
                currentScene.WestRoom = GenerateNewRoom();
            else
                Debug.Log("PCG West Transition is not null when it should have been");

            currentScene.WestRoom.EastRoom = temp;
            currentScene = currentScene.WestRoom;
            newCoordinates = coordinates + Vector2.left;
        }
        if (direction == "SouthDoor")
        {
            if (currentScene.SouthRoom == null)            
                currentScene.SouthRoom = GenerateNewRoom();
            else
                Debug.Log("PCG South Transition is not null when it should have been");

            currentScene.SouthRoom.NorthRoom = temp;
            currentScene = currentScene.SouthRoom;
            newCoordinates = coordinates + Vector2.down;
        }
        if (direction == "NorthDoor")
        {
            if (currentScene.NorthRoom == null)            
                currentScene.NorthRoom = GenerateNewRoom();
            else
                Debug.Log("PCG North Transition is not null when it should have been");

            currentScene.NorthRoom.SouthRoom = temp;
            currentScene = currentScene.NorthRoom;
            newCoordinates = coordinates + Vector2.up;            
        }

            currentScene.RoomCoordinates = newCoordinates;
            allRoomCoordinates.Add(newCoordinates);
            SetDoorAvailability();
       
    }

    public void Reload()
    {
        LoadScene();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(currentScene.GetSceneName());
        StartCoroutine(SetDoorsInScene());
        PositionPlayer();
        Minimap.miniMap.ChangeRoom();
        float x;
        float y;
        x = currentScene.RoomCoordinates.x;
        y = currentScene.RoomCoordinates.y;

        Debug.Log("Current Room Coordinates: (" + x + "," + y + ")");
    }

    /* Scene loads at the end of the current frame so this function waits
     * for one frame before setting the doors in the new scene */
    IEnumerator SetDoorsInScene()
    {
        yield return 0;
        currentScene.SetDoors();
    }

    public void MovePlayerToStart()
    {
        currentScene = startScene;
        LoadScene();
    }

    IEnumerator WaitOneFrame()
    {
        yield return 0;
    }

    public void Reset()
    {
        Destroy(PCGSingleton);
        Destroy(gameObject);
        Minimap.miniMap.Reset();        
        StartCoroutine(WaitOneFrame());
        SceneManager.LoadScene(0);
    }

    /* This is to test the edge case that the rooms will generate a loop
     * that leads itself to a dead end
     * eg. the room generation looks like:
     *          XOOOX
     *          XONOX
     *          XOOXX
     * where N is the room picked to be the next room and O are rooms already generated.
     * Because the level can no longer proceed if N is the next room, this
     * case must be detected and a solution must be found for it.
     * 
     */
     
    private bool CheckIfSelectedDoorIsDeadEnd(string room)
    {
        Vector2 testCoords;

        if (room == "NorthDoor")
        {
            testCoords = currentScene.RoomCoordinates + Vector2.up;

            if (allRoomCoordinates.Contains(testCoords + Vector2.up) &&
                allRoomCoordinates.Contains(testCoords + Vector2.left) &&
                allRoomCoordinates.Contains(testCoords + Vector2.right))
            {
                Debug.Log("Path put itself in a dead end after trying to go north.");
                InsertDeadEnd("NorthDoor");
                return false;
            }

        }
        else if (room == "WestDoor")
        {
            testCoords = currentScene.RoomCoordinates + Vector2.left;

            if (allRoomCoordinates.Contains(testCoords + Vector2.up) &&
                allRoomCoordinates.Contains(testCoords + Vector2.left) &&
                allRoomCoordinates.Contains(testCoords + Vector2.down))
            {
                Debug.Log("Path put itself in a dead end after trying to go west.");
                InsertDeadEnd("WestDoor");
                return false;
            }
        }
        else if (room == "EastDoor")
        {
            testCoords = currentScene.RoomCoordinates + Vector2.right;

            if (allRoomCoordinates.Contains(testCoords + Vector2.up) &&
                allRoomCoordinates.Contains(testCoords + Vector2.right) &&
                allRoomCoordinates.Contains(testCoords + Vector2.down))
            {
                Debug.Log("Path put itself in a dead end after trying to go east.");
                InsertDeadEnd("EastDoor");
                return false;
            }
        }
        else if (room == "SouthDoor")
        {
            testCoords = currentScene.RoomCoordinates + Vector2.down;

            if (allRoomCoordinates.Contains(testCoords + Vector2.right) &&
                allRoomCoordinates.Contains(testCoords + Vector2.left) &&
                allRoomCoordinates.Contains(testCoords + Vector2.down))
            {
                Debug.Log("Path put itself in a dead end after trying to go south.");
                InsertDeadEnd("SouthDoor");
                return false;
            }
        }
        return true;
    }
    
    private void InsertDeadEnd(string direction)
    {
        Vector2 coords;

        switch (direction)
        {
            case "EastDoor":
                coords = currentScene.RoomCoordinates + Vector2.right;
                if (allRoomCoordinates.Contains(coords))
                {
                    Debug.Log("Room already exists!  Cannot insert dead end.");
                    break;
                }
                currentScene.EastRoom = new PCGScene("DeadEnd");
                currentScene.EastRoom.RoomCoordinates = coords;
                allRoomCoordinates.Add(coords);
                currentScene.EastRoom.WestRoom = currentScene;
                SetDoorAvailability(currentScene.EastRoom);
                break;
            case "WestDoor":
                coords = currentScene.RoomCoordinates + Vector2.left;
                if (allRoomCoordinates.Contains(coords))
                {
                    Debug.Log("Room already exists!  Cannot insert dead end.");
                    break;
                }
                currentScene.WestRoom = new PCGScene("DeadEnd");
                currentScene.WestRoom.RoomCoordinates = coords;
                allRoomCoordinates.Add(coords);
                currentScene.WestRoom.EastRoom = currentScene;
                SetDoorAvailability(currentScene.WestRoom);
                break;
            case "NorthDoor":
                coords = currentScene.RoomCoordinates + Vector2.up;
                if (allRoomCoordinates.Contains(coords))
                {
                    Debug.Log("Room already exists!  Cannot insert dead end.");
                    break;
                }
                currentScene.NorthRoom = new PCGScene("DeadEnd");
                currentScene.NorthRoom.RoomCoordinates = coords;
                allRoomCoordinates.Add(coords);
                currentScene.NorthRoom.SouthRoom = currentScene;
                SetDoorAvailability(currentScene.NorthRoom);
                break;
            case "SouthDoor":
                coords = currentScene.RoomCoordinates + Vector2.down;
                if (allRoomCoordinates.Contains(coords))
                {
                    Debug.Log("Room already exists!  Cannot insert dead end.");
                    break;                    
                }
                currentScene.SouthRoom = new PCGScene("DeadEnd");
                currentScene.SouthRoom.RoomCoordinates = coords;
                allRoomCoordinates.Add(coords);
                currentScene.SouthRoom.NorthRoom = currentScene;
                SetDoorAvailability(currentScene.SouthRoom);
                break;
        }
    }


    private void GenerateRoomPositions()
    {
        int count;
        string chosenDoor;
        string randomDoor;

        numberOfRoomsDesired = rmsPerDif * maxDifficulty + 1;

        /* Generate the correct path the player should take in this while loop */
        while (numberOfRoomsGenerated < numberOfRoomsDesired)
        {
            List<string> availableRooms = currentScene.GetAvailableDoors();

            count = availableRooms.Count;

            if (count == 0)
            {
                Debug.Log("No Rooms Are Available.  Resetting.");
                Reset();
            }

            if (count > 2)
            {
                if (UnityEngine.Random.Range(0, 101) < rngHundred)
                {
                    randomDoor = availableRooms[UnityEngine.Random.Range(0, count)];
                    InsertDeadEnd(randomDoor);
                    //Debug.Log("Random Room Inserted on Correct Path");
                }
            }

            availableRooms = currentScene.GetAvailableDoors();
            count = availableRooms.Count;

            try
            {
                chosenDoor = availableRooms[UnityEngine.Random.Range(0, count)];
                if (!CheckIfSelectedDoorIsDeadEnd(chosenDoor))
                    continue;

                PCGRoomTransition(chosenDoor);
            }
            catch (Exception e)
            {
                Debug.Log("Exception Caught due to edge case.");
            }           

            numberOfRoomsGenerated++;

            if (numberOfRoomsGenerated == 1)
                Debug.Log("Correct Room Starts at: " + currentScene.RoomCoordinates);
        }

        
        currentScene = startScene;
        listIndex = 1;
        numberOfRoomsGenerated = 0;
        

        /* Generate an incorrect path starting from the start room in this loop */
        
        while (numberOfRoomsGenerated < numberOfRoomsDesired - 1)
        {
            List<string> availableRooms = currentScene.GetAvailableDoors();

            numberOfRoomsGenerated++;
            count = availableRooms.Count;

            if (count == 0)
            {
                Debug.Log("No Rooms Are Available.  Terminating incorrect path generation.");
                break;
            }

            if (count > 2)
            {
                if (UnityEngine.Random.Range(0, 101) < rngHundred)
                {
                    randomDoor = availableRooms[UnityEngine.Random.Range(0, count)];
                    InsertDeadEnd(randomDoor);
                   // Debug.Log("Random Room Inserted on Incorrect Path");
                }
            }

            availableRooms = currentScene.GetAvailableDoors();
            count = availableRooms.Count;

            try
            {
                chosenDoor = availableRooms[UnityEngine.Random.Range(0, count)];

                if (!CheckIfSelectedDoorIsDeadEnd(chosenDoor))
                    continue;

                PCGRoomTransition(chosenDoor);
            }catch (Exception e)
            {
                Debug.Log("Exception caught due to edge case.");
            }
            

        }

        /* Make the final room in the incorrect path a warp room */
        /* A warp room will transport the player back to the start room */
        if (numberOfRoomsGenerated > 1)
            currentScene.SetSceneName("WarpRoom");

        /* Reset the pointer to the start room and begin the game */
        currentScene = startScene;

        if (allRoomCoordinates.Count() != allRoomCoordinates.Distinct().Count())
        {
            /* Duplicate room coordinates exist due to a failed catch of an edge case
             * PCG will restart and try again
             */
            Debug.Log("DUPLICATE ROOM COORDINATES EXIST.");
            Reset();
        }
        LoadScene();
    }
    

    /* Main Method to generate room order.  It takes the
     * difficulty and a random number between 1 and 3
     * to name the scene that will be added to the list
     */

    /* TODO: Create a more robust algorithm so it can be plugged in
     * to the 4-door PCGScene */


    private void GenerateRoomOrder()
    {
        
        
        int sceneToAdd;
        int roomDifficulty = 1;

        //Starting room will always be Start
        sceneOrder.Add(currentScene);

        //Grab Room By Difficulty
        while (roomDifficulty <= maxDifficulty)
        {
            int x = 0;

            //Populate Rooms for this difficulty
            while (x < rmsPerDif)
            {
                sceneToAdd = UnityEngine.Random.Range(1, 4);
                sceneOrder.Add(new PCGScene(roomDifficulty + "_" + sceneToAdd));
                x++;
            }

            roomDifficulty++;
        }

        //Finish With Boss Room
        sceneOrder.Add(new PCGScene("BossRoom"));


        //Print out room order for play testing
        int y = 0;

        while (y < rmsPerDif * maxDifficulty + 2)
        {
            //Debug.Log("Room " + y + " " + "is " + sceneOrder[y].GetSceneName());
            y++;
        }

    }

    /* This method gets called in ScreenTransition, which is a script
     * attached to the portal that does room transitions.  It will return
     * the next PCGScene in the list.
     * 
     * ** This method will soon be removed
     */

    public PCGScene GetNextRoom()
    {

        sceneIndex++;
        if (sceneIndex > (rmsPerDif * maxDifficulty) + 1)
            sceneIndex = 0;

        Debug.Log("Entering Room " + sceneOrder[sceneIndex].GetSceneName());
        return (sceneOrder[sceneIndex]);
    }
  
    
}
