#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Aya.DataBinding
{
    [CustomEditor(typeof(TypeBinder), true), CanEditMultipleObjects]
    public class TypeBinderEditor : BaseBinderEditor
    {
        public TypeBinder TypeBinder => target as TypeBinder;

        protected SerializedProperty BindingTypeProperty;
        protected SerializedProperty InstanceProperty;
        
        protected SerializedProperty ContainerKeyProperty;
        protected SerializedProperty DataKeyProperty;
        protected SerializedProperty DirectionProperty;
        protected SerializedProperty UpdateTypeProperty;

        protected SerializedProperty TypeProperty;
        protected SerializedProperty MapProperty;

        public virtual void OnEnable()
        {
            BindingTypeProperty = serializedObject.FindProperty("BindingType");
            InstanceProperty = serializedObject.FindProperty("Instance");
            
            ContainerKeyProperty = serializedObject.FindProperty("Container");
            DataKeyProperty = serializedObject.FindProperty("Key");
            DirectionProperty = serializedObject.FindProperty("Direction");
            UpdateTypeProperty = serializedObject.FindProperty("UpdateType");

            TypeProperty = serializedObject.FindProperty("Type");
            MapProperty = serializedObject.FindProperty("Map");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(BindingTypeProperty);
            var typeBinding = BindingTypeProperty.enumValueIndex == 0;
            if (typeBinding)
                DrawTypeMapping();
            else
                DrawInstanceMapping();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInstanceMapping()
        {
            DrawInstance(InstanceProperty);
            DrawDirection(DirectionProperty);

            if (InstanceProperty.objectReferenceValue == null) return;

            DrawTypeMapList(InstanceProperty.objectReferenceValue.GetType());
        }

        private void DrawTypeMapping()
        {
            DrawContainerKey(ContainerKeyProperty);
            DrawDataKey(DataKeyProperty);
            DrawDirection(DirectionProperty);
                
            // GUIUtil.AssemblyMenu("Assembly", AssemblyProperty);
            GUIUtil.TypeMenu("Type", TypeProperty);
                
            if (TypeCaches.TryFindDerivedBindable(TypeProperty.stringValue, out var currentType))
            {
                DrawTypeMapList(currentType);
            }
            else
            {
                if (TypeBinder.Map.Count > 0)
                {
                    TypeBinder.Map.Clear();
                }
            }
        }

        protected virtual void DrawTypeMapList(Type currentType)
        {
            var foldOutTitle = "Map [" + TypeBinder.Map.Count + "]";
            using (GUIFoldOut.Create(this, foldOutTitle))
            {
                if (!GUIFoldOut.GetState(this, foldOutTitle)) return;
                for (var i = 0; i < TypeBinder.Map.Count; i++)
                {
                    var map = TypeBinder.Map[i];
                    using (GUIGroup.Create())
                    {
                        using (GUIHorizontal.Create())
                        {
                            using (GUIVertical.Create())
                            {
                                var itemProperty = MapProperty.GetArrayElementAtIndex(i);
                                var propertyProperty = itemProperty.FindPropertyRelative("Property");
                                var componentProperty = itemProperty.FindPropertyRelative("Target");
                                var targetPropertyProperty = itemProperty.FindPropertyRelative("TargetProperty");

                                GUIMenu.DrawPropertyMenu(null, currentType, "Source Prop", propertyProperty);
                                DrawTargetAndProperty<Component>("Target", componentProperty, TypeBinder.transform, "Target Prop", targetPropertyProperty);
                            }

                            var btnDelete = GUILayout.Button("×", GUILayout.Width(EditorGUIUtility.singleLineHeight));
                            if (btnDelete)
                            {
                                TypeBinder.Map.Remove(map);
                            }
                        }
                    }

                    GUIUtil.ColorLine(EditorStyle.SplitLineColor, 2);
                }

                if (GUILayout.Button("＋"))
                {
                    var map = new TypeBindMap();
                    TypeBinder.Map.Add(map);
                }
            }
        }
    }
}
#endif