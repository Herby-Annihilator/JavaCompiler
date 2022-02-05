using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SemanticTree
    {
        private SemanticTree _parent;
        private SemanticTree _leftChild;
        private SemanticTree _rightChild;
        public Node Data { get; set; }

        public SemanticTree(SemanticTree parent, SemanticTree leftChild, SemanticTree rightChild, Node data)
        {
            _parent = parent;
            _leftChild = leftChild;
            _rightChild = rightChild;
            Data = data;
        }

        public void AddDataToLeft(Node data)
        {
            _leftChild = new SemanticTree(this, null, null, data);
        }

        public void AddDataToRight(Node data)
        {
            _rightChild = new SemanticTree(this, null, null, data);
        }

        public SemanticTree FindUp(SemanticTree startVertex, Lexemes lexeme)
        {
            SemanticTree tree = startVertex;
            while ((tree != null) && (lexeme != Data.Type))
            {
                tree = tree._parent;
            }
            return tree;
        }

        public SemanticTree FindUp(Lexemes lexeme) => FindUp(this, lexeme);

        public SemanticTree FindRightLeft(SemanticTree startVertex, Lexemes lexeme)
        {
            SemanticTree right = startVertex._rightChild;
            while ((right != null) && (lexeme != right.Data.Type))
            {
                right = right._leftChild;
            }
            return right;
        }

        public SemanticTree FindRightLeft(Lexemes lexeme) => FindRightLeft(this, lexeme);
    }
}
