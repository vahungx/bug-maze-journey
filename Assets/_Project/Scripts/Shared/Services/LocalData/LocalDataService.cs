namespace _Project.Scripts.Shared.Services.LocalData
{
     using System;
     using System.Collections.Generic;
     using System.Reflection;
     using UnityEngine;

     public sealed class LocalDataService : ILocalDataService,
          IDisposable
     {
          private readonly Dictionary<Type, BaseLocalData> _dataByType = new();
          private          bool                            _isInitialized;

          public bool IsInitialized => _isInitialized;

          public LocalDataService()
          {
               var dataTypeByEnum = new Dictionary<LocalDataEnum, Type>();
               var baseType       = typeof(BaseLocalData);
               var allTypes       = Assembly.GetAssembly(baseType).GetTypes();

               foreach (var dataType in allTypes)
               {
                    var isConcreteLocalData = dataType.IsClass && !dataType.IsAbstract && dataType.IsSubclassOf(baseType);

                    if (!isConcreteLocalData) continue;

                    var data = (BaseLocalData)Activator.CreateInstance(dataType);

                    if (data == null) continue;

                    var enumType = data.GetEnumType();

                    if (!dataTypeByEnum.TryAdd(enumType, dataType))
                    {
                    #if UNITY_EDITOR
                         Debug.LogError($"Multiple local data classes use enum type {enumType}.");
                    #endif
                         continue;
                    }

                    _dataByType.TryAdd(dataType, data);
               }
          }

          public void Initialize(bool forceReload = false)
          {
               if (_isInitialized && !forceReload) return;

               _isInitialized = false;

               foreach (var data in _dataByType.Values)
               {
                    data.Load();
               }

               _isInitialized = true;
          #if UNITY_EDITOR
               Debug.Log("<color=yellow>Loaded local Data</color>");
          #endif
          }

          public void Save()
          {
               if (!_isInitialized) return;

               foreach (var data in _dataByType.Values)
               {
                    data.Save();
               }

               PlayerPrefs.Save();
          #if UNITY_EDITOR
               Debug.Log("<color=yellow>Saved local data...</color>");
          #endif
          }

          public T Get<T>()
               where T : BaseLocalData
          {
               if (!_isInitialized) Initialize();

               return _dataByType.TryGetValue(typeof(T), out var data) ? data as T : null;
          }

          public void Dispose() { Save(); }
     }
}
