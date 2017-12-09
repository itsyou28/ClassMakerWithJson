using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using LitJson;

public class ClassMaker : EditorWindow
{

    [MenuItem("Tools/ClassMaker")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<ClassMaker>();
    }


    ModuleBuilder moduleBuilder;
    AssemblyName assemblyName;
    AssemblyBuilder assemblyBuilder;

    string fieldName = "";

    private void Awake()
    {
        InitializeAssembly();
    }

    private void InitializeAssembly()
    {

        // Get the current application domain for the current thread
        AppDomain currentDomain = AppDomain.CurrentDomain;

        // Create a dynamic assembly in the current application domain,
        // and allow it to be executed and saved to disk.
        assemblyName = new AssemblyName("AutoClass");
        assemblyBuilder = currentDomain.DefineDynamicAssembly(assemblyName,
                                              AssemblyBuilderAccess.RunAndSave);

        // Define a dynamic module in "MyEnums" assembly.
        // For a single-module assembly, the module has the same name as the assembly.
        moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name,
                                          assemblyName.Name + ".dll");
    }

    string classname = "SampleName";
    string jsonStr;

    private void OnGUI()
    {
        if (GUILayout.Button("Create"))
        {
            if(string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("json 데이터를 입력해주세요");
                return;
            }

            JsonReader reader = null;
            try
            {
                reader = new JsonReader(jsonStr);
            }
            catch (Exception e)
            {
                Debug.LogError("입력한 json 문법을 확인해주세요" + e.Message);
                return;
            }

            if (moduleBuilder == null)
                InitializeAssembly();

            try
            {
                TypeBuilder dataBuilder = moduleBuilder.DefineType(classname, TypeAttributes.Public);

                fieldName = "";
                CreateClass(ref reader, ref dataBuilder, classname);

                dataBuilder.CreateType();

                //클래스가 정의된 dll을 생성한다. 
                assemblyBuilder.Save(assemblyName.Name + ".dll");

                //유니티 프로젝트에서 사용할 수 있도록 Plugins 폴더에 dll을 이동한다. 
                if(File.Exists("Assets/Plugins/"+assemblyName.Name + ".dll"))
                    File.Delete("Assets/Plugins/" + assemblyName.Name + ".dll");

                File.Move(assemblyName.Name + ".dll", "Assets/Plugins/" + assemblyName.Name + ".dll");

                Debug.LogWarning("Complete Create Protocol Data Field");

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        classname = GUILayout.TextField(classname);
        jsonStr = EditorGUILayout.TextArea(jsonStr, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    /// <summary>
    /// Token을 분석해 재귀호출구조로 
    /// json을 ToObject<T>() 형태로 파싱될 수 있도록 자동으로 클래스를 생성한다. 
    /// </summary>
    /// <param name="_reader"></param>
    /// <param name="_builder"></param>
    /// <param name="className"></param>
    void CreateClass(ref JsonReader _reader, ref TypeBuilder _builder, string className, bool isMakeArr = false)
    {
        while (_reader.Read())
        {
            Debug.Log(_reader.Token + " // " + _reader.Value);
            switch (_reader.Token)
            {
                case JsonToken.ArrayStart:
                    string newArrClassName = className + "_" + fieldName;

                    Debug.LogWarning("ArrayStart : " + newArrClassName);

                    TypeBuilder newArrBuilder = moduleBuilder.DefineType(newArrClassName, TypeAttributes.Public);

                    string arrFieldname = fieldName;
                    fieldName = "";

                    CreateClass(ref _reader, ref newArrBuilder, newArrClassName, true);

                    Type newType = newArrBuilder.CreateType();
                    _builder.DefineField(arrFieldname, newType.MakeArrayType(), FieldAttributes.Public);
                    Debug.Log(className + " Define Array Field : " + arrFieldname);
                    break;
                case JsonToken.ArrayEnd:
                    Debug.LogWarning("ArrayEnd : " + className);
                    return;
                case JsonToken.ObjectStart:
                    if (fieldName == "")
                        break;
                    string objectFieldName = fieldName;
                    string newClassName = className + "_" + fieldName;

                    Debug.LogWarning("ObjectStart : " + newClassName);
                    
                    TypeBuilder newObjectBuilder = moduleBuilder.DefineType(newClassName, TypeAttributes.Public);

                    CreateClass(ref _reader, ref newObjectBuilder, newClassName);
                    _builder.DefineField(objectFieldName, newObjectBuilder.CreateType(), FieldAttributes.Public);
                    break;
                case JsonToken.ObjectEnd:
                    if (isMakeArr)
                        break;
                    Debug.LogWarning("ObjectEnd : " + className);
                    return;

                case JsonToken.PropertyName:
                    fieldName = _reader.Value.ToString();
                    break;
                case JsonToken.Int:
                    _builder.DefineField(fieldName,
                                    typeof(int), FieldAttributes.Public);
                    break;
                case JsonToken.Double:
                    _builder.DefineField(fieldName,
                                    typeof(double), FieldAttributes.Public);
                    break;
                case JsonToken.String:
                    _builder.DefineField(fieldName,
                                    typeof(String), FieldAttributes.Public);
                    break;
                case JsonToken.Long:
                    _builder.DefineField(fieldName,
                                    typeof(long), FieldAttributes.Public);
                    break;
                case JsonToken.Boolean:
                    _builder.DefineField(fieldName,
                                    typeof(Boolean), FieldAttributes.Public);
                    break;
            }

            switch (_reader.Token)
            {
                case JsonToken.Int:
                case JsonToken.Double:
                case JsonToken.String:
                case JsonToken.Long:
                case JsonToken.Boolean:
                    Debug.Log(className + " // Create Field!! // " + _reader.Token + " // " + fieldName);
                    break;
            }
        }
    }
}
