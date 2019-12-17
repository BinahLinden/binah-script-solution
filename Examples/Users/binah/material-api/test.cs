using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;
using System;
using System.Linq;

/* Test script to test the Materials API, which takes chat input and applies it to a MeshComponent

Usage:
* Add script to scene
* Configure MeshComponent parameter (if left blank, will use the first MeshComponent found on the scripted object)
* Configure Log Tag parameter if using multiple objects, to disambiguate script output
* Issue chat commands (described low) to get/set materials parameters
* Optionally, add chat command strings to the "Run Commands" array parameter - these commands will be run in sequence on init

/getRenderMaterials
 - get a list of rendermaterials on the MeshComponent, using Mesh.GetRenderMaterials()

/getRenderMaterial [materialName]
 - Get static attributes of the material (e.g. shader type, other flags for material), using  Mesh.GetRenderMaterial()

/getMaterialProperties [materialName]
 - Get dynamic properties of material (e.g. brightness, tint), using RenderMaterial.GetProperties()
 
/setMaterialProperty [materialName] [propertyName] [propertyValue]
 - Configure a property value on the material, using RenderMaterial.SetProperties()
 - Supported properties are:
	absorption (float value)
	brightness (float value)
	emissiveintensity (float value)
	flipbookframe (float value)
	tint (color value, e.g. "(0,1.0,0)" for green)
	
/setMaterialProperty [materialName] [propertyName] [propertyValue] [transitionTime] [interpolationMode]
 - Configure a property on the material over time, with a specified transition time and interpolation mode, using RenderMaterial.SetProperties()
 - transitionTime should be some positive float value
 - Supported interpolation modes are:
	easein		Ease In. The value will gradually speed up and continue quickly at the end.
	easeout		Ease Out. The value will change quickly at first and slow down to smoothly reach the goal.
	linear		Linear Interpolation. The value will change at a constant rate.
	smoothstep	Smoothstep Interpolation. The value will speed up gradually and slow down to smoothly reach the goal.
	stepStep	Step. The value will step abruptly from the initial value to the final value half-way through.

/setOpComplete [true|false]
 - Set whether to use a callback when calling 

*/

public class TestMaterials : SceneObjectScript
{
    #region EditorProperties
    [Tooltip("Mesh Component to test.  Leave blank to use the first mesh component found on the scripted object.")]
    public MeshComponent Mesh = null;

    [DisplayName("Run Commands")]
    [Tooltip("Each of these commands will run in this order as soon as the script starts.")]
    public readonly List<string> OnStartCommands = null;

    [DisplayName("Log Tag")]
    [Tooltip("Tag to use when writing messages to debug console")]
    public readonly string LogTag = "";

    [DefaultValue(true)]
    [Tooltip("Whether to use OpComplete on RenderMaterial.SetProperties() calls")]
    public bool UseOpComplete = true;
    #endregion

