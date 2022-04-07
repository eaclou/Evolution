using UnityEngine;

public class CritterModuleFriends : IBrainModule
{
    public float[] friendPosX;
    public float[] friendPosY;
    public float[] friendVelX;
    public float[] friendVelY;
    public float[] friendDirX;
    public float[] friendDirY;

    public CritterModuleFriends() {
        friendPosX = new float[1];
        friendPosY = new float[1];
        friendVelX = new float[1];
        friendVelY = new float[1];
        friendDirX = new float[1];
        friendDirY = new float[1];
    }

    public void SetNeuralValue(Neuron neuron) {
        neuron.currentValues = (float[])GetType().GetField(neuron.name).GetValue(this);
    }

    public void Tick(Agent agent) {
        Vector2 friendPos = Vector2.zero;
        Vector2 friendDir = Vector2.zero;
        Vector2 friendVel = Vector2.zero;
        
        if (agent.coreModule.nearestFriendAgent) {
            var nearestFriend = agent.coreModule.nearestFriendAgent.bodyRigidbody;
            friendPos = new Vector2(nearestFriend.transform.localPosition.x - agent.ownPos.x, nearestFriend.transform.localPosition.y - agent.ownPos.y);
            friendDir = friendPos.normalized;
            friendVel = new Vector2(nearestFriend.velocity.x, nearestFriend.velocity.y);
        }

        friendPosX[0] = friendPos.x / 20f;
        friendPosY[0] = friendPos.y / 20f;
        friendVelX[0] = (friendVel.x - agent.ownVel.x) / 15f;
        friendVelY[0] = (friendVel.y - agent.ownVel.y) / 15f;
        friendDirX[0] = friendDir.x;
        friendDirY[0] = friendDir.y;
    }
}
