using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public struct UserEventArgs
{
    public GameObject target;
    public Behaviour behaviour;

    public UserEventArgs(Behaviour behaviour) {
        this.target = null;
        this.behaviour = behaviour;
    }

    public UserEventArgs(Behaviour behaviour, GameObject target) { 
        this.target = target; 
        this.behaviour = behaviour; 
    }
}

public enum Behaviour {
    Watch,
    Enter,
    Exit,
    Stay,
    CompletelyEnter,
    CompletelyStay,
    Grab,
    Release,
    Rotate,
    Open,
}

public class UserEvent : UnityEvent<GameObject> {}

public class User : Transform2D
{ 
    private Camera face;
    private UserBody body;
    private Hand[] hands;

    public Camera Face {
        get { return face; }
    }

    public Hand[] Hands {
        get { return hands; }
    }

    public UserBody Body {
        get { return body; }
    }

    private Dictionary<Behaviour, Dictionary<string, UserEvent>> events;

    public override void Initializing()
    {

        face = GetComponentInChildren<Camera>(); // 현재 object에서 camera component를 찾는다
        body = GetComponentInChildren<UserBody>();
        hands = GetComponentsInChildren<Hand>();
        // pointer = GetComponentInChildren<CustomLaserPointer>();
        
        if(body == null) throw new System.Exception("User body(Collider) is required.");
        if(face == null) throw new System.Exception("User face(Camera) is required.");
        if(hands == null) throw new System.Exception("User hand(Hand) is required.");

        // body.gameObject.layer = LayerMask.NameToLayer("Virtual User");

        events = new Dictionary<Behaviour, Dictionary<string, UserEvent>>();

        foreach(Behaviour behave in Enum.GetValues(typeof(Behaviour))) {
            events[behave] = new Dictionary<string, UserEvent>();

            for(int i=0; i<32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if(layerName == "") continue;
                events[behave].Add(layerName, new UserEvent());
            }
        }
    }

    public IEnumerator CallAfterRotation(float degree) {
        Vector2 prevForward = Body.Forward;
        float sumAngle = 0;

        while(true) {

            Vector2 currentForward = Body.Forward;

            float deltaAngle = Vector2.SignedAngle(prevForward, currentForward);
            sumAngle += deltaAngle;


            if(degree < 0) {
                if(sumAngle <= degree)
                    break;
            }
            else {
                if(sumAngle >= degree)
                    break;
            }

            prevForward = currentForward;

            yield return new WaitForFixedUpdate();
        }

        UserEventArgs e = new UserEventArgs(Behaviour.Rotate);
        ProcessingEvent(e);
    }

    public void ToggleHandPointer(bool enabled) {
        foreach(var hand in hands) {
            CustomLaserPointer pointer = hand.GetComponent<CustomLaserPointer>();
            if(pointer != null) pointer.active = enabled;
        }
    }

    public void AddEvent(Behaviour behaviour, string layer, UnityAction<GameObject> call) { // 주어진 layer를 가진 Object에 대해서 User가 어떤 행동을 했을때 일어날 Event를 추가
        int layerToInt = LayerMask.NameToLayer(layer);
        if(layerToInt < 0) throw new System.Exception("InValid Layer index");

        events[behaviour][layer].AddListener(call);
    }

    public void RemoveEvent(Behaviour behaviour, string layer, UnityAction<GameObject> call) { // 위 함수와 반대로 등록된 Event를 제거
        int layerToInt = LayerMask.NameToLayer(layer);
        if(layerToInt < 0) throw new System.Exception("InValid Layer index");

        events[behaviour][layer].RemoveListener(call);
    }

    public void InvokeEvent(Behaviour behaviour, GameObject target) { // 주어진 layer를 가진 Object에 대해서 User가 어떤 행동을 했을때 등록된 Event들을 실행
        if(target == null) events[behaviour]["Default"].Invoke(target);
        else events[behaviour][LayerMask.LayerToName(target.layer)].Invoke(target);
    }

    public void ProcessingEvent(UserEventArgs e) { // UserEventArgs 라는 객체를 인자로 줘서 위 함수를 호출
        InvokeEvent(e.behaviour, e.target);
    }

    public bool IsTargetInUserFov(Vector2 target, float bound = 0) // global 좌표계 기준으로 비교
    {
        Vector2 userToTarget = target - this.body.Position;
        Vector2 userForward = this.body.Forward;

        float unsignedAngle = Vector2.Angle(userToTarget, userForward);

        if (unsignedAngle - ((face.fieldOfView + bound)) < 0.01f)
            return true;
        else
            return false;
    }

    public bool IsTargetInUserFov(Vector2 start, Vector2 end, float bound = 0) {
        Vector2 userToStart = start - this.body.Position;
        Vector2 userToEnd = end - this.body.Position;
        Vector2 userForward = this.body.Forward;
        
        float angleUserToStart = Vector2.SignedAngle(userForward, userToStart);
        float angleUserToEnd = Vector2.SignedAngle(userForward, userToEnd);
        float angleStartToEnd = Vector2.Angle(userToStart, userToEnd);

        if(angleUserToStart * angleUserToEnd < 0 && Mathf.Abs(angleUserToStart) + Mathf.Abs(angleUserToEnd) < angleStartToEnd + 0.01f) {
            return true;
        }
        else if(IsTargetInUserFov(start, bound) || IsTargetInUserFov(end, bound)) {
            return true;
        }
        else {
            return false;
        }
    }

}
