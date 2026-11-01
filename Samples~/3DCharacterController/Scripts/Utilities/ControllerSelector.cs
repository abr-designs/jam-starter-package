using Samples.Cameras;
using Samples.CharacterController3D.Scripts;
using UnityEngine;

/// <summary>
/// Keeps only the selected <see cref="IThirdPersonCharacter"/> & <see cref="ICameraRig"/> active & links them together.
/// Select indices refer to the order of the assigned GameObject arrays.
/// </summary>
/// <remarks>Rewritten by Claude (claude-opus-5-5) — 2026-11-01</remarks>
public class ControllerSelector : MonoBehaviour
{
    [SerializeField, Tooltip("GameObjects holding an IThirdPersonCharacter component.")]
    private GameObject[] characterGameObjects;
    [SerializeField, Tooltip("GameObjects holding an ICameraRig component.")]
    private GameObject[] cameraGameObjects;

    [SerializeField, Min(0)]
    private int startCharacterIndex;
    [SerializeField, Min(0)]
    private int startCameraIndex;

    private IThirdPersonCharacter[] m_characters;
    private ICameraRig[] m_cameras;

    private IThirdPersonCharacter m_selectedCharacter;
    private ICameraRig m_selectedCamera;

    //Unity Function
    //================================================================================================================//

    private void Start()
    {
        m_characters = new IThirdPersonCharacter[characterGameObjects.Length];
        for (int i = 0; i < characterGameObjects.Length; i++)
        {
            m_characters[i] = characterGameObjects[i].GetComponent<IThirdPersonCharacter>();
        }

        m_cameras = new ICameraRig[cameraGameObjects.Length];
        for (int i = 0; i < cameraGameObjects.Length; i++)
        {
            m_cameras[i] = cameraGameObjects[i].GetComponent<ICameraRig>();
        }

        m_selectedCharacter = m_characters[startCharacterIndex];
        m_selectedCamera = m_cameras[startCameraIndex];
        UpdateSelected();
    }

    //================================================================================================================//

    /// <summary>
    /// Activates the character at <paramref name="index"/> in <see cref="characterGameObjects"/> & deactivates the rest.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-5-5) — 2026-11-01</remarks>
    public void SelectCharacter(int index)
    {
        m_selectedCharacter = m_characters[index];
        UpdateSelected();
    }

    /// <summary>
    /// Activates the camera at <paramref name="index"/> in <see cref="cameraGameObjects"/> & deactivates the rest.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-5-5) — 2026-11-01</remarks>
    public void SelectCamera(int index)
    {
        m_selectedCamera = m_cameras[index];
        UpdateSelected();
    }

    //================================================================================================================//

    private void UpdateSelected()
    {
        foreach (var cameraRig in m_cameras)
        {
            cameraRig.transform.gameObject.SetActive(cameraRig == m_selectedCamera);
            cameraRig.SetTarget(m_selectedCharacter.transform);
        }

        foreach (var character in m_characters)
        {
            character.transform.gameObject.SetActive(character == m_selectedCharacter);
            character.SetCameraTransform(m_selectedCamera.transform);
        }
    }

    //Unity Editor Functions
    //================================================================================================================//

#if UNITY_EDITOR
    private void OnValidate()
    {
        WarnMissingInterface<IThirdPersonCharacter>(characterGameObjects);
        WarnMissingInterface<ICameraRig>(cameraGameObjects);

        void WarnMissingInterface<T>(GameObject[] gameObjects)
        {
            if (gameObjects == null)
                return;

            foreach (var element in gameObjects)
            {
                if (element != null && element.GetComponent<T>() == null)
                    Debug.LogWarning($"{element.name} has no {typeof(T).Name} component", this);
            }
        }
    }
#endif

}
