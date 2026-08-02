using UnityEngine;

public class SceneRuntimeBinder : MonoBehaviour
{
    [SerializeField] private AIAnimationExecutor aiAnimationExecutor;
    [SerializeField] private CharacterAnimationService characterAnimationService;
    [SerializeField] private CharacterRigService characterRigService;
    [SerializeField] private CameraMoveService cameraMoveService;
    [SerializeField] private ClothingService clothingService;

    public AIAnimationExecutor AIAnimationExecutor => aiAnimationExecutor;
    public CharacterAnimationService CharacterAnimationService => characterAnimationService;
    public CharacterRigService CharacterRigService => characterRigService;
    public CameraMoveService CameraMoveService => cameraMoveService;
    public ClothingService ClothingService => clothingService;
}
