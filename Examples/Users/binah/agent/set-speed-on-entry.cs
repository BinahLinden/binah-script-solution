using System;
using System.Linq;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;


public class AgentSetSpeedOnEntry : SceneObjectScript
{
    [Tooltip("The speed factor for agent upon entry.")]
    [DefaultValue(2.0f)]
    [Range(0.1f, 4.0f)]
    [DisplayName("Agent Speed")]
    public readonly float AgentSpeed;

    [DefaultValue(true)]
    public bool DebugLogging;

    public override void Init()
    {
        ScenePrivate.User.Subscribe(User.AddUser, SessionId.Invalid, (UserData data) => {
            AgentPrivate agent = ScenePrivate.FindAgent(data.User);
            agent.SetSpeedFactor(AgentSpeed);
        }, true);

        if (DebugLogging) Log.Write("");
    }
}