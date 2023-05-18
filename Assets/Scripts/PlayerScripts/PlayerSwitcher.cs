using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSwitcher : MonoBehaviour
{
    PlayerAction actions;
    int currentIndex = 1;
    Player _player;
    [SerializeField] GameObject[] CharactersToSwitchTo;
    void Start()
    {
        _player = FindObjectOfType<Player>();
        actions = new PlayerAction();
        actions.Player.Enable();
        actions.Player.Switch1.performed += Switch1;
        actions.Player.Switch2.performed += Switch2;
        actions.Player.Switch3.performed += Switch3;
        actions.Player.Switch4.performed += Switch4;
    }
    public void Switch1(InputAction.CallbackContext context)
    {
        if(context.performed && currentIndex != 1)
        {
            Switcher(0);
            currentIndex = 1;
        }
    }
    public void Switch2(InputAction.CallbackContext context)
    {
        if (context.performed && currentIndex != 2)
        {
            Switcher(1);
            currentIndex = 2;
        }
    }
    public void Switch3(InputAction.CallbackContext context)
    {
        if (context.performed && currentIndex != 3)
        {
            Switcher(2);
            currentIndex = 3;
        }
    }
    public void Switch4(InputAction.CallbackContext context)
    {
        if (context.performed && currentIndex != 4)
        {
            Switcher(3);
            currentIndex = 4;
        }
    }
    void Switcher(int index)
    {
        GameObject Temp = Instantiate(CharactersToSwitchTo[index], _player.transform.position, _player.transform.rotation);
        Player TempPlayer = Temp.GetComponent<Player>();
        Temp.GetComponent<Player>().Health = _player.Health;
        TempPlayer.RespawnPoint = _player.RespawnPoint;
        TempPlayer._jumpPhase = 5;
        Temp.GetComponent<Rigidbody>().velocity = _player.GetComponent<Rigidbody>().velocity;
        CameraScript temp = FindObjectOfType<CameraScript>();
        temp.focus = Temp.transform;
        Destroy(_player.gameObject);
        _player = Temp.GetComponent<Player>();
    }
}