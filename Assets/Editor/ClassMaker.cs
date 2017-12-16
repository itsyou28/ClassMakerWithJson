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
                TypeBuilder dataBuilder = CreateClass(ref reader, classname);

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
    /// Token을 분석해 Generic type 일 경우 필드를 생성하고 ObjectStart 일 경우 재귀호출을 통해 클래스를 생성한다. 
    /// json을 ToObject<T>() 형태로 파싱될 수 있도록 자동으로 클래스를 생성한다. 
    /// </summary>
    /// <param name="_reader"></param>
    /// <param name="_className"></param>
    TypeBuilder CreateClass(ref JsonReader _reader, string _className)
    {
        TypeBuilder _builder = moduleBuilder.DefineType(_className, TypeAttributes.Public);
        Debug.LogWarning("Define New Type : " + _builder.Name);

        JsonToken curToken = JsonToken.Null;
        JsonToken beforeToken = JsonToken.Null;
        string propertyName = "";

        while (_reader.Read())
        {
            beforeToken = curToken;
            curToken = _reader.Token;

            Debug.Log("before token : " + beforeToken  +"        READ " + _reader.Token + " // " + _reader.Value);

            switch (curToken)
            {
                case JsonToken.ArrayStart://'['
                    Debug.Log("ArrayStart " );
                    break;
                case JsonToken.ArrayEnd://']'
                    Debug.Log("ArrayEnd ");
                    break;
                case JsonToken.ObjectStart://'{'
                    if (beforeToken == JsonToken.Null)
                        break;

                    Debug.Log("ObjectStart ");
                    
                    string objectFieldName = propertyName;
                    string newClassName = _className + "_" + propertyName;
                    
                    //새타입의 필드 정의를 위한 재귀호출
                    TypeBuilder newObjectBuilder = CreateClass(ref _reader, newClassName);

                    //정의된 타입으로 현재 클래스의 멤버 변수 생성. 직전 토큰이 ArrayStart 였을 경우 배열 타입으로 생성
                    if (beforeToken == JsonToken.ArrayStart)
                    {
                        Type arrType = newObjectBuilder.CreateType();
                        _builder.DefineField(objectFieldName, arrType.MakeArrayType(), FieldAttributes.Public);
                        Debug.LogWarning("Define Array Field // " + newObjectBuilder.Name + " // " + _builder.Name + "." + objectFieldName + "[]");
                    }
                    else
                    {
                        _builder.DefineField(objectFieldName, newObjectBuilder.CreateType(), FieldAttributes.Public);
                        Debug.LogWarning("Define Field // " + newObjectBuilder.Name + " // " + _builder.Name + "." + objectFieldName);
                    }

                    break;
                case JsonToken.ObjectEnd://'}'
                    Debug.LogWarning("ObjectEnd : " + _className);
                    return _builder;
                case JsonToken.PropertyName:
                    propertyName = _reader.Value.ToString();
                    break;
                case JsonToken.Int:
                    if (beforeToken == JsonToken.ArrayStart)
                        _builder.DefineField(propertyName, typeof(int).MakeArrayType(), FieldAttributes.Public);
                    else
                        _builder.DefineField(propertyName, typeof(int), FieldAttributes.Public);
                    break;
                case JsonToken.Double:
                    if (beforeToken == JsonToken.ArrayStart)
                        _builder.DefineField(propertyName, typeof(double).MakeArrayType(), FieldAttributes.Public);
                    else
                        _builder.DefineField(propertyName, typeof(double), FieldAttributes.Public);
                    break;
                case JsonToken.String:
                    if (beforeToken == JsonToken.ArrayStart)
                        _builder.DefineField(propertyName, typeof(String).MakeArrayType(), FieldAttributes.Public);
                    else
                        _builder.DefineField(propertyName, typeof(String), FieldAttributes.Public);
                    break;
                case JsonToken.Long:
                    if (beforeToken == JsonToken.ArrayStart)
                        _builder.DefineField(propertyName, typeof(long).MakeArrayType(), FieldAttributes.Public);
                    else
                        _builder.DefineField(propertyName, typeof(long), FieldAttributes.Public);
                    break;
                case JsonToken.Boolean:
                    if (beforeToken == JsonToken.ArrayStart)
                        _builder.DefineField(propertyName, typeof(Boolean).MakeArrayType(), FieldAttributes.Public);
                    else
                        _builder.DefineField(propertyName, typeof(Boolean), FieldAttributes.Public);
                    break;
            }

            switch (_reader.Token)
            {
                case JsonToken.Int:
                case JsonToken.Double:
                case JsonToken.String:
                case JsonToken.Long:
                case JsonToken.Boolean:
                    if(beforeToken == JsonToken.ArrayStart)
                        Debug.LogWarning("Create Array Field // " + _builder.Name + "." + propertyName + "[] // " + _reader.Token + " // " + _reader.Value);
                    else
                        Debug.LogWarning("Define Field // " + _builder.Name + "." + propertyName + " // " + _reader.Token + " // " + _reader.Value );
                    break;
            }
        }

        return _builder;
    }
}
