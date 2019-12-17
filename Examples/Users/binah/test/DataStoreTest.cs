using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System.Collections.Generic;
using System;
using System.Linq;

// A small starter script.
public class DataStoreTest : SceneObjectScript
{
    #region EditorProperties
    [DisplayName("Run Commands")]
    [Tooltip("Each of these commands will run in this order as soon as the script starts.")]
	
	[AddEntry("/createDataStore myTestDS")]
	[AddEntry("/storeKey myStringKey string \"greatValue\"")]
	[AddEntry("/restoreKey myStringKey")]
	[AddEntry("/restoreKey myStringKey string")]
	[AddEntry("/storeKey myShoppingList string greatValue")]
	[AddEntry("/restoreKey myStringKey")]
	[AddEntry("/restoreKey myStringKey string")]
	[AddEntry("/storeKey myIntKey int 31337")]
	[AddEntry("/restoreKey myIntKey")]
	[AddEntry("/restoreKey myIntKey int")]
	[AddEntry("/storeKey myIntArrayKey int[] [1,-2,9]")]
	[AddEntry("/restoreKey myIntArrayKey")]
	[AddEntry("/restoreKey myIntArrayKey int[]")]
	
    public List<string> OnStartCommands = null;
	
    #endregion
	
	// DataStore to use; created via /createDataStore command
	DataStore DS = null;
	
