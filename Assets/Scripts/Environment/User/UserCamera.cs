using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCamera : MonoBehaviour
{
    public LayerMask viewLayerMask;

    public User parentUser {
        get { return transform.parent.GetComponent<User>(); }
    }

    // Update is called once per frame
    private void Update() // 매 번 Update가 될때마다 Watch Event가 발생했는지 검사하고 이것을 상위 User 객체에 위임함
    {        
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool bHit = Physics.Raycast(raycast, out hit, Mathf.Infinity);

        if(bHit) {
            UserEventArgs caller = new UserEventArgs(Behaviour.Watch, hit.transform.gameObject);
            parentUser.ProcessingEvent(caller);
        }
    }

    
}
