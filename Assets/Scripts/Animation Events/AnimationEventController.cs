using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    // any animation events we want to run on certain frames can be triggered in here
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void DestroyParent()
    {
        Destroy(gameObject.transform.root.gameObject);
    }

}
