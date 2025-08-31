using System;
using System.Collections.Generic;
using System.Linq;

namespace Aya.DataBinding
{
    public class RuntimeTypeBinder : DataBinder<object, object>
    {
        public override Type TargetType => Target.GetType();

        private List<DataBinder> _binderCaches;

        public RuntimeTypeBinder(string container, string key, DataDirection direction, object target)
        {
            Container = container;
            Key = key;
            Direction = direction;
            Target = target;
        }

        public override void Bind()
        {
            if (_binderCaches == null)
            {
                _binderCaches = new List<DataBinder>();
                var type = TargetType;
                var (properties, fields) = TypeCaches.GetTypePropertiesAndFields(type);
                    
                foreach (var propertyInfo in properties)
                {
                    // exclude properties without a setter
                    var propertyDirection = Direction;
                    if (Direction == DataDirection.Both && propertyInfo.GetSetMethod() == null)
                        propertyDirection = DataDirection.Source;
                    else if (Direction == DataDirection.Target && propertyInfo.GetSetMethod() == null)
                        continue;
                    
                    var key = type.Name + "." + propertyInfo.Name + "." + Key;
                    var binder = new RuntimePropertyBinder(Container, key, propertyDirection, Target, propertyInfo, null);
                    _binderCaches.Add(binder);
                }

                foreach (var fieldInfo in fields)
                {
                    // exclude const from set

                    var isConst = fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
                    var fieldDirection = Direction;
                    if (Direction == DataDirection.Both && isConst)
                        fieldDirection = DataDirection.Source;
                    else if (Direction == DataDirection.Target && isConst)
                        continue;
                    
                    var key = type.Name + "." + fieldInfo.Name + "." + Key;
                    var binder = new RuntimePropertyBinder(Container, key, fieldDirection, Target, null, fieldInfo);
                    _binderCaches.Add(binder);
                }
            }

            foreach (var binder in _binderCaches)
            {
                binder.Bind();
                binder.UpdateTarget();
            }
        }

        public override void UnBind()
        {
            foreach (var binder in _binderCaches)
            {
                binder.UnBind();
            }
        }

        public override object Value { get; set; }
    }
}
