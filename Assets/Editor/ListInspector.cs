using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
public class ListInspector<T>    
{
    private string _name;
    private List<T> _collection;
    private Func<T, T> _displayItem;
    private Func<T> _newItem;
    private bool _isOpen;

    public ListInspector(string name, List<T> collection, Func<T, T> displayItem, Func<T> newItem)
    {
        _name = name;
        _collection = collection;
        _displayItem = displayItem;
        _newItem = newItem;
        _isOpen = true;
    }

    public void Draw()
    {
        // Popup
        EditorGUILayout.BeginHorizontal();
        _isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_isOpen, _name);
        SetItemCount(EditorGUILayout.DelayedIntField(_collection.Count, GUILayout.Width(40f)));
        EditorGUILayout.EndHorizontal();

        if (!_isOpen) return;
        
        EditorGUILayout.BeginVertical("Box");
        // Display items
        for(int i = 0; i< _collection.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(20f));
            _collection[i] = _displayItem(_collection[i]);
            if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(40f)))
            {
                RemoveItem(i);
            }

            GUILayout.EndHorizontal();
        }

        // Control
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (_collection.Count > 0 && GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(40f))) RemoveItem(_collection.Count - 1);
        if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(40f))) AddItem();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.EndVertical();
    }

    private void AddItem()
    {
        _collection.Add(_newItem());
    }

    private void RemoveItem(int i)
    {
        _collection.RemoveAt(i);
    }

    private void SetItemCount(int count)
    {
        if (_collection.Count == count) return;
        if (count < 0) return;

        while (_collection.Count < count) AddItem();
        while (_collection.Count > count) RemoveItem(_collection.Count - 1);
    }
}
#endif