using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// シンボルを設定するウィンドウを管理するクラス
/// </summary>
public class SymbolWindow : EditorWindow
{
    //===================================================================================================
    // クラス
    //===================================================================================================

    /// <summary>
    /// シンボルのデータを管理するクラス
    /// </summary>
    private class SymbolData
    {
        public string Name { get; private set; }   // 定義名を返します
        public string Comment { get; private set; }   // コメントを返します
        public bool IsEnable { get; set; }   // 有効かどうかを取得または設定します

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SymbolData(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            Comment = node.Attributes["comment"].Value;
        }
    }

    //===================================================================================================
    // 定数
    //===================================================================================================

    private const string ITEM_NAME = "Tools/Symbols";              // コマンド名
    private const string WINDOW_TITLE = "Symbols";                 // ウィンドウのタイトル
    private const string XML_PATH = "Assets/Scripts/Konno/Editor/symbols.xml";  // 読み込む .xml のファイルパス

    //===================================================================================================
    // 変数
    //===================================================================================================

    private static Vector2 mScrollPos;          // スクロール座標
    private static SymbolData[] mSymbolList;    // シンボルのリスト

    // コンパイル終了コールバック（CompilationPipeline.compilationFinished のデリゲート型に合わせる）
    private Action<object> mOnCompilationFinished;

    // delayCall 登録制御（同じ delay を複数登録しないためのフラグ）
    private bool mDelayRegistered = false;

    //===================================================================================================
    // 静的関数
    //===================================================================================================

    /// <summary>
    /// ウィンドウを開きます
    /// </summary>
    [MenuItem(ITEM_NAME)]
    private static void Open()
    {
        var window = GetWindow<SymbolWindow>(true, WINDOW_TITLE);
        window.Init();
        window.Show();
    }

    //===================================================================================================
    // 関数
    //===================================================================================================

    /// <summary>
    /// 初期化する時に呼び出します
    /// </summary>
    private void Init()
    {
        // XML 読み込み。ファイルがない場合は空データを作る（安全策）
        var document = new XmlDocument();
        try
        {
            document.Load(XML_PATH);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load symbols xml at '{XML_PATH}': {e.Message}");
            mSymbolList = new SymbolData[0];
            return;
        }

        var root = document.GetElementsByTagName("root")[0];
        var symbolList = new List<XmlNode>();

        foreach (XmlNode n in root.ChildNodes)
        {
            if (n.Name == "symbol")
            {
                symbolList.Add(n);
            }
        }

        mSymbolList = symbolList
            .Select(c => new SymbolData(c))
            .ToArray();

        var group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defineSymbols = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(group)
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var n in mSymbolList)
        {
            n.IsEnable = defineSymbols.Any(c => c == n.Name);
        }
    }

    /// <summary>
    /// GUI を表示する時に呼び出されます
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        mScrollPos = EditorGUILayout.BeginScrollView(
            mScrollPos,
            GUILayout.Height(position.height)
        );

        if (mSymbolList == null)
        {
            EditorGUILayout.LabelField("No symbols loaded.");
        }
        else
        {
            foreach (var n in mSymbolList)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                n.IsEnable = EditorGUILayout.Toggle(n.IsEnable, GUILayout.Width(16));
                if (GUILayout.Button("Copy", GUILayout.Width(48)))
                {
                    EditorGUIUtility.systemCopyBuffer = n.Name;
                }
                EditorGUILayout.LabelField(n.Name, GUILayout.ExpandWidth(true), GUILayout.MinWidth(0));
                EditorGUILayout.LabelField(n.Comment, GUILayout.ExpandWidth(true), GUILayout.MinWidth(0));
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(8);
        if (GUILayout.Button("Save"))
        {
            SaveAndCompile();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Save ボタンを押した時に呼ばれ、即コンパイルを確実に行うための処理を開始します
    /// </summary>
    private void SaveAndCompile()
    {
        if (mSymbolList == null)
        {
            Debug.LogWarning("No symbol list. Initialize first.");
            Init();
        }

        // enable なシンボルを収集して設定
        var defineSymbols = (mSymbolList ?? new SymbolData[0])
            .Where(c => c.IsEnable)
            .Select(c => c.Name)
            .ToArray();

        var group = EditorUserBuildSettings.selectedBuildTargetGroup;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            group,
            string.Join(";", defineSymbols)
        );

        // ProjectSettings に確実に書き込む（念のため）
        AssetDatabase.SaveAssets();

        // delayCall を使って一度エディタのメインループに制御を返す
        if (!mDelayRegistered)
        {
            mDelayRegistered = true;
            EditorApplication.delayCall += OnDelayCompile;
        }

        // ウィンドウを閉じる（好みにより閉じないよう変更可）
        Close();
    }

    /// <summary>
    /// delayCall で実行される。ここでコンパイル要求とコールバック登録を行う（重要）
    /// </summary>
    private void OnDelayCompile()
    {
        // 二重登録防止
        EditorApplication.delayCall -= OnDelayCompile;
        mDelayRegistered = false;

        // プログレス表示
        EditorUtility.DisplayProgressBar("Compiling", "Scripts are compiling...", 0f);

        // コンパイル終了時のコールバックを登録（先に登録しておく）
        mOnCompilationFinished = (ctx) =>
        {
            EditorUtility.ClearProgressBar();

            // 念のため登録解除
            try
            {
                CompilationPipeline.compilationFinished -= mOnCompilationFinished;
            }
            catch { /* ignore */ }

            mOnCompilationFinished = null;

            Debug.Log("Script compilation finished (SymbolWindow).");
        };

        CompilationPipeline.compilationFinished += mOnCompilationFinished;

        // 明示的にスクリプトコンパイルをリクエスト
        CompilationPipeline.RequestScriptCompilation();
    }

    private void OnDestroy()
    {
        // ウィンドウが閉じられるときに後片付け
        if (mOnCompilationFinished != null)
        {
            try
            {
                CompilationPipeline.compilationFinished -= mOnCompilationFinished;
            }
            catch { /* ignore */ }
            mOnCompilationFinished = null;
        }

        if (mDelayRegistered)
        {
            try
            {
                EditorApplication.delayCall -= OnDelayCompile;
            }
            catch { /* ignore */ }
            mDelayRegistered = false;
        }

        EditorUtility.ClearProgressBar();
    }
}
