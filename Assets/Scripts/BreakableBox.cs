using System.IO;
using UnityEngine;

public class BreakableBox : MonoBehaviour, IActions
{
    public void Action()
    {
        var brokenBoxPrefab = Resources.Load(Path.Combine("PhotonPrefabs", "BrokenBox"));
        Instantiate(brokenBoxPrefab, transform.position, transform.rotation);
        Destroy(this.gameObject, .1f);
        //cam = FindObjectOfType<MainCamera>();
        //cam.GetComponent<MainCamera>().Shake(.1f, .1f);
        //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BrokenBox"), transform.position, transform.rotation);
        //PhotonNetwork.Destroy(photonView);
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "PlayerAttack")
            Action();
    }
}