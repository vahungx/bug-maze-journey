namespace _Project.Scripts.Shared.Services
{
     using System;
     using System.Collections.Generic;

     public static class ServiceLocator
     {
          private sealed class ServiceEntry
          {
               public object Service;
               public bool   DisposeOnReset;
          }

          private static readonly Dictionary<Type, ServiceEntry> Services = new();

          public static void Register<T>(T service, bool overwrite = false, bool disposeOnReset = true)
               where T : class
          {
               if (service == null) throw new ArgumentNullException(nameof(service));

               var type = typeof(T);

               if (Services.TryGetValue(type, out var existing))
               {
                    if (!overwrite)
                         throw new InvalidOperationException(
                              $"[ServiceLocator] Service already registered: {type.FullName}");

                    if (existing.DisposeOnReset && existing.Service is IDisposable disposable)
                         disposable.Dispose();
               }

               Services[type] = new ServiceEntry
               {
                    Service        = service,
                    DisposeOnReset = disposeOnReset
               };
          }

          public static T Resolve<T>()
               where T : class
          {
               var type = typeof(T);

               if (!Services.TryGetValue(type, out var entry))
                    throw new InvalidOperationException($"[ServiceLocator] Service not registered: {type.FullName}");

               return (T)entry.Service;
          }

          public static bool TryResolve<T>(out T service)
               where T : class
          {
               if (Services.TryGetValue(typeof(T), out var entry))
               {
                    service = (T)entry.Service;

                    return true;
               }

               service = null;

               return false;
          }

          public static bool IsRegistered<T>()
               where T : class
          {
               return Services.ContainsKey(typeof(T));
          }

          public static void Unregister<T>(bool dispose = true)
               where T : class
          {
               var type = typeof(T);

               if (!Services.TryGetValue(type, out var entry)) return;

               if (dispose && entry.DisposeOnReset && entry.Service is IDisposable disposable)
                    disposable.Dispose();

               Services.Remove(type);
          }

          public static void Reset(bool dispose = true)
          {
               if (dispose)
               {
                    foreach (var pair in Services)
                    {
                         var entry = pair.Value;

                         if (entry.DisposeOnReset && entry.Service is IDisposable disposable)
                              disposable.Dispose();
                    }
               }

               Services.Clear();
          }
     }
}
