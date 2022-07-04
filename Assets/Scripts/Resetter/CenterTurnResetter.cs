using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CTResetState {
    Idle,
    Rotating,
    Translating,
}

public enum CTResetInput {
    ExitRealSpace,
    StayRealSpace,
    UserRotationDone,
}

public class CenterTurnResetter : BaseTask<CTResetState, CTResetInput>
{
    protected float targetAngle;
    protected float ratio;

    private IEnumerator coroutine1, coroutine2;
    private float rotateDir;

    private GainRedirector redirector;
    private CoinCollectTask exp2;

    protected override void GenerateTask()
    {
        redirector = this.GetComponent<GainRedirector>();
        exp2 = this.GetComponent<CoinCollectTask>();

        GenerateUI("Rotation UI", uiInfo[0]);
        GenerateUI("Translation UI", uiInfo[1]);
        GenerateUI("Translation Guide UI", uiInfo[2]);

        AddTaskEvent(CTResetInput.ExitRealSpace, Behaviour.Exit, "RealSpace");
        AddTaskEvent(CTResetInput.StayRealSpace, Behaviour.CompletelyStay, "RealSpace");
        AddTaskEvent(CTResetInput.UserRotationDone, Behaviour.Rotate, "Default");

        task.AddStateStart(CTResetState.Idle)
        .AddTransition(CTResetState.Idle, CTResetState.Rotating, CTResetInput.ExitRealSpace, () => ToggleRedirector(false))
        .AddStateStart(CTResetState.Rotating, CalculateResetAngle, StartRotation, () => EnableUI("Rotation UI"), () => CallAfterRotation(targetAngle))
        .AddTransition(CTResetState.Rotating, CTResetState.Translating, CTResetInput.UserRotationDone, StopRotation, () => DisableUI("Rotation UI"))
        .AddStateStart(CTResetState.Translating, StartTranslation, () => EnableUI("Translation UI"), () => EnableUI("Translation Guide UI", true))
        .AddTransition(CTResetState.Translating, CTResetState.Idle, CTResetInput.StayRealSpace, StopTranslation, DisableUIs);

        task.Begin(CTResetState.Idle); 
    }

    public void DisableUIs() {
        bool disableHandPointer = true;
        if(exp2 != null) {
            if(exp2.Task.GetCurrentState() == Exp2State.Initial || exp2.Task.GetCurrentState() == Exp2State.GoToNextRoom || exp2.Task.GetCurrentState() == Exp2State.EnterNextRoom)
                disableHandPointer = false;
        }

        DisableUI("Translation UI", false, disableHandPointer);
        DisableUI("Translation Guide UI", true, disableHandPointer);
        ToggleRedirector(true);
    }

    public void CalculateResetAngle() {
        User user = users.GetActiveUser();

        Vector2 userToCenter = realSpace.Position - realSpace.realUser.Position;

        targetAngle = Vector2.SignedAngle(realSpace.realUser.Forward, userToCenter);
        ratio = 360 / Mathf.Abs(targetAngle);

        if(targetAngle > 0) {
            rotateDir = 1;
            UIManager.Instance.ToggleUIBase("Rotation UI", false, "image_2");
            UIManager.Instance.ToggleUIBase("Rotation UI", true, "image_3");

        }
        else {
            rotateDir = -1;
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
            virtualEnvironment.RotateAround(user.Body.Position, (ratio - 1) * user.Body.deltaRotation * Time.fixedDeltaTime);    
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
