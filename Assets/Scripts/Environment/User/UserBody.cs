using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserBody : Circle2D
{
    private float _deltaRotation;    
    private Vector2 _deltaPosition;
    private Vector2 _previousPosition;
    private float _previousRotation;
    private Vector2 _previousForward;
    private bool isFirstEnter;

    public User parentUser {
        get { return transform.parent.GetComponent<User>(); }
    }

    public float deltaRotation {
        get { return _deltaRotation; }
    }

    public Vector2 deltaPosition {
        get { return _deltaPosition; }
    }

    private void Start() {
        ResetCurrentState();
    }
    
    private void FixedUpdate() {
        _deltaPosition = (this.Position - _previousPosition) / Time.fixedDeltaTime;
        _deltaRotation = Vector2.SignedAngle(_previousForward, this.Forward) / Time.fixedDeltaTime;

        _previousPosition = this.Position;
        _previousForward = this.Forward;
    }

    private void ResetCurrentState()
    {
        _deltaPosition = Vector2.zero;
        _deltaRotation = 0;
        _previousPosition = this.Position;
        _previousForward = this.Forward;
    }

    private void OnTriggerEnter(Collider other) { // Unity 자체에서 User Body의 TriggerEnter Event가 발생했을 때 이것을 상위 User 객체에 위임함
        UserEventArgs caller = new UserEventArgs(Behaviour.Enter, other.gameObject);
        parentUser.ProcessingEvent(caller);
    }

    private void OnTriggerExit(Collider other) { // Unity 자체에서 User Body의 TriggerExit Event가 발생했을 때 이것을 상위 User 객체에 위임함
        UserEventArgs caller = new UserEventArgs(Behaviour.Exit, other.gameObject);
        parentUser.ProcessingEvent(caller);
        isFirstEnter = true;
    }

    private void OnTriggerStay(Collider other) { // Unity 자체에서 User Body의 TriggerStay Event가 발생했을 때 이것을 상위 User 객체에 위임함

        if(other.GetComponent<Bound2D>() != null) {
            if(other.GetComponent<Bound2D>().IsInSide(this)) {
                if(isFirstEnter) {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyEnter, other.gameObject);
                    parentUser.ProcessingEvent(caller);
                    isFirstEnter = false;
                }
                else {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyStay, other.gameObject);
                    parentUser.ProcessingEvent(caller);
                }
            }
        }
    }
}
