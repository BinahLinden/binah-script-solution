/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

/*
 
    Creates a table that records entry into a world, table name is world name
     {
     }
     */



public class DataStoreList : SceneObjectScript
{
    #region EditorProperties
    [TooltipAttribute("Use a long phrase that is unique and hard to guess, or a generated UUID")]
    [EditorVisible(true)]
    private readonly string dataStoreId;

    [DisplayName("World Name")]
    [Tooltip("Table created to store visitors to this world")]
    public readonly string WorldName = "";
    #endregion

    private DataStore dataStore;

    public class Visitor
    {
        public string Handle { get; internal set; }
        public string Name { get; internal set; }
        public string EntryTimes { get; internal set; }
        public string ExitTimes { get; internal set; }
    }


    public override void Init()
    {
        dataStore = ScenePrivate.CreateDataStore(dataStoreId);

        if (dataStore != null)
        {
            Log.Write("DataStore id is " + dataStore.Id);
            
            //subscribe to agent entry

            //dataStore.Restore<List<DateTime>>(timeKey, getStartTime);
        }
        else
        {
            Log.Write("Unable to create a data store with id " + dataStoreId);
        }
    }

    private void onAgentEntry()
    {

    }


}
