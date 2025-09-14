using System.Reflection;
using AIFramework.Common;
using Chocolate4.ScreenSystem;
using UnityEngine;

namespace Aya.DataBinding
{
    /// <summary>
    /// Binds a specific property to a specific target.
    /// This should be source
    /// </summary>
    public class InstanceRuntimePropertyBinder : DataBinder<object, object>
    {
        private readonly MemberInfo memberToModify;
        private readonly MemberInfo propertyFromInstance;
        private readonly Component toModify;

        private bool wasBound;
        
        public override object Value
        {
            get => propertyFromInstance.GetValue(Target);
            set => propertyFromInstance.SetValue(Target, value);
        }

        public InstanceRuntimePropertyBinder(MonoBehaviour instance,
            PropertyInfo instanceProperty,
            FieldInfo instanceField,
            DataDirection direction,
            Component toModify,
            PropertyInfo toModifyProperty,
            FieldInfo toModifyField)
        {
            this.toModify = toModify;
            memberToModify = toModifyProperty == null ? toModifyField : toModifyProperty;
            propertyFromInstance = instanceProperty == null ? instanceField : instanceProperty;
            Direction = direction;
            Target = instance;
        }
        
        public InstanceRuntimePropertyBinder(IBindable instance,
            PropertyInfo instanceProperty,
            FieldInfo instanceField,
            DataDirection direction,
            Component toModify,
            PropertyInfo toModifyProperty,
            FieldInfo toModifyField)
        {
            this.toModify = toModify;
            memberToModify = toModifyProperty == null ? toModifyField : toModifyProperty;
            propertyFromInstance = instanceProperty == null ? instanceField : instanceProperty;
            Direction = direction;
            Target = instance;
        }

        public override void Bind()
        {
            if (wasBound)
                return;
            
            wasBound = true;
            BindUpdater.Ins.Add(this);
        }

        public override void UnBind()
        {
            if (!wasBound)
                return;
            
            wasBound = false;
            BindUpdater.Ins.Remove(this);
        }

        public override void Broadcast()
        {
            if (!IsSource) return;
            var data = Value;
            PreviousData = data;
            if (data != null)
                PreviousDataHashCode = data.GetHashCode();

            memberToModify.SetValue(toModify, data);
        }
    }
}