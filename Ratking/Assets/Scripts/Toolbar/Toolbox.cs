using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[Serializable]
public class ToolboxData
{
    public SerializedDictionary<int, ResourceLocation> Tools;

    public ToolboxData()
    {
       
    }
}

public class Toolbox : MonoBehaviour, IDataPersistence
{
    private SerializedDictionary<int, ToolBase> Tools=new SerializedDictionary<int, ToolBase>();

   public int Slots=5;
   public bool IsEnabled = true;

   public void RemoveTool(int index)
   {
       if (Tools.ContainsKey(index))
       {
           this.Tools[index] = null;
           if (OnToolboxUpdated != null)
               OnToolboxUpdated.Invoke();
       }
     
    }

   public void PlaceTool(ToolBase tool)
   {
       tool.gameObject.transform.SetParent(this.transform);
       if (HasFreeSlot())
       {
           int index = -1;
           foreach (KeyValuePair<int, ToolBase> keyValuePair in this.Tools)
           {
               if (keyValuePair.Value == null)
               {
                   Tools[keyValuePair.Key] = tool;
                   tool.SetInToolbox(this,keyValuePair.Key);
                   break;
               }
           }

            //this.Tools.Add(tool);
           if (OnToolboxUpdated != null)
           {
               OnToolboxUpdated.Invoke();
           }
       }
   }

   public void PlaceTool(ToolBase tool, int index)
   {
       if (Tools.ContainsKey(index))
       {
           this.Tools[index] = tool;
           tool.SetInToolbox(this,index);
       }
       if (OnToolboxUpdated != null)
       {
           OnToolboxUpdated.Invoke();
       }
    }
    public void UseTool(int index)
   {
       Debug.Log($"Attempted to use ToolBase {index}");

        if (Tools.ContainsKey(index) && Tools[index]!=null)
       {
           Debug.Log($"Use ToolBase {index}");
           this.Tools[index].OnSelected();
           if (OnToolboxUpdated!=null)
           {
               OnToolboxUpdated.Invoke();
           }
       }
   }

   public delegate void ToolboxUpdated();

   public event ToolboxUpdated OnToolboxUpdated;

   public ToolBase GetTool(int index)
   {
       if (Tools.ContainsKey(index))
       {
           return this.Tools[index];
       }

       return null;
   }

   public void useToolInputOne(InputAction.CallbackContext context)
   {
       if (!(context.performed))
           return;
       UseTool(1);
   }
   public void useToolInputTwo(InputAction.CallbackContext context)
   {
       if (!(context.performed))
           return;
       UseTool(2);
   }
   public void useToolInputThree(InputAction.CallbackContext context)
   {
       if (!(context.performed))
           return;
       UseTool(3);
   }
   public void useToolInputFour(InputAction.CallbackContext context)
   {
       if (!(context.performed))
           return;
       UseTool(4);
   }
   public void useToolInputFive(InputAction.CallbackContext context)
   {
       if (!(context.performed))
           return;
       UseTool(5);
   }

  

   public void Clear()
   {
       this.Tools.Clear();
   }

   public bool HasFreeSlot()
   {
       foreach (KeyValuePair<int, ToolBase> keyValuePair in this.Tools)
       {
           if (keyValuePair.Value == null)
           {
               return true;
           }
       }

       return false;
   }


   public void LoadData(GameData data)
   {
       
       this.Tools = new SerializedDictionary<int, ToolBase>
       {
           { 1, null }, {2, null },{ 3, null },{ 4, null },{ 5, null },
       };

       if (data.ToolboxData!= null && data.ToolboxData.Tools!=null)
       {
           foreach (var toolLocation in data.ToolboxData.Tools)
           {
               if (toolLocation.Value==null)
               {
                   continue;
               }
               ToolBase toolBase = Resources.Load<ToolBase>(toolLocation.Value.ResourceName);
               if (toolBase != null)
               {
                   var t = Instantiate(toolBase, this.transform);
                   t.SetInToolbox(this, toolLocation.Key);
                   this.Tools[toolLocation.Key] = t;
               }
           }
        }

       
       
        if (OnLoaded!=null)
           OnLoaded.Invoke(data);
   }

   public void SaveData(ref GameData data)
   {

        data.ToolboxData.Tools=new SerializedDictionary<int, ResourceLocation>
        {
            { 1, null }, { 2, null },{ 3, null },{ 4, null },{ 5, null },
        };

        foreach (var tool in Tools)
        {
            if (tool.Value == null)
            {
                data.ToolboxData.Tools[tool.Key] = new ResourceLocation() { ResourceName = "" };

            }
            else
            {
                data.ToolboxData.Tools[tool.Key] = new ResourceLocation() { ResourceName = tool.Value.ResourcePath };
            }
        }

   }

    public IDataPersistence.Loaded OnLoaded { get; set; }
}
