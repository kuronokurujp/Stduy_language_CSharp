using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    /// <summary>
    /// 局所集団 Genomeを使ってPfGAを実行する
    /// RuleBookに従がって良い結果のGenome(遺伝子)を選択する
    /// </summary>
    public class Colony
    {
        private RuleBook rules = null;
        public List<Genome> Citizens { get; private set; } = new List<Genome>();
        private int seed = 0;

        public Colony(RuleBook aRuleBook)
        {
            this.rules = aRuleBook;
            this.seed = Environment.TickCount + this.seed;
        }

        public void AddHuman(Genome genome)
        {
            this.Citizens.Add(genome);
        }

        public void Sort()
        {
            this.Citizens.Sort(this.rules);
        }

        public Genome RandamGetHuman()
        {
            Random rand = new Random(this.seed++);
            int ix = rand.Next(this.Citizens.Count);
            Genome ans = this.Citizens[ix];
            this.Citizens.RemoveAt(ix);

            return ans;
        }

        public async Task<bool> Exted_Humans((Genome g1, Genome g2) l, (Genome g1, Genome g2) l2)
        {
            // 各ゲノムの評価処理
            await l.g1.Evaluation();
            await l.g2.Evaluation();
            await l2.g1.Evaluation();
            await l2.g2.Evaluation();

            var myList = new List<Tuple<Genome, string>>()
            {
                Tuple.Create(l.g1, "P"), Tuple.Create(l.g2, "P"),
                Tuple.Create(l2.g1, "C"), Tuple.Create(l2.g2, "C"),
            };

            // ４つのゲノムの評価をする
            // 評価が高い順に降順にならべる
            // 要素0は一番評価が高い、要素1は2番目に評価が高い
            myList.Sort((a, b) =>
            {
                int cmp = this.rules.Compare(a.Item1, b.Item1);
                // リストを降順にする為に高い値程、小さくするようにしている
                return (-1 * cmp);
            });

            // 上位２つから優勢識別子を取得
            string ss = myList[0].Item2 + myList[1].Item2;
            bool request = false;
            switch (ss)
            {
                case "CC":
                    {
                        this.AddHuman(myList[0].Item1);
                        this.AddHuman(myList[1].Item1);
                        this.AddHuman(myList[3].Item1);
                        break;
                    }
                case "PP":
                    {
                        this.AddHuman(myList[0].Item1);
                        if (this.Citizens.Count < 2)
                        {
                            request = true;
                        }

                        break;
                    }
                case "PC":
                    {
                        this.AddHuman(myList[0].Item1);
                        this.AddHuman(myList[1].Item1);

                        break;
                    }
                case "CP":
                    {
                        this.AddHuman(myList[0].Item1);
                        request = true;

                        break;
                    }
            }

            return request;
        }
    }
}