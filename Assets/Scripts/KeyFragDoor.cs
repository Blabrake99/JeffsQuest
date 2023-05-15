using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFragDoor : MonoBehaviour
{
    [SerializeField] int requiredFragments = 1;
    int collectedFragments = 0;
    bool openDoor = false;
    Transform player;
    Transform _transform;
    private void Start()
    {
        player = FindObjectOfType<Player>().gameObject.transform;
        _transform = transform;
    }
    private void Update()
    {
        if(openDoor)
        {
            if (Vector3.Distance(player.position, _transform.position) < 3)
                Destroy(gameObject);
        }
    }
    public void GotKeyFragment(GameObject fragment)
    {
        collectedFragments += 1;
        if (collectedFragments >= requiredFragments)
            OpenDoor();
        Destroy(fragment);
    }
    void OpenDoor()
    {
        openDoor = true;
    }
}
