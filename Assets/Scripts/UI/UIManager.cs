using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;

// UI 관련 메타 정보를 나타냄
[System.Serializable]
public class UIContainer {
    public UICanvas canvas;
    public Vector3 initPosition;
    public Vector3 initRotation;
    public bool attachToUser;
    [HideInInspector]
    public Transform parent; // UI의 부모 좌표계를 나타냄. 없다면 월드 좌표계를 기준으로 생성
}

public enum UIBehaviour {
    Click
}

// UI를 관리하는 단일 매니저 클래스를 나타냄
public class UIManager : Singleton<UIManager>
{
    
    private Dictionary<string, UICanvas> ui = new Dictionary<string, UICanvas>(); // task에 필요한 모든 UICanvas를 저장
    private Dictionary<string, UIContainer> initInfo = new Dictionary<string, UIContainer>(); // 얘가 왜 필요하지?

    public void GenerateUI(string name, UIContainer uiInfo) {
        UICanvas instantiatedUI = Instantiate(uiInfo.canvas);

        if(uiInfo.parent) {
            instantiatedUI.transform.SetParent(uiInfo.parent.transform);

            instantiatedUI.transform.localPosition = uiInfo.initPosition;
            instantiatedUI.transform.localRotation = Quaternion.Euler(uiInfo.initRotation);
        }
        else {
            instantiatedUI.transform.position = uiInfo.initPosition;
            instantiatedUI.transform.rotation = Quaternion.Euler(uiInfo.initRotation);
        }

        instantiatedUI.name = name;
        ui.Add(name, instantiatedUI);
        initInfo.Add(name, uiInfo);
        ui[name].gameObject.SetActive(false);
    }

    public UICanvas GetUI(string name) {
        return ui[name];
    }

    public void DestroyUI(string name) {
        Destroy(ui[name]);
        ui.Remove(name);
    }

    public void ToggleUIBase(string name, bool enabled, params string[] childName) {
        Transform target = ui[name].transform;

        foreach(string targetName in childName) {
            target = target.Find(targetName);
        }

        target.gameObject.SetActive(enabled);
    }

    public void ToggleUICanvas(string name, bool enabled, User user, bool useLocal = false) {

        if(enabled) {
            if(useLocal) {
                ui[name].transform.SetParent(this.transform);
                ui[name].transform.position = user.Face.transform.TransformPoint(initInfo[name].initPosition);
            }
        }
        else {
            if(useLocal) {
                ui[name].transform.SetParent(user.Face.transform);
                ui[name].transform.localPosition = initInfo[name].initPosition;
                ui[name].transform.localRotation = Quaternion.Euler(initInfo[name].initRotation);
            }
        }
        
        ui[name].gameObject.SetActive(enabled);
    }

    public void AddEvent(UIBehaviour eventType, string name, UnityAction call, params string[] childName) { // 주어진 UICanvas의 모든 하위 BaseUI에 대해서 어떤 행동을 했을때 일어날 Event를 추가
        Transform target = ui[name].transform;

        foreach(string targetName in childName) {
            target = target.Find(targetName);
        }

        target.GetComponent<BaseUI>().AddEvent(eventType, call);
    }

     public void RemoveEvent(UIBehaviour eventType, string name, UnityAction call, params string[] childName) { // 위 함수와 반대로 등록된 Event를 제거
        Transform target = this.transform;
  
        foreach(string targetName in childName) {
            target = target.Find(targetName);
        }

        target.GetComponent<BaseUI>().RemoveEvent(eventType, call);
    }
}
