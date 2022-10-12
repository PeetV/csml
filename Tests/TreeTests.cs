using Xunit;

namespace Csml.Tests.Tree;

public class BinaryTree
{
    [Fact]
    public void Train_classify_gini()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", "gini");
        tree.Train(features, target);
        Assert.Equal(0.5, tree.nodes[0].splitPoint);
        Assert.Equal(0.05555555555555555, tree.nodes[0].purityGain);
        Assert.Equal(0, tree.nodes[0].index);
        Assert.False(tree.nodes[0].isLeaf);
        Assert.True(tree.nodes[0].yesIndex > 0);
        Assert.True(tree.nodes[0].noIndex > 0);
        Assert.Equal(2, tree.minColumns);
    }

    [Fact]
    public void Train_regress_gini()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regress", "gini");
        tree.Train(features, target);
        Assert.Equal(0.5, tree.nodes[0].splitPoint);
        Assert.Equal(0.05555555555555555, tree.nodes[0].purityGain);
        Assert.Equal(0, tree.nodes[0].index);
        Assert.False(tree.nodes[0].isLeaf);
        Assert.True(tree.nodes[0].yesIndex > 0);
        Assert.True(tree.nodes[0].noIndex > 0);
    }

    [Fact]
    public void Train_constructor_exceptions()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        Assert.Throws<ArgumentException>(() =>
        {
            CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regression", "gini");
        }
        );
        Assert.Throws<ArgumentException>(() =>
        {
            CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regress", "gina");
        }
        );
    }

    [Fact]
    public void Train_input_exceptions()
    {
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", "gini");
        Assert.Throws<ArgumentException>(()=>
        {
            tree.Train(new double[,]{}, new double[]{});
        });
        Assert.Throws<ArgumentException>(()=>
        {
            tree.Train(new double[,]{{1, 1}}, new double[]{1, 2});
        });
    }
}