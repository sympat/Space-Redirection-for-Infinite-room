﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TTResetState {
    Idle,
    Rotating,
    Translating,
}

public enum TTResetInput {
    ExitRealSpace,
    StayRealSpace,
    UserRotationDone,
}

public class TwoOneTurnResetter : BaseTask<TTResetState, TTResetInput>
{
    protected float targetAngle;
    protected float ratio;

    private IEnumerator coroutine1, coroutine2;
    private GainRedirector redirector;

    protected override void GenerateTask()
    {
        redirector = this.GetComponent<GainRedirector>();

        GenerateUI("Rotation UI", uiInfo[0]);
        GenerateUI("Translation UI", uiInfo[1]);
        GenerateUI("Translation Guide UI", uiInfo[2]);

        AddTaskEvent(TTResetInput.ExitRealSpace, Behaviour.Exit, "RealSpace");
        AddTaskEvent(TTResetInput.StayRealSpace, Behaviour.Stay, "RealSpace");
        AddTaskEvent(TTResetInput.UserRotationDone, Behaviour.Rotate, "Default");

        task.AddStateStart(TTResetState.Idle)
        .AddTransition(TTResetState.Idle, TTResetState.Rotating, TTResetInput.ExitRealSpace, () => ToggleRedirector(false))
        .AddStateStart(TTResetState.Rotating, CalculateResetAngle, StartRotation, () => EnableUI("Rotation UI"), () => CallAfterRotation(targetAngle))
        .AddTransition(TTResetState.Rotating, TTResetState.Translating, TTResetInput.UserRotationDone, StopRotation, () => DisableUI("Rotation UI"))
        .AddStateStart(TTResetState.Translating, StartTranslation, () => EnableUI("Translation UI"), () => EnableUI("Translation Guide UI", true))
        .AddTransition(TTResetState.Translating, TTResetState.Idle, TTResetInput.StayRealSpace, StopTranslation, () => DisableUI("Translation UI"), () => DisableUI("Translation Guide UI", true), () => ToggleRedirector(true));

        task.Begin(TTResetState.Idle); 
    }

    public void CalculateResetAngle() {
        targetAngle = 180;
        ratio = 2;

        if(targetAngle > 0) {
            UIManager.Instance.ToggleUIBase("Rotation UI", false, "image_2");
            UIManager.Instance.ToggleUIBase("Rotation UI", true, "image_3");

        }
        else {
            UIManager.Instance.ToggleUIBase("Rotation UI", true, "image_2");
            UIManager.Instance.ToggleUIBase("Rotation UI", false, "image_3");

        }
    }

    public void ToggleRedirector(bool enabled) {
        if(redirector != null) redirector.enabled = enabled;
    }

    public void CallAfterRotation(float degree) {
        User user = users.GetActiveUser();
        StartCoroutine(user.CallAfterRotation(degree));
    }

    public void StartRotation() {
        StartCoroutine(PlayResetSound());
        StartCoroutine(coroutine1 = _ApplyRotation());
    }

    public IEnumerator PlayResetSound() {
        AudioSource resetSound = Instantiate<AudioSource>(SoundSetting.Instance.resetSoundPrefab, virtualEnvironment.transform);
        resetSound.Play();
        yield return new WaitForSeconds(2.5f);
        resetSound.Stop();
    }

    public void StopRotation() {
        StopCoroutine(coroutine1);
    }

    public void StartTranslation() {
        StartCoroutine(coroutine2 = _ApplyTranslation());
    }

    public void StopTranslation() {
        StopCoroutine(coroutine2);
    }

    IEnumerator _ApplyRotation() {
        User user = users.GetActiveUser();

        while(true) {
            virtualEnvironment.RotateAround(user.Body.Position, user.Body.deltaRotation * Time.fixedDeltaTime);    
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator _ApplyTranslation() {
        User user = users.GetActiveUser();
        UICanvas flagUI = UIManager.Instance.GetUI("Translation Guide UI");

        while(true) {
            virtualEnvironment.Translate(user.Body.deltaPosition * Time.fixedDeltaTime, Space.World);
            flagUI.Translate(user.Body.deltaPosition * Time.fixedDeltaTime, Space.World);
            yield return new WaitForFixedUpdate();
        }
    }

}