    // This is where new chat commands should be added. 
    // This should be the only portion of the script that needs to be modified.
    private void AddChatCommands()
    {
        ChatCommands["/getRenderMaterials"] = (string[] args) =>
        {
            /* /getRenderMaterials
				
				Prints properties of all materials found on the MeshComponent
			*/

            List<RenderMaterial> materials = Mesh.GetRenderMaterials().ToList();
            if (materials.Count == 0)
            {
                Log.Write(LogTag, "No materials found.");
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
                        Log.Write(LogTag, $"Material {material.Index}:{material.Name} {material}");
                        /*if(material.HasTint)
                        {
                            Log.Write("tintable edit");
                            MaterialProperties newTint = new MaterialProperties();
                            Color endColor = new Color(1,1,0);
                            newTint.Tint = endColor;
                            material.SetProperties(newTint, 1.0f, InterpolationMode.Linear);
                        }*/
                    }
                }
            }
        };

        ChatCommands["/getRenderMaterial"] = (string[] args) =>
        {
            /* /getRenderMaterial materialName
				e.g. /getRenderMaterial woodFloor
				
				Prints material flags, if the material is found.
			*/
            if (args.Length == 2)
            {
                RenderMaterial m = Mesh.GetRenderMaterial(args[1]);
                if (m == null)
                {
                    Log.Write(LogTag, $"Material {args[1]} was not found.");
                }
                else
                {
                    Log.Write(LogTag, $"Material {args[1]} has flags: HasAbsorption={m.HasAbsorption}, HasBrightness={m.HasBrightness}, HasEmissiveIntensity={m.HasEmissiveIntensity}, HasFlipbookFrame={m.HasFlipbookFrame}, HasTint={m.HasTint}, Index={m.Index}, IsValid={m.IsValid}, Name={m.Name}, ShaderType={m.ShaderType}");
                }
            }
            else Log.Write(LogLevel.Warning, LogTag, $"No variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };

        ChatCommands["/getMaterialProperties"] = (string[] args) =>
        {
            /* /getMaterialProperties
				e.g. /getMaterialProperties woodFloor
				
				Prints material properties, if the material is found.
			*/
            if (args.Length == 2)
            {
                RenderMaterial m = Mesh.GetRenderMaterial(args[1]);
                if (m == null)
                {
                    Log.Write(LogTag, $"Material {args[1]} was not found.");
                }
                else
                {
                    MaterialProperties p = m.GetProperties();

                    Log.Write(LogTag, $"Material {args[1]} has properties: {DisplayProperties(p)}");
                }
            }
            else Log.Write(LogLevel.Warning, LogTag, $"No variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };

        ChatCommands["/setOpComplete"] = (string[] args) =>
        {
            /* Toggle callbacks on set operations.
				'/setOpComplete true' to use OperationCompleteEvent for SetProperties() commands
				'/setOpComplete false' to call SetProperties() without OperationCompleteEvent
			*/
            bool val;
            if (!bool.TryParse(args[1], out val))
            {
                Log.Write(LogLevel.Error, LogTag, $"Failed to parse args[1] as bool");
                return;
            }
            UseOpComplete = val;
        };

        ChatCommands["/setMaterialProperty"] = (string[] args) =>
        {
            /* Set a value on named material property
					/setMaterialProperty materialName propertyName propertyValue
					/setMaterialProperty materialName propertyName propertyValue transitionTime interpolationMode
				Valid propertyName values are:
					absorption (float value)
					brightness (float value)
					emissiveintensity (float value)
					flipbookframe (float value)
					tint  (color value)
				e.g. /setMaterialProperty woodFloor absorption 0.5
					/setMaterialProperty woodFloor absorption 0.5 3.5 smoothstep
				
				Sets absorption (float) on named material, with transition time 
			*/
            if (args.Length == 4 || args.Length == 6)
            {
                RenderMaterial m = Mesh.GetRenderMaterial(args[1]);
                if (m == null)
                {
                    Log.Write(LogTag, $"Material {args[1]} was not found.");
                    return;
                }

                MaterialProperties p = m.GetProperties();

                switch (args[2].ToLower())
                {
                    case "absorption":
                        if (!float.TryParse(args[3], out p.Absorption))
                        {
                            Log.Write(LogLevel.Error, LogTag, $"{args[2]}: Failed to parse {args[3]} as float");
                            return;
                        }
                        break;
                    case "brightness":
                        if (!float.TryParse(args[3], out p.Brightness))
                        {
                            Log.Write(LogLevel.Error, LogTag, $"{args[2]}: Failed to parse {args[3]} as float");
                            return;
                        }
                        break;
                    case "emissiveintensity":
                        if (!float.TryParse(args[3], out p.EmissiveIntensity))
                        {
                            Log.Write(LogLevel.Error, LogTag, $"{args[2]}: Failed to parse {args[3]} as float");
                            return;
                        }
                        break;
                    case "flipbookframe":
                        if (!float.TryParse(args[3], out p.FlipbookFrame))
                        {
                            Log.Write(LogLevel.Error, LogTag, $"{args[2]}: Failed to parse {args[3]} as float");
                            return;
                        }
                        break;
                    case "tint":
                        if (!Sansar.Color.TryParse(args[3], out p.Tint))
                        {
                            Log.Write(LogLevel.Error, LogTag, $"{args[2]}: Failed to parse {args[3]} as Sansar.Color");
                            return;
                        }
                        break;
                    default:
                        Log.Write(LogLevel.Error, LogTag, $"Unknown property {args[2]} (see Sansar.Simulation.MaterialProperties for valid options");
                        return;
                }

                Log.Write(LogTag, $"Material {m.Name}: setting {args[2]}={args[3]}.  New properties: {DisplayProperties(p)}");

                if (args.Length == 4)
                {
                    if (UseOpComplete)
                    {
                        m.SetProperties(p, (o) => Log.Write(LogTag, $"Received OpComplete on '{String.Join(" ", args)}' with success={o.Success} message={o.Message}"));
                    }
                    else
                    {
                        m.SetProperties(p);
                    }
                }
                else
                {
                    // Set property with interpolation
                    float duration = 0f;
                    if (!float.TryParse(args[4], out duration))
                    {
                        Log.Write(LogLevel.Error, LogTag, $"Interpolation time: failed to parse {args[4]} as float");
                        return;
                    }

                    InterpolationMode iMode = InterpolationModeParse(args[5]);

                    Log.Write(LogTag, $"Executing '{String.Join(" ", args)}' ...");
                    if (UseOpComplete)
                    {
                        m.SetProperties(p, duration, iMode, (o) => Log.Write(LogTag, $"Received OpComplete on '{String.Join(" ", args)}' with success={o.Success}"));
                    }
                    else
                    {
                        m.SetProperties(p, duration, iMode);
                    }
                }
            }
            else Log.Write(LogLevel.Warning, LogTag, $"No variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };
    }

    /////////////////////////////////////////////////////////////////////////
    // Framework: Nothing below here should need to be regularly modified. //
    /////////////////////////////////////////////////////////////////////////
    private Dictionary<string, Action<string[]>> ChatCommands = new Dictionary<string, Action<string[]>>();
    // Init() is where the script is setup and is run when the script starts.
    public override void Init()
    {
        if (Mesh == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out Mesh))
            {
                Log.Write(LogLevel.Error, LogTag, "No MeshComponent found!  Aborting.");
                return;
            }
        }
        Log.Write(LogTag, $"Mesh.IsScriptable={Mesh.IsScriptable}");

        if (!Mesh.IsScriptable)
        {
            Log.Write(LogLevel.Warning, LogTag, $"MeshComponent {Mesh.Name} is not scriptable");
        }

        if (Mesh.GetRenderMaterials() == null)
        {
            Log.Write(LogLevel.Error, LogTag, "GetRenderMaterials() == null! Aborting.");
            return;
        }

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (data) => OnChat(data.Message), true);

        AddChatCommands();

        foreach (string command in OnStartCommands)
        {
            OnChat(command);
        }
    }

    public void OnChat(string message)
    {
        string[] args = message.Split(' ');
        try
        {
            Log.Write(LogTag, "Starting command [args: " + args.Length + "]: " + string.Join(" ", args));
            ChatCommands[args[0]].Invoke(args);
            Log.Write(LogTag, "Command executed [args: " + args.Length + "]: " + string.Join(" ", args));
        }
        catch (Exception e)
        {
            Log.Write(LogLevel.Error, LogTag, "Caught exception " + e.GetType().Name + " in " + args[0] + " handler.");
        }
    }

    /*MaterialProperties MaterialPropertyParse(string s)
	{
		s = s.ToLower();
		if(s == "absorption") return MaterialProperties.Absorption;
		if(s == "brightness") return MaterialProperties.Brightness;
		if(s == "emissiveintensity") return MaterialProperties.EmissiveIntensity;
		if(s == "flipbookframe") return MaterialProperties.FlipbookFrame;
		if(s == "tint") return MaterialProperties.Tint;

		
		Log.Write(LogLevel.Error, $"Unknown render material '{s}'!");
		return null;
	}*/

    InterpolationMode InterpolationModeParse(string s)
    {
        s = s.ToLower();
        if (s == "easein") return InterpolationMode.EaseIn;
        if (s == "easeout") return InterpolationMode.EaseOut;
        if (s == "linear") return InterpolationMode.Linear;
        if (s == "smoothstep") return InterpolationMode.Smoothstep;
        if (s == "step") return InterpolationMode.Step;
        Log.Write(LogLevel.Warning, $"Unknown InterpolationMode '{s}'!  Using Linear...");
        return InterpolationMode.Linear;
    }

    string DisplayProperties(MaterialProperties p)
    {
        // originally, I used p.Tint.ToRGBA() but that formats the output in 0-255 scale, which is confusing since Sansar.Color parsing uses a 0-1 scale.
        return $"Absorption={p.Absorption}, Brightness={p.Brightness}, EmissiveIntensity={p.EmissiveIntensity}, FlipbookFrame={p.FlipbookFrame}, Tint={p.Tint}";
    }
}