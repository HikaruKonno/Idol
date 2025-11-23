/*
 * ファイル
 * SceneLoadBridge C#
 * 
 * システム
 * シーンの切り替え処理をタイムラインのシグナルで呼び出す為のクラス
 * シーンマネージャーのシングルトンが起動前の状態だとヒエラルキーに存在しないため、
 * シグナルに設定する為に用意
 * 
 * 変更履歴
 * 2025/09/24　奥山　凜　作成
 */

using UnityEngine;

/// <summary>
/// IdolSceneManagerがあらかじめシーンにいないため、代わりにSignalReceiverに登録するスクリプトと関数
/// </summary>
public class SceneLoadBridge : MonoBehaviour
{
    /// <summary>
    /// シーンマネージャーにシーンのロードを要求<br/>
    /// 引数1：_sceneBuildIndex 読み込むシーンのビルドインデックス
    /// </summary>
    /// <param name="_sceneBuildIndex">読み込むシーンのビルドインデックス</param>
    /// <returns>なし</returns>
    public void RequestSceneLoad(int _sceneBuildIndex)
    {
        // ここでシングルトンのシーンマネージャーを呼び出す
        IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(_sceneBuildIndex);
    }
}