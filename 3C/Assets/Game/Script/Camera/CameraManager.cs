using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    public CameraState CameraState;

    [SerializeField]
    public CinemachineVirtualCamera _fpsCamera;
    
    [SerializeField]
    private CinemachineFreeLook _tpsCamera;

    [SerializeField]
    private InputManager _inputManager;

    public Action OnChangePerspective;

      
    // Start is called before the first frame update
    void Start()
    {
        _inputManager.OnChangePOVInput += SwitchCamera;
    }

    private void OnDestroy()
    {
        _inputManager.OnChangePOVInput -= SwitchCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        CinemachinePOV pov = _fpsCamera.GetCinemachineComponent<CinemachinePOV>();
        if (isClamped)
        {
            pov.m_HorizontalAxis.m_Wrap = false;
            pov.m_HorizontalAxis.m_MinValue = playerRotation.y - 45;
            pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + 45;
        }
        else
        {
            pov.m_HorizontalAxis.m_MinValue = -180;
            pov.m_HorizontalAxis.m_MaxValue = 180;
            pov.m_HorizontalAxis.m_Wrap = true;
        }
    }

    private void SwitchCamera()
    {
        OnChangePerspective();
        if (CameraState == CameraState.ThirdPerson)
        {
            CameraState = CameraState.FirstPerson;
            _tpsCamera.gameObject.SetActive(false);
            _fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            CameraState = CameraState.ThirdPerson;
            _fpsCamera.gameObject.SetActive(false);
            _tpsCamera.gameObject.SetActive(true);
        }
    }

    public void SetTPSFieldOfView(float FieldOfView)
    {
        _tpsCamera.m_Lens.FieldOfView = FieldOfView;
    } 
};
