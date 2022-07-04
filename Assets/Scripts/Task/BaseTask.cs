using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

// 해당 level에서 진행되는 task를 FSM 형태로 저장하고 
// task 중에 사용되는 UI들을 입력받고 UI 관리는 내부적으로 UIManager를 호출해서 진행
public class BaseTask<TState, TInput> : MonoBehaviour 
where TState : Enum
where TInput : Enum
{
    // manager를 굳이 분리한 이유는 Task 여러 개가 동시에 진행될 수 있기 때문
    public UIContainer[] uiInfo;
    protected FiniteStateMachine<TState, TInput> task = new FiniteStateMachine<TState, TInput>();

    protected Environment environment {
        get { return GetComponent<Environment>(); }
    }

    public Users users {
        get { return environment.users; }
    }

    public VirtualSpace virtualEnvironment {
        get { return environment.virtualEnvironment; }
    }

    public RealSpace realSpace {
        get { return environment.realSpace; }
    }

    public FiniteStateMachine<TState, TInput> Task {
        get { return task; }
    }

    protected void Start() {
        GenerateTask();
    }

    protected virtual void GenerateTask() {
        // Debug for task process
        // task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        // task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        // task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        // task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });
    }

    protected void GenerateUI(string name, UIContainer uiInfo) {
        User user = users.GetActiveUser();
        if(uiInfo.attachToUser) {
            uiInfo.parent = user.Face.transform;
        }

        UIManager.Instance.GenerateUI(name, uiInfo);
    }

    protected void EnableUI(string name, bool useLocal = false) {
        User user = users.GetActiveUser();

        UIManager.Instance.ToggleUICanvas(name, true, user, useLocal);
        user.ToggleHandPointer(true); 
    }

    protected void DisableUI(string name, bool useLocal = false, bool disableHandPointer = true) {
        User user = users.GetActiveUser();

        UIManager.Instance.ToggleUICanvas(name, false, user, useLocal); 
        if(disableHandPointer) user.ToggleHandPointer(false);
    }

    protected void AddTaskEvent(TInput input, UIBehaviour behaviour, string name, params string[] childName) {
        User user = users.GetActiveUser();


        UIManager.Instance.AddEvent(behaviour, name, () => task.Processing(input), childName);
    }

    // user가 name을 가지는 Object에 대해서 behaviour를 행했을 때 input을 주고 FSM의 Processing을 실행함 (input을 FSM에 전달함)
    //TODO: Naming 개선?
    protected void AddTaskEvent(TInput input, Behaviour behaviour, string name) { 
        User user = users.GetActiveUser();

        user.AddEvent(behaviour, name, (_) => task.Processing(input));
    }

    public void CallInputAfterSeconds(float time, TInput input) {
        StartCoroutine(_CallInputAfterSeconds(time, input));
    }

    public IEnumerator _CallInputAfterSeconds(float time, TInput input) {
        yield return new WaitForSeconds(time);
        task.Processing(input);
    }
}

