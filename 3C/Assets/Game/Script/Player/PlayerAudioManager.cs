using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private AudioSource _footstepSfx;
    [SerializeField] private AudioSource _glideSfx;
    [SerializeField] private AudioSource _punchSfx;

    private void PlayFootstepSfx()
    {
        _footstepSfx.pitch = Random.Range(0.8f, 1.2f);
        _footstepSfx.volume = Random.Range(0.8f, 1f);
        _footstepSfx.Play();
    }

    public void PlayGlideSfx()
    {
        _glideSfx.Play();
    }

    public void StopGlideSfx()
    {
        _glideSfx.Stop();
    }

    private void PlayPunchSfx()
    {
        _punchSfx.pitch = Random.Range(0.8f, 1.2f);
        _punchSfx.volume = Random.Range(0.8f, 1f);
        _punchSfx.Play();
    }
}
