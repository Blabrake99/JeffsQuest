using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Pause : MonoBehaviour
{
    [SerializeField]
    GameObject PausePage;

    public GameObject PauseFirstButton;

    GameObject player;

    float pauseTimer;

    public bool isPaused;

    //PlayerSwitcher Ps;
    PlayerAction actions;
    void Start()
    {
        //player = FindObjectOfType<PlayerMovement>().gameObject;
        actions.Player.Enable();
        actions.Player.Pause.performed += OnPause;
        //Ps = GetComponent<PlayerSwitcher>();
        if (PausePage == null)
        {
            PausePage = GameObject.FindGameObjectWithTag("Pause_Menu");
            PausePage.SetActive(false);
        }
    }
    void Update()
    {
        //if (player != null)
        //{
        //    if (PausePage != null)
        //    {
        //        if (InputManager.GetButtonDown("Pause") && pauseTimer <= 0)
        //        {
        //            isPaused = !isPaused;
        //            pause(isPaused);
        //            pauseTimer = .2f;
        //        }
        //    }
        //}
        //else
        //{
        //    player = PhotonFindCurrentClient();
        //}
        pauseTimer -= Time.unscaledDeltaTime;
    }
    //GameObject PhotonFindCurrentClient()
    //{
    //    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

    //    foreach (GameObject g in players)
    //    {
    //        if (g.GetComponent<PhotonView>().IsMine)
    //            return g;
    //    }
    //    return null;
    //}
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed && pauseTimer <= 0)
        {
            pauseTimer = .2f;
            //Ps.CanSwitch = isPaused;
            if (isPaused)
            {
                //player.GetComponent<PlayerMovement>().CantMove = true;
                //player.GetComponent<PlayerMovement>().StopAllAnimations();
                GameManager.unlockCursor();
                PausePage.SetActive(true);
                //if (EventSystem.current != PauseFirstButton)
                //{
                //    EventSystem.current.SetSelectedGameObject(null);
                //    EventSystem.current.SetSelectedGameObject(PauseFirstButton);
                //}

                //setSliderValue();

            }
            else
            {
                //player.GetComponent<PlayerMovement>().CantMove = false;
                GameManager.lockCursor();
                PausePage.SetActive(false);
            }
        }
    }


    //private void setSliderValue()
    //{
    //    GameObject[] sliders = GameObject.FindGameObjectsWithTag("SoundSlider");
    //    for (int i = 0; i < sliders.Length; i++)
    //    {
    //        sliders[i].GetComponent<SoundSliders>().setVolumeLevel();
    //    }
    //}
}
