using UnityEngine;

public class Fading_Screen : MonoBehaviour
{
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void FadeOut()
    {
        anim.SetBool("FadeOut", true);
    }
    public void FadeIn()
    {
        anim.SetBool("FadeOut", false);
    }
}