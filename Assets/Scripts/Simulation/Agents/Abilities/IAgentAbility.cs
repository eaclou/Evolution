public interface IAgentAbility
{
    void Begin();
    
    void End();
    
    bool inProcess { get; }
    int frameCount { get; }
}
