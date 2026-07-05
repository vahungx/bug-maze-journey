namespace _Project.Scripts.UI
{
     using UnityEngine;

     public abstract class BaseUI : MonoBehaviour
     {
          public bool IsOpen => gameObject.activeSelf;

          internal void Open()
          {
               gameObject.SetActive(true);
               OnOpen();
          }

          internal void Close()
          {
               OnClose();
               gameObject.SetActive(false);
          }

          protected virtual void OnOpen() { }

          protected virtual void OnClose() { }
     }
}
