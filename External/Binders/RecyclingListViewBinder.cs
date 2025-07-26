using System.Collections.Generic;
using Aya.DataBinding;
using UnityEngine;

namespace Project.Game.Views
{
    [AddComponentMenu("Data Binding/RecyclingListView Binder")]
    public class RecyclingListViewBinder : ComponentBinder<RecyclingListView, List<object>, RuntimeRecyclingListViewBinder>
    {
        public override bool NeedUpdate => true;
    }

    public class RuntimeRecyclingListViewBinder : DataBinderList<RecyclingListView, List<object>>
    {
        #region DataBinderList

        public override bool NeedUpdate => true;
        
        public override List<object> Value
        {
            get => list;
            set
            {
                if (list != null)
                {
                    foreach (var binder in registeredBinders)
                    {
                        binder.UnBind();
                    }
                
                    registeredBinders.Clear();
                }
                
                list = value;
                if (value == null)
                {
                    Target.RowCount = 0;
                    return;
                }
                
                for (var rowIndex = 0; rowIndex < value.Count; rowIndex++)
                {
                    var obj = value[rowIndex];
                    var key = $"{Key}[{rowIndex}]";
                    var binder = UBind.BindSource(Key, key, obj);
                    
                    registeredBinders.Add(binder);
                }

                Target.ItemCallback = PopulateItem;
                Target.RowCount = value.Count;
            }
        }

        #endregion

        private readonly List<RuntimeTypeBinder> registeredBinders = new();
        
        private List<object> list;
        
        private void PopulateItem(RecyclingListViewItem item, int rowIndex)
        {
            if (!item.TryGetComponent<ElementTypeBinder>(out var binder))
            {
                Debug.LogError("You forgot to add the binder");
                return;
            }

            var newKey = $"{Key}[{rowIndex}]";
            binder.Bind(Key, newKey);
        }
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(RecyclingListViewBinder)), UnityEditor.CanEditMultipleObjects]
    public class RecyclingListViewBinderEditor : ComponentBinderEditor<RecyclingListView, List<object>, RuntimeRecyclingListViewBinder>
    {
    }

#endif
}