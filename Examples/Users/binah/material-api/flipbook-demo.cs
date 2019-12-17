using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;
using System;
using System.Linq;

/*
 
 Description:
    A demonstration script for Materials API that responds to events to advance the frame of a flipbook, change the tint of an object and change it's emissive properties.
 Usage:
    Add script to mesh
    Configure Log Tag parameter if using multiple objects, to disambiguate script output


*/

public class MaterialDemo : SceneObjectScript
{

    #region EditorProperties
    [DisplayName("Log Tag")]
    [Tooltip("Tag to use when writing messages to debug console")]
    public readonly string LogTag = "";

    [DisplayName("Flipbook Command")]
    [Tooltip("Command to play frames of a flipbook")]
    [DefaultValue("play_flipbook")]
    public string FlipbookCommand;

    [Tooltip("Total number of frames")]
    [DefaultValue(32.0f)]
    [DisplayName("Frames")]
    public readonly float Frame;

    #endregion


    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    private MeshComponent Mesh = null;

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out Mesh))
        {
            Log.Write(LogLevel.Error, LogTag, "No MeshComponent found!  Aborting.");
            return;
        }
        else
        {
            List<RenderMaterial> materials = Mesh.GetRenderMaterials().ToList();
            if (materials.Count == 0)
            {
                Log.Write(LogLevel.Error, LogTag, "GetRenderMaterials() == null! Aborting.");
                return;
            }
            else
            {
                foreach (RenderMaterial material in materials)
                {
                    if (material == null)
                    {
                        Log.Write(LogTag, "Material is null");
                    }
                    else
                    {
                        Log.Write(LogTag, material.Name);
                        Log.Write(LogTag, material.ToString());
                    }
                }
            }

            Log.Write(LogTag, $"Mesh.IsScriptable={Mesh.IsScriptable}");

            if (!Mesh.IsScriptable)
            {
                Log.Write(LogLevel.Warning, LogTag, $"MeshComponent {Mesh.Name} is not scriptable");
            }


            SubscribeToScriptEvent(FlipbookCommand, (ScriptEventData data) =>
            {
                ISimpleData idata = data.Data.AsInterface<ISimpleData>();

                RenderMaterial m = Mesh.GetRenderMaterial(materials[0].Name);
                MaterialProperties p = m.GetProperties();

                p.FlipbookFrame = Frame;
                m.SetProperties(p); 
                
            });

        }
    }

}
