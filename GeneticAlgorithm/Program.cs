using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    /// <summary>
    /// DNAを一つの数と見立て定義
    /// </summary>
    public class NumberDNA : DNA
    {
        public int num = 0;

        public NumberDNA()
        {
            this.num = 0;
        }

        public NumberDNA(int num)
        {
            this.num = num;
        }

        public void Copy(DNA org)
        {
            NumberDNA numberDNA = org as NumberDNA;
            this.num = numberDNA.num;
        }

        public DNA Clone()
        {
            return new NumberDNA(this.num);
        }

        public static NumberDNA[] Make(int size, int seed, int min, int max)
        {
            Random rand = new Random((int)seed);
            return Enumerable.Range(0, size)
                .Select(c => new NumberDNA(rand.Next(min, max + 1)))
                .ToArray();
        }

        public override string ToString()
        {
            return string.Format("{0}", this.num.ToString());
        }
    }

    /// <summary>
    /// ゲノムの突然変異
    /// </summary>
    public class NumberMutant : Mutant
    {
        private int seed = 0;
        private int min, max = 0;

        public NumberMutant(int seed, int min, int max)
        {
            this.seed = seed;
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// DNAに基づいて突然変異する
        /// </summary>
        /// <param name="dns"></param>
        public void Action(DNA[] dns)
        {
            Random rand = new Random(this.seed++);
            HashSet<int> hsTable = new HashSet<int>();
            while (true)
            {
                int point = rand.Next(this.max);
                if (hsTable.Add(point) == false)
                {
                    break;
                }

                var dna = dns[point] as NumberDNA;
                dna.num = rand.Next(min, max + 1);
            }
        }
    }

    /// <summary>
    /// ゲノムの情報交換
    /// </summary>
    public class NumberCrossover : Crossover
    {
        private int seed = 0;
        private int min = 0;
        private int max = 0;


        /// <summary>
        /// ゲノムの情報交換に乱数を使っているので種を設定
        /// </summary>
        /// <param name="in_seed"></param>
        public NumberCrossover(int in_seed, int in_min, int in_max)
        {
            this.seed = in_seed;
            this.min = in_min;
            this.max = in_max;
        }

        /// <summary>
        /// ゲノム同士の情報交換を行う
        /// </summary>
        /// <param name="anAdam"></param>
        /// <param name="anEve"></param>
        /// <returns></returns>
        public (Genome, Genome) Action(Genome anAdam, Genome anEve)
        {
            var (ch1, ch2) = this.NCrossPoint(anAdam.DNA, anEve.DNA);

            this.seed += 2;
            var genomeCh1 = new Genome(ch1, new NumberMutant(this.seed, this.min, this.max), this);

            this.seed += 2;
            var genomeCh2 = new Genome(ch1, new NumberMutant(this.seed, this.min, this.max), this);

            return (genomeCh1, genomeCh2);
        }

        /// <summary>
        /// ゲノムのDNAから新しいDNAを生成
        /// </summary>
        /// <param name="aMyDna"></param>
        /// <param name="aSomeOneDNA"></param>
        /// <returns></returns>
        private (DNA[], DNA[]) NCrossPoint(DNA[] aMyDna, DNA[] aSomeOneDNA)
        {
            Random rand = new Random(this.seed++);
            int[] cutPoint = this.MkCutPoint(rand.Next(1, aMyDna.Length), 0, aMyDna.Length);
            Array.Resize(ref cutPoint, cutPoint.Length + 1);

            // memo: Length+1して拡張した要素に元のDNA最大数を入れているが意味あるのか？
            cutPoint[cutPoint.Length - 1] = aMyDna.Length;

            DNA[] ch1 = new NumberDNA[aMyDna.Length];
            {
                for (int i = 0; i < ch1.Length; ++i)
                {
                    ch1[i] = new NumberDNA(0);
                }
            }

            DNA[] ch2 = new NumberDNA[aMyDna.Length];
            {
                for (int i = 0; i < ch2.Length; ++i)
                {
                    ch2[i] = new NumberDNA(0);
                }
            }

            int IX = 0;
            var st = 0;
            foreach (var pt in cutPoint)
            {
                // pt >= stにならないと入れ替えが発生しない
                // st ～ pt - stの範囲内で値の入れ替えをしている
                if (IX++ % 2 == 0)
                {
                    for (int i = st; i < pt - st; ++i)
                    {
                        ch1[i].Copy(aMyDna[i]);
                        ch2[i].Copy(aSomeOneDNA[i]);
                    }
                }
                else
                {
                    for (int i = st; i < pt - st; ++i)
                    {
                        ch2[i].Copy(aMyDna[i]);
                        ch1[i].Copy(aSomeOneDNA[i]);
                    }
                }

                st = pt;
            }

            return (ch1, ch2);
        }

        /// <summary>
        /// DNA配列の要素のどこからどこまでを切り取るのか範囲を生成 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        private int[] MkCutPoint(int num, int start, int stop)
        {
            Random rand = new Random(this.seed++);
            SortedSet<int> ssTable = new SortedSet<int>();
            while (ssTable.Count() < num)
            {
                ssTable.Add(rand.Next(start + 1, stop));
            }

            int[] ans = new int[num];
            ssTable.CopyTo(ans);

            return ans;
        }
    }

    /// <summary>
    /// OneMax問題を解く為のルールブック
    /// ゲノムの生成も兼任している
    /// RuleBokkはゲノム定義次第で処理が変わるのでゲノムクラスと関連している
    /// なのでゲノム生成を持たせても良いかと思った
    /// </summary>
    public class OneMaxRuleBook : RuleBook
    {
        public int GenomeSize { get; private set; } = 100;
        public int Max { get; private set; } = 1;
        public int Min { get; private set; } = 0;

        private int seed = 0;
        private int dnaMax = 0;

        /// <summary>
        /// コンストラクタ
        /// ゲノムを生成する乱数の種とゲノムが保有するDNA最大数を決める
        /// </summary>
        /// <param name="in_seed"></param>
        /// <param name="in_dnaMax"></param>
        public OneMaxRuleBook(int in_seed, int in_dnaMax)
        {
            this.seed = in_seed;
            this.dnaMax = in_dnaMax;
        }

        /// <summary>
        /// RuleBook側で扱えるゲノムを作成
        /// </summary>
        /// <returns></returns>
        public Genome CreateGenome()
        {
            var genome = new Genome(NumberDNA.Make(this.dnaMax, this.seed, this.Min, this.Max),
                                    new NumberMutant(this.seed, this.Min, this.Max),
                                    new NumberCrossover(this.seed, this.Min, this.Max));

            this.seed += 1;

            return genome;
        }

        /// <summary>
        /// 引数のゲノムを評価するか
        /// 常に評価で良い
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public override bool IsEvaluationGenome(Genome g)
        {
            return true;
        }

        /// <summary>
        /// ゲノムの評価
        /// 数が大きい方が評価が高い
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public override int CompareEvaluation(Genome g1, Genome g2)
        {
            return this.CalcEvaluation(g1) - this.CalcEvaluation(g2);
        }

        /// <summary>
        /// ゲノム情報を文字列で出力
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public string Printf(Genome genome)
        {
            return string.Join("", genome.DNA.Select(x => x.ToString()));
        }

        /// <summary>
        /// ゲノムの評価値
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private int CalcEvaluation(Genome g)
        {
            // 各DNAの合計値を評価値とする
            int sum = 0;
            foreach (var dns in g.DNA)
            {
                var numberDNA = dns as NumberDNA;
                sum += numberDNA.num;
            }

            return sum;
        }
    }

    class Program
    {
        /// <summary>
        /// 遺伝的アルゴリズム(PfGA)の検証
        /// OneMax問題を検証
        /// 0/1の数字の並びが全て1で並びのが理想
        /// しかし3000回の試行では全て1で並ぶことはなかった
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // アルゴリズムで解く問題データ用意
            OneMaxRuleBook ru = new OneMaxRuleBook(0, 25);

            Colony eden = new Colony(aRuleBook: ru);

            // ゲノム作成してコロニー所属させる
            eden.AddHuman(ru.CreateGenome());
            eden.AddHuman(ru.CreateGenome());

            Random rand = new Random();

            // ゲノム同士の比較を非同期で行うのでTask.Runが必要
            // 比較するための評価値を算出するのに数秒かかるとその間はプログラムが止まるのを防ぐため非同期にした
            Task.Run(async () =>
            {
                // 適当な試行回数
                int max = 3000;
                for (int gen = 0; gen < max; ++gen)
                {
                    // コロニーからゲノムをランダム取得
                    var adam = eden.RandamGetHuman();
                    var eve = eden.RandamGetHuman();

                    // ゲノムから新しいゲノムを2つ作成
                    (Genome ch1, Genome ch2) = adam.NCrossPoint(eve);

                    // 新しいゲノムを突然変異させる
                    // 50%で作成どちらかを変異
                    if (rand.Next(2) == 0)
                    {
                        ch1.Mutation();
                    }
                    else
                    {
                        ch2.Mutation();
                    }

                    // 4つのゲノムを評価
                    bool extedHumans = await eden.Exted_Humans((adam, eve), (ch1, ch2));
                    if (extedHumans)
                    {
                        eden.AddHuman(ru.CreateGenome());
                    }

                    // 評価結果をデバッグ出力
                    Console.WriteLine("第{0}世代", gen + 1);
                    foreach (Genome s in eden.Citizens)
                    {
                        Console.WriteLine("{0}", s.ToString());
                    }
                    Console.WriteLine("");
                }

                // 結果表示
                {
                    Console.WriteLine("結果:{0}", ru.Printf(eden.Citizens[0]));
                    Console.Write(System.Environment.NewLine);
                }

            }).Wait();
        }
    }
}
