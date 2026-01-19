using System;
using System.Collections.Generic;

namespace WindowsCleanerUtility.Services
{
    // Устаревший контейнер для внедрения зависимостей
    // Используйте Microsoft.Extensions.DependencyInjection вместо этого класса
    [Obsolete("Use Microsoft.Extensions.DependencyInjection instead")]
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            throw new InvalidOperationException($"Service of type {typeof(T)} is not registered");
        }
    }
}