    // This is where new chat commands should be added. 
    // This should be the only portion of the script that needs to be modified.
    private void AddChatCommands()
    {
        ChatCommands["/createDataStore"] = (string[] args) =>
        {
			// e.g. "/createDataStore myDS" or "/createDataStore myDS"
			if(args.Length == 2)
			{
				string dataStoreName = args[1];
				Guid dataStoreGuid;
				if(Guid.TryParse(dataStoreName, out dataStoreGuid))
				{
					Log.Write($"Creating DataStore with Guid {dataStoreGuid}");
					DS = ScenePrivate.CreateDataStore(dataStoreGuid);
				}
				else
				{
					Log.Write($"Creating DataStore with name {dataStoreName}");
					DS = ScenePrivate.CreateDataStore(dataStoreName);
				}
				Log.Write(DS.Id.ToString(), $"Active DataStore is now {DS.Id}");
			}
			else Log.Write(LogLevel.Warning, $"No variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };
		
		ChatCommands["/storeKey"] = (string[] args) =>
        {
			
            // args[0] is always the command
            /* This one takes a type argument.  Supported types are:
			
				/storeKey myKey string myValue
				/storeKey myKey string[] myValue
				/storeKey myKey int myValue
				/storeKey myKey int[] myValue
				/storeKey myKey float myValue
				/storeKey myKey float[] myValue
				/storeKey myKey bool myValue
				/storeKey myKey bool[] myValue
				/storeKey myKey null myValue				
			*/
			// /storeKey myKey string myValue
            if (args.Length == 4)
			{
				switch(args[2])
                {
                    case "int":
                    DeserializeAndStore<int>(args[1], args[3]);
                    break;
                    case "int[]":
                    DeserializeAndStore<int[]>(args[1], args[3]);
                    break;
                    case "string":
                    DeserializeAndStore<string>(args[1], args[3]);
					break;
					 case "string[]":
                    DeserializeAndStore<string[]>(args[1], args[3]);
                    break;
					case "float":
                    DeserializeAndStore<string>(args[1], args[3]);
					break;
					 case "float[]":
                    DeserializeAndStore<string[]>(args[1], args[3]);
                    break;
					case "bool":
                    DeserializeAndStore<string>(args[1], args[3]);
					break;
					 case "bool[]":
                    DeserializeAndStore<string[]>(args[1], args[3]);
                    break;
					//add other types here as needed
					default:
					Log.Write(LogLevel.Error, $"Unsupported datatype {args[1]}!  Aborting Store operation.");
					break;                    
                }
				//Log.Write($"store type is args[2]");
			}
			else Log.Write(LogLevel.Warning, $"StNo variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };

		ChatCommands["/restoreKey"] = (string[] args) =>
        {
			
            // args[0] is always the command
            /* This variant only has a key argument - only the jsonString is returned:
			
				/restoreKey myKey
				
			*/
			// /storeKey myKey string myValue
            if (args.Length == 2)
			{
				DS.Restore<string>(args[1], (o) =>
				{
					if(!o.Success)
					{
						Log.Write($"Failed to restore key {args[1]}: {o.Message}");
					}
					else
					{
						Log.Write($"Restored {args[1]} has JsonString value '{o.JsonString}'");
					}
				});
			}
			/* Optionally, we can look at the object returned, but this requires the user to specify type:
				/restoreKey myKey string
				/restoreKey myKey string[]
				/restoreKey myKey int
				/restoreKey myKey int[]
				/restoreKey myKey float
				/restoreKey myKey float[]
				/restoreKey myKey bool
				/restoreKey myKey bool[]
			*/
			else if (args.Length == 3)
			{
				switch(args[2])
				{
					case "int":
						RestoreAndSerialize<int>(args[1]);
					break;
					case "int[]":
						RestoreAndSerialize<int[]>(args[1]);
					break;
					case "string":
						RestoreAndSerialize<string>(args[1]);
					break;
					case "string[]":
						RestoreAndSerialize<string[]>(args[1]);
					break;
					case "bool":
						RestoreAndSerialize<bool>(args[1]);
					break;
					case "bool[]":
						RestoreAndSerialize<bool[]>(args[1]);
					break;
					default:
						Log.Write(LogLevel.Warning, $"Unsupported type {args[2]} !");
					break;
					//add other types here as needed
				}
			}
			else Log.Write(LogLevel.Warning, $"No variant of {args[0]} takes {args.Length} arguments!  Aborting.");
        };
    }
	
	void DeserializeAndStore<T>(string key, string keyValue)
	{
		Log.Write(DS.Id.ToString(), "DeserializeAndStore");
		JsonSerializer.Deserialize<T>(keyValue, (o) =>
		{
			//Log.Write(DS.Id.ToString(), "JsonSerializer.Deserialize");
			if(!o.Success)
			{
				Log.Write(DS.Id.ToString(), $"Failed to parse json: {o.Message}");
			}
			else
			{
				Log.Write(DS.Id.ToString(), $"Storing Key {key}='{o.JsonString}'..");
				//WaitFor(DS.Store, key, o.Object);
				DS.Store(key, o.Object, (p) =>
				{
					if(p.Success)
					{
						Log.Write(DS.Id.ToString(), $"Stored Key {key} successfully, with version={p.Version}");
					}
					else					
					{
						Log.Write(DS.Id.ToString(), $"Failed to store Key {key}: {p.Message}");
					}
				});
			}
		});
	}
	
	void RestoreAndSerialize<T>(string key)
	{
		DS.Restore<T>(key, (o) =>
		{
			if(!o.Success)
			{
				Log.Write($"Failed to restore key {key}: {o.Message}");
			}
			else
			{
				//string type = args[2];
				Log.Write($"Restored {key} has JsonString value '{o.JsonString}'");
				
				// Failed attempt to parse various data types to string.
				/*string val = "";
				
				switch(o.Object)
				{
					case System.Collections.IEnumerable e:
						val = e.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y);
						break;
					default:
						val = o.Object.ToString();
						break;
				}
				
				Log.Write($"Restored {key} has value {val}");
				
				*/
			}
		});
	}
	
	
    /////////////////////////////////////////////////////////////////////////
    // Framework: Nothing below here should need to be regularly modified. //
    /////////////////////////////////////////////////////////////////////////
    private Dictionary<string, Action<string[]>> ChatCommands = new Dictionary<string, Action<string[]>>();
    // Init() is where the script is setup and is run when the script starts.
    public override void Init()
    {
		Log.Write($"ObjectPrivate.IsMovable={ObjectPrivate.IsMovable}");
		
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (data) => OnChat(data.Message), true);

        AddChatCommands();

        foreach(string command in OnStartCommands)
        {
            OnChat(command);
        }
    }

    public void OnChat(string message)
    {
        string[] args = message.Split(' ');
        try
        {
            Log.Write("Starting command [args: " + args.Length + "]: " + string.Join(" ", args));
            ChatCommands[args[0]].Invoke(args);
            Log.Write("Command executed [args: " + args.Length + "]: " + string.Join(" ", args));
        }
        catch (Exception e)
        {
            Log.Write(LogLevel.Error, "Caught exception " + e.GetType().Name + " in " + args[0] + " handler.");
        }
    }
}