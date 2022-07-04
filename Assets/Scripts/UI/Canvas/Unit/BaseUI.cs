using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

// UI를 나타내는 기본 단위 클래스
public class BaseUI : MonoBehaviour, IPointerClickHandler
{
    private Dictionary<UIBehaviour, UnityEvent> uiEvents; // 해당 UI에 대해서 어떤 행동을 했을 때 일어나는 Event를 저장

    private void Awake() {
        foreach(Transform child in this.transform) {
            if(child.GetComponent<BaseUI>() == null) { // 하위에 있는 UI(Text, Image 등)에 BaseUI 라는 클래스를 추가 (TODO: UICanvas 에서 이를 다 해결하도록 개선 가능)
                child.gameObject.AddComponent<BaseUI>();
            }
        }

        uiEvents = new Dictionary<UIBehaviour, UnityEvent>();

        foreach(UIBehaviour behave in Enum.GetValues(typeof(UIBehaviour))) { // 정의된 행동 enumeration에 대해서 Event 객체를 생성
            uiEvents.Add(behave, new UnityEvent());
        }
    }

    public void AddEvent(UIBehaviour eventType, UnityAction call) { // 주어진 행동을 했을 때 일어날 Event를 추가
        uiEvents[eventType].AddListener(call);
    }

    public void RemoveEvent(UIBehaviour eventType, UnityAction call) { // 위 함수와 반대로 등록된 Event를 제거
        uiEvents[eventType].RemoveListener(call);
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        // Debug.Log("ONPointerClick!");
        uiEvents[UIBehaviour.Click].Invoke();
    }

    // public BaseUI AssignText(string text) {
    //     this.transform.GetComponentInChildren<TextMeshProUGUI>().SetText(text);

    //     return this;
    // }

    // public BaseUI AddClickEvent(int index, UnityAction call)  {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     buttons[index].onClick.AddListener(call);

    //     return this;
    // }

    // public BaseUI AddClickEvent(UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     foreach(Button button in buttons) {
    //         button.onClick.AddListener(call);
    //     }

    //     return this;
    // }

    // public BaseUI RemoveClickEvent(int index, UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     buttons[index].onClick.RemoveListener(call);

    //     return this;

    // }

    // public BaseUI RemoveClickEvent(UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     foreach(Button button in buttons) {
    //         button.onClick.RemoveListener(call);
    //     }

    //     return this;
    // }
}
