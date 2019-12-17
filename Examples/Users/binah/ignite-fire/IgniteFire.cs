using Sansar.Simulation;
using Sansar.Script;
using System;

public class IgniteFire : SceneObjectScript
{
    

    [DefaultValue(50.0f)]
    [Range(0.0f, 100.0f)]
    public float Loudness;

    private MeshComponent component;
  
    private AudioComponent _audio = null;
    private PlaySettings playSettings;

    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }
    public override void Init()
    {

      //Check for audio component
      if (!ObjectPrivate.TryGetFirstComponent(out _audio))
      {
          Log.Write("IgniteFire script is on an object that does not have an audio emitter.");
          return;
      }

      Log.Write("Found AudioComponent");

      //Check for Mesh component
      if (!ObjectPrivate.TryGetFirstComponent<MeshComponent>(out component))
      {
          Log.Write("No mesh component found!");
          return;
      }

      Log.Write("Found mesh component");

      if (component.IsScriptable)
      {
          // Listen for the 'ignite_fire' message
          SubscribeToScriptEvent("ignite_fire", (ScriptEventData data) =>
          {
              ISimpleData idata = data.Data.AsInterface<ISimpleData>();
              if (idata == null)
              {
                  ScenePrivate.Chat.MessageAllUsers("The 'ignite_fire' message does not have a simple script payload!");
              }
              else
              {
                //show mesh and lighting - the flame
                component.SetIsVisible(true);
                //play fire sound
                playSettings.Loudness = (60.0f * (Loudness / 100.0f)) - 48.0f;  // Convert percentage to decibels (dB)
              }
          });


      }
    }
}
