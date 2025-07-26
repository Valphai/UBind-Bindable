using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aya.DataBinding
{
    [Serializable]
    public class TypeBindMap
    {
        public string Property;
        public Component Target;
        public string TargetProperty;
    }

    [AddComponentMenu("Data Binding/Type Binder")]
    public class TypeBinder : MonoBehaviour
    {
        public string Container = DataContainer.Default;
        public string Key;
        public DataDirection Direction = DataDirection.Target;

        public string Type;
        public List<TypeBindMap> Map = new List<TypeBindMap>();

        protected List<DataBinder> _binderCaches;

        public virtual void OnEnable()
        {
            if (_binderCaches == null)
            {
                if (!TryCacheBinders()) 
                    return;
            }

            foreach (var binder in _binderCaches)
            {
                binder.Bind();
                //binder.UpdateTarget();
            }
        }

        public virtual void OnDisable()
        {
            foreach (var binder in _binderCaches)
            {
                binder.UnBind();
            }
        }
        
        public void Bind(string container, string newKey)
        {
            if (_binderCaches == null)
                TryCacheBinders();
            
            if (string.IsNullOrEmpty(container) || string.IsNullOrEmpty(newKey))
                return;

            foreach (var binder in _binderCaches)
            {
                binder.Container = container;
                if (string.IsNullOrEmpty(Key))
                {
                    binder.Key += newKey;
                    continue;
                }
                
                binder.Key = binder.Key.Replace(Key, newKey);
            }

            Container = container;
            Key = newKey;

            foreach (var binder in _binderCaches)
            {
                binder.UpdateSource();
                binder.UpdateTarget();
            }
        }
        
        private bool TryCacheBinders()
        {
            _binderCaches = new List<DataBinder>();
            if (!TypeCaches.TryFindDerivedBindable(Type, out var type))
            {
                Debug.LogError($"Couldn't find bindable for {Type}");
                return false;
            }
                
            foreach (var map in Map)
            {
                var key = type.Name + "." + map.Property + "." + Key;
                var (property, field) =
                    TypeCaches.GetTypePropertyOrFieldByName(map.Target.GetType(), map.TargetProperty);
                var binder = new RuntimePropertyBinder(Container, key, Direction, map.Target, property, field);
                _binderCaches.Add(binder);
            }

            return true;
        }
    }
}