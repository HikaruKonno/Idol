/* 
 * ファイル：PsylliumMng C#
 * システム：サイリウムのアニメーション再生を一括管理するスクリプト
 * 
 * 制作者：寺門　冴羽　2025/08/06
 */

using UnityEngine;
//using static Unity.VisualScripting.Metadata;

public class PsylliumMng : MonoBehaviour
{
	//------------------------------------------------------------------------------------------------
	// 変数宣言
	//------------------------------------------------------------------------------------------------

	[SerializeField, Range(0, 1)]
	float Delay;

	string[] animStateName = { "Psyllium1", "Psyllium2" };

	//------------------------------------------------------------------------------------------------
	// メイン関数
	//------------------------------------------------------------------------------------------------

	// 最初のフレームの処理
	void Start()
	{
		// 子オブジェクト格納用の配列
		Transform[] _children = GetChildren(this.gameObject.transform);
		
		// 
		for(int i = 0; i < _children.Length; i++)
		{
			// 子オブジェクトのアニメーターを取得
			Animator _animator = _children[i].GetComponent<Animator>();

			// アニメーターの再生開始時間をランダムで設定して再生する
			_animator.Play(animStateName[Random.Range(0, 2)], 0, Random.Range(0f, Delay));
		}
	}

	//------------------------------------------------------------------------------------------------
	// サブ関数
	//------------------------------------------------------------------------------------------------

	// 子オブジェクトを探索して返す関数
	// 引数　：オブジェクト
	// 戻り値：子オブジェクトの配列
	Transform[] GetChildren(Transform _parent)
	{
		// 子オブジェクトを格納する配列作成
		Transform[] children = new Transform[_parent.childCount];

		// 0～個数-1までの子を順番に配列に格納
		for (int i = 0; i < children.Length; i++)
		{
			children[i] = _parent.GetChild(i);
		}

		return children;
	}
}
