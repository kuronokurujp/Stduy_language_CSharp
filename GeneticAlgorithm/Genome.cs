using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    // 計算したい問題の要素となる
    // これを継承して問題のデータを作成
    public interface DNA
    {
        void Copy(DNA org);
        DNA Clone();

        string ToString();
    }

    /// <summary>
    /// 突然変異を行う者
    /// </summary>
    public interface Mutant
    {
        void Action(DNA[] dns);
    }

    /// <summary>
    /// 交叉を行うもの
    /// </summary>
    public interface Crossover
    {
        (Genome, Genome) Action(Genome anAdma, Genome anEve);
    }

    /// <summary>
    /// データ要素(遺伝的アルゴリズムでは遺伝子ともいう)
    /// ゲノムの事
    /// </summary>
    public class Genome : IEquatable<Genome>
    {
        private DNA[] dnas = null;
        public Mutant mutant { get; private set; } = null;
        public Crossover crossover { get; private set; } = null;

        public DNA[] DNA
        {
            get { return this.dnas; }
        }

        public Genome(DNA[] dnaArray, Mutant mutant, Crossover crossover)
        {
            this.mutant = mutant;
            this.crossover = crossover;

            this.dnas = new DNA[0];
            Array.Resize<DNA>(ref this.dnas, dnaArray.Length);

            int i = 0;
            foreach (var dnaOrg in dnaArray)
            {
                this.dnas[i] = dnaOrg.Clone();
                ++i;
            }
        }

        public (Genome ch1, Genome ch2) NCrossPoint(Genome gen)
        {
            return this.crossover.Action(this, gen);
        }

        /// <summary>
        /// 突然変異
        /// </summary>
        public void Mutation()
        {
            // todo: これどうする
            // DNAクラス側で投げるか
            this.mutant.Action(this.dnas);
        }

        public virtual async Task Evaluation() { return; }

        // objectのメソッド
        public override string ToString()
        {
            // 要素の状態を表示
            string str = string.Empty;
            foreach (var dna in this.dnas)
            {
                str += dna.ToString();
            }

            return str;
        }

        // インターフェイスのメソッド
        public bool Equals(Genome other)
        {
            if (other == null)
            {
                return false;
            }

            return EqualityComparer<DNA[]>.Default.Equals(this.dnas, other.dnas);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Genome);
        }

        public override int GetHashCode()
        {
            return 273486370 + EqualityComparer<DNA[]>.Default.GetHashCode(this.dnas);
        }

        public static bool operator ==(Genome genome1, Genome genome2)
        {
            return EqualityComparer<Genome>.Default.Equals(genome1, genome2);
        }

        public static bool operator !=(Genome genome1, Genome genome2)
        {
            return !(genome1 == genome2);
        }
    }
}