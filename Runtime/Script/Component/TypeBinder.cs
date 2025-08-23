using System;
using System.Collections.Generic;
using Chocolate4.ScreenSystem;
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

    public enum BindingType
    {
        Type, Instance
    }
    
    [AddComponentMenu("Data Binding/Type Binder")]
    public class TypeBinder : MonoBehaviour
    {
        public BindingType BindingType;
        
        public MonoBehaviour Instance;
        
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
                binder.UpdateTarget();
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
            if (BindingType == BindingType.Type)
            {
                if (!TypeCaches.TryFindDerivedBindable(Type, out var type))
                {
                    Debug.LogError($"Couldn't find bindable for {Type}");
                    return false;
                }
                
                CacheType(type);
            }
            else if (BindingType == BindingType.Instance)
                CacheInstance();
            
            return true;
        }

        private void CacheInstance()
        {
            var instanceType = Instance.GetType();
            foreach (var map in Map)
            {
                var (targetProperty, targetField) =
                    TypeCaches.GetTypePropertyOrFieldByName(map.Target.GetType(), map.TargetProperty);
                
                var (instanceProperty, instanceField) =
                    TypeCaches.GetTypePropertyOrFieldByName(instanceType, map.Property);
                
                var binder = new InstanceRuntimePropertyBinder(Instance, instanceProperty, instanceField, Direction, map.Target, targetProperty, targetField);
                _binderCaches.Add(binder);
            }
        }

        private void CacheType(Type type)
        {
            foreach (var map in Map)
            {
                var key = type.Name + "." + map.Property + "." + Key;
                var (property, field) =
                    TypeCaches.GetTypePropertyOrFieldByName(map.Target.GetType(), map.TargetProperty);
                
                // binder => target is ur source
                // if direction != source do nothing
                // thats all
                // value => value of the property on target
                var binder = new RuntimePropertyBinder(Container, key, Direction, map.Target, property, field);
                _binderCaches.Add(binder);
            }
        }
    }
}