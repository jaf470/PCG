using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/* Script to handle room transitions */

public class SceneTransition : MonoBehaviour {

    private bool isColliding = false;
    public bool isLocked = false;

    private Animator anim;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        isColliding = false;
        anim.SetBool("locked", isLocked);
    }
    /* On entry the method will call our singleton PCG script
     * and grab the next room from the list
     */
     //Colliders for the doors
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && isColliding == false)
        {
            isColliding = true;
            //SceneManager.LoadScene(PCG.PCGSingleton.GetNextRoom().GetSceneName());
            if (gameObject.tag == "EastDoor")
            {
                PCG.PCGSingleton.RoomTransition("East");
            }
            else if (gameObject.tag == "WestDoor")
            {
                PCG.PCGSingleton.RoomTransition("West");
            }
            else if (gameObject.tag == "NorthDoor")
            {
                PCG.PCGSingleton.RoomTransition("North");
            }
            else if (gameObject.tag == "SouthDoor")
            {
                PCG.PCGSingleton.RoomTransition("South");
            }
        }
    }

    public void lockPortals()
    {
        isLocked = true;
    }

    public void unlockPortals()
    {
        isLocked = false;
    }

}
