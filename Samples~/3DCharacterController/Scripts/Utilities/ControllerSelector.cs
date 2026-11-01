using Samples.Cameras;
using Samples.CharacterController3D.Scripts;
using UnityEngine;

public class ControllerSelector : MonoBehaviour
{
    private FreeLookCamera m_freeLookCamera;
    private ThirdPersonCamera m_thirdPersonCamera;
    private ThirdPersonCharacterController m_thirdPersonCharacterController;
    private CharacterController3D m_characterController3D;

    private MonoBehaviour m_selectedCharacter;
    private MonoBehaviour m_selectedCamera;
    
    //Unity Function
    //================================================================================================================//

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_freeLookCamera = Object.FindAnyObjectByType<FreeLookCamera>(FindObjectsInactive.Include);
        m_thirdPersonCamera = Object.FindAnyObjectByType<ThirdPersonCamera>(FindObjectsInactive.Include);
        m_thirdPersonCharacterController = Object.FindAnyObjectByType<ThirdPersonCharacterController>(FindObjectsInactive.Include);
        m_characterController3D = Object.FindAnyObjectByType<CharacterController3D>(FindObjectsInactive.Include);
        
        SelectSpringController(false);
        SelectOrbitCamera(false);
        UpdateSelected();
    }
    
    //================================================================================================================//

    public void SelectSpringController(bool update = true)
    {
        m_selectedCharacter = m_characterController3D;
        
        if(update)
            UpdateSelected();
    }

    public void SelectClassicController(bool update = true)
    {
        m_selectedCharacter = m_thirdPersonCharacterController;
                
        if(update)
            UpdateSelected();
    }
    //================================================================================================================//

    public void SelectOrbitCamera(bool update = true)
    {
        m_selectedCamera = m_freeLookCamera;
                
        if(update)
            UpdateSelected();
    }

    public void SelectShoulderCamera(bool update = true)
    {
        m_selectedCamera = m_thirdPersonCamera;
                
        if(update)
            UpdateSelected();
    }

    //================================================================================================================//

    private void UpdateSelected()
    {
        m_freeLookCamera.gameObject.SetActive(m_selectedCamera == m_freeLookCamera);
        m_thirdPersonCamera.gameObject.SetActive(m_selectedCamera == m_thirdPersonCamera);
        
        m_thirdPersonCharacterController.gameObject.SetActive(m_selectedCharacter == m_thirdPersonCharacterController);
        m_characterController3D.gameObject.SetActive(m_selectedCharacter == m_characterController3D);

        
        m_thirdPersonCharacterController.SetCameraTransform(m_selectedCamera.transform);
        m_characterController3D.SetCameraTransform(m_selectedCamera.transform);
        
        m_freeLookCamera.SetTarget(m_selectedCharacter.transform);
        m_thirdPersonCamera.SetTarget(m_selectedCharacter.transform);
        
    }

}
