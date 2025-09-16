using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Collector : MonoBehaviour, IInteractAction
{
   
    // Start is called before the first frame update
    private List<Collectible> _collectiblesInrange;


    private void Start()
    {
        _collectiblesInrange = new List<Collectible>();
        this.Priority = 100;
        LevelData.InteractActionManager.RegisterAction(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Collectible collectible = col.gameObject.GetComponent<Collectible>();
        if (collectible != null)
        {
            TutorialBox.Instance.SetActionTutorialText(TutorialBox.TutorialAction.ItemInRange);
            _collectiblesInrange.Add(collectible);
            _collectiblesInrange= _collectiblesInrange.OrderBy(x=> Vector3.Distance(this.gameObject.transform.position,x.gameObject.transform.position)).ToList();
            if (OnChangedAvailability!=null)
            {
               OnChangedAvailability.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        Collectible collectible = col.gameObject.GetComponent<Collectible>();
        if (collectible != null)
        {
            _collectiblesInrange.Remove(collectible);
            //if (_collectiblesInrange.Count == 0)
            //{
            //    PromptManager.Remove(KeyCode.E);
            //}
            if (OnChangedAvailability != null)
            {
                OnChangedAvailability.Invoke();
            }
        }
    }


    Collectible _lastClosest=null;
    // Update is called once per frame
    private void Update()
    {
        if (TutorialBox.Instance.isActiveAndEnabled)
            return;

    }

    [SerializeField] public GameObject PromptOnActive;
   
    public int Priority { get; private set; }
    public bool CanPerform()
    {
       return _collectiblesInrange.Count != 0 && LevelData.PlayerObject.GetComponent<Thrower>().ThrowItemPrefab == null && LevelData.InventoryView.CanEnable();
    }

    public void OnActivated()
    {
        Vector2 downPoint = _collectiblesInrange[0].GetComponent<Collider2D>().ClosestPoint(Vector2.down * 1000);
        LevelData.ButtonPromptManager.SpawnOrUpdate(KeyCode.E, downPoint, PromptOnActive);
    }

    public void OnDeactivated()
    {
        LevelData.ButtonPromptManager.Remove(KeyCode.E);
    }

    public void ActiveUpdate()
    {
        if (_collectiblesInrange.Count != 0)
        {

            if (_lastClosest != _collectiblesInrange[0])
            {
                Vector2 downPoint = _collectiblesInrange[0].GetComponent<Collider2D>().ClosestPoint(Vector2.down * 1000);
                LevelData.ButtonPromptManager.SpawnOrUpdate(KeyCode.E, downPoint, PromptOnActive);
            }
            _lastClosest = _collectiblesInrange[0];

        }
    }

   
    public void DoAction()
    {
        Collectible first = _collectiblesInrange.First();
        _collectiblesInrange.Remove(first);
        
        LevelData.InventoryView.SetItem(first);
        Destroy(first.gameObject);
        LevelData.InventoryView.gameObject.SetActive(true);
        
    }

    public int CompareTo(IInteractAction other)
    {
        return Priority.CompareTo(other.Priority);
    }

    public event IInteractAction.ChangedAvailability OnChangedAvailability;
}