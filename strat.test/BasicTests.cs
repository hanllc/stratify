using Xunit;

using Tyler.Avm.Stratify;

namespace Tyler.Avm.StratifyTest
{
    public class FinderTest
    {
        [Fact]
        public void PassingTest()
        {
            var s0 =
                new StratTerm[] {
                    new StratTerm { variable = "z", condition = StratTermVal.gt, constant = "50"},
                    new StratTerm { variable = "y", condition = StratTermVal.lte, constant = "20"},
                    new StratTerm { variable = "x", condition = StratTermVal.gt, constant = "10"}
                    
                };
            var s1 =
                new StratTerm[] {
                    new StratTerm { variable = "q", condition = StratTermVal.gt, constant = "10"},
                    new StratTerm { variable = "z", condition = StratTermVal.lte, constant = "30"},
                    new StratTerm { variable = "x", condition = StratTermVal.lte, constant = "20"}
                };

            var f = new Finder();

            f.AddStataDef("s0",s0);
            f.AddStataDef("s1",s1);

            f.Preprocess();
        }
    }
    /* 
    public class Class1
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(5, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
    */
}