using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// task를 진행하는 모든 UI들의 최상위 Wrapper 클래스
public class UICanvas : Transform2D
{
    private void Awake() {
        foreach(Transform child in this.transform) {
            if(child.GetComponent<BaseUI>() == null) { // 하위에 있는 UI(Text, Image 등)에 BaseUI 라는 클래스를 추가
                child.gameObject.AddComponent<BaseUI>();
            }
        }
    }
}
