﻿using System;

namespace Aya.DataBinding
{
    public interface IDataBinder<out T>
    {
        T Value { get; }
    }
    
    public abstract class DataBinder<T> : DataBinder, IDataBinder<T>
    {
        public T PreviousData { get; internal set; }
        public int PreviousDataHashCode { get; internal set; }

        // Source Only
        public Action<T> OnValueChanged { get; set; } = delegate { };

        #region Reflection Cache

        public override Type BinderType
        {
            get
            {
                if (_binderType == null)
                {
                    _binderType = GetType();
                }

                return _binderType;
            }
        }

        private Type _binderType;

        public override Type DataType
        {
            get
            {
                if (_dataType == null)
                {
                    _dataType = typeof(T);
                }

                return _dataType;
            }
        }

        private Type _dataType;

        #endregion

        public abstract T Value { get; set; }

        public virtual void OnValueChangedCallback(T data)
        {
            UpdateSource();
        }

        public override void Broadcast()
        {
            if (!IsSource) return;
            var dataBinders = DataContainer.GetDestinations(Key);
            var data = Value;
            PreviousData = data;
            if (data != null)
                PreviousDataHashCode = data.GetHashCode();
            
            for (var i = 0; i < dataBinders.Count; i++)
            {
                var dataBinder = dataBinders[i];
                dataBinder.SetDataInternal(data);
            }
        }

        public override void UpdateSource()
        {
            if (!IsSource) return;
            var currentData = Value;
            if (currentData != null && PreviousData != null)
            {
                var currentDataHashCode = currentData.GetHashCode();
                if (CheckHashCodeEquals(currentDataHashCode, PreviousDataHashCode)) return;
            }
            else
            {
                if (CheckEquals(currentData, PreviousData)) return;
            }

            Broadcast();
            OnValueChanged?.Invoke(currentData);
        }

        public override void UpdateTarget()
        {
            if (!IsDestination) return;
            var latestData = DataContainer.GetData<T>(Container, Key);
            Value = latestData;
        }

        public virtual bool CheckHashCodeEquals(int data1, int data2)
        {
            return data1.Equals(data2);
        }

        public virtual bool CheckEquals(T data1, T data2)
        {
            if (data1 == null && data2 == null) return true;
            if (data1 == null || data2 == null) return false;
            return data1.Equals(data2);
        }
    }
}