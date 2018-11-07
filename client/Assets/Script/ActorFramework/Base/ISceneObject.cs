/// <summary>
/// 场景物品
/// </summary>
public interface ISceneObject
{
    void OnCreate();
    void OnUpdate();
    void EnterScene();

    void LeaveScene();

    void OnDelete();
}