namespace _Project.Scripts.UI
{
     using UnityEngine;

     public abstract class BaseUI : MonoBehaviour
     {
          public bool IsOpen => gameObject.activeSelf;

          public IUIModel Model { get; private set; }

          internal void Open(IUIModel model = null)
          {
               Model = model;
               gameObject.SetActive(true);
               OnOpen(model);
          }

          internal void Close()
          {
               OnClose();
               gameObject.SetActive(false);
               Model = null;
          }

          /// <summary>
          /// Kept for UIs that do not need an open model.
          /// </summary>
          protected virtual void OnOpen() { }

          /// <summary>
          /// Override this method when the UI needs data at open time.
          /// </summary>
          protected virtual void OnOpen(IUIModel model) { OnOpen(); }

          protected virtual void OnClose() { }

          protected TModel GetModel<TModel>()
               where TModel : class, IUIModel
          {
               if (Model is TModel typedModel)
                    return typedModel;

               throw new System.InvalidOperationException(
                    $"[{GetType().Name}] Expected model {typeof(TModel).Name}, " +
                    $"but received {Model?.GetType().Name ?? "null"}.");
          }
     }
}
