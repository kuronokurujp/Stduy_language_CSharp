using System.Collections.Generic;

namespace GeneticAlgorithm
{
    /// <summary>
    /// 遺伝的アルゴリズムで解決したい問題を定義
    /// 選択するGenomeの規則を定義
    /// このクラスの中身を入れ替える事で解きたい問題内容を変える事が出来る
    /// </summary>
    public abstract class RuleBook : IComparer<Genome>
    {
        /// <summary>
        /// ゲノム評価
        /// 戻り値が高い程評価が高い
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public int Compare(Genome g1, Genome g2)
        {
            int ans = 0;
            // 評価出来るゲノムかチェック
            bool chkG1 = this.IsEvaluationGenome(g1);
            bool chkG2 = this.IsEvaluationGenome(g2);

            if (chkG1 && chkG2)
            {
                // ２つのゲノムで評価する
                ans = this.CompareEvaluation(g1, g2);
            }
            else if ((chkG1 == false) && chkG2)
            {
                ans = -1;
            }
            else if (chkG1 && (chkG2 == false))
            {
                ans = 1;
            }

            return ans;
        }

        /// <summary>
        /// 引数のゲノムが評価出来るゲノムか
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public abstract bool IsEvaluationGenome(Genome g);

        /// <summary>
        /// 派生側で実装するゲノム評価
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public abstract int CompareEvaluation(Genome g1, Genome g2);
    }
}