using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unityを再起動するためのクラス
/// </summary>
public class EditorRestartUnity
{
    [MenuItem("File/Restart")]
    static void RestartUnity()
    {
        // Unityの実行ファイルのフルパス
        string unityPath = EditorApplication.applicationPath;
        // プロジェクトのルートフォルダーパス
        string projectPath = Directory.GetCurrentDirectory().Replace("\"", "");
        // 現在のUnityエディタのプロセスID
        int oldPid = Process.GetCurrentProcess().Id;

        try
        {
            // OSの判定（Windowsだった場合）
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // 一時ファイルとしてバッチファイルのパスを作る
                string batPath = Path.Combine(Path.GetTempPath(), "UnityRestartHelper.bat");
                // バッチの中身を行ごとの配列で作る（エスケープ問題を避けるため行配列で生成）
                string[] batLines = new string[]
                {
                    "@echo off",    // コマンドのエコーを切る（表示を抑える）
                    "REM args: %1 = unityExe, %2 = projectPath, %3 = oldPid",   // 引数の説明
                    "set UNITY_EXE=%~1",    // %~1: 引数1（囲みの"を取り除いたパス）を環境変数に入れる
                    "set PROJECT=%~2",      // %~2: プロジェクトパス
                    "set OLD_PID=%~3",      // %~3: 古いプロセスの PID
                    "",
                    ":waitloop",            // ラベル：プロセス存在チェックループ開始
                    // tasklist で PID を検索し、見つかる限り待つ（PID が消える＝プロセス終了）
                    "tasklist /FI \"PID eq %OLD_PID%\" | findstr /R /C:\"%OLD_PID%\" >nul",
                    "if %ERRORLEVEL%==0 (",
                    "    timeout /t 1 /nobreak >nul", // 1秒待って再チェック
                    "    goto waitloop",
                    ")",
                    "",
                    ":waitlock",    // ラベル：Unity のロックファイル確認ループ開始
                    // プロジェクトの Temp/UnityLockfile が残っている間は待つ（ロック解放を待つ）
                    "if exist \"%PROJECT%\\Temp\\UnityLockfile\" (",
                    "    timeout /t 1 /nobreak >nul",
                    "    goto waitlock",
                    ")",
                    "",
                    // Unity を起動するコマンド。start の最初の "" はウィンドウタイトル（空にするため）
                    "start \"\" \"%UNITY_EXE%\" -projectPath \"%PROJECT%\"",
                    "exit /b 0"
                };
                // バッチファイルを書き出す。Encoding.Default を使うのは Windows のバッチで日本語パス等が扱いやすいため
                File.WriteAllLines(batPath, batLines, Encoding.Default);

                // バッチを起動するための ProcessStartInfo を準備
                var psi = new ProcessStartInfo
                {
                    FileName = batPath, // 実行するファイル（作成したバッチ）
                    // バッチに渡す引数 : unityPath, projectPath, oldPid
                    // パスに空白が含まれる可能性があるため、ここでダブルクォートで囲んで渡す
                    Arguments = $"\"{unityPath}\" \"{projectPath}\" {oldPid}",
                    UseShellExecute = true, // シェル経由で実行（バッチ実行時は true が扱いやすい）
                    CreateNoWindow = true   // ウィンドウを出さないようにしたい指定（ただし UseShellExecute=true のときに環境によって無視される場合あり）
                };
                Process.Start(psi);
            }
            // macOS 用の処理
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // 一時シェルスクリプトのパス
                string shPath = Path.Combine(Path.GetTempPath(), "UnityRestartHelper.sh");

                // シェルの中身を行配列で定義（UTF-8 で書き出す）
                string[] shLines = new string[]
                {
                    "#!/bin/sh",
                    "UNITY_EXE=\"$1\"",   // $1: Unity 実行ファイルパス
                    "PROJECT=\"$2\"",     // $2: プロジェクトパス
                    "OLD_PID=$3",         // $3: 古いプロセス ID",
                    "",
                    // kill -0 でプロセスが生きているかチェック（存在する限り sleep して待つ）
                    "while kill -0 $OLD_PID 2>/dev/null; do",
                    "  sleep 1",
                    "done",
                    "",
                    // Temp/UnityLockfile が残っている間は待つ（ロック解放を確認）
                    "while [ -f \"$PROJECT/Temp/UnityLockfile\" ]; do",
                    "  sleep 1",
                    "done",
                    "",
                    // Unity をバックグラウンドで起動してスクリプトは終了
                    "\"$UNITY_EXE\" -projectPath \"$PROJECT\" &"
                };

                // シェルを UTF-8 で書き出す
                File.WriteAllLines(shPath, shLines, Encoding.UTF8);

                // 実行権限を付与（chmod +x）
                var chmod = Process.Start(new ProcessStartInfo("/bin/chmod", $"+x \"{shPath}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (chmod != null) chmod.WaitForExit();

                // sh でスクリプトを実行。引数は unityPath, projectPath, oldPid
                Process.Start(new ProcessStartInfo("/bin/sh", $"\"{shPath}\" \"{unityPath}\" \"{projectPath}\" {oldPid}")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else
            {
                // 対応していないプラットフォームの場合はエラーを出す
                UnityEngine.Debug.LogError("Unsupported platform for restart helper.");
            }
        }
        catch (Exception e)
        {
            // 何か問題があれば Unity コンソールにエラーメッセージを出す
            UnityEngine.Debug.LogError("Failed to start restart helper: " + e);
        }
        finally
        {
            // 最後に現在のエディタを終了する（外部スクリプトが残り処理を行う）
            EditorApplication.Exit(0);
        }
    }
}
