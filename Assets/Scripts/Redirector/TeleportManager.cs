using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    protected CoinCollectTask experiment2;

    private void Awake() {
        experiment2 = GetComponent<CoinCollectTask>();
    }

    private void FixedUpdate() {
        experiment2.isLocomotionDone = true;
    }
}
