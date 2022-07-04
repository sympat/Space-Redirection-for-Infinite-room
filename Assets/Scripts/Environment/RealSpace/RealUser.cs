using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealUser : Circle2D
{
    private Users trackedUsers;
    private Vector2 offsetPosition;
    private float offsetRotation;
    private bool isFirstEnter;
    
    public override void Initializing() {
        base.Initializing();
        this.gameObject.layer = LayerMask.NameToLayer("RealUser");
    }

    private void Start() {
        trackedUsers = this.transform.parent.parent.GetComponent<Environment>().users;
        User trackedUser = trackedUsers.GetActiveUser();

        offsetPosition = this.Position - trackedUser.Body.Position;
        offsetRotation = this.Rotation - trackedUser.Body.Rotation;
    }

    private void OnTriggerEnter(Collider other) { // Unity 자체에서 Real User의 TriggerEnter Event가 발생했을 때 이것을 상위 User 객체에 위임함
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            UserEventArgs caller = new UserEventArgs(Behaviour.Enter, other.gameObject);
            user.ProcessingEvent(caller);
        }
    }

    private void OnTriggerExit(Collider other) { // Unity 자체에서 Real User의 TriggerExit Event가 발생했을 때 이것을 상위 User 객체에 위임함
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            UserEventArgs caller = new UserEventArgs(Behaviour.Exit, other.gameObject);
            user.ProcessingEvent(caller);
            isFirstEnter = true;
        }
    }

    private void OnTriggerStay(Collider other) { // Unity 자체에서 Real User의 TriggerStay Event가 발생했을 때 이것을 상위 User 객체에 위임함
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            if(other.GetComponent<Bound2D>().IsInSide(this)) {
                if(isFirstEnter) {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyEnter, other.gameObject);
                    user.ProcessingEvent(caller);
                    isFirstEnter = false;
                }
                else {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyStay, other.gameObject);
                    user.ProcessingEvent(caller);
                }
            }
            else {
                UserEventArgs caller = new UserEventArgs(Behaviour.Stay, other.gameObject);
                user.ProcessingEvent(caller);
            }
        }
    }

    private void FixedUpdate() {
        User trackedUser = trackedUsers.GetActiveUser();

        this.Position = trackedUser.Body.Position + offsetPosition;
        this.Rotation = trackedUser.Body.Rotation + offsetRotation;
    }
}
