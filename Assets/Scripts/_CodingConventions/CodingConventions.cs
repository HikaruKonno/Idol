// コーディング規約

// namespace は、パスカルケース
namespace Player
{
    public class CodingConventions
    {
        // enumの定義には、パスカルケース
        public enum Condition
        {
            Fine,
            Poison,
        }

        // インターフェイスには I を付けてパスカルケース。
        interface IUnit
        {
            // このメソッドはpublicになるのでアクセス修飾子は省略する
            void HitDamage();
        }

        // publicは、パスカルケース
        public int HogeHoge;

        // privateのメンバー変数は、m_ でキャメルケース
        private int m_hogeHoge;

        // privateの定数変数は、コンスタントケース（アッパーケースとも呼ばれる）
        private static readonly int HOGE_HOGE;

        // getterとsetterはプロパティで書いてください
        public int m_fuga { get; private set; }

        void Start()
        {
            // ローカル変数には、キャメルケース
            int hogeHoge = 0;

            // 1つの行に1つのステートメントや宣言を記述します
            // 良い例
            SetHogeHoge(hogeHoge);
            GetHogeHoge();
            int x = 0;
            int y = 0;

            // 悪い例
            SetHogeHoge(hogeHoge); GetHogeHoge();
            int xx, yy;
        }

        /// <summary>
        /// 関数の説明を書く<br/>
        /// 引数1：何の引数なのかの説明
        /// </summary>
        /// <param name="_hogeHoge">何の引数なのかの説明</param>
        private void SetHogeHoge(int _hogeHoge)
        {
            HogeHoge = _hogeHoge;
        }

        /// <summary>
        /// 関数の説明を書く
        /// </summary>
        /// <returns>戻り値の説明を書く</returns>
        private int GetHogeHoge()
        {
            return m_hogeHoge;
        }
    }
}
